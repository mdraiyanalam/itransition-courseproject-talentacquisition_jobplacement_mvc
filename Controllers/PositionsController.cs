using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    [Authorize] // All authenticated users can view positions
    public class PositionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PositionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Positions (Candidates + Recruiters + Admins)
        public async Task<IActionResult> Index()
        {
            var positions = await _context.Positions
                .Include(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Profile Completion Check for Candidates
            if (User.IsInRole("Candidate"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var profile = await _context.CandidateProfiles
                    .Include(cp => cp.User)
                    .FirstOrDefaultAsync(cp => cp.UserId == userId);

                if (profile == null ||
                    string.IsNullOrWhiteSpace(profile.User?.FullName) ||
                    string.IsNullOrWhiteSpace(profile.Summary))
                {
                    ViewBag.ProfileIncomplete = true;
                    ViewBag.ProfileMessage = "Please complete your Full Name and Professional Summary before applying to any position.";
                }
            }

            return View(positions);
        }

        // GET: Positions/Create (Recruiter + Admin only)
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Attributes = await _context.AttributeDefinitions.OrderBy(a => a.Name).ToListAsync();
            return View();
        }

        // POST: Positions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Create(Position position, int[] selectedAttributes)
        {
            if (ModelState.IsValid)
            {
                _context.Add(position);

                if (selectedAttributes != null)
                {
                    for (int i = 0; i < selectedAttributes.Length; i++)
                    {
                        position.PositionAttributes.Add(new PositionAttribute
                        {
                            AttributeDefinitionId = selectedAttributes[i],
                            Order = i
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Attributes = await _context.AttributeDefinitions.ToListAsync();
            return View(position);
        }

        // GET: Positions/Edit/5
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (position == null) return NotFound();

            ViewBag.Attributes = await _context.AttributeDefinitions.OrderBy(a => a.Name).ToListAsync();
            ViewBag.SelectedAttributeIds = position.PositionAttributes.Select(pa => pa.AttributeDefinitionId).ToList();

            return View(position);
        }

        // POST: Positions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Edit(int id, Position position, int[] selectedAttributes)
        {
            if (id != position.Id) return NotFound();

            var existing = await _context.Positions
                .Include(p => p.PositionAttributes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existing.Title = position.Title;
                    existing.Description = position.Description;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.PositionAttributes.RemoveRange(existing.PositionAttributes);

                    if (selectedAttributes != null && selectedAttributes.Length > 0)
                    {
                        for (int i = 0; i < selectedAttributes.Length; i++)
                        {
                            existing.PositionAttributes.Add(new PositionAttribute
                            {
                                AttributeDefinitionId = selectedAttributes[i],
                                Order = i
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving position: " + ex.Message);
                }
            }

            ViewBag.Attributes = await _context.AttributeDefinitions.OrderBy(a => a.Name).ToListAsync();
            ViewBag.SelectedAttributeIds = selectedAttributes?.ToList() ?? new List<int>();

            return View(position);
        }

        // NEW: Duplicate Position
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Duplicate(int id)
        {
            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (position == null) return NotFound();

            var newPosition = new Position
            {
                Title = position.Title + " (Copy)",
                Description = position.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var pa in position.PositionAttributes)
            {
                newPosition.PositionAttributes.Add(new PositionAttribute
                {
                    AttributeDefinitionId = pa.AttributeDefinitionId,
                    Order = pa.Order
                });
            }

            _context.Positions.Add(newPosition);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"✅ Position '<b>{position.Title}</b>' duplicated successfully as '<b>{newPosition.Title}</b>'!";

            return RedirectToAction(nameof(Index));
        }

        private bool PositionExists(int id)
        {
            return _context.Positions.Any(e => e.Id == id);
        }
    }
}