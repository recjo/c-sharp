using System;
using OsApiModels.Users;
using OSAPIData.DataReader;
using Onestop.Core.Datastore;

namespace OSAPIData.DataLayer
{
    public class CarrierRepository : ICarrierRepository
    {
        private readonly IDataReaderManager _dataReader;

        public CarrierRepository(IDataReaderManager dataReader)
        {
            _dataReader = dataReader;
        }

        /// <summary>
        /// Gets client's home facilty address
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns>AddressInfo</returns>
        public AddressInfo GetClientHomeFacilityAddress(int clientId)
        {
            var db = _dataReader.Procedure("GetClientHomeFacilityAddress", "OSAPIData.Properties.Settings.OSCommonConnectionString");
            db.AddParam("@ClientID", clientId);
            var ai = new AddressInfo();
            using (var reader = db.Go())
            {
                if (reader.Read())
                {
                    ai.FirstName = reader.GetValue<string>("FirstName", String.Empty);
                    ai.LastName = reader.GetValue<string>("LastName", String.Empty);
                    ai.AddressLine1 = reader.GetValue<string>("AddressLine1", String.Empty);
                    ai.AddressLine2 = reader.GetValue<string>("AddressLine2", String.Empty);
                    ai.City = reader.GetValue<string>("City", String.Empty);
                    ai.State = reader.GetValue<string>("State", String.Empty);
                    ai.PostalCode = reader.GetValue<string>("PostalCode", String.Empty);
                    ai.IsoCode = reader.GetValue<string>("ISOCode", String.Empty);
                    ai.IsoCode3 = reader.GetValue<string>("ISOCode3", String.Empty);
                    ai.CountryCode = reader.GetValue<string>("CountryCode", String.Empty);
                    ai.CountryName = reader.GetValue<string>("CountryName", String.Empty);
                    ai.PhoneNumber = reader.GetValue<string>("PhoneNumber", String.Empty);
                }
            }
            return ai;
        }
    }
}
