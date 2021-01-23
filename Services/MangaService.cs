using HtmlAgilityPack;
using MangaCompanion.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MangaCompanion.Services
{
    public class MangaService : IMangaService
    {
        const string url = "http://centraldemangas.online/titulos";
        const string defautlUrl = "http://centraldemangas.online";

        private readonly ILogger<MangaService> _logger;

        public MangaService(ILogger<MangaService> logger)
        {
            _logger = logger;
        }

        public async Task<Manga> ListarCapitulosManga(Manga manga)
        {
            HttpClient httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync($"{defautlUrl}{manga.Url}");

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            // No central de mangás os itens estão dentro de uma tabela;
            var tabela = htmlDocument.DocumentNode.Descendants("table").FirstOrDefault();

            var listaCapitulos = ListarCapitulos(tabela);
            var listaDatas = ListarDatas(tabela);
            var capitulos = JuntarCapitulosComDatas(listaCapitulos, listaDatas);
            var titulo = PegarTitulo(htmlDocument);
            var capa = PegarUrlImagem(htmlDocument);

            manga.Capa = capa;
            manga.Capitulos = capitulos;

            return manga;
        }

        public async Task<IEnumerable<Manga>> ListarMangas()
        {
            var listaContendoTodosMangas = new List<Manga>();

            using HttpClient httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var pageList = PegarListaProximos(htmlDocument);

            foreach (var page in pageList)
            {
                html = await httpClient.GetStringAsync(page);
                htmlDocument.LoadHtml(html);
                var dados = PegarElementoHtmlListaDosMangas(htmlDocument);
                listaContendoTodosMangas.AddRange(dados);
            }

            foreach (var manga in listaContendoTodosMangas)
            {
                try
                {
                    await ListarCapitulosManga(manga);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            return listaContendoTodosMangas;
        }

        private IEnumerable<string> PegarListaProximos(HtmlDocument htmlDocument)
        {
            var elementoDiv = htmlDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("column center aligned")).FirstOrDefault();

            var nodes = elementoDiv.Descendants("a").ToList();

            List<string> listUrlProximasPaginas = new List<string>();
            foreach (var item in nodes)
            {
                listUrlProximasPaginas.Add($"{defautlUrl}{item.GetAttributeValue("href", "")}");
            }
            return listUrlProximasPaginas;
        }

        private IEnumerable<Manga> PegarElementoHtmlListaDosMangas(HtmlDocument htmlDocument)
        {
            List<Manga> list = new List<Manga>();

            var node = htmlDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("ui divided big relaxed list")).FirstOrDefault();

            var itens = node.Descendants("a").Where(node => node.Attributes.Contains("href") && node.FirstChild.Name != "img").ToList();

            foreach (var d in itens)
            {
                list.Add(new Manga { Titulo = d.InnerHtml.Trim(), Url = d.Attributes[0].Value });
            }
            return list;
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
        /// <summary>
        /// As datas de lançamento estão dentro de tags <small>
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
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
