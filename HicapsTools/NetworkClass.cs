using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace HicapsTools
{
    public class NetworkClass
    {
        #region For support of ReverseIPLookup function

        [DllImport("Dnsapi.dll", EntryPoint = "DnsQuery_W", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        static extern Int32 DnsQuery(String lpstrName, Int16 wType, Int32 options, IntPtr pExtra, ref IntPtr ppQueryResultsSet, IntPtr pReserved);

        [DllImport("Dnsapi.dll", SetLastError = true)]
        static extern void DnsRecordListFree(IntPtr pRecordList, Int32 freeType);

        [StructLayout(LayoutKind.Sequential)]
        struct DnsRecordPtr
        {
            public IntPtr pNext;
            public String pName;
            public Int16 wType;
            public Int16 wDataLength;
            public Int32 flags;
            public Int32 dwTtl;
            public Int32 dwReserved;
            public IntPtr pNameHost;
        }

        static String DnsGetPtrRecord(String domain)
        {
            const Int16 DNS_TYPE_PTR = 0x000C;
            const Int32 DNS_QUERY_STANDARD = 0x00000000;
            const Int32 DNS_ERROR_RCODE_NAME_ERROR = 9003;

            IntPtr queryResultSet = IntPtr.Zero;

            try
            {
                int dnsStatus = DnsQuery(domain, DNS_TYPE_PTR, DNS_QUERY_STANDARD, IntPtr.Zero, ref queryResultSet, IntPtr.Zero);

                if (dnsStatus == DNS_ERROR_RCODE_NAME_ERROR)
                    return null;
                if (dnsStatus != 0)
                    throw new Win32Exception(dnsStatus);

                DnsRecordPtr dnsRecordPtr;

                for (var pointer = queryResultSet; pointer != IntPtr.Zero; pointer = dnsRecordPtr.pNext)
                {
                    dnsRecordPtr = (DnsRecordPtr)Marshal.PtrToStructure(pointer, typeof(DnsRecordPtr));

                    if (dnsRecordPtr.wType == DNS_TYPE_PTR)
                        return Marshal.PtrToStringUni(dnsRecordPtr.pNameHost);
                }

                return null;
            }
            finally
            {
                const Int32 DnsFreeRecordList = 1;

                if (queryResultSet != IntPtr.Zero)
                    DnsRecordListFree(queryResultSet, DnsFreeRecordList);
            }
        }

        #endregion

#region Public members...
//Method...
        /// <summary>
        /// Added to speed up the loading of the treeview in the Diagnosis screen. 
        /// 
        /// This is very much faster than using GetHostEntry and doesn't cause the screen to freeze.
        /// 
        /// http://stackoverflow.com/questions/997046/gethostentry-is-very-slow
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static String ReverseIPLookup(IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("IP address is not IPv4.", "ipAddress");

            var domain = String.Join(".", ipAddress.GetAddressBytes().Reverse().Select(b => b.ToString())) + ".in-addr.arpa";

            return DnsGetPtrRecord(domain);
        }
        public static string[] getActiveIps()
        {
            // Query for all the enabled network adapters 
            List<string> ipList = new List<string>();
            try
            {
                UInt16 connected = 0;
                ManagementObjectSearcher objSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'");
                ManagementObjectCollection objCollection = objSearcher.Get();

                // Loop through all available network interfaces

                foreach (ManagementObject obj in objCollection)
                {

                    // List all IP addresses of the current network interface

                    string[] AddressList = (string[])obj["IPAddress"];
                    UInt32 IndexList = (UInt32)obj["Index"];
                    bool ipConnected = false;

                    // Some disconnected nic cards, still have ip's set to them, so we need to exclude those from our list.
                    ManagementObjectSearcher objNicSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE DeviceID = '" + IndexList.ToString() + "'");
                    ManagementObjectCollection objNicCollection = objNicSearcher.Get();
                    foreach (ManagementObject nicObj in objNicCollection)
                    {
                        connected = 0;
                        try
                        {
                            connected = (UInt16)nicObj["NetConnectionStatus"];
                        }
                        catch (Exception) { connected = 0; }
                        //a status of 2 or 9 means "connected" so we only get the nics that are online.
                        if (connected == 2 || connected == 9)
                        {
                            ipConnected = true;
                        }
                    }


                    // First thing to do is check that the nic card is actually connected.
                    if (ipConnected)
                    {
                        string logMessage;
                        string ipToUse = "";

                        logMessage = String.Format("Found {0} Nics ", objCollection.Count);
                        foreach (string Address in AddressList)
                        {
                            if (ipToUse.Trim() == "") { ipToUse = Address; }
                            if ((Address != "0.0.0.0") && (!Address.Contains("::")))
                            {
                                ipList.Add(Address);
                            }
                            logMessage = String.Format("{0}[{1}];", logMessage, Address);
                        }
                    }
                    // Log(logMessage);

                }
                //if (ipList.Count == 0)
                //{
                //    ipList.Add(STR_LocalHostIP);
                //}
            }
            catch { }
            return ipList.ToArray();
        }
        public static string getComputerIpAddress()
        {
            try
            {
                string hostName = Environment.MachineName;
                IPHostEntry local = Dns.GetHostEntry(hostName);
                foreach (IPAddress ipaddress in local.AddressList)
                {
                    if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ipaddress.ToString();
                    }
                }
            }
            catch (Exception) { }
            return "";
        }
        public static string getHostNameFromIp(string ipNumber)
        {
            try
            {
                var hostname = ReverseIPLookup(IPAddress.Parse(ipNumber));
                if (string.IsNullOrWhiteSpace(hostname))
                {
                    hostname = ipNumber;
                }
                return hostname;
            } catch (Exception )
            {
                return ipNumber;
            }
           
        }
        public static bool isLocalIp(string localIp)
            {
                String ipAddress = getComputerIpAddress();

                return localIp.Equals(ipAddress);
                //////string[] ips = getActiveIps();
                //////foreach (string ip in ips)
                //////{
                //////    if (localIp.Equals(ip))
                //////    {
                //////        return true;
                //////    }
                //////}
                //////return false;
            }
        public static bool isNetworkAvailable()
        {
            try
            {
                string[] ips = getActiveIps();
                if (ips.Length > 0)
                {
                    return true;
                }
            }
            catch (Exception)
            { }
            return false;
        }
        public static bool isTcpPortListening(string ipAddress, int port)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            IPEndPoint[] endPoints = ipProperties.GetActiveTcpListeners();
            foreach (var localPort in endPoints)
            {
                if ((localPort.Port == port)&&(localPort.Address.ToString() == ipAddress || localPort.Address.ToString() == "0.0.0.0") )
                {
                    return true; 
                }
            }

            return false;
            //TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();
            //foreach (TcpConnectionInformation info in tcpConnections)
            //{
            //    if (info.LocalEndPoint.Port == port)
            //    {
            //        return true;
            //    }
            //}
            //return false;
        }
        public static bool isUdpPortListening(string ipAddress, int port)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
           
            IPEndPoint[] endPoints = ipProperties.GetActiveUdpListeners();
            foreach (var localPort in endPoints)
            {
                if ((localPort.Port == port) && (localPort.Address.ToString() == ipAddress))
                {
                    return true;
                }
            }
       
            return false;
        }

        public static string DownloadString(string url, int timeout = 30000)
        {
            try
            {
                return new WebClientTimeout { Timeout = timeout}.DownloadString(url);
            }
            catch
            {
                return string.Empty;
            }

        }
#endregion
    }
}
