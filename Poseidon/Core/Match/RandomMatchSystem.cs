using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace Poseidon;

public class RandomMatchSystem
{
    RandomMatchDictionary randomMatchDictionary = RandomMatchDictionary.GetRandomMatchDictionary();
    ActiveMatchDictionary activeMatchDictionary = ActiveMatchDictionary.GetActiveMatchDictionary();
    RandomMatchMessageSend randomMatchMessageSend = new RandomMatchMessageSend();
    Mutex mutex = new Mutex(false,"RandomMatchSystemDetect");
    Random random = new Random();
    // 한개의 매칭당 필요 인원
    readonly int matchRequireUserCount = 2;
    readonly double matchMakeCountControl = 0.1;
    private readonly int matchJoinLimitTime = 30;
    public void Detect(object? state)
    {
        mutex.WaitOne();
        ConcurrentDictionary<User,WebSocket> randomMatchWaitList = randomMatchDictionary.GetRandomList();
        // 매칭 대기중인 유저들중 접속 끊긴 유저들 제외
        foreach (var ramdomMatchWaitUser in randomMatchWaitList)
        {
            User user = ramdomMatchWaitUser.Key;
            WebSocket mySocket = ramdomMatchWaitUser.Value;
            WebSocketState websocketState = mySocket.State;
            if (websocketState != WebSocketState.Open)
            {
                randomMatchDictionary.RemoveMeFromRandomList(user);
            }
        }

        // 매치 매이킹 개수 설정 ( 큐에 따른 조절 )
        var matchMakeCount = randomMatchWaitList.Count / Convert.ToDouble(matchRequireUserCount) * matchMakeCountControl;
        for (int i = 0; i < matchMakeCount; i++)
        {
            if (randomMatchWaitList.Count >= matchRequireUserCount)
            {
                string matchId = Guid.NewGuid().ToString();
                var randomPairs = randomMatchWaitList.OrderBy(x => random.Next())
                    .Take(matchRequireUserCount)
                    .ToDictionary(x => x.Key, x => x.Value);
                
                ConcurrentDictionary<User,WebSocket> completeRandomMatch = new ConcurrentDictionary<User,WebSocket>(randomPairs);
                string[] userList = new string[] { };
                foreach (var ramdomMatchWaitUser in completeRandomMatch)
                {
                    User user = ramdomMatchWaitUser.Key;
                    userList = userList.Append(user.uid).ToArray();
                    randomMatchMessageSend.Send(user, MessageSendType.RandomMatchComplete,"랜덤 매치가 성공적으로 매칭되었습니다.", matchId);
                    randomMatchWaitList.TryRemove(user, out _);
                }
                activeMatchDictionary.SetActiveMatch(matchId, userList);
                waitMatchJoin(matchId, completeRandomMatch);
            }
        }

        mutex.ReleaseMutex();
    }

    public async Task waitMatchJoin(string matchId, ConcurrentDictionary<User,WebSocket> completeRandomMatch)
    {
        MatchDictionary matchDictionary = MatchDictionary.GetMatchDictionary();
        CurrentMatchDictionary currentMatchDictionary = CurrentMatchDictionary.GetCurrentMatchDictionary();
        ActiveMatchDictionary activeMatchDictionary = ActiveMatchDictionary.GetActiveMatchDictionary();
        
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        using var timer = new Timer(async state =>
        {
            ConcurrentDictionary<User, WebSocket> match = matchDictionary.GetMatch(matchId);
            if (match?.Count == matchRequireUserCount)
            {
                cancellationTokenSource.Cancel();
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        
        // 10초 동안 대기하는 비동기 작업
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(matchJoinLimitTime), cancellationToken);
        }
        catch (TaskCanceledException)
        {
            Program.logger.Info($"{matchId} 매치 인원이 전부 입장 완료");
        }
        timer.Dispose();
        
        ConcurrentDictionary<User, WebSocket> match = matchDictionary.GetMatch(matchId);
        // 랜덤매칭 결과
        // 매칭 성공 or 실패
        if (match?.Count == matchRequireUserCount)
        {
            var tasks = new List<Task>();
            ResponseMatchStatusType responseMatchStatusType = new ResponseMatchStatusType
            {
                type = Enum.GetName(typeof(MessageSendType), MessageSendType.MatchStart),
                matchId = matchId,
                username = "System",
                message = "매치 인원이 전부 입장이 완료 되어 매치가 시작됩니다."
            };
            string responseMatchStatusJson = JsonConvert.SerializeObject(responseMatchStatusType);
            byte[] encodedMessage = Encoding.UTF8.GetBytes(responseMatchStatusJson);
            
            foreach (var socket in match)
            {
                if (socket.Value.State == WebSocketState.Open)
                {
                    tasks.Add(socket.Value.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }
            Task.WhenAll(tasks);
            Program.logger.Info($"{matchId} 매치가 시작됩니다.");
        }
        else
        {
            RandomMatchDictionary randomMatchDictionary = RandomMatchDictionary.GetRandomMatchDictionary();
            var tasks = new List<Task>();
            ResponseMatchStatusType responseMatchStatusType = new ResponseMatchStatusType
            {
                type = Enum.GetName(typeof(MessageSendType), MessageSendType.MatchFail),
                matchId = matchId,
                username = "System",
                message = "매치 인원이 전부 입장하지 않아 매치 대기상태로 돌아갑니다."
            };
            string responseMatchStatusJson = JsonConvert.SerializeObject(responseMatchStatusType);
            byte[] encodedMessage = Encoding.UTF8.GetBytes(responseMatchStatusJson);
            
            // 매치 취소로 인한 매치 관련 딕셔너리 삭제
            matchDictionary.RemoveMatch(matchId);
            activeMatchDictionary.RemoveActiveMatch(matchId);
            foreach (var randomMatchUser in completeRandomMatch)
            {
                User user = randomMatchUser.Key;
                string uid = user.uid;
                currentMatchDictionary.RemoveMyMatch(uid);
            }
            
            // 매치 실패로 인한 매치 대기 딕셔너리에 다시 추가
            foreach (var matchJoinData in match)
            {
                User user = matchJoinData.Key;
                WebSocket mySocket = matchJoinData.Value;
                randomMatchDictionary.SetMySocketFromRandomList(user, mySocket);
                if (mySocket.State == WebSocketState.Open)
                {
                    tasks.Add(mySocket.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }
            Task.WhenAll(tasks);
            Program.logger.Info($"{matchId} 매치가 실패하였습니다 들어온 유저들은 매치 대기상태로 이동됩니다.");
        }
    }
}