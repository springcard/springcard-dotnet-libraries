/**h* SpringCard/PCSC_Utils
 *
 * NAME
 *   PCSC : PCSC_Utils
 * 
 * DESCRIPTION
 *   SpringCard's misc utilities for the PC/SC API
 *
 * COPYRIGHT
 *   Copyright (c) 2010-2015 SpringCard - www.springcard.com
 *
 * AUTHOR
 *   Johann.D / SpringCard
 *
 **/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardAnalysis
{
	public static class GetAttrib
	{
        private static void AddAttribBytes(SCardChannel cardChannel, Dictionary<string, string> result, uint AttrId, string AttrName)
        {
            byte[] b = cardChannel.GetAttrib(AttrId);
            if (b != null)
                result[AttrName] = BinConvert.ToHex(b);
        }
        private static void AddAttribString(SCardChannel cardChannel, Dictionary<string, string> result, uint AttrId, string AttrName)
        {
            byte[] b = cardChannel.GetAttrib(AttrId);
            if (b != null)
                result[AttrName] = StrUtils.ToStr(b);
        }
        private static void AddAttribDWord(SCardChannel cardChannel, Dictionary<string, string> result, uint AttrId, string AttrName)
        {
            byte[] b = cardChannel.GetAttrib(AttrId);
            if (b != null)
                result[AttrName] = BinConvert.ToHex(b);
        }

        public static Dictionary<string,string> Analysis(SCardChannel cardChannel)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            AddAttribBytes(cardChannel, result, SCARD.ATTR_ATR_STRING, "SCARD_ATTR_ATR_STRING");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CHANNEL_ID, "SCARD_ATTR_CHANNEL_ID");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CHARACTERISTICS, "SCARD_ATTR_CHARACTERISTICS");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_BWT, "SCARD_ATTR_CURRENT_BWT");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_CLK, "SCARD_ATTR_CURRENT_CLK");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_CWT, "SCARD_ATTR_CURRENT_CWT");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_D, "SCARD_ATTR_CURRENT_D");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_EBC_ENCODING, "SCARD_ATTR_CURRENT_EBC_ENCODING");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_F, "SCARD_ATTR_CURRENT_F");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_IFSC, "SCARD_ATTR_CURRENT_IFSC");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_IFSD, "SCARD_ATTR_CURRENT_IFSD");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_N, "SCARD_ATTR_CURRENT_N");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_PROTOCOL_TYPE, "SCARD_ATTR_CURRENT_PROTOCOL_TYPE");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_CURRENT_W, "SCARD_ATTR_CURRENT_W");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_DEFAULT_CLK, "SCARD_ATTR_DEFAULT_CLK");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_DEFAULT_DATA_RATE, "SCARD_ATTR_DEFAULT_DATA_RATE");
            AddAttribString(cardChannel, result, SCARD.ATTR_DEVICE_FRIENDLY_NAME, "SCARD_ATTR_DEVICE_FRIENDLY_NAME");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_DEVICE_IN_USE, "SCARD_ATTR_DEVICE_IN_USE");
            AddAttribString(cardChannel, result, SCARD.ATTR_DEVICE_SYSTEM_NAME, "SCARD_ATTR_DEVICE_SYSTEM_NAME");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_DEVICE_UNIT, "SCARD_ATTR_DEVICE_UNIT");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_ICC_INTERFACE_STATUS, "SCARD_ATTR_ICC_INTERFACE_STATUS");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_ICC_PRESENCE, "SCARD_ATTR_ICC_PRESENCE");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_ICC_TYPE_PER_ATR, "SCARD_ATTR_ICC_TYPE_PER_ATR");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_MAX_CLK, "SCARD_ATTR_MAX_CLK");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_MAX_DATA_RATE, "SCARD_ATTR_MAX_DATA_RATE");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_MAX_IFSD, "SCARD_ATTR_MAX_IFSD");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_POWER_MGMT_SUPPORT, "SCARD_ATTR_POWER_MGMT_SUPPORT");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_PROTOCOL_TYPES, "SCARD_ATTR_PROTOCOL_TYPES");
            AddAttribString(cardChannel, result, SCARD.ATTR_VENDOR_IFD_SERIAL_NO, "SCARD_ATTR_VENDOR_IFD_SERIAL_NO");
            AddAttribString(cardChannel, result, SCARD.ATTR_VENDOR_IFD_TYPE, "SCARD_ATTR_VENDOR_IFD_TYPE");
            AddAttribDWord(cardChannel, result, SCARD.ATTR_VENDOR_IFD_VERSION, "SCARD_ATTR_VENDOR_IFD_VERSION");
            AddAttribString(cardChannel, result, SCARD.ATTR_VENDOR_NAME, "SCARD_ATTR_VENDOR_NAME");

            foreach (KeyValuePair<string,string> entry in result)
            {
                Logger.Debug("{0}: {1}", entry.Key, entry.Value);
            }

            return result;
        }
	}
}
