using System;

namespace OsApiModels.Carrier
{

    #region ups serialization classes
    public class UpsTracking
    {
        public UpsSecurity UPSSecurity;
        public UpsTrackRequest TrackRequest;
    }

    public class UpsTrackRequest
    {
        public UpsTRequest Request;
        public string InquiryNumber;
    }

    public class UpsTRequest
    {
        public string RequestOption;
        public UpsTransactionReference TransactionReference;
    }

    //=========================

    public class UpsReturnReq
    {
        public UpsSecurity UPSSecurity;
        public UpsRetShipmentRequest ShipmentRequest;
    }

    public class UpsRetShipmentRequest
    {
        public UpsRequest Request;
        public UpsReturn Shipment;
        public UpsLabelSpecification LabelSpecification;
    }

    public class UpsReturn
    {
        public string Description;
        public UpsReturnServices ReturnServices;
        public UpsShipper Shipper;
        public UpsShipContact ShipTo;
        public UpsShipContact ShipFrom;
        public UpsPaymentInformation PaymentInformation;
        public UpsCodeDescription Service;
        public UpsShipmentRatingOptions ShipmentRatingOptions;
        public UpsPackage Package;
    }

    public class UpsReturnServices
    {
        public string Code;
        public string Description;
    }

    //=========================

    public class UpsShipReq
    {
        public UpsSecurity UPSSecurity;
        public UpsShipmentRequest ShipmentRequest;
    }

    public class UpsLabelReq
    {
        public UpsSecurity UPSSecurity;
        public UpsLabelRecoveryRequest LabelRecoveryRequest;
    }

    public class UpsLabelRecoveryRequest
    {
        public UpsLabelSpecification LabelSpecification;
        public string TrackingNumber;
    }

    public class UpsSecurity
    {
        public UpsUsernameToken UsernameToken;
        public UpsServiceAccessToken ServiceAccessToken;

    }

    public class UpsShipmentRequest
    {
        public UpsRequest Request;
        public UpsShipment Shipment;
        public UpsLabelSpecification LabelSpecification;
    }

    public class UpsLabelSpecification
    {
        public UpsCodeDescription LabelImageFormat;
        public string HTTPUserAgent;
    }

    public class UpsShipment
    {
        public string Description;
        public UpsShipper Shipper;
        public UpsShipContact ShipTo;
        public UpsShipContact ShipFrom;
        public UpsPaymentInformation PaymentInformation;
        public UpsCodeDescription Service;
        public UpsShipmentRatingOptions ShipmentRatingOptions;
        public UpsPackage Package;
    }

    public class UpsPackage
    {
        public string Description;
        public UpsCodeDescription Packaging;
        public UpsPackageWeight PackageWeight;
        public UpsReferenceNumber ReferenceNumber;
    }

    public class UpsReferenceNumber
    {
        public string Code;
        public string Value;
    }

    public class UpsPackageWeight
    {
        public UpsCodeDescription UnitOfMeasurement;
        public string Weight;
    }

    public class UpsCodeDescription
    {
        public string Code;
        public string Description;
    }

    public class UpsShipmentRatingOptions
    {
        public string NegotiatedRatesIndicator;
    }

    public class UpsPaymentInformation
    {
        public UpsShipmentCharge ShipmentCharge;
    }

    public class UpsShipmentCharge
    {
        public string Type;
        public UpsBillShipper BillShipper;
    }

    public class UpsBillShipper
    {
        public string AccountNumber;
    }

    public class UpsShipContact
    {
        public string Name;
        public string AttentionName;
        public UpsPhone Phone;
        public UpsAddress Address;
    }

    public class UpsShipper
    {
        public string Name;
        public string AttentionName;
        public string TaxIdentificationNumber;
        public UpsPhone Phone;
        public string ShipperNumber;
        public UpsAddress Address;
    }

    public class UpsAddress
    {
        public string AddressLine;
        public string City;
        public string StateProvinceCode;
        public string PostalCode;
        public string CountryCode;
    }

    public class UpsPhone
    {
        public string Number;
    }

    public class UpsRequest
    {
        public string RequestOption;
        public UpsTransactionReference TransactionReference;
    }

    public class UpsTransactionReference
    {
        public string CustomerContext;
    }

    public class UpsUsernameToken
    {
        public string Username;
        public string Password;
    }

    public class UpsServiceAccessToken
    {
        public string AccessLicenseNumber;
    }
    #endregion
}
