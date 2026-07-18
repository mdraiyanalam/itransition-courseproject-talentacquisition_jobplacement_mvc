using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    [Authorize(Roles = "Recruiter,Administrator")]
    public class AttributesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttributesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Attributes with Search & Category Filter
        public async Task<IActionResult> Index(string searchString, string categoryFilter)
        {
            var attributes = _context.AttributeDefinitions
                .OrderBy(a => a.Category)
                .ThenBy(a => a.Name)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                attributes = attributes.Where(a =>
                    a.Name.ToLower().Contains(searchString) ||
                    (a.Description != null && a.Description.ToLower().Contains(searchString))
                );
            }

            if (!string.IsNullOrEmpty(categoryFilter))
                attributes = attributes.Where(a => a.Category == categoryFilter);

            ViewBag.SearchString = searchString;
            ViewBag.CategoryFilter = categoryFilter;

            ViewBag.Categories = await _context.AttributeDefinitions
                .Select(a => a.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.RecentAttributes = await _context.PositionAttributes
                .Include(pa => pa.AttributeDefinition)
                .GroupBy(pa => pa.AttributeDefinition)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(8)
                .ToListAsync();

            return View(await attributes.ToListAsync());
        }

        // GET: Attributes/Create
        public IActionResult Create()
        {
            ViewBag.Categories = GetPredefinedCategories();
            return View();
        }

        // POST: Attributes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AttributeDefinition attributeDefinition)
        {
            if (ModelState.IsValid)
            {
                _context.Add(attributeDefinition);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Attribute created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = GetPredefinedCategories();
            return View(attributeDefinition);
        }

        // GET: Attributes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var attribute = await _context.AttributeDefinitions.FindAsync(id);
            if (attribute == null) return NotFound();

            ViewBag.Categories = GetPredefinedCategories();
            return View(attribute);
        }

        // POST: Attributes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AttributeDefinition attributeDefinition)
        {
            if (id != attributeDefinition.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attributeDefinition);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Attribute updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttributeDefinitionExists(attributeDefinition.Id))
                        return NotFound();
                    else throw;
                }
            }
            ViewBag.Categories = GetPredefinedCategories();
            return View(attributeDefinition);
        }

        // GET: Attributes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var attribute = await _context.AttributeDefinitions
                .FirstOrDefaultAsync(m => m.Id == id);

            if (attribute == null) return NotFound();

            return View(attribute);
        }

        // POST: Attributes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attribute = await _context.AttributeDefinitions.FindAsync(id);
            if (attribute != null)
            {
                _context.AttributeDefinitions.Remove(attribute);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Attribute deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AttributeDefinitionExists(int id)
        {
            return _context.AttributeDefinitions.Any(e => e.Id == id);
        }

        private List<string> GetPredefinedCategories()
        {
            return new List<string>
            {
                "Personal Information", "Education", "Experience", "Skills",
                "Certifications", "Languages", "Soft Skills",
                "Domain Knowledge", "Availability", "General"
            };
        }
    }
}