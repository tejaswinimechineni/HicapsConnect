﻿using System.Collections.Generic;

namespace HicapsConnectIntegrate.Class
{
    public class QuotePharmRequest : QuoteRequest
    {
        private List<string> _itemList = new List<string>();

        public void setItemList(List<string> itemList)
        {
            _itemList.Clear();
            _itemList.AddRange(itemList);
        }

        private List<string> _scriptDetails;

        public List<string> ScriptDetails
        {
            get { return _scriptDetails; }
            set { _scriptDetails = value; }
        }

        public QuotePharmRequest()
        {
            _scriptDetails = new List<string>();
        }

        public override bool validateMessage(ref string validationMessage)
        {
            base.validateMessage(ref validationMessage);

            foreach (string myRow in ClaimDetails)
            {
                validationMessage += validateClaimLine(myRow);
                if (myRow.Length > 6)
                {
                    string item = myRow.Substring(2, 4);
                    if (!_itemList.Contains(item))
                    {
                        validationMessage += "Invalid Item :-" + item + "|";
                    }
                }
            }
            return checkValidationMessage(validationMessage);
        }
    }
}