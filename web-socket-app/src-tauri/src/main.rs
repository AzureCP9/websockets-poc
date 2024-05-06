// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod app_config;
mod client;

use std::{
    env, error::Error, fmt, net::TcpStream, sync::Mutex
};

use app_config::AppConfig;
use client::{ClientFactory, WebSocketClientError};
use config::{Config, File, FileFormat};
use serde::Serialize;
use tauri::Manager;
use websocket::{sync::{Reader, Writer}, OwnedMessage};
// Learn more about Tauri commands at https://tauri.app/v1/guides/features/command

struct AppState {
    client_sender: Mutex<Option<Writer<TcpStream>>>,
    client_receiver: Mutex<Option<Reader<TcpStream>>>
}

#[derive(Serialize, Clone)]
struct Payload {
    message: String,
}

enum Event {
    WebSocketMsg,
}

impl fmt::Display for Event {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        match self {
            Event::WebSocketMsg => write!(f, "WebSocketMsg"),
        }
    }
}

fn get_config() -> Result<AppConfig, config::ConfigError> {
    let config = Config::builder()
        .set_default("default", "1")?
        .add_source(File::new("config/settings", FileFormat::Json))
        .build()?;

    config.try_deserialize()
}

#[tauri::command]
fn set_username_and_connect(state: tauri::State<AppState>, username: String) -> Result<(), String> {
    let config = get_config().expect("Failed to load config!");

    let mut client = ClientFactory::new(config);

    let client_split = client.create_split_client(username)
        .map_err(|e| e.to_string())?;

    let mut sender_lock = state.client_sender.lock().unwrap();
    *sender_lock = Some(client_split.1);

    let mut receiver_lock = state.client_receiver.lock().unwrap();
    *receiver_lock = Some(client_split.0);

    Ok(())
}

#[tauri::command]
fn send_websocket_message(state: tauri::State<AppState>, message: String) -> Result<(), String> {

    let mut sender_lock = state.client_sender.lock().unwrap();

    let sender = match &mut *sender_lock {
        Some(client_sender) => client_sender,
        None => return Err(WebSocketClientError::ClientNotCreatedError.to_string())
    };

    let websocket_message = OwnedMessage::Text(message.clone());

    match sender.send_message(&websocket_message) {
        Ok(_) => {
            println!("Sent websocket message: {:?}", message);
            Ok(())
        },
        Err(e) => {
            eprintln!("Failed to send websocket message: {}", e.to_string());
            eprintln!("Source: {:?}", e.source());
            Err(e.to_string())
        }
    }
}

fn server_listener(app_handle: tauri::AppHandle) -> () {
    std::thread::spawn(move || loop {
        let state = app_handle.state::<AppState>();
        let mut client_receiver_lock = state.client_receiver.lock().unwrap();
        let message_result = {
            match &mut *client_receiver_lock {
                Some(client_receiver) => client_receiver.recv_message(),
                None => {
                    std::thread::sleep(std::time::Duration::from_millis(100));
                    continue;
                }
            }
        };

        match message_result {
            Ok(OwnedMessage::Text(text)) => {
                app_handle
                    .emit_all(&Event::WebSocketMsg.to_string(), Payload { message: text })
                    .unwrap()
            },
            Ok(_) => (),
            Err(e) => eprintln!("Error reading message from server: {}", e),
        }
    });

    ()
}
fn main() {
    let app_state = AppState {
        client_sender: None.into(),
        client_receiver: None.into()
    };

    tauri::Builder::default()
        .manage(app_state)
        .setup(|app| {
            server_listener(app.app_handle());
            Ok(())
        })
        .invoke_handler(tauri::generate_handler![set_username_and_connect, send_websocket_message])
        .run(tauri::generate_context!())
        .expect("Error while running tauri application!");
}
