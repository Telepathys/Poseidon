using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poseidon;

public class MatchCustomData
{
    public void Send(User user, StringBuilder message, CancellationTokenSource cts)
    {
        CurrentMatchDictionary currentMatchDictionary = CurrentMatchDictionary.GetCurrentMatchDictionary();
        MatchDictionary matchDictionary = MatchDictionary.GetMatchDictionary();
        MatchCustomDataType matchCustomData = JsonConvert.DeserializeObject<MatchCustomDataType>(JObject.Parse(message.ToString()).First.First.ToString());
        string uid = user.uid;
        string usn = user.usn;
        string matchId = currentMatchDictionary.GetMyMatchId(uid);
        int dataType = matchCustomData.dataType;
        matchCustomData.data.GetType();
        if(matchCustomData.data.GetType() != typeof(byte[]))
        {
            Program.logger.Error("데이터 타입이 byte[]가 아닙니다. <MatchCustomData-1>", user);
            return;
        }
        byte[] data = matchCustomData.data;
        
        if (matchId != null)
        {
            ConcurrentDictionary<User, WebSocket> matchList = matchDictionary.GetMatch(matchId);
            ResponseMatchCustomDataType ResponseMatchCustomData = new ResponseMatchCustomDataType
            {
                type = Enum.GetName(typeof(MessageSendType), MessageSendType.MatchCustomData),
                matchId = matchId,
                uid = uid,
                username = usn,
                dataType = dataType,
                data = data,
            };
            string ResponseMatchCustomDataJson = JsonConvert.SerializeObject(ResponseMatchCustomData);
            byte[] encodedMessage = Encoding.UTF8.GetBytes(ResponseMatchCustomDataJson);
            
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
            Program.logger.Warn($"{usn}({uid})님은 매치에 참여하고 있지 않습니다.");
        }
    }
}