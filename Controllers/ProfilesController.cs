using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    [Authorize(Roles = "Candidate,Recruiter,Administrator")]
    public class ProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfilesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: My Professional Profile (or Read-only for Recruiters)
        public async Task<IActionResult> Index(string? userId = null)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var targetUserId = userId ?? currentUserId;

            var user = await _userManager.FindByIdAsync(targetUserId);
            if (user == null) return NotFound();

            var profile = await _context.CandidateProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.ProfileAttributes)
                .FirstOrDefaultAsync(cp => cp.UserId == targetUserId);

            if (profile == null)
            {
                profile = new CandidateProfile { UserId = targetUserId, User = user };
                _context.CandidateProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            // Load all attributes
            ViewBag.AllAttributes = await _context.AttributeDefinitions
                .OrderBy(a => a.Category)
                .ThenBy(a => a.Name)
                .ToListAsync();

            ViewBag.IsReadOnly = userId != null && userId != currentUserId;

            return View(profile);
        }

        // POST: Update Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CandidateProfile model, IFormFile? profilePhoto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Security Check: Prevent editing read-only profiles
            bool isReadOnly = Request.Form["IsReadOnly"] == "true" ||
                             (ViewBag.IsReadOnly != null && (bool)ViewBag.IsReadOnly);

            if (isReadOnly)
            {
                return Forbid();
            }

            // Profile Photo Upload
            if (profilePhoto != null && profilePhoto.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.ProfilePhotoUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfilePhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-photos");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePhoto.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await profilePhoto.CopyToAsync(stream);

                user.ProfilePhotoUrl = "/uploads/profile-photos/" + uniqueFileName;
                await _userManager.UpdateAsync(user);
            }

            // Update Full Name
            if (model.User != null && !string.IsNullOrEmpty(model.User.FullName))
            {
                user.FullName = model.User.FullName;
                await _userManager.UpdateAsync(user);
            }

            // Get or create profile
            var profile = await _context.CandidateProfiles
                .Include(cp => cp.ProfileAttributes)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId };
                _context.CandidateProfiles.Add(profile);
            }

            profile.Summary = model.Summary;
            profile.Experience = model.Experience;
            profile.Education = model.Education;

            // Handle attribute values
            var attributeValues = new Dictionary<int, string>();
            foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("attributeValues[")))
            {
                if (int.TryParse(key.Replace("attributeValues[", "").Replace("]", ""), out int attrId))
                {
                    attributeValues[attrId] = Request.Form[key].ToString().Trim();
                }
            }

            // Sync to profile
            await SyncProfileAttributes(profile, attributeValues);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully! Attributes synced.";
            return RedirectToAction(nameof(Index));
        }

        private async Task SyncProfileAttributes(CandidateProfile profile, Dictionary<int, string> attributeValues)
        {
            if (profile == null) return;

            var selectedAttributeIds = Request.Form["selectedAttributeIds"]
                .Select(id => int.TryParse(id, out int parsed) ? parsed : 0)
                .Where(id => id > 0)
                .ToList();

            foreach (var attrId in selectedAttributeIds)
            {
                string value = attributeValues.GetValueOrDefault(attrId, "");

                var existingAttr = profile.ProfileAttributes
                    .FirstOrDefault(pa => pa.AttributeDefinitionId == attrId);

                if (existingAttr != null)
                {
                    existingAttr.Value = value;
                }
                else
                {
                    profile.ProfileAttributes.Add(new CandidateProfileAttribute
                    {
                        CandidateProfileId = profile.Id,
                        AttributeDefinitionId = attrId,
                        Value = value
                    });
                }
            }

            // Remove unchecked attributes
            var attributesToRemove = profile.ProfileAttributes
                .Where(pa => !selectedAttributeIds.Contains(pa.AttributeDefinitionId))
                .ToList();

            foreach (var toRemove in attributesToRemove)
            {
                _context.CandidateProfileAttributes.Remove(toRemove);
            }
        }

        // === PROJECTS ===
        public async Task<IActionResult> Projects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.CandidateProfiles
                .Include(cp => cp.Projects)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            return View(profile?.Projects ?? new List<Project>());
        }

        // GET: Edit Project
        public async Task<IActionResult> EditProject(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var project = await _context.Projects
                .Include(p => p.CandidateProfile)
                .ThenInclude(cp => cp.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.CandidateProfile.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Edit Project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(Project model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var project = await _context.Projects
                .Include(p => p.CandidateProfile)
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.CandidateProfile.UserId == userId);

            if (project == null) return NotFound();

            project.Name = model.Name;
            project.StartDate = model.StartDate;
            project.EndDate = model.EndDate;
            project.Description = model.Description;
            // Normalize tags - accept JSON-like or comma-separated
            if (!string.IsNullOrWhiteSpace(model.TechnologyTags) && model.TechnologyTags.TrimStart().StartsWith("["))
            {
                // simple JSON array of objects like [{"value":"ASP.NET"},{"value":"SSMS"}]
                try
                {
                    var values = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(model.TechnologyTags);
                    if (values != null)
                    {
                        project.TechnologyTags = string.Join(", ", values.Select(v => v.GetValueOrDefault("value")?.Trim()).Where(s => !string.IsNullOrEmpty(s)));
                    }
                }
                catch
                {
                    // fallback: keep raw
                    project.TechnologyTags = model.TechnologyTags;
                }
            }
            else
            {
                project.TechnologyTags = model.TechnologyTags;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Project updated successfully.";
            return RedirectToAction(nameof(Projects));
        }

        // API endpoints for attribute suggestions / recent
        [HttpGet]
        public async Task<IActionResult> AttributeSuggest(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Json(new object[0]);

            var query = q.Trim().ToLower();
            var results = await _context.AttributeDefinitions
                .Where(a => a.Name.ToLower().Contains(query) || (a.Description != null && a.Description.ToLower().Contains(query)))
                .OrderBy(a => a.Name)
                .Take(12)
                .Select(a => new { a.Id, a.Name, a.Category, a.Type })
                .ToListAsync();

            return Json(results);
        }

        [HttpGet]
        public async Task<IActionResult> AttributeRecent()
        {
            var recent = await _context.PositionAttributes
                .Include(pa => pa.AttributeDefinition)
                .GroupBy(pa => pa.AttributeDefinitionId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.First().AttributeDefinition)
                .Take(8)
                .Select(a => new { a.Id, a.Name, a.Category, a.Type })
                .ToListAsync();

            return Json(recent);
        }

        // GET: Tag suggestions used by Tagify on the Projects form
        [HttpGet]
        public async Task<IActionResult> TagSuggestions()
        {
            var tagStrings = await _context.Projects
                .Where(p => !string.IsNullOrEmpty(p.TechnologyTags))
                .Select(p => p.TechnologyTags)
                .ToListAsync();

            var tags = tagStrings
                .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();

            return Json(tags);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProject(Project project)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ===== FIX FOR POSTGRESQL DATETIME =====
            if (project.StartDate.Kind == DateTimeKind.Unspecified)
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);

            if (project.EndDate.HasValue && project.EndDate.Value.Kind == DateTimeKind.Unspecified)
                project.EndDate = DateTime.SpecifyKind(project.EndDate.Value, DateTimeKind.Utc);
            // =======================================

            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(cp => cp.UserId == userId);
            if (profile == null) return NotFound();

            project.CandidateProfileId = profile.Id;
            // normalize tags
            if (!string.IsNullOrWhiteSpace(project.TechnologyTags) && project.TechnologyTags.TrimStart().StartsWith("["))
            {
                try
                {
                    var values = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(project.TechnologyTags);
                    if (values != null)
                    {
                        project.TechnologyTags = string.Join(", ", values.Select(v => v.GetValueOrDefault("value")?.Trim()).Where(s => !string.IsNullOrEmpty(s)));
                    }
                }
                catch
                {
                    // ignore, keep original
                }
            }
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Project added successfully!";
            return RedirectToAction(nameof(Projects));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Projects));
        }

        // GET: View Candidate Profile (Read-only for Recruiters/Admins)
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> ViewProfile(string userId)
        {
            var profile = await _context.CandidateProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.ProfileAttributes)
                    .ThenInclude(pa => pa.AttributeDefinition)
                .Include(cp => cp.Projects)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null) return NotFound();

            ViewBag.IsReadOnly = true;
            ViewBag.AllAttributes = await _context.AttributeDefinitions
                .OrderBy(a => a.Category).ThenBy(a => a.Name).ToListAsync();

            return View("Index", profile);
        }

        private async Task CheckAndAwardAchievements(ApplicationUser user, CandidateProfile profile)
        {
            if (profile.Projects.Count >= 10)
            {
                if (!profile.Achievements.Any(a => a.Name == "ProjectMaster"))
                {
                    profile.Achievements.Add(new UserAchievement
                    {
                        UserId = user.Id,
                        Name = "ProjectMaster",
                        Description = "10+ Projects",
                        Icon = "🏆"
                    });
                }
            }

            var cvCount = await _context.CVs.CountAsync(c => c.UserId == user.Id);
            if (cvCount >= 5)
            {
                if (!profile.Achievements.Any(a => a.Name == "ActiveApplicant"))
                {
                    profile.Achievements.Add(new UserAchievement
                    {
                        UserId = user.Id,
                        Name = "ActiveApplicant",
                        Description = "5+ Applications",
                        Icon = "📝"
                    });
                }
            }
        }
    }
}