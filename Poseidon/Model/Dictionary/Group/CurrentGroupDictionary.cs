using System.Collections.Concurrent;

namespace Poseidon;

public class CurrentGroupDictionary
{
    private static CurrentGroupDictionary _currentGroupDictionary = null;
    private ConcurrentDictionary<string, string> currentGroup;

    private CurrentGroupDictionary()
    {
        currentGroup = new ConcurrentDictionary<string, string>();
    }

    public static CurrentGroupDictionary GetCurrentGroupDictionary()
    {
        if (_currentGroupDictionary == null)
        {
            _currentGroupDictionary = new CurrentGroupDictionary();
        }

        return _currentGroupDictionary;
    }

    public string GetMyGroup(string uid)
    {
        return currentGroup.TryGetValue(uid, out string myGroup) ? myGroup : null;
    }
    
    public void SetMyGroup(string uid, string groupKey)
    {
        currentGroup.TryAdd(uid, groupKey);
    }
    
    public void RemoveMyGroup(string uid)
    {
        currentGroup.TryRemove(uid, out _);
    }
}