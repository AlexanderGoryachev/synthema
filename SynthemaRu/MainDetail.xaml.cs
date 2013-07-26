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
            string mainDetailPath = NavigationContext.QueryString["mainDetailPath"].ToString();
            DownloadMainDetail(mainDetailPath);
        }

        private void DownloadMainDetail(string Path)
        {
            WebClient newsDetails = new WebClient();
            newsDetails.Encoding = new Windows1251Encoding();
            newsDetails.DownloadStringAsync(new Uri(Path));
            newsDetails.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadMainDetailStringCompleted);
            TopPageProgressBar.IsIndeterminate = true;
        }

        private void DownloadMainDetailStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                return;

            ParseMainDetailHtml(e.Result);

            CommentsListBox.ItemsSource = AppData.Comments;

            TopPageProgressBar.IsIndeterminate = false;
        }

        private void ParseMainDetailHtml(string HtmlString)
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
                    var _text = node.SelectSingleNode(@"div[@class='comtext']/div").InnerText;
                    var _date = node.SelectSingleNode(@"div[@class='comtext']/text()").InnerText;

                    _text = HttpUtility.HtmlDecode(_text);
                    _date = _date.Replace("&nbsp;", "");

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

            HtmlNode FullDescriptionBaseNode = doc.DocumentNode.SelectSingleNode(@".//*[@id='dle-content']/div[1]/div[3]");

            var _details    = FullDescriptionBaseNode.SelectSingleNode("div/div[2]/b[1]").InnerHtml;
            var _link       = FullDescriptionBaseNode.SelectSingleNode("div//b/a").GetAttributeValue("href", "");
            var _link_text  = FullDescriptionBaseNode.SelectSingleNode("div//b/a").InnerText;
            var _promotext  = FullDescriptionBaseNode.SelectSingleNode("div/div[text()][1]").InnerHtml;
            var _tracklist  = FullDescriptionBaseNode.SelectSingleNode("div/div[b = 'Tracklist:']").InnerHtml;

            _details    = HttpUtility.HtmlDecode(_details);
            _promotext  = HttpUtility.HtmlDecode(_promotext);
            _tracklist  = HttpUtility.HtmlDecode(_tracklist);

            _details    = _details.Replace("<br>", "\n");
            _promotext  = _promotext.Replace("<br>", "\n");
            _tracklist  = _tracklist.Replace("<br>", "\n").Replace("<b>", string.Empty).Replace("</b>", string.Empty);

            DetailsTextBlock.Text =   _details;
            LinkTextBlock.Text =      _link_text;
            PromotextTextBlock.Text = _promotext;
            TracklistTextBlock.Text = _tracklist;
        }
    }
}