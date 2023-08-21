using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UI.Infrastructure.Filters;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    [APIAuthorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.IsInRole(Role.Admin.ToString()))
            {
                return RedirectToAction("Index", "Admin");
            }
            if (User.IsInRole(Role.AdminAgency.ToString()))
            {
#if DEBUGOFFLINE
                return RedirectToAction("Statistics", "AdminAgency");
#else
                return RedirectToAction("Index", "AdminAgency");
#endif
            }
            if (User.IsInRole(Role.Operator.ToString()))
            {
                return RedirectToAction("Index", "Operator");
            }
            if (User.IsInRole(Role.User.ToString()))
            {
                return RedirectToAction("Index", "User");
            }
            return NotFound();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}