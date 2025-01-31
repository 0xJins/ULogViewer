﻿using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace CarinaStudio.ULogViewer.ViewModels
{
    /// <summary>
    /// Application information.
    /// </summary>
    class AppInfo : AppSuite.ViewModels.ApplicationInfo
    {
        // URI of GitHub project.
        public override Uri? GitHubProjectUri => new Uri("https://github.com/carina-studio/ULogViewer");


        // URI of PayPal.
        public override Uri? PayPalUri => new Uri("https://paypal.me/CarinaStudio");


        // URI of privacy policy.
        public override Uri? PrivacyPolicyUri => this.Application.CultureInfo.ToString() switch
        {
            "zh-TW" => new Uri("https://carina-studio.github.io/ULogViewer/privacy_policy_zh-TW.html"),
            _ => new Uri("https://carina-studio.github.io/ULogViewer/privacy_policy.html"),
        };


        // URI of user agreement.
        public override Uri? UserAgreementUri => this.Application.CultureInfo.ToString() switch
        {
            "zh-TW" => new Uri("https://carina-studio.github.io/ULogViewer/user_agreement_zh-TW.html"),
            _ => new Uri("https://carina-studio.github.io/ULogViewer/user_agreement.html"),
        };
    }
}
