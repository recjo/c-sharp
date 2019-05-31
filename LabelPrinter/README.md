# Label Printer with Zebra LP 2844 printer

I was asked to write a program that printed labels to identify different areas of warehouse storage racks. This project was interesting to me because it involved using the PrintDocument class to print labels to a [Zebra LP 2844](/LabelPrinter/images/zebra-lp2844.png) printer. 

After instantiating the PrintDocument class, I set properties like margins, printer name, paper size, orientation, and assigned the printing method to the PrintPage event handler.

This simple command line utility printed 10 copies of a label with warehouse codes from A to W.