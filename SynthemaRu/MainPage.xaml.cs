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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Linq;
using SynthemaRu.Common;
using Microsoft.Phone.Net.NetworkInformation;

namespace SynthemaRu
{
    public partial class MainPage : PhoneApplicationPage
    {        
        // Constructor

        private MediaPlayerLauncher mediaPlayerLauncher = new MediaPlayerLauncher();
        private BitmapImage playBmp = new BitmapImage();
        private BitmapImage playPrsBmp = new BitmapImage();
        private BitmapImage pauseBmp = new BitmapImage();
        private BitmapImage pausePrsBmp = new BitmapImage();

        public MainPage()
        {
            InitializeComponent();

            MainListBox.ItemsSource = AppData.MainItems;
            NewsListBox.ItemsSource = AppData.NewsItems;
            DownloadingService.webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(MainStringDownloadCompleted);

            if (!AppData.IsInternetAccess)
                MessageBox.Show("Подключение к Интернету отсутствует. Для работы приложения необходим доступ к сети");

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

        public void MainStringDownloadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                return;

            AppData.MainString = e.Result;

            // Функции парсинга выполняются 5,5 секунд. Необходимо оптимизировать.
            ParsingService.ParseMainHtml(e.Result);
            ParsingService.ParseNewsHtml(e.Result);
            LoadingBar.IsIndeterminate = false;
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

        private void NewsImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string imgUrl = AppData.NewsItems.ElementAt(NewsListBox.SelectedIndex).ImgUrl;
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
                    LoadingBar.IsIndeterminate = false;
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
            LoadingBar.IsIndeterminate = true;
            BackgroundAudioPlayer.Instance.SkipNext();
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e) // НЕ РАБОТАЕТ /////////////////////////////////////////
        {
            playButton.Source = playBmp;
            LoadingBar.IsIndeterminate = true;
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
                LoadingBar.IsIndeterminate = true;
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
                SynthemaAdverts.Visibility = Visibility.Collapsed;
            }
            // news
            else if (MainPivot.SelectedIndex == 1)
            {
                ApplicationBar.IsVisible = true;
                SynthemaAdverts.Visibility = Visibility.Collapsed;
            }
            // Reviews
            else if (MainPivot.SelectedIndex == 2)
            {
                ApplicationBar.IsVisible = true;
                //if (IsReviewsDownload == false)
                //    DownloadReviewsRss(Constants.ReviewsRssPath);
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
            if (MainPivot.SelectedIndex == 0 || MainPivot.SelectedIndex == 1) // обновить главную и новости
            {
                LoadingBar.IsIndeterminate = true;
                DownloadingService.DownloadMainAndNews(Constants.BaseUrl);
                LoadingBar.IsIndeterminate = false;
            }
            else if (MainPivot.SelectedIndex == 2) // обновить рецензии
            {
                //
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