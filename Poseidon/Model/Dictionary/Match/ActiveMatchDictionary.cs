using System.Collections.Concurrent;

namespace Poseidon;

public class ActiveMatchDictionary
{
    private static ActiveMatchDictionary _activeMatchDictionary = null;
    private static string[] activeMatchList;

    private ActiveMatchDictionary()
    {
        activeMatchList = new string[]{};
    }

    public static ActiveMatchDictionary GetActiveMatchDictionary()
    {
        if (_activeMatchDictionary == null)
        {
            _activeMatchDictionary = new ActiveMatchDictionary();
        }

        return _activeMatchDictionary;
    }

    public string[] GetActiveMatchIdList()
    {
        return activeMatchList;
    }
    
    public bool CheckActiveMatch(string matchId)
    {
        return Array.Exists(activeMatchList, element => element == matchId);
    }

    public void SetActiveMatch(string matchId)
    {
        if (!CheckActiveMatch(matchId))
        {
            activeMatchList = activeMatchList.Append(matchId).ToArray();
        }
    }
    
    public void RemoveActiveMatch(string matchId)
    {
        if (CheckActiveMatch(matchId))
        {
            activeMatchList = activeMatchList.Where(element => element != matchId).ToArray();
        }
    }
}