use serde::Deserialize;

#[derive(Debug, Deserialize)]
pub struct AppConfig {
    pub websocket_server_url: String
}