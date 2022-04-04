/**h* SpringCard/PCSC_CcidOver
 *
 **/
using System;
using SpringCard.PCSC;

namespace SpringCard.PCSC.ZeroDriver
{
	public abstract class CCID
	{
		#region CCID constants
		public const byte EP_Control_To_RDR = 0x00;
		public const byte EP_Control_To_PC = 0x80;
		public const byte EP_Bulk_RDR_To_PC = 0x81;
		public const byte EP_Bulk_PC_To_RDR = 0x02;
		public const byte EP_Interrupt = 0x83;
		
		public const byte PC_TO_RDR_ICCPOWERON = 0x62;
		public const byte PC_TO_RDR_ICCPOWEROFF = 0x63;
		public const byte PC_TO_RDR_GETSLOTSTATUS = 0x65;
		public const byte PC_TO_RDR_XFRBLOCK = 0x6F;
		public const byte PC_TO_RDR_ESCAPE = 0x6B;
		
		public const byte RDR_TO_PC_DATABLOCK = 0x80;
		public const byte RDR_TO_PC_SLOTSTATUS = 0x81;
		public const byte RDR_TO_PC_ESCAPE = 0x83;
		
		public const byte GET_STATE = 0x00;
		public const byte GET_DESCRIPTOR = 0x06;
		public const byte SET_CONFIGURATION = 0x09;
		
		public const byte RDR_TO_PC_NOTIFYSLOTCHANGE = 0x50;

		/* Errors */
		public const byte ERR_SUCCESS = 0x81;
		public const byte ERR_UNKNOWN = 0x82;
		public const byte ERR_PARAMETERS = 0x83;
		public const byte ERR_PROTOCOL = 0x84;

		public const byte ERR_CMD_NOT_SUPPORTED = 0x00;
		public const byte ERR_BAD_LENGTH = 0x01;
		public const byte ERR_BAD_SLOT = 0x05;
		public const byte ERR_BAD_POWERSELECT = 0x07;
		public const byte ERR_BAD_PROTOCOLNUM = 0x07;
		public const byte ERR_BAD_CLOCKCOMMAND = 0x07;
		public const byte ERR_BAD_ABRFU_3B = 0x07;
		public const byte ERR_BAD_ABRFU_2B = 0x08;
		public const byte ERR_BAD_LEVELPARAMETER = 0x08;
		public const byte ERR_BAD_FIDI = 0x0A;
		public const byte ERR_BAD_T01CONVCHECKSUM = 0x0B;
		public const byte ERR_BAD_GUARDTIME = 0x0C;
		public const byte ERR_BAD_WAITINGINTEGER = 0x0D;
		public const byte ERR_BAD_CLOCKSTOP = 0x0E;
		public const byte ERR_BAD_IFSC = 0x0F;
		public const byte ERR_BAD_NAD = 0x10;

		/* Standard error codes */
		public const byte ERR_CMD_ABORTED = 0xFF;
		public const byte ERR_ICC_MUTE = 0xFE;
		public const byte ERR_XFR_PARITY_ERROR = 0xFD;
		public const byte ERR_XFR_OVERRUN = 0xFC;
		public const byte ERR_HW_ERROR = 0xFB;
		public const byte ERR_BAD_ATR_TS = 0xF8;
		public const byte ERR_BAD_ATR_TCK = 0xF7;
		public const byte ERR_ICC_PROTOCOL_NOT_SUPPORTED = 0xF6;
		public const byte ERR_ICC_CLASS_NOT_SUPPORTED = 0xF5;
		public const byte ERR_PROCEDURE_BYTE_CONFLICT = 0xF4;
		public const byte ERR_DEACTIVATED_PROTOCOL = 0xF3;
		public const byte ERR_BUSY_WITH_AUTO_SEQUENCE = 0xF2;
		public const byte ERR_PIN_TIMEOUT = 0xF0;

		public const byte ERR_PIN_CANCELLED = 0xEF;

		/* Do not use inside the reader itself, reserved for the driver and bridge system */
		public const byte ERR_CMD_SLOT_OR_READER_IDLE = 0xE1;

		public const byte ERR_CMD_SLOT_BUSY = 0xE0;

		/* Private error codes for the GemCore library */
		public const byte ERR_CMD_NOT_ABORTED = 0xC0;
		public const byte ERR_CARD_WANTS_RESYNCH = 0xC4;
		public const byte ERR_CARD_ABORTED = 0xC5;
		public const byte ERR_CARD_NOT_HEARING = 0xC6;
		public const byte ERR_CARD_IS_LOOPING = 0xC7;
		public const byte ERR_CARD_REMOVED = 0xC1;
		public const byte ERR_CARD_POWERED_DOWN = 0xC2;
		public const byte ERR_CARD_PROTOCOL_UNSET = 0xC3;
		public const byte ERR_COMM_OVERFLOW = 0xC8;
		public const byte ERR_COMM_FAILED = 0xC9;
		public const byte ERR_COMM_TIMEOUT = 0xCA;
		public const byte ERR_COMM_PROTOCOL = 0xCB;
		public const byte ERR_COMM_FORMAT = 0xCC;

		public const byte STATUS_ICC_MASK = 0x03;
		public const byte STATUS_ICC_PRESENT_ACTIVE = 0x00;
		public const byte STATUS_ICC_PRESENT_INACTIVE = 0x01;
		public const byte STATUS_ICC_ABSENT = 0x02;

		public const byte STATUS_COMMAND_MASK = 0xC0;
		public const byte STATUS_COMMAND_SUCCESS = 0x00;
		public const byte STATUS_COMMAND_FAILED = 0x40;
		public const byte STATUS_COMMAND_TIME_EXTENSION = 0x80;

		#endregion

		#region CCID blocks

		public class PC_to_RDR_Block
		{
			public byte Message;
			public byte Slot;
			public byte Sequence;
			public byte Param1;
			public byte Param2;
			public byte Param3;
			public byte[] Data;
			
			public PC_to_RDR_Block(byte[] buffer)
			{
				this.Message = buffer[0];
				this.Slot = buffer[5];
				this.Sequence = buffer[6];
				this.Param1 = buffer[7];
				this.Param2 = buffer[8];
				this.Param3 = buffer[9];
				if (buffer.Length > 10)
				{
					this.Data = new byte[buffer.Length - 10];
					Array.Copy(buffer, 10, this.Data, 0, this.Data.Length);
				}
			}
		}
		
		public class RDR_to_PC_Block
		{
			public byte Message;
			public byte Slot;
			public byte Sequence;
			public byte Status;
			public byte Error;
			public byte Chain;
			public byte[] Data;
            public bool Secure;
			
			public RDR_to_PC_Block(byte[] buffer)
			{
				this.Message = buffer[0];
				uint Length = 0;
                Secure = ((buffer[4] & 0x80) != 0) ? true : false;
                Length += buffer[4]; Length *= 256;
				Length += buffer[3]; Length *= 256;
				Length += buffer[2]; Length *= 256;
				Length += buffer[1];
				this.Slot = buffer[5];
				this.Sequence = buffer[6];
				this.Status = buffer[7];
				this.Error = buffer[8];
				this.Chain = buffer[9];
				if (Length > 0)
				{
					this.Data = new byte[Length];
					Array.Copy(buffer, 10, this.Data, 0, Length);
				}
			}	

        }

		#endregion
	}
}
