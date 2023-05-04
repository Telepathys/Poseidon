namespace Poseidon;

public enum MessageSendType
{
    SystemMessage,
    ErrorMessage,
    ServerMessage,
    WhisperMessage,
    GroupMessage,
    GroupJoin,
    GroupLeave,
    RandomMatchWait,
    RandomMatchCancel,
    RandomMatchComplete,
    MatchJoin,
    MatchStart,
    MatchFail,
    MatchLeave,
}