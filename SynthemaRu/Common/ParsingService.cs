using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SynthemaRu.Common
{
    class ParsingService
    {
        public static void ParseMainHtml(string HtmlString)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlString);

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(@".//*[@id='dle-content']/div[@class='theblock']");
            if (nodes == null)
                return;

            AppData.MainItems.Clear();

            foreach (HtmlNode node in nodes)
            {
                var title = node.SelectSingleNode(@"div[@class='tbh']/h2/a").InnerText;
                var link = node.SelectSingleNode(@"div[@class='tbh']/h2/a").GetAttributeValue("href", "http://");
                var thumbUrl = Constants.BaseUrl + node.SelectSingleNode(@"div[@class='news']/div/div[1]//img").GetAttributeValue("src", "");
                var description = node.SelectSingleNode(@"div[@class='news']/div/div/text()").InnerText;
                var pubDate = node.SelectSingleNode(@"div[@class='tbnfo']").InnerText;
                var imgUrl = string.Empty;
                try { imgUrl = node.SelectSingleNode(@"div[@class='news']/div/div[1]/a").GetAttributeValue("href", ""); }
                catch { }

                HtmlNodeCollection Details = node.SelectNodes(@"div[@class='news']/div/div/b[
                                                                    contains(text(),'Label') or
                                                                    contains(text(),'Format') or
                                                                    contains(text(),'Style') or
                                                                    contains(text(),'Country') or
                                                                    contains(text(),'Quality') or
                                                                    contains(text(),'Size')
                                                                ]/text()");

                var details = string.Empty;
                var i = 0;
                foreach (HtmlNode detailNode in Details)
                {
                    var detail = HttpUtility.HtmlDecode(detailNode.InnerHtml);
                    if (i < 1) details = detail;
                    else details = details + "\n" + detail;
                    i++;
                }

                title = HttpUtility.HtmlDecode(title);
                description = HttpUtility.HtmlDecode(description);

                pubDate = pubDate.Replace("&nbsp;", "");

                AppData.MainItems.Add(new AppData.MainItem
                {
                    Title = title,
                    Link = link,
                    ImgUrl = imgUrl,
                    ThumbUrl = thumbUrl,
                    Details = details,
                    Description = description,
                    PubDate = pubDate
                });
            }
        }

        public static void ParseNewsHtml(string HtmlString)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlString);

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(@".//div[@class='col2-3'][1]/div[@class='sceneCol23c']/div[@class='sceneNews']");
            if (nodes == null)
                return;

            AppData.NewsItems.Clear();

            foreach (HtmlNode node in nodes)
            {
                var _title = node.SelectSingleNode(@"div[@class='title']/h2/a").InnerText;
                var _link = node.SelectSingleNode(@"div[@class='title']/h2/a").GetAttributeValue("href", "http://");

                var _thumbUrl = node.SelectSingleNode(@"div[@class='newsm']//img").GetAttributeValue("src", "");

                var _imgUrl = node.SelectSingleNode(@"div[@class='newsm']//a").GetAttributeValue("href", "");

                var _description = string.Empty;
                try { _description = node.SelectSingleNode(@"div[@class='newsm']/text()").InnerText; }
                catch { }

                _description = HttpUtility.HtmlDecode(_description);

                // костыль, помогающий обойти косяк в CMS, лишний раз приписывающий BaseUrl к относительному пути до картинки
                // т.е. иногда Url выглядит так: http://www.synthema.ruhttp://www.synthema.ru/uploads/posts/2013-06/thumbs/1371912912_war.jpg
                _thumbUrl = _thumbUrl.Replace(Constants.BaseUrl, string.Empty);
                _thumbUrl = Constants.BaseUrl + _thumbUrl;

                AppData.NewsItems.Add(new AppData.NewsItem
                {
                    Title = _title,
                    Link = _link,
                    ImgUrl = _imgUrl,
                    ThumbUrl = _thumbUrl,
                    Description = _description,
                });
            }
        }


    }
}
