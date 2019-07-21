using System.Collections.Generic;
using System.Threading.Tasks;

namespace CartApi.Model
{
    public interface IRedisRepository
    {
        Task<Cart> GetCartAsync(string cartId);

        IEnumerable<string> GetUsers();

        Task<Cart> UpdateCartAsync(Cart basket);

        Task<bool> DeleteCartAsync(string id);

    }
}