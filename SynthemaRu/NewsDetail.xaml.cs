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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string newsDetailPath = NavigationContext.QueryString["newsDetailPath"].ToString();
            DownloadNewsDetail(newsDetailPath);
        }

        private void DownloadNewsDetail(string Path)
        {
            WebClient newsDetails = new WebClient();
            newsDetails.Encoding = new Windows1251Encoding();
            newsDetails.DownloadStringAsync(new Uri(Path));
            newsDetails.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadNewsDetailStringCompleted);
            TopPageProgressBar.IsIndeterminate = true;
        }

        private void DownloadNewsDetailStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                return;

            ParseNewsDetailHtml(e.Result);

            CommentsListBox.ItemsSource = AppData.Comments;

            TopPageProgressBar.IsIndeterminate = false;
        }

        private void ParseNewsDetailHtml(string HtmlString)
        {
            AppData.Comments.Clear();

            HtmlDocument doc = new HtmlDocument();
            //HtmlNode.ElementsFlags.Remove("form");
            doc.LoadHtml(HtmlString);

            try
            {
                HtmlNodeCollection commentsNodes = doc.DocumentNode.SelectNodes(@".//*[@id='dle-content']/div/div[@class='theblock']");

                foreach (HtmlNode node in commentsNodes)
                {
                    var _username = node.SelectSingleNode(@"div[@class='comtext']/b").InnerText;
                    var _userpic = Constants.BaseUrl + node.SelectSingleNode(@"div[@class='avatar']/img").GetAttributeValue("src", "");
                    var _text = node.SelectSingleNode(@"div[@class='comtext']/div").InnerText.Replace("&quot;", "\"");
                    var _date = node.SelectSingleNode(@"div[@class='comtext']/text()").InnerText.Replace("&nbsp;", "");

                    //_text = AppData.ReplaceHtmlTags(_text);
                    //_date = AppData.ReplaceHtmlTags(_date);

                    AppData.Comments.Add(new AppData.Comment
                    {
                        Username = _username,
                        Userpic = _userpic,
                        Text = _text,
                        Date = _date
                    });
                }
            }
            catch
            {
                return;
            }            
        }
    }
}