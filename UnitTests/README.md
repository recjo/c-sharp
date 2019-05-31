# Unit Tests for Shopify Order JSON

These 16 unit tests ensured the "translator" classes correctly transformed the order JSON from the Shopify API before importing into the orders database.

Specifically, the unit test targets the algorithms that calculate an order's grand total based upon line item quantity divided by any items discount, item tax, item shipping charge, item shipping discounts, and whether any VAT taxes are deducted from the item price.

Each unit test targets specific order variables such as orders with a single item, multipe items, items with multiple quantities, in combination with the absence or presence of sales tax, promo codes, and gift cards.

A sample file of Shopify order JSON used as data source can be viewed: [TRTL-Shopify_Order_With_Promo_Mult_Items.json](/UnitTests/Data/TRTL-Shopify_Order_With_Promo_Mult_Items.json)