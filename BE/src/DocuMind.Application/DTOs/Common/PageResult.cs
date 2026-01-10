using System.Collections.Generic;

namespace DocuMind.Application.DTOs.Common
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public PagedResult(List<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPage = page;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
    }
}
