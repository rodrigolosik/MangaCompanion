using MangaCompanion.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MangaCompanion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MangaController : ControllerBase
    {
        private readonly IMangaService _service;

        public MangaController(IMangaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var mangas = await _service.ListarMangas();
            return Ok(mangas);
        }

    }
}
