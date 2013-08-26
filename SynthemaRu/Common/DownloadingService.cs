using MSPToolkit.Encodings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SynthemaRu.Common
{
    class DownloadingService
    {
        public static void DownloadMainAndNews(string Path)
        {
            WebClient mainClient = new WebClient();
            mainClient.Encoding = new Windows1251Encoding();
            mainClient.DownloadStringAsync(new Uri(Path));
            mainClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadMainStringCompleted);
        }

        public static void DownloadMainStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                return;

            AppData.MainString = e.Result;
            ParsingService.ParseMainHtml(e.Result);
            ParsingService.ParseNewsHtml(e.Result);
        }
    }
}
