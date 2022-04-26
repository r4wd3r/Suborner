using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Suborner.Module.SAM
{
    /// <summary>
    /// Class <c>Sam</c> models all the data needed and extracted from the SAM
    /// </summary>
    class Sam
    {
        Dictionary<string, SamAccount> localAccounts = new Dictionary<string, SamAccount>();
        public byte[] bootKey;

        public Sam() 
        { 
            
        }
    }
}
