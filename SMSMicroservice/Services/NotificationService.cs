
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace SMSMicroservice;

public class NotificationService : INotificationService
{
    private readonly AppSettings _appSettings;
    private readonly ISMSService _smsService;
    private readonly ICacheService _cacheService;
    private readonly IEventBus _eventBus;

    public NotificationService(IOptions<AppSettings> appSettings, ICacheService cacheService, ISMSService smsService, IEventBus eventBus)
    {
        _appSettings = appSettings.Value;
        _cacheService = cacheService;
        _smsService = smsService;
        _eventBus = eventBus;
    }

    public async Task HandleNotification(QueueModel notificationQueue)
    {
        // Check if the idempotence key exists in the database or cache
        bool isKeyExists = await CheckIdempotenceKey(notificationQueue.IdempotenceKey.ToString());

        if (!isKeyExists)
        {
            var SMSModel = new SMSModel
            {
                PhoneNumber = notificationQueue.PhoneNumber,
                Message = notificationQueue.Message,
                Subject = notificationQueue.Subject
            };
            // Send the SMS
            bool sent = await _smsService.SendSMS(SMSModel);

            // throwing an exception here prevents the message from being marked as completed in the queue, so it will be retried
            if (!sent) throw new Exception("Failed to send SMS");

            // Save the idempotence key in the database or cache
            await SaveIdempotenceKey(notificationQueue.IdempotenceKey.ToString());
            await _eventBus.PublishAsync(Commands.SMSSent, "SMS sent successfully");
        }
    }


    private async Task<bool> CheckIdempotenceKey(string idempotenceKey)
    {
        // Implement the logic to check if the idempotence key exists in the cache
        bool isKeyExists = await _cacheService.Get<bool>(idempotenceKey);
        return isKeyExists;
    }

    private async Task SaveIdempotenceKey(string idempotenceKey)
    {
        await _cacheService.Set(idempotenceKey, true, TimeSpan.FromHours(24));
    }
}
