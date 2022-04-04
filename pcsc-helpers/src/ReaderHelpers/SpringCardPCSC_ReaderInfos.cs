using SpringCard.LibCs;

namespace SpringCard.PCSC.ReaderHelpers
{
    /**
	 * \brief Static class that gives a direct access to PC/SC functions (SCard... provided by winscard.dll or libpcsclite)
	 *
	 **/
    public static class ReaderInfos
    {
        private static readonly string[] SpringCardVendorNames = new string[] { "SpringCard " };

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

                            default:
                                ProductName = string.Join(" ", pieces, 1, pieces.Length - 3);
                                SlotName = pieces[pieces.Length - 2];
                                ReaderIndex = pieces[pieces.Length - 1];
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
