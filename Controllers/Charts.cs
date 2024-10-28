using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Models;
using System.Globalization;

namespace POS.Controllers
{
    public class Charts : Controller
    {
        private readonly AppDbContext _context;

        public Charts(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalSales(string format)
        {
            // Fetch sales data
            var salesData = _context.Sell.AsEnumerable();

            // Group by the selected format
            var result = format switch
            {
                "week" => salesData
                    .GroupBy(s => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(s.CreatedAt.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                    .Select(g => new
                    {
                        Date = $"Week {g.Key}",
                        TotalSell = g.Sum(s => s.TotalTotalPrice),
                        TotalDeposit = g.Sum(s => s.Deposit)
                    }),

                "month" => salesData
                    .GroupBy(s => new { s.CreatedAt.Value.Year, s.CreatedAt.Value.Month })
                    .Select(g => new
                    {
                        Date = $"{g.Key.Year}-{g.Key.Month:D2}", // Ensures month is two digits
                        TotalSell = g.Sum(s => s.TotalTotalPrice),
                        TotalDeposit = g.Sum(s => s.Deposit)
                    }),

                "year" => salesData
                    .GroupBy(s => s.CreatedAt.Value.Year)
                    .Select(g => new
                    {
                        Date = $"{g.Key}",
                        TotalSell = g.Sum(s => s.TotalTotalPrice),
                        TotalDeposit = g.Sum(s => s.Deposit)
                    }),

                _ => salesData
                    .GroupBy(s => s.CreatedAt?.Date)
                    .Select(g => new
                    {
                        Date = g.Key?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        TotalSell = g.Sum(s => s.TotalTotalPrice),
                        TotalDeposit = g.Sum(s => s.Deposit)
                    }),
            };

            return Json(result.OrderBy(d => d.Date).ToList());
        }

        // This action renders the HTML view with the chart
        public IActionResult SalesReport()
        {
            return View(); // Ensure this renders the view with the chart code
        }
    }
}
