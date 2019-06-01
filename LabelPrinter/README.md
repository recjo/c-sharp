# Barcode Label Printer with Zebra LP 2844 printer

I was asked to write a program that printed barcode labels for various product skus to identify pick locations in the warehouse storage racks. This project was interesting to me because it involved using the PrintDocument class to print barcode labels to a [Zebra LP 2844](/LabelPrinter/images/zebra-lp2844.png) printer.

Program.cs
[Program.cs](/LabelPrinter/Program.cs) is responsible for processing user input of product sku from the command-line.

LabelPrinter.cs
The [LabelPrinter.cs](/LabelPrinter/LabelPrinter.cs) class initializes printer properties in the constructor for settings like margins, printer name, paper size, orientation, and assigning the printing method to the PrintPage event handler.

The LabelPrintHandler() sends the product sku to the label in both plain text and bardcode format to the Zebra printer.

<!--- Click to view a sample [barcode label](/LabelPrinter/images/barcodelabel.jpg). -->
