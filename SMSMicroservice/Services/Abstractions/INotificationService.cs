namespace SMSMicroservice;

public interface INotificationService
{
    Task HandleNotification(QueueModel notificationQueue);
}
