using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;
using Xunit;
using Onestop.Rosetta.Tests.Common;

namespace Onestop.Rosetta.Tests.Translator
{
    public class ShopifyTranslationTest
    {
        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_Single_Item_No_Avalara()
        {
            //no pre_tax_price - this JSON has shipping tax
            var sourceDataJson = GetShopifyUSOrderWithSingleItemNoAvalaraData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 2);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 120M);
                Assert.True(order.OrderItems[0].ItemDiscount == 0M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 12.3M);
                //item 2 shipping product
                Assert.True(order.OrderItems[1].ItemCost == 13M);
                Assert.True(order.OrderItems[1].ItemDiscount == 0M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 1.33M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_Single_Item_Mult_Qty_No_Avalara()
        {
            //no pre_tax_price - this JSON has shipping tax
            var sourceDataJson = GetShopifyUSOrderWithSingleItemMultQtyNoAvalaraData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 3);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 120M);
                Assert.True(order.OrderItems[0].ItemDiscount == 0M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 12.3M);
                //item 2
                Assert.True(order.OrderItems[1].ItemCost == 120M);
                Assert.True(order.OrderItems[1].ItemDiscount == 0M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 12.3M);
                //item 3 shipping product
                Assert.True(order.OrderItems[2].ItemCost == 13M);
                Assert.True(order.OrderItems[2].ItemDiscount == 0M);
                Assert.True(order.OrderItems[2].ItemTax[0].TaxAmount == 1.33M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_Single_Item_With_Promo_No_Avalara()
        {
            //TODO: Shopify Bug reports no discount at line-item level
            //no pre_tax_price - this JSON has shipping tax
            var sourceDataJson = GetShopifyUSOrderWithSingleItemWithPromoNoAvalaraData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 2);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 120M);
                Assert.True(order.OrderItems[0].ItemDiscount == 0M); //should be $36 but JSON=$0
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 8.61M);
                //item 2 shipping product
                Assert.True(order.OrderItems[1].ItemCost == 13M);
                Assert.True(order.OrderItems[1].ItemDiscount == 0M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 1.33M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory(Skip = "JSON does not contain line-item discount values")]
        [Container]
        public void Translate_Shopify_US_Order_Multiple_Items_With_Promo_No_Avalara()
        {
            //TODO: when (MDNA) JSON contains valid line-item, discount values
        }

        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_GC_Only_Item_GC_To_Deposco()
        {
            //GIFT CARD ONLY ORDER -- YES TO DEPOSCO TEST
            var sourceDataJson = GetShopifyUSOrderWithGCOnlyItemData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            //to send a GC to Depsoco, set client id to 168 in translator
            translator.SetPartnerProcessing(168);
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                //a GC item must be in Placed status to be sent to Deposco. 
                Assert.True(order.OrderItems[0].ItemStatus == "Placed");
                //order header status should be Placed to be sent to Deposco. 
                Assert.True(order.OrderStatus == "Placed");
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_GC_Only_Item_GC_No_Deposco()
        {
            //GIFT CARD ONLY ORDER -- NO TO DEPOSCO TEST
            //gift cards orders should not be sent to Depsoco
            var sourceDataJson = GetShopifyUSOrderWithGCOnlyItemData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                //a GC item must be in Shipped status so it is NOT sent to Deposco.
                Assert.True(order.OrderItems[0].ItemStatus == "Shipped");
                //order header status should be Shipped status because nothing to send to Deposco
                Assert.True(order.OrderStatus == "Shipped");
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_GC_And_Reg_Item_GC_To_Deposco()
        {
            //GIFT CARD MIXED ORDER -- YES TO DEPOSCO TEST
            var sourceDataJson = GetShopifyUSOrderWithGCAndRegItemsData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            //to send a GC to Depsoco, set client id to 168 in translator
            translator.SetPartnerProcessing(168);
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                //a GC item must be in Placed status to be sent to Deposco. 
                Assert.True(order.OrderItems[0].ItemStatus == "Placed");
                //regular item must be in Placed status to be sent to Deposco. 
                Assert.True(order.OrderItems[1].ItemStatus == "Placed");
                //order header status should be Placed to be sent to Deposco. 
                Assert.True(order.OrderStatus == "Placed");
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_GC_And_Reg_Item_GC_No_Deposco()
        {
            //GIFT CARD MIXED ORDER -- NO TO DEPOSCO TEST
            //gift cards orders should not be sent to Depsoco, but regular items should
            var sourceDataJson = GetShopifyUSOrderWithGCAndRegItemsData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                //a GC item must be in Shipped status so it is NOT sent to Deposco.
                Assert.True(order.OrderItems[0].ItemStatus == "Shipped");
                //Regular item must be in Placed status to be sent to Depsoco.
                Assert.True(order.OrderItems[1].ItemStatus == "Placed");
                //order header status should be Placed because regular item needs to go to Deposco
                Assert.True(order.OrderStatus == "Placed");
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_Single_Item()
        {
            //with Avalara pre_tax_price
            var sourceDataJson = GetShopifyUSOrderWithSingleItemsData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 2);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 49.95M);
                Assert.True(order.OrderItems[0].ItemDiscount == 0M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 4.74M);
                //item 2 shipping product
                Assert.True(order.OrderItems[1].ItemCost == 0M);
                Assert.True(order.OrderItems[1].ItemDiscount == 0M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 0M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_With_Promo_Single_Item()
        {
            //with Avalara pre_tax_price
            var sourceDataJson = GetShopifyUSOrderWithPromoSingleItemsData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 2);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 49.95M);
                Assert.True(order.OrderItems[0].ItemDiscount == 4.99M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 4.27M);
                //item 2 shipping product
                Assert.True(order.OrderItems[1].ItemCost == 0M);
                Assert.True(order.OrderItems[1].ItemDiscount == 0M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 0M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_With_Shipping_Promo_Single_Item()
        {
            //with Avalara pre_tax_price
            var sourceDataJson = GetShopifyUSOrderWithShippingPromoSingleItemsData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 2);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 149.95M);
                Assert.True(order.OrderItems[0].ItemDiscount == 0M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 13.86M);
                //item 2 shipping product
                Assert.True(order.OrderItems[1].ItemCost == 40M);
                Assert.True(order.OrderItems[1].ItemDiscount == 40M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 0M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct (order.Totals.DiscountTotal is not subtratced for a shipping discount, since the discount is just subtracted from ShippingTotal)
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_US_Order_With_Promo_Mult_Items()
        {
            //with Avalara pre_tax_price
            var sourceDataJson = GetShopifyUSOrderWithPromoMultItemsData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm calculations
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 10);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 14.95M);
                Assert.True(order.OrderItems[0].ItemDiscount == 1.495M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 1.2775M);
                //item 2
                Assert.True(order.OrderItems[1].ItemCost == 14.95M);
                Assert.True(order.OrderItems[1].ItemDiscount == 1.495M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 1.2775M);
                //item 3
                Assert.True(order.OrderItems[2].ItemCost == 14.95M);
                Assert.True(order.OrderItems[2].ItemDiscount == 1.495M);
                Assert.True(order.OrderItems[2].ItemTax[0].TaxAmount == 1.2775M);
                //item 4
                Assert.True(order.OrderItems[3].ItemCost == 14.95M);
                Assert.True(order.OrderItems[3].ItemDiscount == 1.495M);
                Assert.True(order.OrderItems[3].ItemTax[0].TaxAmount == 1.2775M);
                //item 5
                Assert.True(order.OrderItems[4].ItemCost == 249.95M);
                Assert.True(order.OrderItems[4].ItemDiscount == 24.994M);
                Assert.True(order.OrderItems[4].ItemTax[0].TaxAmount == 21.372M);
                //item 6
                Assert.True(order.OrderItems[5].ItemCost == 249.95M);
                Assert.True(order.OrderItems[5].ItemDiscount == 24.994M);
                Assert.True(order.OrderItems[5].ItemTax[0].TaxAmount == 21.372M);
                //item 7
                Assert.True(order.OrderItems[6].ItemCost == 249.95M);
                Assert.True(order.OrderItems[6].ItemDiscount == 24.994M);
                Assert.True(order.OrderItems[6].ItemTax[0].TaxAmount == 21.372M);
                //item 8
                Assert.True(order.OrderItems[7].ItemCost == 249.95M);
                Assert.True(order.OrderItems[7].ItemDiscount == 24.994M);
                Assert.True(order.OrderItems[7].ItemTax[0].TaxAmount == 21.372M);
                //item 9
                Assert.True(order.OrderItems[8].ItemCost == 249.95M);
                Assert.True(order.OrderItems[8].ItemDiscount == 24.994M);
                Assert.True(order.OrderItems[8].ItemTax[0].TaxAmount == 21.372M);
                //item 10 shipping product
                Assert.True(order.OrderItems[9].ItemCost == 40M);
                Assert.True(order.OrderItems[9].ItemDiscount == 0M);
                Assert.True(order.OrderItems[9].ItemTax[0].TaxAmount == 0M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_Order_Single_Item_With_Vat()
        {
            var sourceDataJson = GetShopifyOrderSingleItemWithVatData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm VAT taxes and Item price minus the VAT
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 2);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 24.99M);
                Assert.True(order.OrderItems[0].ItemDiscount == 0M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 5M);
                //item 2 shipping product
                Assert.True(order.OrderItems[1].ItemCost == 5.83M);
                Assert.True(order.OrderItems[1].ItemDiscount == 0M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 1.16M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_Order_Single_Item_With_Vat_With_Promo()
        {
            var sourceDataJson = GetShopifyOrderSingleItemWithVatWithPromoData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm VAT taxes and Item price minus the VAT
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 2);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 25.49M);
                Assert.True(order.OrderItems[0].ItemDiscount == 2.99M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 4.5M);
                //item 2 shipping product
                Assert.True(order.OrderItems[1].ItemCost == 4.16M);
                Assert.True(order.OrderItems[1].ItemDiscount == 0M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 0.83M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_Order_Multiple_Item_With_Vat()
        {
            var sourceDataJson = GetShopifyOrderMultipleItemWithVatData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm VAT taxes and Item price minus the VAT
            foreach (StandardOrder order in result)
            {
                //item 1
                Assert.True(order.OrderItems.Count == 3);
                Assert.True(order.OrderItems[0].ItemCost == 24.99M);
                Assert.True(order.OrderItems[0].ItemDiscount == 0M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 5M);
                //item 2
                Assert.True(order.OrderItems[1].ItemCost == 24.99M);
                Assert.True(order.OrderItems[1].ItemDiscount == 0M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 5M);
                //item 3 shipping product
                Assert.True(order.OrderItems[2].ItemCost == 4.16M);
                Assert.True(order.OrderItems[2].ItemDiscount == 0M);
                Assert.True(order.OrderItems[2].ItemTax[0].TaxAmount == 0.83M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_Order_Multiple_Item_With_Vat_With_Promo()
        {
            var sourceDataJson = GetShopifyOrderMultipleItemWithVatWithPromoData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm VAT taxes and Item price minus the VAT
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 3);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 25.49M);
                Assert.True(order.OrderItems[0].ItemDiscount == 3.00M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 4.5M);
                //item 2
                Assert.True(order.OrderItems[1].ItemCost == 144.49M);
                Assert.True(order.OrderItems[1].ItemDiscount == 16.99M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 25.5M);
                //item 3 shipping product
                Assert.True(order.OrderItems[2].ItemCost == 4.16M);
                Assert.True(order.OrderItems[2].ItemDiscount == 0M);
                Assert.True(order.OrderItems[2].ItemTax[0].TaxAmount == 0.83M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        [Theory]
        [Container]
        public void Translate_Shopify_Order_Multiple_Qty_Item_With_Vat_With_Promo()
        {
            var sourceDataJson = GetShopifyOrderMultipleQtyItemWithVatWithPromoData();
            var source = JsonConvert.DeserializeObject<ShopifyOrderResult>(sourceDataJson);
            var translator = new ShopifyToStandardOrderTranslator();
            var result = translator.Translate<ShopifyOrderResult, List<StandardOrder>>(source);
            Assert.True(result.Count == source.orders.Length);

            //confirm VAT taxes and Item price minus the VAT
            foreach (StandardOrder order in result)
            {
                Assert.True(order.OrderItems.Count == 3);
                //item 1
                Assert.True(order.OrderItems[0].ItemCost == 25.49M);
                Assert.True(order.OrderItems[0].ItemDiscount == 2.995M);
                Assert.True(order.OrderItems[0].ItemTax[0].TaxAmount == 4.5M);
                //item 2
                Assert.True(order.OrderItems[1].ItemCost == 25.49M);
                Assert.True(order.OrderItems[1].ItemDiscount == 2.995M);
                Assert.True(order.OrderItems[1].ItemTax[0].TaxAmount == 4.5M);
                //item 3 shipping product
                Assert.True(order.OrderItems[2].ItemCost == 4.16M);
                Assert.True(order.OrderItems[2].ItemDiscount == 0M);
                Assert.True(order.OrderItems[2].ItemTax[0].TaxAmount == 0.83M);
                //confirm total of items (exluding shipProduct) equals subtotal
                Assert.True(order.OrderItems.Where(a => !a.IsShipping).Sum(s => s.ItemCost) == order.Totals.SubTotal);
                //confirm grand total is correct
                Assert.True((order.Totals.SubTotal + order.Totals.ShippingTotal + order.Totals.TaxTotal - order.Totals.DiscountTotal) == order.Totals.GrandTotal);
            }
        }

        private string GetShopifyOrderSingleItemWithVatData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRUK-Shopify_Order_Single_Item_With_Vat.json"));
        }

        private string GetShopifyOrderSingleItemWithVatWithPromoData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRUK-Shopify_Order_Single_Item_With_Vat_With_Promo.json"));
        }

        private string GetShopifyOrderMultipleItemWithVatData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRUK-Shopify_Order_Multiple_Item_With_Vat.json"));
        }

        private string GetShopifyOrderMultipleItemWithVatWithPromoData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRUK-Shopify_Order_Multiple_Item_With_Vat_With_Promo.json"));
        }

        private string GetShopifyOrderMultipleQtyItemWithVatWithPromoData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRUK-Shopify_Order_Multiple_Qty_Item_With_Vat_With_Promo.json"));
        }

        private string GetShopifyUSOrderWithPromoMultItemsData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRTL-Shopify_Order_With_Promo_Mult_Items.json"));
        }

        private string GetShopifyUSOrderWithPromoSingleItemsData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRTL-Shopify_Order_With_Promo_Single_Item.json"));
        }

        private string GetShopifyUSOrderWithGCOnlyItemData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRTL-Shopify_US_Order_With_GC_Only_Item.json"));
        }

        private string GetShopifyUSOrderWithGCAndRegItemsData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRTL-Shopify_US_Order_With_GC_Reg_Items.json"));
        }

        private string GetShopifyUSOrderWithSingleItemsData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRTL-Shopify_US_Order_With_Single_Item.json"));
        }

        private string GetShopifyUSOrderWithSingleItemNoAvalaraData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"MDNA-Shopify_US_Order_With_Single_Item_No_Avalara.json"));
        }
        
        private string GetShopifyUSOrderWithSingleItemMultQtyShippingTaxNoAvalaraData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"ATOM-Shopify_US_Order_With_Single_Item_Mult_Qty_With_Shipping_Tax_No_Avalara.json"));
        }

        private string GetShopifyUSOrderWithSingleItemMultQtyNoAvalaraData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"MDNA-Shopify_US_Order_With_Single_Item_Mult_Qty_No_Avalara.json"));
        }

        private string GetShopifyUSOrderWithSingleItemWithPromoNoAvalaraData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"MDNA-Shopify_US_Order_With_Single_Item_With_Promo_No_Avalara.json"));
        }
        
        private string GetShopifyUSOrderWithShippingPromoSingleItemsData()
        {
            return File.ReadAllText(Path.Combine(GetDataFolderPath(), @"TRTL-Shopify_Order_With_Shipping_Promo_Single_Item.json"));
        }
        #endregion

        private string GetDataFolderPath()
        {
            return ConfigurationManager.AppSettings["TranslatorDataFolder"];
        }
    }
}
