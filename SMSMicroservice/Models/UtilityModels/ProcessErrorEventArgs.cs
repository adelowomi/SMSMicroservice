namespace SMSMicroservice;

public class ProcessErrorEventArgs : EventArgs
{
    public ProcessErrorEventArgs(string message, Exception exception)
    {
        Message = message;
        Exception = exception;
    }

    public string Message { get; }
    public Exception Exception { get; }
}
