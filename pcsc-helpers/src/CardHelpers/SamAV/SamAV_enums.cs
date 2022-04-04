namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        public enum SamVersionE
        {
            Unknown,
            AV1,
            AV1_on_AV2,
            AV2,
            AV3_unactive,
            AV3
        }

        public enum AuthTypeE
        {
            TDES_CRC16 = 0,
            TDES_CRC32 = 1,
            AES = 2,
            MIFARE = 3
        }

        public enum ResultE
        {
            NotYetImplemented = -3,
            Continue = -2,
            NothingToDo = -1,
            Success = 0,
            CommunicationError,
            InternalError,
            VersionUnknown,
            InvalidInitialState,
            ExecutionFailed,
            UnexpectedStatusWord,
            InvalidResponseData,
            InvalidParameters,
            SecurityError
        }
    }
}
