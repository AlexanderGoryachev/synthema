using HtmlAgilityPack;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using MSPToolkit.Encodings;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace SynthemaRu
{
    public partial class MainPage : PhoneApplicationPage
    {        
        // Constructor
        
        private List<ReviewsItem> reviewsItems = new List<ReviewsItem>();
        private XElement xmlReviewsOld = null;
        private MediaPlayerLauncher mediaPlayerLauncher = new MediaPlayerLauncher();

        private BitmapImage playBmp = new BitmapImage();
        private BitmapImage playPrsBmp = new BitmapImage();
        private BitmapImage pauseBmp = new BitmapImage();
        private BitmapImage pausePrsBmp = new BitmapImage();

        public MainPage()
        {
            InitializeComponent();

            BackgroundAudioPlayer.Instance.PlayStateChanged += new EventHandler(Instance_PlayStateChanged);

            playBmp.UriSource = new Uri(@"Resources/play.png", UriKind.Relative);
            playPrsBmp.UriSource = new Uri(@"Resources/play_prs.png", UriKind.Relative);
            pauseBmp.UriSource = new Uri(@"Resources/pause.png", UriKind.Relative);
            pausePrsBmp.UriSource = new Uri(@"Resources/pause_prs.png", UriKind.Relative);

            //Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }
        
        #region News
        
        void DownloadNewsRss(string Path)
        {
            WebClient news = new WebClient();
            news.Encoding = new Windows1251Encoding();
            news.DownloadStringAsync(new Uri(Path));
            news.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadNewsStringCompleted);
            TopPageProgressBar.IsIndeterminate = true;
        }

        void DownloadNewsStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                return;

            if (AppData.NewsString.Equals(e.Result) == false)
            {
                AppData.NewsString = e.Result;
                ParseNewsHtml(e.Result);
                NewsListBox.ItemsSource = AppData.NewsItems;
            }
            else
            {
                NewsListBox.ItemsSource = AppData.NewsItems;                
            }

            TopPageProgressBar.IsIndeterminate = false;
        }

        private void ParseNewsHtml(string HtmlString)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlString);

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(@".//*[@id='dle-content']/div[@class='theblock']");
            if (nodes == null)
                return;

            foreach (HtmlNode node in nodes)
            {
                var _title = node.SelectSingleNode(@"div[@class='tbh']/h2/a").InnerText;
                var _link = node.SelectSingleNode(@"div[@class='tbh']/h2/a").GetAttributeValue("href", "http://");

                var _thumbUrl = "";
                try { _thumbUrl = Constants.NewsUrl + node.SelectSingleNode(@"div[@class='news']/div/div[1]/a/img").GetAttributeValue("src", ""); }
                catch
                {
                    try { _thumbUrl = Constants.NewsUrl + node.SelectSingleNode(@"div[@class='news']/div/div[1]/img").GetAttributeValue("src", ""); }
                    catch { }
                }

                var _imgUrl = "";
                try { _imgUrl = node.SelectSingleNode(@"div[@class='news']/div/div[1]/a").GetAttributeValue("href", ""); }
                catch { }

                var _details = node.SelectSingleNode(@"div[@class='news']/div/div[2]").LastChild.InnerHtml.Replace("<br>", "\n");

                var _description = node.SelectSingleNode(@"div[@class='news']/div").LastChild.InnerText;
                var _pubDate = node.SelectSingleNode(@"div[@class='tbnfo']").InnerText.Replace("&nbsp;", "");

                AppData.NewsItems.Add(new AppData.NewsItem
                {
                    Title = _title,
                    Link = _link,
                    ImgUrl = _imgUrl,
                    ThumbUrl = _thumbUrl,
                    Details = _details,
                    Description = _description,
                    PubDate = _pubDate
                });
            }
        }

        private void NewsListBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (NewsListBox.SelectedItem != null)
            {
                WebBrowserTask task = new WebBrowserTask();
                task.Uri = new Uri(AppData.NewsItems.ElementAt(NewsListBox.SelectedIndex).Link, UriKind.RelativeOrAbsolute);
                task.Show();
            }
        }

        private void NewsImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string imgUrl = AppData.NewsItems.ElementAt(NewsListBox.SelectedIndex).ImgUrl;
            if (imgUrl != "")
            {
                NavigationService.Navigate(new Uri("/ImagePage.xaml?imgUrl=" + imgUrl, UriKind.Relative));
            }
        }

        private void NewsTitleStackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (NewsListBox.SelectedItem != null)
            {
                WebBrowserTask task = new WebBrowserTask();
                task.Uri = new Uri(AppData.NewsItems.ElementAt(NewsListBox.SelectedIndex).Link, UriKind.RelativeOrAbsolute);
                task.Show();
            }
        }
        
        #endregion

        #region Reviews

        public class ReviewsItem
        {
            public string Title { get; set; }
            public string Link { get; set; }
            public string ImgUrl { get; set; }
            public string ThumbUrl { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public DateTime PubDate { get; set; }

            public string Label { get; set; }
            public string Format { get; set; }
            public string Country { get; set; }
            public string Style { get; set; }
            public string Quality { get; set; }
            public string Size { get; set; }
        }

        void DownloadReviewsRss(string Path)
        {
            WebClient reviews = new WebClient();
            reviews.Encoding = new Windows1251Encoding();
            reviews.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadReviewsStringCompleted);
            reviews.DownloadStringAsync(new Uri (Path));
            TopPageProgressBar.IsIndeterminate = true;
        }

        void DownloadReviewsStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            TopPageProgressBar.IsIndeterminate = false;
            if (e.Error != null)
                return;
            XElement xmlReviews = XElement.Parse(e.Result);

            if (xmlReviewsOld == null | xmlReviewsOld != xmlReviews)
            {
                xmlReviewsOld = xmlReviews;

                var reviews_Items = from item in xmlReviews.Descendants("item")
                                select new ReviewsItem
                                {
                                    Title = item.Element("title").Value,
                                    Link = item.Element("link").Value,
                                    ImgUrl = Regex.Match(item.Element("description").Value, @"(?<=<a href=\"")(.*)(?=\"" onclick)").ToString(),        
                                    ThumbUrl = Regex.Match(item.Element("description").Value, @"(?<=<img src=\"")(.*)(?=\"" alt)").ToString(),
                                    PubDate = Convert.ToDateTime(item.Element("pubDate").Value),

                                    Label = Regex.Match(item.Element("description").Value, @"(?<=Label: )(.*?)(?=<)").ToString(),
                                    Format = Regex.Match(item.Element("description").Value, @"(?<=Format: )(.*?)(?=<)").ToString(),
                                    Country = Regex.Match(item.Element("description").Value, @"(?<=Country: )(.*?)(?=<)").ToString(),
                                    Style = Regex.Match(item.Element("description").Value, @"(?<=Style: )(.*?)(?=<)").ToString(),
                                    Quality = Regex.Match(item.Element("description").Value, @"(?<=Quality: )(.*?)(?=<)").ToString(),
                                    Size = Regex.Match(item.Element("description").Value, @"(?<=Size: )(.*?)(?=<)").ToString(),
                                };
                int n = reviews_Items.Count();
                //int n = 2;

                if (reviewsItems.Any())
                {
                    if (reviews_Items.ElementAt(0).Title != reviewsItems.ElementAt(0).Title)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            reviewsItems.Clear();
                            reviews_ListBox.Items.Clear();
                            reviewsItems.Add(reviews_Items.ElementAt(i));
                            reviews_ListBox.Items.Add(reviews_Items.ElementAt(i));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        reviewsItems.Add(reviews_Items.ElementAt(i));
                        reviews_ListBox.Items.Add(reviews_Items.ElementAt(i));
                    }
                }
            }     
        }

        private void reviewsImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string imgUrl = reviewsItems.ElementAt(reviews_ListBox.SelectedIndex).ImgUrl;
            if (imgUrl != "")
            {
                NavigationService.Navigate(new Uri("/ImagePage.xaml?imgUrl=" + imgUrl, UriKind.Relative));
            }
        }
        
        #endregion

        #region Search

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Что ищем?")
            {
                SearchBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "")
            {
                SearchBox.Text = "Что ищем?";
            }
        }

        #endregion  

        #region Radio player

        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Playing:
                    playButton.Source = pauseBmp;
                    TopPageProgressBar.IsIndeterminate = false;
                    break;

                case PlayState.Paused:
                case PlayState.Stopped:
                    playButton.Source = playBmp;
                    break;
            }

            if (null != BackgroundAudioPlayer.Instance.Track)
            {
            //    txtCurrentTrack.Text = BackgroundAudioPlayer.Instance.Track.Title + " by " +  BackgroundAudioPlayer.Instance.Track.Artist;
            }
        }
        
        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e) // НЕ РАБОТАЕТ /////////////////////////////////////////
        {
            playButton.Source = playBmp;
            TopPageProgressBar.IsIndeterminate = true;
            BackgroundAudioPlayer.Instance.SkipNext();
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e) // НЕ РАБОТАЕТ /////////////////////////////////////////
        {
            playButton.Source = playBmp;
            TopPageProgressBar.IsIndeterminate = true;
            BackgroundAudioPlayer.Instance.SkipPrevious();
        }

        private void playButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (playButton.Source == pauseBmp)
            {
                playButton.Source = pausePrsBmp;
            }
            else
            {
                playButton.Source = playPrsBmp;
            }
        }

        private void playButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (playButton.Source == playPrsBmp)
            {
                TopPageProgressBar.IsIndeterminate = true;
                playButton.Source = pauseBmp;
                BackgroundAudioPlayer.Instance.Play();
            }
            else
            {
                playButton.Source = playBmp;
                BackgroundAudioPlayer.Instance.Pause();
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
                WebBrowserTask wbt = new WebBrowserTask();
                wbt.Uri = new Uri("http://synth-radio.ru", UriKind.RelativeOrAbsolute);
                wbt.Show();
        }

#endregion

        #region Page naviation

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // News
            if (MainPivot.SelectedIndex == 0)
            {
                ApplicationBar.IsVisible = true;
                DownloadNewsRss(Constants.NewsUrl);
                Adverts.Visibility = Visibility.Collapsed;
            }

            // Reviews
            else if (MainPivot.SelectedIndex == 1)
            {
                ApplicationBar.IsVisible = true;
                DownloadReviewsRss(Constants.ReviewsRssPath);
                Adverts.Visibility = Visibility.Collapsed;
            }

            // Search
            else if (MainPivot.SelectedIndex == 2) 
            {
                ApplicationBar.IsVisible = true;
                Adverts.Visibility = Visibility.Visible;
            }

            // Synth Radio
            else if (MainPivot.SelectedIndex == 3)
            {
                ApplicationBar.IsVisible = false;
                Adverts.Visibility = Visibility.Visible;

                if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
                    { playButton.Source = pauseBmp; }
                else
                    { playButton.Source = playBmp;}
            }

            // Links
            else if (MainPivot.SelectedIndex == 4)
            {
                ApplicationBar.IsVisible = false;
                Adverts.Visibility = Visibility.Visible;
            }

            // About
            else if (MainPivot.SelectedIndex == 5)
            {
                ApplicationBar.IsVisible = false;
                Adverts.Visibility = Visibility.Visible;
            }
        }

        #endregion
        
        #region App tiles

        //void ChangeBackFlipTileData(string backTitle, string backContent, string wideBackContent, string wideBackBackgroundImage, string backBackgroundImage) 
        //{
        //    ShellTile apptile = ShellTile.ActiveTiles.First();
        //    FlipTileData appFlipTileData = new FlipTileData();

        //    appFlipTileData.BackTitle = backTitle;

        //    appFlipTileData.BackContent = backContent;
        //    appFlipTileData.BackBackgroundImage = new Uri(backBackgroundImage, UriKind.RelativeOrAbsolute);

        //    appFlipTileData.WideBackContent = wideBackContent;
        //    appFlipTileData.WideBackBackgroundImage = new Uri(wideBackBackgroundImage, UriKind.RelativeOrAbsolute);

        //    apptile.Update(appFlipTileData);
        //}

        #endregion

        #region AppBar

        private void RefreshAppButton_Click_1(object sender, EventArgs e)
        {
            if (MainPivot.SelectedIndex == 0) // обновить новости
            {
               // DownloadNewsRss(newsUrl);
            }
            else if (MainPivot.SelectedIndex == 1) // обновить рецензии
            {
               // DownloadReviewsRss(reviewsRssPath);
            }
            else if (MainPivot.SelectedIndex == 2) // обновить поиск
            {
                //
            }
            else if (MainPivot.SelectedIndex == 3) // обновить радио
            {
                //
            }
            else if (MainPivot.SelectedIndex == 4) // обновить ссылки
            {
                //
            }
            else if (MainPivot.SelectedIndex == 5) // обновить о программе
            {
                //
            }
        }

        #endregion

        // Sample code for building a localized ApplicationBar

        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}