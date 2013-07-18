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

namespace SynthemaRu
{
    public partial class ImagePage : PhoneApplicationPage
    {
        string imgUrl = "";
        public ImagePage()
        {
            InitializeComponent();
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