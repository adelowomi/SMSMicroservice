namespace SMSMicroservice;

public class QueueModel : SMSModel
{
    public Guid IdempotenceKey { get; set; }
}


