using Org.BouncyCastle.Math;

namespace SpringCard.LibCs.Crypto
{
    public abstract class AsymKey
    {
        internal static BigInteger BI(byte[] value)
        {
            value = BinUtils.Concat(0x00, value);
            return new BigInteger(value);
        }
    }

    public abstract class PublicKey : AsymKey
    {
        
    }
    public abstract class PrivateKey : AsymKey
    {

    }
}
