using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Client.Models;
using Client.Services;
using Client.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Client.Controllers
{
    public class CatalogController : Controller
    {

        private ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public async Task<IActionResult> Index(int? BrandFilterApplied, int? TypesFilterApplied, int? page)
        {
            var itemPage = 10;
            var catalog =
                await _catalogService.GetCatalogItems(page ?? 0, itemPage, BrandFilterApplied, TypesFilterApplied);
            var vm = new CatalogIndexViewModel()
            {
                CatalogItems =  catalog.Data,
                Brands = await _catalogService.GetBrands(),
                Types = await _catalogService.GetTypes(),
                BrandFilterApplied =  BrandFilterApplied ?? 0,
                TypesFilterApplied =  TypesFilterApplied ?? 0,
                PaginationInfo = new PaginationInfo()
                {
                    ActualPage = page ?? 0,
                    ItemsPerPage = itemPage,
                    TotalItems = catalog.Count,
                    TotalPages =  (int)Math.Ceiling((decimal)catalog.Count/itemPage)
                }
            };
            vm.PaginationInfo.Next = (vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1)? "is-disabled" : "is-enabled";
            vm.PaginationInfo.Previous = (vm.PaginationInfo.ActualPage == 0) ? "is-disabled" : "is-enabled";
            return View(vm);
        }

        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
