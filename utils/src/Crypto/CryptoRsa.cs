using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using System.IO;

namespace SpringCard.LibCs.Crypto
{
    public class RSAPublicKey : PublicKey
    {
        public byte[] N { get; private set; }
        public byte[] E { get; private set; }

        private RSAPublicKey()
        {

        }

        public RSAPublicKey(byte[] N, byte[] E)
        {
            this.N = N;
            this.E = E;
        }

        public override string ToString()
        {
            string result = string.Format("E={0},N={1}", BinConvert.ToHex(E), BinConvert.ToHex(N));
            return result;
        }

        public static RSAPublicKey LoadFromPem(string FileName)
        {
            StreamReader inputStream = File.OpenText(FileName);
            PemReader pemReader = new PemReader(inputStream);
            AsymmetricKeyParameter keyParameter = (AsymmetricKeyParameter)pemReader.ReadObject();
            return Create(keyParameter);
        }

        public static RSAPublicKey Create(AsymmetricKeyParameter keyParameter)
        {
            if (keyParameter.GetType() != typeof(RsaKeyParameters))
                throw new PemException("Not a RSA public key");
            return Create((RsaKeyParameters)keyParameter);
        }

        public static RSAPublicKey Create(RsaKeyParameters publicKey)
        {
            RSAPublicKey result = new RSAPublicKey();

            result.N = publicKey.Modulus.ToByteArrayUnsigned();
            result.E = publicKey.Exponent.ToByteArrayUnsigned();

            return result;
        }
    }

    public class RSAPrivateKey : PrivateKey
    {
        public byte[] E { get; private set; }
        public byte[] P { get; private set; }
        public byte[] Q { get; private set; }

        public byte[] N
        {
            get
            {
                BigInteger _N = BI(P).Multiply(BI(Q));
                return _N.ToByteArrayUnsigned();
            }
        }

        public byte[] dP
        {
            get
            {
                BigInteger _dP = BI(E).ModPow(new BigInteger("-1"), BI(P).Subtract(new BigInteger("1")));
                return _dP.ToByteArrayUnsigned();
            }
        }

        public byte[] dQ
        {
            get
            {
                BigInteger _dQ = BI(E).ModPow(new BigInteger("-1"), BI(Q).Subtract(new BigInteger("1")));
                return _dQ.ToByteArrayUnsigned();
            }
        }

        public byte[] iPQ
        {
            get
            {
                BigInteger _iPQ = BI(P).ModPow(new BigInteger("-1"), BI(Q));
                return _iPQ.ToByteArrayUnsigned();
            }
        }

        private RSAPrivateKey()
        {

        }

        public RSAPrivateKey(byte[] P, byte[] Q, byte[] E)
        {
            this.E = E;

            if (BI(P).CompareTo(BI(Q)) > 0)
            {
                /* P shall be smaller than Q */
                this.Q = P;
                this.P = Q;
            }
            else
            {
                this.P = P;
                this.Q = Q;
            }
        }

        public RSAPublicKey GetPublicKey()
        {
            return new RSAPublicKey(N, E);
        }

        public override string ToString()
        {
            string result = string.Format("E={0},P={1},Q={2}", BinConvert.ToHex(E), BinConvert.ToHex(P), BinConvert.ToHex(Q));
            return result;
        }

        public static RSAPrivateKey LoadFromPem(string FileName)
        {
            StreamReader inputStream = File.OpenText(FileName);
            PemReader pemReader = new PemReader(inputStream);
            AsymmetricCipherKeyPair keyParameter = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            return Create(keyParameter);
        }

        public static RSAPrivateKey Create(AsymmetricCipherKeyPair keyParameter)
        {
            if (keyParameter.Private == null)
                throw new PemException("Not a private key");
            if (keyParameter.Private.GetType() != typeof(RsaPrivateCrtKeyParameters))
                throw new PemException("Not a RSA private key (CRT format expected)");
            if (keyParameter.Public.GetType() != typeof(RsaKeyParameters))
                throw new PemException("Not a RSA private key (public key missing)");
            return Create((RsaKeyParameters) keyParameter.Public, (RsaPrivateCrtKeyParameters) keyParameter.Private);
        }

        public static RSAPrivateKey Create(RsaKeyParameters publicKey, RsaPrivateCrtKeyParameters privateKey)
        {
            RSAPrivateKey result = new RSAPrivateKey();

            result.E = publicKey.Exponent.ToByteArrayUnsigned();

            if (privateKey.P.CompareTo(privateKey.Q) > 0)
            {
                /* P shall be smaller than Q */
                result.Q = privateKey.P.ToByteArrayUnsigned();
                result.P = privateKey.Q.ToByteArrayUnsigned();
            }
            else
            {
                result.P = privateKey.P.ToByteArrayUnsigned();
                result.Q = privateKey.Q.ToByteArrayUnsigned();
            }

            return result;
        }
    }
}
