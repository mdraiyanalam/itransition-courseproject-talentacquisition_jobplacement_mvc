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

        // GET: Attributes
        public async Task<IActionResult> Index()
        {
            var attributes = await _context.AttributeDefinitions
                .OrderBy(a => a.Name)
                .ToListAsync();
            return View(attributes);
        }

        // GET: Attributes/Create
        public IActionResult Create()
        {
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
                return RedirectToAction(nameof(Index));
            }
            return View(attributeDefinition);
        }

        // GET: Attributes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var attribute = await _context.AttributeDefinitions.FindAsync(id);
            if (attribute == null) return NotFound();

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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttributeDefinitionExists(attributeDefinition.Id))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
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
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AttributeDefinitionExists(int id)
        {
            return _context.AttributeDefinitions.Any(e => e.Id == id);
        }
    }
}