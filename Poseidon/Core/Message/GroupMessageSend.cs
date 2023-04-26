using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poseidon;

public class GroupMessage
{
    public void Send(ConcurrentDictionary<User,WebSocket> webSockets, User user, StringBuilder message, CancellationTokenSource cts)
    {
        GroupMessageSendType groupMessageSend = JsonConvert.DeserializeObject<GroupMessageSendType>(JObject.Parse(message.ToString()).First.First.ToString());
        string uid = user.uid;
        string usn = user.uid;
        Program.currenGroup.TryGetValue(uid, out string myGroupKey);
        if (myGroupKey != null)
        {
            Program.group.TryGetValue(myGroupKey, out ConcurrentDictionary<string, WebSocket> groupSocket);
            ResponseGroupMessageSendType responseGroupMessageSend = new ResponseGroupMessageSendType
            {
                type = Enum.GetName(typeof(MessageSendType), MessageSendType.GroupMessage),
                uid = uid,
                username = usn,
                message = groupMessageSend.message
            };
            string responseServerMessageSendJson = JsonConvert.SerializeObject(responseGroupMessageSend);
            byte[] encodedMessage = Encoding.UTF8.GetBytes(responseServerMessageSendJson);
            
            var tasks = new List<Task>();
            foreach (var socket in groupSocket)
            {
                if (socket.Value.State == WebSocketState.Open && socket.Key != uid)
                {
                    tasks.Add(socket.Value.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, cts.Token));
                }
            }
            
            Task.WhenAll(tasks);
        }
        else
        {
            Program.systemMessage.Send(user, webSockets, "You are not in a group");
            Program.logger.Warn($"{user.usn}님은 그룹에 속해 있지 않습니다.");
        }
    }
}