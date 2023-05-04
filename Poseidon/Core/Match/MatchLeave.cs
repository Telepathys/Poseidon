using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poseidon;

public class MatchLeave
{
    private byte[] encodedMessage;
    private ResponseMatchLeaveType responseMatchLeave;
    private string responseMatchLeaveJson;
    
    public void Leave(User user, StringBuilder message, CancellationTokenSource cts)
    {
        CurrentMatchDictionary currentMatchDictionary = CurrentMatchDictionary.GetCurrentMatchDictionary();
        MatchDictionary matchDictionary = MatchDictionary.GetMatchDictionary();
        ActiveMatchDictionary activeMatchDictionary = ActiveMatchDictionary.GetActiveMatchDictionary();
        string uid = user.uid;
        string usn = user.usn;
        string matchId = currentMatchDictionary.GetMyMatchId(uid);

        if (matchId == null)
        {
            Program.systemMessage.Send(user, "매치에 참여하고 있지 않습니다.");
            Program.logger.Warn("매치에 참여하고 있지 않습니다. <MatchLeave-1>");
        }
        currentMatchDictionary.RemoveMyMatch(uid);
        ConcurrentDictionary<User,WebSocket> match = matchDictionary.GetMatch(matchId);
        if (match == null)
        {
            Program.logger.Error($"매치({matchId})가 존재하지 않습니다. <MatchLeave-2>", user);
            return;
        }

        match.TryGetValue(user, out WebSocket mySocket);
        if (mySocket == null)
        {
            Program.logger.Error("해당 매치에 나의 소켓이 존재하지 않습니다. <MatchLeave-3>", user);
            return;
        }
        match.TryRemove(user, out _);
        Program.logger.Info($"{usn}님이 {matchId} 매치에 퇴장하였습니다.");
        
        List<Task> tasks = new List<Task>();
        responseMatchLeave = new ResponseMatchLeaveType
        {
            type = Enum.GetName(typeof(MessageSendType), MessageSendType.MatchLeave),
            matchId = matchId,
            uid = user.uid,
            username = "System",
            message = $"{user.usn} leave Match"
        };
        responseMatchLeaveJson = JsonConvert.SerializeObject(responseMatchLeave);
        encodedMessage = Encoding.UTF8.GetBytes(responseMatchLeaveJson);
        
        tasks.Add(mySocket.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, cts.Token));
        switch (match.Count)
        {
            case 0: 
                matchDictionary.RemoveMatch(matchId);
                activeMatchDictionary.RemoveActiveMatch(matchId);
                break;
            case int n when (n > 0):
                // 떠난 그룹에 인원이 있다면 떠난 사람을 알림
                foreach (var socket in match)
                {
                    if (socket.Value.State == WebSocketState.Open && socket.Key.uid != uid)
                    {
                        tasks.Add(socket.Value.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, cts.Token));
                    }
                }
                break;
        }
        Task.WhenAll(tasks);
    }
}