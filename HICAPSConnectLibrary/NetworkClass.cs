using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Net.NetworkInformation;
using System.Net;

namespace HICAPSConnectLibrary
{
    public class NetworkClass
    {
#region Public members...
//Method...


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
                //Get the name of the IP address
                string entry = Dns.GetHostEntry(IPAddress.Parse(ipNumber)).HostName.ToString();
                if (entry.IndexOf('.') > 0)
                {
                    entry = entry.Substring(0, entry.IndexOf('.'));
                }
                return entry;
            }
            catch (Exception)
            {
                return ipNumber;
            }
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
            //UdpConnectionInformation[] tcpConnections = ipProperties.
            //foreach (TcpConnectionInformation info in tcpConnections)
            //{
            //    if (info.LocalEndPoint.Port == port)
            //    {
            //        return true;
            //    }
            //}
            return false;
        }
        public static string DownloadString(string url, int timeout = 30000)
        {
            try
            {
                return new WebClientTimeout { Timeout = timeout }.DownloadString(url);
            }
            catch
            {
                return string.Empty;
            }

        }
        #endregion
    }
}
