import { invoke } from "@tauri-apps/api";
import { IMessageOutbound } from "../domain/messages";

class WebsocketService {
	sendMessage = async (outboundMessage: IMessageOutbound) => {
		const messageDtoString = JSON.stringify(outboundMessage);
		await invoke("send_websocket_message", { message: messageDtoString });
	};
}

export { WebsocketService };