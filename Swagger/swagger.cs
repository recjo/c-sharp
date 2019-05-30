using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OsapiPosterizer
{
    public class Swagger
    {
        private string url;
        private Dictionary<string, string> jsonModels;

        public Swagger(string url)
        {
            this.url = url;
        }

        public List<SwaggerEndpoint> GetSwaggerEndpoints()
        {
            var json = string.Empty;
            var endpoints = new List<SwaggerEndpoint>();
            using (var client = new WebClient())
            {
                json = client.DownloadString(url);
            }
            if (String.IsNullOrEmpty(json))
                return endpoints;
            var swagger = (dynamic)JObject.Parse(json);
            //fetch and save all definitions (json model schemas) into dictionary
            try
            {
                jsonModels = new Dictionary<string, string>();
                var definitions = (dynamic)swagger.definitions;
                if (definitions != null)
                {
                    foreach (var item in definitions)
                    {
                        if (!jsonModels.ContainsKey(item.Name))
                            jsonModels.Add(item.Name, item.Value.properties.ToString());
                    }
                }
            }
            catch { }
            //now do paths
            var paths = (dynamic)swagger.paths;
            if (paths == null)
                return endpoints;
            foreach (var item in paths)
            {
                endpoints.Add(new SwaggerEndpoint() { Url = item.Name, Verbs = GetVerbs(item.Value), Definitions = GetDefinitions(item.Value) });
            }
            return endpoints;
        }

        private Dictionary<string, string> GetDefinitions(JToken val)
        {
            var dict = new Dictionary<string, string>();
            foreach (var item in (dynamic)val)
            {
                //if no definitions were found, no sense looking for sample json
                if (jsonModels == null || jsonModels.Count == 0)
                {
                    dict.Add(item.Name.ToLower(), "json not found");
                    continue;
                }

                if (item.Value.parameters == null || (!item.Name.ToLower().Equals("put") && !item.Name.ToLower().Equals("post") && !item.Name.ToLower().Equals("patch")))
                    continue;

                foreach (var subItem in (dynamic)item.Value.parameters)
                {
                    if (subItem.schema == null)
                        continue;

                    subItem.schema.sref = subItem.schema["$ref"];

                    //look for any request json needed for the specified Verb
                    var defName = "";
                    var isArray = false;
                    if (subItem.schema.sref != null)
                    {
                        defName = subItem.schema.sref.ToString();
                    }
                    else if (subItem.schema.items != null)
                    {
                        subItem.schema.items.sref = subItem.schema.items["$ref"];
                        if (subItem.schema.items.sref == null)
                            break;
                        if (subItem.schema.type != null && subItem.schema.type.ToString().ToLower().Equals("array"))
                            isArray = true;
                        defName = subItem.schema.items.sref.ToString();
                    }

                    if (!String.IsNullOrEmpty(defName) && defName.IndexOf("#/definitions") > -1)
                    {
                        dict.Add(item.Name.ToLower(), GetJson(defName, isArray));
                    }
                }
            }
            return dict;
        }

        private string GetJson(string definitionName, bool isArray = false)
        {
            try
            {
                //this method is called recursively (by Form1 and HandleType methods)
                if (definitionName != null && definitionName.ToString().IndexOf("#/definitions") > -1)
                {
                    var json = new StringBuilder();
                    if (isArray)
                        json.Append("[");
                    json.Append("{");
                    var modelName = definitionName.ToString().Substring(definitionName.ToString().IndexOf('/', 2) + 1);
                    json.Append(JsonStringBuilder(modelName));
                    json.Append("}");
                    if (isArray)
                        json.Append("]");
                    return json.ToString().Replace(",}", "}").Replace(",]", "]");
                }
            }
            catch { }
            return string.Empty;
        }

        private string JsonStringBuilder(string modelName)
        {
            var json = new StringBuilder();
            var modelSchema = jsonModels[modelName];
            var jsonModel = (JObject)JsonConvert.DeserializeObject(modelSchema);
            foreach (var x in jsonModel) //jsonModel.Count
            {
                //schema: string name = x.Key;
                //schema: JToken value = x.Value;
                if (x.Value == null)
                    continue;
                json.Append(HandleProperty(x.Key, x.Value));
            }
            return json.ToString();
        }

        private string HandleProperty(string propName, JToken token)
        {
            var json = new StringBuilder();
            var type = token["type"];
            var sref = token["$ref"]; //some properties don't have type, only definitions
            if (type != null)
            {
                switch (type.ToString().ToLower())
                {
                    case "integer":
                    case "number":
                        json.Append(HandleNumber(propName, token["format"]));
                        break;
                    case "boolean":
                        json.Append(HandleBoolean(propName));
                        break;
                    case "string":
                        json.Append(HandleString(propName, token["format"]));
                        break;
                    case "array":
                        json.Append(HandleArray(propName, token["items"]));
                        break;
                    case "object":
                        json.Append(HandleObject(propName, token["additionalProperties"]));
                        break;
                    default:
                        Console.WriteLine("unsupported " + type.ToString());
                        break;
                }
            }
            else if (sref != null)
            {
                json.Append(String.Format("\"{0}\": {1},", propName, GetJson(sref.ToString())));
            }
            return json.ToString();
        }

        private string HandleBoolean(string propName)
        {
            return String.Format("\"{0}\": true,", propName);
        }

        private string HandleNumber(string propName, JToken formatToken)
        {
            //formatToken can be either "int32" or "double", but since they both return zero, I will ignore it
            //if propName is null, items are part of an array, so just return item
            if (String.IsNullOrEmpty(propName))
                return "0";
            else
                return String.Format("\"{0}\": 0,", propName);
        }

        private string HandleString(string propName, JToken formatToken)
        {
            var val = "string";
            if (formatToken != null)
            {
                if (formatToken.ToString() == "date-time")
                    val = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                else
                    val = "unsupported";
            }
            //if propName is null, items are part of an array, so just return item
            if (String.IsNullOrEmpty(propName))
                return String.Format("\"{0}\"", val);
            else
                return String.Format("\"{0}\": \"{1}\",", propName, val);
        }

        private string HandleArray(string propName, JToken itemsToken)
        {
            if (itemsToken == null)
                return string.Empty;
            var json = string.Empty;
            if (itemsToken["$ref"] != null)
            {
                var definition = GetJson(itemsToken["$ref"].ToString());
                if (!String.IsNullOrEmpty(definition))
                    json = definition;
            }
            else
            {
                json = HandleProperty(null, itemsToken);
            }
            //return item(s) as array
            return (String.IsNullOrEmpty(json) ? string.Empty : String.Format("\"{0}\": [{1}],", propName, json));
        }

        private string HandleObject(string propName, JToken propertyToken)
        {
            if (propertyToken == null)
                return string.Empty;
            var definition = GetJson(propertyToken["$ref"].ToString());
            if (!String.IsNullOrEmpty(definition))
                return String.Format("\"{0}\": {1},", propName, definition);
            return string.Empty;
        }

        private string GetVerbs(JToken val)
        {
            var verbList = new StringBuilder();
            foreach (JProperty item in (JToken)val)
            {
                verbList.Append(item.Name + ",");
            }
            return verbList.ToString().TrimEnd(',');
        }

            private bool isJson(string json)
            {
                try
                {
                    JToken.Parse(json);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
    }

    public class SwaggerEndpoint
    {
        public string Url { get; set; }
        public string Verbs { get; set; }
        public Dictionary<string, string> Definitions { get; set; }
    }
}
