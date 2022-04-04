using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Reflection;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        public bool Activate_Av2ToAv3(byte keyNo, byte keyVer, uint maxChainBlock, byte[] aesKeyValue)
        {
            return LockUnlock(LockUnlockMode.Activate, new byte[] { keyNo, keyVer, (byte)(maxChainBlock & 0x0FF), (byte)((maxChainBlock >> 8) & 0x0FF), (byte)((maxChainBlock >> 16) & 0x0FF) }, aesKeyValue);
        }

        public bool Activate_Av2ToAv3(byte keyNo, byte keyVer, byte[] aesKeyValue)
        {
            return Activate_Av2ToAv3(keyNo, keyVer, 0, aesKeyValue);
        }

        public bool Activate_Av1ToAv2()
        {
            Logger.Debug("Switching SAM from AV1 mode to AV2 mode");

            /* Verify we are authenticated */
            if (activeAuth == null)
            {
                Logger.Error("Authentication must be performed first");
                LastError = ResultE.InvalidInitialState;
                return false;
            }
            if ((activeAuth.KeyIdx != 0x00) || ((activeAuth.AuthType != AuthTypeE.TDES_CRC16) && (activeAuth.AuthType != AuthTypeE.AES)))
            {
                Logger.Error("Authentication shall be performed with key 00 in AV1 mode");
                LastError = ResultE.InvalidInitialState;
                return false;
            }

            /* 1 - Update key 0 */
            KeyEntry key0Entry = new KeyEntry();
            key0Entry.ValueA = BlankKey128;
            key0Entry.ValueB = BlankKey128;
            key0Entry.ValueC = BlankKey128;
            key0Entry.DesfireAid = 0x000000;
            key0Entry.DesfireKeyIdx = 0x00;
            key0Entry.ChangeKeyIdx = 0x00;
            key0Entry.ChangeKeyVersion = 0x00;
            key0Entry.CounterIdx = 0xFF; // No counter
            key0Entry.SET_HI = 0x20; // 20 (remember, we are still in AV1 mode)
            key0Entry.SET_LO = 0x00; // 00
            key0Entry.VersionA = 0x00;
            key0Entry.VersionB = 0x00;
            key0Entry.VersionC = 0x00;

            if (!ChangeKeyEntryAV1(0, key0Entry))
            {
                Logger.Error("Failed to change key entry #00 to AES");
                return false;
            }

            /* 2 - LockUnlock using key 0 */
            byte[] unlockData = new byte[5];
            if (!LockUnlock(LockUnlockMode.Activate, unlockData, BlankKey128))
            {
                Logger.Error("Failed to set the SAM in AV2 mode");
                return false;
            }

            return true;
        }

    }
}
