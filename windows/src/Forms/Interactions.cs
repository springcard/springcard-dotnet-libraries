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
using System.Drawing;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Forms
{
    public class CPasswordInteraction : IPasswordInteraction
    {
        bool IPasswordInteraction.CreatePassword(CPasswordInteractionSettings Settings, out string Password)
        {
            if (Settings == null)
                Settings = CPasswordInteractionSettings.Create_Password();
            return CreatePasswordForm.Display(Settings, out Password);
        }

        bool IPasswordInteraction.EnterPassword(CPasswordInteractionSettings Settings, out string Password)
        {
            if (Settings == null)
                Settings = CPasswordInteractionSettings.Create_Password();
            return EnterPasswordForm.Display(Settings, out Password);
        }
    }

    public class CPassphraseInteraction : IPasswordInteraction
    {
        bool IPasswordInteraction.CreatePassword(CPasswordInteractionSettings Settings, out string Password)
        {
            if (Settings == null)
                Settings = CPasswordInteractionSettings.Create_Passphrase();
            return CreatePassphraseForm.Display(Settings, out Password);
        }
        bool IPasswordInteraction.EnterPassword(CPasswordInteractionSettings Settings, out string Password)
        {
            if (Settings == null)
                Settings = CPasswordInteractionSettings.Create_Passphrase();
            return EnterPassphraseForm.Display(Settings, out Password);
        }

    }
}