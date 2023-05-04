using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poseidon;

public class GroupLeave
{
    private byte[] encodedMessage;
    private ResponseGroupLeaveType responseGroupLeave;
    private string responseGroupLeaveJson;
    
    public void Leave(User user, StringBuilder message, CancellationTokenSource cts, string extraGroupKey = null)
    {
        CurrentGroupDictionary currentGroupDictionary = CurrentGroupDictionary.GetCurrentGroupDictionary();
        GroupDictionary groupDictionary = GroupDictionary.GetGroupDictionary();
        string uid = user.uid;
        string usn = user.usn;
        string groupKey;
        if (extraGroupKey == null)
        {
            GroupLeaveType groupLeave = JsonConvert.DeserializeObject<GroupLeaveType>(JObject.Parse(message.ToString()).First.First.ToString());
            groupKey = extraGroupKey != null ? extraGroupKey : groupLeave.groupKey;
        }
        else
        {
            groupKey = extraGroupKey;
        }

        if (groupKey == null)
        {
            Program.logger.Error("그룹 키가 없습니다. <GroupLeave-1>", user);
            return;
        }
        currentGroupDictionary.RemoveMyGroup(uid);
        ConcurrentDictionary<string, WebSocket> groupList = groupDictionary.GetGroupList(groupKey);
        if (groupList == null)
        {
            Program.logger.Error("그룹키에 맞는 그룹이 존재하지 않습니다. <GroupLeave-2>", user);
            return;
        }
        groupList.TryGetValue(uid, out WebSocket mySocket);
        if (mySocket == null)
        {
            Program.logger.Error("해당 그룹에 나의 소켓이 존재하지 않습니다. <GroupLeave-3>", user);
            return;
        }

        groupList.TryRemove(uid, out _);
        Program.logger.Info($"{usn}님이 {groupKey} 그룹에 퇴장하였습니다.");
        
        List<Task> tasks = new List<Task>();
        responseGroupLeave = new ResponseGroupLeaveType
        {
            type = Enum.GetName(typeof(MessageSendType), MessageSendType.GroupLeave),
            groupKey = groupKey,
            uid = user.uid,
            username = "System",
            message = $"{user.usn} leave Group"
        };
        responseGroupLeaveJson = JsonConvert.SerializeObject(responseGroupLeave);
        encodedMessage = Encoding.UTF8.GetBytes(responseGroupLeaveJson);
        
        tasks.Add(mySocket.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, cts.Token));
        switch (groupList.Count)
        {
            case 0: 
                groupDictionary.RemoveGroupList(groupKey);
                break;
            case int n when (n > 0):
                // 떠난 그룹에 인원이 있다면 떠난 사람을 알림
                foreach (var socket in groupList)
                {
                    if (socket.Value.State == WebSocketState.Open && socket.Key != uid)
                    {
                        tasks.Add(socket.Value.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, cts.Token));
                    }
                }
                break;
        }
        Task.WhenAll(tasks);
    }
}