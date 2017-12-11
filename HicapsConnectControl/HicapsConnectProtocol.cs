using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using HicapsTools;

namespace HicapsConnectControl
{
    [ComVisible(false)]
    public class HicapsConnectProtocol
    {
        public string ServerUrl { get; set; }
        public string XmlRequest { get; set; }
        public string XmlResponse { get; set; }
        public int timeout { get; set; }

        private const string STR_StandAloneRequestTimedOutCheckServiceIsRunning = "StandAlone Request timed out, Check Service is running";
        private readonly int UPD_ListenPort = 11001;
        private readonly int TCP_ServerPort = 11000;
        private readonly string STR_AckReceived = "Ack Received, Waiting for Terminal";
        private readonly string STR_Reading = "Reading";
        private readonly string STR_QueGoAway = "Que ? Go away";
        private readonly string STR_NetworkError = "Network Error";
        private readonly string STR_DestinationErrorCouldNotConnectToServer = "Destination Error, Could not connect to Server{0}";
        private readonly string STR_NetworkRequestTimedOut = "Network Request timed out{0}";
        private const string STR_ACK00000000000000000 = "ACK00000000000000000";
        private readonly string STR_Connecting = "Connecting";
        private readonly string STR_SendingRequest = "Sending request...";
        private readonly string STR_Sending = "Sending";
        StatusForm _myStatusForm;
        private string networkName = "";

        public HicapsConnectProtocol() { }
        public HicapsConnectProtocol(string aNetworkName) 
        {
            if(!string.IsNullOrEmpty(aNetworkName))
            {
                aNetworkName = aNetworkName.Trim();
                networkName = aNetworkName.CalculateMD5();
            }
        }

        //begin Observer pattern
        public delegate void StatusMessageEventHandler(string param);

        public event StatusMessageEventHandler StatusMessage;

        // JP Add null handler exceptions
        public void OnStatusMessage(string param)
        {
            if (StatusMessage != null)
            {
                // Call the Event
                StatusMessage(param);
                setStatus(param);
            }
        }
        public delegate void CompletedMessageEventHandler();
        public event CompletedMessageEventHandler CompletedMessage;

        // JP Add null handler exceptions
        public void OnCompletedMessage()
        {
            if (CompletedMessage != null)
            {
                // Call the Event
                CompletedMessage();
            }
        }
        #region TCPIP Async Methods
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                TcpClient client = (TcpClient)ar.AsyncState;
                client.EndConnect(ar); // Complete the connection.
                // Console.WriteLine("TcpClient connected to {0}", client.Client.RemoteEndPoint.ToString());
                connectDone.Set(); // trigger the connectDone event 
            }
            catch (Exception e)
            {
                // Console.WriteLine(e.ToString());
            }
        }

        #endregion
        #region Encryption Routines
        private static string EncryptTextToMemory(string Data, byte[] Key, byte[] IV)
        {
            try
            {
                // Create a MemoryStream.
                MemoryStream mStream = new MemoryStream();
                // Create a CryptoStream using the MemoryStream 
                // and the passed key and initialization vector (IV).
                TripleDESCryptoServiceProvider myPop = new TripleDESCryptoServiceProvider();
                //myPop.Mode = CipherMode.OFB;

                CryptoStream cStream = new CryptoStream(mStream,
                   myPop.CreateEncryptor(Key, IV),
                   CryptoStreamMode.Write);

                // Convert the passed string to a byte array.
                byte[] toEncrypt = Encoding.Default.GetBytes(Data);

                // Write the byte array to the crypto stream and flush it.
                cStream.Write(toEncrypt, 0, toEncrypt.Length);
                cStream.FlushFinalBlock();

                // Get an array of bytes from the 
                // MemoryStream that holds the 
                // encrypted data.
                byte[] ret = mStream.ToArray();

                // Close the streams.
                cStream.Close();
                mStream.Close();

                // Return the encrypted buffer.
                return Convert.ToBase64String(ret);
            }
            catch (CryptographicException e)
            {
                //  Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }

        }

        private static string DecryptTextFromMemory(string stringData, byte[] Key, byte[] IV)
        {
            byte[] Data;
            try
            {
                // Create a new MemoryStream using the passed 
                // array of encrypted data.
                // stringData = Base64Data String.
                Data = Convert.FromBase64String(stringData);
                MemoryStream msDecrypt = new MemoryStream(Data);

                // Create a CryptoStream using the MemoryStream 
                // and the passed key and initialization vector (IV).
                CryptoStream csDecrypt = new CryptoStream(msDecrypt,
                    new TripleDESCryptoServiceProvider().CreateDecryptor(Key, IV),
                    CryptoStreamMode.Read);

                // Create buffer to hold the decrypted data.
                byte[] fromEncrypt = new byte[Data.Length];

                // Read the decrypted data out of the crypto stream
                // and place it into the temporary buffer.
                csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

                //Convert the buffer into a string and return it.
                string buffer = Encoding.Default.GetString(fromEncrypt);
                // Wipe any nulls.
                buffer.Replace("\0", "").Trim();
                return buffer;
            }
            catch (CryptographicException e)
            {
                //  Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }
        #endregion
        //private void setStatus(string status)
        //{
        //    OnStatusMessage(status);
        //         }
        internal string encodeXmlMessage(string xmlResponse, bool encryptionFlag)
        {
            byte[] eKey = { 147, 72, 89, 123, 125, 120, 252, 168, 8, 144, 196, 133, 198, 169, 8, 207, 203, 128, 62, 16, 63, 192, 244, 172 };
            byte[] eVec = { 18, 240, 79, 176, 164, 196, 81, 151 };
            if (encryptionFlag)
            {
               
                byte[] ehash = Encoding.Default.GetBytes(networkName);
                if (!string.IsNullOrEmpty(networkName))
                {
                    int z = 0;
                    for (int i = 0; i < eKey.Length;i++ )
                    {
                        eKey[i] = (byte)((int) eKey[i] ^ (int) ehash[z]);
                        if (ehash.Length == z + 1) { z = 0; } else { z++; }
                    }
                }

                TripleDESCryptoServiceProvider tDESalg = new TripleDESCryptoServiceProvider();

                string encrypt = EncryptTextToMemory(xmlResponse, eKey, eVec);
                // string base64String = Convert.ToBase64String(Encoding.Default.GetBytes(xmlResponse));
                xmlResponse = encrypt;
            }
            string messageResponseSize = "00000000000000000000" + xmlResponse.Length;
            messageResponseSize = messageResponseSize.Substring(messageResponseSize.Length - 20);
            xmlResponse = messageResponseSize + xmlResponse;
            return xmlResponse;
        }
        internal string decodeXmlMessage(string xmlResponse, bool encryptionFlag)
        {
            byte[] eKey = { 147, 72, 89, 123, 125, 120, 252, 168, 8, 144, 196, 133, 198, 169, 8, 207, 203, 128, 62, 16, 63, 192, 244, 172 };
            byte[] eVec = { 18, 240, 79, 176, 164, 196, 81, 151 };
            if (encryptionFlag)
            {
                byte[] ehash = Encoding.Default.GetBytes(networkName);
                if (!string.IsNullOrEmpty(networkName))
                {
                    int z = 0;
                    for (int i = 0; i < eKey.Length; i++)
                    {
                        eKey[i] = (byte)((int)eKey[i] ^ (int)ehash[z]);
                        if (ehash.Length == z + 1) { z = 0; } else { z++; }
                    }
                }
                string decrypt = DecryptTextFromMemory(xmlResponse, eKey, eVec);
                xmlResponse = decrypt;
            }
            return xmlResponse;
        }
        private static string returnNetworkErrorXml(string errorCode, string errorMessage, string classname)
        {
            string xmlResponse;
            HicapsConnectControl.BaseResponse myResponse = new HicapsConnectControl.BaseResponse();
            myResponse.ResponseCode = errorCode;
            myResponse.ResponseText = errorMessage;
            myResponse.ResponseTime = DateTime.Now;
            //  myRequest.ServerUrl = terminal;
            MemoryStream myMS = new MemoryStream();
            XmlSerializer serializer = new XmlSerializer(typeof(HicapsConnectControl.BaseResponse));
            serializer.Serialize(myMS, myResponse);
            xmlResponse = Encoding.Default.GetString(myMS.GetBuffer());
            // Make Xml match correct request type
            xmlResponse = xmlResponse.Replace("BaseResponse", classname.Replace("Request", "Response"));
            return xmlResponse;
        }
        public void setStatus(string text)
        {
            if (_myStatusForm != null && !_myStatusForm.IsDisposed)
            {
                _myStatusForm.SetText(text);
            }
        }
        private string SendMessageLocal(string XmlRequest, int timeout)
        {
            string XmlResponse = "";
            string clearXmlRequest = XmlRequest;
            int serverConnectTimeout = 5000;
            //DevelopmentLicence myForm = new DevelopmentLicence();
            // myForm.ShowDialog();
            try
            {
                NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "HicapsConnectPipe", PipeDirection.InOut);

                // Connect to the pipe or wait until the pipe is available.
                setStatus(STR_Connecting);

                pipeClient.Connect(serverConnectTimeout);




                XmlRequest = encodeXmlMessage(XmlRequest, true);
                setStatus(STR_Sending);
                Byte[] request = Encoding.Default.GetBytes(XmlRequest);
                pipeClient.Write(request, 0, request.Length);
                pipeClient.Flush();

                byte[] xmlLength;
                string messageLength;
                int lenbytesRead;
                bool messageLoop = false;
                bool statusLoop = true;
                do
                {
                    // Big Status Loop.
                    do
                    {
                        // Inner Ack Loop, maybe we can get rid off.
                        xmlLength = new byte[20];
                        messageLength = "";
                        lenbytesRead = pipeClient.Read(xmlLength, 0, 20);//s.Receive(incomingBuffer);
                        messageLength = Encoding.Default.GetString(xmlLength);
                        // Log("Message size is " + messageLength);
                        switch (messageLength)
                        {
                            case STR_ACK00000000000000000:
                                setStatus(STR_AckReceived);
                                messageLoop = true;
                                break;
                            default:

                                setStatus(STR_Reading);
                                messageLoop = false; break;
                        }
                    } while (messageLoop && pipeClient.IsConnected);
                    int xmlMessageSize = Convert.ToInt32(messageLength);

                    //Byte[] response = new Byte[xmlMessageSize];
                    // int bytesRead = client.GetStream().Read(response, 0, xmlMessageSize);
                    //byte[] resultArray = new byte[bytesRead];
                    // Array.Copy(response, resultArray, bytesRead);
                    //XmlResponse = Encoding.Default.GetString(response).Trim();
                    List<byte> dataBuffer = new List<byte>();
                    int bytesReadSoFar = 0;
                    do
                    {
                        Byte[] response = new Byte[8192];
                        int bytesRead = pipeClient.Read(response, 0, response.Length);
                        bytesReadSoFar += bytesRead;
                        byte[] resultArray = new byte[bytesRead];
                        Array.Copy(response, resultArray, bytesRead);
                        dataBuffer.AddRange(resultArray);
                    } while (bytesReadSoFar < xmlMessageSize);

                    XmlResponse = Encoding.Default.GetString(dataBuffer.ToArray(), 0, dataBuffer.Count).Trim();

                    // Log("Received response: " + XmlResponse);
                    if (XmlResponse.IndexOf('<') < 0)
                    {
                        //  Log("Message was encrypted");
                        XmlResponse = XmlResponse.Replace("\0", "");
                        XmlResponse = decodeXmlMessage(XmlResponse, true);
                    }
                    if (XmlResponse.IndexOf("<StatusResponse") > 0)
                    {
                        //Do something with the status message....
                        try
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(HicapsConnectControl.StatusResponse));
                            MemoryStream myResultMS = new MemoryStream(Encoding.Default.GetBytes(XmlResponse));
                            HicapsConnectControl.StatusResponse myResult = (HicapsConnectControl.StatusResponse)serializer.Deserialize(myResultMS);
                            setStatus(myResult.ResponseText);
                            statusLoop = true;
                        }
                        catch (Exception ex)
                        {
                            statusLoop = false;
                        }
                    }
                    else
                    {
                        statusLoop = false;
                    }

                } while (pipeClient.IsConnected && statusLoop);
                pipeClient.Close();

            }
            catch (TimeoutException te)
            {
                string className = HicapsConnectControl.BaseMessage.extractClassName(clearXmlRequest);
                XmlResponse = returnNetworkErrorXml("EN", String.Format(STR_StandAloneRequestTimedOutCheckServiceIsRunning), className);
                return XmlResponse;
            }
            catch (Exception ex) { }
            return XmlResponse;
        }

        public void SendMessage()
        {

            string clearXmlRequest = XmlRequest;
            int serverConnectTimeout = 5000; // 5 Seconds
            //DevelopmentLicence myForm = new DevelopmentLicence();
            // myForm.ShowDialog();
            // used new methods
            // string base64String = Convert.ToBase64String(Encoding.Default.GetBytes(XmlRequest));
            //XmlRequest = base64String;

            _myStatusForm = new StatusForm();
            _myStatusForm.Show();
            _myStatusForm.BringToFront();
            _myStatusForm.Refresh();

            try
            {
                string[] serveripandport;
                serveripandport = ServerUrl.Split(':');
                string ip = serveripandport[0];
                int port = System.Convert.ToInt32(serveripandport[1]);

                //TcpClient client = new TcpClient(ip, port);
                connectDone.Reset();
                TcpClient client = new TcpClient();
                try
                {
                    OnStatusMessage(STR_Connecting);
                    client.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), client); // here the clients starts to connect asynchronously (because it is asynchronous it doesn't have a timeout only events are triggered)
                    // immediately after calling beginconnect the control is returned to the method, now we need to wait for the event or connection timeout
                    connectDone.WaitOne(serverConnectTimeout, false); // now application waits here for either an connection event (fi. connection accepted) or the timeout specified by connectionTimeout (in milliseconds)

                    if (!client.Connected) { throw new SocketException(10061); } /*10061 = Could not connect to server */

                    client.SendTimeout = timeout;
                    client.ReceiveTimeout = timeout;
                    // client.NoDelay = true;
                    client.ReceiveBufferSize = 32768;
                    // First 20 bytes = length in Ascii.
                    //string messageSize = "00000000000000000000" + XmlRequest.Length;
                    //messageSize = messageSize.Substring(messageSize.Length - 20);
                    //XmlRequest = messageSize + XmlRequest;
                    XmlRequest = encodeXmlMessage(XmlRequest, true);
                    OnStatusMessage(STR_Sending);
                    Byte[] request = Encoding.Default.GetBytes(XmlRequest);
                    client.GetStream().Write(request, 0, request.Length);
                    client.GetStream().Flush();

                    byte[] xmlLength;
                    string messageLength;
                    int lenbytesRead;
                    bool messageLoop = false;
                    bool statusLoop = true;
                    do
                    {
                        // Big Status Loop.
                        do
                        {
                            // Inner Ack Loop, maybe we can get rid off.
                            xmlLength = new byte[20];
                            messageLength = "";
                            lenbytesRead = client.GetStream().Read(xmlLength, 0, 20);//s.Receive(incomingBuffer);
                            messageLength = Encoding.Default.GetString(xmlLength);
                            // Log("Message size is " + messageLength);
                            switch (messageLength)
                            {
                                case STR_ACK00000000000000000:
                                    OnStatusMessage(STR_AckReceived);
                                    messageLoop = true;
                                    break;
                                default:

                                    OnStatusMessage(STR_Reading);
                                    messageLoop = false; break;
                            }
                        } while (messageLoop && client.Connected);
                        int xmlMessageSize = Convert.ToInt32(messageLength);

                        List<byte> dataBuffer = new List<byte>();
                        int bytesReadSoFar = 0;
                        do
                        {
                            Byte[] response = new Byte[client.ReceiveBufferSize];
                            int bytesRead = client.GetStream().Read(response, 0, client.ReceiveBufferSize);
                            bytesReadSoFar += bytesRead;
                            byte[] resultArray = new byte[bytesRead];
                            Array.Copy(response, resultArray, bytesRead);
                            dataBuffer.AddRange(resultArray);
                        } while (bytesReadSoFar < xmlMessageSize);

                        XmlResponse = Encoding.Default.GetString(dataBuffer.ToArray(), 0, dataBuffer.Count).Trim();

                        //Log("Received response: " + XmlResponse);
                        if (XmlResponse.IndexOf('<') < 0)
                        {
                            //  Log("Message was encrypted");
                            XmlResponse = XmlResponse.Replace("\0", "");
                            XmlResponse = decodeXmlMessage(XmlResponse, true);
                        }
                        if (XmlResponse.IndexOf("<StatusResponse") > 0)
                        {
                            //Do something with the status message....
                            try
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(HicapsConnectControl.StatusResponse));
                                MemoryStream myResultMS = new MemoryStream(Encoding.Default.GetBytes(XmlResponse));
                                HicapsConnectControl.StatusResponse myResult = (HicapsConnectControl.StatusResponse)serializer.Deserialize(myResultMS);
                                OnStatusMessage(myResult.ResponseText);
                                statusLoop = true;
                            }
                            catch (Exception ex)
                            {
                                statusLoop = false;
                            }
                        }
                        else
                        {
                            statusLoop = false;
                        }

                    } while (client.Connected && statusLoop);
                    client.Close();
                }
                catch (SocketException)
                {
                    string className = HicapsConnectControl.BaseMessage.extractClassName(clearXmlRequest);
                    XmlResponse = returnNetworkErrorXml("ED", String.Format(STR_DestinationErrorCouldNotConnectToServer, ServerUrl), className);
                }
                catch (TimeoutException)
                {
                    string className = HicapsConnectControl.BaseMessage.extractClassName(clearXmlRequest);
                    XmlResponse = returnNetworkErrorXml("EN", String.Format(STR_NetworkRequestTimedOut, ServerUrl), className);
                }
                catch (Exception)
                {
                    string className = HicapsConnectControl.BaseMessage.extractClassName(clearXmlRequest);
                    XmlResponse = returnNetworkErrorXml("EN", STR_NetworkError, className);
                }

                client.Close();

                //Console.ReadLine();

                //client.Close();
            }
            catch (Exception ex)
            {
                //lock (ignoreServerList)
                //{
                //    if (!ignoreServerList.Contains(server))
                //    {
                //        ignoreServerList.Add(server);
                //    }
                //}
            }
            if (XmlResponse.Length == 0 || XmlResponse == "Que ? Go away")
            {
                string className = HicapsConnectControl.BaseMessage.extractClassName(clearXmlRequest);
                XmlResponse = returnNetworkErrorXml("EN", "Network Error", className);
            }
            OnStatusMessage("Waiting");
            OnCompletedMessage();
            return;
        }
    }
}
