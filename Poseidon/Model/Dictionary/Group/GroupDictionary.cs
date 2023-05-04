using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Poseidon;

public class GroupDictionary
{
    private static GroupDictionary _groupDictionary = null;
    private static ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> group;

    private GroupDictionary()
    {
        group = new ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>>();
    }

    public static GroupDictionary GetGroupDictionary()
    {
        if (_groupDictionary == null)
        {
            _groupDictionary = new GroupDictionary();
        }

        return _groupDictionary;
    }

    public ConcurrentDictionary<string,WebSocket> GetGroupList(string groupKey)
    {
        return group.TryGetValue(groupKey, out ConcurrentDictionary<string,WebSocket> groupList) ? groupList : null;
    }
    
    public void SetGroupList(string groupKey, ConcurrentDictionary<string, WebSocket> sockets)
    {
        group.TryAdd(groupKey, sockets);
    }
    
    public void RemoveGroupList(string groupKey)
    {
        group.TryRemove(groupKey, out _); 
    }
}