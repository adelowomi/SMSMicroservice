namespace SMSMicroservice;

public interface ISMSService
{
    Task<bool> SendSMS(SMSModel smsModel);
}
