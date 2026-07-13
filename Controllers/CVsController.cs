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
        public async Task<IActionResult> Create(int positionId) { /* ... your existing code ... */ }

        // POST: Save CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Create(CV cv, Dictionary<int, string> attributeValues) { /* ... */ }

        // GET: My Applications
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> MyApplications() { /* ... */ }

        // GET: All CVs (Recruiter)
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Index(string searchString) { /* ... */ }

        // GET: CV Details
        [Authorize]
        public async Task<IActionResult> Details(int id) { /* ... */ }

        // GET: Edit CV
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Edit(int id) { /* ... */ }

        // POST: Edit CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Edit(int id, CV cv, Dictionary<int, string> attributeValues) { /* ... */ }

        // GET: Download PDF
        [Authorize]
        public async Task<IActionResult> Download(int id) { /* ... */ }

        // POST: Add Comment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int cvId, string message) { /* ... */ }

        // POST: Publish CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Publish(int id) { /* ... */ }

        // POST: Unpublish CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Unpublish(int id) { /* ... */ }

        // POST: Toggle Like
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int cvId) { /* ... */ }

        private bool IsCVComplete(CV cv) { /* ... */ }

        private async Task SyncCVAttributesToProfile(CandidateProfile profile, Dictionary<int, string> attributeValues) { /* ... */ }
    }
}