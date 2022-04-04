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
        public enum ExpectE
        {
            Success,
            Continue,
            SuccessOrContinue,
            DontCare
        }

        private bool Command(CAPDU capdu, out RAPDU rapdu)
        {
            rapdu = Transmit(capdu);

            if (rapdu == null)
            {
                OnCommunicationError();
                return false;
            }

            return true;
        }

        private bool Command(CAPDU capdu, out RAPDU rapdu, ExpectE expect, int minLength)
        {
            if (!Command(capdu, out rapdu, expect))
                return false;
            if ((rapdu == null) || (rapdu.DataBytes == null) || (rapdu.DataBytes.Length < minLength))
            {
                LastError = ResultE.InvalidResponseData;
                return false;
            }
            return true;
        }

        private bool Command(CAPDU capdu, out RAPDU rapdu, ExpectE expect)
        {
            rapdu = Transmit(capdu);

            if (rapdu == null)
            {
                OnCommunicationError();
                LastError = ResultE.CommunicationError;
                return false;
            }

            switch (rapdu.SW)
            {
                case 0x9000:
                    if (expect != ExpectE.Continue)
                    {
                        LastError = ResultE.Success;
                        return true;
                    }
                    break;
                    
                case 0x90AF:
                    if (expect != ExpectE.Success)
                    {
                        LastError = ResultE.Continue;
                        return true;
                    }
                    break;

                default:
                    if (expect == ExpectE.DontCare)
                    {
                        LastError = ResultE.Success;
                        return true;
                    }
                    break;
            }
            
            return OnStatusWordError(rapdu.SW);
        }

        private ResultE CommandEx(INS ins, byte P1, byte P2, byte[] in_data, bool send_le, out byte[] out_data, out ushort out_sw, ExpectE expect)
        {
            out_data = null;
            out_sw = 0xFFFF;

            CAPDU capdu;

            if (in_data != null)
            {
                if (send_le)
                {
                    capdu = new CAPDU(CLA, (byte)ins, P1, P2, in_data, 0x00);
                }
                else
                {
                    capdu = new CAPDU(CLA, (byte)ins, P1, P2, in_data);
                }
            }
            else
            {
                capdu = new CAPDU(CLA, (byte)ins, P1, P2, 0x00);
            }

            RAPDU rapdu = Transmit(capdu);

            if (rapdu == null)
            {
                OnCommunicationError();
                return ResultE.CommunicationError;
            }

            out_data = rapdu.DataBytes;
            out_sw = rapdu.SW;

            ResultE result;

            switch (rapdu.SW)
            {
                case 0x9000:
                    if (expect != ExpectE.Continue)
                    {
                        result = ResultE.Success;
                    }
                    else
                    {
                        result = ResultE.UnexpectedStatusWord;
                    }
                    break;

                case 0x90AF:
                    if (expect != ExpectE.Success)
                    {
                        result = ResultE.Continue;
                    }
                    else
                    {
                        result = ResultE.UnexpectedStatusWord;
                    }
                    break;

                default:
                    if (expect != ExpectE.DontCare)
                    {
                        OnStatusWordError(rapdu.SW);
                        result = ResultE.UnexpectedStatusWord;
                    }
                    else
                    {
                        result = ResultE.Success;
                    }
                    break;
            }

            LastError = result;
            return result;
        }

        public ResultE Command(INS ins, byte P1, byte P2, byte[] in_data, ExpectE expect)
        {
            return CommandEx(ins, P1, P2, in_data, false, out byte[] out_data, out ushort out_sw, expect);
        }

        public ResultE Command(INS ins, byte P1, byte P2, byte[] in_data)
        {
            return CommandEx(ins, P1, P2, in_data, false, out byte[] out_data, out ushort out_sw, ExpectE.Success);
        }

        public ResultE Command(INS ins, byte P1, byte P2, out byte[] out_data, ExpectE expect)
        {
            return CommandEx(ins, P1, P2, null, true, out out_data, out ushort out_sw, expect);
        }

        public ResultE Command(INS ins, byte P1, byte P2, out byte[] out_data)
        {
            return CommandEx(ins, P1, P2, null, true, out out_data, out ushort out_sw, ExpectE.Success);
        }

        public ResultE Command(INS ins, byte P1, byte P2, ExpectE expect)
        {
            return CommandEx(ins, P1, P2, null, true, out byte[] out_data, out ushort out_sw, expect);
        }

        public ResultE Command(INS ins, byte P1, byte P2)
        {
            return CommandEx(ins, P1, P2, null, true, out byte[] out_data, out ushort out_sw, ExpectE.Success);
        }

        public ResultE Command(INS ins, byte P1, byte P2, byte[] in_data, bool send_le, out ushort out_sw)
        {
            return CommandEx(ins, P1, P2, in_data, send_le, out byte[] out_data, out out_sw, ExpectE.DontCare);
        }

        public ResultE Command(INS ins, byte P1, byte P2, byte[] in_data, bool send_le, out byte[] out_data, ExpectE expect)
        {
            return CommandEx(ins, P1, P2, in_data, send_le, out out_data, out ushort out_sw, expect);
        }





        public bool ChangePkiEntry(byte keyEntry, byte[] keyEntryData, bool setRandomKeyValues)
        {
            Logger.Debug("ChangePkiEntry(" + BinConvert.ToHex(keyEntry) + ")");

            if (keyEntryData == null)
            {
                Logger.Debug("No data!");
                userInteraction.Error("Pki Entry " + BinConvert.ToHex(keyEntry) + " has no data.");
                LastError = ResultE.InvalidParameters;
                return false;
            }

            if (keyEntryData.Length == 0)
            {
                Logger.Debug("Invalid length!");
                userInteraction.Error("Pki Entry " + BinConvert.ToHex(keyEntry) + " has an invalid length.");
                LastError = ResultE.InvalidParameters;
                return false;
            }

            if (setRandomKeyValues)
            {
                throw new Exception("This part is not implemented!");
            }

            CAPDU capdu;
            RAPDU rapdu;

            byte[] cmde;
            byte[] data;

            if (keyEntryData.Length > 248)
            {
                // write 14 header before data PKI_No to PKi_ipq
                //0000630000ff0100000400800080
                int offset = 13;
                if (keyEntry == 0x02)
                {
                    offset = 9;
                }

                data = BinUtils.Copy(keyEntryData, 0, offset);

                cmde = new byte[data.Length + 1];

                //cmde[0] = 0x0E;
                cmde[0] = keyEntry;
                data.CopyTo(cmde, 1);

                capdu = new CAPDU(CLA, 0x19, 0x00, 0xAF, cmde);
                if (!Command(capdu, out rapdu))
                    return false;

                // maximum two call to get full key
                if (rapdu.SW == 0x90AF)
                {
                    do
                    {
                        if ((keyEntryData.Length - offset) > 248)
                        {
                            cmde = BinUtils.Copy(keyEntryData, offset, 248);
                            capdu = new CAPDU(CLA, 0x19, 0x00, 0xAF, cmde);
                        }
                        else
                        {
                            cmde = BinUtils.Copy(keyEntryData, offset, keyEntryData.Length - offset);
                            capdu = new CAPDU(CLA, 0x19, 0x00, 0x00, cmde);
                        }

                        if (!Command(capdu, out rapdu))
                            return false;
                        
                        offset += 248;
                    }
                    while (rapdu.SW == 0x90AF);

                }
            }
            else
            {
                // rest of command
                data = BinUtils.Copy(keyEntryData, 0, keyEntryData.Length);
                cmde = new byte[data.Length + 1];
                data.CopyTo(cmde, 1);
                cmde[0] = (byte)keyEntryData.Length;

                capdu = new CAPDU(CLA, 0x19, 0x00, 0x00, cmde);

                if (!Command(capdu, out rapdu))
                    return false;
            }

            if ((rapdu.SW != 0x9000))
            {
                Logger.Debug("ChangePkiEntry(" + BinConvert.ToHex(keyEntry) + "): SW=" + BinConvert.ToHex(rapdu.SW));
				userInteraction.Error(string.Format("Failed to change PKI key entry {0:X02} (SAM error)", keyEntry));
                LastError = ResultE.ExecutionFailed;
				return false;
            }

            return true;
        }


    }
}
