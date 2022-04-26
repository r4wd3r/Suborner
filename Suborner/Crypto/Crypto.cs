using Suborner.Core;
using Suborner.Module;
using Suborner.Module.SAM;
using Suborner.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using static Suborner.Natives;

namespace Suborner.Crypto
{
    public static class Crypto
    {
        const int SYSKEY_LENGTH = 16;
        const int SAM_KEY_DATA_SALT_LENGTH = 16;
        const int SAM_KEY_DATA_KEY_LENGTH = 16;

        const string LSA_QWERTY = "!@#$%^&*()qwertyUIOPAzxcvbnmQQQQQQQQQQQQ)(*@&%\0";
        const string LSA_0123 = "0123456789012345678901234567890123456789\0";

        const uint MD5_DIGEST_LENGTH = 16;

        public static byte[] EncryptPasswordToSamV(int rid, string password, DOMAIN_ACCOUNT_F domAccF)
        {
            byte[] encryptedPassword = null;
            byte[] samKey;
            byte[] sysKey = SubornerContext.Instance.SysKey;
            

            //1. Differentiate if < Windows 10 v1607 or >

            //2. Calculate NTLM Hash
            string NTLMHash = NTLM.CalculateNTLM(password).Trim();
            Printer.PrintInfo(String.Format("NTLM Hash for password: {0}", NTLMHash));

            //3. Divide NTLM Hash in 2 (NTLMPart1 and NTLMPart2)
            byte[] NTLMPart1 = Utility.StringToByteArray(NTLMHash.Substring(0, 16));
            byte[] NTLMPart2 = Utility.StringToByteArray(NTLMHash.Substring(16, 16));   // TODO: Confirm that this utility work!

            //4. Calculate DES Keys 1 and 2 for each part NTLM_1 and NTLM_2
            List<byte> DESKey1 = new List<byte>();
            List<byte> DESKey2 = new List<byte>();
            RidToKey(rid, ref DESKey1, ref DESKey2);

            //5. DES Encrypt each part of NTLM with each key and concat them
            byte[] NTLMPart1DES = ObfuscateHashPart(NTLMPart1, DESKey1);
            byte[] NTLMPart2DES = ObfuscateHashPart(NTLMPart2, DESKey2);
            byte[] DESHash = NTLMPart1DES.Concat(NTLMPart2DES).ToArray();

            Printer.PrintDebug("Calculated DES Hash Key: " + Utility.ByteArrayToString(DESHash));

            //6. Calculate the SAM Key from SysKey
            samKey = CalculateSamKey(sysKey,domAccF);
            Printer.PrintDebug("Calculated SAM Key: " + Utility.ByteArrayToString(samKey));

            //7. Calculate SAM Encrypted Hash
            encryptedPassword = EncryptSamNTHash(rid, DESHash, samKey, domAccF);
            return encryptedPassword;
        }

        private static byte[] EncryptSamNTHash(int rid, byte[] DESHash, byte[] samKey, DOMAIN_ACCOUNT_F domAccF)
        {
            // TODO: Re-implement this to craft both LM/NTLM if needed :)
            const string LSA_NTPASSWORD = "NTPASSWORD\0",
            LSA_LMPASSWORD = "LMPASSWORD\0",
            LSA_NTPASSWORDHISTORY = "NTPASSWORDHISTORY",
            LSA_LMPASSWORDHISTORY = "LMPASSWORDHISTORY",
            LMHASH = "aad3b435b51404eeaad3b435b51404ee",
            NTHASH = "31d6cfe0d16ae931b73c59d7e0c089c0"; 

            byte[] encryptedHash = null;
            IntPtr pDomAccF = Marshal.AllocHGlobal(Marshal.SizeOf(domAccF));
            Marshal.StructureToPtr(domAccF, pDomAccF, false);

            switch (domAccF.Revision)
            {
                case 2:
                case 3:
                    switch (domAccF.keys1.Revision)
                    {
                        case 1:        //  < Windows 10 v1607
                            SAM_HASH samHash = new SAM_HASH();
                            samHash.PEKID = 1; //?
                            samHash.Revision = 1;

                            byte[] NTHashDecryptionKey = new byte[SAM_KEY_DATA_KEY_LENGTH + sizeof(uint) + LSA_NTPASSWORD.Length + LSA_0123.Length];

                            byte[] hexBitRid = BitConverter.GetBytes(rid);
                            //Array.Reverse(hexBitRid);

                            // Craft NTHashDecryption key with SAM Key, RID and the NTPASSWORD string
                            NTHashDecryptionKey = samKey.Concat(hexBitRid).Concat(Encoding.ASCII.GetBytes(LSA_NTPASSWORD)).ToArray();
                            Printer.PrintDebug(String.Format("NT Key about to MD5: {0}", Utility.ByteArrayToString(NTHashDecryptionKey))); ;

                            MD5 md5 = new MD5CryptoServiceProvider();
                            byte[] MD5NTHashDecryptionKey = md5.ComputeHash(NTHashDecryptionKey);

                            samHash.data = RC4EncryptDecrypt(MD5NTHashDecryptionKey, DESHash);

                            Printer.PrintDebug(String.Format("Encrypt RC4. Data:{0}", Utility.ByteArrayToString(samHash.data)));
                            IntPtr pSamHash = Marshal.AllocHGlobal(Marshal.SizeOf(samHash));
                            encryptedHash = new byte[Marshal.SizeOf(samHash)];
                            Marshal.StructureToPtr(samHash, pSamHash, false); 
                            Marshal.Copy(pSamHash, encryptedHash, 0, Marshal.SizeOf(samHash));
                            Marshal.FreeHGlobal(pSamHash);

                            Printer.PrintDebug("Until here for now");
                            break;
                        case 2:        // >= Windows 10 v1607
                            SAM_HASH_AES samHashAes = new SAM_HASH_AES();
                            samHashAes.PEKID = 2;
                            samHashAes.Revision = 2;
                            samHashAes.dataOffset = 16;

                            // Calculate stored IV
                            byte[] newHashIV = new byte[16];
                            samHashAes.data = EncryptAES_CBC(DESHash, samKey, out newHashIV);
                            samHashAes.Salt = newHashIV;
                            Printer.PrintDebug(String.Format("Encrypt AES. IV:{0} Data:{1}", Utility.ByteArrayToString(samHashAes.Salt), Utility.ByteArrayToString(samHashAes.data)));
                            

                            IntPtr pSamHashAes = Marshal.AllocHGlobal(Marshal.SizeOf(samHashAes));
                            encryptedHash = new byte[Marshal.SizeOf(samHashAes)];
                            Marshal.StructureToPtr(samHashAes, pSamHashAes, false); 
                            Marshal.Copy(pSamHashAes, encryptedHash, 0, Marshal.SizeOf(samHashAes));
                            Marshal.FreeHGlobal(pSamHashAes);

                            break;
                        default:
                            Printer.PrintError(String.Format("Error: Unknow Struct Key revision (%u)", domAccF.keys1.Revision));
                            break;
                    }
                    break;
                default:
                    Printer.PrintError(String.Format("Unknow F revision (%hu)", domAccF.Revision));
                    break;
            }
            if (encryptedHash == null)
            {
                Printer.PrintError("Error calculating the SAM Key");
                System.Environment.Exit(1);
            }
            return encryptedHash;
        }

        private static byte[] CalculateSamKey(byte[] sysKey, DOMAIN_ACCOUNT_F domAccF)
        {
            byte[] samKey = null;
            IntPtr pDomAccF = Marshal.AllocHGlobal(Marshal.SizeOf(domAccF));
            Marshal.StructureToPtr(domAccF, pDomAccF, false);

            // Calculate SAM Key based on 
            switch (domAccF.Revision)
            {
                case 2:
                case 3:
                    switch (domAccF.keys1.Revision)
                    {
                        case 1:        //  < Windows 10 v1607
                            Printer.PrintDebug(String.Format("Detected MD5 Encryption mode "));
                            byte[] data = new byte[SAM_KEY_DATA_SALT_LENGTH + LSA_QWERTY.Length + SYSKEY_LENGTH + LSA_0123.Length];
                            data = domAccF.keys1.Salt.Concat(Encoding.Default.GetBytes(LSA_QWERTY)).Concat(sysKey).Concat(Encoding.Default.GetBytes(LSA_0123)).ToArray();
                            byte[] md5Out = MD5.Create().ComputeHash(data.ToArray());
                            samKey = RC4EncryptDecrypt(md5Out, domAccF.keys1.Key);
                            break;
                        case 2:        // >= Windows 10 v1607
                            Printer.PrintDebug("Detected AES Encryption mode");
                            SAM_KEY_DATA_AES AesKey = new SAM_KEY_DATA_AES();
                            IntPtr pAesKey = IntPtr.Add(pDomAccF, Utility.FieldOffset<DOMAIN_ACCOUNT_F>("keys1"));
                            AesKey = (SAM_KEY_DATA_AES)Marshal.PtrToStructure(pAesKey, typeof(SAM_KEY_DATA_AES));

                            AesKey.data = UpdateDataBytes(pAesKey, Utility.FieldOffset<SAM_KEY_DATA_AES>("data"), (int)AesKey.DataLen);
                            samKey = DecryptAES_CBC(AesKey.data, sysKey, AesKey.Salt).Take(SAM_KEY_DATA_KEY_LENGTH).ToArray();
                            break;
                        default:
                            Printer.PrintError(String.Format("Error: Unknow Struct Key revision (%u)", domAccF.keys1.Revision));
                            break;
                    }
                    break;
                default:
                    Printer.PrintError(String.Format("Unknow F revision (%hu)", domAccF.Revision));
                    break;
            }
            if (samKey == null) {
                Printer.PrintError("Error calculating the SAM Key");
                System.Environment.Exit(1);
            } 
            return samKey;
        }

        private static byte[] UpdateDataBytes(IntPtr start, int fieldoffset, int count)
        {
            byte[] res = new byte[count];
            IntPtr p = IntPtr.Add(start, fieldoffset);
            Marshal.Copy(p, res, 0, count);
            return res;
        }

        

        public static byte[] EncryptAES_CBC(byte[] value, byte[] key, out byte[] iv)
        {
            byte[] valuePadded;
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.Mode = CipherMode.CBC;
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Key = key;
            
            aes.GenerateIV();
            iv = aes.IV;
            aes.Padding = PaddingMode.Zeros;

            //TODO: Make this cleaner
            List<byte> manualPadding = new List<byte>();
            for (int i = 16; i > 0; i--)
            {
                manualPadding.Add(0x10);    // 1010 according to dumps
            }
            byte[] concat = new byte[value.Length + manualPadding.Count];
            System.Buffer.BlockCopy(value, 0, concat, 0, value.Length);
            System.Buffer.BlockCopy(manualPadding.ToArray(), 0, concat, value.Length, manualPadding.Count);
            value = concat;

            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                using (var ms = new MemoryStream())
                using (var cryptoStream = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(value, 0, value.Length);
                    cryptoStream.FlushFinalBlock();

                    return ms.ToArray();
                }
            }
        }

        public static byte[] DecryptAES_CBC(byte[] value, byte[] key, byte[] iv)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.IV = iv;
            aes.Padding = PaddingMode.Zeros;

            int tailLength = value.Length % 16;
            if (tailLength != 0)
            {
                List<byte> manualPadding = new List<byte>();
                for (int i = 16 - tailLength; i > 0; i--)
                {
                    manualPadding.Add(0x00);
                }
                byte[] concat = new byte[value.Length + manualPadding.Count];
                System.Buffer.BlockCopy(value, 0, concat, 0, value.Length);
                System.Buffer.BlockCopy(manualPadding.ToArray(), 0, concat, value.Length, manualPadding.Count);
                value = concat;
            }

            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                byte[] dest = decrypt.TransformFinalBlock(value, 0, value.Length);
                return dest;
            }
        }



        public static byte[] ComputeSha256(byte[] key, byte[] value)
        {
            MemoryStream memStream = new MemoryStream();
            memStream.Write(key, 0, key.Length);
            for (int i = 0; i < 1000; i++)
            {
                memStream.Write(value, 0, 32);
            }
            byte[] shaBase = memStream.ToArray();
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] newSha = sha256Hash.ComputeHash(shaBase);
                return newSha;
            }
        }



        //https://stackoverflow.com/questions/7217627/is-there-anything-wrong-with-this-rc4-encryption-code-in-c-sharp
        public static byte[] RC4EncryptDecrypt(byte[] pwd, byte[] data)
        {
            int a, i, j, k, tmp;
            int[] key, box;
            byte[] cipher;

            key = new int[256];
            box = new int[256];
            cipher = new byte[data.Length];

            for (i = 0; i < 256; i++)
            {
                key[i] = pwd[i % pwd.Length];
                box[i] = i;
            }
            for (j = i = 0; i < 256; i++)
            {
                j = (j + box[i] + key[i]) % 256;
                tmp = box[i];
                box[i] = box[j];
                box[j] = tmp;
            }
            for (a = j = i = 0; i < data.Length; i++)
            {
                a++;
                a %= 256;
                j += box[a];
                j %= 256;
                tmp = box[a];
                box[a] = box[j];
                box[j] = tmp;
                k = box[((box[a] + box[j]) % 256)];
                cipher[i] = (byte)(data[i] ^ k);
            }
            return cipher;
        }

        //method from SidToKey - https://github.com/woanware/ForensicUserInfo/blob/master/Source/SamParser.cs
        private static void RidToKey(int rid, ref List<byte> key1, ref List<byte> key2)
        {
            List<byte> temp1 = new List<byte>();

            byte temp = (byte)(rid & 0xFF);
            temp1.Add(temp);

            temp = (byte)(((rid >> 8) & 0xFF));
            temp1.Add(temp);

            temp = (byte)(((rid >> 16) & 0xFF));
            temp1.Add(temp);

            temp = (byte)(((rid >> 24) & 0xFF));
            temp1.Add(temp);

            temp1.Add(temp1[0]);
            temp1.Add(temp1[1]);
            temp1.Add(temp1[2]);

            List<byte> temp2 = new List<byte>();
            temp2.Add(temp1[3]);
            temp2.Add(temp1[0]);
            temp2.Add(temp1[1]);
            temp2.Add(temp1[2]);

            temp2.Add(temp2[0]);
            temp2.Add(temp2[1]);
            temp2.Add(temp2[2]);

            key1 = TransformKey(temp1);
            key2 = TransformKey(temp2);
        }

        private static List<byte> TransformKey(List<byte> inputData)
        {
            List<byte> data = new List<byte>();
            data.Add(Convert.ToByte(((inputData[0] >> 1) & 0x7f) << 1));
            data.Add(Convert.ToByte(((inputData[0] & 0x01) << 6 | ((inputData[1] >> 2) & 0x3f)) << 1));
            data.Add(Convert.ToByte(((inputData[1] & 0x03) << 5 | ((inputData[2] >> 3) & 0x1f)) << 1));
            data.Add(Convert.ToByte(((inputData[2] & 0x07) << 4 | ((inputData[3] >> 4) & 0x0f)) << 1));
            data.Add(Convert.ToByte(((inputData[3] & 0x0f) << 3 | ((inputData[4] >> 5) & 0x07)) << 1));
            data.Add(Convert.ToByte(((inputData[4] & 0x1f) << 2 | ((inputData[5] >> 6) & 0x03)) << 1));
            data.Add(Convert.ToByte(((inputData[5] & 0x3f) << 1 | ((inputData[6] >> 7) & 0x01)) << 1));
            data.Add(Convert.ToByte((inputData[6] & 0x7f) << 1));
            return data;
        }

        //from https://github.com/woanware/ForensicUserInfo/blob/master/Source/SamParser.cs
        private static byte[] DeObfuscateHashPart(byte[] obfuscatedHash, List<byte> key)
        {
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            cryptoProvider.Padding = PaddingMode.None;
            cryptoProvider.Mode = CipherMode.ECB;
            ICryptoTransform transform = cryptoProvider.CreateDecryptor(key.ToArray(), new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            MemoryStream memoryStream = new MemoryStream(obfuscatedHash);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[obfuscatedHash.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            return plainTextBytes;
        }

        private static byte[] ObfuscateHashPart(byte[] hash, List<byte> key)
        {
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            cryptoProvider.Padding = PaddingMode.None;
            cryptoProvider.Mode = CipherMode.ECB;
            ICryptoTransform transform = cryptoProvider.CreateEncryptor(key.ToArray(), new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            MemoryStream memoryStream = new MemoryStream(hash);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[hash.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            return plainTextBytes;
        }



    }
}
