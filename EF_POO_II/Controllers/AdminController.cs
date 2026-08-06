using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    [HttpGet("")]
    [HttpGet("[action]")]
    public IActionResult Index()
    {
        return View();
    }
}
