using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineStore.Web.WebMvc.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineStore.Web.WebMvc.Services
{
    public interface ICatalogService
    {
        Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type);
        Task<IEnumerable<SelectListItem>> GetBrands();
        Task<IEnumerable<SelectListItem>> GetTypes();
    }
}
