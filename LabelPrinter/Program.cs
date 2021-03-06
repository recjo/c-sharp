﻿using System;

namespace BarcodeLabelPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            var sku = string.Empty;
            var lp = new LabelPrinter();

            #if DEBUG
                args = new[] { "FR-4053-BLK" }; //simulates command-line arguments
            #endif

            if (args != null && args.Length > 0)
            {
                sku = args[0].ToString().ToLower();
            }

            while (true)
            {
                Console.WriteLine("Enter sku to print (and press Return):");

                //if "Q" is pressed, exit program (without enter key press)
                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Q)
                {
                    break;
                }

                //add first keypress to sku
                Console.Write(keyInfo.Key);
                sku = keyInfo.Key + Console.ReadLine();

                //print sku label
                Console.WriteLine("Printing: {0}", sku);
                lp.print(sku);

                //press q to exit
                Console.WriteLine("Press \"q\" to quit...\n");
            }
        }
    }
}
