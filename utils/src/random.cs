/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2019 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Security.Cryptography;

namespace SpringCard.LibCs
{
    /**
	 * \brief Pseudo-random number generater
	 */
    public class PRNG
    {
        private static RandomNumberGenerator generator = RandomNumberGenerator.Create();

        public static byte[] Generate(int length)
        {
            byte[] result = new byte[length];
            generator.GetBytes(result);
            return result;
        }
    }
}

