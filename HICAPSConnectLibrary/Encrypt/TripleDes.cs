using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace HICAPSConnectLibrary.Encrypt
{
    public static class TripleDesOFB
    {
        /// <summary>
        /// Encrypt/Decrypt using Symmetrical TripleDES OFB Mode.  Uses default IV.
        /// </summary>
        /// <param name="Data">Byte array of data to Encrypt/Decrtype</param>
        /// <param name="Key">16 byte key array</param>
        /// <returns>byte[] Encrypted/Decrypted text</returns>
        public static byte[] OFBCrypt(byte[] Data, byte[] Key)
        {
            //Default IV
            var IV = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            return OFBCrypt(Data, Key, IV);
        }

        /// <summary>
        /// Get KeyCheck Value.  
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] OFBKeyCheck(byte[] key)
        {
            byte[] zeroArray = { 0, 0, 0, 0, 0, 0, 0, 0 };
            return RunTripleDesNET(key, zeroArray);
        }
        /// <summary>
        /// Encrypt/Decrypt using Symmetrical TripleDES OFB Mode
        /// </summary>
        /// <param name="Data">Byte array of data to Encrypt/Decrtype</param>
        /// <param name="Key">16 byte key array</param>
        /// <param name="IV">8 byte IV array</param>
        /// <returns>byte[] Encrypted/Decrypted text</returns>
        public static byte[] OFBCrypt(byte[] Data, byte[] Key, byte[] IV)
        {
            // Need Method protection for byte array lengths.
            // Only data cannot be 8 bytes long

            long blockCount, bytesRemain, block;
            if (Data == null || Data.Length == 0)
            {
                throw new ArgumentOutOfRangeException("Data", "Data cannot be null or empty");
            }

            if (IV == null || IV.Length != 8)
            {
                throw new ArgumentOutOfRangeException("IV", "IV cannot be null or length 0");
            }

            var dataOut = new List<byte>(Data.Length);

            long dataLen = Data.Length;
            blockCount = dataLen / 8;
            bytesRemain = dataLen % 8;

            long iData = 0; // array index indicator, steps in 8

            for (block = 0; block < blockCount; block++, iData += 8)
            {
                // OK so first step is encrypt IV with key
                // Then overwrite the current IV for the block.
                IV = RunTripleDesNET(Key, IV);

                // Second step XOR encrypted IV with data.
                // then Add to result
                dataOut.AddRange(XORWord(Data, IV, iData, 8));
            }

            if (bytesRemain > 0)
            {
                // OK so first step is encrypt IV with key
                // Then overwrite the current IV for the block.
                IV = RunTripleDesNET(Key, IV);

                // Second step XOR encrypted IV with data.
                // then Add to result.
                dataOut.AddRange(XORWord(Data, IV, iData, (int)bytesRemain));
            }

            return dataOut.ToArray();
        }

        /// <summary>
        /// Method to run simple ECB TripleDes encryption, with no IV.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataArray"></param>
        /// <returns></returns>
        private static byte[] RunTripleDesNET(byte[] key, byte[] dataArray)
        {
            byte[] resultArray;
            using (var tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = key;
                // Set mode to be ECB.  We don't want chaining...
                tdes.Mode = CipherMode.ECB;

                //Padding mode, What to do if dataarray not a full block-size..
                //Typically this is either Zeros or PKCS7
                //For OFB, this is irrelevant as the size of encrypted = size of decrypted and vise-versa
                tdes.Padding = PaddingMode.Zeros;

                ICryptoTransform cTransform = tdes.CreateEncryptor();
                // Perform magic.
                resultArray = cTransform.TransformFinalBlock(dataArray, 0, dataArray.Length);
                //Cleanup Encryptor.
                tdes.Clear();
            }
            return resultArray;
        }

        /// <summary>
        /// XOR two byte arrays together.  If missized, XOR up to smallest dimension of each array
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        private static byte[] XORWord(byte[] array1, byte[] array2)
        {
            int limit = Math.Min(array1.Length, array2.Length);
            return XORWord(array1, array2, 0, limit);
        }

        /// <summary>
        /// XOR two byte arrays,  each array must be at least 'limit' in size
        /// Start XORing array1 from array1Offset.
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <param name="array1Offset">Offset to start xoring array1</param>
        /// <param name="limit">how many bytes to xor</param>
        /// <returns>byte[limit] in size</returns>
        // ReSharper disable once InconsistentNaming
        private static byte[] XORWord(byte[] array1, byte[] array2, long array1Offset, int limit)
        {
            // define result array.
            var pbOut = new byte[limit];

            uint i;
            for (i = 0; i < limit; i++)
                pbOut[i] = (byte)(array1[i + array1Offset] ^ array2[i]);

            return pbOut;
        }
    }
}