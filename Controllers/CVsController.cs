using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Helpers;
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
            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .FirstOrDefaultAsync(p => p.Id == positionId);

            if (position == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.CandidateProfiles
                .Include(cp => cp.ProfileAttributes)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null)
            {
                return RedirectToAction("Index", "Profiles");
            }

            var model = new CV
            {
                PositionId = positionId,
                Position = position,
                UserId = userId,
                CandidateProfileId = profile.Id,
                CandidateProfile = profile
            };

            ViewBag.AttributeValues = new Dictionary<int, string>();
            return View(model);
        }

        // POST: Save CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Create(CV cv, Dictionary<int, string> attributeValues)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            cv.UserId = userId;
            cv.CreatedAt = DateTime.UtcNow;
            cv.AttributeValues = JsonSerializer.Serialize(attributeValues);

            if (ModelState.IsValid)
            {
                _context.CVs.Add(cv);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Application submitted successfully!";
                return RedirectToAction("MyApplications");
            }

            return View(cv);
        }

        // GET: My Applications
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> MyApplications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var applications = await _context.CVs
                .Include(c => c.Position)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(applications);
        }

        // GET: All CVs (Recruiter)
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Index(string searchString)
        {
            var cvs = _context.CVs
                .Include(c => c.User)
                .Include(c => c.Position)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                cvs = cvs.Where(c => c.User.FullName.Contains(searchString) ||
                                     c.Position.Title.Contains(searchString));
            }

            return View(await cvs.ToListAsync());
        }

        // GET: CV Details
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var cv = await _context.CVs
                .Include(c => c.User)
                .Include(c => c.Position)
                .ThenInclude(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .Include(c => c.CandidateProfile)
                .ThenInclude(cp => cp.Projects)
                .Include(c => c.Comments)
                .ThenInclude(com => com.User)
                .Include(c => c.Likes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cv == null) return NotFound();

            var attributeValues = string.IsNullOrEmpty(cv.AttributeValues)
                ? new Dictionary<int, string>()
                : JsonSerializer.Deserialize<Dictionary<int, string>>(cv.AttributeValues)
                  ?? new Dictionary<int, string>();

            ViewBag.AttributeValues = attributeValues;

            return View(cv);
        }

        // GET: Edit CV
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var cv = await _context.CVs
                .Include(c => c.Position)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cv == null) return NotFound();

            // Authorization check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cv.UserId != userId && !User.IsInRole("Administrator"))
                return Forbid();

            return View(cv);
        }

        // POST: Edit CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Edit(int id, CV cv, Dictionary<int, string> attributeValues)
        {
            var existing = await _context.CVs.FindAsync(id);
            if (existing == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existing.UserId != userId && !User.IsInRole("Administrator"))
                return Forbid();

            if (ModelState.IsValid)
            {
                existing.AttributeValues = JsonSerializer.Serialize(attributeValues);
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                TempData["Success"] = "CV updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(cv);
        }

        // GET: Download PDF
        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            var cv = await _context.CVs
                .Include(c => c.User)
                .Include(c => c.Position)
                .Include(c => c.CandidateProfile)
                .ThenInclude(cp => cp.Projects)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cv == null) return NotFound();

            var pdfBytes = _cvGenerator.GenerateCV(cv);
            return File(pdfBytes, "application/pdf", $"CV_{cv.User.FullName}.pdf");
        }

        // POST: Add Comment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int cvId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return BadRequest();

            var comment = new Comment
            {
                CVId = cvId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                Message = message.Trim()
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = cvId });
        }

        // POST: Publish CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Publish(int id)
        {
            var cv = await _context.CVs.FindAsync(id);
            if (cv == null) return NotFound();

            cv.IsPublished = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "CV published successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Unpublish CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Unpublish(int id)
        {
            var cv = await _context.CVs.FindAsync(id);
            if (cv == null) return NotFound();

            cv.IsPublished = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "CV unpublished.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Toggle Like
        [HttpPost]
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> ToggleLike(int cvId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

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
                    UserId = userId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = cvId });
        }

        private bool IsCVComplete(CV cv)
        {
            return !string.IsNullOrWhiteSpace(cv.AttributeValues) && cv.Position != null;
        }

        private async Task SyncCVAttributesToProfile(CandidateProfile profile, Dictionary<int, string> attributeValues)
        {
            // Implementation for syncing attributes (can be expanded later)
            await Task.CompletedTask;
        }
    }
}