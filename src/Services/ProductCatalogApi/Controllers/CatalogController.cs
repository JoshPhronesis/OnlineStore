using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductCatalogApi.Data;
using ProductCatalogApi.Domain;
using ProductCatalogApi.Helpers;
using ProductCatalogApi.ViewModels;

namespace ProductCatalogApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Catalog")]
    public class CatalogController : Controller
    {
        private readonly CatalogContext _context;
        private readonly IOptions<Settings> _settings;

        public CatalogController(CatalogContext catalogContext, IOptions<Settings> settings)
        {
            this._context = catalogContext;
            this._settings = settings;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [HttpGet()]
        [Route("[action]")]
        public async Task<IActionResult> CatalogTypes()
        {
            var items = await _context.CatalogTypes.ToListAsync();
            return Ok(items);
        }

        [HttpGet()]
        [Route("[action]")]
        public async Task<IActionResult> CatalogBrands()
        {
            var items = await _context.CatalogBrands.ToListAsync();
            return Ok(items);
        }

        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetItembyId(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var item = await _context.CatalogItems.SingleOrDefaultAsync(c => c.Id == id);
            if (item != null)
            {
                item.PictureUrl = item.PictureUrl.Replace("http://externalcatalogbaseurltobereplaced", _settings.Value.ExternalBaseUrl);
                return Ok(item);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Items([FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {
            var totalItemsCount = await _context.CatalogItems.LongCountAsync();
            var itemsOnPage = await _context.CatalogItems
                                        .OrderBy(c => c.Name)
                                        .Skip(pageSize * pageIndex)
                                        .Take(pageSize)
                                        .ToListAsync();
            itemsOnPage = ChangeUrl(itemsOnPage);

            return Ok(new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, (int)totalItemsCount, itemsOnPage));
        }

        [HttpGet]
        [Route("[action]/withname/{name:minlength(1)}")]
        public async Task<IActionResult> Items(string name, [FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {
            var totalItemsCount = await _context.CatalogItems
                                                .Where(c => c.Name.StartsWith(name))
                                                .LongCountAsync();
            var itemsOnPage = await _context.CatalogItems
                                        .Where(c => c.Name.StartsWith(name))
                                        .OrderBy(c => c.Name)
                                        .Skip(pageSize * pageIndex)
                                        .Take(pageSize)
                                        .ToListAsync();
            itemsOnPage = ChangeUrl(itemsOnPage);

            return Ok(new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, (int)totalItemsCount, itemsOnPage));
        }


        [HttpGet]
        [Route("[action]/type/{catalogTypeId}/brand/{catalogBrandId}")]
        public async Task<IActionResult> Items(int? catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {
            var root = _context.CatalogItems.AsQueryable();

            if (catalogTypeId.HasValue)
            {
                root = root.Where(c => c.CatalogTypeId == catalogTypeId);
            }

            if (catalogBrandId.HasValue)
            {
                root = root.Where(c => c.CatalogBrandId == catalogTypeId);
            }
            var totalItemsCount = await root.LongCountAsync();
            var itemsOnPage = await root.OrderBy(c => c.Name)
                                        .Skip(pageSize * pageIndex)
                                        .Take(pageSize)
                                        .ToListAsync();
            itemsOnPage = ChangeUrl(itemsOnPage);

            return Ok(new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, (int)totalItemsCount, itemsOnPage));
        }

        [HttpPost]
        [Route("items")]
        public async Task<IActionResult> CreateProduct([FromBody] CatalogItem product)
        {
            var item = new CatalogItem
            {
                CatalogBrandId = product.CatalogBrandId,
                CatalogTypeId = product.CatalogTypeId,
                Description = product.Description,
                Name = product.Name,
                PictureFileName = product.PictureFileName,
                Price = product.Price
            };

            _context.CatalogItems.Add(item);

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetItembyId), new { id = item.Id });
        }

        [HttpPut]
        [Route("items")]
        public async Task<IActionResult> UpdateProduct([FromBody] CatalogItem product)
        {
            var catalogItem = await _context.CatalogItems
                                            .SingleOrDefaultAsync(i => i.Id == product.Id);

            if (catalogItem == null)
            {
                return NotFound(new { Message = $"product {product.Id} not found" });
            }

            catalogItem = product;

            _context.CatalogItems.Update(catalogItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItembyId), new { id = product.Id });
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = _context.CatalogItems.SingleOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            _context.CatalogItems.Remove(product.Result);

            await _context.SaveChangesAsync();
            return NoContent();

        }

        private List<CatalogItem> ChangeUrl(List<CatalogItem> items)
        {
            items.ForEach(i => i.PictureUrl.Replace("http://externalcatalogbaseurltobereplaced", _settings.Value.ExternalBaseUrl));
            return items;
        }
    }
}