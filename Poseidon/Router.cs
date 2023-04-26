using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poseidon;

public class Router
{
    private static readonly ChatRouter _chatRouter = new ChatRouter();
    private static readonly  GroupRouter _groupRouter = new GroupRouter();
    public void Routing(ConcurrentDictionary<User,WebSocket> webSockets, User user, StringBuilder message, CancellationTokenSource cts)
    {
        string route = JObject.Parse(message.ToString()).First.Path;
        switch (route)
        {
            case "server_message_send":
            case "whisper_message_send":
            case "group_message_send":
                _chatRouter.ChatRouting(route, webSockets, user, message, cts);
                break;
            case "group_join":
            case "group_leave":
                _groupRouter.GroupRouting(route, webSockets, user, message, cts);
                break;
            default:
                Program.logger.Error("Route not found");
                break;
        }
    }
}
