using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Controllers
{
    public class SiteController : Controller
    {
        private readonly ISiteClient _siteClient;

        public SiteController(ISiteClient siteClient)
        {
            _siteClient = siteClient;
        }

        public async Task<IActionResult> Index()
        {
            List<SheetSite> sites = (await _siteClient.GetSites()).Where(s => s.IsActive).ToList();
            return View(sites);
        }

        public IActionResult Add()
        {
            return View("Edit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(SheetSite site)
        {
            if (ModelState.IsValid)
            {
                if (await _siteClient.Add(site))
                {
                    TempData["Message"] = $"Add site '{site.Name}'";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = $"Error adding a site '{site.Name}'";
            }
            return View("Edit", site);

        }

        public async Task<IActionResult> Edit(int siteId)
        {
            var site = await _siteClient.GetSiteById(siteId);
            if (site is null)
            {
                TempData["Error"] = $"Site with id '{siteId}' not found";
                return RedirectToAction(nameof(Index));
            }

            return View("Edit", site);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SheetSite site)
        {
            if (ModelState.IsValid)
            {
                if (await _siteClient.Update(site))
                {
                    TempData["Message"] = $"The '{site.Name}' site has been updated";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = $"'{site.Name} ' site has been updated";
            }
            return View("Edit", await _siteClient.GetSiteById(site.Id));
        }

        public async Task<IActionResult> Delete(int siteId, string name)
        {
            if (await _siteClient.Delete(siteId))
            {
                TempData["Message"] = $"The site has been deleted";
            }
            else
            {
                TempData["Error"] = $"Error deleting a site with id '{siteId}'";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
