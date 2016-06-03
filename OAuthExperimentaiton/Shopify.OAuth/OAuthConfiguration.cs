﻿#region Copyright Teference
// ************************************************************************************
// <copyright file="OAuthConfiguration.cs" company="Teference">
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
    /// <summary>
    /// Represents configuration parameters required to perform SHOPIFY OAUTH 2.0 authorization for a shop.
    /// </summary>
    public sealed class OAuthConfiguration
    {
        /// <summary>
        /// Gets or sets SHOPIFY application API access key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets SHOPIFY application secret key.
        /// </summary>
        public string SecretKey { get; set; }
    }
}