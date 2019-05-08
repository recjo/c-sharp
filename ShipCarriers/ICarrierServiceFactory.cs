using System;
using OsApiModels.Carrier;

namespace OSAPIServices.Service.Carrier
{
    /// <summary>
    /// Service for carrier
    /// </summary>
    public interface ICarrierServiceFactory
    {
        ICarrierService GetCarrierService();
    }
}
