using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_sign.
  /// </summary>
  public partial class Desfire
  {
    public long ReadSign(byte bAddr, ref byte[] pSignature, ref UInt32? iSignatureSize)
    {
      UInt32 recv_length = 0;
      long status;

      if ((pSignature == null) || (iSignatureSize == null))
      {
        return DFCARD_LIB_CALL_ERROR;
      }
      if (iSignatureSize < (DF_SIG_LENGTH_ENC + DF_SIG_LENGTH + 2))
      {
        return DFCARD_WRONG_LENGTH;
      }
      if (bAddr != 0x00)
      {
        return DFCARD_LIB_CALL_ERROR;
      }

      /* create the info block containing the command code */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_READ_SIGN;
      xfer_buffer[xfer_length++] = bAddr;

      if ((secure_mode != DF_SECURE_MODE_EV0) && (session_key_id >= 0x0000))
      {
        status = Command(0, NO_CHECK_RESPONSE_LENGTH | COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | APPEND_COMMAND_CMAC | WANTS_OPERATION_OK, true);
      }
      else
      {
        status = Command(0, COMPUTE_COMMAND_CMAC | WANTS_OPERATION_OK, true);
      }

      if (status != DF_MORE_OPERATION_OK)
      {
        return status;
      }

      recv_length = xfer_length;

      if (secure_mode != DF_SECURE_MODE_EV0)
      {
        recv_length = xfer_length - 1;

        byte[] data = new byte[recv_length];
        Array.ConstrainedCopy(xfer_buffer, 1, data, 0, (int)recv_length);

        DeCipherSP80038A(SesAuthENCKey, ref data, ref recv_length, true);

        /* remove cmac */
        if ((recv_length - 1) != DF_SIG_LENGTH_ENC)
        {
          status = DFCARD_WRONG_LENGTH;
        }
      }
      else
      {
        if ((recv_length - 1) != DF_SIG_LENGTH)
        {
          status = DFCARD_WRONG_LENGTH;
        }
      }

      return status;
    }
   
  }
}
