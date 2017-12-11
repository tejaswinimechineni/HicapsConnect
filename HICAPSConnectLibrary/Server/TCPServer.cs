using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HICAPSConnectLibrary.Protocol;

namespace HICAPSConnectLibrary.Server
{
    public class TCPServer
    {
        //Network Settings
        #region TCP Server settings
        private int _tcpPort = 11000; ////// = 11005;
        private string _networkName = "";
        private HParams _hParams = new HParams();
        private bool _isRunning = false;
        private bool _isStarted = false;

        private TcpListener tcpListener;


        // private IPAddress tcpServerIP;
        private int socketCount = 0;
        // private Thread t; // TCP Server Thread

        public delegate void MessageHandler(byte[] message, HConnection clientConnection);

        public event MessageHandler OnReceivedMessage;

        #endregion

        ///// <summary>
        ///// ctor
        ///// </summary>
        ///// <param name="tcpPort"></param>
        ///// <param name="networkName"></param>
        public TCPServer(int tcpPort, string networkName)
        {
            _tcpPort = tcpPort;
            _networkName = networkName;
        }
        public TCPServer(ref int tcpPort, ref string networkName)
        {
            _tcpPort = tcpPort;
            _networkName = networkName;
        }

        public TCPServer(HParams hParams)
        {
            _hParams = hParams;
            _tcpPort = _hParams.IPPort.ToInt();
        }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(TCPListener));
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public bool IsRunning()
        {
            return _isRunning;
        }
        public bool IsStarted()
        {
            return _isStarted;
        }

        public int GetIPPort()
        {
            return _tcpPort;
        }
        private void TCPListener(object blah)
        {
            bool addPort;
            do
            {
                addPort = false;
                try
                {
                    tcpListener = new TcpListener(IPAddress.Any, _tcpPort);
                    tcpListener.Start();
                }
                catch (Exception e)
                {
                    _tcpPort++;
                    addPort = true;
                }
            } while (addPort && _tcpPort < 15000);
            _isStarted = true;
            while (_isRunning)
            {
               
                //blocks until a client has connected to the server
                TcpClient client = tcpListener.AcceptTcpClient();
                var myClient = new HConnection();
                myClient.tcpClient = client;

                ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClientComm), myClient);
            }
        }
        private void HandleClientComm(object clientConnection)
        {
            // Protocol Variables.
            byte[] STXBuffer = new byte[1];
            byte[] LENBuffer = new byte[2];
            byte[] MSGBuffer;
            byte[] EOTBuffer = new byte[3];
            int MessageLength = 0;
            //   Byte[] outgoingBuffer;
            List<byte> messageArray = new List<byte>();
            TcpClient sTcp;
            NetworkStream clientStream;

            socketCount++;
            try
            {
                int bytesRead;

                sTcp = ((HConnection)clientConnection).tcpClient;
                ((HConnection)clientConnection).SocketCount = socketCount;
                //while (())
                //{
                try
                {
                    if (_isRunning)
                    {

                        clientStream = sTcp.GetStream();

                        // Reset variables for new message;
                        messageArray = new List<byte>();
                        MessageLength = 0;
                        STXBuffer = new byte[1];
                        LENBuffer = new byte[2];
                        EOTBuffer = new byte[3];
                        MSGBuffer = new byte[1]; // This is not really required.
                    }
                    else
                    {
                        // Exit out of while loop
                        throw new Exception("Server is stopping");
                    }
                    // Look for STX
                    do
                    {
                        bytesRead = clientStream.Read(STXBuffer, 0, 1);
                    } while (bytesRead > 0 && STXBuffer[0] != 2);

                    if (bytesRead > 0)
                    {
                        messageArray.AddRange(STXBuffer);
                    }
                    // Get Length of message
                    bytesRead = clientStream.Read(LENBuffer, 0, 2);

                    if (bytesRead != 2)
                    {
                        throw new IndexOutOfRangeException("Invalid ECR Message");
                    }
                    //TODO Convert LENBuffer into int MessageLength
                    MessageLength = DoubleBcdToInt(LENBuffer);
                    messageArray.AddRange(LENBuffer);

                    MSGBuffer = new byte[MessageLength];
                    bytesRead = clientStream.Read(MSGBuffer, 0, MessageLength);
                    messageArray.AddRange(MSGBuffer);
                    // Now get EOT 2 charachers
                    bytesRead = clientStream.Read(EOTBuffer, 0, 2);
                    messageArray.AddRange(EOTBuffer);

                    // Fnal sendBack
                    OnReceivedMessage(messageArray.ToArray(), ((HConnection)clientConnection));

                }
                catch (Exception ex)
                {
                }

                sTcp.Close();

            }
            catch { }
            finally
            {
                // Log(String.Format("Disconnected {0}\r\n", socketCount));
                socketCount--;
            }
        }
        /// <summary>
        /// Converts a BCD value to an INT.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private int DoubleBcdToInt(byte[] size)
        {
            //            int value;
            int value = System.Convert.ToInt32(size[1].ToString("X2"));
            int value100 = System.Convert.ToInt32(size[0].ToString("X2")) * 100;
            return value + value100;
        }

    }
}
