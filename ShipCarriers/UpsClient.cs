using System;
using OSAPIUtils.Common;

namespace OSAPIServices.Service.Carrier
{
    public interface IUpsClient
    {
        string HttpPostWebForm(string uri, string parameters);
    }

    public class UpsClient : IUpsClient
    {
        public string HttpPostWebForm(string uri, string json)
        {
            var request = System.Net.WebRequest.Create(uri);
            request.Method = "POST";
            var byteArray = System.Text.Encoding.UTF8.GetBytes(json);
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            //obtain a reference to the upload stream, request sent after writing complete
            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            var responseFromServer = string.Empty;
            using (var response = (System.Net.HttpWebResponse)request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                using (var reader = new System.IO.StreamReader(responseStream))
                {
                    responseFromServer = reader.ReadToEnd();
                }
            }
            return responseFromServer;
        }
    }

    public class UpsClientSub : SubstituteBase, IUpsClient
    {
        private readonly string _repoInterface = "IUpsClient";
        public UpsClientSub(string staticDataFolder)
        {
            StaticDataFolder = staticDataFolder;
            RepoInterface = _repoInterface;
        }
        public string HttpPostWebForm(string uri, string parameters)
        {
            var hash = GetHash(uri, parameters);
            return LoadStaticData<string>(hash);
        }
    }
}
