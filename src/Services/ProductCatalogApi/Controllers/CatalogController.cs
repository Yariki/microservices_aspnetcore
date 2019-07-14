using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductCatalogApi.Data;
using ProductCatalogApi.Domain;
using ProductCatalogApi.ViewModels;

namespace ProductCatalogApi.Controllers
{
    [Produces("application/json")]
    [Route("api/catalog")]
    public class CatalogController : Controller
    {
        private readonly CatalogContext _catalogContext;
        private readonly IOptionsSnapshot<CatalogSettings> _settings;
        

        public CatalogController(CatalogContext catalogContext, IOptionsSnapshot<CatalogSettings> settings)
        {
            _catalogContext = catalogContext;
            _settings = settings;
            ((DbContext) _catalogContext).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Types()
        {
            var items = await _catalogContext.CatalogTypes.ToListAsync();
            return Ok(items);
        }
        
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Brands()
        {
            var items = await _catalogContext.CatalogBrands.ToListAsync();
            return Ok(items);
        }

        [HttpGet]
        [Route("items/{id:int}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(c => c.Id == id);
            if (item != null)
            {
                item.PictureUrl = item.PictureUrl.Replace("externalcatalogbaseurltobereplaced", _settings.Value.ExternalCatalogBaseUrl);
                return Ok(item);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Items([FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {
            var total = await _catalogContext.CatalogItems.LongCountAsync();
            var itemsOnPage = await _catalogContext.CatalogItems
                .OrderBy(c => c.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
            itemsOnPage = ChangeUrlPlaceHolder(itemsOnPage);
            var model = new PaginatedItemsViewModel<CatalogItem>(pageSize, pageIndex, (int)total, itemsOnPage);

            return Ok(model);

        }

        [HttpGet]
        [Route("[action]/withname/{name:minlength(1)}")]
        public async Task<IActionResult> Items(string name, [FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {
            var total = await _catalogContext.CatalogItems
                .Where(c => c.Name.StartsWith(name))
                .LongCountAsync();
            var itemsOnPage = await _catalogContext.CatalogItems
                .Where(c => c.Name.StartsWith(name))
                .OrderBy(c => c.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
            itemsOnPage = ChangeUrlPlaceHolder(itemsOnPage);
            var model = new PaginatedItemsViewModel<CatalogItem>(pageSize, pageIndex, (int)total, itemsOnPage);

            return Ok(model);

        }
        
        //GET api/catalog/items/type/2/brand/3[?pageSize=4&pageIndex=2]
        [HttpGet]
        [Route("[action]/type/{catalogTypeId}/brand/{catalogBrandId}")]
        public async Task<IActionResult> Items(int? catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {
            var root = ((IQueryable<CatalogItem>) _catalogContext.CatalogItems);

            if (catalogBrandId.HasValue)
            {
                root = root.Where(c => c.CatalogTypeId == catalogBrandId.Value);
            }

            if (catalogBrandId.HasValue)
            {
                root = root.Where(c => c.CatalogBrandId == catalogBrandId.Value);
            }
            
            var total = await root
                .LongCountAsync();
            var itemsOnPage = await root
                .OrderBy(c => c.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
            itemsOnPage = ChangeUrlPlaceHolder(itemsOnPage);
            var model = new PaginatedItemsViewModel<CatalogItem>(pageSize, pageIndex, (int)total, itemsOnPage);

            return Ok(model);

        }

        [HttpPost]
        [Route("items")]
        public async Task<IActionResult> CreateProduct([FromBody]CatalogItem product)
        {
             var item = new CatalogItem()
             {
                 CatalogBrandId = product.CatalogBrandId,
                 CatalogTypeId = product.CatalogTypeId,
                 Description = product.Description,
                 Name = product.Name,
                 PictureFileName = product.PictureFileName,
                 PictureUrl = product.PictureUrl
             };
             _catalogContext.CatalogItems.Add(item);

             await _catalogContext.SaveChangesAsync();

             return CreatedAtAction(nameof(GetItemById), new {id = item.Id});
        }

        [HttpPut]
        [Route("items")]
        public async Task<IActionResult> UpdateProduct([FromBody] CatalogItem product)
        {
            var catalogItem = await ((IQueryable<CatalogItem>) _catalogContext.CatalogItems)
                .SingleOrDefaultAsync(i => i.Id == product.Id);
            if (catalogItem == null)
            {
                return NotFound($"The product with id {product.Id} doesn't exist.");
            }

            catalogItem = product;
            
            _catalogContext.CatalogItems.Update(catalogItem);
            await _catalogContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemById), new {id = catalogItem.Id});
        }

        [HttpDelete]
        [Route("items/{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _catalogContext.CatalogItems
                .SingleOrDefaultAsync(c => c.Id == id);
            if (product == null)
            {
                return NotFound($"Product with id {id} doesn't exist.");
            }

            _catalogContext.CatalogItems.Remove(product);
            await _catalogContext.SaveChangesAsync();

            return NoContent();
        }






        private List<CatalogItem> ChangeUrlPlaceHolder(List<CatalogItem> items)
        {
            items.ForEach(c =>
            {
                c.PictureUrl = c.PictureUrl.Replace("externalcatalogbaseurltobereplaced", _settings.Value.ExternalCatalogBaseUrl);
            });
            return items;
        }
        
        

    }
}