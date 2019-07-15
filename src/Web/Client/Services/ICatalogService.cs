using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Client.Services
{
    public interface ICatalogService
    {
        Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type);

        Task<IEnumerable<SelectListItem>> GetBrands();

        Task<IEnumerable<SelectListItem>> GetTypes();

    }
}