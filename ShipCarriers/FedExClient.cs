using System;
using OSAPIUtils.Common;

namespace OSAPIServices.Service.Carrier
{
    public interface IFedExClient
    {
        string HttpPostWebForm(string uri, string parameters);
    }

    public class FedExClient : IFedExClient
    {
        public string HttpPostWebForm(string uri, string json)
        {
            return string.Empty;
        }
    }

    public class FedExClientSub : SubstituteBase, IFedExClient
    {
        private readonly string _repoInterface = "IFedExClient";
        public FedExClientSub(string staticDataFolder)
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
