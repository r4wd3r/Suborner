using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suborner.Module.SAM
{
    class SamAccount
    {
        public SamAccountF F;
        public SamAccountV V;
        public string Username;
        public int RID;
        public int FRID;

        public SamAccount(byte[] F, byte[] V) {
            this.V = new SamAccountV(V);
        }
    }
}
