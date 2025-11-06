using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using JconsultGC.Models;

namespace JconsultGC.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Trouver d'abord l'utilisateur par email pour vérifier s'il est actif
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Ce compte est désactivé.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (user != null)
                {
                    // Mettre à jour LastLoginAt
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    // Rediriger selon le rôle
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Indexateur"))
                    {
                        return RedirectToAction("Dashboard", "Indexateur");
                    }
                    return RedirectToAction("Index", "Home");
                }
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Le compte est verrouillé.");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "La connexion n'est pas autorisée pour ce compte.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
            }
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Societe = model.Societe,
                Telephone = model.Telephone,
                Role = model.Role,
                ServiceId = model.ServiceId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role.ToString());
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}