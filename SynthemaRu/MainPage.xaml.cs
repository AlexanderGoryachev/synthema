﻿using HtmlAgilityPack;
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

        private bool IsMainAndNewsDownload = false;
        private bool IsReviewsDownload = false;

        private double a = 0;

        public MainPage()
        {
            InitializeComponent();

            //проверка jsonString
            //var jsonString = (Application.Current as App).LoadFileFromIsoStorage(Constants.MainItemsStorageFileName);

            MainListBox.ItemsSource = AppData.MainItems;
            NewsListBox.ItemsSource = AppData.NewsItems;
            NewsListBox2.ItemsSource = AppData.NewsItems;
            LinksListBox.ItemsSource = AppData.Links;

            // Player
            BackgroundAudioPlayer.Instance.PlayStateChanged += new EventHandler(Instance_PlayStateChanged);
            playBmp.UriSource = new Uri(@"Resources/play.png", UriKind.Relative);
            playPrsBmp.UriSource = new Uri(@"Resources/play_prs.png", UriKind.Relative);
            pauseBmp.UriSource = new Uri(@"Resources/pause.png", UriKind.Relative);
            pausePrsBmp.UriSource = new Uri(@"Resources/pause_prs.png", UriKind.Relative);

            //Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }
        
        #region Main
        
        void DownloadMainAndNews(string Path)
        {
            WebClient mainClient = new WebClient();
            mainClient.Encoding = new Windows1251Encoding();
            mainClient.DownloadStringAsync(new Uri(Path));
            mainClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadMainStringCompleted);
            TopPageProgressBar.IsIndeterminate = true;
        }

        void DownloadMainStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                return;

            // Это условие будет выполняться всегда, т.к. на странице находятся динамические рекламные ссылки. ИСПРАВИТЬ
            if (AppData.MainString.Equals(e.Result) == false)
            {
                AppData.MainString = e.Result;
                ParseMainHtml(e.Result);
                MainListBox.ItemsSource = null;
                MainListBox.ItemsSource = AppData.MainItems;

                ParseNewsHtml(e.Result);
                NewsListBox.ItemsSource = null;
                NewsListBox.ItemsSource = AppData.NewsItems;
            }

            TopPageProgressBar.IsIndeterminate = false;
        }

        private void ParseMainHtml(string HtmlString)
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

        private void MainListBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MainListBox.SelectedItem != null)
            {
                WebBrowserTask task = new WebBrowserTask();
                task.Uri = new Uri(AppData.MainItems.ElementAt(MainListBox.SelectedIndex).Link, UriKind.RelativeOrAbsolute);
                task.Show();
            }
        }

        private void MainImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string imgUrl = AppData.MainItems.ElementAt(MainListBox.SelectedIndex).ImgUrl;
            if (imgUrl != "")
            {
                NavigationService.Navigate(new Uri("/ImagePage.xaml?imgUrl=" + imgUrl, UriKind.Relative));
            }
        }

        private void MainStackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MainListBox.SelectedItem != null)
            {
                //WebBrowserTask task = new WebBrowserTask();
                //task.Uri = new Uri(AppData.MainItems.ElementAt(MainListBox.SelectedIndex).Link, UriKind.RelativeOrAbsolute);
                //task.Show();

                var mainDetailPath = AppData.MainItems.ElementAt(MainListBox.SelectedIndex).Link;
                NavigationService.Navigate(new Uri("/MainDetail.xaml?mainDetailPath=" + mainDetailPath, UriKind.Relative));
            }
        }
        
        #endregion

        #region News

        private void ParseNewsHtml(string HtmlString)
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

        private void NewsImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string imgUrl = AppData.NewsItems.ElementAt(NewsListBox.SelectedIndex).ImgUrl;
            if (imgUrl != "")
            {
                NavigationService.Navigate(new Uri("/ImagePage.xaml?imgUrl=" + imgUrl, UriKind.Relative));
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

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            while (NavigationService.BackStack.Any())
                NavigationService.RemoveBackEntry();
            base.OnBackKeyPress(e);
        }
        
        private void Pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // Main
            if (MainPivot.SelectedIndex == 0)
            {
                ApplicationBar.IsVisible = true;
                if (IsMainAndNewsDownload == false)
                {
                    DownloadMainAndNews(Constants.BaseUrl);
                    IsMainAndNewsDownload = true;
                }
                SynthemaAdverts.Visibility = Visibility.Collapsed;
            }
            // news
            else if (MainPivot.SelectedIndex == 1)
            {
                if (IsMainAndNewsDownload == false)
                {
                    DownloadMainAndNews(Constants.BaseUrl);
                    IsReviewsDownload = true;
                }
                ApplicationBar.IsVisible = true;
                SynthemaAdverts.Visibility = Visibility.Collapsed;
            }
            // Reviews
            else if (MainPivot.SelectedIndex == 2)
            {
                ApplicationBar.IsVisible = true;
                if (IsReviewsDownload == false)
                    DownloadReviewsRss(Constants.ReviewsRssPath);
                SynthemaAdverts.Visibility = Visibility.Collapsed;
            }

            // Search
            else if (MainPivot.SelectedIndex == 3)
            {
                ApplicationBar.IsVisible = true;
                SynthemaAdverts.Visibility = Visibility.Visible;
            }

            // Synth Radio
            else if (MainPivot.SelectedIndex == 4)
            {   
                ApplicationBar.IsVisible = false;
                SynthemaAdverts.Visibility = Visibility.Visible;

                if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
                    { playButton.Source = pauseBmp; }
                else
                    { playButton.Source = playBmp;}
            }

            // Links
            else if (MainPivot.SelectedIndex == 5)
            {
                ApplicationBar.IsVisible = false;
                SynthemaAdverts.Visibility = Visibility.Visible;
            }

            // About
            else if (MainPivot.SelectedIndex == 6)
            {
                ApplicationBar.IsVisible = false;
                SynthemaAdverts.Visibility = Visibility.Visible;
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
            if (MainPivot.SelectedIndex == 0) // обновить главную
            {
                DownloadMainAndNews(Constants.BaseUrl);
            }
            if (MainPivot.SelectedIndex == 1) // обновить новости
            {
                DownloadMainAndNews(Constants.BaseUrl);
            }
            else if (MainPivot.SelectedIndex == 2) // обновить рецензии
            {
                DownloadMainAndNews(Constants.ReviewsRssPath);
            }
            else if (MainPivot.SelectedIndex == 3) // обновить поиск
            {
                //
            }
            else if (MainPivot.SelectedIndex == 4) // обновить радио
            {
                //
            }
            else if (MainPivot.SelectedIndex == 5) // обновить ссылки
            {
                //
            }
            else if (MainPivot.SelectedIndex == 6) // обновить о программе
            {
                //
            }
        }

        #endregion


        private void OnPinchCompleted(object sender, PinchGestureEventArgs e)
        {
            if (a < (a * e.DistanceRatio))
            {
                // pinch
                NewsListBox.Visibility = Visibility.Collapsed;
                NewsListBox2.Visibility = Visibility.Visible;
            }
            else
            {
                // stretch
                NewsListBox.Visibility = Visibility.Visible;
                NewsListBox2.Visibility = Visibility.Collapsed;
            }
        }

        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            a = e.Distance;
        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            a = a * e.DistanceRatio;
        }

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