using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Poseidon;


// 메세지 입력시 유저 최근 채팅 기록 딕셔너리에 계속 채팅을 담고 담기전에 채팅 리밋 전 값의 날짜가 현재 날짜와 5초 안쪽이면 밴 딕션너리에 해당 유저를 추가하고 30초가 지나면 밴 딕션너리에서 해당 유저를 제거한다.
// 리밋 개수 전에 데이트 날짜가 5초 이외면 계속 초기화

public class MessageLimit
{
    // 메세지 초당 개수 제한
    readonly int messageCountlimit = 5;
    // 메세지 개수 제한 체크 시간
    readonly int messageSecondlimit = 5;
    // 메세지 제한 시간
    readonly int banSecondlimit = 30;
    
    public bool Check(User user)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
        MessageHistoryDictionary messageHistoryDictionary = MessageHistoryDictionary.GetMessageHistoryDictionary();
        MessageBanDictionary messageBanDictionary = MessageBanDictionary.GetMessageBanDictionary();
        DateTime now = DateTime.Now;
        string uid = user.uid;
        string usn = user.usn;
        // 메세지 밴 유저인지 확인
        if (messageBanDictionary.Check(uid))
        {
            DateTime banTime = messageBanDictionary.GetMessageBan(uid);
            int leftBanTime = banSecondlimit - (now - banTime).Seconds;
            if (leftBanTime > 0)
            {
                Program.systemMessage.Send(user, $"메세지가 제한된 상태입니다. {leftBanTime}초 후에 다시 시도해주세요.");
                return false;
            }
            messageBanDictionary.RemoveMessageBan(uid);
        }
        
        DateTime[] messageHistoryArray = messageHistoryDictionary.GetMyMessageHistory(uid);
        if (messageHistoryArray == null)
        {
            messageHistoryArray = new DateTime[]{};
        }

        // 배열 돌면서 시간이 만료된 것들은 제거
        foreach (var messageTime in messageHistoryArray)
        {
            TimeSpan dateDiff = now - messageTime;
            if(dateDiff.Seconds > messageSecondlimit)
            {
                messageHistoryArray = messageHistoryArray.Where(e => e != messageTime).ToArray();
            }
        }

        if (messageHistoryArray.Length >= messageCountlimit)
        {
            Program.logger.Warn($"{usn}({uid})님이 무분별한 메세지로 {banSecondlimit}초간 메세지 전송이 제한됩니다.");
            Program.systemMessage.Send(user, $"무분별한 메세지로 {banSecondlimit}초간 메세지 전송이 제한됩니다.");
            messageBanDictionary.SetMessageBan(uid);
            messageHistoryDictionary.RemoveMyMessageHistory(uid);
            return false;
        }

        messageHistoryDictionary.RemoveMyMessageHistory(uid);
        DateTime[] newChatDateTimeArray = messageHistoryArray.Append(now).ToArray();
        messageHistoryDictionary.SetMyMessageHistory(uid, newChatDateTimeArray);

        return true;
    }
}