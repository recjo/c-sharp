using System;
using OsApiModels.Users;

namespace OSAPIData.DataLayer
{
    public interface ICarrierRepository
    {
        AddressInfo GetClientHomeFacilityAddress(int clientId);
    }
}
