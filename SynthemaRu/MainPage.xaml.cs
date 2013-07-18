using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using MSPToolkit.Encodings;
using System;
using System.Collections.Generic;
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
        private Uri news_RssPath = new Uri("http://www.synthema.ru/rss.xml");
        private Uri reviews_RssPath = new Uri("http://www.synthema.ru/reviews/rss.xml");
        private List<NewsItem> news_ItemsList = new List<NewsItem>();
        private List<ReviewsItem> reviews_ItemsList = new List<ReviewsItem>();
        private XElement xmlNewsOld = null;
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

            DownloadNewsRss(news_RssPath);

            playBmp.UriSource = new Uri(@"Resources/play.png", UriKind.Relative);
            playPrsBmp.UriSource = new Uri(@"Resources/play_prs.png", UriKind.Relative);
            pauseBmp.UriSource = new Uri(@"Resources/pause.png", UriKind.Relative);
            pausePrsBmp.UriSource = new Uri(@"Resources/pause_prs.png", UriKind.Relative);

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        void ChangeBackFlipTileData(string backTitle, string backContent, string wideBackContent, string wideBackBackgroundImage, string backBackgroundImage) 
        {
            ShellTile apptile = ShellTile.ActiveTiles.First();
            FlipTileData appFlipTileData = new FlipTileData();

            appFlipTileData.BackTitle = backTitle;

            appFlipTileData.BackContent = backContent;
            appFlipTileData.BackBackgroundImage = new Uri(backBackgroundImage, UriKind.RelativeOrAbsolute);

            appFlipTileData.WideBackContent = wideBackContent;
            appFlipTileData.WideBackBackgroundImage = new Uri(wideBackBackgroundImage, UriKind.RelativeOrAbsolute);

            apptile.Update(appFlipTileData);
        }

        void DownloadNewsRss(Uri rssPath)
        {
            WebClient news = new WebClient();
            news.Encoding = new Windows1251Encoding();
            news.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadNewsStringCompleted);
            news.DownloadStringAsync(rssPath);
            TopPageProgressBar.IsIndeterminate = true;
        }

        void DownloadNewsStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            TopPageProgressBar.IsIndeterminate = false;
            if (e.Error != null)
                return;

            XElement xmlNews = XElement.Parse(e.Result);

            if (xmlNewsOld == null | xmlNewsOld != xmlNews)
            {
                xmlNewsOld = xmlNews;
                var news_Items = from item in xmlNews.Descendants("item")
                                select new NewsItem
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
                int n = news_Items.Count();
                //int n = 2;

                if (news_ItemsList.Any())
                {
                    if (news_Items.ElementAt(0).Title != news_ItemsList.ElementAt(0).Title)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            news_ItemsList.Clear();
                            news_ListBox.Items.Clear();
                            news_ItemsList.Add(news_Items.ElementAt(i));
                            news_ListBox.Items.Add(news_Items.ElementAt(i)); 
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        news_ItemsList.Add(news_Items.ElementAt(i));
                        news_ListBox.Items.Add(news_Items.ElementAt(i));
                    }
                }

                ChangeBackFlipTileData("Synthema.ru", news_ItemsList[0].Title, news_ItemsList[0].Title, news_ItemsList[0].ImgUrl, news_ItemsList[0].ThumbUrl);
            }     
        }

        public class NewsItem
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

        void DownloadReviewsRss(Uri rssPath)
        {
            WebClient reviews = new WebClient();
            reviews.Encoding = new Windows1251Encoding();
            reviews.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadReviewsStringCompleted);
            reviews.DownloadStringAsync(rssPath);
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

                if (reviews_ItemsList.Any())
                {
                    if (reviews_Items.ElementAt(0).Title != reviews_ItemsList.ElementAt(0).Title)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            reviews_ItemsList.Clear();
                            reviews_ListBox.Items.Clear();
                            reviews_ItemsList.Add(reviews_Items.ElementAt(i));
                            reviews_ListBox.Items.Add(reviews_Items.ElementAt(i));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        reviews_ItemsList.Add(reviews_Items.ElementAt(i));
                        reviews_ListBox.Items.Add(reviews_Items.ElementAt(i));
                    }
                }
            }     
        }

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

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void Links_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void news_ListBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (news_ListBox.SelectedItem != null)
            {
                WebBrowserTask wbt = new WebBrowserTask();
                wbt.Uri = new Uri(news_ItemsList.ElementAt(news_ListBox.SelectedIndex).Link, UriKind.RelativeOrAbsolute);
                wbt.Show();
            } 
        }

        private void newsImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string imgUrl = news_ItemsList.ElementAt(news_ListBox.SelectedIndex).ImgUrl;
            if (imgUrl != "")
            {
                NavigationService.Navigate(new Uri("/ImagePage.xaml?imgUrl=" + imgUrl, UriKind.Relative));
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ImagePage.xaml", UriKind.Relative));
        }

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

        private void Pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 0) // новости
            {
                ApplicationBar.IsVisible = true;
                DownloadNewsRss(news_RssPath);
                Adverts.Visibility = Visibility.Collapsed;
            }
            else if (MainPivot.SelectedIndex == 1) // рецензии
            {
                ApplicationBar.IsVisible = true;
                DownloadReviewsRss(reviews_RssPath);
                Adverts.Visibility = Visibility.Collapsed;
            }
            else if (MainPivot.SelectedIndex == 2) // поиск
            {
                ApplicationBar.IsVisible = true;
                Adverts.Visibility = Visibility.Visible;
            }
            else if (MainPivot.SelectedIndex == 3) // радио
            {
                ApplicationBar.IsVisible = false;
                Adverts.Visibility = Visibility.Visible;
                
                if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
                {
                    playButton.Source = pauseBmp;
                }
                else
                {
                    playButton.Source = playBmp;
                }
            }
            else if (MainPivot.SelectedIndex == 4) // ссылки
            {
                ApplicationBar.IsVisible = false;
                Adverts.Visibility = Visibility.Visible;
            }
            else if (MainPivot.SelectedIndex == 5) // о программе
            {
                ApplicationBar.IsVisible = false;
                Adverts.Visibility = Visibility.Visible;
            }
        }

        private void news_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (news_ListBox.SelectedIndex != null)
            { 
            }
        }

        private void news_ListBox_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            var sView = e.ManipulationContainer as ScrollViewer;
            if (sView.VerticalOffset >= sView.ExtentHeight - sView.ViewportHeight)
            {
                // MessageBox.Show("Manipulation Completed");
            }
        }

        private void news_ListBox_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            // MessageBox.Show("Manipulation Delta");
        }

        private void RefreshAppButton_Click_1(object sender, EventArgs e)
        {
            if (MainPivot.SelectedIndex == 0) // обновить новости
            {
                DownloadNewsRss(news_RssPath);
            }
            else if (MainPivot.SelectedIndex == 1) // обновить рецензии
            {
                DownloadReviewsRss(reviews_RssPath);
            }
            else if (MainPivot.SelectedIndex == 2) // обновить поиск
            {
            }
            else if (MainPivot.SelectedIndex == 3) // обновить радио
            {
            }
            else if (MainPivot.SelectedIndex == 4) // обновить ссылки
            {
            }
            else if (MainPivot.SelectedIndex == 5) // обновить о программе
            {
            }
        }

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
        
        private void ToggleSwitch_Checked_1(object sender, RoutedEventArgs e) // НЕ РАБОТАЕТ /////////////////////////////////////////
        {
            playButton.Source = playBmp;
            TopPageProgressBar.IsIndeterminate = true;
            BackgroundAudioPlayer.Instance.SkipNext();
        }

        private void ToggleSwitch_Unchecked_1(object sender, RoutedEventArgs e) // НЕ РАБОТАЕТ /////////////////////////////////////////
        {
            playButton.Source = playBmp;
            TopPageProgressBar.IsIndeterminate = true;
            BackgroundAudioPlayer.Instance.SkipPrevious();
        }

        private void reviewsImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string imgUrl = reviews_ItemsList.ElementAt(reviews_ListBox.SelectedIndex).ImgUrl;
            if (imgUrl != "")
            {
                NavigationService.Navigate(new Uri("/ImagePage.xaml?imgUrl=" + imgUrl, UriKind.Relative));
            }
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

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
                WebBrowserTask wbt = new WebBrowserTask();
                wbt.Uri = new Uri("http://synth-radio.ru", UriKind.RelativeOrAbsolute);
                wbt.Show();
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