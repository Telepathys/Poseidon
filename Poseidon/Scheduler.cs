namespace Poseidon;

public class Scheduler
{
    public void Start()
    {
        new Timer(new RandomMatchSystem().Detect, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
}