using System.Collections.Concurrent;

namespace Poseidon;

public class ActiveMatchDictionary
{
    private static ActiveMatchDictionary _activeMatchDictionary = null;
    private static ConcurrentDictionary<string, string[]> activeMatchList;

    private ActiveMatchDictionary()
    {
        activeMatchList = new ConcurrentDictionary<string, string[]>();
    }

    public static ActiveMatchDictionary GetActiveMatchDictionary()
    {
        if (_activeMatchDictionary == null)
        {
            _activeMatchDictionary = new ActiveMatchDictionary();
        }

        return _activeMatchDictionary;
    }

    
    public bool CheckActiveMatch(string matchId)
    {
        return activeMatchList.ContainsKey(matchId);
    }
    
    public string[] GetActiveMatchUserList(string matchId)
    {
        return activeMatchList.TryGetValue(matchId, out var uid) ? uid : null;
    }

    public void SetActiveMatch(string matchId, string[] uid)
    {
        activeMatchList.TryAdd(matchId, uid);
    }
    
    public void RemoveActiveMatch(string matchId)
    {
        activeMatchList.TryRemove(matchId, out _);
    }
}