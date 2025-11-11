using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using JconsultGC.Models;
using Microsoft.AspNetCore.Authorization;

namespace JconsultGC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<User> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<User> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Redirection selon le rôle de l'utilisateur
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (User.IsInRole("Indexateur"))
                {
                    return RedirectToAction("Dashboard", "Indexateur");
                }
                else if (User.IsInRole("Viseur"))
                {
                    return RedirectToAction("Dashboard", "Viseur");
                }
                else if (User.IsInRole("ServiceExpéditeur"))
                {
                    return RedirectToAction("Dashboard", "Service");
                }
                // Ajouter d'autres rôles au besoin
            }
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
