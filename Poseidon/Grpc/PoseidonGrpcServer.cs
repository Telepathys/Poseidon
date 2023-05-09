using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Poseidon;
public class PoseidonGrpcServer : Poseidon.PoseidonBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        var authorization = context.RequestHeaders.Get("Authorization");
        JwtTokenSystem jwtTokenSystem = new JwtTokenSystem();
        User user = jwtTokenSystem.ValidateJwtToken(authorization.Value.Replace("Bearer ", ""));

        if (user == null)
        {
            Program.logger.Error("gRPC Token is invalid");
            throw new RpcException(new Status(StatusCode.Unauthenticated, "gRPC Token is invalid"));
        }

        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + user.usn
        });
    }

    public override Task<UserListReply> GetUserList(Empty request, ServerCallContext context)
    {
        var authorization = context.RequestHeaders.Get("Authorization");
        JwtTokenSystem jwtTokenSystem = new JwtTokenSystem();
        User user = jwtTokenSystem.ValidateJwtToken(authorization.Value.Replace("Bearer ", ""));

        if (user == null)
        {
            Program.logger.Error("gRPC Token is invalid");
            throw new RpcException(new Status(StatusCode.Unauthenticated, "gRPC Token is invalid"));
        }

        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        var socketList = socketDictionary.GetSocketList();
        
        UserListReply reply = new UserListReply();
        foreach (var thisSocket in socketList)
        {
            User thisUser = thisSocket.Key;
            UserProto thisUserProto = new UserProto
            {
                Uid = thisUser.uid,
                Usn = thisUser.usn
            };
            reply.UserProto.Add(thisUserProto);
        }

        reply.Total = reply.UserProto.Count;

        return Task.FromResult(reply);
    }
}

