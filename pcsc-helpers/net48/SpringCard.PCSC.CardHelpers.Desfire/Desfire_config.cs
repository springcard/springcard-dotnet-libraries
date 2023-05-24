using SpringCard.LibCs;
using System;
using System.Security.Cryptography;


namespace SpringCard.PCSC.CardHelpers
{
    /// <summary>
    /// Description of DESFire_config.
    /// </summary>
    public partial class Desfire
    {
        public long SetVCMandatory()
        {
            byte[] b = new byte[1] { 0x08 };
            return this.SetConfiguration(0x00, b, (byte)b.Length);
        }
        public long SetPCMandatory()
        {
            byte[] b = new byte[1] { 0x04 };
            return this.SetConfiguration(0x00, b, (byte)b.Length);
        }
        public long EnableIsoRandomID()
        {
            byte[] b = new byte[1] { 0x02 };
            return this.SetConfiguration(0x00, b, (byte)b.Length);
        }
        public long DisableCommandFormat()
        {
            byte[] b = new byte[1] { 0x01 };
            return this.SetConfiguration(0x00, b, (byte)b.Length);
        }
        public long SetPiccAppDefaultKey(byte[] key, byte version)
        {
            byte[] b = new byte[key.Length + 1];

            if (b.Length > 25)
                return DF_PARAMETER_ERROR;

            Array.Copy(key, 0, b, 0, key.Length);
            key[key.Length] = version;

            return this.SetConfiguration(0x01, b, (byte)b.Length);
        }
        public long SetUserAts(byte[] ats)
        {
            /* enlarge iso exchange to 128 bytes */
            //byte[] ats = new byte[] { 0x06, 0x75, 0x77, 0x81, 0x02, 0x80 };
            //rc = this.SetConfiguration(0x02, ats, sizeof(ats));
            return this.SetConfiguration(0x02, ats, (byte)ats.Length);
        }
        public long SetUserSak(byte[] sak)
        {
            if( sak.Length != 2)
                return DF_PARAMETER_ERROR;

            return this.SetConfiguration(0x04, sak, (byte)sak.Length);
        }
        public long DisableChainedWriting()
        {
            byte[] b = new byte[2] { 0x00, 0x04 };
            return this.SetConfiguration(0x04, b, (byte)b.Length);
        }
        public long DisableEv1Secure()
        {
            byte[] b = new byte[2] { 0x00, 0x02 };
            return this.SetConfiguration(0x04, b, (byte)b.Length);
        }
        public long DisableD40Secure()
        {
            /* disable D40 */
            byte[] b = new byte[2] { 0x00, 0x01 };
            return this.SetConfiguration(0x04, b, (byte)b.Length);
        }
        public long SetVCConfiguration(byte card_size, byte pcSupport, byte MasterKeySetId, byte PDCaps25, byte PDCaps26, byte VCTID_Override)
        {
            return DF_OPERATION_OK;
        }
        public long SetVCID()
        {
            return DF_OPERATION_OK;
        }
        public long SetDelegatedApplication()
        {
            return DF_OPERATION_OK;
        }
        public long SetConventionnalApplication()
        {
            return DF_OPERATION_OK;
        }

        public long SetIsoMaxSize(UInt32 size)
        {
            MAX_FSC = size;
            return DF_OPERATION_OK;
        }
    }
}
