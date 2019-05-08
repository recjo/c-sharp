using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using OSAPIData.DataLayer;
using OsApiModels.Order;
using OsApiModels.Rma;
using OsApiModels.Users;
using OSAPIServices.Service.Carrier;

namespace OSAPIServices.Service.Rma
{
    public class RmaService : IRmaService
    {
        private readonly ICarrierServiceFactory _carrierServiceFactory;

        public RmaService( ICarrierServiceFactory carrierServiceFactory)
        {
            _carrierServiceFactory = carrierServiceFactory;
        }

        public void CreateRma(int rmaId)
        {
            var carrierService = _carrierServiceFactory.GetCarrierService();
            var result = carrierService.CreateReturnLabel(rmaId);
        }
    }
}