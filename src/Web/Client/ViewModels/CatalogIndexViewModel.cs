using System.Collections.Generic;
using Client.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Client.ViewModels
{
    public class CatalogIndexViewModel
    {
        public IEnumerable<CatalogItem> CatalogItems { get; set; }

        public IEnumerable<SelectListItem> Brands { get; set; }

        public IEnumerable<SelectListItem> Types { get; set; }

        public int? BrandFilterApplied { get; set; }

        public int? TypesFilterApplied { get; set; }

        public PaginationInfo PaginationInfo { get; set; }
    }
}