using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    [AllowAnonymous]
    public class PositionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PositionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var positions = _context.Positions
                .Include(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                positions = positions.Where(p =>
                    p.Title.ToLower().Contains(searchString) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchString))
                );
            }

            ViewBag.SearchString = searchString;

            if (User.IsInRole("Candidate"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var profile = await _context.CandidateProfiles
                    .Include(cp => cp.User)
                    .FirstOrDefaultAsync(cp => cp.UserId == userId);

                if (profile == null || string.IsNullOrWhiteSpace(profile.User?.FullName) ||
                    string.IsNullOrWhiteSpace(profile.Summary))
                {
                    ViewBag.ProfileIncomplete = true;
                    ViewBag.ProfileMessage = "Please complete your Full Name and Professional Summary before applying.";
                }
            }

            return View(await positions.ToListAsync());
        }

        // GET: Positions/Create
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
                position.AllowedRoles = Request.Form["AllowedRoles"].ToString() ?? "Candidate";

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
                    existing.AllowedRoles = Request.Form["AllowedRoles"].ToString() ?? "Candidate";

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

        // Duplicate Position
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Duplicate(int id)
        {
            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (position == null) return NotFound();

            var newPosition = new Position
            {
                Title = position.Title + " (Copy)",
                Description = position.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AllowedRoles = position.AllowedRoles
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

            TempData["Success"] = $"✅ Position '<b>{position.Title}</b>' duplicated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Positions/Delete/5
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (position == null) return NotFound();

            return View(position);
        }

        // POST: Positions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position != null)
            {
                _context.Positions.Remove(position);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Position '{position.Title}' deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}