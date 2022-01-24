using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernyzeWebsite.Data;
using ModernyzeWebsite.Models.FourUp;

namespace ModernyzeWebsite.Controllers;

public class FourUpController : Controller {
    private readonly ModernyzeWebsiteContext db;

    public FourUpController(ModernyzeWebsiteContext context) {
        this.db = context;
    }

    // GET: FourUp
    public async Task<IActionResult> Index() {
        return View(await this.db.FourUp.ToListAsync());
    }

    // GET: FourUp/Create
    public IActionResult Create() {
        return View();
    }

    // GET: Latest Four-Up
    public async Task<IActionResult> GetLatest() {
        Task<FourUp> getLatest = this.db.FourUp.OrderByDescending(f => f.WeekOf).FirstAsync();
        await getLatest.WaitAsync(new TimeSpan(0, 0, 10));
        FourUp latest = getLatest.Result;
        return PartialView("Latest", latest);
    }

    // POST: FourUp/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Source,WeekOf")] FourUp fourUp) {
        if (this.ModelState.IsValid) {
            this.db.Add(fourUp);
            await this.db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(fourUp);
    }
}
