using System.Threading.Tasks;

namespace MangaCompanion.Services
{
    public interface IMangaService
    {
        Task<object> ListarCapitulosManga();
    }
}
