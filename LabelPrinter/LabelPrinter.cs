using System;
using System.Drawing;
using System.Drawing.Printing;

namespace BarcodeLabelPrinter
{
    public class LabelPrinter
    {
        private PrintDocument pd;
        private int LeftEdge;
        private int TopEdge;
        private int RightEdge;
        private int BotEdge;
        private string printString;

        public LabelPrinter()
        {
            pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.Color = false;
            pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            pd.DefaultPageSettings.PaperSize = new PaperSize("Letter", 400, 600); //5x6 label roll
            pd.PrinterSettings.PrinterName = "ZDesigner LP 2844";
            pd.PrintPage += new PrintPageEventHandler(LabelPrintHandler);

            LeftEdge = 0;
            TopEdge = 0;
            RightEdge = 600;
            BotEdge = 400;
        }

        public void print(string sku)
        {
            printString = sku.ToUpper();
            pd.Print();
        }

        private void LabelPrintHandler(object sender, PrintPageEventArgs ev)
        {
            ev.HasMorePages = false;

            var textFont = new Font("Arial", 60, FontStyle.Bold, GraphicsUnit.Point);
            var barcode = new Font("IDAutomationC128S", 36, FontStyle.Regular, GraphicsUnit.Point);

            var centerMid = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var centerBottom = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Far
            };

            var pageWidthRect2 = new Rectangle(LeftEdge, TopEdge, RightEdge, BotEdge);
            ev.Graphics.DrawString(printString, textFont, Brushes.Black, pageWidthRect2, centerMid);
            ev.Graphics.DrawString("!" + printString + "!", barcode, Brushes.Black, pageWidthRect2, centerBottom);
        }
    }
}
