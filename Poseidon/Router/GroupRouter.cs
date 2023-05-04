using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Poseidon;

public class GroupRouter
{
    private static readonly GroupJoin groupJoin = new GroupJoin();
    private static readonly GroupLeave groupLeave = new GroupLeave();
    public void GroupRouting(string route, User user, StringBuilder message, CancellationTokenSource cts)
    {
        switch (route)
        {
            case "group_join":
                groupJoin.Join(user, message, cts);
                break;
            case "group_leave":
                groupLeave.Leave(user, message, cts);
                break;
        }
    }
}