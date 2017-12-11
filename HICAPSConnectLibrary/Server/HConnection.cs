using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace HICAPSConnectLibrary.Server
{
    public class HConnection
    {
        public TcpClient tcpClient;
        public int SocketCount
        {
            get { return socketCount; }
            set
            {
                if (socketCount == value)
                    return;
                socketCount = value;
            }
        }
        private int socketCount = 0;
        public HConnection() { }
    }
}
