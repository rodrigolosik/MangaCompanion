using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Text;

namespace MangaCompanion.Services
{
    public class MangaService : IMangaService
    {
        const string url = "http://centraldemangas.online/titulos/9b0f585f_2f67_4995_86dd_f0903d5a452d";
        const string urlLista = "http://centraldemangas.online/titulos";
        const string defautlUrl = "http://centraldemangas.online";

        public async Task<object> ListarCapitulosManga()
        {
            HttpClient httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

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

        public async Task<object> ListarMangas()
        {
            HttpClient httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(urlLista);
            List<object> x = new List<object>();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var listaDeProximos = PegarElementoHtmlListaProximos(htmlDocument);
            var proximos = ListarProximos(listaDeProximos);
            var next = MontarListaDeProximos(proximos);
            var list = PegarElementoHtmlListaDosMangas(htmlDocument);
            var mangas = PegarDadosMangas(list);

            foreach (var item in next)
            {
                var htmlNovo = await httpClient.GetStringAsync(item);
                var htmlDocumentNovo = new HtmlDocument();
                htmlDocumentNovo.LoadHtml(htmlNovo);

                var j = PegarElementoHtmlListaDosMangas(htmlDocumentNovo);
                var dados = PegarDadosMangas(j);

                x.AddRange(MontarObjetoManga(dados));
            }

            return new { x };
        }

        private IEnumerable<object> MontarObjetoManga(IEnumerable<HtmlNode> htmlNode)
        {
            List<object> list = new List<object>();

            foreach (var d in htmlNode)
            {
                list.Add(new { d.Attributes[0].Value, texto = d.InnerHtml.Trim() });
            }
            return list;
        }

        private HtmlNode PegarElementoHtmlListaProximos(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("column center aligned")).FirstOrDefault();
        }

        private HtmlNode PegarElementoHtmlListaDosMangas(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("ui divided big relaxed list")).FirstOrDefault();
        }

        private IEnumerable<HtmlNode> PegarDadosMangas(HtmlNode node)
        {
            return node.Descendants("a").Where(node => node.Attributes.Contains("href") && node.FirstChild.Name != "img").ToList();
        }

        private IEnumerable<HtmlNode> ListarProximos(HtmlNode node)
        {
            return node.Descendants("a").ToList();
        }

        private IEnumerable<string> MontarListaDeProximos(IEnumerable<HtmlNode> nodes)
        {
            List<string> listUrlProximasPaginas = new List<string>();
            foreach (var item in nodes)
            {
                listUrlProximasPaginas.Add($"{defautlUrl}{item.GetAttributeValue("href", "")}");
            }
            return listUrlProximasPaginas;
        }

        private string PegarTitulo(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.Descendants("h1").FirstOrDefault().InnerText.Trim();
        }

        private string PegarUrlImagem(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.Descendants("img").Where(node => node.GetAttributeValue("src", "").Contains("capas")).FirstOrDefault().Attributes.FirstOrDefault().Value;
        }

        private IEnumerable<HtmlNode> ListarCapitulos(HtmlNode node)
        {
            return node.Descendants("a").ToList();
        }

        private IEnumerable<HtmlNode> ListarDatas(HtmlNode node)
        {
            return node.Descendants("small").ToList();
        }

        private IEnumerable<object> JuntarCapitulosComDatas(IEnumerable<HtmlNode> capitulos, IEnumerable<HtmlNode> datas)
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
