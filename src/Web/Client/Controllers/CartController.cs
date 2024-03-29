﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Infrastructure;
using Client.Models;
using Client.Models.CartModels;
using Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace WebMvc.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ICatalogService _catalogService;
        private readonly IIdentityService<ApplicationUser> _identityService;

        public CartController(IIdentityService<ApplicationUser> identityService, ICartService cartService,
            ICatalogService catalogService)
        {
            _identityService = identityService;
            _cartService = cartService;
            _catalogService = catalogService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Dictionary<string, int> quantities, string action)
        {
            if (action.IndexOf("Checkout") > -1) return RedirectToAction("Create", "Order");

            try
            {
                var user = _identityService.Get(HttpContext.User);
                var basket = await _cartService.SetQuantities(user, quantities);
                var vm = await _cartService.UpdateCart(basket);

                //if (action == "[ Checkout ]")
                //{
                //    var order = _cartService.MapBasketToOrder(basket);
                //    return RedirectToAction("Create", "Order");
                //}
            }
            catch (BrokenCircuitException)
            {
                // Catch error when CartApi is in open circuit  mode                 
                HandleBrokenCircuitException();
            }

            return View();
        }

        public async Task<IActionResult> AddToCart(CatalogItem productDetails)
        {
            try
            {
                if (productDetails.Id != null)
                {
                    var user = _identityService.Get(HttpContext.User);
                    var product = new CartItem()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Quantity = 1,
                        ProductName = productDetails.Name,
                        PictureUrl = productDetails.PictureUrl,
                        UnitPrice = productDetails.Price,
                        ProductId = productDetails.Id.ToString()
                    };
                    await _cartService.AddItemToCart(user, product);
                }

                return RedirectToAction("Index", "Catalog");
            }
            catch (BrokenCircuitException)
            {
                // Catch error when CartApi is in circuit-opened mode                 
                HandleBrokenCircuitException();
            }

            return RedirectToAction("Index", "Catalog");
        }
        //public async Task WriteOutIdentityInfo()
        //{
        //    var identityToken =
        //        await HttpContext.Authentication.
        //         GetAuthenticateInfoAsync(OpenIdConnectParameterNames.IdToken);
        //    Debug.WriteLine($"Identity Token: {identityToken}");
        //    foreach (var claim in User.Claims)
        //    {
        //        Debug.WriteLine($"Claim Type: {claim.Type} - Claim Value : {claim.Value}");
        //    }

        //}

        private void HandleBrokenCircuitException()
        {
            TempData["BasketInoperativeMsg"] =
                "cart Service is inoperative, please try later on. (Business Msg Due to Circuit-Breaker)";
        }
    }
}