using System.Collections.Generic;

namespace MangaCompanion.Models
{
    public class Manga
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Titulo { get; set; }
        public string Capa { get; set; }
        public IEnumerable<object> Capitulos { get; set; }
    }
}
