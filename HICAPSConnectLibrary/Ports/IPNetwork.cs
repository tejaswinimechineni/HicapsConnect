using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using HICAPSConnectLibrary.Utils;

namespace HICAPSConnectLibrary.Ports
{
    public class IPNetwork : ICommPort
    {
        #region ICommPort Members
        NetworkStream clientStream;
        TcpClient sTcp;
        Thread _readThread;
        volatile bool _keepReading;
        private bool disposed = false;
        private bool _logData = false;
        private bool _sendMessageAckCheck = false;
        private bool _checkAck = false;
        private bool _disableAcks = false;
        private DateTime _ackTime;
        public delegate void EventHandler(string param, byte[] byteParam);
        public delegate void StatusChangedEventHandler(string param);

        public bool IsAckReceived()
        {
            return !_checkAck;
        }
        public bool IsAckSending()
        {
            return false; // not needed.
        }
        #region TCPIP Parts
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private int serverConnectTimeout = 3000;
        // this function gets called when an connection event occurs... possible events: client connected, connection refused etc. ...but it doesn't have a connection timeout...it waits as long as necessary to get anykind of reply
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                TcpClient client = (TcpClient)ar.AsyncState;
                client.EndConnect(ar); // Complete the connection.
                // Console.WriteLine("TcpClient connected to {0}", client.Client.RemoteEndPoint.ToString());
                connectDone.Set(); // trigger the connectDone event 
            }
            catch (Exception)
            {
                // Console.WriteLine(e.ToString());
            }
        }
        #endregion

        public void Close()
        {
            _keepReading = false;
            Thread.Sleep(100);
            sTcp.GetStream().Close(50);
            sTcp.Close();

        }

        public event CommPort.EventHandler DataReceived;
        public void OnDataReceived(string param, byte[] byteParam)
        {
            if (DataReceived != null)
            {
                // Call the Event
                DataReceived(param, byteParam);
            }
        }

        public void Dispose()
        {

        }

        public bool IsOpen
        {
            get { return sTcp.Connected; }
        }

        public void Open(string ipNumber, int port)
        {
            try
            {
                if (ipNumber.Contains("."))
                {
                    // TCPIP Mode.
                    // portName = IP Address
                    // baudRate = IP Port.
                    sTcp = new TcpClient();
                    sTcp.BeginConnect(ipNumber, port, new AsyncCallback(ConnectCallback), sTcp); // here the clients starts to connect asynchronously (because it is asynchronous it doesn't have a timeout only events are triggered)
                    // immediately after calling beginconnect the control is returned to the method, now we need to wait for the event or connection timeout
                    connectDone.WaitOne(serverConnectTimeout, false); // now application waits here for either an connection event (fi. connection accepted) or the timeout specified by connectionTimeout (in milliseconds)
                    Thread.Sleep(100);
                    if (!sTcp.Connected) { throw new UnauthorizedAccessException("Specified Port is unavailable"); } /*10061 = Could not connect to server */
                    clientStream = sTcp.GetStream();
                    StartReading();
                }
                else
                {
                    throw new InvalidOperationException("Port parameter must be a IP Number");
                }
            }
            catch (IOException ioex)
            {
                OnStatusChanged(String.Format("{0} does not exist", port.ToString()));
                logData(null, ioex.ToString());
            }
            catch (UnauthorizedAccessException uaeEx)
            {
                OnStatusChanged(String.Format("{0} already in use", port.ToString()));
                logData(null, uaeEx.ToString());
            }
            catch (Exception ex)
            {
                OnStatusChanged(String.Format("{0}", ex.ToString()));
                logData(null, ex.ToString());
            }
        }

        public void Open(string portName, int baudRate, System.IO.Ports.Parity portParity, int portDataBits, System.IO.Ports.StopBits portStopBits, System.IO.Ports.Handshake portHandshake, int timeOut, Encoding dataEncoding)
        {
            Open(portName, baudRate);
        }

        public void Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void SendAck()
        {
            throw new NotImplementedException();
        }

        public void SendAsync(byte[] dataArray, bool skipAckCheck)
        {
            logData(dataArray, "Message Sent");
            clientStream.Write(dataArray, 0, dataArray.Length);
            _sendMessageAckCheck = true;
            //clientStream.Flush();
        }

        public void setLogging(bool truefalse)
        {
            _logData = truefalse;
        }

        private void StartReading()
        {

            if (!_keepReading)
            {
                logData(null, String.Format("Starting Read Thread "));
                _keepReading = true;
                _readThread = new Thread(ReadPort);
                _readThread.Start();
            }
            //TimeSpan waitTime = new TimeSpan(0, 0, 0, 0, 50);
            //Thread.Sleep(waitTime);
        }

        private void StopReading()
        {

            if (_keepReading)
            {
                logData(null, String.Format("Stopping Read Thread "));
                _keepReading = false;
                _readThread.Join();	//block until exits
                _readThread = null;
            }
        }

        ///
        /// <summary> Get the data and pass it on. </summary>
        /// 
        private void ReadPort()
        {
            // Now do continous loop for bytes
            // TcpClient sTcp;
            byte[] STXBuffer = new byte[1];
            byte[] LENBuffer = new byte[2];
            byte[] MSGBuffer;
            byte[] EOTBuffer = new byte[3];
            int MessageLength = 0;
            Byte[] outgoingBuffer;
            List<byte> messageArray = new List<byte>();

            int bytesRead;
            while ((_keepReading))
            {
                //try
                //{
                if (_keepReading)
                {
                    // clientStream = sTcp.GetStream();

                    // Reset variables for new message;
                    messageArray = new List<byte>();
                    MessageLength = 0;
                    STXBuffer = new byte[1];
                    LENBuffer = new byte[2];
                    EOTBuffer = new byte[2];
                    MSGBuffer = new byte[1]; // This is not really required.
                }
                // Look for STX
                try
                {
                    do
                    {
                        bytesRead = clientStream.Read(STXBuffer, 0, 1);
                    } while (STXBuffer[0] != 2);


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
                    // Deal with multipacket..
                    int leftOver = MessageLength - bytesRead;
                    while (leftOver > 0)
                    {
                        int bits = clientStream.Read(MSGBuffer, bytesRead, leftOver);
                        leftOver -= bits;
                    }

                    messageArray.AddRange(MSGBuffer);
                    // Now get EOT 2 charachers
                    bytesRead = clientStream.Read(EOTBuffer, 0, 2);
                    messageArray.AddRange(EOTBuffer);
                    // Okay Now have complete message send Ack Back
                    if (!_disableAcks) clientStream.WriteByte(6);
                    // Fire Event that data is ready.

                    OnDataReceived("TCP Message", messageArray.ToArray());
                    logData(messageArray.ToArray(), "Message Received");
                    //(messageArray, "Message Received");
                }
                catch (Exception ex)
                {
                    _keepReading = false;
                    bytesRead = 0;
                }
            }

            logData(null, "finished");
        }

        public event CommPort.StatusChangedEventHandler StatusChanged;

        // JP Add null handler exceptions
        public void OnStatusChanged(string param)
        {
            logData(null, "Status Changed " + param);
            if (StatusChanged != null)
            {
                // Call the Event
                StatusChanged(param);
            }
        }

        #region logging
        public void logData(byte[] data, string headerString)
        {
            try
            {
                if (_logData)
                {
                    string path = getServiceConfigPath();
                    if (path.Contains(Path.DirectorySeparatorChar)) { path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar) + 1); }
                    String logfile = String.Format("{0}{1:dd_MMM_yyyy}simdata.txt", path, System.DateTime.Now);

                    SLogging log = new SLogging(logfile);

                    headerString = String.Format("{0} {1:dd MMM yyyy H:mm:ss:fff} Version {2}", headerString, System.DateTime.Now, Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    headerString = String.Format("{0}\r\n{1}", headerString, new String('-', headerString.Length));
                    headerString += "\r\n";

                    log.LogData(data, headerString);
                }
            }
            catch (Exception) { }

        }
        private string getServiceConfigPath()
        {

            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + Path.DirectorySeparatorChar + "HicapsConnect" + Path.DirectorySeparatorChar;

        }
        private static string WrapString(string text, int maxLength)
        {
            string returnString = "";

            foreach (string inputString in Wrap(text, maxLength))
            {
                returnString += String.Format("{0}{1}     {2}\r\n", inputString, new String(' ', 48 - inputString.Length), ConvertToByteString(inputString));
            }

            return returnString;
        }
        private int DoubleBcdToInt(byte[] size)
        {
            int value, Value100;
            value = System.Convert.ToInt32(size[1].ToString("X2"));
            Value100 = System.Convert.ToInt32(size[0].ToString("X2")) * 100;
            return value + Value100;
        }
        private static string ConvertToByteString(string hexValues)
        {
            string[] hexValuesSplit = hexValues.Split(' ');
            string returnString = "";
            foreach (String hex in hexValuesSplit)
            {
                // Convert the number expressed in base-16 to an integer.
                int value = Convert.ToInt32(hex, 16);
                // Any non-alpha show as full stop.
                if (value < 32 || value > 127) { value = 46; }
                // Get the character corresponding to the integral value.
                returnString += Char.ConvertFromUtf32(value);
            }
            return returnString;
        }

        private int _baudRate = 11100;
        public int GetBaudRate()
        {
            return _baudRate;
        }

        public void setBaudRate(int baudRate)
        {
            _baudRate = baudRate;
        }
        private static List<String> Wrap(string text, int maxLength)
        {

            // Return empty list of strings if the text was empty
            if (text.Length == 0) return new List<string>();

            var words = text.Split('-');
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

        #endregion
        public void DisableAcks()
        {
            _disableAcks = true;
        }
    }
}
