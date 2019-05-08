using System;
using OsApiModels.Carrier;

namespace OSAPIServices.Service.Carrier
{
    public interface ICarrierService
    {
        ReturnLabelDetails CreateReturnLabel(int rmaNumber, string shipTypeCode = null, string shipTypeName = null, string packageWeight = null);
        ReturnLabelDetails CreateShipmentLabel(int orderId, string shipTypeCode = null, string shipTypeName = null, string packageWeight = null);
        ReturnLabelDetails GetShippingLabel(string trackingNumber);
        object GetTracking(string trackingNumber);
        string GetReturnLabelUrl(string trackingNumber);
    }
}
