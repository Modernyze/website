using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernyzeWebsite.Data;
using ModernyzeWebsite.Models.FourUp;

namespace ModernyzeWebsite.Controllers; 

public class FourUpController : Controller {
    private readonly ModernyzeWebsiteContext _context;

    public FourUpController(ModernyzeWebsiteContext context) {
        this._context = context;
    }

    // GET: FourUp
    public async Task<IActionResult> Index() {
        return View(await this._context.FourUp.ToListAsync());
    }

    // GET: FourUp/Create
    public IActionResult Create() {
        return View();
    }

    // POST: FourUp/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Source,WeekOf")] FourUp fourUp) {
        if (this.ModelState.IsValid) {
            this._context.Add(fourUp);
            await this._context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(fourUp);
    }

    
}
