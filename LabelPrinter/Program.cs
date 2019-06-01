using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLabelPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            var sku = string.Empty;

            #if DEBUG
                args = new[] { "FR-4053-BLK" }; //simulates command-line arguments
            #endif

            if (args != null && args.Length > 0)
            {
                sku = args[0].ToString().ToLower();
            }

            while (String.IsNullOrEmpty(sku))
            {
                Console.WriteLine("Enter sku to print (and press Return):");
                sku = Console.ReadLine();
            }

            Console.WriteLine("Printing: {0}", sku);
            var lp = new LabelPrinter();
            lp.print(sku);

            //press q to exit
            Console.Write("\nPress \"q\" to quit...");
            while (Console.ReadKey(true).Key != ConsoleKey.Q) ;
        }
    }
}
