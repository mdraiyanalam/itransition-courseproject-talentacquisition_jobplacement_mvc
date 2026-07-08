using Microsoft.AspNetCore.Authorization;
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

        public ProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.CandidateProfiles
                .Include(cp => cp.User)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId };
                _context.CandidateProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CandidateProfile model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _context.Users.FindAsync(userId);
            if (user != null && model.User != null)
            {
                user.FullName = model.User.FullName;
            }

            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId };
                _context.CandidateProfiles.Add(profile);
            }

            profile.Summary = model.Summary;
            profile.Experience = model.Experience;
            profile.Education = model.Education;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Profile updated successfully! You can now apply to positions.";

            return RedirectToAction(nameof(Index));
        }
    }
}