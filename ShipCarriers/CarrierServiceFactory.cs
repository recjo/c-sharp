using System;
using log4net;
using System.Reflection;
using OsApiModels.Carrier;
using OSAPIServices.Service.Platform;

namespace OSAPIServices.Service.Carrier
{
    public class CarrierServiceFactory : ICarrierServiceFactory
    {
        private readonly IResolver _resolver;
        private readonly IPlatformService _platformService;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public CarrierServiceFactory(IResolver resolver, IPlatformService platformService)
        {
            _resolver = resolver;
            _platformService = platformService;
        }

        public ICarrierService GetCarrierService()
        {
            ICarrierService carrierService;

            try
            {
                var carrierType = _platformService.GetSetting<string>("CarrierType");
                carrierService = _resolver.Resolve<ICarrierService>(carrierType);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
            return carrierService;
        }
    }
}
