using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HICAPSConnectLibrary.Protocol;
using HICAPSConnectLibrary.Utils;

namespace HICAPSConnectLibrary.Server
{
    public class UDPServer
    {
        //Network Settings
    //    private int _udpPort = 11000;
    //    private int _tcpPort = 11005;
        private HParams _hParams = new HParams();
        private bool _isRunning = false;
        private readonly ILogging _udpLogging = new Logging();
        
        ///// <summary>
        ///// ctore
        ///// </summary>
        ///// <param name="udpPort">port to listen on</param>
        ///// <param name="networkName">network name to check</param>
        //public UDPServer(int udpPort, string networkName)
        //{
        //    _udpPort = udpPort;
        //    SetNetworkName(networkName);
        //    SetTCPPort(_tcpPort);
        //}
        /// <summary>
        /// ctore
        /// </summary>
        /// <param name="hParams">Terminal Params class</param>

        public UDPServer(HParams hParams)
        {
            _hParams = hParams;
        }

        public void SetParams(HParams hParams)
        {
            _hParams = hParams;
            _hParams.IPAddress = NetworkClass.getComputerIpAddress();
        }
        public void SetNetworkName(string networkName)
        {
            _hParams.NetworkName = networkName;
        }
        public void SetTCPPort(int tcpPort)
        {
            _hParams.IPPort = tcpPort.ToString();
            _hParams.IPAddress = NetworkClass.getComputerIpAddress();
        }
        /// <summary>
        /// Start up the service
        /// </summary>
        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(UDPListener));
            }
        }

        /// <summary>
        /// Stop the service if running
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
        }

        #region UDP Part
        void UDPListener(object state)
        {
            //TODO make UDP port paramater from command line

            Socket remoteHosts;
            IPEndPoint iep;
            EndPoint ep;
            List<string> remoteIpList = new List<string>();
            try
            {
                remoteHosts = new Socket(AddressFamily.InterNetwork,
                          SocketType.Dgram, ProtocolType.Udp);
                remoteHosts.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                iep = new IPEndPoint(IPAddress.Any, _hParams.UDPPort.ToInt());
                ep = (EndPoint)iep;
                remoteHosts.Bind(iep);
                remoteHosts.ReceiveTimeout = 5000;
            }
            catch { return; }
            while (_isRunning)
            {
                string remoteIp = "";
                byte[] data = new byte[1024];
                int recv = 0;
                try
                {
                    recv = remoteHosts.ReceiveFrom(data, ref ep);
                    if (ep.AddressFamily == AddressFamily.InterNetwork)
                    {
                        remoteIp = ep.ToString();
                    }
                    else
                    {
                        remoteIp = "";
                    }

                }
                catch
                {
                    recv = 0;
                    Thread.Sleep(1000);
                    remoteIpList.Clear();
                }
                // Code to stop double/triple udp packets.
                // after 1 second the list will be wiped. 
                if (recv > 0 && remoteIpList.Contains(remoteIp))
                {
                    recv = 0;
                }
                if (recv > 0)
                {
                    remoteIpList.Add(remoteIp);
                    string stringData = Encoding.Default.GetString(data, 0, recv);

                    Console.WriteLine("Revieved Message from " + remoteIp);
                    Array.Resize(ref data, recv);
                    try
                    {
                        // Decode the message.
                        var requestMessage = new HMessage(data);
                        _udpLogging.LogData(data, "UDP Packet :- " + recv + "bytes");
                        string ip = requestMessage.FieldValues["IA"];
                        string port = requestMessage.FieldValues["IP"];

                        if (!requestMessage.isPartOfNetwork(_hParams.NetworkName))
                        {
                            Debug.WriteLine("Received Find IP Terminal Message, Ignoring Network Name different");
                        }
                        else
                        {
                            Debug.WriteLine("Received Find IP Terminal Message, Connecting back on TCP IP-" + ip + ":" +
                                              port);

                            try
                            {
                                var ClientIP = IPAddress.Parse(ip);
                                int ClientIPPort;
                                if (!int.TryParse(port, out ClientIPPort)) { throw new ArgumentOutOfRangeException(); }
                                //var terminalResponse = new HParams();
                                //terminalResponse.IPAddress = NetworkClass.getComputerIpAddress();
                                //terminalResponse.IPPort = _tcpPort.ToString();
                                var hMessage = HTerminal.getTerminalIPTestResponse(_hParams);
                                byte[] packetData = hMessage.SaveToByteArray();

                                _udpLogging.LogData(packetData, "Sending Find Terminal Test");
                                Debug.WriteLine("Waiting for Message...... Hit the ESC key to Exit");
                                try
                                {
                                    using (var client = new TcpClient(ip, ClientIPPort))
                                    {

                                        using (NetworkStream stream = client.GetStream())
                                        {
                                            stream.Write(packetData, 0, packetData.Length);
                                            stream.Close();
                                        }


                                        client.Close();

                                    }
                                }
                                catch (ArgumentNullException e)
                                {
                                    Debug.WriteLine(string.Format("ArgumentNullException: {0}", e));
                                }
                                catch (SocketException e)
                                {
                                    Debug.WriteLine(string.Format("SocketException: {0}", e));
                                }


                            }
                            catch (Exception )
                            {
                                Debug.WriteLine("Error, Invalid IP and port combination" + ip + ":" +
                                             port);
                            }



                        }
                    }
                    catch (Exception )
                    {
                        _udpLogging.LogData(data, "Bad/Invalid HC UDP Packet :- " + recv + "bytes");
                    }
                }
            }
        }
        #endregion


     
    }
}
