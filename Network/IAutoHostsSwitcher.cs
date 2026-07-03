namespace Barbar.HostsSwitcher.Network
{
    public interface IAutoHostsSwitcher
    {
        AutoSwitchResult EvaluateAndSwitch();
    }
}