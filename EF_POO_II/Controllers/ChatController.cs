using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
[Authorize]
public class ChatController : Controller
{
    [HttpGet("")]
    [HttpGet("[action]")]
    public IActionResult Index()
    {
        return View();
    }
}
