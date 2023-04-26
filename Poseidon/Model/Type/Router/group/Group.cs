namespace Poseidon;

public class GroupJoinType {
    public string groupName { get; set; }
}

public class ResponseGroupJoinType {
    public string type { get; set; }
    public string groupKey { get; set; }
    public string uid { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}

public class GroupLeaveType {
    public string groupKey { get; set; }
}

public class ResponseGroupLeaveType {
    public string type { get; set; }
    public string groupKey { get; set; }
    public string uid { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}