using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectClient12
{
    class TerminalConnection
    {
        private HicapsConnectControl.HicapsConnectControl hicaps;
        private String termName;

        public TerminalConnection(HicapsConnectControl.HicapsConnectControl hicaps, String termName)
        {
            this.hicaps = hicaps;
            this.termName = termName;
        }


        // Synchronises data with the terminal
        public void sync()
        {

        }
    }
}
