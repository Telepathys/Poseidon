using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Poseidon;

public class ErrorMessage
{
    public void Send(User user, string message)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
        string error;
        try
        {
            MatchCollection matcher = Regex.Matches(message, "\\<(.*?)\\>");
            error = matcher[0].Groups[1].Value;
        }
        catch (Exception e)
        {
            error = "Unknown";
        }
        
        ResponseErrorMessageSendType responseErrorMessageSend = new ResponseErrorMessageSendType
        {
            type = Enum.GetName(typeof(MessageSendType), MessageSendType.ErrorMessage),
            error = error,
            username = "Error",
            message = message
        };
        
        string responseErrorMessageSendJson = JsonConvert.SerializeObject(responseErrorMessageSend);
        byte[] encodedMessage = Encoding.UTF8.GetBytes(responseErrorMessageSendJson);

        webSockets.TryGetValue(user, out WebSocket errorSocket);
        errorSocket.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}