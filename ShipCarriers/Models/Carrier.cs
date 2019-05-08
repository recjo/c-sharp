using System;

namespace OsApiModels.Carrier
{
    public class ReturnLabelDetails
    {
        public string gifBase64;
        public string htmlBase64;
        public string trackingNumber;
        public string labelUrl;
    }

    public enum CarrierType
    {
        UPS,
        FedEx
    }
}
