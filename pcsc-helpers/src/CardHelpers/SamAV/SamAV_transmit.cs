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
        private RAPDU Transmit(CAPDU capdu)
        {
            Logger.Debug("SAM<{0}", capdu.AsString());
            RAPDU result = samReader.Transmit(capdu);
            if (result == null)
            {
                Logger.Debug("SAM> (PC/SC error)");
                OnCommunicationError();
            }
            else
            {
                Logger.Debug("SAM>{0}", result.AsString());
                _StatusWord = result.SW;
            }
            return result;
        }
    }
}
