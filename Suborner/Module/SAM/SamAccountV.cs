using Suborner.UI;
using System;

namespace Suborner.Module.SAM
{
    class SamAccountV
    {
        public SAM_ACCOUNT_V V;
        public int size;

        public SamAccountV(byte[] inputV)
        {
            // Initialize object properties dynamically through reflection with values extracted from V 
            int i = 0;
            int size = 0;
            object tempV = new SAM_ACCOUNT_V(); // needed to initialize as object to prevent boxing issue

            foreach (var f in typeof(SAM_ACCOUNT_V).GetFields())
            {
                SAM_ACCOUNT_V_ENTRY entry = new SAM_ACCOUNT_V_ENTRY();
                byte[] headerOffsetReader = { inputV[i], inputV[i + 1], inputV[i + 2], inputV[i + 3] };
                byte[] headerLengthReader = { inputV[i + 4], inputV[i + 5], inputV[i + 6], inputV[i + 7] };
                byte[] headerUnkReader = { inputV[i + 8], inputV[i + 9], inputV[i + 10], inputV[i + 11] };

                entry.offset = BitConverter.ToInt32(headerOffsetReader, 0);
                entry.length = BitConverter.ToInt32(headerLengthReader, 0);
                entry.unknown = BitConverter.ToInt32(headerUnkReader, 0);
                entry.value = new byte[entry.length];
                Array.Copy(inputV, entry.offset + 0xcc, entry.value, 0, entry.length);

                f.SetValue(tempV,entry);
                
                size += 12 + GetAlignedLength(entry.length); // Windows aligns V by 4
                i += 12;
            }
            this.size = size;
            this.V = (SAM_ACCOUNT_V)tempV;
        }
        public void ClearPassword()
        {
            // TODO: Del this!
            ChangeSAMVEntryValue("LMHash", new byte[0]);
            ChangeSAMVEntryValue("NTLMHash", new byte[0]);
        }
        /// <summary>
        /// Method <c>ChangeSAMVEntryValue</c> dynamically modifies the SAM_ACCOUNT_V structure of the current object. 
        /// </summary>
        /// <param name="fieldName"> Name of user property to modify</param>
        /// <param name="value">Value to set new property</param>
        public void ChangeSAMVEntryValue(string fieldName, byte[] value)
        {
            // Changes the length, value, and offsets of subsequent V entries
            int lengthDifference = 0;
            int newSize = this.size;
            bool isUpdated = false;
            object tempV = new SAM_ACCOUNT_V();
            tempV = this.V;     // Added for unboxing issue with reflection and fields
            foreach (var f in typeof(SAM_ACCOUNT_V).GetFields())
            {
                if (f.Name.Equals(fieldName))
                {
                    Printer.PrintDebug("Updating V " + fieldName);
                    SAM_ACCOUNT_V_ENTRY prevEntry;
                    SAM_ACCOUNT_V_ENTRY newEntry = new SAM_ACCOUNT_V_ENTRY();
                    prevEntry = (SAM_ACCOUNT_V_ENTRY)f.GetValue(tempV);
                    newEntry.offset = prevEntry.offset;
                    newEntry.length = value.Length;
                    newEntry.value = new byte[value.Length];
                    Array.Copy(value, 0, newEntry.value, 0, newEntry.value.Length);
                    f.SetValue(tempV, newEntry);
                    lengthDifference = GetAlignedLength(value.Length) - GetAlignedLength(prevEntry.length);
                    if (lengthDifference == 0) break;  // No need to change subsequent offsets
                    newSize += lengthDifference;
                    isUpdated = true;
                    continue;
                }
                if (isUpdated)
                {
                    SAM_ACCOUNT_V_ENTRY entry;
                    entry = (SAM_ACCOUNT_V_ENTRY)f.GetValue(tempV);
                    entry.offset += lengthDifference;     // Update offset of subsequent properties based on new value length
                    f.SetValue(tempV, entry);
                    continue;
                }
            }
            this.V = (SAM_ACCOUNT_V)tempV;
            this.size = newSize;
        }

        /// <summary>
        /// Method <c>GetAsByteArray</c> converts the currenct <c>SAMV</c> object to <c>byte[]</c>.
        /// </summary>
        public byte[] GetAsByteArray()
        {
            byte[] VArray = new byte[size];
            int i = 0;
            foreach (var f in typeof(SAM_ACCOUNT_V).GetFields())
            {
                SAM_ACCOUNT_V_ENTRY entry = new SAM_ACCOUNT_V_ENTRY();
                entry = (SAM_ACCOUNT_V_ENTRY)f.GetValue(this.V);
                Array.Copy(BitConverter.GetBytes(entry.offset), 0, VArray, i, 4);
                Array.Copy(BitConverter.GetBytes(entry.length), 0, VArray, i + 4, 4);
                Array.Copy(BitConverter.GetBytes(entry.unknown), 0, VArray, i + 8, 4);
                if (entry.value != null) Array.Copy(entry.value, 0, VArray, entry.offset + 0xcc, entry.length);
                i += 12;
            }
            return VArray;
        }
        /// <summary>
        /// Method <c>GetAlignedLength</c> rounds up to next multiple of 4 for memory alignment.
        /// </summary>
        private int GetAlignedLength(int length)
        {
            return (int)Math.Ceiling(((double)length / 4)) * 4;
        }
    }
}
