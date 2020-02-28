using System.Linq;
using System.Text.RegularExpressions;
using Castle.Core.Internal;

namespace Push.Foundation.Web.Helpers
{
    public class LinkHeader
    {
        public string FirstLink { get; set; }
        public string PrevLink { get; set; }
        public string NextLink { get; set; }
        public string LastLink { get; set; }

        public bool Empty => FirstLink.IsNullOrEmpty() 
                            && PrevLink.IsNullOrEmpty() 
                            && NextLink.IsNullOrEmpty() 
                            && LastLink.IsNullOrEmpty();
        public bool EOF => NextLink.IsNullOrEmpty() || PrevLink == NextLink;


        public static LinkHeader FromHeader(string linkHeaderStr)
        {
            LinkHeader linkHeader = new LinkHeader();

            if (!string.IsNullOrWhiteSpace(linkHeaderStr))
            {
                string[] linkStrings = linkHeaderStr.Split(',');

                if (linkStrings.Any())
                {
                    linkHeader = new LinkHeader();

                    foreach (string linkString in linkStrings)
                    {
                        var relMatch = Regex.Match(linkString, "(?<=rel=\").+?(?=\")", RegexOptions.IgnoreCase);
                        var linkMatch = Regex.Match(linkString, "(?<=<).+?(?=>)", RegexOptions.IgnoreCase);

                        if (relMatch.Success && linkMatch.Success)
                        {
                            string rel = relMatch.Value.ToUpper();
                            string link = linkMatch.Value;

                            switch (rel)
                            {
                                case "FIRST":
                                    linkHeader.FirstLink = link;
                                    break;
                                case "PREV":
                                    linkHeader.PrevLink = link;
                                    break;
                                case "NEXT":
                                    linkHeader.NextLink = link;
                                    break;
                                case "LAST":
                                    linkHeader.LastLink = link;
                                    break;
                            }
                        }
                    }
                }
            }

            return linkHeader;
        }
    }

}
