using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    public class ClaimPharmResponse : ClaimResponse
    {
        private List<string> _scriptDetails;

        public ClaimPharmResponse()
        {
            _scriptDetails = new List<string>();
        }

        public List<string> ScriptDetails
        {
            get { return _scriptDetails; }
            set { if (!ReadOnly) { _scriptDetails = value; } }
        }

    }
}
