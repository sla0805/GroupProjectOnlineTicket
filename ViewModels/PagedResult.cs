using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineTicket.ViewModels
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        public static PagedResult<T> CreateFromList(IEnumerable<T> source, int page, int pageSize)
        {
            var list = source.ToList();
            var totalItems = list.Count;

            return new PagedResult<T>
            {
                Items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
    }
}
