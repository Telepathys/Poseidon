using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poseidon;

public class MatchMessage
{
    public void Send(User user, StringBuilder message, CancellationTokenSource cts)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
        CurrentMatchDictionary currentMatchDictionary = CurrentMatchDictionary.GetCurrentMatchDictionary();
        MatchDictionary matchDictionary = MatchDictionary.GetMatchDictionary();
        MatchMessageSendType matchMessageSend = JsonConvert.DeserializeObject<MatchMessageSendType>(JObject.Parse(message.ToString()).First.First.ToString());
        string uid = user.uid;
        string usn = user.usn;
        string matchId = currentMatchDictionary.GetMyMatchId(uid);
        if (matchId != null)
        {
            ConcurrentDictionary<User, WebSocket> matchList = matchDictionary.GetMatch(matchId);
            ResponseMatchMessageSendType ResponseMatchMessageSend = new ResponseMatchMessageSendType
            {
                type = Enum.GetName(typeof(MessageSendType), MessageSendType.MatchMessage),
                uid = uid,
                username = usn,
                message = matchMessageSend.message
            };
            string ResponseMatchMessageSendJson = JsonConvert.SerializeObject(ResponseMatchMessageSend);
            byte[] encodedMessage = Encoding.UTF8.GetBytes(ResponseMatchMessageSendJson);
            
            var tasks = new List<Task>();
            foreach (var socket in matchList)
            {
                if (socket.Value.State == WebSocketState.Open && socket.Key.uid != uid)
                {
                    tasks.Add(socket.Value.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, cts.Token));
                }
            }
            
            Task.WhenAll(tasks);
        }
        else
        {
            Program.systemMessage.Send(user, "매치에 참여하고 있지 않습니다.");
            Program.logger.Warn($"{usn}({uid})님은 그룹에 속해 있지 않습니다.");
        }
    }
}