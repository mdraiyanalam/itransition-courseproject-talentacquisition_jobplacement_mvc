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

            return View(profile);
        }

        // POST: Update Profile + Photo
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
    }
}