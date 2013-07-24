using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthemaRu
{
    class AppData
    {
        #region News

        public static string NewsString = string.Empty;

        public static List<NewsItem> NewsItems = new List<NewsItem>();

        public class NewsItem
        {
            public string Title { get; set; }
            public string Link { get; set; }
            public string ImgUrl { get; set; }
            public string ThumbUrl { get; set; }
            public string Details { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public string PubDate { get; set; }
        }

        #endregion

        #region NewsDetail

        public static List<Comment> Comments = new List<Comment>();

        public class Comment
        {
            public string Username { get; set; }
            public string Userpic { get; set; }
            public string Date { get; set; }
            public string Text { get; set; }
        }

        #endregion

        #region AppMethods

        public static string ReplaceHtmlTags(string HtmlString)
        {
            HtmlString.Replace("&quot;", "\"");
            HtmlString.Replace("&nbsp;", " ");
            HtmlString.Replace("<br>", "\n"); 
            HtmlString.Replace("<br/>", "\n"); 
            HtmlString.Replace("<br />", "\n");

            return HtmlString;
        }

        #endregion
    }
}
