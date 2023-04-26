using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Poseidon;

public class ChatRouter
{
    private static readonly ServerMessage serverMessage = new ServerMessage();
    private static readonly WhisperMessage whisperMessage = new WhisperMessage();
    private static readonly GroupMessage groupMessage = new GroupMessage();
    public void ChatRouting(string route, ConcurrentDictionary<User,WebSocket> webSockets, User user, StringBuilder message, CancellationTokenSource cts)
    {
        switch (route)
        {
            case "server_message_send":
                serverMessage.Send(webSockets, user, message, cts);
                break;
            case "whisper_message_send":
                whisperMessage.Send(webSockets, user, message, cts);
                break;
            case "group_message_send":
                groupMessage.Send(webSockets, user, message, cts);
                break;
        }
    }
}