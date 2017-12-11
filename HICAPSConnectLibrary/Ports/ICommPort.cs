namespace HICAPSConnectLibrary.Ports
{
    public interface ICommPort
    {
        void Close();
        event CommPort.EventHandler DataReceived;
        void Dispose();
        /*        string[] GetAvailablePorts(); */
        bool IsOpen { get; }
        void OnDataReceived(string param, byte[] byteParam);
        void OnStatusChanged(string param);
        /* void Open(); */
        void Open(string portName, int baudRate, System.IO.Ports.Parity portParity, int portDataBits, System.IO.Ports.StopBits portStopBits, System.IO.Ports.Handshake portHandshake, int timeOut, System.Text.Encoding dataEncoding);
        void Send(byte[] data);
        /*void Send(string data);*/
        void SendAck();
        void SendAsync(byte[] data, bool skipAckCheck);
        void setLogging(bool truefalse);
        int GetBaudRate();
        void setBaudRate(int baudRate);
        void logData(byte[] data, string headerString);
        bool IsAckReceived();
        bool IsAckSending();
        void DisableAcks();
        event CommPort.StatusChangedEventHandler StatusChanged;
    }
}
