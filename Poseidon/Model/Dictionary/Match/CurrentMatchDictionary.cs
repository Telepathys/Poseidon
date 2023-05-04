using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Poseidon;

public class CurrentMatchDictionary
{
    private static CurrentMatchDictionary _currentMatchDictionary = null;
    private static ConcurrentDictionary<string, string> currenMatch;

    private CurrentMatchDictionary()
    {
        currenMatch = new ConcurrentDictionary<string, string>();
    }

    public static CurrentMatchDictionary GetCurrentMatchDictionary()
    {
        if (_currentMatchDictionary == null)
        {
            _currentMatchDictionary = new CurrentMatchDictionary();
        }

        return _currentMatchDictionary;
    }

    public string GetMyMatchId(string uid)
    {
        return currenMatch.TryGetValue(uid, out var matchId) ? matchId : null;
    }
    
    public bool Check(string uid)
    {
        return currenMatch.ContainsKey(uid);
    }
    
    public void SetMyMatch(string uid, string matchId)
    {
        currenMatch.TryAdd(uid, matchId);
    }
    
    public void RemoveMyMatch(string uid)
    {
        currenMatch.TryRemove(uid, out _);
    }
}