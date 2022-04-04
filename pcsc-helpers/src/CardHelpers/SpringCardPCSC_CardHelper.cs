/**
 *
 * \author
 *   Johann.D et al. / SpringCard
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Runtime.InteropServices;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.PCSC.CardHelpers
{
	public abstract class CardHelper
	{	
		protected ICardApduTransmitter Channel;

		public CardHelper(ICardApduTransmitter Channel)
		{
			this.Channel = Channel;
		}
		
		public abstract bool SelfTest();
	}	

	public class CardInfo : CardHelper
	{
		public CardInfo(ICardApduTransmitter Channel) : base (Channel)
		{
		}

		public byte[] GetUid()
		{
			CAPDU capdu = new CAPDU(0xFF, 0xCA, 0x00, 0x00, 0x00);
			RAPDU rapdu = Channel.Transmit(capdu);
			if ((rapdu != null) && (rapdu.SW == 0x9000))
				return rapdu.DataBytes;
			return null;
		}

		public override bool SelfTest()
		{
			return true;
		}
	}

	public interface IRandomGenerator
	{
		byte[] Get(int length);
	}

	public class DefaultRandomGenerator : IRandomGenerator
	{
		Random rand;

		public DefaultRandomGenerator()
		{
			rand = new Random();
		}

		public byte[] Get(int length)
		{
			byte[] result = new byte[length];
			for (int i = 0; i < result.Length; i++)
				result[i] = (byte)rand.Next(0x00, 0xFF);
			return result;
		}
	}

	public class DummyRandomGenerator : IRandomGenerator
	{
		byte state;

		public DummyRandomGenerator()
		{
			state = 0x00;
		}

		public void Set(byte state)
		{
			this.state = state;
		}

		public void Reset()
        {
			this.state = 0x00;
        }

		public byte[] Get(int length)
		{
			byte[] result = new byte[length];
			for (int i = 0; i < result.Length; i++)
				result[i] = state++;
			return result;
		}
	}



}
