using System.Text;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poseidon;

public class WhisperMessage
{
    private byte[] encodedMessage;
    private ResponseWhisperMessageSendType responseWhisperMessageSend;
    private string responseWhisperMessageSendJson;
    
    public void Send(ConcurrentDictionary<User,WebSocket> webSockets, User user, StringBuilder message, CancellationTokenSource cts)
    {
        WhisperMessageSendType whisperMessageSend = JsonConvert.DeserializeObject<WhisperMessageSendType>(JObject.Parse(message.ToString()).First.First.ToString());
        string uid = user.uid;
        string usn = user.uid;
        ResponseWhisperMessageSendType responseWhisperMessageSend = new ResponseWhisperMessageSendType
        {
            type = Enum.GetName(typeof(MessageSendType), MessageSendType.WhisperMessage),
            uid = uid,
            username = usn,
            message = whisperMessageSend.message
        };
        responseWhisperMessageSendJson = JsonConvert.SerializeObject(responseWhisperMessageSend);
        encodedMessage = Encoding.UTF8.GetBytes(responseWhisperMessageSendJson);
        
        User targetUser = webSockets.Keys.FirstOrDefault(thisUser => thisUser.uid == whisperMessageSend.uid);
        if (targetUser != null && targetUser.uid != uid)
        {
            if (webSockets.TryGetValue(targetUser, out WebSocket whisperSocket))
            {
                whisperSocket.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, cts.Token);
            }
        }
        else
        {
            Program.systemMessage.Send(webSockets, user, "A user who does not exist or is not login.");
            Program.logger.Warn("Target user not found");
        }
    }
}