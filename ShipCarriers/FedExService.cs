using System;
using OsApiModels.Users;
using OsApiModels.Carrier;
using OSAPIData.DataLayer;
using OSAPIServices.Service.Platform;

namespace OSAPIServices.Service.Carrier
{
    public class FedExService : ICarrierService
    {
        private readonly IPlatformService _platformService;
        private readonly IFedExClient _fedExClient;
        private readonly ICarrierRepository _carrierRepository;
        private string fedExAccessKey;
        private string fedExAccountNumber;
        private string fedExUserName;
        private string fedExPassword;
        private string fedExEndpoint;

        public FedExService(IPlatformService platformService, IFedExClient fedExClient)
        {
            _platformService = platformService;
            _fedExClient = fedExClient;
            //modify to get these keys from Settings table via platform service
            fedExAccessKey = "asasdadasd";
            fedExAccountNumber = "dfkjsahjdasdas";
            fedExUserName = "asasdasdasd";
            fedExPassword = "asasdasdsa";
            fedExEndpoint = "https://www.fedex.com";
        }

        public ReturnLabelDetails CreateReturnLabel(int rmaNumber, string shipTypeCode = null, string shipTypeName = null, string packageWeight = null)
        {
            return new ReturnLabelDetails();
        }

        public ReturnLabelDetails CreateShipmentLabel(int orderId, string shipTypeCode = null, string shipTypeName = null, string packageWeight = null)
        {
            return new ReturnLabelDetails();
        }

        public ReturnLabelDetails GetShippingLabel(string trackingNumber)
        {
            return new ReturnLabelDetails();
        }

        public object GetTracking(string trackingNumber)
        {
            return string.Empty;
        }

        public string GetReturnLabelUrl(string trackingNumber)
        {
            return string.Empty;
        }
    }
}
