namespace SMSMicroservice;

public abstract class QueueProcessor
{
    public event Func<ProcessEventArgs, Task> ProcessMessageAsync;
    public event Func<ProcessErrorEventArgs, Task> ProcessErrorAsync;

    public abstract Task StartProcessingAsync(CancellationToken cancellationToken = default);

    protected bool ProcessMessageEventIsNull(Func<ProcessEventArgs, Task> processMessageEvent)
    {
        return processMessageEvent == null;
    }

    protected bool ProcessErrorEventIsNull(Func<ProcessErrorEventArgs, Task> processErrorEvent)
    {
        return processErrorEvent == null;
    }
}

// public class QueueProcessorInstance : QueueProcessor
// {
    
//     public override async Task StartProcessingAsync(CancellationToken cancellationToken = default)
//     {
//         await Task.CompletedTask;
//     }
// }

// public class testIQClient : IQueueClient
// {
//     public QueueProcessor CreateProcessor(string QueueName, QueueProcessorOptions model)
//     {
//         return new QueueProcessorInstance();
//     }
// }
