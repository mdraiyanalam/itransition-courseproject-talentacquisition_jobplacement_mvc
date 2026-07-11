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
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId, User = user };
                _context.CandidateProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            // Load all attributes for selection
            ViewBag.AllAttributes = await _context.AttributeDefinitions
                .OrderBy(a => a.Name)
                .ToListAsync();

            return View(profile);
        }

        // POST: Update Profile + Photo + Attributes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CandidateProfile model, IFormFile? profilePhoto, int[] selectedAttributeIds)
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

            // Update basic info
            if (model.User != null && !string.IsNullOrEmpty(model.User.FullName))
            {
                user.FullName = model.User.FullName;
                await _userManager.UpdateAsync(user);
            }

            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(cp => cp.UserId == userId);
            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId };
                _context.CandidateProfiles.Add(profile);
            }

            profile.Summary = model.Summary;
            profile.Experience = model.Experience;
            profile.Education = model.Education;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully!";

            return RedirectToAction(nameof(Index));
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

        // GET: Edit Project
        public async Task<IActionResult> EditProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();
            return View(project);
        }

        // POST: Edit Project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(int id, Project project)
        {
            if (id != project.Id) return NotFound();

            var existing = await _context.Projects.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = project.Name;
            existing.StartDate = project.StartDate;
            existing.EndDate = project.EndDate;
            existing.Description = project.Description;
            existing.TechnologyTags = project.TechnologyTags;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Project updated successfully!";
            return RedirectToAction(nameof(Projects));
        }

        // POST: Update Profile + Photo + Attributes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CandidateProfile model, IFormFile? profilePhoto, int[] selectedAttributeIds, Dictionary<int, string> attributeValues)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Photo Upload (existing code)
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

            // Update basic info
            if (model.User != null && !string.IsNullOrEmpty(model.User.FullName))
            {
                user.FullName = model.User.FullName;
                await _userManager.UpdateAsync(user);
            }

            var profile = await _context.CandidateProfiles
                .Include(cp => cp.ProfileAttributes)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId };
                _context.CandidateProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            profile.Summary = model.Summary;
            profile.Experience = model.Experience;
            profile.Education = model.Education;

            // === Handle Profile Attributes ===
            if (selectedAttributeIds != null)
            {
                // Remove old attributes not in new selection
                var attributesToRemove = profile.ProfileAttributes
                    .Where(pa => !selectedAttributeIds.Contains(pa.AttributeDefinitionId))
                    .ToList();

                _context.CandidateProfileAttributes.RemoveRange(attributesToRemove);

                // Add/Update selected attributes
                foreach (var attrId in selectedAttributeIds)
                {
                    var existing = profile.ProfileAttributes
                        .FirstOrDefault(pa => pa.AttributeDefinitionId == attrId);

                    var value = attributeValues.ContainsKey(attrId) ? attributeValues[attrId] : string.Empty;

                    if (existing != null)
                    {
                        existing.Value = value;
                    }
                    else
                    {
                        profile.ProfileAttributes.Add(new CandidateProfileAttribute
                        {
                            AttributeDefinitionId = attrId,
                            Value = value
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}