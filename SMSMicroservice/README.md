
# SMS Microservice

This repository contains an example implementation of the SMS microservice for M-KOPA, written in .NET Core. This microservice acts as a wrapper around a third-party SMS service API, providing an asynchronous and reliable solution for sending SMS messages to customers.


## Functionality

- Receives SendSms commands containing phone number, message text, and idempoten key from a message queue.
- Sends asynchronous HTTP requests to the third-party SMS service API.
- Publishes "SmsSent" events to a global event bus upon successful delivery.
- Leverages abstractions for message queue, event bus, logger and SMSService for flexible integration.


## Tech Stack
**Server:** .Net Core 8

### Abstractions
#### 
    1. IQueueClient 
Concrete implementation must be provideed. An abstract class `QueueProcessor` has been provided that should be inherited with it's methods overridden and returned from the `CreateProcessor` method. This allows for proper handling of messages within the queue from the `QueueProcessingService` class.

    2. ISMSService
This currently contains one method called send SMS that needs to be implementated in a Concrete class. This methid is meant to contain the implementation of the actual API call to the Third Party API. It should yield a boolean response with `true ` indicating a success and `false` indicating that the sms failed to send.

    3. ICacheService
The cache service was introduced for idempotent purpose. It takes care of storing the idempotent key/id sent from the client. Concrete implementation is needed for this too in order to be able to track which SMS has been successfuly handled or not.

    4. QueueProcessor
The `QueueProcessor` class is an abstract class with the method and event required for the `QueueProcessingService` and the `IQueueClient` to function as expected. This class can be futher extended to macke sure that the `QueueProcessingService` is robust enough especially for retries. 

    5. IQueueProcessingService
This interface has only on e method and has already been implemented by the `QueueProcessingService` and has also been added to the DI. The `ReceiveMessage` method calls the `IQueueClient.CreateProcessor` with needed parameters and then starts processing which could be trigger a background job or a hosted service with the goal of continously listening for events in the queue and processing them via the right mehod as specified in the `IQueueClient.CreateProcessor` method.

    6. INotificationService
This is the interface that astracts the function of actually handling the notification/sms. This service can be extended for other purposes but for now it's only function is to trigger a call to the `ISMSService` to call the thrid party api and also add/remove idempotent keys to the cache via the ICacheService.

    7. IEventBus
This interface is responsible for the bit of publishing the `SmsSent` event to the Global event queue. A concrete implementation is needed for this as well.



## Dependencies
This solution depends on concrete implementations for the message queue, event bus, and logger interfaces. These will be injected through dependency injection mechanisms. Additionally, a concrete implementation for the `ISMSService` interacting with the third-party SMS service API needs to be developed separately.
## Getting Started

Implement concrete classes for `IQueueClient`, `IEventBus`, and `ILogger` based on your chosen messaging technologies and logging frameworks.
Develop the `ISMSService` implementation interacting with the specific API of your third-party SMS service provider.
Configure dependency injection to provide these concrete implementations to the SmsMicroservice.
Implement a background service to call `ReceiveMessage` to continuously process incoming messages.
## Other Considerations
In an ideal scenario with more time, I would also consider some additional performance optimizations for the SMS microservice. Here are some areas I'd explore:


- Threading and Concurrency:  For high message volumes, I'd ensure thread-safe access to shared resources and leverage asynchronous processing throughout the service. This keeps things responsive under heavy load.

- Error Handling and Retries:  Implementing a retry mechanism with exponential backoff for failed deliveries would be crucial. Additionally, a dead-letter queue for permanently failed messages would allow for further analysis or manual intervention.

- Monitoring and Observability:  Collecting metrics on processing times, success rates, and retries would provide valuable insights into the service's health. Distributed tracing would also be beneficial for tracking message lifecycles across different systems.

- Additional Optimizations:  Depending on the specific use case, batching messages, implementing a circuit breaker pattern for failing services, and exploring message caching could further enhance efficiency.