using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Poseidon;

public class RandomMatchDictionary
{
    private static RandomMatchDictionary _randomMatchDictionary = null;
    private static ConcurrentDictionary<User, WebSocket> ramdomMatchWaitList;

    private RandomMatchDictionary()
    {
        ramdomMatchWaitList = new ConcurrentDictionary<User, WebSocket>();
    }

    public static RandomMatchDictionary GetRandomMatchDictionary()
    {
        if (_randomMatchDictionary == null)
        {
            _randomMatchDictionary = new RandomMatchDictionary();
        }

        return _randomMatchDictionary;
    }
    
    public ConcurrentDictionary<User, WebSocket> GetRandomList()
    {
        return ramdomMatchWaitList;
    }

    public WebSocket GetMySocketFromRandomList(User user)
    {
        return ramdomMatchWaitList.TryGetValue(user, out WebSocket mySocket) ? mySocket : null;
    }
    
    public bool CheckKey(User user)
    {
        return ramdomMatchWaitList.ContainsKey(user);
    }
    
    public void SetMySocketFromRandomList(User user, WebSocket mySocket)
    {
        ramdomMatchWaitList.TryAdd(user, mySocket);
    }
    
    public void RemoveMeFromRandomList(User user)
    {
        ramdomMatchWaitList.TryRemove(user, out _); 
    }
}