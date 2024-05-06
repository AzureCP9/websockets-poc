use core::fmt;
use std::{error::Error, net::TcpStream};

use websocket::{
    sync::{client, Reader, Writer},
    url, WebSocketError,
};

use crate::app_config::AppConfig;

pub struct ClientFactory {
    app_config: AppConfig
}

impl ClientFactory {
    pub fn new(app_config: AppConfig) -> Self {
        Self {
            app_config
        }
    }

    pub fn create_split_client(&mut self, username: String) -> Result<(Reader<TcpStream>, Writer<TcpStream>), WebSocketClientError> {
        let ws_url = format!("{}?username={}", self.app_config.websocket_server_url, username);
        println!("Attempting to connect to {}", ws_url);
        let mut client_builder = client::ClientBuilder::new(&ws_url)
            .map_err(|err| WebSocketClientError::ParseError(err))?;

        let client = client_builder
            .connect_insecure()
            .map_err(|err| WebSocketClientError::WebSocketError(err))?
            .split()
            .map_err(|e| WebSocketClientError::IOError(e))?;

        Ok(client)
    }
}

#[derive(Debug)]
pub enum WebSocketClientError {
    ParseError(url::ParseError),
    WebSocketError(WebSocketError),
    ClientNotCreatedError,
    IOError(std::io::Error)
}

impl fmt::Display for WebSocketClientError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            WebSocketClientError::ParseError(e) => write!(f, "URL parse error: {}", e),
            WebSocketClientError::WebSocketError(e) => write!(f, "WebSocket error: {}", e),
            WebSocketClientError::ClientNotCreatedError => write!(f, "The client is not yet created"),
            WebSocketClientError::IOError(e) => write!(f, "IO error: {}", e)
        }
    }
}

impl Error for WebSocketClientError {}
