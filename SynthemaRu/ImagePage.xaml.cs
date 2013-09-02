using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;

namespace SynthemaRu
{
    public partial class ImagePage : PhoneApplicationPage
    {
        string imgUrl = "";
        PageOrientation lastOrientation;

        public ImagePage()
        {
            InitializeComponent();

            this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(MainPage_OrientationChanged);
            lastOrientation = this.Orientation;

            if (!AppData.IsInternetAccess)
                MessageBox.Show("Подключение к Интернету отсутствует. Для работы приложения необходим доступ к сети");
        }

        void MainPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PageOrientation newOrientation = e.Orientation;

            // Orientations are (clockwise) 'PortraitUp', 'LandscapeRight', 'LandscapeLeft'

            RotateTransition transitionElement = new RotateTransition();

            switch (newOrientation)
            {
                case PageOrientation.Landscape:
                case PageOrientation.LandscapeRight:
                    // Come here from PortraitUp (i.e. clockwise) or LandscapeLeft?
                    if (lastOrientation == PageOrientation.PortraitUp)
                        transitionElement.Mode = RotateTransitionMode.In90Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In180Clockwise;
                    break;
                case PageOrientation.LandscapeLeft:
                    // Come here from LandscapeRight or PortraitUp?
                    if (lastOrientation == PageOrientation.LandscapeRight)
                        transitionElement.Mode = RotateTransitionMode.In180Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In90Clockwise;
                    break;
                case PageOrientation.Portrait:
                case PageOrientation.PortraitUp:
                    // Come here from LandscapeLeft or LandscapeRight?
                    if (lastOrientation == PageOrientation.LandscapeLeft)
                        transitionElement.Mode = RotateTransitionMode.In90Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In90Clockwise;
                    break;
                default:
                    break;
            }

            // Execute the transition
            PhoneApplicationPage phoneApplicationPage = (PhoneApplicationPage)(((PhoneApplicationFrame)Application.Current.RootVisual)).Content;
            ITransition transition = transitionElement.GetTransition(phoneApplicationPage);
            transition.Completed += delegate
            {
                transition.Stop();
            };
            transition.Begin();

            lastOrientation = newOrientation;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (NavigationService.CanGoBack)
            {
                e.Cancel = true;
                NavigationService.GoBack();
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (NavigationContext.QueryString.ContainsKey("imgUrl"))
            {
                imgUrl = NavigationContext.QueryString["imgUrl"].ToString();
                if (imgUrl != "")
                {
                    BitmapImage bm = new BitmapImage(new Uri(imgUrl, UriKind.Absolute));
                    BigImage.Source = bm;
                }
            }            
        }

    }
}