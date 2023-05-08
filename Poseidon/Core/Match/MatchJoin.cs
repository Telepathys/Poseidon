using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poseidon;

public class MatchJoin
{
    private byte[] encodedMessage;
    private ResponseMatchJoinType responseMatchJoin;
    private string responseResponseMatchJoinJson;
    public void Join(User user, StringBuilder message, CancellationTokenSource cts)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
        MatchDictionary matchDictionary = MatchDictionary.GetMatchDictionary();
        ActiveMatchDictionary activeMatchDictionary = ActiveMatchDictionary.GetActiveMatchDictionary();
        CurrentMatchDictionary currentMatchDictionary = CurrentMatchDictionary.GetCurrentMatchDictionary();
        MatchJoinType matchJoin = JsonConvert.DeserializeObject<MatchJoinType>(JObject.Parse(message.ToString()).First.First.ToString());
        string uid = user.uid;
        string usn = user.usn;
        string matchId = matchJoin.matchId;
        webSockets.TryGetValue(user, out WebSocket mySocket);
        
        if (matchId == null || matchId.Length != 36)
        {
            Program.logger.Error("매치 아이디가 없거나 정상적이지 않은 매치 아이디입니다. <MatchJoin-1>", user);
            return;
        }
        
        // 존재하는 매치인지 확인 필요
        if (!activeMatchDictionary.CheckActiveMatch(matchId))
        {
            Program.logger.Error("존재하지 않는 매치입니다. <MatchJoin-2>", user);
            return;
        }

        string[] thisMatchUserList = activeMatchDictionary.GetActiveMatchUserList(matchId);
        if (!Array.Exists(thisMatchUserList, element => element == uid))
        {
            Program.logger.Error("해당 매치에 참여가능한 상태가 아닙니다. <MatchJoin-3>", user);
            return;
        }

        currentMatchDictionary.SetMyMatch(uid, matchId);
        ConcurrentDictionary<User, WebSocket> matchList = matchDictionary.GetMatch(matchId);
        if (matchList != null)
        {
            // 내가 해당 그룹에 존재 하지 않으면 추가
            if (!matchList.ContainsKey(user))
            {
                matchList.TryAdd(user, mySocket);
            }
        }
        else
        {
            ConcurrentDictionary<User, WebSocket> emptySocket = new ConcurrentDictionary<User, WebSocket>();
            emptySocket.TryAdd(user, mySocket);
            matchDictionary.SetMatch(matchId, emptySocket);
            matchList = emptySocket;
        }
        
        Program.logger.Info($"{usn}님이 {matchId} 매치에 입장하였습니다.");
        var tasks = new List<Task>();
        responseMatchJoin = new ResponseMatchJoinType
        {
            type = Enum.GetName(typeof(MessageSendType), MessageSendType.MatchJoin),
            matchId = matchId,
            uid = uid,
            username = "System",
            message = $"{usn}({uid})님이 매치에 입장하였습니다."
        };
        
        responseResponseMatchJoinJson = JsonConvert.SerializeObject(responseMatchJoin);
        encodedMessage = Encoding.UTF8.GetBytes(responseResponseMatchJoinJson);
        
        foreach (var socket in matchList)
        {
            if (socket.Value.State == WebSocketState.Open)
            {
                tasks.Add(socket.Value.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, cts.Token));
            }
        }
            
        Task.WhenAll(tasks);
    }
}