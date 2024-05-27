namespace SMSMicroservice;

public interface IQueueClient
{
    QueueProcessor CreateProcessor(string QueueName, QueueProcessorOptions model);
}
