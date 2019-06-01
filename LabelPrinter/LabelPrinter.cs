using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;

namespace BarcodeLabelPrinter
{
    public class LabelPrinter
    {
        private PrintDocument pd;
        protected static string printString;

        public LabelPrinter()
        {
            pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.Color = false;
            pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            pd.DefaultPageSettings.PaperSize = new PaperSize("Letter", 400, 600); //5x6 label roll
            pd.PrinterSettings.PrinterName = "ZDesigner LP 2844";
            pd.PrintPage += new PrintPageEventHandler(LabelPrintHandler);
        }

        public void print(string sku)
        {
            printString = sku.ToUpper();
            pd.Print();
        }

        private static void LabelPrintHandler(object sender, PrintPageEventArgs ev)
        {
            ev.HasMorePages = false;

            Font textFont = new Font("Arial", 60, FontStyle.Bold, GraphicsUnit.Point);
            Font barcode = new Font("IDAutomationC128S", 36, FontStyle.Regular, GraphicsUnit.Point);

            StringFormat centerF = new StringFormat();
            centerF.Alignment = StringAlignment.Center;
            centerF.LineAlignment = StringAlignment.Center;

            StringFormat centerBottom = new StringFormat();
            centerBottom.Alignment = StringAlignment.Center;
            centerBottom.LineAlignment = StringAlignment.Far;

            int LeftEdge = 0;
            int TopEdge = 0;
            int RightEdge = 600;
            int BotEdge = 400;

            Rectangle pageWidthRect2 = new Rectangle(LeftEdge, TopEdge, RightEdge, BotEdge);
            ev.Graphics.DrawString(printString, textFont, Brushes.Black, pageWidthRect2, centerF);
            ev.Graphics.DrawString("!" + printString + "!", barcode, Brushes.Black, pageWidthRect2, centerBottom);
        }
    }
}
