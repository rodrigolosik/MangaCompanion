using MangaCompanion.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace MangaCompanion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MangaController : ControllerBase
    {
        //const string url = "http://centraldemangas.online/titulos/bf8d2eba-c4fd-42e7-b520-1916df132737";
        const string url = "http://centraldemangas.online/titulos/9b0f585f_2f67_4995_86dd_f0903d5a452d";

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
