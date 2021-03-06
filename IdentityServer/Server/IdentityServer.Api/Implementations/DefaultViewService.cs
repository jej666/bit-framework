﻿using IdentityServer3.Core.Services;
using System;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;
using IdentityServer3.Core.ViewModels;
using System.IO;
using IdentityServer.Api.Contracts;
using System.Threading;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Http;

namespace IdentityServer.Api.Implementations
{
    public class DefaultViewService : IViewService
    {
        private readonly ISsoPageHtmlProvider _ssoHtmlPageProvider;

        public DefaultViewService(ISsoPageHtmlProvider ssoHtmlPageProvider)
        {
            if (ssoHtmlPageProvider == null)
                throw new ArgumentNullException(nameof(ssoHtmlPageProvider));

            _ssoHtmlPageProvider = ssoHtmlPageProvider;
        }

        protected DefaultViewService()
        {

        }

        public virtual Task<Stream> ClientPermissions(ClientPermissionsViewModel model)
        {
            throw new NotImplementedException();
        }

        public virtual Task<Stream> Consent(ConsentViewModel model, ValidatedAuthorizeRequest authorizeRequest)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<Stream> Error(ErrorViewModel model)
        {
            string content = $@"<!DOCTYPE html>
                            <html>
                                <head>
                                    <title>{model.ErrorMessage}</title>
                                </head>
                                <body>{model.ErrorMessage}</body>
                            </html>";

            return await ReturnHtmlAsync(model, content, CancellationToken.None);
        }

        public virtual async Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message)
        {
            string content = $@"<!DOCTYPE html>
                            <html>
                                <head>
                                    <meta http-equiv='refresh' content='0;{model.RedirectUrl}'>
                                </head>
                                <body></body>
                            </html>";

            return await ReturnHtmlAsync(model, content, CancellationToken.None);
        }

        public virtual async Task<Stream> Login(LoginViewModel model, SignInMessage message)
        {
            JsonSerializerSettings jsonSerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };

            if (model.Custom == null && message.ReturnUrl != null)
            {
                string state = new Uri(message.ReturnUrl).ParseQueryString()["state"];
                model.Custom = JsonConvert.DeserializeObject<dynamic>(state, jsonSerSettings);
            }

            string json = JsonConvert.SerializeObject(new
            {
                model.AdditionalLinks,
                model.AllowRememberMe,
                model.AntiForgery,
                model.ClientLogoUrl,
                model.ClientName,
                model.ClientUrl,
                model.CurrentUser,
                model.Custom,
                model.ErrorMessage,
                model.ExternalProviders,
                model.LoginUrl,
                model.LogoutUrl,
                model.RememberMe,
                model.RequestId,
                model.SiteName,
                model.SiteUrl,
                model.Username,
                ReturnUrl = message.ReturnUrl == null ? "" : new Uri(message.ReturnUrl).ParseQueryString()["redirect_uri"]
            }, Formatting.None, jsonSerSettings);

            string loginPageHtml = (await _ssoHtmlPageProvider.GetSsoPageAsync(CancellationToken.None))
                .Replace("{model}", Microsoft.Security.Application.Encoder.HtmlEncode(json));

            return await ReturnHtmlAsync(model, loginPageHtml, CancellationToken.None);
        }

        private async Task<Stream> ReturnHtmlAsync(CommonViewModel model, string html, CancellationToken cancellationToken)
        {
            MemoryStream viewStream = new MemoryStream();

            StreamWriter writter = new StreamWriter(viewStream);

            await writter.WriteAsync(html);

            await writter.FlushAsync();

            viewStream.Seek(0, SeekOrigin.Begin);

            return viewStream;
        }

        public virtual async Task<Stream> Logout(LogoutViewModel model, SignOutMessage message)
        {
            // Based on current InvokeLogOut Middleware, this method will not be called, because of context.Authentication.SignOut("custom", "Barear"); code.

            string content = $@"<!DOCTYPE html>
                            <html>
                                <body>
                                    <form id='logoutForm' method='post' action='{model.LogoutUrl}'>
                                        <input type='hidden' name='{model.AntiForgery.Name}' value='{model.AntiForgery.Value}'>
                                    </form>
                                    <script>
                                        document.getElementById('logoutForm').submit();
                                    </script>
                                </body>
                            </html>";

            return await ReturnHtmlAsync(model, content, CancellationToken.None);
        }
    }
}
