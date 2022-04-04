/**
 *
 * \ingroup Windows
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using Microsoft.Win32;
using System;
using System.Security.AccessControl;
using System.Security.Principal;
#if !NET5_0_OR_GREATER
using System.Windows.Forms;
#endif

namespace SpringCard.LibCs.Windows
{
    /**
	 * \brief Registry-based configuration
	 */
    public class RegistryApi
    {
        public static RegistrySecurity FreeRegistrySecurityAccessRule()
        {
            RegistrySecurity rs = new RegistrySecurity();
            SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            rs.AddAccessRule(new RegistryAccessRule(everyone,
                RegistryRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None, AccessControlType.Allow));

            return rs;
        }


    }
}

