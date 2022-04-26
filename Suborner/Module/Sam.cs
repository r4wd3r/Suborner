using System;

namespace Suborner.Module
{
    
    class Sam
    {
        public void parseV(byte[] V)
        {
            // Initialize object properties dynamically through reflection with values extracted from V 
            int i = 0;
            int size = 0;
            foreach (var f in typeof(SAM_ACCOUNT_V).GetFields())
            {
                SAM_ACCOUNT_V_ENTRY entry = new SAM_ACCOUNT_V_ENTRY();
                byte[] headerOffsetReader = { V[i], V[i + 1], V[i + 2], V[i + 3] };
                byte[] headerLengthReader = { V[i + 4], V[i + 5], V[i + 6], V[i + 7] };
                byte[] headerUnkReader = { V[i + 8], V[i + 9], V[i + 10], V[i + 11] };

                entry.offset = BitConverter.ToInt32(headerOffsetReader, 0);
                entry.length = BitConverter.ToInt32(headerLengthReader, 0);
                entry.unknown = BitConverter.ToInt32(headerUnkReader, 0);
                entry.value = new byte[entry.length];
                Array.Copy(V, entry.offset + 0xcc, entry.value, 0, entry.length);
                f.SetValue(this, entry);
                size += 12 + GetAlignedLength(entry.length); // Windows aligns V by 4
                i += 12;
            }
            this.Size = size;
        }


    }
}
