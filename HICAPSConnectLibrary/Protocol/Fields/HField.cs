using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HICAPSConnectLibrary.Protocol.Fields
{
    public enum HFieldType
    {
        AlphaNumeric,Medicare,Numeric,BitMap,BCD
    }

    public class HField
    {
        public HField()
        {
            
        }
        public HField(string key, HFieldType hFieldType, uint minLength, uint maxLength)
        {
            Key = key;
            HType = hFieldType;
            MinLength = minLength;
            MaxLength = maxLength;
        }

        public const uint MAX_LENGTH = uint.MaxValue;
        /// <summary>
        /// 2 Digit Key Value.  e.g  "00"
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Minimum Length of Encoded Field 
        /// Values from 0 to uint.MaxValue
        /// </summary>
        public uint MinLength { get; set; }

        /// <summary>
        /// Maximum Length of Encoded Field 
        /// Values from 0 to int.MaxValue
        /// If set to -1, then length is theory unlimited.
        /// </summary>
        public uint MaxLength { get; set; }

        /// <summary>
        /// Type of field, as stored in byte level message
        /// </summary>
        public HFieldType HType { get; set; }

        private readonly List<string> _fieldEncrypted = new List<string> { "M0", "M1", "M2", "M3", "C1", "30" };
        public bool Encrypted
        {
            get { return _fieldEncrypted.Contains(Key); }
            set { }
        }

    }
}
