using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Poseidon;

public class MatchDictionary
{
    private static MatchDictionary _matchDictionary = null;
    private static ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> matchList;

    private MatchDictionary()
    {
        matchList = new ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>>();
    }

    public static MatchDictionary GetMatchDictionary()
    {
        if (_matchDictionary == null)
        {
            _matchDictionary = new MatchDictionary();
        }

        return _matchDictionary;
    }

    public ConcurrentDictionary<string, WebSocket> GetMatch(string matchId)
    {
        return matchList.TryGetValue(matchId, out var match) ? match : null;
    }
}