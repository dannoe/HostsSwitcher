namespace Barbar.HostsSwitcher.Network
{
    public enum AutoSwitchStatus
    {
        Switched,
        NoMatch,
        AmbiguousMatch,
        AlreadyActive,
        Error
    }

    public class AutoSwitchResult
    {
        public AutoSwitchStatus Status { get; set; }
        public string SelectedProfile { get; set; }
        public string Message { get; set; }
        public string[] AmbiguousCandidates { get; set; }

        public AutoSwitchResult(AutoSwitchStatus status, string message, string selectedProfile = null, string[] ambiguousCandidates = null)
        {
            Status = status;
            Message = message ?? string.Empty;
            SelectedProfile = selectedProfile;
            AmbiguousCandidates = ambiguousCandidates;
        }
    }
}