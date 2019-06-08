using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogApi.ViewModels
{
    public class PaginatedItemsViewModel<TEntity> where TEntity : class
    {
        public PaginatedItemsViewModel(int pageIndex, int pageSize, int count, IEnumerable<TEntity> data)
        {
            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.Count = count;
            this.Data = data;
        }

        public int PageSize { get; private set; }
        public int PageIndex { get; private set; }
        public int Count { get; private set; }
        public IEnumerable<TEntity> Data { get; set; }
    }
}
