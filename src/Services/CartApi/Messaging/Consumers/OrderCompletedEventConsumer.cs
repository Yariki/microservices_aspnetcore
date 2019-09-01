using System.Threading.Tasks;
using CartApi.Model;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Common.Messaging.Consumers
{
    public class OrderCompletedEventConsumer : IConsumer<OrderCompletedEvent>
    {
        private IRedisRepository _cartRepository;
        private ILogger<OrderCompletedEventConsumer> _logger;

        public OrderCompletedEventConsumer(IRedisRepository cartRepository, ILogger<OrderCompletedEventConsumer> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            return _cartRepository.DeleteCartAsync(context.Message.BuyerId);
        }
    }
}