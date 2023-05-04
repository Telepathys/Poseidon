namespace Poseidon;
 
public class ServerMessageSendType {
    public string message { get; set; }
}

public class ResponseServerMessageSendType {
    public string type { get; set; }
    public string uid { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}

public class WhisperMessageSendType {
    public string uid { get; set; }
    public string message { get; set; }
}

public class ResponseWhisperMessageSendType {
    public string type { get; set; }
    public string uid { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}

public class GroupMessageSendType {
    public string message { get; set; }
}

public class ResponseGroupMessageSendType {
    public string type { get; set; }
    public string uid { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}

public class MatchMessageSendType {
    public string message { get; set; }
}

public class ResponseMatchMessageSendType {
    public string type { get; set; }
    public string uid { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}

public class ResponseErrorMessageSendType {
    public string type { get; set; }
    public string error { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}

public class ResponseSystemMessageSendType {
    public string type { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}

public class ResponseRandomMatchMessageSendType {
    public string type { get; set; }
    public string matchId { get; set; }
    public string uid { get; set; }
    public string username { get; set; }
    public string message { get; set; }
}