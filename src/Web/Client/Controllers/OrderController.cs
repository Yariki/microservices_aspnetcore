using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Models;
using Client.Models.OrderModels;
using Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebMvc.Controllers
{

    [Authorize]
    public class OrderController : Controller
    {
        private readonly ICartService _cartSvc;
        private readonly IOrderService _orderSvc;
        private readonly IIdentityService<ApplicationUser> _identitySvc;
        private readonly ILogger<OrderController> _logger;


        public OrderController(ILogger<OrderController> logger, IOrderService orderSvc, ICartService cartSvc, IIdentityService<ApplicationUser> identitySvc)
        {
            _identitySvc = identitySvc;
            _orderSvc = orderSvc;
            _cartSvc = cartSvc;
            _logger = logger;
        }

        public async Task<IActionResult> Create()
        {
            var user = _identitySvc.Get(HttpContext.User);
            var cart = await _cartSvc.GetCart(user);
            Order order;
            if(cart != null)
            { 
                order = _cartSvc.MapCartToOrder(cart);
                return View(order);
            }

            return View(new Order());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order frmOrder)
        {
            if (!ModelState.IsValid)
            {
                return View(frmOrder);
            }
            var user = _identitySvc.Get(HttpContext.User);

            Order order = frmOrder;

            order.UserName = user.Email;
            order.BuyerId = user.Id;
            order.OrderTotal = GetTotal(order.OrderItems);

            try
            {

                int orderId = await _orderSvc.CreateOrder(order);

                await _cartSvc.ClearCart(user);
                return RedirectToAction("Complete", new { id = orderId, userName = user.UserName });

            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error,e.Message,e);
                return BadRequest(e);
            }
        }

        public IActionResult Complete(int id, string userName)
        {

            _logger.LogInformation("User {userName} completed checkout on order {orderId}.", userName, id);
            return View(id);

        }


        public async Task<IActionResult> Detail(string orderId)
        {
            var user = _identitySvc.Get(HttpContext.User);

            var order = await _orderSvc.GetOrder(orderId);
            return View(order);
        }

        public async Task<IActionResult> Index()
        {
            var vm = await _orderSvc.GetOrders();
            return View(vm);
        }


        private decimal GetTotal(List<OrderItem> orderItems)
        {
            return orderItems.Select(p => p.UnitPrice * p.Units).Sum();

        }
    }
}