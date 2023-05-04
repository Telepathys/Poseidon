using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Poseidon;

public class Socket
{
    WebSocketReceiveResult result;
    
    public async Task Start(WebSocket webSocket, HttpContext context)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        var query = context.Request.Query;
        var token = query["token"];
        JwtTokenSystem jwtTokenSystem = new JwtTokenSystem();
        User user = jwtTokenSystem.ValidateJwtToken(token);
        if (user != null)
        {
            ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
            Program.logger.Info($"{user.usn}({user.uid})님이 서버 연결");
            Init(webSockets, user);
            webSockets = socketDictionary.SetMySocket(user, webSocket);
            var buffer = new byte[1024 * 4];
            CancellationTokenSource cts = new CancellationTokenSource();
            while (true)
            {
                StringBuilder message = new StringBuilder();
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), cts.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Program.logger.Info($"{user.usn}님이 서버 연결 해제");
                    Init(webSockets, user);
                    break;
                }
                message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                Program.Router.Routing(user, message, cts);
            }
        }
        else
        {
            Program.logger.Error("Token is invalid");
            context.Response.StatusCode = 400;
        }
    }
    
    public void Init(ConcurrentDictionary<User,WebSocket> webSockets, User user)
    {
        string uid = user.uid;
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        CurrentGroupDictionary currentGroupDictionary = CurrentGroupDictionary.GetCurrentGroupDictionary();
        MessageHistoryDictionary messageHistoryDictionary = MessageHistoryDictionary.GetMessageHistoryDictionary();
        MessageBanDictionary messageBanDictionary = MessageBanDictionary.GetMessageBanDictionary();
        
        // 소켓 초기화
        socketDictionary.RemoveMySocket(user);
        
        // 그룹 관련 초기화

        string myGroupKey = currentGroupDictionary.GetMyGroup(uid);
        if (myGroupKey != null)
        {
            GroupLeave groupLeave = new GroupLeave();
            groupLeave.Leave(user, new StringBuilder(), new CancellationTokenSource(), myGroupKey);
        }
        
        // 메세지 제한 초기화
        messageHistoryDictionary.RemoveMyMessageHistory(uid);
        messageBanDictionary.RemoveMessageBan(uid);
    }
}
