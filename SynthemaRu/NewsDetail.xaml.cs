using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MSPToolkit.Encodings;
using HtmlAgilityPack;

namespace SynthemaRu
{
    public partial class NewsDetail : PhoneApplicationPage
    {
        public NewsDetail()
        {
            InitializeComponent();
        }

        private void DownloadNewsDetail(string Path)
        {
            WebClient news = new WebClient();
            news.Encoding = new Windows1251Encoding();
            news.DownloadStringAsync(new Uri(Path));
            news.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadNewsDetailStringCompleted);
            TopPageProgressBar.IsIndeterminate = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string newsDetailPath = NavigationContext.QueryString["newsDetailPath"].ToString();
            DownloadNewsDetail(newsDetailPath);
        }

        private void DownloadNewsDetailStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                return;

            ParseNewsDetailHtml(e.Result);
        }

        private void ParseNewsDetailHtml(string HtmlString)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlString);

            HtmlNodeCollection commentsNodes = doc.DocumentNode.SelectNodes(@".//*[@id='dlemasscomments']");

            foreach (HtmlNode node in commentsNodes)
            {
                var _username = node.SelectSingleNode(@"div/div[@class='theblock]/div[@class='comtext']/b").InnerText;
                var _userpic = Constants.BaseUrl + node.SelectSingleNode(@"div/div[@class='theblock]/div[@class='avatar']/img").GetAttributeValue("src", "");
                var _text = node.SelectSingleNode(@"div/div[@class='theblock]/div[@class='comtext']/div").InnerText;
                var _date = node.SelectSingleNode(@"div/div[@class='theblock]/div[@class='comtext']").InnerText;

                AppData.Comments.Add(new AppData.Comment
                {
                    Username = _username,
                    Userpic = _userpic,
                    Text = _text,
                    Date = _date
                });
            }
        }

    }
}