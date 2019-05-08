using System;
using System.IO;
using System.Reflection;
using log4net;
using OsApiModels.Users;
using OsApiModels.Order;
using OsApiModels.Carrier;
using OSAPIData.DataLayer;
using OSAPIServices.Service.Orders;
using OSAPIServices.Service.Platform;
using OSAPIServices.Service.Logging;

namespace OSAPIServices.Service.Carrier
{
    public class UpsService : ICarrierService
    {
        private readonly IPlatformService _platformService;
        private readonly IOrderService _orderService;
        private readonly IUpsClient _upsClient;
        private readonly ApiUser _apiUser;
        private readonly ICarrierRepository _carrierRepository;
        private string upsAccessKey;
        private string upsAccountNumber;
        private string upsUserName;
        private string upsPassword;
        private string upsShipRequestEndpoint;
        private string upsLabelRecoveryEndpoint;
        private string upsTrackingEndpoint;
        private string _shipLabelsMediaFolder = "shiplabels";
        private string _shipLabelsFileExtension = ".gif";
        private readonly ILoggingService _loggingService;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public UpsService(IPlatformService platformService, IOrderService orderService, ICarrierRepository carrierRepository, ApiUser apiUser, IUpsClient upsClient, ILoggingService loggingService)
        {
            _platformService = platformService;
            _orderService = orderService;
            _carrierRepository = carrierRepository;
            _apiUser = apiUser;
            _upsClient = upsClient;
            _loggingService = loggingService;
            upsAccessKey = _platformService.GetSetting<string>("UpsApiAccessKey");
            upsAccountNumber = _platformService.GetSetting<string>("UpsApiAccountNumber");
            upsUserName = _platformService.GetSetting<string>("UpsApiUserName");
            upsPassword = _platformService.GetSetting<string>("UpsApiPassword");
            upsShipRequestEndpoint = _platformService.GetSetting<string>("UpsApiShipRequestEndpoint");
            upsLabelRecoveryEndpoint = _platformService.GetSetting<string>("UpsApiLabelRecoveryEndpoint");
            upsTrackingEndpoint = _platformService.GetSetting<string>("UpsApiTrackingEndpoint");
        }

        public ReturnLabelDetails CreateReturnLabel(int rmaNumber, string shipTypeCode = null, string shipTypeName = null, string packageWeight = null)
        {
            var orderDetails = _orderService.GetOrderDetailsByRMA(rmaNumber);
            var homeFacilityAddress = _carrierRepository.GetClientHomeFacilityAddress(_apiUser.ClientId);
            var fullRmaNum = String.Format("{0}-{1}", _apiUser.ApiUserName, rmaNumber);
            var retReq = GetReturnRequestObject(homeFacilityAddress, orderDetails.BillingAddressInfo, orderDetails.OrderNumber, fullRmaNum, shipTypeCode ?? "03", shipTypeName ?? "UPS Ground", packageWeight ?? "1");
            if (retReq != null)
            {
                var reqJson = Newtonsoft.Json.JsonConvert.SerializeObject(retReq);
                var resp = _upsClient.HttpPostWebForm(upsShipRequestEndpoint, reqJson);
                var tData = ParseShipResponse(resp);
                if (tData != null)
                {
                    var labelUrl = GetReturnLabelUrl(tData.Item1);
                    var isSaved = SaveLabelImage(tData.Item1, tData.Item2);
                    return new ReturnLabelDetails() 
                    {
                        gifBase64 = tData.Item2,
                        htmlBase64 = tData.Item3,
                        trackingNumber = tData.Item1,
                        labelUrl = (isSaved ? labelUrl : "error. access denied. could not write file.")
                    };
                }
            }
            return null;
        }

        public ReturnLabelDetails CreateShipmentLabel(int orderId, string shipTypeCode = null, string shipTypeName = null, string packageWeight = null)
        {
            var orderDetails = _orderService.GetOrderDetails(orderId);
            var homeFacilityAddress = _carrierRepository.GetClientHomeFacilityAddress(_apiUser.ClientId);
            var fullOrderId = String.Format("{0}-{1}", _apiUser.ApiUserName, orderId);
            var shipReq = GetShipRequestObject(homeFacilityAddress, orderDetails.ShippingAddressInfo, fullOrderId, shipTypeCode ?? "03", shipTypeName ?? "UPS Ground", packageWeight ?? "1");
            if (shipReq != null)
            {
                var reqJson = Newtonsoft.Json.JsonConvert.SerializeObject(shipReq);
                var resp = _upsClient.HttpPostWebForm(upsShipRequestEndpoint, reqJson);
                var tData = ParseShipResponse(resp);
                if (tData != null)
                {
                    var isSaved = SaveLabelImage(tData.Item1, tData.Item2);
                    return new ReturnLabelDetails()
                    {
                        gifBase64 = tData.Item2,
                        htmlBase64 = tData.Item3,
                        trackingNumber = tData.Item1,
                        labelUrl =
                            (isSaved
                                ? GetReturnLabelUrl(tData.Item1)
                                : "error. access denied. could not write file.")
                    };
                }
            }
            return null;
        }

        public ReturnLabelDetails GetShippingLabel(string trackingNumber)
        {
            var userAgent = System.Web.HttpContext.Current.Request.UserAgent;

            var lblReq = GetLabelRecoveryRequestObject(userAgent, trackingNumber);

            if (lblReq != null)
            {
                var reqJson = Newtonsoft.Json.JsonConvert.SerializeObject(lblReq);
                var resp = _upsClient.HttpPostWebForm(upsLabelRecoveryEndpoint, reqJson);
                var tData = ParseLabelResponse(resp);
                if (tData != null)
                    return new ReturnLabelDetails() { gifBase64 = tData.Item2, htmlBase64 = tData.Item3, trackingNumber = tData.Item1 };
            }

            return null;
        }

        public object GetTracking(string trackingNumber)
        {
            var trackRequest = GetTrackingRequestObject(trackingNumber);
            if (trackRequest != null)
            {
                var reqJson = Newtonsoft.Json.JsonConvert.SerializeObject(trackRequest);
                var resp = _upsClient.HttpPostWebForm(upsTrackingEndpoint, reqJson);
                return ParseTrackingResponse(resp);
            }
            return null;
        }

        public string GetReturnLabelUrl(string trackingNumber)
        {
            return String.Format("{0}/{1}/{2}{3}", _platformService.GetSetting<string>("MediaFileUrl"),
                _shipLabelsMediaFolder, trackingNumber, this._shipLabelsFileExtension);
        }

        //=============================================================================================================

        private UpsReturnReq GetReturnRequestObject(AddressInfo facilityAddress, AddressInfo customerAddress, string fullOrderId,
                string rmaNumber, string shipTypeCode, string shipTypeName, string packageWeight)
        {
            //concat strings now, sto later can validate combined string length
            var shipToName = String.Format("{0} {1}", facilityAddress.FirstName, facilityAddress.LastName);
            if (shipToName.Length > 35)
                shipToName = shipToName.Substring(0, 35);
            var shipToAddress = String.Format("{0}{1}", facilityAddress.AddressLine1,
                                    (String.IsNullOrEmpty(facilityAddress.AddressLine2) ? string.Empty : " " + facilityAddress.AddressLine2));
            if (shipToAddress.Length > 35)
                shipToAddress = shipToAddress.Substring(0, 35);
            var shipFromName = String.Format("{0} {1}", customerAddress.FirstName, customerAddress.LastName);
            if (shipFromName.Length > 35)
                shipFromName = shipFromName.Substring(0, 35);

            return new UpsReturnReq()
            {
                UPSSecurity = new UpsSecurity()
                {
                    ServiceAccessToken = new UpsServiceAccessToken() { AccessLicenseNumber = upsAccessKey },
                    UsernameToken = new UpsUsernameToken() { Password = upsPassword, Username = upsUserName }
                },
                ShipmentRequest = new UpsRetShipmentRequest()
                {
                    Request = new UpsRequest()
                    {
                        RequestOption = "validate",
                        TransactionReference = new UpsTransactionReference() { CustomerContext = string.Empty }
                    },
                    Shipment = new UpsReturn()
                    {
                        Description = string.Empty,
                        ReturnServices = new UpsReturnServices() { Code = "9", Description = "UPS Print Return Label (PRL)" },
                        Shipper = new UpsShipper()
                        {
                            Name = shipToName,
                            AttentionName = string.Empty,
                            TaxIdentificationNumber = "",
                            Phone = new UpsPhone() { Number = facilityAddress.PhoneNumber },
                            ShipperNumber = upsAccountNumber,
                            Address = new UpsAddress()
                            {
                                AddressLine = shipToAddress,
                                City = facilityAddress.City,
                                StateProvinceCode = facilityAddress.State,
                                PostalCode = facilityAddress.PostalCode,
                                CountryCode = facilityAddress.IsoCode
                            }
                        },
                        ShipTo = new UpsShipContact()
                        {
                            Name = shipToName,
                            AttentionName = string.Empty,
                            Phone = new UpsPhone() { Number = facilityAddress.PhoneNumber },
                            Address = new UpsAddress()
                            {
                                AddressLine = shipToAddress,
                                City = facilityAddress.City,
                                StateProvinceCode = facilityAddress.State,
                                PostalCode = facilityAddress.PostalCode,
                                CountryCode = facilityAddress.IsoCode
                            }
                        },
                        ShipFrom = new UpsShipContact()
                        {
                            Name = shipFromName,
                            AttentionName = string.Empty,
                            Phone = new UpsPhone() { Number = facilityAddress.PhoneNumber },
                            Address = new UpsAddress()
                            {
                                AddressLine = shipToAddress,
                                City = facilityAddress.City,
                                StateProvinceCode = facilityAddress.State,
                                PostalCode = facilityAddress.PostalCode,
                                CountryCode = facilityAddress.IsoCode
                            }
                        },
                        PaymentInformation = new UpsPaymentInformation()
                        {
                            ShipmentCharge = new UpsShipmentCharge()
                            {
                                Type = "01",
                                BillShipper = new UpsBillShipper() { AccountNumber = upsAccountNumber }
                            }
                        },
                        Service = new UpsCodeDescription() { Code = shipTypeCode, Description = shipTypeName },
                        ShipmentRatingOptions = new UpsShipmentRatingOptions() { NegotiatedRatesIndicator = "0" },
                        Package = new UpsPackage()
                        {
                            Description = "Retail Product",
                            Packaging = new UpsCodeDescription() { Code = "02", Description = "Customer Supplied" },
                            PackageWeight = new UpsPackageWeight()
                            {
                                UnitOfMeasurement = new UpsCodeDescription() { Code = "LBS", Description = "Pounds" },
                                Weight = packageWeight
                            },
                            ReferenceNumber = new UpsReferenceNumber()
                            {
                                Code = "00",
                                Value = String.Format("{0}   (RMA {1})", fullOrderId, rmaNumber)
                            }
                        }
                    },
                    LabelSpecification = new UpsLabelSpecification()
                    {
                        HTTPUserAgent = string.Empty,
                        LabelImageFormat = new UpsCodeDescription() { Code = "GIF", Description = "GIF" }
                    }
                }
            };
        }

        private UpsShipReq GetShipRequestObject(AddressInfo facilityAddress, AddressInfo customerAddress, string fullOrderId, 
                string shipTypeCode, string shipTypeName, string packageWeight)
        {
            //concat strings now, sto later can validate combined string length
            var shipToName = String.Format("{0} {1}", facilityAddress.FirstName, facilityAddress.LastName);
            if (shipToName.Length > 35)
                shipToName = shipToName.Substring(0, 35);
            var shipToAddress = String.Format("{0}{1}", facilityAddress.AddressLine1,
                                    (String.IsNullOrEmpty(facilityAddress.AddressLine2) ? string.Empty : " " + facilityAddress.AddressLine2));
            if (shipToAddress.Length > 35)
                shipToAddress = shipToAddress.Substring(0, 35);
            var shipFromName = String.Format("{0} {1}", customerAddress.FirstName, customerAddress.LastName);
            if (shipFromName.Length > 35)
                shipFromName = shipFromName.Substring(0, 35);

            return new UpsShipReq()
            {
                UPSSecurity = new UpsSecurity()
                {
                    ServiceAccessToken = new UpsServiceAccessToken() { AccessLicenseNumber = upsAccessKey },
                    UsernameToken = new UpsUsernameToken() { Password = upsPassword, Username = upsUserName }
                },
                ShipmentRequest = new UpsShipmentRequest()
                {
                    Request = new UpsRequest()
                    {
                        RequestOption = "validate",
                        TransactionReference = new UpsTransactionReference() { CustomerContext = string.Empty }
                    },
                    Shipment = new UpsShipment()
                    {
                        Description = string.Empty,
                        Shipper = new UpsShipper()
                        {
                            Name = shipToName,
                            AttentionName = string.Empty,
                            TaxIdentificationNumber = "",
                            Phone = new UpsPhone() { Number = facilityAddress.PhoneNumber },
                            ShipperNumber = upsAccountNumber,
                            Address = new UpsAddress()
                            {
                                AddressLine = shipToAddress,
                                City = facilityAddress.City,
                                StateProvinceCode = facilityAddress.State,
                                PostalCode = facilityAddress.PostalCode,
                                CountryCode = facilityAddress.IsoCode
                            }
                        },
                        ShipTo = new UpsShipContact()
                        {
                            Name = shipToName,
                            AttentionName = string.Empty,
                            Phone = new UpsPhone() { Number = facilityAddress.PhoneNumber },
                            Address = new UpsAddress()
                            {
                                AddressLine = shipToAddress,
                                City = facilityAddress.City,
                                StateProvinceCode = facilityAddress.State,
                                PostalCode = facilityAddress.PostalCode,
                                CountryCode = facilityAddress.IsoCode
                            }
                        },
                        ShipFrom = new UpsShipContact()
                        {
                            Name = shipFromName,
                            AttentionName = string.Empty,
                            Phone = new UpsPhone() { Number = facilityAddress.PhoneNumber },
                            Address = new UpsAddress()
                            {
                                AddressLine = shipToAddress,
                                City = facilityAddress.City,
                                StateProvinceCode = facilityAddress.State,
                                PostalCode = facilityAddress.PostalCode,
                                CountryCode = facilityAddress.IsoCode
                            }
                        },
                        PaymentInformation = new UpsPaymentInformation()
                        {
                            ShipmentCharge = new UpsShipmentCharge()
                            {
                                Type = "01",
                                BillShipper = new UpsBillShipper() { AccountNumber = upsAccountNumber }
                            }
                        },
                        Service = new UpsCodeDescription() { Code = shipTypeCode, Description = shipTypeName },
                        ShipmentRatingOptions = new UpsShipmentRatingOptions() { NegotiatedRatesIndicator = "0" },
                        Package = new UpsPackage()
                        {
                            Description = "Retail Product",
                            Packaging = new UpsCodeDescription() { Code = "02", Description = "Customer Supplied" },
                            PackageWeight = new UpsPackageWeight()
                            {
                                UnitOfMeasurement = new UpsCodeDescription() { Code = "LBS", Description = "Pounds" },
                                Weight = packageWeight
                            },
                            ReferenceNumber = new UpsReferenceNumber()
                            {
                                Code = "00",
                                Value = fullOrderId
                            }
                        }
                    },
                    LabelSpecification = new UpsLabelSpecification()
                    {
                        HTTPUserAgent = string.Empty,
                        LabelImageFormat = new UpsCodeDescription() { Code = "GIF", Description = "GIF" }
                    }
                }
            };
        }

        private UpsLabelReq GetLabelRecoveryRequestObject(string userAgent, string trackingNumber)
        {
            if (!String.IsNullOrEmpty(userAgent) && userAgent.Length > 65)
                userAgent = userAgent.Substring(0, 65);

            return new UpsLabelReq()
            {
                UPSSecurity = new UpsSecurity()
                {
                    ServiceAccessToken = new UpsServiceAccessToken() { AccessLicenseNumber = upsAccessKey },
                    UsernameToken = new UpsUsernameToken() { Password = upsPassword, Username = upsUserName }
                },
                LabelRecoveryRequest = new UpsLabelRecoveryRequest()
                {
                    TrackingNumber = trackingNumber,
                    LabelSpecification = new UpsLabelSpecification() { HTTPUserAgent = userAgent, LabelImageFormat = new UpsCodeDescription() { Code = "GIF" } }
                }
            };
        }

        private UpsTracking GetTrackingRequestObject(string trackingNumber)
        {
            return new UpsTracking()
            {
                UPSSecurity = new UpsSecurity()
                {
                    ServiceAccessToken = new UpsServiceAccessToken() { AccessLicenseNumber = upsAccessKey },
                    UsernameToken = new UpsUsernameToken() { Password = upsPassword, Username = upsUserName }
                },
                TrackRequest = new UpsTrackRequest()
                {
                    InquiryNumber = trackingNumber,
                    Request = new UpsTRequest()
                    {
                        RequestOption = "1",
                        TransactionReference = new UpsTransactionReference() { CustomerContext = string.Empty }
                    }
                }
            };
        }

        private Tuple<string, string, string> ParseShipResponse(string json)
        {
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            if (response == null)
            {
                LogError("Could not deserialize carrier Ship Response json.");
                return null;
            }
            if (response["Fault"] != null)
            {
                //Carrier is returning an error response in JSON. Parse and log it.
                ParseFaultResponse(response);
                return null;
            }
            if (response["ShipmentResponse"] == null)
            {
                LogError("ShipmentResponse element not found in carrier Ship Response json.");
                return null;
            }
            else if (response["ShipmentResponse"]["Response"] == null)
            {
                LogError("ShipmentResponse/Response element not found in carrier Ship Response json.");
                return null;
            }
            else if (response["ShipmentResponse"]["Response"]["ResponseStatus"] == null)
            {
                LogError("ShipmentResponse/Response/ResponseStatus element not found in carrier Ship Response json.");
                return null;
            }
            else if (response["ShipmentResponse"]["Response"]["ResponseStatus"]["Code"] == null)
            {
                LogError("ShipmentResponse/Response/ResponseStatus/Code element not found in carrier Ship Response json.");
                return null;
            }
            var status = response["ShipmentResponse"]["Response"]["ResponseStatus"]["Code"];
            if (status.Value != "1")
            {
                LogError(String.Format("CODE element in carrier Ship Response json is {0}.", status.Value));
                return null;
            }
            if (response["ShipmentResponse"]["ShipmentResults"] == null || response["ShipmentResponse"]["ShipmentResults"]["PackageResults"] == null)
            {
                LogError("PACKAGERESULTS element not found in carrier Ship Response json.");
                return null;
            }
            var packageResults = response["ShipmentResponse"]["ShipmentResults"]["PackageResults"];
            if (packageResults["ShippingLabel"] == null || packageResults["ShippingLabel"]["GraphicImage"] == null)
            {
                LogError("GRAPHICIMAGE element not found in carrier Ship Response json.");
                return null;
            }
            return Tuple.Create(packageResults["TrackingNumber"].Value, packageResults["ShippingLabel"]["GraphicImage"].Value, packageResults["ShippingLabel"]["HTMLImage"].Value);
        }

        private Tuple<string, string, string> ParseLabelResponse(string json)
        {
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            if (response == null)
                return null;
            if (response["Fault"] != null)
                return null;
            var status = response["LabelRecoveryResponse"]["Response"]["ResponseStatus"]["Code"];
            if (status == null || status.Value != "1")
                return null;
            var labelResults = response["LabelRecoveryResponse"]["LabelResults"];
            if (labelResults == null)
                return null;
            return Tuple.Create(labelResults["TrackingNumber"].Value, labelResults["LabelImage"]["GraphicImage"].Value, labelResults["LabelImage"]["HTMLImage"].Value);
        }

        private object ParseTrackingResponse(string json)
        {
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            if (response == null)
                return null;
            if (response["Fault"] != null)
                return response;
            var status = response["TrackResponse"]["Response"]["ResponseStatus"]["Code"];
            if (status == null || status.Value != "1")
                return null;
            var trackingHistory = response["TrackResponse"]["Shipment"];
            if (trackingHistory == null)
                return null;
            return trackingHistory.Value;
        }

        private void ParseFaultResponse(dynamic response)
        {
            var errStr = string.Empty;
            if (response["Fault"]["detail"] == null)
            {
                errStr = "Fault/Detail element not found in carrier Fault Response json.";
            }
            else if (response["Fault"]["detail"]["Errors"] == null)
            {
                errStr = "Fault/Detail/Errors element not found in carrier Fault Response json.";
            }
            else if (response["Fault"]["detail"]["Errors"]["ErrorDetail"] == null)
            {
                errStr = "Fault/Detail/Errors/ErrorDetail element not found in carrier Fault Response json.";
            }
            else if (response["Fault"]["detail"]["Errors"]["ErrorDetail"]["PrimaryErrorCode"] == null)
            {
                errStr = "Fault/Detail/Errors/ErrorDetail/PrimaryErrorCode element not found in carrier Fault Response json.";
            }
            else if (response["Fault"]["detail"]["Errors"]["ErrorDetail"]["PrimaryErrorCode"]["Description"] == null)
            {
                errStr = "Fault/Detail/Errors/ErrorDetail/PrimaryErrorCode/Description element not found in carrier Fault Response json.";
            }
            else
            {
                errStr = response["Fault"]["detail"]["Errors"]["ErrorDetail"]["PrimaryErrorCode"]["Description"].Value;
            }
            if (!String.IsNullOrEmpty(errStr))
            {
                LogError(String.Format("UPSService Fault response. {0}", errStr));
            }
        }

        private void LogError(string msg)
        {
            //log 4 net
            Log.Error(msg);
            //email
            var ex = new Exception(msg);
            _loggingService.SendDetailException(this, ex);
        }

        private bool SaveLabelImage(string tracking, string gif)
        {

            var retVal = false;
            var server = System.Configuration.ConfigurationManager.AppSettings["MediaServer"];
            var mediaFolderPath = System.Configuration.ConfigurationManager.AppSettings["MediaFolderPath"];
            if (!String.IsNullOrEmpty(server))
            {
                var path = string.Empty;
                string mediaFolderPathFormatted = string.Empty;
                try
                {
                    mediaFolderPathFormatted = String.Format(mediaFolderPath, _apiUser.ApiUserName.ToLower());
                }
                catch (Exception ex)
                {
                    _loggingService.SendDetailException(this, ex);
                    ex.Data.Add("UPSService", "Problem parsing and formatting MediaFolderPath setting in store settings.");
                    retVal = false;
                }

                path = String.Format(@"{0}\{1}\{2}", server.TrimEnd('\\'), mediaFolderPathFormatted, this._shipLabelsMediaFolder);

                try
                {
                    //if tracking folder does not exist, create it
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    File.WriteAllBytes(String.Format(@"{0}\{1}{2}", path, tracking, this._shipLabelsFileExtension), Convert.FromBase64String(gif));
                }
                catch (Exception ex)
                {
                    ex.Data.Add("UPSService", string.Format("Problem writing tracking file image for tracking# {0}.", tracking));
                    _loggingService.SendDetailException(this, ex);
                    retVal = false;
                }
            }
            return true;
        }
    }
}
