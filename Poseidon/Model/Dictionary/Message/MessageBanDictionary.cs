using System.Collections.Concurrent;

namespace Poseidon;

public class MessageBanDictionary
{
    private static MessageBanDictionary _messageBanDictionary = null;
    private static ConcurrentDictionary<string, DateTime> messageBan;

    private MessageBanDictionary()
    {
        messageBan = new ConcurrentDictionary<string, DateTime>();
    }

    public static MessageBanDictionary GetMessageBanDictionary()
    {
        if (_messageBanDictionary == null)
        {
            _messageBanDictionary = new MessageBanDictionary();
        }

        return _messageBanDictionary;
    }
    
    public bool Check(string uid)
    {
        return messageBan.ContainsKey(uid);
    }

    public DateTime GetMessageBan(string uid)
    {
        messageBan.TryGetValue(uid, out DateTime banTime);
        return banTime;
    }
    
    public void SetMessageBan(string uid)
    {
        DateTime now = DateTime.Now;
        messageBan.TryAdd(uid, now);;
    }
    
    public void RemoveMessageBan(string uid)
    {
        messageBan.TryRemove(uid, out _);
    }
}