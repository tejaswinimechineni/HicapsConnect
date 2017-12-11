using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    public class QuotePharmResponse : QuoteResponse
    {
        private List<string> _scriptDetails;

        public QuotePharmResponse()
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
