using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Poseidon;

// 1. 매치 그룹이 있는지 먼저 확인 후 없으면 생성 있으면 해당 그룹에 들어가서 대기
// 2. 매치 그룹에 설정한 인원수가 다 차면 모든 인원에게 매치 완료 메세지 전송하고 수락 거부 여부 확인
// 3. 전체 수락이면 매치 그룹 인원들에게 매치 아이디 전송
// 4. 1명이라도 거부면 다시 대기

public class RandomMatchWait
{
    RandomMatchMessageSend randomMatchMessageSend = new RandomMatchMessageSend();
    public void Wait(User user, StringBuilder message, CancellationTokenSource cts)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
        RandomMatchDictionary randomMatchDictionary = RandomMatchDictionary.GetRandomMatchDictionary();
        string uid = user.uid;
        string usn = user.usn;
        if (!randomMatchDictionary.CheckKey(user))
        {
            webSockets.TryGetValue(user, out WebSocket mySocket);
            randomMatchDictionary.SetMySocketFromRandomList(user, mySocket);
            Program.logger.Info($"{usn}({uid})님이 랜덤 매치를 정상적으로 시작하였습니다.");
            randomMatchMessageSend.Send(user, MessageSendType.RandomMatchWait,"랜덤 매치를 시작합니다.");
        }
        else
        {
            Program.systemMessage.Send(user, "이미 매치 대기중입니다.");
        }
    }
}
