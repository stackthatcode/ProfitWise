#region Copyright Teference
// ************************************************************************************
// <copyright file="ShopifyOAuth.cs" company="Teference">
// Copyright © Teference 2015. All right reserved.
// </copyright>
// ************************************************************************************
// <author>Jaspalsinh Chauhan</author>
// <email>jachauhan@gmail.com</email>
// <project>Teference - Shopify OAuth Helper</project>
// ************************************************************************************
#endregion

namespace WebApplication1
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;
    using Newtonsoft.Json;

    public sealed class ShopifyOAuth    // : IShopifyOAuth
    {

        public ShopifyOAuth(OAuthConfiguration configuration)
        {
            if (null == configuration)
            {
                throw new ArgumentNullException("configuration");
            }

            this.Configuration = configuration;
        }


        public OAuthConfiguration Configuration { get; private set; }

        //public string GetOAuthUrl(string shopName, OAuthScope scope)
        //{
        //    if (string.IsNullOrEmpty(shopName))
        //    {
        //        throw new ArgumentNullException("shopName");
        //    }

        //    if (shopName.EndsWith(AppResources.MyShopifyDomain, StringComparison.InvariantCulture))
        //    {
        //        var indexOfShopifyDomain = shopName.IndexOf(AppResources.MyShopifyDomain, StringComparison.InvariantCulture);
        //        shopName = shopName.Substring(0, indexOfShopifyDomain);
        //    }

        //    return string.Format(
        //        CultureInfo.InvariantCulture,
        //        AppResources.AuthorizationUrl,
        //        shopName,
        //        this.Configuration.ApiKey,
        //        ScopeBuilder.Build(scope));
        //}


        public OAuthState AuthorizeClient(string shopName, string authorizationCode, string hmacHash, string timestamp)
        {
            if (string.IsNullOrEmpty(shopName))
                throw new ArgumentNullException("shopName");
            if (string.IsNullOrEmpty(authorizationCode))
                throw new ArgumentNullException("authorizationCode");
            if (string.IsNullOrEmpty(hmacHash))
                throw new ArgumentNullException("hmacHash");
            if (string.IsNullOrEmpty(timestamp))
                throw new ArgumentNullException("timestamp");


            //var hashValidationResult = this.ValidateInstallResponse(new OAuthState { ShopName = shopName, AuthorizationCode = authorizationCode, HmacHash = hmacHash, AuthorizationTimestamp = timestamp });
            //if (!hashValidationResult)
            //{
            //    return new OAuthState { IsSuccess = false, Error = "HMAC signature validation failed" };
            //}

            try
            {
                var accessTokenUrl = string.Format("https://{0}/admin/oauth/access_token", shopName);

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(accessTokenUrl);
                httpWebRequest.Method = "POST";

                var postdata_template = @"client_id={0}&client_secret={1}&code={2}";

                var postdata =
                    string.Format(
                        postdata_template,
                        HttpUtility.UrlEncode(this.Configuration.ApiKey),
                        HttpUtility.UrlEncode(this.Configuration.SecretKey),
                        HttpUtility.UrlEncode(authorizationCode));

                //// Not sure if that would help but I encountered this while fixing another BUG so going to keep it in place.
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(postdata);
                    streamWriter.Close();
                }

                var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var jsonResponse = string.Empty;
                using (var responseStream = httpWebResponse.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            jsonResponse = streamReader.ReadToEnd();
                            streamReader.Close();
                        }
                    }
                }

                var oauthResponse = JsonConvert.DeserializeObject<OAuthState>(jsonResponse);
                oauthResponse.ShopName = shopName;
                oauthResponse.IsSuccess = true;
                return oauthResponse;
            }
            catch (Exception exception)
            {
                return new OAuthState { IsSuccess = false, Error = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", exception.GetType().Name, exception.Message) };
            }
        }

        private static string ByteArrayToHexString(ICollection<byte> byteData)
        {
            var hexStringBuild = new StringBuilder(byteData.Count * 2);
            foreach (var item in byteData)
            {
                hexStringBuild.AppendFormat("{0:x2}", item);
            }

            return hexStringBuild.ToString();
        }

        //private bool ValidateInstallResponse(OAuthState installState)
        //{
        //    var queryStringBuilder = new QueryStringBuilder { StartsWith = null };
        //    queryStringBuilder.Add(AppResources.CodeKeyword, installState.AuthorizationCode);
        //    queryStringBuilder.Add(AppResources.ShopKeyword, installState.ShopName);
        //    queryStringBuilder.Add(AppResources.TimestampKeyword, installState.AuthorizationTimestamp);




        //    var secretKeyBytes = Encoding.UTF8.GetBytes(this.Configuration.SecretKey);
        //    var installResponseBytes = Encoding.UTF8.GetBytes(queryStringBuilder.ToString());
        //    using (var hmacsha256 = new HMACSHA256(secretKeyBytes))
        //    {
        //        var generatedInstallResponseHmacHashBytes = hmacsha256.ComputeHash(installResponseBytes);
        //        var generatedInstallResponseHmacHash = ByteArrayToHexString(generatedInstallResponseHmacHashBytes);
        //        return generatedInstallResponseHmacHash.Equals(installState.HmacHash);
        //    }
        //}
    }
}