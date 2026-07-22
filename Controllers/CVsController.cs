using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;
using talentacquisition_jobplacement_mvc.Services;
using talentacquisition_jobplacement_mvc.Helpers;

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
        [Authorize(Roles = "Candidate")]
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
                return RedirectToAction("Index", "Profiles");

            var model = new CV
            {
                PositionId = positionId,
                Position = position,
                UserId = userId,
                CandidateProfileId = profile.Id,
                CandidateProfile = profile
            };

            return View(model);
        }

        // POST: Save CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Create(CV cv, Dictionary<string, string> attributeValues)
        {
            System.Diagnostics.Debug.WriteLine("=== CV CREATE POST CALLED ===");

            // Get PositionId from hidden field
            if (int.TryParse(Request.Form["PositionId"], out int positionId))
            {
                cv.PositionId = positionId;
            }

            if (cv.PositionId <= 0)
            {
                ModelState.AddModelError("", "Invalid Position.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Manually set navigation properties
            cv.UserId = userId;
            cv.CreatedAt = DateTime.UtcNow;

            // Load Position
            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .FirstOrDefaultAsync(p => p.Id == cv.PositionId);

            if (position == null)
            {
                ModelState.AddModelError("", "Position not found.");
            }
            else
            {
                cv.Position = position;
            }

            // Load CandidateProfile
            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile != null)
            {
                cv.CandidateProfileId = profile.Id;
                cv.CandidateProfile = profile;
            }
            else
            {
                ModelState.AddModelError("", "Candidate profile not found. Please complete your profile first.");
            }

            // Parse attributes
            var parsed = attributeValues?
                .Where(kv => int.TryParse(kv.Key, out _))
                .ToDictionary(kv => int.Parse(kv.Key), kv => kv.Value ?? "")
                ?? new Dictionary<int, string>();

            cv.AttributeValues = JsonSerializer.Serialize(parsed);

            // Enforce access rules: prevent applying if position rules do not allow
            try
            {
                if (position != null)
                {
                    var allowed = AccessRuleEvaluator.CanApply(position, profile, parsed);
                    if (!allowed)
                    {
                        ModelState.AddModelError("", "You are not eligible to apply for this position based on access rules.");
                    }
                }
            }
            catch
            {
                // Fail closed: if evaluation errors, prevent application
                ModelState.AddModelError("", "Unable to evaluate access rules. Application is blocked.");
            }

            // Prevent duplicate CV for same position by same user
            var existingCv = await _context.CVs.FirstOrDefaultAsync(c => c.PositionId == cv.PositionId && c.UserId == userId);
            if (existingCv != null)
            {
                // Redirect candidate to edit the existing CV instead of creating a duplicate
                TempData["Error"] = "You already have an application for this position. You can update it instead.";
                return RedirectToAction(nameof(Edit), new { id = existingCv.Id });
            }

            await SyncCVAttributesToProfile(userId, parsed);

            if (ModelState.IsValid)
            {
                _context.CVs.Add(cv);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Application submitted successfully!";
                return RedirectToAction("MyApplications");
            }

            // Reload the view with errors (inline)
            var reloadPosition = await _context.Positions
                .Include(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .FirstOrDefaultAsync(p => p.Id == cv.PositionId);

            if (reloadPosition != null)
            {
                cv.Position = reloadPosition;
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

        // GET: All CVs
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Index(string searchString)
        {
            var cvs = _context.CVs
                .Include(c => c.User)
                .Include(c => c.Position)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                cvs = cvs.Where(c =>
                    (c.User.FullName != null && c.User.FullName.Contains(searchString)) ||
                    (c.Position.Title != null && c.Position.Title.Contains(searchString))
                );
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
                    .ThenInclude(p => p.PositionAttributes)
                    .ThenInclude(pa => pa.AttributeDefinition)
                .Include(c => c.CandidateProfile)
                    .ThenInclude(cp => cp.ProfileAttributes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cv == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cv.UserId != userId && !User.IsInRole("Administrator"))
                return Forbid();

            var attrValues = new Dictionary<int, string>();
            if (cv.CandidateProfile != null)
            {
                foreach (var pa in cv.CandidateProfile.ProfileAttributes)
                    attrValues[pa.AttributeDefinitionId] = pa.Value;
            }

            var cvValues = string.IsNullOrEmpty(cv.AttributeValues)
                ? new Dictionary<int, string>()
                : JsonSerializer.Deserialize<Dictionary<int, string>>(cv.AttributeValues) ?? new();

            foreach (var kv in cvValues)
                attrValues[kv.Key] = kv.Value;

            ViewBag.AttributeValues = attrValues;
            return View(cv);
        }

        // POST: Edit CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Edit(int id, CV cv, Dictionary<int, string> attributeValues, string? action = null)
        {
            var existingCv = await _context.CVs
                .Include(c => c.Position)
                    .ThenInclude(p => p.PositionAttributes)
                .Include(c => c.CandidateProfile)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingCv == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingCv.UserId != userId && !User.IsInRole("Administrator"))
                return Forbid();

            existingCv.AttributeValues = JsonSerializer.Serialize(attributeValues ?? new Dictionary<int, string>());
            existingCv.UpdatedAt = DateTime.UtcNow;

            // If trying to publish, validate completeness
            if (action?.ToLower() == "publish")
            {
                if (!IsCVComplete(existingCv))
                {
                    TempData["Error"] = "Cannot publish: All required attributes must be filled.";
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Edit), new { id });
                }
                existingCv.IsPublished = true;
            }
            else if (action?.ToLower() == "unpublish")
            {
                existingCv.IsPublished = false;
            }
            // If just saving, keep current publish status

            await SyncCVAttributesToProfile(userId, attributeValues);
            await _context.SaveChangesAsync();

            if (action?.ToLower() == "publish")
                TempData["Success"] = "CV published successfully!";
            else if (action?.ToLower() == "unpublish")
                TempData["Success"] = "CV unpublished.";
            else
                TempData["Success"] = "CV updated successfully!";

            return RedirectToAction(nameof(Details), new { id });
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
            return File(pdfBytes, "application/pdf", $"CV_{cv.User?.FullName?.Replace(" ", "_") ?? "Candidate"}.pdf");
        }

        // POST: Add Comment
        [HttpPost]
        [Authorize(Roles = "Candidate,Recruiter,Administrator")]
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

        // POST: Publish
        [HttpPost]
        [Authorize(Roles = "Candidate,Recruiter,Administrator")]
        public async Task<IActionResult> Publish(int id)
        {
            var cv = await _context.CVs
                .Include(c => c.Position)
                    .ThenInclude(p => p.PositionAttributes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cv == null) return NotFound();

            if (!IsCVComplete(cv))
            {
                TempData["Error"] = "Cannot publish: All required attributes must be filled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            cv.IsPublished = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "CV published successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

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

        [HttpPost]
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> ToggleLike(int cvId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existing = await _context.CVLikes
                .FirstOrDefaultAsync(l => l.CVId == cvId && l.UserId == userId);

            if (existing != null)
                _context.CVLikes.Remove(existing);
            else
                _context.CVLikes.Add(new CVLike { CVId = cvId, UserId = userId });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = cvId });
        }

        private bool IsCVComplete(CV cv)
        {
            if (cv.Position == null) return false;

            var values = string.IsNullOrEmpty(cv.AttributeValues)
                ? new Dictionary<int, string>()
                : JsonSerializer.Deserialize<Dictionary<int, string>>(cv.AttributeValues)
                  ?? new Dictionary<int, string>();

            foreach (var pa in cv.Position.PositionAttributes)
            {
                if (pa.AttributeDefinition.IsRequired &&
                    string.IsNullOrWhiteSpace(values.GetValueOrDefault(pa.AttributeDefinitionId, "")))
                    return false;
            }
            return true;
        }

        private async Task SyncCVAttributesToProfile(string userId, Dictionary<int, string> attributeValues)
        {
            var profile = await _context.CandidateProfiles
                .Include(cp => cp.ProfileAttributes)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null) return;

            foreach (var kv in attributeValues ?? new Dictionary<int, string>())
            {
                var existing = profile.ProfileAttributes.FirstOrDefault(pa => pa.AttributeDefinitionId == kv.Key);

                if (existing != null)
                    existing.Value = kv.Value ?? "";
                else
                {
                    profile.ProfileAttributes.Add(new CandidateProfileAttribute
                    {
                        CandidateProfileId = profile.Id,
                        AttributeDefinitionId = kv.Key,
                        Value = kv.Value ?? ""
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // Export to CSV
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> ExportToCsv(int positionId)
        {
            var cvs = await _context.CVs
                .Include(c => c.User)
                .Include(c => c.Position)
                .Where(c => c.PositionId == positionId)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Candidate Name,Email,Position Title,Applied Date,Published");

            foreach (var cv in cvs)
            {
                sb.AppendLine($"\"{cv.User?.FullName}\",\"{cv.User?.Email}\",\"{cv.Position?.Title}\",{cv.CreatedAt:yyyy-MM-dd HH:mm},{cv.IsPublished}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"Position_{positionId}_Applications.csv");
        }
    }
}




