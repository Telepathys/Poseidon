using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace Poseidon;

public class RandomMatchMessageSend
{
    public void Send(User user, MessageSendType messageSendType, string message, string matchId = null)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
        string uid = user.uid;
        string usn = user.usn;
        ResponseRandomMatchMessageSendType responseRandomMatchMessageSend = new ResponseRandomMatchMessageSendType
        {
            type = Enum.GetName(typeof(MessageSendType), messageSendType),
            matchId = matchId,
            uid = uid,
            username = "System",
            message = message
        };
        
        string responseRandomMatchMessageSendJson = JsonConvert.SerializeObject(responseRandomMatchMessageSend);
        byte[] encodedMessage = Encoding.UTF8.GetBytes(responseRandomMatchMessageSendJson);

        webSockets.TryGetValue(user, out WebSocket mySocket);
        mySocket.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}