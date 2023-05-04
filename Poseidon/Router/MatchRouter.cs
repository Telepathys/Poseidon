using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Poseidon;

public class MatchRouter
{
    private static readonly RandomMatchWait randomMatchWait = new RandomMatchWait();
    private static readonly RandomMatchCancel randomMatchCancel = new RandomMatchCancel();
    public void MatchRouting(string route, User user, StringBuilder message, CancellationTokenSource cts)
    {
        switch (route)
        {
            case "random_match_wait":
                randomMatchWait.Wait(user, message, cts);
                break;
            case "random_match_cancel":
                randomMatchCancel.Cancel(user, message, cts);
                break;
            case "random_match_join":
                break;
            case "random_match_leave":
                break;
        }
    }
}