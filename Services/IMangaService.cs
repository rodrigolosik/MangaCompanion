using MangaCompanion.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MangaCompanion.Services
{
    public interface IMangaService
    {
        Task<Manga> ListarCapitulosManga(Manga manga);

        Task<IEnumerable<Manga>> ListarMangas();
    }
}
