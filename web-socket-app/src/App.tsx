import { FormEvent, KeyboardEvent, useEffect, useRef, useState } from "react";
import {
	IDisplayedMessage,
	MessageType,
	MessageStatus,
	clusterMessages,
	MessageTimeStamp,
	IRustPayload,
	IMessageOutbound,
	IMessageInbound,
} from "./domain/messages";
import { invoke } from "@tauri-apps/api/tauri";
import { listen, Event } from "@tauri-apps/api/event";
import { Guid } from "./utils/guid-helper";
import { WebsocketService } from "./services/websocketService";
import { MessageCluster } from "./components/messageCluster";

function App() {
	const displayedMessageLimit = 100;
	const defaultChatHeight = 52;
	const maxChatHeight = 316;
	const [message, setMessage] = useState("");
	const [messages, setMessages] = useState<IDisplayedMessage[]>([]);
	const webSocketService = new WebsocketService();

	interface IAppState {
		userName: string | null;
		userId: string | null;
		room: string | null;
	}

	const [appState, setAppState] = useState<IAppState>({
		userName: null,
		userId: null,
		room: null,
	});

	function updateUserId(userId: string) {
		setAppState((prevAppState) => ({ ...prevAppState, userId }));
	}

	function updateUserName(userName: string) {
		setAppState((prevAppState) => ({ ...prevAppState, userName: userName }));
	}

	const [setupForm, setSetupForm] = useState({
		userName: "",
	});

	useEffect(() => {
		const listenToMessageAsync = async (event: Event<IRustPayload>) => {
			const messageInbound = JSON.parse(
				event.payload.message
			) as IMessageInbound;

			if (messageInbound.type === MessageType.UserConnected) {
				updateUserId(messageInbound.serverMessage);
				return;
			}

			let isUsersOwnMessage = messageInbound.userMessage?.userId === appState.userId;

			if (isUsersOwnMessage){
				updateMessage(messageInbound);
			} else {
				let serverMessage: IDisplayedMessage = {
					id: messageInbound.backendId,
					userName: messageInbound.userMessage?.userName ?? null,
					type: messageInbound.type,
					message: messageInbound.userMessage?.message ?? messageInbound.serverMessage,
					timeStamp: new MessageTimeStamp(messageInbound.timeStamp),
					status: MessageStatus.Received,
				};
		
				addDisplayMessage(serverMessage);
			}
		};

		const eventName = "WebSocketMsg";

		const unlistenPromise = listen(eventName, listenToMessageAsync);

		return () => {
			unlistenPromise
				.then((unlistenFn) => {
					unlistenFn();
				})
				.catch((err) =>
					console.error(`Failed to unlisten to event ${eventName}. ${err}`)
				);
		};
	});

	function setSetupFormUsername(userName: string) {
		setSetupForm((prevForm) => ({ ...prevForm, userName: userName }));
	}

	async function connectWithUsername(e?: FormEvent<HTMLFormElement>) {
		e?.preventDefault();

		try {
			await invoke("set_username_and_connect", {
				username: setupForm.userName,
			});
			updateUserName(setupForm.userName);
		} catch (error: unknown) {
			console.error(error);
		}
	}

	function addDisplayMessage(displayMsg: IDisplayedMessage) {
		if (messages.length >= displayedMessageLimit) {
			setMessages((prevMessages) => prevMessages.slice(1));
		}

		setMessages((prevMessages) => [...prevMessages, displayMsg]);
		setMessage("");
	}

	async function addAndSendMessage(e?: FormEvent<HTMLFormElement>) {
		e?.preventDefault();
		let displayMsg: IDisplayedMessage = {
			id: Guid.newGuid(),
			userName: appState.userName,
			type: MessageType.Message,
			message: message,
			timeStamp: new MessageTimeStamp(new Date()),
			status: MessageStatus.Sent,
		};

		if (!appState.userId)
			throw new Error("User id is not successfully initialized");

		const messageDto: IMessageOutbound = {
			frontendId: displayMsg.id,
			payload: {
				roomId: appState.room,
				userId: appState.userId,
				message: message,
			},
		};

		try {
			webSocketService.sendMessage(messageDto);
		} catch (err: unknown) {
			console.error(err);
		}

		addDisplayMessage(displayMsg);
	}

	function updateMessage(messageInbound: IMessageInbound) {
    setMessages((currentMessages) => {
        const index = currentMessages.findIndex(msg => msg.id === messageInbound.frontendId);
        if (index === -1) return currentMessages;

        return currentMessages.map((item, idx) => {
            if (idx === index) {
                return { 
                    ...item, 
                    status: MessageStatus.Delivered, 
                    timeStamp: new MessageTimeStamp(messageInbound.timeStamp)
                };
            }
            return item;
        });
    });
}

	function handleKeyDown(e: KeyboardEvent<HTMLTextAreaElement>) {
		if (e.key === "Enter" && !e.shiftKey) {
			e.preventDefault();
			addAndSendMessage();
		}
	}

	const textareaRef = useRef<HTMLTextAreaElement>(null);

	useEffect(() => {
		const textarea = textareaRef.current;
		if (textarea) {
			textarea.style.height = `${defaultChatHeight}px`;
			textarea.style.height = `${Math.min(
				textarea.scrollHeight,
				maxChatHeight
			)}px`;
		}
	}, [message]);

	if (appState.userName === null) {
		return (
			<main className="flex justify-center items-center w-full">
				<form
					className="flex flex-col items-center justify-center gap-4 bg-gray-200 p-8 rounded-xl"
					onSubmit={connectWithUsername}
				>
					<input
						className="w-60 px-4 py-2 rounded-xl text-center"
						type="text"
						placeholder="Insert username"
						onChange={(e) => setSetupFormUsername(e.target.value)}
						maxLength={25}
					/>
					<button className="bg-gray-700 w-60 rounded-lg p-2 text-white font-bold hover:bg-gray-600 active:bg-gray-600">
						Accept
					</button>
				</form>
			</main>
		);
	}

	console.log(messages);
	return (
		<main className="flex items-center">
			<p className="absolute top-2 right-3 text-xs text-blue-400 font-bold">
				Username: {appState.userName}
			</p>
			{/* <div className="flex flex-col w-1/4 h-full bg-gray-600">
        <div className="flex flex-col p-8 text-gray-300">
          <h3 className="text-md font-bold">ROOMS</h3>
        </div>
      </div> */}
			<div className="flex flex-col w-full h-full items-center">
				<div className="flex flex-col h-full w-full">
					<div className="flex flex-col rounded-lg p-6 h-full w-full overflow-y-scroll">
						{clusterMessages(messages).map((msgCluster) => (
							<MessageCluster msgCluster={msgCluster} />
						))}
					</div>
					<div className="flex justify-center p-8">
						<form
							className="flex justify-center items-center w-full rounded-2xl bg-white"
							onSubmit={addAndSendMessage}
						>
							<div className="flex rounded-full w-full items-center relative">
								<textarea
									ref={textareaRef}
									value={message}
									style={{ height: `${defaultChatHeight}px` }}
									className="resize-none w-5/6 outline-none p-3.5 overflow-y-scroll bg-transparent"
									rows={5}
									onChange={(e) => setMessage(e.currentTarget.value)}
									onKeyDown={(e) => handleKeyDown(e)}
									placeholder="Type a message..."
								/>
								<button
									className="text-gray-600 font-bold hover:text-blue-400 absolute right-4 bottom-1.5 py-2"
									type="submit"
								>
									Send
								</button>
							</div>
						</form>
					</div>
				</div>
			</div>
		</main>
	);
}

export default App;
