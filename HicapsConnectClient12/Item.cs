using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
/*
 * This is a copy of the same file in HicapsPharmaceuticalApi. It would be 
 * better to share the code between them, but at the moment that's not 
 * possible because that project targets .net 4.0
 */
namespace HicapsConnectClient12
{
    public class Item
    {
        public string Number { get; set; }
        public string Description { get; set; }
        public char Modality { get; set; }
        public decimal Amount { get; set; }
    }
}