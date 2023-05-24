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
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Forms
{
	public enum FormStyle
	{
        Default = -1,
		Classical = 0,
        Modern,
		ModernRed,
        [Obsolete("New color chart does not use Marroon anymore")]
        ModernMarroon,
        ModernWhite,
        ModernBlack,
        ModernAccent1,
        ModernAccent2,
        ModernAccent1Red,
        ModernAccent2Red
    }

    public enum ControlType
    {
        Unknown,
        Basic,
        Fixed,
        Link,
        Heading
    }

    public static class Forms
    {
        public static FormStyle DefaultStyle = FormStyle.Classical;

        private static FontFamily FixedFontFamily = new FontFamily("Consolas");

        private static Font OldTextFont = new Font("Microsoft Sans Serif", 8, FontStyle.Regular);
        private static Font OldHeadingFont = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);

        public static Color BlackColor = Color.FromArgb(0, 0, 0);
        public static Color GreyColor = Color.FromArgb(64, 64, 64);

        public static Color WhiteColor = Color.FromArgb(255, 255, 255);
        public static Color DarkWhiteColor = Color.FromArgb(240, 240, 240);

        public static Color ClassicColor = Color.FromArgb(240, 240, 240);
        public static Color DarkClassicColor = Color.FromArgb(218, 218, 218);

        public static Color RedColor = Color.FromArgb(0xD8, 0x0A, 0x1D);
        public static Color MarroonColor = Color.FromArgb(0xAE, 0x8D, 0x80);

        public static Color DarkRedColor = Color.FromArgb(153, 7, 20);
        public static Color DarkMarroonColor = Color.FromArgb(123, 100, 91);

        public static Color Accent1Color = Color.FromArgb(0x10, 0x2C, 0x43);
        public static Color Accent2Color = Color.FromArgb(0x2C, 0x4D, 0x60);


        public static ControlType GuessControlType(Control control)
        {
            ControlType result = ControlType.Unknown;

            if (control is LinkLabel)
            {
                result = ControlType.Link;
            }
            else if (control.Font.FontFamily.Equals(FixedFontFamily))
            {
                result = ControlType.Fixed;
            }
            else
            { 
                if (control.Font.Size <= 12)
                {
                    result = ControlType.Basic;
                }
                else if (control.Font.Size >= 14)
                {
                    result = ControlType.Heading;
                }
            }

            return result;
        }

        public static Font GetTextFont(FormStyle style, ControlType controlType)
        {
            Font result = null;

            if (style < FormStyle.Modern)
            {
                switch (controlType)
                {
                    case ControlType.Basic:
                    case ControlType.Link:
                        result = OldTextFont;
                        break;
                    case ControlType.Heading:
                        result = OldHeadingFont;
                        break;
                }
            }

            return result;
        }

        public static Color GetTextColor(FormStyle style, ControlType controlType)
        {
            Color result = Color.Transparent;

            if (style >= FormStyle.Modern)
            {
                switch (controlType)
                {
                    case ControlType.Basic:
                        result = BlackColor;
                        break;

                    case ControlType.Heading:
                        switch (style)
                        {
                            case FormStyle.Modern:
                                result = RedColor;
                                break;

                            case FormStyle.ModernAccent1:
                            case FormStyle.ModernAccent1Red:
                            case FormStyle.ModernAccent2:
                            case FormStyle.ModernAccent2Red:
                                result = WhiteColor;
                                break;

                            case FormStyle.ModernRed:
                            case FormStyle.ModernMarroon:
                            case FormStyle.ModernBlack:
                                result = WhiteColor;
                                break;

                            case FormStyle.ModernWhite:
                                result = BlackColor;
                                break;
                        }
                        break;

                    case ControlType.Link:
                        switch (style)
                        {
                            case FormStyle.Modern:
                            case FormStyle.ModernRed:
                                result = RedColor;
                                break;

                            case FormStyle.ModernAccent1Red:
                            case FormStyle.ModernAccent1:
                                result = Accent1Color;
                                break;

                            case FormStyle.ModernAccent2Red:
                            case FormStyle.ModernAccent2:
                                result = Accent2Color;
                                break;

                            case FormStyle.ModernMarroon:
                                result = DarkMarroonColor;
                                break;

                            case FormStyle.ModernWhite:
                            case FormStyle.ModernBlack:
                                result = GreyColor;
                                break;
                        }
                        break;
                }
            }

            return result;
        }

        public static Color GetHeaderColor(FormStyle style)
        {
            Color result = Color.Transparent;

            switch (style)
            {
                case FormStyle.ModernRed:
                    result = RedColor;
                    break;

                case FormStyle.ModernMarroon:
                    result = MarroonColor;
                    break;

                case FormStyle.ModernBlack:
                    result = BlackColor;
                    break;

                case FormStyle.ModernAccent1:
                case FormStyle.ModernAccent1Red:
                    result = Accent1Color;
                    break;

                case FormStyle.ModernAccent2:
                case FormStyle.ModernAccent2Red:
                    result = Accent2Color;
                    break;
            }

            return result;
        }

        public static void ApplyStyle(Control.ControlCollection controls, FormStyle style)
        {
            foreach (Control control in controls)
            {
                if (control is Panel)
                {
                    if (control.Name.ToLower().Contains("header"))
                    {
                        Color backColor = GetHeaderColor(style);
                        if (backColor != Color.Transparent)
                            control.BackColor = backColor;
                    }
                }

                if (control is PictureBox)
                {
                    if (control.Name.ToLower().Contains("logowhite"))
                    {
                        switch (style)
                        {
                            case FormStyle.ModernRed:
                            case FormStyle.ModernMarroon:
                            case FormStyle.ModernBlack:
                            case FormStyle.ModernAccent1:
                            case FormStyle.ModernAccent2:
                                control.Visible = true;
                                break;
                            default:
                                control.Visible = false;
                                break;
                        }
                    }

                    if (control.Name.ToLower().Contains("logocolor"))
                    {
                        switch (style)
                        {
                            case FormStyle.ModernRed:
                            case FormStyle.ModernMarroon:
                            case FormStyle.ModernBlack:
                            case FormStyle.ModernAccent1:
                            case FormStyle.ModernAccent2:
                                control.Visible = false;
                                break;
                            default:
                                control.Visible = true;
                                break;
                        }
                    }
                }

                ControlType type = GuessControlType(control);

                Font font = GetTextFont(style, type);

                if (font != null)
                {
                    control.Font = font;
                }

                Color foreColor = GetTextColor(style, type);

                if (foreColor != Color.Transparent)
                {
                    control.ForeColor = foreColor;
                    /* Link */
                    if (control is LinkLabel)
                    {
                        LinkLabel link = (LinkLabel)control;
                        link.ActiveLinkColor = link.ForeColor;
                        link.LinkColor = link.ForeColor;
                        link.VisitedLinkColor = link.ForeColor;
                    }
                }

                if (control.HasChildren)
                    ApplyStyle(control.Controls, style);
            }
        }

        public static void ApplyStyle(Form form, FormStyle style)
        {
            form.Font = GetTextFont(style, ControlType.Basic);
            ApplyStyle(form.Controls, style);
        }
    }
}
