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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.CandidateProfiles
                .Include(cp => cp.User)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null || string.IsNullOrWhiteSpace(profile.User?.FullName) ||
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

            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                .FirstOrDefaultAsync(p => p.Id == cv.PositionId);

            // Access Rules Check
            if (position != null && !AccessRuleEvaluator.CanApply(position, profile, attributeValues))
            {
                TempData["Message"] = "You do not meet the requirements for this position.";
                return RedirectToAction("Index", "Positions");
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

        // GET: My Applications
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

        // GET: All CVs (Recruiter)
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
                    (cv.User.FullName != null && cv.User.FullName.ToLower().Contains(searchString)) ||
                    (cv.Position.Title != null && cv.Position.Title.ToLower().Contains(searchString))
                );
            }
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

        // GET: Edit CV
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var cv = await _context.CVs
                .Include(cv => cv.Position)
                .ThenInclude(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .Include(cv => cv.User)
                .FirstOrDefaultAsync(cv => cv.Id == id);

            if (cv == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cv.UserId != userId && !User.IsInRole("Administrator"))
                return Forbid();

            ViewBag.AttributeValues = string.IsNullOrEmpty(cv.AttributeValues)
                ? new Dictionary<int, string>()
                : JsonSerializer.Deserialize<Dictionary<int, string>>(cv.AttributeValues);

            return View(cv);
        }

        // POST: Edit CV + Enhanced Profile Sync
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Edit(int id, CV cv, Dictionary<int, string> attributeValues)
        {
            var existing = await _context.CVs
                .Include(c => c.CandidateProfile)
                    .ThenInclude(cp => cp.ProfileAttributes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existing.UserId != userId && !User.IsInRole("Administrator"))
                return Forbid();

            // Update CV values
            existing.AttributeValues = JsonSerializer.Serialize(attributeValues ?? new Dictionary<int, string>());
            existing.UpdatedAt = DateTime.UtcNow;

            if (existing.CandidateProfile != null && attributeValues?.Any() == true)
            {
                await SyncCVAttributesToProfile(existing.CandidateProfile, attributeValues);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "CV updated successfully! Profile attributes synced.";
            return RedirectToAction("Details", new { id });
        }

        private async Task SyncCVAttributesToProfile(CandidateProfile profile, Dictionary<int, string> attributeValues)
        {
            if (profile == null || attributeValues == null) return;

            // Load existing profile attributes (already included via ThenInclude)
            var existingProfileAttrs = profile.ProfileAttributes.ToList();

            foreach (var kvp in attributeValues)
            {
                int attrId = kvp.Key;
                string value = kvp.Value?.Trim() ?? "";

                var profileAttr = existingProfileAttrs
                    .FirstOrDefault(pa => pa.AttributeDefinitionId == attrId);

                if (profileAttr != null)
                {
                    // Update existing
                    profileAttr.Value = value;
                }
                else
                {
                    // Create new
                    profile.ProfileAttributes.Add(new CandidateProfileAttribute
                    {
                        CandidateProfileId = profile.Id,
                        AttributeDefinitionId = attrId,
                        Value = value
                    });
                }
            }

            // Optional: Remove profile attributes that are no longer in this CV (if you want strict sync)
            // For now we keep them (profile can have more attributes than a single CV)
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
            return File(pdfBytes, "application/pdf", $"CV_{cv.User?.FullName ?? "Candidate"}_{cv.Position?.Title ?? "Position"}.pdf");
        }

        // POST: Add Comment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int cvId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return RedirectToAction("Details", new { id = cvId });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new Comment
            {
                CVId = cvId,
                UserId = userId,
                Message = message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = cvId });
        }

        // POST: Publish CV (Only if complete)
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Publish(int id)
        {
            var cv = await _context.CVs
                .Include(cv => cv.Position)
                    .ThenInclude(p => p.PositionAttributes)
                .Include(cv => cv.CandidateProfile)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cv == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cv.UserId != userId && !User.IsInRole("Administrator"))
                return Forbid();

            // Check if all required attributes are filled
            bool isComplete = IsCVComplete(cv);

            if (!isComplete)
            {
                TempData["Message"] = "Cannot publish: Please fill all required fields first.";
                return RedirectToAction("Details", new { id });
            }

            cv.IsPublished = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "CV published successfully! It is now visible to Recruiters.";
            return RedirectToAction("Details", new { id });
        }

        private bool IsCVComplete(CV cv)
        {
            if (cv.Position == null) return false;

            var attributeValues = string.IsNullOrEmpty(cv.AttributeValues)
                ? new Dictionary<int, string>()
                : JsonSerializer.Deserialize<Dictionary<int, string>>(cv.AttributeValues)
                  ?? new Dictionary<int, string>();

            foreach (var pa in cv.Position.PositionAttributes)
            {
                if (pa.AttributeDefinition.IsRequired)
                {
                    var value = attributeValues.GetValueOrDefault(pa.AttributeDefinitionId, "");
                    if (string.IsNullOrWhiteSpace(value))
                        return false;
                }
            }

            return true;
        }

        // POST: Unpublish CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Unpublish(int id)
        {
            var cv = await _context.CVs.FindAsync(id);
            if (cv == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cv.UserId != userId && !User.IsInRole("Administrator"))
                return Forbid();

            cv.IsPublished = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "CV unpublished.";
            return RedirectToAction("Details", new { id });
        }

        // POST: Like / Unlike CV
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int cvId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cv = await _context.CVs.FindAsync(cvId);
            if (cv == null) return NotFound();

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
            return RedirectToAction("Details", new { id = cvId });
        }
    }
}