using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;
using talentacquisition_jobplacement_mvc.Services;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    public class CVsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CVGeneratorService _cvGenerator;

        public CVsController(ApplicationDbContext context, CVGeneratorService cvGenerator)
        {
            _context = context;
            _cvGenerator = cvGenerator;
        }

        // GET: Apply to Position
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Create(int positionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.CandidateProfiles
                .Include(cp => cp.User)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null ||
                string.IsNullOrWhiteSpace(profile.User?.FullName) ||
                string.IsNullOrWhiteSpace(profile.Summary))
            {
                TempData["Message"] = "Please complete your Full Name and Professional Summary before applying.";
                return RedirectToAction("Index", "Profiles");
            }

            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .FirstOrDefaultAsync(p => p.Id == positionId);

            if (position == null) return NotFound();

            // === Access Rules Check ===
            var allowedRoles = position.AllowedRoles?.Split(',').Select(r => r.Trim()).ToList()
                              ?? new List<string> { "Candidate" };

            if (!allowedRoles.Any(r => User.IsInRole(r)))
            {
                TempData["Message"] = "You are not authorized to apply for this position.";
                return RedirectToAction("Index", "Positions");
            }

            var cv = new CV { PositionId = positionId, Position = position };
            return View(cv);
        }

        // POST: Save CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Create(CV cv, Dictionary<int, string> attributeValues)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(cp => cp.UserId == userId);
            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId };
                _context.CandidateProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            cv.UserId = userId;
            cv.CandidateProfileId = profile.Id;
            cv.CreatedAt = DateTime.UtcNow;
            cv.AttributeValues = JsonSerializer.Serialize(attributeValues);

            _context.CVs.Add(cv);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"CV for <b>{cv.Position?.Title}</b> submitted successfully!";

            return RedirectToAction("MyApplications");
        }

        // GET: My Applications (Candidate)
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> MyApplications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myCVs = await _context.CVs
                .Include(cv => cv.Position)
                .Where(cv => cv.UserId == userId)
                .OrderByDescending(cv => cv.CreatedAt)
                .ToListAsync();

            return View(myCVs);
        }

        // GET: All CVs with Search (Recruiter/Admin)
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Index(string searchString)
        {
            var cvs = _context.CVs
                .Include(cv => cv.Position)
                .Include(cv => cv.User)
                .OrderByDescending(cv => cv.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                cvs = cvs.Where(cv =>
                    cv.User.FullName.ToLower().Contains(searchString) ||
                    cv.Position.Title.ToLower().Contains(searchString) ||
                    (cv.User.Email != null && cv.User.Email.ToLower().Contains(searchString))
                );
            }

            ViewBag.SearchString = searchString;
            return View(await cvs.ToListAsync());
        }

        // GET: CV Details
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var cv = await _context.CVs
                .Include(cv => cv.Position)
                .ThenInclude(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .Include(cv => cv.User)
                .Include(cv => cv.Comments)
                .ThenInclude(c => c.User)
                .Include(cv => cv.Likes)
                .FirstOrDefaultAsync(cv => cv.Id == id);

            if (cv == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cv.UserId != userId && !User.IsInRole("Recruiter") && !User.IsInRole("Administrator"))
                return Forbid();

            ViewBag.AttributeValues = string.IsNullOrEmpty(cv.AttributeValues)
                ? new Dictionary<int, string>()
                : JsonSerializer.Deserialize<Dictionary<int, string>>(cv.AttributeValues);

            return View(cv);
        }

        // GET: Download PDF
        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            var cv = await _context.CVs
                .Include(cv => cv.Position)
                .ThenInclude(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .Include(cv => cv.User)
                .FirstOrDefaultAsync(cv => cv.Id == id);

            if (cv == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cv.UserId != userId && !User.IsInRole("Recruiter") && !User.IsInRole("Administrator"))
                return Forbid();

            var pdfBytes = _cvGenerator.GenerateCV(cv);

            return File(pdfBytes, "application/pdf",
                $"CV_{cv.User?.FullName ?? "Candidate"}_{cv.Position?.Title ?? "Position"}.pdf");
        }

        // POST: Add Comment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int cvId, string message)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new Comment
            {
                CVId = cvId,
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = cvId });
        }

        // POST: Toggle Like on CV
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int cvId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingLike = await _context.CVLikes
                .FirstOrDefaultAsync(l => l.CVId == cvId && l.UserId == userId);

            if (existingLike != null)
            {
                _context.CVLikes.Remove(existingLike);
            }
            else
            {
                _context.CVLikes.Add(new CVLike
                {
                    CVId = cvId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = cvId });
        }
    }
}