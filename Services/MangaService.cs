using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MangaCompanion.Services
{
    public class MangaService : IMangaService
    {
        const string url = "http://centraldemangas.online/titulos/9b0f585f_2f67_4995_86dd_f0903d5a452d";

        public async Task<object> ListarCapitulosManga()
        {
            string html = string.Empty;

            using (HttpClient httpClient = new HttpClient())
                html = await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            // No central de mangás os itens estão dentro de uma tabela;
            var tabela = htmlDocument.DocumentNode.Descendants("table").FirstOrDefault();

            var listaCapitulos = ListarCapitulos(tabela);
            var listaDatas = ListarDatas(tabela);
            var capitulos = JuntarCapitulosComDatas(listaCapitulos, listaDatas);
            var titulo = PegarTitulo(htmlDocument);
            var capa = PegarUrlImagem(htmlDocument);

            return new { titulo, capa, capitulos };
        }

        public string PegarTitulo(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.Descendants("h1").FirstOrDefault().InnerText.Trim();
        }

        public string PegarUrlImagem(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.Descendants("img").Where(node => node.GetAttributeValue("src", "").Contains("capas")).FirstOrDefault().Attributes.FirstOrDefault().Value;
        }

        public IEnumerable<HtmlNode> ListarCapitulos(HtmlNode node)
        {
            return node.Descendants("a").ToList();
        }

        public IEnumerable<HtmlNode> ListarDatas(HtmlNode node)
        {
            return node.Descendants("small").ToList();
        }

        public IEnumerable<object> JuntarCapitulosComDatas(IEnumerable<HtmlNode> capitulos, IEnumerable<HtmlNode> datas)
        {
            return capitulos.Zip(datas, (c, d) =>
                 new
                 {
                     capitulo = c.InnerText.Trim(),
                     dataLancamento = d.InnerText.Trim()
                 })
                .ToList();
        }
    }
}
