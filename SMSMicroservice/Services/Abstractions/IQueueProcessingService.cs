namespace SMSMicroservice;

public interface IQueueProcessingService
{
    Task ReceiveMessage();
}
