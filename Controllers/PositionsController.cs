using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Hubs;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    [Authorize]
    public class PositionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DiscussionHub> _hubContext;

        public PositionsController(ApplicationDbContext context, IHubContext<DiscussionHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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
                    (p.Description != null && p.Description.ToLower().Contains(searchString)) ||
                    (p.Company != null && p.Company.ToLower().Contains(searchString)) ||
                    (p.ProjectTags != null && p.ProjectTags.ToLower().Contains(searchString))
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
                    ViewBag.ProfileMessage = "Please complete your profile before applying.";
                }
            }

            return View(await positions.ToListAsync());
        }

        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Attributes = await _context.AttributeDefinitions.OrderBy(a => a.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Create(Position position, int[] selectedAttributes)
        {
            if (ModelState.IsValid)
            {
                position.CreatedAt = DateTime.UtcNow;
                position.UpdatedAt = DateTime.UtcNow;
                position.AccessRules = Request.Form["AccessRulesJson"].ToString() ?? "[]";

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
                TempData["Success"] = $"Position '<b>{position.Title}</b>' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Attributes = await _context.AttributeDefinitions.ToListAsync();
            return View(position);
        }

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
                    existing.Company = position.Company;
                    existing.Level = position.Level;
                    existing.ProjectTags = position.ProjectTags;
                    existing.MaxProjects = position.MaxProjects;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.AccessRules = Request.Form["AccessRulesJson"].ToString() ?? existing.AccessRules;

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
                    TempData["Success"] = $"Position '<b>{existing.Title}</b>' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            ViewBag.Attributes = await _context.AttributeDefinitions.OrderBy(a => a.Name).ToListAsync();
            ViewBag.SelectedAttributeIds = selectedAttributes?.ToList() ?? new List<int>();
            return View(position);
        }

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
                Company = position.Company,
                Level = position.Level,
                ProjectTags = position.ProjectTags,
                MaxProjects = position.MaxProjects,
                AccessRules = position.AccessRules,
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

            TempData["Success"] = "Position duplicated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Position Details
        public async Task<IActionResult> Details(int id)
        {
            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                    .ThenInclude(pa => pa.AttributeDefinition)
                .Include(p => p.CVs)
                .Include(p => p.DiscussionPosts)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (position == null) return NotFound();

            return View(position);
        }

        // POST: Add Discussion Post with SignalR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDiscussion(int PositionId, string Message)
        {
            if (string.IsNullOrWhiteSpace(Message))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = PositionId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);

            var discussion = new DiscussionPost
            {
                PositionId = PositionId,
                UserId = userId,
                Message = Message.Trim()
            };

            _context.DiscussionPosts.Add(discussion);
            await _context.SaveChangesAsync();

            // Broadcast via SignalR to all clients
            await _hubContext.Clients.All.SendAsync("ReceiveMessage",
                user?.FullName ?? "Anonymous",
                discussion.Message,
                discussion.CreatedAt.ToString("g"));

            TempData["Success"] = "Comment posted successfully!";
            return RedirectToAction(nameof(Details), new { id = PositionId });
        }
    }
}