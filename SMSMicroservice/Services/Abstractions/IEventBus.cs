namespace SMSMicroservice;

public interface IEventBus
{
    Task PublishAsync(string command, string message);
}
