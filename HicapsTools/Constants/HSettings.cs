using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsTools.Constants
{
    /// <summary>
    /// configuration Settings go here
    /// </summary>
    /// <remarks>
    /// Made it HSettings so no conflict with normal Settings
    /// </remarks>
    public static class HSettings
    {
        /// <summary>
        /// Inital setup screen popup with controller runs first time
        /// </summary>
        public static readonly string Configured = "Configured";

        // Server Settings
        /// <summary>
        /// Gets/Sets the Network name for the terminal
        /// </summary>
        public static readonly string NetworkName = "NetworkName";
        /// <summary>
        /// Gets/Sets Server IP Settings
        /// </summary>
        public static readonly string ServerIP = "ServerIp";
        public static readonly string ServerPort = "ServerPort";
        public static readonly string ServerPortDefault = "11000";
        public static readonly string FoundTerminals = "FoundTerminals";

        // Logging
        public static readonly string LoggingEnabled = "LoggingEnabled";
        public static readonly string LogName = "LogName";
        public static readonly string LogKeyName = "LogKeyName";

        // Gets/Sets ComPorts to scan for terminals
        public static readonly string TerminalCommPorts = "TerminalCommPorts";

        // Client Settings
        public static readonly string StatusBox = "StatusBox";
        public static readonly string ManualPanPopup = "ManualPanPopup";
        public static readonly string DefaultTerminal = "DefaultTerminal";

        // General Setting Values
        /// <summary>
        /// Yes/No Values
        /// </summary>
        public static readonly string Yes = "Y";
        public static readonly string No = "N";

        /// <summary>
        /// True/False Values
        /// </summary>
        public static readonly string True = "true";
        public static readonly string False = "false";

    }
}
