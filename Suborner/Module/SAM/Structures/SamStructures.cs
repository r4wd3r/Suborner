using System.Runtime.InteropServices;
using static Suborner.Natives;

namespace Suborner.Module.SAM
{
    /// <summary>
    /// Struct <c>SAM_ACCOUNT_F</c> models the structure of the F registry key for local users stored in the SAM.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SAM_ACCOUNT_F
    {
        public byte[] Unknown1;             // 8 bytes.
        public byte[] LastLogon;            // 8 bytes. NT Time Format. Nulls if never logged on.
        public byte[] Unknown2;             // 8 bytes. Always zero?
        public byte[] PasswordLastSet;      // 8 bytes. NT Time Format. Nulls if never changed.
        public byte[] AccountExpires;       // 8 bytes. NT Time Format. Nulls if not.
        public byte[] LastIncorrectPwd;     // 8 bytes. NT Time Format. Nulls if not.
        public byte[] FRid;                 // 4 bytes. RID used for primary access token generation.
        public byte[] Unknown3;             // 4 bytes. Always 0x01, 0x02?
        public byte[] Unknown4;             // 4 bytes. Always 0x01, 0x02?

    }

    /// <summary>
    /// Struct <c>SAM_ACCOUNT_V_ENTRY</c> models the data that is stored in the V object for local accounts 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SAM_ACCOUNT_V_ENTRY
    {
        public int offset;
        public int length;
        public int unknown;
        public byte[] value;
    }
    /// <summary>
    /// Struct <c>SAM_ACCOUNT_V</c> models the structure of the V registry key for local users stored in the SAM.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SAM_ACCOUNT_V
    {
        public SAM_ACCOUNT_V_ENTRY Permissions;
        public SAM_ACCOUNT_V_ENTRY Username;
        public SAM_ACCOUNT_V_ENTRY Fullname;
        public SAM_ACCOUNT_V_ENTRY Comment;
        public SAM_ACCOUNT_V_ENTRY UserComment;
        public SAM_ACCOUNT_V_ENTRY Unknown2;
        public SAM_ACCOUNT_V_ENTRY Homedir;
        public SAM_ACCOUNT_V_ENTRY HomedirConnect;
        public SAM_ACCOUNT_V_ENTRY ScriptPath;
        public SAM_ACCOUNT_V_ENTRY ProfilePath;
        public SAM_ACCOUNT_V_ENTRY Workstations;
        public SAM_ACCOUNT_V_ENTRY HoursAllowed;
        public SAM_ACCOUNT_V_ENTRY Unknown3;
        public SAM_ACCOUNT_V_ENTRY LMHash;
        public SAM_ACCOUNT_V_ENTRY NTLMHash;
        public SAM_ACCOUNT_V_ENTRY NTLMHistory;
        public SAM_ACCOUNT_V_ENTRY LMHistory;
    }
    enum DOMAIN_SERVER_ENABLE_STATE
    {
        DomainServerEnabled = 1,
        DomainServerDisabled
    }

    enum DOMAIN_SERVER_ROLE
    {
        DomainServerRoleBackup = 2,
        DomainServerRolePrimary = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SAM_ENTRY
    {
        public uint offset;
        public uint lenght;
        uint unk;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SAM_HASH
    {
        public ushort PEKID;
        public ushort Revision;
        [MarshalAs(UnmanagedType.ByValArray)]
        public byte[] data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SAM_HASH_AES
    {
        public ushort PEKID;
        public ushort Revision;
        public uint dataOffset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Salt;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] // Initially did not have SizeConst
        public byte[] data; // Data
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SAM_KEY_DATA
    {
        public uint Revision;
        uint Length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Salt;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Key;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        byte[] CheckSum;
        uint unk0;
        uint unk1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SAM_KEY_DATA_AES
    {
        uint Revision; // 2
        uint Length;
        uint CheckLen;
        public uint DataLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Salt;
        [MarshalAs(UnmanagedType.ByValArray)]
        public byte[] data; // Data, then Check
    }

    /// <summary>
    /// Struct <c>DOMAIN_ACCOUNT_F</c> models the data for the Domain Account F structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DOMAIN_ACCOUNT_F
    {
        public ushort Revision;
        ushort unk0;
        uint unk1;
        OLD_LARGE_INTEGER CreationTime;
        OLD_LARGE_INTEGER DomainModifiedCount;
        OLD_LARGE_INTEGER MaxPasswordAge;
        OLD_LARGE_INTEGER MinPasswordAge;
        OLD_LARGE_INTEGER ForceLogoff;
        OLD_LARGE_INTEGER LockoutDuration;
        OLD_LARGE_INTEGER LockoutObservationWindow;
        OLD_LARGE_INTEGER ModifiedCountAtLastPromotion;
        public uint NextRid;
        uint PasswordProperties;
        ushort MinPasswordLength;
        ushort PasswordHistoryLength;
        ushort LockoutThreshold;
        DOMAIN_SERVER_ENABLE_STATE ServerState;
        DOMAIN_SERVER_ROLE ServerRole;
        bool UasCompatibilityRequired;
        uint unk2;
        public SAM_KEY_DATA keys1;
        SAM_KEY_DATA keys2;
        uint unk3;
        uint unk4;
    }


}
