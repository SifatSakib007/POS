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
        public async Task<IActionResult> GetTotalSalesPerDay()
        {
            var salesData = _context.Sell
                .AsEnumerable()
                .GroupBy(s => s.CreatedAt?.Date)
                .Select(g => new
                {
                    Date = g.Key?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    TotalSell = g.Sum(s => s.TotalTotalPrice),
                    TotalDeposit = g.Sum(s => s.Deposit)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return Json(salesData);
        }

        // This action renders the HTML view with the chart
        public IActionResult SalesReport()
        {
            return View(); // Ensure this renders the view with the chart code
        }

    }
}
