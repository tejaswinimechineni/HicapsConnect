using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using HICAPSConnectLibrary.Utils;

namespace HICAPSConnectLibrary.Ports
{

    public class CommPort : ICommPort
    {
        SerialPort _serialPort;
        Thread _readThread;
        volatile bool _keepReading;
        private bool disposed = false;
        private bool _logData = false;
        private bool _checkAck = false;
        private bool _sendingAck = false;
        private DateTime _ackTime;
        private int _iOpenDelay = getConfigValueInt("OpenDelay", 100); // Thread.Sleep Delay after Opening Port
        private int _iReadDelay = getConfigValueInt("ReadDelay", 100); // Thread.Sleep Delay after Reading Partial Message

        private int _iReadTimeOut = getConfigValueInt("ReadTimeout", 2000); // ReadTimeout in _SerialPort Object
        private int _iWriteTimeOut = getConfigValueInt("WriteTimeout", 2000);// WriteTimeout in _SerialPort Object
        private int _iWriteTimeoutOutAsync = getConfigValueInt("WriteTimeoutAsync", 2000); // WriteTimeout in Async Methods
        private int _iSendAckCount = getConfigValueInt("SendAckCount", 1); // Number of Acks to send per message
        private int _iSendAckDelay = getConfigValueInt("SendAckDelay", 0); // Delay Ack Sending til after message is processed
        private int _iSendAckAsync = getConfigValueInt("SendAckAsync", 0); // Whether and How to send 'Acks' asycn or not.
        // 0 = Use Normal Default SendAsync for _iSendAckCount Ack
        // 1 = Use Normal Default SendAsync for 1 Ack Only, Use nonAsync til _iSendAckCount reached
        // 2 = Use nonAsync til _iSendAckCount reached
        // 3 = Do not send Ack at all
        // 4 = Use new Thread Pool via SendAsync method for 1 Ack Only.
        // 5 = Use new Thread Pool SendAsync method for _iSendAckCount Ack 
        // 6 = Via Thread Pool, Queue ThreadPool method for _iSendAckCount Ack Counts

        private int _iSendByByte = getConfigValueInt("SendByByte", 0);      // Set to 1 to force sending character by character
        private int _iSendByByteDelay = getConfigValueInt("SendByByte", 10); // Time in milliseconds to delay writing each character to serial port

        //     private int _iFlushWriteBuffer = getConfigValueInt("FlushWriteBuffer", 0); // Flush output Buffer on each write

        //begin Singleton pattern
        // static readonly CommPort instance = new CommPort();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        //static CommPort()
        //{
        //}
        public bool IsAckReceived()
        {
            return !_checkAck;
        }
        public bool IsAckSending()
        {
            return _sendingAck;
        }
        public CommPort()
        {
            //_serialPort = new SerialPort();
            _readThread = null;
            _keepReading = false;
        }

        //public static CommPort Instance
        //{
        //    get
        //    {
        //        return new CommPort();
        //    }
        //}
        //end Singleton pattern

        //begin Observer pattern
        public delegate void EventHandler(string param, byte[] byteParam);
        public delegate void StatusChangedEventHandler(string param);

        public event StatusChangedEventHandler StatusChanged;

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

        public event EventHandler DataReceived;
        public void OnDataReceived(string param, byte[] byteParam)
        {
            if (DataReceived != null)
            {
                // Call the Event
                DataReceived(param, byteParam);
            }
        }
        //end Observer pattern

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

        /// <summary> Get the data and pass it on. </summary>
        /// 
        ///
        /// <summary> Get the data and pass it on. </summary>
        /// 
        private void ReadPort()
        {
            List<byte> bBuffer = new List<byte>();
            byte[] bcdArray = new byte[2];
            int STXpos = -1;
            int BCDEOTpos = -1;
            int loopcount = 0;
            while (_keepReading)
            {
                int bytesToRead = _serialPort.BytesToRead;
                byte[] readInputBuffer = new byte[bytesToRead];
                bytesToRead = _serialPort.Read(readInputBuffer, 0, bytesToRead);
                if (readInputBuffer != null && readInputBuffer.Length > 0)
                {
                    bBuffer.AddRange(readInputBuffer);
                    // Get Start of Text character
                    if (STXpos < 0)
                    {
                        STXpos = bBuffer.IndexOf(2);
                        // Reset loop count check because we are at the start of a new message
                        loopcount = 0;
                    }
                    if (_checkAck)
                    {
                        int ackIndex = bBuffer.IndexOf(6);
                        if (ackIndex >= 0)
                        {
                            byte[] data = new byte[1];
                            data[0] = 6;
                            logData(data, "Received Ack");
                            _ackTime = DateTime.Now;
                            _checkAck = false;
                        }
                    }
                    // Get End of Text position.
                    if (STXpos >= 0 && BCDEOTpos < 0 && bBuffer.Count > STXpos + 3)
                    {
                        bcdArray[0] = bBuffer[STXpos + 1];
                        bcdArray[1] = bBuffer[STXpos + 2];
                        BCDEOTpos = DoubleBcdToInt(bcdArray) + 4;
                    }
                    if (bBuffer.Count - STXpos >= (BCDEOTpos + STXpos) && STXpos >= 0 && BCDEOTpos > 0)
                    {
                        // We have complete message in buffer
                        byte[] messageArray = bBuffer.GetRange(STXpos, BCDEOTpos + STXpos).ToArray();
                        // If bBuffer had more bytes than message tag it on for the next message.
                        if (bBuffer.Count - (STXpos + BCDEOTpos + 1) > 0)
                        {
                            byte[] messageTrailer = bBuffer.GetRange(BCDEOTpos + 1, (bBuffer.Count - (BCDEOTpos + 1))).ToArray();
                            bBuffer = new List<byte>();
                            bBuffer.AddRange(messageTrailer);
                        }
                        else
                        {
                            bBuffer = new List<byte>();
                        }
                        if (_iSendAckDelay == 0) { SendAck(); } // If Delay off (Default) send ack before message is processed
                        bcdArray = new byte[2];
                        STXpos = -1;
                        BCDEOTpos = -1;
                        string SerialIn = System.Text.Encoding.Default.GetString(messageArray);
                        logData(messageArray, "Message Received");

                        OnDataReceived(SerialIn, messageArray);
                        if (_iSendAckDelay == 1) { SendAck(); }// If Delay on  send ack after message is processed

                    }
                }
                else
                {
                    if (STXpos >= 0 && BCDEOTpos > 0)
                    {
                        logData(null, "Waiting for " + ((BCDEOTpos + STXpos) - bBuffer.Count).ToString() + " bytes ");
                        if (loopcount == 15)
                        {
                            logData(null, "Resetting message");
                            // This statement is to reset the message loop.  If after 10 & 100 ms = 1sec
                            // we still only have part of a message... reset the loop, because after 2 sec the terminal
                            // will resend the complete message again.
                            bcdArray = new byte[2];
                            STXpos = -1;
                            BCDEOTpos = -1;

                        }
                    }
                    logData(null, "Waiting for serial data...." + loopcount.ToString());
                    Thread.Sleep(_iReadDelay);
                    loopcount++;

                }

            }
            logData(null, "finished");
        }


        /// <summary> Open the serial port with current settings. </summary>
        public void Open(string portName, int baudRate, Parity portParity, int portDataBits, StopBits portStopBits, Handshake portHandshake, int timeOut, Encoding dataEncoding)
        {
            //Close();

            try
            {
                string configDataString = "SerialPort Settings: ";
                configDataString = configDataString + "WriteTimeOut=" + _iWriteTimeOut.ToString() + ", ";
                configDataString = configDataString + "ReadTimeOut=" + _iReadTimeOut.ToString() + ", ";
                configDataString = configDataString + "WriteTimeoutOutAsync=" + _iWriteTimeoutOutAsync.ToString() + ", ";
                configDataString = configDataString + "OpenDelay=" + _iOpenDelay.ToString() + ", ";
                configDataString = configDataString + "ReadDelay=" + _iReadDelay.ToString() + ", ";
                configDataString = configDataString + "SendAckCount=" + _iSendAckCount.ToString();
                //    configDataString = configDataString + "FlushWriteBuffer=" + _iFlushWriteBuffer.ToString();

                logData(null, String.Format("Opening Port {0}", portName));
                logData(null, configDataString);
                if (portName != "")
                    _serialPort = new SerialPort(portName);

                _serialPort.PortName = portName;
                _serialPort.BaudRate = baudRate;
                _serialPort.Parity = portParity;
                _serialPort.DataBits = portDataBits;
                _serialPort.StopBits = portStopBits;
                _serialPort.Handshake = portHandshake;
                _serialPort.ReadTimeout = timeOut;
                _serialPort.Encoding = dataEncoding;
                //  _serialPort.PinChanged += new SerialPinChangedEventHandler(_serialPort_PinChanged);
                //   _serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(_serialPort_ErrorReceived);
                // Set the read/write timeouts
                _serialPort.ReadTimeout = _iReadTimeOut;
                _serialPort.WriteTimeout = _iWriteTimeOut;
                _serialPort.Open();
                // PJ Added Sleep  in case old data is still sitting on the wire.
                Thread.Sleep(_iOpenDelay);
                // PJ Added flush in case old data is still sitting on the wire.
                //  _serialPort.DiscardInBuffer();
                // _serialPort.DiscardOutBuffer();

                StartReading();
            }
            catch (IOException ioex)
            {
                OnStatusChanged(String.Format("{0} does not exist", portName));
                logData(null, ioex.ToString());
            }
            catch (UnauthorizedAccessException uaeEx)
            {
                OnStatusChanged(String.Format("{0} already in use", portName));
                logData(null, uaeEx.ToString());
            }
            catch (Exception ex)
            {
                OnStatusChanged(String.Format("{0}", ex.ToString()));
                logData(null, ex.ToString());
            }

            // Update the status
            if (_serialPort.IsOpen)
            {
                string p = _serialPort.Parity.ToString().Substring(0, 1);   //First char
                string h = _serialPort.Handshake.ToString();
                if (_serialPort.Handshake == Handshake.None)
                    h = "no handshake"; // more descriptive than "None"

                OnStatusChanged(String.Format("{0}: {1} bps, {2}{3}{4}, {5}",
                    _serialPort.PortName, _serialPort.BaudRate,
                    _serialPort.DataBits, p, (int)_serialPort.StopBits, h));
            }
            else
            {
                OnStatusChanged(String.Format("{0} already open or in use ?", portName));
            }
        }


        /// <summary> Close the serial port. </summary>
        public void Close()
        {

            StopReading();
            if (_serialPort != null)
            {
                logData(null, String.Format("Closing Port {0} ", _serialPort.PortName));
                ClearSerialInternalBuffers();
                _serialPort.Close();
                _serialPort.Dispose();
            }
            OnStatusChanged("connection closed");
        }

        /// <summary> Get the status of the serial port. </summary>
        public bool IsOpen
        {
            get
            {
                return _serialPort.IsOpen;
            }
        }

        /// <summary> Get a list of the available ports. Already opened ports
        /// are not returned. </summary>
        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        ///// <summary>Send data to the serial port after appending line ending. </summary>
        ///// <param name="data">An string containing the data to send. </param>
        //public void Send(string data)
        //{
        //    if (IsOpen)
        //    {
        //        string lineEnding = "";
        //        switch (Settings.Option.AppendToSend)
        //        {
        //            case Settings.Option.AppendType.AppendCR:
        //                lineEnding = "\r"; break;
        //            case Settings.Option.AppendType.AppendLF:
        //                lineEnding = "\n"; break;
        //            case Settings.Option.AppendType.AppendCRLF:
        //                lineEnding = "\r\n"; break;
        //        }

        //        _serialPort.Write(data + lineEnding);
        //    }
        //}
        // this function gets called when an connection event occurs... possible events: client connected, connection refused etc. ...but it doesn't have a connection timeout...it waits as long as necessary to get anykind of reply
        private static void WriteCallback(IAsyncResult ar)
        {
            try
            {
                Stream client = (Stream)ar.AsyncState;
                client.EndWrite(ar); // Complete the connection.

                connectDone.Set(); // trigger the connectDone event 
            }
            catch (Exception e)
            {

                //Console.WriteLine(e.ToString());
            }
        }
        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        public void SendAsync(byte[] data, bool skipAckCheck)
        {
            //int WriteTimeout = _iWriteTimeoutOutAsync; // 2000 mili Seconds
            if (_iSendByByte == 1) { SendAsyncByByte(data, skipAckCheck); return; }

            if (IsOpen)
            {
                if (data.Length > 1)
                {
                    if (skipAckCheck)
                    {
                        _checkAck = false;
                    }
                    else
                    {
                        _checkAck = true;
                    }
                }

                logData(data, "Sending Async Buffer:- " + _iWriteTimeoutOutAsync.ToString());
                _serialPort.BaseStream.BeginWrite(data, 0, data.Length, new AsyncCallback(WriteCallback), _serialPort.BaseStream);
                bool fired = connectDone.WaitOne(_iWriteTimeoutOutAsync, false);

                // JP NOTE this line causes the whole thing to fall in a heap......
                //  if (_iFlushWriteBuffer == 1) { _serialPort.BaseStream.Flush(); }

                logData(null, "Sent:- " + bool.TrueString);
            }
        }
        public void SendAsyncByByte(byte[] data, bool skipAckCheck)
        {
            if (IsOpen)
            {
                logData(data, "Sending Async Buffer By Byte:- " + _iWriteTimeoutOutAsync.ToString());

                if (data.Length > 1)
                {
                    if (skipAckCheck)
                    {
                        _checkAck = false;
                    }
                    else
                    {
                        _checkAck = true;
                    }
                }

                for (int a = 0; a < data.Length; a++)
                {
                    _serialPort.BaseStream.BeginWrite(data, a, 1, new AsyncCallback(WriteCallback), _serialPort.BaseStream);
                    bool fired = connectDone.WaitOne(_iWriteTimeoutOutAsync, false);

                    // sleep thread for some ms.
                    Thread.Sleep(_iSendByByteDelay);
                }

                logData(null, "Sent");
            }
        }

        public void SendByByte(byte[] data)
        {
            if (IsOpen)
            {
                logData(data, "Sending Buffer By Byte:- " + _iWriteTimeOut.ToString());
                for (int a = 0; a < data.Length; a++)
                {
                    _serialPort.WriteTimeout = _iWriteTimeOut;
                    _serialPort.Write(data, a, 1);

                    // sleep thread for some ms.
                    Thread.Sleep(_iSendByByteDelay);
                }

                logData(null, "Sent");
            }
        }

        public void Send(byte[] data)
        {
            if (IsOpen)
            {
                if (data.Length > 1)
                {
                    _checkAck = true;
                }
                logData(data, "Sending Buffer, Timeout:" + _iWriteTimeOut.ToString());
                _serialPort.WriteTimeout = _iWriteTimeOut;

                _serialPort.Write(data, 0, data.Length);

                logData(null, "Sent");
            }
        }
        public void TPSendAck()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPSendAck));
        }

        public void TPSendQueueAck(int count)
        {
            for (int i = 0; i < count; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(TPSendAck));
            }

        }
        public void TPSendAck(object oData)
        {
            byte[] data = new byte[1];
            data[0] = 6;
            for (int i = 0; i < _iSendAckCount; i++)
            {
                if (_iSendAckAsync == 4) { SendAsync(data, true); }
            }
        }
        /// <summary>
        /// Send ack to serial port
        /// 
        /// </summary>
        public void SendAck()
        {
            if (_iSendAckAsync == 4) { TPSendAck(); return; }
            if (_iSendAckAsync == 6) { TPSendQueueAck(_iSendAckCount); return; }

            _sendingAck = true;
            if (_iSendAckCount <= 0) { _iSendAckCount = 1; }
            byte[] data = new byte[1];
            data[0] = 6;

            for (int i = 0; i < _iSendAckCount; i++)
            {
                if (_iSendAckAsync == 0) { SendAsync(data, true); }
                if (_iSendAckAsync == 1 && i == 0) { SendAsync(data, true); }
                if ((_iSendAckAsync == 2) || (_iSendAckAsync == 1 && i > 0)) { Send(data); }
                if (_iSendAckAsync == 5) { TPSendAck(); }

            }

            logData(data, "Sending Ack");
            _sendingAck = false;

        }
        private int _baudRate = 9600;
        public int GetBaudRate()
        {
            return 9600;
        }

        public void setBaudRate(int baudRate)
        {
            _baudRate = baudRate;
        }
        /// <summary>
        /// Releases unmanaged and managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <value>Releases the comm port.</value>
        virtual protected void Dispose(bool disposing)
        {
            StopReading();
            if (disposing)
            {
                if (!disposed)
                {
                    try
                    {
                        if (_serialPort != null)
                        {
                            ClearSerialInternalBuffers();

                            if (_serialPort.IsOpen)
                            {
                                _serialPort.Close();
                            }
                            _serialPort.Dispose();
                            _serialPort = null;
                        }
                    }
                    finally
                    {
                        disposed = true;
                    }
                }
            }
        }

        private void ClearSerialInternalBuffers()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                logData(null, "Clearing internal Serial Buffers");
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }


        //// JP BCD Code
        //private int BcdToInt(byte size)
        //{
        //    return System.Convert.ToInt32(size.ToString("X2"));
        //}
        private int DoubleBcdToInt(byte[] size)
        {
            int value, Value100;
            value = System.Convert.ToInt32(size[1].ToString("X2"));
            Value100 = System.Convert.ToInt32(size[0].ToString("X2")) * 100;
            return value + Value100;
        }

        public void setLogging(bool truefalse)
        {
            _logData = truefalse;
        }
        private string getServiceConfigPath()
        {

            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + Path.DirectorySeparatorChar + "HicapsConnect" + Path.DirectorySeparatorChar;

        }
        public void logData(byte[] data, string headerString)
        {
            try
            {
                if (_logData)
                {
                    string path = getServiceConfigPath();
                    if (path.Contains(Path.DirectorySeparatorChar)) { path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar) + 1); }
                    String logfile = String.Format("{0}{1:dd_MMM_yyyy}serialdata.txt", path, System.DateTime.Now);
                    headerString = String.Format("{0} {1:dd MMM yyyy H:mm:ss:fff} Version {2}", headerString, System.DateTime.Now, Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    headerString = String.Format("{0}\r\n{1}", headerString, new String('-', headerString.Length));
                    if (data != null && data.Length > 0)
                    {
                        headerString = String.Format("{0}\r\n{1}\r\n", headerString, WrapString(BitConverter.ToString(data), 46));
                    }
                    else
                    {
                        headerString += "\r\n";
                    }
                    File.AppendAllText(logfile, headerString);
                }
            }
            catch (Exception) { }

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
        private static int getConfigValueInt(string setting, int defaultValue)
        {
            int result;
            string value = getConfigValue(setting, defaultValue.ToString());
            if (int.TryParse(value, out result)) { return result; }
            return defaultValue;
        }
        private static string getConfigValue(string setting, string defaultValue)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location ?? "") ?? "", "CommSettings.ini");
            if (File.Exists(path))
            {

                string[] rules = File.ReadAllLines(path);
                string ruleValue = rules.Where(c => c.ToLower().Replace(" ", "").StartsWith(setting.ToLower() + '=')).FirstOrDefault();
                if (string.IsNullOrEmpty(ruleValue)) { return defaultValue; } // didn't find setting
                return ruleValue.Substring(ruleValue.IndexOf("=") + 1);
            }
            return defaultValue;
        }

        public void DisableAcks()
        {
            throw new NotImplementedException("Acks for Serial are compulsary, and cannot be disabled");
        }
    }
}