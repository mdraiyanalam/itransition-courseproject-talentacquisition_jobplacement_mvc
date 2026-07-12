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

        // GET: My Professional Profile
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var profile = await _context.CandidateProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.ProfileAttributes)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId, User = user };
                _context.CandidateProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            // Load all attributes for the view
            ViewBag.AllAttributes = await _context.AttributeDefinitions
                .OrderBy(a => a.Category)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return View(profile);
        }

        // POST: Update Profile + Photo + Enhanced Attribute Sync
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CandidateProfile model, IFormFile? profilePhoto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Photo Upload
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

            // === ENHANCED PROFILE ATTRIBUTE SYNC ===
            await SyncProfileAttributes(profile);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully! Attributes synced.";
            return RedirectToAction(nameof(Index));
        }

        // Helper: Sync submitted attribute values from form to CandidateProfileAttribute
        private async Task SyncProfileAttributes(CandidateProfile profile)
        {
            if (profile == null) return;

            // Get selected attribute IDs
            var selectedAttributeIds = Request.Form["selectedAttributeIds"]
                .Select(id => int.TryParse(id, out int parsed) ? parsed : 0)
                .Where(id => id > 0)
                .ToList();

            // Get attribute values from form
            var attributeValues = new Dictionary<int, string>();
            foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("attributeValues[")))
            {
                if (int.TryParse(key.Replace("attributeValues[", "").Replace("]", ""), out int attrId))
                {
                    attributeValues[attrId] = Request.Form[key].ToString().Trim();
                }
            }

            // Update or create profile attributes
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

            // Remove attributes that were unchecked
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

        [HttpPost]
        public async Task<IActionResult> SaveProject(Project project)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(cp => cp.UserId == userId);
            if (profile == null) return NotFound();

            project.CandidateProfileId = profile.Id;
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
                .OrderBy(a => a.Category)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return View("Index", profile); // Reuse Index view with read-only mode
        }
    }
}