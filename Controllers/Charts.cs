using Microsoft.AspNetCore.Mvc;
using POS.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

public class Charts : Controller
{
    private readonly AppDbContext _context;

    public Charts(AppDbContext context)
    {
        _context = context;
    }
    private int? GetLoggedInUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out int userId))
        {
            return userId;
        }
        return null;
    }
    [HttpGet]
    public async Task<IActionResult> GetTotalSales(string format)
    {
        // Fetch sales data
        var salesData = await _context.Sell.ToListAsync();

        // Group by the selected format
        var result = format switch
        {
            "week" => salesData
                .GroupBy(s => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    s.CreatedAt ?? DateTime.MinValue, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .Select(g => new
                {
                    Date = $"Week {g.Key}",
                    TotalSell = g.Sum(s => s.TotalTotalPrice),
                    TotalDeposit = g.Sum(s => s.Deposit)
                }),

            "month" => salesData
                .GroupBy(s => new { Year = s.CreatedAt.Value.Year, Month = s.CreatedAt.Value.Month })
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
                    Date = g.Key?.ToString("yyyy-MM-dd"), // Safe access with null conditional
                    TotalSell = g.Sum(s => s.TotalTotalPrice),
                    TotalDeposit = g.Sum(s => s.Deposit)
                }),
        };

        return Json(result.OrderBy(d => d.Date).ToList());
    }

    public IActionResult SalesReport()
    {
        return View(); // Ensure this renders the view with the chart code
    }
    [HttpGet]
    public async Task<IActionResult> GetTotalProfit(string format)
    {
        // Retrieve the logged-in user ID
        int? userId = GetLoggedInUserId();

        // Fetch sell data filtered by user ID
        var sellData = await _context.Sell
            .Where(s => s.UserId == userId && s.CreatedAt.HasValue)
            .ToListAsync();

        // Group by the selected format and calculate profit
        var result = format switch
        {
            "week" => sellData
                .GroupBy(s => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    s.CreatedAt.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .Select(g => new
                {
                    Date = $"Week {g.Key}",
                    //TotalSell = g.Sum(s => s.TotalTotalPrice),
                    Profit = g.Sum(s => s.Profit)
                }),

            "month" => sellData
                .GroupBy(s => new { Year = s.CreatedAt.Value.Year, Month = s.CreatedAt.Value.Month })
                .Select(g => new
                {
                    Date = $"{g.Key.Year}-{g.Key.Month:D2}",
                   // TotalSell = g.Sum(s => s.TotalTotalPrice),
                    Profit = g.Sum(s => s.Profit)
                }),

            "year" => sellData
                .GroupBy(s => s.CreatedAt.Value.Year)
                .Select(g => new
                {
                    Date = $"{g.Key}",
                  //  TotalSell = g.Sum(s => s.TotalTotalPrice),
                    Profit = g.Sum(s => s.Profit)
                }),

            // Default case for daily grouping
            _ => sellData
                .GroupBy(s => s.CreatedAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                  //  TotalSell = g.Sum(s => s.TotalTotalPrice),
                    Profit = g.Sum(s => s.Profit)
                }),
        };       
        var resultList = result.OrderBy(d => d.Date).ToList();
        return Json(resultList);
    }

    [HttpGet]
    public async Task<IActionResult> GetProductSalesData(string format)
    {
        // Retrieve the logged-in user ID
        int? userId = GetLoggedInUserId();

        // Fetch sell data filtered by user ID
        var sellData = await _context.Sell
            .Where(s => s.UserId == userId && s.CreatedAt.HasValue)
            .ToListAsync();
        Console.WriteLine("Raw Sell Data Count: " + sellData.Count);

        // Helper function to calculate product sales for a given set of sales
        Dictionary<string, int> CalculateProductSales(IEnumerable<Sell> sales)
        {
            var productSales = new Dictionary<string, int>();

            foreach (var sale in sales)
            {
                if (string.IsNullOrEmpty(sale.ProductNames) || string.IsNullOrEmpty(sale.Quantities))
                    continue;

                var productNames = sale.ProductNames.Split(',').Select(p => p.Trim()).ToArray();
                var quantities = sale.Quantities.Split(',').Select(q => int.TryParse(q.Trim(), out var qty) ? qty : 0).ToArray();

                if (productNames.Length == quantities.Length)
                {
                    for (int i = 0; i < productNames.Length; i++)
                    {
                        string productName = productNames[i];
                        int quantity = quantities[i];

                        if (productSales.ContainsKey(productName))
                        {
                            productSales[productName] += quantity;
                        }
                        else
                        {
                            productSales[productName] = quantity;
                        }
                    }
                }
            }

            return productSales;
        }

        // Create a result based on the selected format (day, week, month, or year)
        var result = format switch
        {
            "week" => sellData
                .Where(s => s.CreatedAt.HasValue)
                .GroupBy(s => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    s.CreatedAt.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .Select(g => new
                {
                    Date = $"Week {g.Key}",
                    Products = CalculateProductSales(g).Select(ps => new { ProductName = ps.Key, SalesAmount = ps.Value })
                }),

            "month" => sellData
                .Where(s => s.CreatedAt.HasValue)
                .GroupBy(s => new { Year = s.CreatedAt.Value.Year, Month = s.CreatedAt.Value.Month })
                .Select(g => new
                {
                    Date = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Products = CalculateProductSales(g).Select(ps => new { ProductName = ps.Key, SalesAmount = ps.Value })
                }),

            "year" => sellData
                .Where(s => s.CreatedAt.HasValue)
                .GroupBy(s => s.CreatedAt.Value.Year)
                .Select(g => new
                {
                    Date = $"{g.Key}",
                    Products = CalculateProductSales(g).Select(ps => new { ProductName = ps.Key, SalesAmount = ps.Value })
                }),

            // Default case for daily grouping
            _ => sellData
                .Where(s => s.CreatedAt.HasValue)
                .GroupBy(s => s.CreatedAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Products = CalculateProductSales(g).Select(ps => new { ProductName = ps.Key, SalesAmount = ps.Value })
                }),
        };

        var resultList = result.OrderBy(d => d.Date).ToList();
        // Serialize resultList to JSON to view the exact structure
        string jsonResult = JsonSerializer.Serialize(resultList);
        Console.WriteLine("Data sent to frontend: " + jsonResult);
        return Json(resultList);
    }





}
