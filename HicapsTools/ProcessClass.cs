using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using System.ServiceProcess;
using System.Security;
using HicapsTools.Constants;

namespace HicapsTools
{
    public static class ProcessClass
    {
        #region Methods (25)

        // Public Methods (20) 

        public static int CleanupLogFiles()
        {
            int ctr = 0;
            try
            {

                string path = getServiceConfigPath();
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] fis = di.GetFiles("*.txt");
                foreach (FileInfo fi in fis)
                {
                    if (fi.LastWriteTime < DateTime.Now.AddDays(-30))
                    {

                        fi.Delete();
                        ctr++;
                    }
                }
            }
            catch (Exception )
            {
            }
            return ctr;
        }

        public static string getServiceConfigFile()
        {
            string configFile = getAssemblyServiceConfigFile();
            // Does Service config file exist at same location as currently executing assembly
            if (!File.Exists(configFile))
            {
                configFile = getAssemblyServiceConfigFileAlt();
            }
            // Does Service config "HicapsConnectService.config" file exist at same location as currently executing assembly
            if (!File.Exists(configFile))
            {
                configFile = getServiceConfigPath().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + @"\HicapsConnectService.exe.config";
            }
            // Does Service config "HicapsConnectService.config" file exist at same user location
            if (!File.Exists(configFile))
            {
                WriteDefaultUserXmlConfig(getAssemblyServiceConfigFile());
                configFile = getAssemblyServiceConfigFile();
            }
            return configFile;
        }

        public static string getServiceConfigPath()
        {
            try
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + Path.DirectorySeparatorChar + "HicapsConnect" + Path.DirectorySeparatorChar;
            }
            catch (Exception)
            {

                //  Log(ex);
                //Log("Could not get location of All Users Folder");
                return "";
            }
        }

        public static string getUserConfigFile()
        {
            string configFile = getUserConfigPath() + Path.DirectorySeparatorChar + Programs.API + ".config";
            CreatePathMissing(configFile);
            if (!File.Exists(configFile))
            {
                WriteDefaultUserXmlConfig(configFile);
            }
            return configFile;
        }

        public static string getUserServiceConfigFile()
        {
            string configFile = getUserConfigPath() + Path.DirectorySeparatorChar + Programs.Service + ".config";
            CreatePathMissing(configFile);
            if (!File.Exists(configFile))
            {
                WriteDefaultServiceXmlConfig(configFile);
            }
            return configFile;
        }

        private static void WriteDefaultServiceXmlConfig(string startPath)
        {
            try
            {
                CreatePathMissing(startPath);
                File.WriteAllText(startPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration><appSettings><add key=\"LoggingEnabled\" value=\"true\" /><add key=\"LogName\" value=\"HICAPS Connect\" /><add key=\"NoNetwork\" value=\"false\" /><add key=\"Configured\" value=\"\" /><add key=\"ServerIp\" value=\"\" /><add key=\"ServerPort\" value=\"11000\" /><add key=\"FoundTerminals\" value=\"\" /><add key=\"TerminalCommPorts\" value = \"\" /><add key=\"NetworkName\" value = \"\" /></appSettings><runtime><legacyUnhandledExceptionPolicy enabled=\"1\"/></runtime></configuration>");
            }
            catch (Exception) { }
        }
        public static string getUserConfigFileSetting(string setting, string defaultSettingValue)
        {
            string startPath = getUserConfigFile();
            AppConfig myApiConfigEdit = new AppConfig(startPath);
            return myApiConfigEdit.GetValue(setting, defaultSettingValue);
        }

        public static bool setUserConfigFileSetting(string setting, string defaultSettingValue)
        {
            string startPath = getUserConfigFile();
            AppConfig myApiConfigEdit = new AppConfig(startPath);
            return myApiConfigEdit.SetValue(setting, defaultSettingValue);
        }
        public static string getUserConfigPath()
        {
            string userConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "HicapsConnect";
            try
            {
                if (!Directory.Exists(userConfigPath))
                {
                    Directory.CreateDirectory(userConfigPath);
                }
            }
            catch (Exception)
            { }
            return userConfigPath;
        }

        public static string getUserConfigServiceServiceFileSetting(string setting, string defaultSettingValue)
        {
            AppConfig myConfigEdit = new AppConfig(getServiceConfigFile(), true);
            return myConfigEdit.GetValue(setting, defaultSettingValue);
        }

        public static bool HICAPSConnectConfigurationCheck()
        {
            //TODO write code
            return true; // everything is okay
        }

        public static bool HICAPSConnectConfigurationFix()
        {
            //TODO write code
            return true; // everything got fixed
        }

        public static bool IsAnAdministrator()
        {
            WindowsIdentity identity =
               WindowsIdentity.GetCurrent();
            WindowsPrincipal principal =
               new WindowsPrincipal(identity);
            return principal.IsInRole
               (WindowsBuiltInRole.Administrator);
        }
        public static bool isInRemoteAppSession()
        {
            try
            {
                return Process.GetCurrentProcess().Parent().MainModule.ToString().Contains("rdpinit");
            }
            catch
            {
            }

            return false;
        }

        public static bool isInTerminalServerSessionOverride()
        {
            // Override check
            if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "rdp.ini"))
            {
                return true;
            }
            return false;
        }

        public static bool isInTerminalServerSession()
        {
            

            // If Current Session = Console Session return false;
            if (ProcessClass.GetConsoleSessionId().ToString() == Process.GetCurrentProcess().SessionId.ToString())
            {
                return false;
            }

            // If Current Session 
            if (!ProcessClass.isTerminalServerInstalled())
            {
                return false;
            }
            if (isInCitrixSession()) { return true; }
 			
 			if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "rdp.ini"))
            {
                return true;
            }

            return System.Windows.Forms.SystemInformation.TerminalServerSession;
        }

        public static bool isSimulatorInstalled()
        {
            if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + Programs.Simulator))
            {
                return true;
            }
            else return false;
        }

        private static void CreatePathMissing(string path)
        {
            try
            {
                path = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(path))
                {
                    bool folderExists = Directory.Exists(path);
                    if (!folderExists)
                        Directory.CreateDirectory(path);
                }
            }
            catch
            {
            }
        }

        public static bool isTerminalServerInstalled()
        {
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Terminal Server");
            if (myKey != null)
            {
                if (myKey.GetValue("TSAppCompat", "0").ToString() == "1")
                {
                    return true;
                }
            }
            return false;
        }

        public static bool RunDiagnosticApp()
        {
            string filePath = Programs.Diagnosis;
            if (!findAppPath(ref filePath))
            {
                return true;
            }
            else
            {
                var psi = new ProcessStartInfo(filePath);
                psi.UseShellExecute = true;
                //if (!IsAnAdministrator())
                //{
                //    psi.Verb = "runas";
                //}
                Process.Start(psi);
            }
            return true;
        }
        public static bool RunFeedbackApp(string attachment)
        {
            string filePath;
            filePath = Programs.Feedback;
            if (!findAppPath(ref filePath))
            {
                return true;
            }
            else
            {

                ProcessStartInfo psi = new ProcessStartInfo(filePath);
                psi.Arguments = attachment;
                psi.LoadUserProfile = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.UseShellExecute = false;
                Process.Start(psi);
            }
            return true;
        }
        public static bool RunHicapsStandaloneService()
        {
            string filePath;
            filePath = Programs.Service;
            if (!findAppPath(ref filePath))
            {
                return true;
            }
            else
            {
                ProcessStartInfo p1 = new ProcessStartInfo(filePath, "/Standalone");
                p1.RedirectStandardOutput = true;
                p1.RedirectStandardInput = true;
                p1.UseShellExecute = false;
                p1.CreateNoWindow = true;
                p1.WindowStyle = ProcessWindowStyle.Hidden;
                System.Diagnostics.Process.Start(p1);
            }
            return true;
        }

 

        public static bool setUserConfigServiceServiceFileSetting(string setting, string defaultSettingValue)
        {
            AppConfig myConfigEdit = new AppConfig(getServiceConfigPath() + Path.DirectorySeparatorChar + "userSettings.xml", true);
            return myConfigEdit.SetValue(setting, defaultSettingValue);
        }

        public static bool setUserConfigUserServiceFileSetting(string setting, string defaultSettingValue)
        {
            string configPath = getUserServiceConfigFile();
            AppConfig myConfigEdit = new AppConfig(configPath, true);
            return myConfigEdit.SetValue(setting, defaultSettingValue);
        }

        public static void WriteDefaultUserXmlConfig(string startPath)
        {
            try
            {
                CreatePathMissing(startPath);
                File.WriteAllText(startPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration>  <appSettings>    <add key=\"DefaultTerminal\" value=\"\" />    <add key=\"StatusBox\" value=\"true\" />  </appSettings></configuration>");
            }
            catch (Exception) { }
        }

        public static void WriteDefaultXmlConfig(string startPath)
        {
            try
            {
                CreatePathMissing(startPath);
                File.WriteAllText(startPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration>  <appSettings>    <add key=\"LoggingEnabled\" value=\"true\" />    <add key=\"LogName\" value=\"HICAPS Connect\" />    <add key=\"NoNetwork\" value=\"false\" />    <add key=\"Configured\" value=\"\" />    <add key=\"ServerIp\" value=\"\" />    <add key=\"ServerPort\" value=\"11000\" />    <add key=\"FoundTerminals\" value=\"\" />    <add key=\"TerminalCommPorts\" value = \"\" />  </appSettings>  <runtime>    <legacyUnhandledExceptionPolicy enabled=\"1\"/>  </runtime></configuration>");
            }
            catch (Exception) { }
        }
        // Private Methods (4) 

        private static string getAssemblyServiceConfigFile()
        {
            // Crazy I know but reset back to original config location for loading.
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "HicapsConnectService.exe.config";
        }

        private static string getAssemblyServiceConfigFileAlt()
        {
            // Crazy I know but reset back to original config location for loading.
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "HicapsConnectService.config";
        }

        public static string getUrl()
        {
            if (File.Exists(Assembly.GetExecutingAssembly().Location + Path.DirectorySeparatorChar + "SandBox.ini"))
            {
                return "http://www.hicapsconnect.com.au/sandbox/";
            }
            else
            {
                return "http://www.hicapsconnect.com.au/";
            }
        }

        /// <summary>
        /// To check for Citrix, use the Session Name enviconment variable
        /// </summary>
        /// <returns></returns>
        public static bool isInCitrixSession()
        {
            switch ((Environment.GetEnvironmentVariable("SessionName") ?? "CON").ToUpper().Substring(0, 3))
            {
                case "ICA": return true;
                default: return false; // false for RDP (Terminal Services) and CON (Console)
            }
        }

        private static bool isProcessRunning(string ProcessName, int SessionId)
        {
            Process[] processlist = Process.GetProcesses();
            foreach (Process myProcess in processlist)
            {
                if (myProcess.ProcessName == ProcessName)
                {
                    if (SessionId == -1) { return true; }
                    if (SessionId == myProcess.SessionId) { return true; }
                }
            }
            return false;
        }
        // Internal Methods (1) 

        internal static bool findAppPath(ref string appFileName)
        {
            string tAppFileName = appFileName;
            string filePath = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + Path.DirectorySeparatorChar + appFileName;
            if (!File.Exists(filePath))
            {
                filePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + Path.DirectorySeparatorChar + "HicapsConnect " + appFileName;
            }
            if (!File.Exists(filePath))
            {
                filePath = @"c:\Program Files\HicapsConnect\" + appFileName;
            }
            if (!File.Exists(filePath))
            {
                filePath = @"c:\Program Files (x86)\HicapsConnect\" + appFileName;
            }
            if (!File.Exists(filePath))
            {
                filePath = appFileName;
            }
            if (!File.Exists(filePath))
            {
                // Search through Environment Path.
                filePath = Environment.GetEnvironmentVariable("PATH")
                .Split(';')
                .Where(s => File.Exists(Path.Combine(s, tAppFileName)))
                .FirstOrDefault();
            }
            if (string.IsNullOrEmpty(filePath))
            {
                // no idea, return same file.
                return false;
            }

            appFileName = filePath;
            return true;
        }

        #endregion Methods

        /* Get Active Console ID */


        #region Kernal32
        [DllImport("kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();
        public static uint GetConsoleSessionId()
        {
            uint result;
            try
            {
                result = WTSGetActiveConsoleSessionId();
                if (result == 0xFFFFFFFF)
                    result = 0;
            }
            catch { result = 0; }

            return result;

        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);
        public static bool Is64BitWindows()
        {
            if (IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool Is32BitProcessOn64BitProcessor()
        {
            bool retVal;

            IsWow64Process(Process.GetCurrentProcess().Handle, out retVal);

            return retVal;
        }
        
     private static string FindIndexedProcessName(int pid) {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexdName = null;

            for (var index = 0; index < processesByName.Length; index++) {
                processIndexdName = index == 0 ? processName : processName + "#" + index;
                var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);
                if ((int) processId.NextValue() == pid) {
                    return processIndexdName;
                }
            }

            return processIndexdName;
        }

    private static Process FindPidFromIndexedProcessName(string indexedProcessName) {
        var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
        return Process.GetProcessById((int) parentId.NextValue());
    }

    public static Process Parent(this Process process) {
        return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
    }

        #endregion

        #region Logging Methods
        public static void LogException(string message, string programName)
        {

            string headerString = "HICAPS Connect " + programName;

            string path = getServiceConfigPath();
            string sessionString = Process.GetCurrentProcess().SessionId.ToString();

            if (path.Contains(Path.DirectorySeparatorChar)) { path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar) + 1); }
            String logfile = String.Format("{0}{1:dd_MMM_yyyy}ExceptionLog[{2}].txt", path, System.DateTime.Now, sessionString);
            headerString = String.Format("{0} {1:dd MMM yyyy H:mm:ss:fff} Version {2}", headerString, System.DateTime.Now, Assembly.GetExecutingAssembly().GetName().Version.ToString());
            headerString = String.Format("\r\n{1}\r\n{0}\r\n{1}", headerString, new String('-', headerString.Length));
            if (message != null && message.Length > 0)
            {
                if (message.Contains(@"<?xml version") || message.StartsWith("[?"))
                {
                    if (!message.StartsWith("[?")) { message = message.Substring(0, message.LastIndexOf('>') + 1); }

                    headerString = String.Format("{0}\r\n{1}\r\n", headerString, message.Trim());
                }
                else
                {
                    headerString = String.Format("{0}\r\n{1}\r\n", headerString, WrapString(message.Trim(), 40));
                }
            }
            else
            {
                headerString += "\r\n";
            }
            try
            {
                File.AppendAllText(logfile, headerString);
            }
            catch (Exception) { }

        }
        private static string WrapString(string text, int maxLength)
        {
            string returnString = "";

            foreach (string inputString in Wrap(text, maxLength))
            {
                string tmp = inputString.Replace("\r", "");
                tmp = tmp.Replace("\n", "");
                returnString += String.Format("{0}\r\n", tmp.Trim());
            }

            return returnString;
        }
        public static void CheckAndRunController()
        {
            CheckAndRunController(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
        }
        public static void CheckAndRunController(string startUpPath)
        {
            if (!isServiceControllerRunning() && !isHicapsStandaloneRunning())
            {
                try
                {
                    string startupApp = Programs.Controller;

                    if (!findAppPath(ref startupApp))
                    {
                        // Couldn't find Service Controller App.
                        return;
                    }
                    ProcessStartInfo p1 = new ProcessStartInfo(startupApp, "");
                    p1.RedirectStandardOutput = true;
                    p1.RedirectStandardInput = true;
                    p1.UseShellExecute = false;
                    p1.CreateNoWindow = true;
                    p1.WindowStyle = ProcessWindowStyle.Hidden;
                    System.Diagnostics.Process.Start(p1);
                }
                catch (Exception ) { }
            }
        }
        public static bool isServiceControllerRunning()
        {
            return isProcessRunning("HicapsConnectServiceController", (int)GetConsoleSessionId());
        }
        public static bool isHicapsStandaloneRunning()
        {
            return isProcessRunning("HicapsConnectService", Process.GetCurrentProcess().SessionId);
        }
        private static List<String> Wrap(string text, int maxLength)
        {

            // Return empty list of strings if the text was empty
            if (text.Length == 0) return new List<string>();
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var currentWord in words)
            {

                if ((currentLine.Length > maxLength) ||
                    ((currentLine.Length + currentWord.Length) > maxLength))
                {
                    lines.Add(currentLine);
                    currentLine = "";
                }

                if (currentLine.Length > 0)
                    currentLine += " " + currentWord;
                else
                    currentLine += currentWord;

            }

            if (currentLine.Length > 0)
                lines.Add(currentLine);


            return lines;
        }
        #endregion

        #region Service Process Code
        public static void CheckAndRestartHICAPSConnectService()
        {
            CheckAndStopHICAPSConnectService();
            CheckAndStartHICAPSConnectService();
        }
        public static void CheckAndStartHICAPSConnectService()
        {
            try
            {
                using (ServiceController myController = new ServiceController(Services.MainService))
                {
                    if (myController.Status != ServiceControllerStatus.Running)
                        try
                        {
                            myController.Start();
                            myController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));

                        }
                        catch
                        {
                        }
                }
            }
            catch (Exception) { }
        }
        public static void CheckAndStopHICAPSConnectService()
        {
            try
            {
                using (ServiceController myController = new ServiceController(Services.MainService))
                {
                    if (myController.Status != ServiceControllerStatus.Stopped)
                        try
                        {
                            myController.Stop();
                            myController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
                        }
                        catch
                        {
                        }
                }
            }
            catch (Exception) { }
        }
        #endregion
        #region Service Commands
        /// <summary>
        /// Reset the service.... This should only really come from the Service Controller.
        /// Service will reload its configuration, then rescan for Terminals
        /// </summary>
        public static void ServiceReset(bool silent = true)
        {         
            serviceExecuteCommand(131, silent);
        }
        public static void ServiceRescan(bool silent = false)
        {
            serviceExecuteCommand(130, silent);
        }

        public static void ServiceResetAgent(bool silent)
        {
            serviceAgentExecuteCommand(131, silent);
        }

      
        private static void serviceExecuteCommand(int number, bool silent)
        {
            ServiceController mainService;

            if (isInTerminalServerSession() )
            {
                return;
            }
            if (!isHicapsConnectServiceInstalled())
            {
                return;
            }
            try
            {
               mainService = new System.ServiceProcess.ServiceController(Services.MainService);
            }
            catch (Exception)
            {
                return;
            }
            try
            {
                mainService.Refresh();

                if (mainService.Status != System.ServiceProcess.ServiceControllerStatus.Running)
                {
                    // if (IsAnAdministrator())
                    //{
                    StopStartService(true);
                    // }
                }

                if (mainService.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                {
                    mainService.ExecuteCommand(number);
                    //waitConfigurationScreen();
                }
                else
                {
                    //ShowServiceNotRunningBox(silent);
                }
            }
            catch (Exception)
            {
                return;
            }

        }
        private static void serviceAgentExecuteCommand(int number, bool silent)
        {

            try
            {
                using (var agentService = new ServiceController(Services.AgentService))
                {
                    if (agentService.Status == ServiceControllerStatus.Running)
                        agentService.ExecuteCommand(number);
                }
            }
            catch (Exception) { return; }

        }
        public static bool isHicapsConnectServiceInstalled()
        {
            try
            {
                using (ServiceController myController = new ServiceController(Services.MainService))
                {
                    try
                    {
                        if (myController.Status == ServiceControllerStatus.Running)
                            return true;
                    }
                    catch (InvalidOperationException)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;

        }
        internal static void StopStartService(bool silent)
        {
            if (!isHicapsConnectServiceInstalled())
            {
                return;
            }

            StopService(silent);
            StartService(true);
        }

        private static void StartService(bool silent)
        {
            if (isInTerminalServerSession())
            {
                return;
            }
            if (!isHicapsConnectServiceInstalled())
            {
                return;
            }
          
            serviceAgentExecuteCommand(133, silent);
            Thread.Sleep(5000);
            return;
        }

        private static void StopService(bool silent)
        {
            if (isInTerminalServerSession())
            {
                return;
            }
            if (!isHicapsConnectServiceInstalled())
            {
                return;
            }

            // If you are not an administrator don't stop service.
            // Send command to service to start ip listeners.

            serviceAgentExecuteCommand(132, silent);
            Thread.Sleep(3000);
            return;
        }

        #endregion
        public static bool RunClientApp()
        {
            string filePath = Programs.Client;
            if (!findAppPath(ref filePath))
            {
                return true;
            }
            else
            {
                var psi = new ProcessStartInfo(filePath);
                psi.UseShellExecute = true;
              
                Process.Start(psi);
            }
            return true;
        }
    }
}
