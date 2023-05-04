using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace Poseidon;

public class GroupJoin
{
    private byte[] encodedMessage;
    private ResponseGroupJoinType responseGroupJoinType;
    private string responseGroupJoinTypeJson;
    private GroupLeave groupLeave = new GroupLeave();
    public void Join(User user, StringBuilder message, CancellationTokenSource cts)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
        CurrentGroupDictionary currentGroupDictionary = CurrentGroupDictionary.GetCurrentGroupDictionary();
        GroupJoinType GroupJoin = JsonConvert.DeserializeObject<GroupJoinType>(JObject.Parse(message.ToString()).First.First.ToString());
        string uid = user.uid;
        string usn = user.usn;
        string groupName = GroupJoin.groupName;
        string groupKey;
        if (groupName == null)
        {
            Program.logger.Error("그룹 이름이 없습니다. <GroupJoin-1>", webSockets, user);
            return;
        }

        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(groupName);
            byte[] hashBytes = sha1.ComputeHash(inputBytes);
            groupKey = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        string myGroupKey = currentGroupDictionary.GetMyGroup(uid);
        if (myGroupKey != groupKey)
        {
            // 그룹에 속해 있고 같은 방이 아닐 경우 퇴장
            if (myGroupKey != null)
            {
                groupLeave.Leave(user, message, cts, myGroupKey);
            }
            currentGroupDictionary.SetMyGroup(uid, groupKey);
            CheckGroupAndJoin(groupKey, groupName, webSockets, user, cts);
        }
    }
    
    public void CheckGroupAndJoin(string groupKey, string groupName, ConcurrentDictionary<User, WebSocket> webSockets, User user, CancellationTokenSource cts)
    {
        GroupDictionary groupDictionary = GroupDictionary.GetGroupDictionary();
        string uid = user.uid;
        string usn = user.usn;
        ConcurrentDictionary<string, WebSocket> groupList = groupDictionary.GetGroupList(groupKey);
        webSockets.TryGetValue(user, out WebSocket mySocket);
        // 존재한다면
        if (groupList != null)
        {
            // 내가 해당 그룹에 존재 하지 않으면 추가
            if (!groupList.ContainsKey(uid))
            {
                groupList.TryAdd(uid, mySocket);
            }
        }
        else // 존재하지 않는다면
        {
            ConcurrentDictionary<string, WebSocket> emptySocket = new ConcurrentDictionary<string, WebSocket>();
            emptySocket.TryAdd(uid, mySocket);
            groupDictionary.SetGroupList(groupKey, emptySocket);
            groupList = emptySocket;
        }
        Program.logger.Info($"{usn}님이 {groupName}({groupKey}) 그룹에 입장하였습니다.");
        
        // 새로운 그룹에 입장했을 경우 그 그룹에 있던 사람들에게 시스템 메세지
        var tasks = new List<Task>();
        responseGroupJoinType = new ResponseGroupJoinType
        {
            type = Enum.GetName(typeof(MessageSendType), MessageSendType.GroupJoin),
            groupKey = groupKey,
            uid = user.uid,
            username = "System",
            message = $"{user.usn} join Group"
        };
        responseGroupJoinTypeJson = JsonConvert.SerializeObject(responseGroupJoinType);
        encodedMessage = Encoding.UTF8.GetBytes(responseGroupJoinTypeJson);
        
        foreach (var socket in groupList)
        {
            if (socket.Value.State == WebSocketState.Open)
            {
                tasks.Add(socket.Value.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, cts.Token));
            }
        }
            
        Task.WhenAll(tasks);
    }
}