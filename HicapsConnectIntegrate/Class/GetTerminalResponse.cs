using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    public class GetTerminalResponse : BaseResponse
    {
        private List<string> _terminalList;

        public List<string> TerminalList
        {
            get { return _terminalList; }
            set { _terminalList = value; }
        }
        private List<string> _serverData = new List<string>();

        public List<string> ServerData
        {
            get { return _serverData; }
            set { _serverData = value; }
        }
        public GetTerminalResponse() { TerminalList = new List<string>(); }
    }
}
