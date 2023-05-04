namespace Poseidon;

public class MatchJoinType {
    public string matchId { get; set; }
}

public class ResponseMatchJoinType {
    public string? type { get; set; }
    public string matchId { get; set; }
    public string uid { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}

public class ResponseMatchLeaveType {
    public string type { get; set; }
    public string matchId { get; set; }
    public string uid { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}

public class ResponseMatchStatusType {
    public string? type { get; set; }
    public string matchId { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}

public class MatchCustomDataType {
    public int dataType { get; set; }
    public byte[] data { get; set; }
}

public class ResponseMatchCustomDataType {
    public string? type { get; set; }
    public string matchId { get; set; }
    public string uid { get; set; }
    public string username { get; set; }
    public int dataType { get; set; }
    public byte[] data { get; set; }
}