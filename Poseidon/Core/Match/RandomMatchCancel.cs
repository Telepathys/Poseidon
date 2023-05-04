using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Poseidon;

public class RandomMatchCancel
{
    RandomMatchMessageSend randomMatchMessageSend = new RandomMatchMessageSend();
    public void Cancel(User user, StringBuilder message, CancellationTokenSource cts)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
        RandomMatchDictionary randomMatchDictionary = RandomMatchDictionary.GetRandomMatchDictionary();
        string uid = user.uid;
        string usn = user.usn;
        if (randomMatchDictionary.CheckKey(user))
        {
            randomMatchDictionary.RemoveMeFromRandomList(user);
            randomMatchMessageSend.Send(user, MessageSendType.RandomMatchCancel,"랜덤 매치를 취소하였습니다.");
            Program.logger.Info($"{usn}({uid})님이 랜덤 매치를 정상적으로 취소하였습니다.");
        }
        else
        {
            Program.systemMessage.Send(webSockets, user,"매치 대기중이 아닙니다.");
        }
    }
}