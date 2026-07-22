using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Latest Positions
            var latestPositions = await _context.Positions
                .Include(p => p.CVs)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            // Most Popular Positions (Top 5 by CV count)
            var popularPositions = await _context.Positions
                .Include(p => p.CVs)
                .OrderByDescending(p => p.CVs.Count)
                .Take(5)
                .ToListAsync();

            // Tag Cloud - aggregate from Positions and Projects
            var positionTags = await _context.Positions
                .Where(p => !string.IsNullOrEmpty(p.ProjectTags))
                .Select(p => p.ProjectTags)
                .ToListAsync();

            var projectTags = await _context.Projects
                .Where(p => !string.IsNullOrEmpty(p.TechnologyTags))
                .Select(p => p.TechnologyTags)
                .ToListAsync();

            var allTags = positionTags.Concat(projectTags).ToList();

            var tagCloud = allTags
                .SelectMany(tags => tags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { Tag = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(15)
                .ToList();

            ViewBag.LatestPositions = latestPositions;
            ViewBag.PopularPositions = popularPositions;
            ViewBag.TagCloud = tagCloud;

            // Quick Stats
            ViewBag.TotalCVs = await _context.CVs.CountAsync();
            ViewBag.TotalPositions = await _context.Positions.CountAsync();
            ViewBag.TotalCandidates = await _context.Users.CountAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Stats()
        {
            var totalCVs = await _context.CVs.CountAsync();
            var publishedCVs = await _context.CVs.CountAsync(c => c.IsPublished);
            var totalPositions = await _context.Positions.CountAsync();
            var totalCandidates = await _context.Users.CountAsync();

            var recruiterRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Recruiter");
            var totalRecruiters = recruiterRole != null
                ? await _context.UserRoles.CountAsync(ur => ur.RoleId == recruiterRole.Id)
                : 0;

            var recentCVs = await _context.CVs
                .Include(c => c.User)
                .Include(c => c.Position)
                .OrderByDescending(c => c.CreatedAt)
                .Take(8)
                .ToListAsync();

            ViewBag.TotalCVs = totalCVs;
            ViewBag.PublishedCVs = publishedCVs;
            ViewBag.TotalPositions = totalPositions;
            ViewBag.TotalCandidates = totalCandidates;
            ViewBag.TotalRecruiters = totalRecruiters;

            return View(recentCVs);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}