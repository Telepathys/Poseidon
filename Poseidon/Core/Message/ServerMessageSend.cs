using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poseidon;

public class ServerMessage
{
    public void Send(User user, StringBuilder message, CancellationTokenSource cts)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
        ServerMessageSendType serverMessageSend = JsonConvert.DeserializeObject<ServerMessageSendType>(JObject.Parse(message.ToString()).First.First.ToString());
        string uid = user.uid;
        string usn = user.uid;
        ResponseServerMessageSendType responseServerMessageSend = new ResponseServerMessageSendType
        {
            type = Enum.GetName(typeof(MessageSendType), MessageSendType.ServerMessage),
            uid = uid,
            username = usn,
            message = serverMessageSend.message
        };
        string responseServerMessageSendJson = JsonConvert.SerializeObject(responseServerMessageSend);
        byte[] encodedMessage = Encoding.UTF8.GetBytes(responseServerMessageSendJson);
        
        var tasks = new List<Task>();
        foreach (var socket in webSockets)
        {
            if (socket.Value.State == WebSocketState.Open && socket.Key.uid != uid)
            {
                tasks.Add(socket.Value.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, cts.Token));
            }
        }
        
        Task.WhenAll(tasks);
    }
}