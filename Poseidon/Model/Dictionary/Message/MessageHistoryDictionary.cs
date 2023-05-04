using System.Collections.Concurrent;

namespace Poseidon;

public class MessageHistoryDictionary
{
    private static MessageHistoryDictionary _messageHistoryDictionary = null;
    private static ConcurrentDictionary<string, DateTime[]> messageHistory;

    private MessageHistoryDictionary()
    {
        messageHistory = new ConcurrentDictionary<string, DateTime[]>();
    }

    public static MessageHistoryDictionary GetMessageHistoryDictionary()
    {
        if (_messageHistoryDictionary == null)
        {
            _messageHistoryDictionary = new MessageHistoryDictionary();
        }

        return _messageHistoryDictionary;
    }

    public DateTime[] GetMyMessageHistory(string uid)
    {
        return messageHistory.TryGetValue(uid, out DateTime[] messageHistoryArray) ? messageHistoryArray : null;
    }
    
    public void SetMyMessageHistory(string uid, DateTime[] dateTime)
    {
        messageHistory.TryAdd(uid, dateTime);
    }
    
    public void RemoveMyMessageHistory(string uid)
    {
        messageHistory.TryRemove(uid,out _);
    }
}