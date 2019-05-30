# Swagger API / Recusion

I wrote a test utility app to allow QA teams to quickly call any REST API in our system. The test app uses Swagger to fetch all api's and displays the endpoints in a drop-down menu [(screenshot)](/Swagger/images/windowsform.png). When an endpoint is selected from the menu, the app dynamically builds the JSON body needed for a PUT or POST request, by parsing the swagger "definitions" object, and displays a link to paste the JSON in the text box. The app also displays a button for each verb supported (GET/POST/PUT/DELETE). and displays the JSON response in a textbox.

My "Swagger" helper class has only one public method, [GetSwaggerEndpoints()](/Swagger/swagger.cs), which calls the local Swagger api "/swagger/docs/v2" to get JSON object of all the api definitons.

<details>
<summary>click to see SWAGGER RESPONSE JSON</summary>
{<br />
&nbsp; &nbsp; "swagger": "2.0",<br />
&nbsp; &nbsp; "info": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; "version": "v2",<br />
&nbsp; &nbsp; &nbsp; &nbsp; "title": "OSAPI 2.0 Documentation"<br />
&nbsp; &nbsp; },<br />
&nbsp; &nbsp; "host": "apiv2.company.com:443",<br />
&nbsp; &nbsp; "basePath": "/myapi",<br />
&nbsp; &nbsp; "schemes": ["https"],<br />
&nbsp; &nbsp; "paths": {...},<br />
&nbsp; &nbsp; "definitions": {...}<br />
}<br />
</details>

The two important properties (above) are "paths", which contain all the api endpoints and the verbs they support (GET/POST/PUT/DELETE), and "definitons", which define the properties that make up the response object, as well as their data types. Each api in "paths" object references a "definition" via the $ref property (see $ref below).

<details>
<summary>click to see SWAGGER RESPONSE - PATHS OBJECT</summary>
"paths": {<br />
&nbsp; &nbsp; "/api/cancellations": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; "get": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "summary": "Get cancellations by date.",<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; //other obejcts removed for brevity<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "responses": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "200": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "description": "OK",<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "schema": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "type": "array",<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "items": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "$ref": "#/definitions/Cancellation"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; }<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; }<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; }<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; }<br />
&nbsp; &nbsp; &nbsp; &nbsp; }<br />
&nbsp; &nbsp; },<br />
&nbsp; &nbsp; //other objects removed for brevity<br />
}<br />
</details>
	
The tricky part, is some of the definitions reference other definitions (see the two $ref properties below). In order for the app to dynamically "re-build" the JSON body required for a PUT or POST request, recursion is needed whenever a definition references another definition. The GetJson() method will indirectly call itself whenever a new definition is found while parsing the JSON. 

<details>
<summary>click to see SWAGGER RESPONSE - DEFINTIIONS OBJECT</summary>
"definitions": {<br />
&nbsp; &nbsp; "Cancellation": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; "required": ["Items"],<br />
&nbsp; &nbsp; &nbsp; &nbsp; "type": "object",<br />
&nbsp; &nbsp; &nbsp; &nbsp; "properties": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "OrderId": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "format": "int32",<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "description": "Order id",<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "type": "integer"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; },<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "Items": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "description": "A list of UPCs included in this cancellation",<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "type": "array",<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "items": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "$ref": "#/definitions/CanceledItem"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; }<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; },<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "ExternalReferences": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "description": "List of external ECP reference IDs",<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "type": "array",<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "items": {<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; "$ref": "#/definitions/ExternalReference"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; }<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; }<br />
&nbsp; &nbsp; &nbsp; &nbsp; }<br />
&nbsp; &nbsp; },<br />
&nbsp; &nbsp; //other objects removed for brevity<br />
}<br />
</details>

