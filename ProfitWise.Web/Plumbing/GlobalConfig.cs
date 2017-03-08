﻿using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace ProfitWise.Web.Plumbing
{
    public class GlobalConfig
    {
        public static readonly string BaseUrl = ConfigurationManager.AppSettings["application_root_url"];                

        public static RedirectResult Redirect(string destinationUrl, string returnUrl = null)
        {
            var url = $"{BaseUrl}" + $"{destinationUrl}";

            if (returnUrl != null)
            {
                url += url.Contains("?") ? "&" : "?";
                url += $"returnUrl={HttpUtility.UrlEncode(returnUrl)}";
            }
            return new RedirectResult(url);
        }
    }
}
