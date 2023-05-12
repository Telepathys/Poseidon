namespace Poseidon;

public class Scheduler
{
    private static readonly App app = AppDictionary.GetAppDictionary().GetApp();
    private readonly int matchCheckTime = app.matchCheckTime != null ? app.matchCheckTime : 1;
    public void Start()
    {
        new Timer(new RandomMatchSystem().Detect, null, TimeSpan.Zero, TimeSpan.FromSeconds(matchCheckTime));
    }
}