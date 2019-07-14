using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogApi.Controllers
{
    [Route("api/Pic")]
    [ApiController]
    public class PickController : ControllerBase
    {
        private readonly IHostingEnvironment _env;

        public PickController(IHostingEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetImage(int id)
        {
            var webRoot = _env.WebRootPath;
            var picPath = Path.Combine(webRoot, "Pics", $"shoes-{id}.png");
            if (System.IO.File.Exists(picPath))
            {
                var buffer = System.IO.File.ReadAllBytes(picPath);
                return File(buffer, "image/png");
            }

            return NotFound($"The image with {id} doesn't exist.");
        }
    }
}