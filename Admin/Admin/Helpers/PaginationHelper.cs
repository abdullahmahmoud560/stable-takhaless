using Admin.DTO;
using Microsoft.EntityFrameworkCore;

namespace Admin.Helpers
{
    public static class PaginationHelper
    {
        public static async Task<PaginatedResponse<T>> CreatePaginatedResponseAsync<T>(
            IQueryable<T> query, 
            int page, 
            int pageSize = 10) where T : class
        {
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResponse<T>.Create(page, pageSize, totalCount, data);
        }

        public static bool IsValidPagination(int page, int pageSize = 10)
        {
            return page > 0 && pageSize > 0 && pageSize <= DTO.Helpers.Constants.MAX_PAGE_SIZE;
        }
    }
} 