namespace SMSMicroservice;

public class ProcessEventArgs : EventArgs
{
    public ProcessEventArgs(QueueMessage message)
    {
        Message = message;
    }

    public QueueMessage Message { get; }

    public async Task CompleteMessageAsync(QueueMessage message)
    {
        message.IsProcessed = true;
        await Task.CompletedTask;
    }
}

public class QueueMessage 
{
    public string Body { get; set; }
    public bool IsProcessed { get; set; }
}
