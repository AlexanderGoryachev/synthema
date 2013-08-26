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
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SynthemaRu
{
    public partial class NewsDetail : PhoneApplicationPage
    {
        private string _mainDetailPath = string.Empty;

        public NewsDetail()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _mainDetailPath = NavigationContext.QueryString["mainDetailPath"].ToString();
            DownloadMainDetail(_mainDetailPath);
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
            SimilarListBox.ItemsSource = AppData.SimilarLinks;

            TopPageProgressBar.IsIndeterminate = false;
        }

        private void ParseMainDetailHtml(string HtmlString)
        {
            AppData.Comments.Clear();

            HtmlDocument doc = new HtmlDocument();
            //HtmlNode.ElementsFlags.Remove("form");
            doc.LoadHtml(HtmlString);

            #region Information

            var Title = doc.DocumentNode.SelectSingleNode(@".//*[@id='dle-content']/div[@class='theblock']/div[@class='tbh']/h1").InnerText;
            GroupTitleTextBlock.Text = Title.Substring(0, Title.IndexOf("- "));
            AlbumTitleTextBlock.Text = Title.Remove(0, Title.IndexOf("- ") + 2);

            HtmlNode informationBaseNode = doc.DocumentNode.SelectSingleNode(@".//*[@id='dle-content']/div[1]/div[3]");

            try 
            {
                HtmlNode coverImageBaseNode = doc.DocumentNode.SelectSingleNode(@".//*[@id='dle-content']/div[@class='theblock']/div[@class='newf']/div/div[1]/a");
                var coverUri = new Uri(Constants.BaseUrl + coverImageBaseNode.SelectSingleNode("img").GetAttributeValue("src", ""), UriKind.Absolute);
                CoverImage.Source = new BitmapImage(coverUri);
                BackgroundCoverImage.Visibility = Visibility.Visible;
            }
            catch (NullReferenceException)
            { 
                CoverImage.Visibility = Visibility.Collapsed;
            }

            try
            {
                HtmlNodeCollection Details = informationBaseNode.SelectNodes(@"//div/b[
                                                                                    contains(text(),'Label') or
                                                                                    contains(text(),'Format') or
                                                                                    contains(text(),'Style') or
                                                                                    contains(text(),'Country') or
                                                                                    contains(text(),'Quality') or
                                                                                    contains(text(),'Size')
                                                                                ]/text()");
                var details = new Dictionary<string, string>();
                foreach (HtmlNode detailNode in Details)
                {
                    var detail = HttpUtility.HtmlDecode(detailNode.InnerHtml);

                    if (detail.Contains("Label"))
                    {
                        detail = detail.Remove(0, 7);
                        details.Add("Лейбл", detail);
                    }
                    else if (detail.Contains("Format"))
                    {
                        detail = detail.Remove(0, 8);
                        details.Add("Формат", detail);
                    }
                    else if (detail.Contains("Style"))
                    {
                        detail = detail.Remove(0, 7);
                        details.Add("Стиль", detail);
                    }
                    else if (detail.Contains("Country"))
                    {
                        detail = detail.Remove(0, 9);
                        details.Add("Страна", detail);
                    }
                    else if (detail.Contains("Quality"))
                    {
                        detail = detail.Remove(0, 9);
                        details.Add("Качество", detail);
                    }
                    else if (detail.Contains("Size"))
                    {
                        detail = detail.Remove(0, 6);
                        details.Add("Размер", detail);
                    }
                }
                DetailsListBox.ItemsSource = details;
            }
            catch (NullReferenceException)
            {
                DetailsListBox.Visibility = Visibility.Collapsed;
            }

            try
            {
                HtmlNodeCollection Links = informationBaseNode.SelectNodes("div//b/a");

                var linksList = new Dictionary<string, string>();

                foreach (HtmlNode node in Links)
                {
                    linksList.Add(node.GetAttributeValue("href", ""), node.InnerText);
                }

                LinksListBox.ItemsSource = linksList;

                LinksListBox.Visibility = Visibility.Visible;
            }
            catch (NullReferenceException)
            {
                LinksListBox.Visibility = Visibility.Collapsed;
            }

            try
            {
                var promotext = (informationBaseNode.SelectSingleNode("div/div[text()][1]")).InnerText;
                promotext = HttpUtility.HtmlDecode(promotext);
                promotext = promotext.Replace("<br>", "\n");
                PromotextTextBlock.Text = promotext;
            }
            catch (NullReferenceException)
            {
                PromotextTextBlock.Visibility = Visibility.Collapsed;
            }

            #endregion

            #region Tracklist

            try
            {
                //var tracklist = informationBaseNode.SelectSingleNode("div/div[b[contains(text(),'Tracklist')]]").InnerHtml;
                //tracklist = HttpUtility.HtmlDecode(tracklist);
                //tracklist = tracklist.Remove(0, tracklist.IndexOf("<br><br>") + 8);
                //tracklist = tracklist.Replace("<br>", "\n").Replace("<b>", string.Empty).Replace("</b>", string.Empty);
                //TracklistTextBlock.Text = tracklist;

                HtmlNodeCollection tracklist = informationBaseNode.SelectNodes("div/div[b[contains(text(),'Tracklist')]]/text()");
                var tracks = new Dictionary<int, string>();
                var track = string.Empty;
                var i = 0;
                foreach (HtmlNode node in tracklist)
                {
                    track = node.InnerText;
                    track = HttpUtility.HtmlDecode(track);
                    track = track.Remove(0, track.IndexOf(" ") + 1);
                    tracks.Add(i + 1, track);
                    i++;
                }
                TracklistListBox.ItemsSource = tracks;

                //tracklist = HttpUtility.HtmlDecode(tracklist);
                //tracklist = tracklist.Remove(0, tracklist.IndexOf("<br><br>") + 8);
                //tracklist = tracklist.Replace("<br>", "\n").Replace("<b>", string.Empty).Replace("</b>", string.Empty);
                //TracklistTextBlock.Text = tracklist;
            }
            catch (NullReferenceException)
            {

            }

            #endregion

            #region Comments

            try
            {
                HtmlNodeCollection CommentsNodes = doc.DocumentNode.SelectNodes(@".//*[@id='dle-content']/div/div[@class='theblock']");

                foreach (HtmlNode node in CommentsNodes)
                {
                    var username = node.SelectSingleNode(@"div[@class='comtext']/b").InnerText;
                    var userpic = Constants.BaseUrl + node.SelectSingleNode(@"div[@class='avatar']/img").GetAttributeValue("src", "");
                    var text = node.SelectSingleNode(@"div[@class='comtext']/div").InnerText;
                    var date = node.SelectSingleNode(@"div[@class='comtext']/text()").InnerText;

                    text = text.Replace("<br>", "\n").Replace("<b>", string.Empty).Replace("</b>", string.Empty);
                    text = HttpUtility.HtmlDecode(text);
                    date = date.Replace("&nbsp;", "");

                    AppData.Comments.Add(new AppData.Comment
                    {
                        Username = username,
                        Userpic = userpic,
                        Text = text,
                        Date = date
                    });
                }
            }
            catch (NullReferenceException)
            {
                NoCommentsTextBlock.Visibility = Visibility.Visible;
            }

            #endregion

            #region Similar links

            try
            {
                HtmlNode similarLinksBaseNode = doc.DocumentNode.SelectSingleNode(@".//div[@id='dle-content']/div[@class='theblock']");
                HtmlNodeCollection similarLinks = similarLinksBaseNode.SelectNodes(@"div[@class='stext']/ul/li/a");

                foreach (HtmlNode node in similarLinks)
                {
                    var title = node.InnerText;
                    title = HttpUtility.HtmlDecode(title);
                    AppData.SimilarLinks.Add(new AppData.SimilarLink
                    {
                        GroupTitle = title.Substring(0, title.IndexOf("- ")),
                        AlbumTitle = title.Remove(0, title.IndexOf("- ") + 2),
                        Url = "/MainDetail.xaml?mainDetailPath=" + node.GetAttributeValue("href", "")
                    });
                }
            }
            catch (NullReferenceException)
            {
                NoSimilarTextBlock.Visibility = Visibility.Visible;
            }

            #endregion

        }

        private void OpenInBrowser_Click(object sender, EventArgs e)
        {
            WebBrowserTask openInBrowser = new WebBrowserTask();
            openInBrowser.Uri = new Uri(_mainDetailPath, UriKind.Absolute);
            openInBrowser.Show();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}