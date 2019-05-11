# UPS Return Labels
This project connects to UPS Rest API to create a shipping return label in the browser.

The program ([starts](/start.cs)) by using the [factory pattern](/CarrierServiceFactory.cs) to create either a UPS or FedEx instance. Then it calls the CreateReturnLabel() method, which is supported by all providers implementing the [ICarrierService](/ICarrierService.cs) interface (UPS/FedEx).

**SHIP CARRIER FACTORY**<br>
**[CarrierServiceFactory.cs](/CarrierServiceFactory.cs)**<br>
Using dependency injection, the desired carrier type is passed to the [bootstrapper](/Bootstrapper.cs), which returns an instance of the UPS or FedEx service.

**SHIP CARRIER SERVICES (PROVIDER MODEL)**<br>
Both the UPS service and the FedEx service implement the [ICarrierService](/ICarrierService.cs) interface (the FedEx service was left a stub file in case of client preference).

**[UpsService.cs](/UpsService.cs)**<br>
This service uses constructor dependency injection for the services it requires. It implements the five interface methods to create a return label, shipping label and tracking number.

To create the JSON for the API requests, C# classes are instantiated and initialized with data, and then serialised using Newtonsoft's Json.NET library. The JSON response is deserialized to a dynamic object using Json.NET.

The CreateReturnLabel() method initiates the process by calling private methods which creates ShipRequest JSON to post to the UPS API, parses the JSON response for any errors, extracts the Base64 string, converts it to gif, and writes it to file. (Account information is embeddes in the request).<br>

**[UpsClient.cs](/UpsClient.cs)**<br>
This file contains all the code for connecting to the external API, so that API calls could be easily be mocked in a unit test.

**MODELS (SERIALIZATION CLASSES)**<br>
The [Models](/Models) directory contains all the classes needed for serialization and deserialization of UPS API request and response.

**DATA LAYER (REPO)**<br>
The [DataLayer](/DataLayer) directory contains code to fetch addresses from the SQL Server database for UPS ship addresses.

**SAMPLE UPS LABEL**<br>
[View](/images/ups_label_sample.png)
