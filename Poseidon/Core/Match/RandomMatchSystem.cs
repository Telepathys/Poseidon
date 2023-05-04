using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace Poseidon;

public class RandomMatchSystem
{
    RandomMatchDictionary randomMatchDictionary = RandomMatchDictionary.GetRandomMatchDictionary();
    RandomMatchMessageSend randomMatchMessageSend = new RandomMatchMessageSend();
    Mutex mutex = new Mutex(false,"RandomMatchSystemDetect");
    Random random = new Random();
    // 한개의 매칭당 필요 인원
    readonly int matchRequireUserCount = 2;
    readonly double matchMakeCountControl = 0.1;
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
                foreach (var ramdomMatchWaitUser in completeRandomMatch)
                {
                    User user = ramdomMatchWaitUser.Key;
                    randomMatchMessageSend.Send(completeRandomMatch, user, MessageSendType.RandomMatchComplete,"랜덤 매치가 성공적으로 매칭되었습니다.", matchId);
                    randomMatchWaitList.TryRemove(user, out _);
                }
                waitMatchJoin(matchId);
            }
        }

        mutex.ReleaseMutex();
    }

    public async Task waitMatchJoin(string matchId)
    {
        MatchDictionary matchDictionary = MatchDictionary.GetMatchDictionary();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        using var timer = new Timer(async state =>
        {
            ConcurrentDictionary<string, WebSocket> match = matchDictionary.GetMatch(matchId);
            if (match?.Count == matchRequireUserCount)
            {
                cancellationTokenSource.Cancel();
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        
        // 10초 동안 대기하는 비동기 작업
        await Task.Delay(TimeSpan.FromSeconds(9), cancellationToken);
        timer.Dispose();
        
        ConcurrentDictionary<string, WebSocket> match = matchDictionary.GetMatch(matchId);
        // 랜덤매칭 결과
        // 매칭 성공 or 실패
        if (match?.Count == matchRequireUserCount)
        {
            Console.WriteLine("asdasdasdas");
        }
        else
        {
            Console.WriteLine("123123");
        }
    }
}