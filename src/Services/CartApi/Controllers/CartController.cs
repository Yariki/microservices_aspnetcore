using System.Net;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using CartApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CartApi.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CartController : Controller
    {
        private IRedisRepository _repository;
        private ILogger _logger;

        public CartController(IRedisRepository repository, ILoggerFactory factory)
        {
            _repository = repository;
            _logger = factory.CreateLogger<CartController>();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Cart), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string id)
        {
            var basket = await _repository.GetCartAsync(id);

            return Ok(basket);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Cart), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> Post([FromBody] Cart value)
        {
            var basket = await _repository.UpdateCartAsync(value);
            return Ok(basket);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"Delete the cart with id {id}");

            await _repository.DeleteCartAsync(id);

            return Ok();
        }

    }
}