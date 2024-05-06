import { Guid } from "../utils/guid-helper";
import { createTupleFromObjectValues } from "../utils/tuple-helper";
import { format, isToday } from "date-fns";

const MessageType = {
	Message: "Message",
	Notification: "Notification",
	Error: "Error",
	UserConnected: "UserConnected",
} as const;

const MessageStatus = {
	Sent: "Sent",
	Delivered: "Delivered",
	Failed: "Failed",
	Received: "Received"
}

interface IRustPayload {
	message: string
}

const MESSAGE_STATUS = createTupleFromObjectValues(MessageStatus);

const MESSAGE_TYPE = createTupleFromObjectValues(MessageType);

type TMessageStatus = (typeof MessageStatus)[keyof typeof MessageStatus];

type TMessageType = (typeof MessageType)[keyof typeof MessageType];

interface IDisplayedMessage {
	id: string;
	userName: string | null;
	type: TMessageType;
	message: string;
	timeStamp: IMessageTimeStamp;
	status: TMessageStatus;
}

interface IMessageOutbound {
	frontendId: string; // Guid
	payload: {
		roomId: string | null;
		userId: string;
		message: string;
	}
}

interface IMessageInbound {
	type: TMessageType;
	frontendId: string | null;
	backendId: string;
	serverMessage: string;
	timeStamp: Date;
	userMessage: IMessageInboundUserMessage | null;
}

interface IMessageInboundUserMessage {
	roomId: string;
	userId: string;
	userName: string;
	message: string;
}

interface IDisplayedMessageCluster {
	id: string;
	userName: string | null;
	messages: IDisplayedMessage[];
}

interface IMessageTimeStamp {
	timeStamp: Date;
	getFormattedDate: () => string;
}

class MessageTimeStamp implements IMessageTimeStamp {
	timeStamp: Date;

	constructor(timeStamp: Date) {
		this.timeStamp = timeStamp;
	}

	getFormattedDate(): string {
    if(isToday(this.timeStamp)){
      return `Today at ${format(this.timeStamp, 'HH:mm')}`;
    }
    return format(this.timeStamp, "dd/MM/yyyy HH:mm");
  }
}

function clusterMessages(
	messages: IDisplayedMessage[]
): IDisplayedMessageCluster[] {
  
	return messages.reduce<IDisplayedMessageCluster[]>((clusters, message) => {  
		const lastCluster = clusters[clusters.length - 1];
		if (lastCluster && lastCluster.userName === message.userName) {
			lastCluster.messages.push(message);
		} else {
			clusters.push({
				id: Guid.newGuid(),
				userName: message.userName,
				messages: [message],
			});
		}
		return clusters;
	}, []);
}

export type { TMessageType, TMessageStatus, IDisplayedMessage, IDisplayedMessageCluster, IRustPayload, IMessageOutbound, IMessageInbound, IMessageInboundUserMessage };
export { MessageType, MESSAGE_TYPE, MessageStatus, MESSAGE_STATUS, clusterMessages, MessageTimeStamp };
