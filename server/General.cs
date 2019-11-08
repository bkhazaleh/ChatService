using System;
namespace server
{
    public class General
    {
        public enum Action
        {
            doNothing,
            WarnTheClient,
            StopTheClient
        }
        public enum Status
        {
            NotSet,
            Connected,
            Warning,
            ConnectionRefused
        }
        public const string SettingsFile = "appSettings.json";
    }
}
