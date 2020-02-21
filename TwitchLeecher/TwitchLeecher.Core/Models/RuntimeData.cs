namespace TwitchLeecher.Core.Models {

    using System;

    public class RuntimeData {

        public String AccessToken { get; set; }

        public MainWindowInfo MainWindowInfo { get; set; }

        public Version Version { get; set; }
    }
}