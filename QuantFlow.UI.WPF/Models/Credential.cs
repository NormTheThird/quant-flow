namespace QuantFlow.UI.WPF.Models;

public class Credential : IDisposable
{
    private const int CRED_TYPE_GENERIC = 1;
    private const int CRED_PERSIST_LOCAL_MACHINE = 2;

    public string Target { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool Load()
    {
        if (CredRead(Target, CRED_TYPE_GENERIC, 0, out IntPtr credPtr))
            try
            {
                var cred = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                Username = cred.UserName;
                Password = Marshal.PtrToStringUni(cred.CredentialBlob, (int)cred.CredentialBlobSize / 2) ?? "";
                return true;
            }
            finally
            {
                CredFree(credPtr);
            }
        return false;
    }

    public void Save()
    {
        var byteCount = (Password.Length + 1) * 2;
        var credential = new CREDENTIAL
        {
            Type = CRED_TYPE_GENERIC,
            TargetName = Target,
            UserName = Username,
            CredentialBlob = Marshal.StringToCoTaskMemUni(Password),
            CredentialBlobSize = (uint)byteCount,
            Persist = CRED_PERSIST_LOCAL_MACHINE,
            AttributeCount = 0,
            Attributes = IntPtr.Zero,
            Comment = null,
            TargetAlias = null
        };

        try
        {
            if (!CredWrite(ref credential, 0))
                throw new Exception($"Failed to save credential. Error code: {Marshal.GetLastWin32Error()}");
        }
        finally
        {
            if (credential.CredentialBlob != IntPtr.Zero)
                Marshal.FreeCoTaskMem(credential.CredentialBlob);
        }
    }

    public void Delete()
    {
        CredDelete(Target, CRED_TYPE_GENERIC, 0);
    }

    public void Dispose() { }

    #region P/Invoke

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool CredDelete(string target, int type, int flags);

    [DllImport("advapi32.dll")]
    private static extern void CredFree([In] IntPtr buffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public uint Flags;
        public uint Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string UserName;
    }

    #endregion
}