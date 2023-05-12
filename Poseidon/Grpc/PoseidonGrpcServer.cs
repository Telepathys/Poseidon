using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using Enum = System.Enum;

namespace Poseidon;
public class PoseidonGrpcServer : Poseidon.PoseidonBase
{
    /// <summary>
    /// 테스트
    /// </summary>
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        User user = AuthenticateUser(context);

        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + user.usn
        });
    }

    
    /// <summary>
    /// 현재 접속중인 유저 검색
    /// </summary>
    public override Task<UserListReply> GetUserList(Empty request, ServerCallContext context)
    {
        AuthenticateUser(context);

        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        var socketList = socketDictionary.GetSocketList();
        
        UserListReply reply = new UserListReply();
        foreach (var thisSocket in socketList)
        {
            User thisUser = thisSocket.Key;
            UserInfo thisUserInfo = new UserInfo
            {
                Uid = thisUser.uid,
                Usn = thisUser.usn
            };
            reply.UserInfo.Add(thisUserInfo);
        }

        reply.Total = reply.UserInfo.Count;

        return Task.FromResult(reply);
    }
    
    /// <summary>
    /// 특정 알림 메세지 전송
    /// </summary>
    public override Task<Empty> Notice(NoticeRequest request, ServerCallContext context)
    {
        AuthenticateUser(context);

        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        WebSocket targetSocket = socketDictionary.GetTargetSocket(request.Uid);
        if (targetSocket == null)
        {
            Program.logger.Error("현재 접속중이지 않거나 존재하지 않는 유저입니다.");
            throw new RpcException(new Status(StatusCode.Cancelled, "현재 접속중이지 않거나 존재하지 않는 유저입니다."));
        }
        var noticeType = request.NoticeType;
        var noticeCount = request.NoticeCount;
        ResponseNoticeMessageSendType responseNoticeMessageSend = new ResponseNoticeMessageSendType
        {
            type = Enum.GetName(typeof(MessageSendType), MessageSendType.Notice),
            noticeType = Enum.GetName(typeof(NoticeType), noticeType),
            noticeCount = noticeCount
        };
        string responseNoticeMessageSendJson = JsonConvert.SerializeObject(responseNoticeMessageSend);
        byte[] encodedMessage = Encoding.UTF8.GetBytes(responseNoticeMessageSendJson);
        targetSocket.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);

        return Task.FromResult(new Empty());
    }
    
    /// <summary>
    /// JWT 토큰 검사
    /// </summary>
    private User AuthenticateUser(ServerCallContext context)
    {
        var authorization = context.RequestHeaders.Get("Authorization");
        JwtTokenSystem jwtTokenSystem = new JwtTokenSystem();
        User user = jwtTokenSystem.ValidateJwtToken(authorization.Value.Replace("Bearer ", ""));

        if (user == null)
        {
            Program.logger.Error("gRPC Token is invalid");
            throw new RpcException(new Status(StatusCode.Unauthenticated, "gRPC Token is invalid"));
        }

        return user;
    }
}

