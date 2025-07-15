using System.Collections.Generic;

namespace PharmaMoov.API.Helpers
{
    public class APIConfigurationManager
    {
        public DataStr DataStrings { get; set; }
        public Token TokenKeys { get; set; }
        public PushNotificationConfig PNConfig { get; set; }
        public SMTPConfig MailConfig { get; set; }
        public string HostURL { get; set; }
        public string DefaultClientSite { get; set; }
        public string MapUrl { get; set; }
        public string WebAPILink { get; set; }
        public SmsParameter SmsConfig { get; set; }
        public MessageConfigurations MsgConfigs { get; set; }
        public DeliveryJobConfig DeliveryJobConfig { get; set; }
        public PaymentConfig PaymentConfig { get; set; }
        public HostedServicesConfig HostedServicesConfig { get; set; }
        public PushNotifMessages PushNotifMessages { get; set; }
    }

    public class DataStr
    {
        public string ConnStr { get; set; }
    }

    public class Token
    {
        public string Key { get; set; }
        public double Exp { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }

    }

    public class PushNotificationConfig
    {
        public string FireBLink { get; set; }
        public string IOSFireBKey { get; set; }
        public string AndroidFireBKey { get; set; }
        public string IOSSenderID { get; set; }
        public string AndroidSenderID { get; set; }
    }

    public class SMTPConfig
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RegistrationLink { get; set; }
        public int Port { get; set; }
        public bool EnableSSL { get; set; }

        public IEnumerable<string> To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; internal set; }
    }

    public class SmsParameter
    {

        public string Endpoint { get; set; }

        // required
        public string Action { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }

        // optional
        public int Maxsplit { get; set; }
        public string Scheduledatetime { get; set; }
        public string Optout { get; set; }
        public string Api { get; set; }
        public string Apireply { get; set; }

        public SmsParameter()
        {
            // TODO : initialize optional parameters?
        }
    }

    public class MessageConfigurations
    {
        public string RegisterMobileUser { get; set; }
        public string RegisterShopUser { get; set; }
        public string ForgotPassword { get; set; }
        public string ChangeNumber { get; set; }
        public string ResendCode { get; set; }
        public string InvoiceOFD { get; set; }
        public string InvoiceRFP { get; set; }
        public string CustomerForgotPassword { get; set; }
        public string ApproveUserAccount { get; set; }
        public string WelcomeMsg { get; set; }
        public string OrderPlaced { get; set; }
        public string OrderPlacedForShop { get; set; }
        public string OrderCompleted { get; set; }
        public string OrderCancelled { get; set; }
        public string OrderRefunded { get; set; }
        public string ShopRequest { get; set; }
        public string ShopConcern { get; set; }
        public string GeneralConcern { get; set; }
        public string HelpRequest { get; set; }
        public string ClosedShop { get; set; }
    }
    public class DeliveryJobConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string BaseUrl { get; set; }
        public string TokenEndpoint { get; set; }
        public string AddressValidationEndpoint { get; set; }
        public string PricingEndpoint { get; set; }
        public string JobValidationEndpoint { get; set; }
        public string JobCreationEndpoint { get; set; }
    }
    public class PaymentConfig
    {
        public string ClientId { get; set; }
        public string ApiKey { get; set; }
        public string CreditedId { get; set; }
        public string WalletId { get; set; }
        public string BaseUrl { get; set; }
        public string ReturlUrl { get; set; }
        public string WebReturlUrl { get; set; }
    }
    public class PushNotifMessages
    {
        public string OrderStatus { get; set; }
    }

    public class HostedServicesConfig
    {
        public bool EnableOrderAutoCancel { get; set; }
        public int AutoCancelRunningIntervalMins { get; set; }
        public bool EnabledAutomaticTransfer { get; set; }
        public bool EnabledDeliveryTransfer { get; set; }
        public int AutomaticFundsTransferIntervalHrs { get; set; }
        public int HostedServiceRunningIntervalMins { get; set; }
        public bool EnableAutoGenerateCommissionInvoice { get; set; }
        public int AutoCommissionInvoiceIntervalMins { get; set; }
        public bool EnableAutoPullDeliveryDistance { get; set; }
        public int AutoPullDeliveryDistanceIntervalMins { get; set; }
    }
}
