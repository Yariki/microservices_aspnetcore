using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Models.OrderModels;

namespace Client.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrders();
        
        Task<Order> GetOrder(string orderId);
        Task<int> CreateOrder(Order order);
    }
}