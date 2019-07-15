using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Infrastructure;
using Client.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IOptionsSnapshot<AppSettings> _settings;
        private readonly IHttpClient _apiClient;
        private readonly ILogger<CatalogItem> _logger;

        private readonly string _remoteServiceBaseUrl;
        
        public CatalogService(IOptionsSnapshot<AppSettings> settings, IHttpClient apiClient, ILogger<CatalogItem> logger)
        {
            _settings = settings;
            _apiClient = apiClient;
            _logger = logger;
            _remoteServiceBaseUrl = $"{_settings.Value.CatalogUrl}/api/catalog/";
        }

        public async Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type)
        {
            var allCatalogItemsUri =
                ApiPaths.Catalog.GetAllCatalogItems(_remoteServiceBaseUrl, page, take, brand, type);
            var dataString = await _apiClient.GetStringAsync(allCatalogItemsUri);
            var response = JsonConvert.DeserializeObject<Catalog>(dataString);
            return response;
        }

        public async Task<IEnumerable<SelectListItem>> GetBrands()
        {
            var getBrandsUri = ApiPaths.Catalog.GetAllBrands( _remoteServiceBaseUrl);
            var dataString = await _apiClient.GetStringAsync(getBrandsUri);
            var items = new List<SelectListItem>()
            {
                new SelectListItem(){Value =null,Text = "All",Selected = true}
            };

            var brands = JArray.Parse(dataString);
            foreach (var brand in brands.Children<JObject>())
            {
                items.Add(new SelectListItem()
                {
                    Value = brand.Value<string>("id"),
                    Text = brand.Value<string>("brand"),
                });
            }

            return items;

        }

        public async Task<IEnumerable<SelectListItem>> GetTypes()
        {
            var getTypessUri = ApiPaths.Catalog.GetAllTypes( _remoteServiceBaseUrl);
            var dataString = await _apiClient.GetStringAsync(getTypessUri);
            var items = new List<SelectListItem>()
            {
                new SelectListItem(){Value =null,Text = "All",Selected = true}
            };

            var types = JArray.Parse(dataString);
            foreach (var type in types.Children<JObject>())
            {
                items.Add(new SelectListItem()
                {
                    Value = type.Value<string>("id"),
                    Text = type.Value<string>("brand"),
                });
            }

            return items;
        }
    }
}