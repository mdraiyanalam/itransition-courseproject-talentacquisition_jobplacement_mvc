using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Stats()
        {
            var totalCVs = await _context.CVs.CountAsync();
            var totalPositions = await _context.Positions.CountAsync();
            var totalCandidates = await _context.Users.CountAsync(); // Simplified

            var recentCVs = await _context.CVs
                .Include(c => c.User)
                .Include(c => c.Position)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalCVs = totalCVs;
            ViewBag.TotalPositions = totalPositions;
            ViewBag.TotalCandidates = totalCandidates;

            return View(recentCVs);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}