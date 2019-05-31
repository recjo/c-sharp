using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;

namespace LabelPrinter
{
    class Program
    {
        protected static int copieseach = 10;
        protected static char singlechar = 'A';
        protected static int singledigit = 0; //turns out this does not change but I will leave it
        protected static int zonenumber = 7; //increment this manually each run because roll only holds 320 labels

        static void Main(string[] args)
        {
            var pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.Color = false;
            pd.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
            pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("Letter", 400, 600);
            pd.PrinterSettings.PrinterName = "ZDesigner LP 2844";
            pd.PrintPage += new PrintPageEventHandler(LabelPrintHandler);

            while (singlechar != 'W')
            {
                for (int cc = 0; cc < copieseach; cc++)
                {
                    pd.Print();
                }
                singlechar = (char)(((int)singlechar) + 1);
            }

            pd.Dispose();

            //press q to exit
            Console.Write("\nPress \"q\" to quit...");
            while (Console.ReadKey(true).Key != ConsoleKey.Q) ;
        }

        private static void LabelPrintHandler(object sender, PrintPageEventArgs ev)
        {
            ev.HasMorePages = false;

            System.Drawing.Font Header1 = new System.Drawing.Font("Arial", 160, FontStyle.Bold, GraphicsUnit.Point);

            StringFormat centerF = new StringFormat();
            centerF.Alignment = StringAlignment.Center;
            centerF.LineAlignment = StringAlignment.Center;

            int LeftEdge = 0;
            int TopEdge = 0;
            int RightEdge = 600;
            int BotEdge = 400;

            Rectangle pageWidthRect2 = new Rectangle(LeftEdge, TopEdge, RightEdge, BotEdge);
            ev.Graphics.DrawString((zonenumber.ToString().Length < 2 ? "0" + zonenumber.ToString() : zonenumber.ToString()) + ":" + singledigit.ToString() + singlechar, Header1, Brushes.Black, pageWidthRect2, centerF);
        }
    }
}
