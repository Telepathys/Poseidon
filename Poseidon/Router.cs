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
    private static readonly  MatchRouter _MatchRouter = new MatchRouter();
    public void Routing(User user, StringBuilder message, CancellationTokenSource cts)
    {
        string route = JObject.Parse(message.ToString()).First.Path;
        switch (route)
        {
            case "server_message_send":
            case "whisper_message_send":
            case "group_message_send":
                if (Program.messageLimit.Check(user))
                    _chatRouter.ChatRouting(route, user, message, cts);
                break;
            case "group_join":
            case "group_leave":
                _groupRouter.GroupRouting(route, user, message, cts);
                break;
            case "random_match_wait":
            case "random_match_cancel":
            case "match_join":
            case "match_leave":
                _MatchRouter.MatchRouting(route, user, message, cts);
                break;
            default:
                Program.logger.Error("Route not found");
                break;
        }
    }
}
