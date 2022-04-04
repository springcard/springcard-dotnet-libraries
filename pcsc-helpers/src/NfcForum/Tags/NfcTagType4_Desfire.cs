/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 12/03/2012
 * Heure: 09:22
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Diagnostics;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.NfcForum.Tags
{
	/// <summary>
	/// Description of NfcTagType4Desfire.
	/// </summary>
	public partial class NfcTagType4 : NfcTag
	{
		public static bool IsDesfireEV1(SCardChannel channel)
		{
			bool is_desfire_ev1 = false;

			CAPDU capdu = new CAPDU(0x90, 0x60, 0x00, 0x00, 0x00);
			
			Logger.Trace("< " + capdu.AsString(" "));

			if (channel == null)
				Logger.Trace("<CACA>");

			RAPDU rapdu = channel.Transmit(capdu);

			if (rapdu == null)
            {
				Logger.Trace("Card removed?");
				return false;
			}

			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x91AF)
			{
				Logger.Trace("Desfire GetVersion function failed");
				return false;
			}
			
			if (rapdu.GetByte(3) > 0)
			{
				Logger.Trace("This is a Desfire EV1");
				is_desfire_ev1 = true;
			} else
			{
				Logger.Trace("This is a Desfire EV0");
			}
			
			capdu = new CAPDU(0x90, 0xAF, 0x00, 0x00, 0x00);
			
			Logger.Trace("< " + capdu.AsString(" "));

			rapdu = channel.Transmit(capdu);

			if (rapdu == null)
			{
				Logger.Trace("Card removed?");
				return false;
			}

			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x91AF)
			{
				Logger.Trace("Desfire GetVersion(2) function failed");
				return false;
			}

			capdu = new CAPDU(0x90, 0xAF, 0x00, 0x00, 0x00);
			
			Logger.Trace("< " + capdu.AsString(" "));

			rapdu = channel.Transmit(capdu);

			if (rapdu == null)
			{
				Logger.Trace("Card removed?");
				return false;
			}

			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9100)
			{
				Logger.Trace("Desfire GetVersion(3) function failed");
				return false;
			}

			return is_desfire_ev1;
		}
	}
}

