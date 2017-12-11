using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HICAPSAPI.Controllers
{
    public class ClaimController : ApiController
    {
        // GET: api/Claim
        public IEnumerable<string> Get()
        {
            HicapsConnectControl.HicapsConnectControl ctr = new HicapsConnectControl.HicapsConnectControl();

            ctr.createobject("0.0.0.0:0:SEE55A:COMT");

            HicapsConnectControl.HicapsConnectControl.TerminalTestResponse myResponse = ctr.sendTerminalTest("0.0.0.0:0:SEE55A:COMT");

            var arr = ctr.getTerminalList();

            HicapsConnectControl.HicapsConnectControl.ClaimPharmRequest req = new HicapsConnectControl.HicapsConnectControl.ClaimPharmRequest();
            req.ProviderNumberId = "0540961A";
            req.ClaimDetails.Add("01R001001112008000");
            req.ServerUrl = "SEE55A";
            req.PmsKey = "8562026";
            req.SoftwareVendorName = "HICAPSConnectClient";
            req.PrintReceiptOnTerminal = false;
         


            var res = ctr.sendClaimRequest(req);

         
            return new string[] { "value1", "value2" };
        }

        // GET: api/Claim/5
        public string Get(int id)
        {
            HicapsConnectControl.HicapsConnectControl ctr = new HicapsConnectControl.HicapsConnectControl();

            ctr.createobject("0.0.0.0:0:SEE55A:COMT");

            HicapsConnectControl.HicapsConnectControl.TerminalTestResponse myResponse = ctr.sendTerminalTest("0.0.0.0:0:SEE55A:COMT");

            var arr = ctr.getTerminalList();

            var req = new HicapsConnectControl.HicapsConnectControl.ClaimRequest() { ProviderNumberId = "AAMT1234" };
            req.addClaimLine("01", "1001", "11", DateTime.Now, 100);



            var res = ctr.sendClaimRequest(req);





            string textString = "";
            textString += "ResponseCode = " + (myResponse.ResponseCode ?? "").Trim() + "\r\n";
            textString += "ResponseText = " + (myResponse.ResponseText ?? "").Trim() + "\r\n";
            if (myResponse.ResponseText.Trim() != "Transaction Timed Out")
            {
                textString += "Merchant Id = " + (myResponse.MerchantId ?? "").Trim() + "\r\n";
                textString += "Terminal Id = " + (myResponse.TerminalId ?? "").Trim() + "\r\n";
                textString += "Transaction Date = " + myResponse.TransactionDate + "\r\n";
                textString += "Response Time = " + myResponse.ResponseTime + "\r\n";
            }
            return textString;
        }

        // POST: api/Claim
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Claim/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Claim/5
        public void Delete(int id)
        {
        }
    }
}
