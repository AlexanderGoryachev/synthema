using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthemaRu.Common
{
    class StringHelper
    {
        public static string ReplaceSubstring(string inputString, string oldSubstring, string newSubstring)
        {
            var tempString = inputString;
            var i = 0;
            while (i != -1)
            {
                var startIndex = tempString.IndexOf(oldSubstring);
                var endIndex = startIndex + oldSubstring.Length;
                if (startIndex != -1 & endIndex != -1)
                {
                    tempString = tempString.Remove(startIndex, (endIndex - startIndex));
                    tempString = tempString.Insert(startIndex, newSubstring);
                }
                i = startIndex;
            }
            return tempString;
        }

        public static string FormatSmiles(string inputString, Dictionary<string, string> smilesDictionary)
        {
            var tempString = inputString;

            var j = 0;
            while (j != -1)
            {
                var startIndex = tempString.IndexOf("<!--smile:");
                var endIndex = tempString.IndexOf("--><!--/smile-->") + 16;
                if (startIndex != -1 & endIndex != -1)
                {
                    var smileName = tempString.Substring(startIndex + 10, (endIndex - startIndex) - 26);
                    tempString = tempString.Remove(startIndex, (endIndex - startIndex));
                    if (smilesDictionary.Keys.Contains(smileName))
                        tempString = tempString.Insert(startIndex, smilesDictionary[smileName]);
                    else
                        tempString = tempString.Insert(startIndex, "*" + smileName + "*");
                }
                j = startIndex;
            }

            return tempString;
        }

        public static List<CommentText> FormatQuotes(string inputString)
        {
            var tempString = inputString;
            var commentText = new List<CommentText>();

            var j = 0;
            while (j != -1)
            {
                var startIndexQuote = tempString.IndexOf("<!--QuoteBegin");

                if (startIndexQuote == -1)
                {
                    commentText.Add(new CommentText { ComText = tempString });
                    j = -1;
                }
                else
                {
                    var text = tempString.Substring(0, startIndexQuote);
                    tempString = tempString.Remove(0, startIndexQuote);

                    var startIndexAuthor = tempString.IndexOf("<!--QuoteBegin ") + 15;
                    var endIndexAuthor = tempString.IndexOf(" -->Цитата:");

                    var startIndexQuoteText = tempString.IndexOf("<!--QuoteEBegin-->") + 18;
                    var endIndexQuoteText = tempString.IndexOf("<!--QuoteEnd-->");

                    var endIndexQuote = tempString.IndexOf("<!--QuoteEEnd-->") + 16;

                    var quoteAuthor = tempString.Substring(startIndexAuthor, endIndexAuthor - startIndexAuthor);
                    var quoteText = tempString.Substring(startIndexQuoteText, endIndexQuoteText - startIndexQuoteText);
                    tempString = tempString.Remove(0, endIndexQuote);

                    commentText.Add(new CommentText { QuoteAuthor = quoteAuthor, QuoteText = quoteText, ComText = text });
                }
            }

            return commentText;
        }

        public class CommentText
        {
            public string QuoteAuthor { get; set; }
            public string QuoteText { get; set; }
            public string ComText { get; set; }
        }
    }
}
