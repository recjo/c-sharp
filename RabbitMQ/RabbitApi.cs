using System;
using System.Net;

namespace Onestop.Rosetta.DataTransport
{
    public class RabbitApi : RabbitTransport
    {
        public RabbitApi()
        {
            base.userName = "userName";
            base.passWord = "passWord";
            base.hostName = "rmq.company.com";
        }

        public RabbitApi(string hostName, string virtualHost, string userName, string passWord)
        {
            base.userName = userName;
            base.passWord = passWord;
            base.virtualHost = virtualHost;
            base.hostName = hostName;
        }

        public string VirtualHostname()
        {
            return base.virtualHost;
        }

        public string Get(string app)
        {
            var resp = string.Empty;
            var request = (HttpWebRequest)WebRequest.Create(String.Format("https://{0}/api/{1}", base.hostName, app.TrimStart('/').TrimEnd('/')));
            request.Credentials = new NetworkCredential(userName, passWord);
            request.Method = "GET";
            request.Accept = "application/json";
            using (WebResponse response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var sr = new System.IO.StreamReader(stream))
                    {
                        resp = sr.ReadToEnd();
                    }
                }
            }
            return resp;
        }
    }
}
