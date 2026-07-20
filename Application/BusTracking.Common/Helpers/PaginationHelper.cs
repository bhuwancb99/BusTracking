namespace BusTracking.Common.Helpers
{
    public static class PaginationHelper
    {
        public static int Clamp(int page, int minPage = 1) => Math.Max(page, minPage);

        public static int ClampPageSize(int size,
            int min = 1,
            int max = AppConstants.MaxPageSize)
            => Math.Clamp(size, min, max);

        /// <summary>
        /// Global common method to fetch active AppConfig PageSize from AppDbContext.
        /// Used across all domain services to eliminate duplicate DB query blocks.
        /// </summary>
        public static async Task<int> GetListPageSizeAsync(AppDbContext db)
        {
            var raw = await db.AppConfigurations
                .Where(c => c.ConfigKey == AppConstants.AppConfigPageSizeKey && c.IsActive)
                .Select(c => c.ConfigValue)
                .FirstOrDefaultAsync();

            return int.TryParse(raw, out var size) && size > 0
                ? ClampPageSize(size)
                : AppConstants.DefaultPageSize;
        }

        /// <summary>
        /// Returns page numbers to display in pagination UI.
        /// E.g. for page 5 of 20 with window=2: [1, …, 3, 4, 5, 6, 7, …, 20]
        /// </summary>
        public static List<int?> GetPageNumbers(int currentPage, int totalPages, int window = 2)
        {
            var result = new List<int?>();
            if (totalPages <= 1) return result;

            result.Add(1);
            int start = Math.Max(2, currentPage - window);
            int end = Math.Min(totalPages - 1, currentPage + window);

            if (start > 2) result.Add(null);           // ellipsis
            for (int i = start; i <= end; i++) result.Add(i);
            if (end < totalPages - 1) result.Add(null);      // ellipsis
            if (totalPages > 1) result.Add(totalPages);

            return result;
        }
    }
}
