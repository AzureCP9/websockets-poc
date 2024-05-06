import { IDisplayedMessageCluster } from "../domain/messages";
import { Guid } from "../utils/guid-helper";
import { Message } from "./message";

interface IMessageClusterProps {
  msgCluster: IDisplayedMessageCluster;
}

const MessageCluster = ({msgCluster} : IMessageClusterProps) => {
 
	return (
		<div key={Guid.newGuid()}>
			{msgCluster.userName && (
				<div className="flex gap-2 items-center pb-2">
					<p className="font-bold">{msgCluster.userName}</p>
					<p className="text-xs text-gray-400">
						{msgCluster.messages[0].timeStamp.getFormattedDate()}
					</p>
				</div>
			)}

			{msgCluster.messages.map((msg) => (
				<Message key={msg.id} displayMessage={msg} />
			))}
		</div>
	);
};

export { MessageCluster };
