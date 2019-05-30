# Swagger API & Parsing Recusion

I wrote a test utility app to allow QA teams to quickly call any REST API in our system. The test app fetches all api's using swagger api and displays the endpoints in a drop-down menu [screenshot](/c-sharp/Swagger/images/windowsform.png). When an endpoint is selected from the menu, the app builds the JSON body needed for a PUT or POST request, using the swagger "definitions" object, and displays a link to paste the JSON in the text box. The app uses the swagger "paths" object to display a button for each verb supported (GET/POST/PUT/DELETE), to execute the request. The app then displays the JSON response in a textbox.

My "Swagger" helper class has only one public method, [GetSwaggerEndpoints()](/c-sharp/Swagger/swagger.cs), which calls the local swagger api "/swagger/docs/v2" to get JSON object of all the api definitons. The abbreviated JSON appears below:

<details>
<summary>click to see **SWAGGER RESPONSE JSON**</sumamry>
```json
{
	"swagger": "2.0",
	"info": {
		"version": "v2",
		"title": "OSAPI 2.0 Documentation"
	},
	"host": "apiv2.company.com:443",
	"basePath": "/myapi",
	"schemes": ["https"],
	"paths": {...}
	"definitions": {...}
}
```
</details>

The two important properties are "paths", which contain all the api paths and the verbs they support (GET/POST/PUT/DELETE), and "definitons", which hold the property names of the response object as well as their data types. Each api in "paths" object references a "definition" via the $ref property, which contains the definition name after the "#/definitions/" (see $ref below).

<details>
<summary>click to see **SWAGGER RESPONSE - PATHS OBJECT**</sumamry>
```json
	"paths": {
		"/api/cancellations": {
			"get": {
				"summary": "Get cancellations by date.",
				//other obejcts remove d for brevity
				"responses": {
					"200": {
						"description": "OK",
						"schema": {
							"type": "array",
							"items": {
								"$ref": "#/definitions/Cancellation"
							}
						}
					}
				}
			}
		},
		//other objects removed for brevity
	}
```
</details>
	
The tricky part, is some of the definitions reference other definitions. In order to dynamically "re-build" the JSON body required for a PUT or POST request, recursion is needed whenever a definition references another definition (see the two $ref properties below). The GetJson() method will indirectly call itself whenever a new definition is found while parsing the JSON. 

<details>
<summary>click to see ****SWAGGER RESPONSE - DEFINTIIONS OBJECT**</sumamry>
```json
	"definitions": {
		"Cancellation": {
			"required": ["Items"],
			"type": "object",
			"properties": {
				"OrderId": {
					"format": "int32",
					"description": "Order id",
					"type": "integer"
				},
				"Items": {
					"description": "A list of UPCs included in this cancellation",
					"type": "array",
					"items": {
						"$ref": "#/definitions/CanceledItem"
					}
				},
				"ExternalReferences": {
					"description": "List of external ECP reference IDs",
					"type": "array",
					"items": {
						"$ref": "#/definitions/ExternalReference"
					}
				}
			}
		},
		//other objects removed for brevity
	}
```
</details>

