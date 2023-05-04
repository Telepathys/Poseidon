using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Poseidon;

public class ChatRouter
{
    private static readonly ServerMessage serverMessage = new ServerMessage();
    private static readonly WhisperMessage whisperMessage = new WhisperMessage();
    private static readonly GroupMessage groupMessage = new GroupMessage();
    private static readonly MatchMessage matchMessage = new MatchMessage();
    public void ChatRouting(string route, User user, StringBuilder message, CancellationTokenSource cts)
    {
        switch (route)
        {
            case "server_message_send":
                serverMessage.Send(user, message, cts);
                break;
            case "whisper_message_send":
                whisperMessage.Send(user, message, cts);
                break;
            case "group_message_send":
                groupMessage.Send(user, message, cts);
                break;
            case "match_message_send":
                matchMessage.Send(user, message, cts);
                break;
        }
    }
}