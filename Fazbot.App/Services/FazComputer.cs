namespace Fazbot.App.Services;

public class FazComputer
{
    public const string Password = "michael";
    private bool _isLocked = true;
    
    public bool IsLocked() => _isLocked;

    public void Unlock() => _isLocked = false;
}