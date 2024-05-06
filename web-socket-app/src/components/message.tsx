import { IDisplayedMessage, TMessageStatus, MessageStatus, MessageType } from "../domain/messages";
import { FaCircleExclamation, FaCircleInfo } from "react-icons/fa6";

type MessageProps = {
	displayMessage: IDisplayedMessage;
};


const Message = ({ displayMessage }: MessageProps) => {
  
  console.log(displayMessage)

  const statusColorDictionary: Record<TMessageStatus, string> = {
    [MessageStatus.Sent]: "text-gray-400",
    [MessageStatus.Delivered]: "text-gray-600",
    [MessageStatus.Failed]: "text-gray-400",
    [MessageStatus.Received]: "text-gray-600"
  }

  const textColor = statusColorDictionary[displayMessage.status];

	return(
    <div>
        { displayMessage.type === MessageType.Message &&
          <p className={`${textColor} overflow-x-scroll p-0 m-0`}>{displayMessage.message}</p>
        }
        
        {displayMessage.status === MessageStatus.Failed &&
        <div className="flex gap-1 mt-2 mb-2 items-center">
          <FaCircleExclamation className="text-red-400 h-3.5 w-3.5"/>
          <p className="text-red-600 text-xs">Failed to send message</p>
        </div>
        }

        {displayMessage.type === MessageType.Error &&
        <div className="flex gap-1 mt-2 mb-2 items-center">
          <FaCircleExclamation className="text-red-400 h-3.5 w-3.5"/>
          <p className="text-red-600 text-xs">{displayMessage.message}</p>
        </div>
        }

        {displayMessage.type === MessageType.Notification &&
        <div className="flex gap-1 mt-2 mb-2 items-center">
          <FaCircleInfo className="text-blue-400 h-3.5 w-3.5"/>
          <p className="text-blue-400 text-xs">{displayMessage.message}</p>
        </div>
        }
    </div>
  )
};

export { Message };
