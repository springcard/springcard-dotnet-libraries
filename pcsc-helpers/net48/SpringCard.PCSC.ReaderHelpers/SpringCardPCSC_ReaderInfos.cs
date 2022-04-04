using SpringCard.LibCs;
using System.Collections.Generic;

namespace SpringCard.PCSC.ReaderHelpers
{
    /**
	 * \brief Static class that gives a direct access to PC/SC functions (SCard... provided by winscard.dll or libpcsclite)
	 *
	 **/
    public static class ReaderInfos
    {
        private static readonly string[] SpringCardVendorNames = new string[] { "SpringCard" };

        public static bool IsSpringCard(string ReaderName)
        {
            foreach (string _vendor in SpringCardVendorNames)
            {
                string[] pieces = ReaderName.Split(' ');

                if (pieces[0].ToLower() == _vendor.ToLower())
                    return true;
            }

            return false;
        }

        public static int CompareReaderNames(string x, string y)
        {
            ExplainReaderName(x, out string x_VendorName, out string x_ProductName, out string x_SlotName, out string x_ReaderIndex);
            ExplainReaderName(y, out string y_VendorName, out string y_ProductName, out string y_SlotName, out string y_ReaderIndex);

            int x_i, y_i, r;

            x_i = IsSpringCard(x_VendorName) ? 0 : 1;
            y_i = IsSpringCard(y_VendorName) ? 0 : 1;
            if (x_i != y_i) return x_i - y_i;

            r = x_ReaderIndex.CompareTo(y_ReaderIndex);
            if (r != 0) return r;

            r = x_VendorName.CompareTo(y_VendorName);
            if (r != 0) return r;

            r = x_ProductName.CompareTo(y_ProductName);
            if (r != 0) return r;

            r = x_SlotName.CompareTo(y_SlotName);
            if (r != 0) return r;

            return x.CompareTo(y);
        }

        public class ReaderNamesComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return CompareReaderNames(x, y);
            }
        }

        public static void ExplainReaderName(string ReaderName, out string VendorName, out string ProductName, out string SlotName, out string ReaderIndex)
        {
            foreach (string _vendor in SpringCardVendorNames)
            {
                string[] pieces = ReaderName.Split(' ');

                if (pieces[0].ToLower() == _vendor.ToLower())
                {
                    VendorName = pieces[0];
                    if (SystemInfo.GetRuntimeSystem() == SystemInfo.RuntimeSystem.Windows)
                    {
                        /* Parse reader name for Windows PC/SC */
                        /* ----------------------------------- */

                        /* Form is "VendorName ProductName SlotName Index" */
                        switch (pieces.Length)
                        {
                            case 0:
                            case 1:
                                ProductName = "";
                                SlotName = "";
                                ReaderIndex = "";
                                break;

                            case 2:
                                ProductName = "";
                                SlotName = "";
                                ReaderIndex = pieces[0];
                                break;

                            case 3:
                                ProductName = pieces[1];
                                SlotName = "";
                                ReaderIndex = pieces[2];
                                break;

                            case 4:
                                ProductName = pieces[1];
                                SlotName = pieces[2];
                                ReaderIndex = pieces[3];
                                break;

                            default:
                                if (pieces[pieces.Length - 2].Length == 1)
                                {
                                    /* Just a letter or an index */
                                    ProductName = string.Join(" ", pieces, 1, pieces.Length - 4);
                                    SlotName = string.Join(" ", pieces, pieces.Length - 3, 2);
                                    ReaderIndex = pieces[pieces.Length - 1];
                                }
                                else
                                {
                                    /* A full name */
                                    ProductName = string.Join(" ", pieces, 1, pieces.Length - 3);
                                    SlotName = pieces[pieces.Length - 2];
                                    ReaderIndex = pieces[pieces.Length - 1];
                                }
                                break;
                        }
                    }
                    else
                    {
                        /* Parse reader name for PCSC-Lite */
                        /* ------------------------------- */

                        /* Form is "VendorName ProductName [CCID] (SerialNumber) BusId LunId" */
                        /* We'll bge using ReaderIndex = BusId and SlotName = LunId */

                        switch (pieces.Length)
                        {
                            case 0:
                            case 1:
                                ProductName = "";
                                ReaderIndex = "";
                                SlotName = "";
                                break;

                            case 2:
                                ProductName = "";
                                ReaderIndex = pieces[0];
                                SlotName = "";
                                break;

                            case 3:
                                ProductName = pieces[1];
                                ReaderIndex = pieces[2];
                                SlotName = "";
                                break;

                            default:
                                ProductName = pieces[1];
                                for (int i = 2; i < pieces.Length - 2; i++)
                                    if (!pieces[i].StartsWith("[") && !pieces[i].StartsWith("("))
                                        ProductName += " " + pieces[i];
                                ReaderIndex = pieces[pieces.Length - 2];
                                SlotName = pieces[pieces.Length - 1];
                                break;
                        }
                    }
                    return;
                }
            }

            {
                string[] pieces = ReaderName.Split(' ');

                if (pieces.Length <= 1)
                {
                    VendorName = "";
                    ProductName = "";
                    SlotName = "";
                    ReaderIndex = pieces[0];
                }
                else if (pieces.Length == 2)
                {
                    VendorName = pieces[0];
                    ProductName = "";
                    SlotName = "";
                    ReaderIndex = pieces[1];
                }
                else if (pieces.Length == 3)
                {
                    VendorName = pieces[0];
                    ProductName = pieces[1];
                    SlotName = "";
                    ReaderIndex = pieces[2];
                }
                else if (pieces.Length == 4)
                {
                    VendorName = pieces[0];
                    ProductName = pieces[1];
                    SlotName = pieces[2];
                    ReaderIndex = pieces[3];
                }
                else
                {
                    VendorName = pieces[0];
                    ProductName = pieces[1];
                    for (int i = 2; i < pieces.Length - 2; i++)
                        ProductName += " " + pieces[i];
                    SlotName = pieces[pieces.Length - 2];
                    ReaderIndex = pieces[pieces.Length - 1];
                }
            }
        }
    }
}
