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
            Logger.Debug("<{0}", capdu.AsString());
            RAPDU result = samReader.Transmit(capdu);
            if (result == null)
            {
                Logger.Debug(">!");
                OnCommunicationError();
            }
            else
            {
                Logger.Debug(">{0}", result.AsString());
            }
            return result;
        }
    }
}
