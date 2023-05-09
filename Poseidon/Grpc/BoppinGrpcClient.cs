using Grpc.Core;
using Grpc.Net.Client;

namespace Poseidon;

public class BoppinGrpcClient
{
    private readonly Boppin.BoppinClient _client;

    public BoppinGrpcClient(string address)
    {
        var channel = GrpcChannel.ForAddress(address);
        _client = new Boppin.BoppinClient(channel);
    }

    public async Task<string> SayHelloAsync(User user)
    {
        var request = new TTTRequest { Name = user.usn };
        var headers = new Metadata()
        {
            { "Authorization", $"Bearer {user.token}" }
        };
        var response = await _client.SayHelloAsync(request,headers);
        return response.Message;
    }
}