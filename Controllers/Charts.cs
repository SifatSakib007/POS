using Microsoft.AspNetCore.Mvc;
using POS.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

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
        /*foreach (var entry in sellData)
        {
            // Temporary manual calculation for testing purposes
            entry.Profit = entry.TotalTotalPrice * 0.1m; // Example: 10% profit
        }*/

        // Log the result to the console
        foreach (var entry in result)
        {

            Console.WriteLine($"Date: {entry.Date}, Profit: {entry.Profit}");

        }
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result.OrderBy(d => d.Date).ToList()));
        // Log the result to the console
        var resultList = result.OrderBy(d => d.Date).ToList();
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(resultList));
        return Json(resultList);

        //return Json(result.OrderBy(d => d.Date).ToList());
    }



}
