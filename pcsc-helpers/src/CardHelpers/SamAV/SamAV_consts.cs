namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        public const byte CLA = 0x80;

        public enum INS : byte
        {
            ActivateOfflineKey = 0x01,
            AuthenticatePicc = 0x0A,
            LockUnlock = 0x10,
            PKIGenerateKeyPair = 0x15,
            PKIGenerateSignature = 0x16,
            PKIGenerateHash = 0x17,
            PKIExportPublicKey = 0x18,
            PKIImportKey = 0x19,
            PKISendSignature = 0x1A,
            PKIVerifySignature = 0x1B,
            PKIUpdateKeyEntry = 0x1D,
            PKIExportPrivateKey = 0x1F,

            SelectApplication = 0x5A,
            VerifyMac = 0x5C,

            GetKUCEntry = 0x6C,
            GenerateMac = 0x7C,
            AuthenticateHost = 0xA4,

            ChangeKeyEntry = 0xC1,
            ChangeKeyPicc = 0xC4,
            ChangeKUCEntry = 0xCC,

            DumpSessionKey = 0xD5,
            DumpSecretKey = 0xD6,
            DecipherData = 0xDD,
            CipherData = 0xED
        }

        public static readonly byte[] BlankKey64 = new byte[8];
        public static readonly byte[] BlankKey128 = new byte[16];
        public static readonly byte[] BlankKey192 = new byte[24];
    }
}
