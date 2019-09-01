using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Messaging;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderApi.Data;
using OrderApi.Models;

namespace OrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersContext _orderContext;
        private readonly IOptionsSnapshot<OrderSettings> _settings;
        private readonly ILogger<OrdersController> _logger;
        private IBus _bus;

        public OrdersController(OrdersContext orderContext, IOptionsSnapshot<OrderSettings> settings, ILogger<OrdersController> logger, IBus bus)
        {
            _orderContext = orderContext ?? throw new ArgumentNullException("orderContext");
            _settings = settings;
            _logger = logger;
            _bus = bus;
            ((DbContext) _orderContext).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }


        [HttpPost("new")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            order.OrderStatus = OrderStatus.Preparing;
            order.OrderDate = DateTime.UtcNow;
            _orderContext.Orders.Add(order);
            _orderContext.OrderItems.AddRange(order.OrderItems);
            try
            {
                await _orderContext.SaveChangesAsync();
                _bus.Publish<OrderCompletedEvent>(new {order.BuyerId}).Wait();
                return CreatedAtRoute("GetOrder", new { id = order.OrderId }, order);
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occured: {e.Message}");
                return BadRequest();
            }
        }

        [HttpGet("{id}", Name = "GetOrder")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOrder(int id)
        {
            var item = await _orderContext.Orders.Include(o => o.OrderItems)
                .SingleOrDefaultAsync(o => o.OrderId == id);

            return item != null ? (IActionResult) Ok(item) : NotFound();
        }

        [HttpGet("")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderContext.Orders.Include(o => o.OrderItems).ToListAsync();

            return Ok(orders);
        }

    }
}
