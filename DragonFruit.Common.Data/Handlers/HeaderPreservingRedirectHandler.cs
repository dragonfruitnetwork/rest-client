// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DragonFruit.Common.Data.Handlers
{
    /// <summary>
    /// <see cref="HttpClient"/> will auto-strip the auth header, even on redirects to the same site.
    /// This handler "bypasses" this protection if the host is the same. It also supports an inner handler, should you wish to configure one.
    ///
    /// <para>
    /// You should only use this if you know what you're doing
    /// </para>
    /// <remarks>
    /// Built from the responses in https://stackoverflow.com/questions/19491525/httpclient-not-sending-basic-authentication-after-redirect/19493338
    /// </remarks>
    /// </summary>
    public class HeaderPreservingRedirectHandler : DelegatingHandler
    {
        public HeaderPreservingRedirectHandler()
            : this(new HttpClientHandler())
        {
        }

        public HeaderPreservingRedirectHandler(HttpMessageHandler innerHandler)
        {
            if (innerHandler is HttpClientHandler clientHandler)
            {
                clientHandler.AllowAutoRedirect = false;
                InnerHandler = clientHandler;
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<HttpResponseMessage>();

            base.SendAsync(request, cancellationToken)
                .ContinueWith(t =>
                {
                    HttpResponseMessage response;

                    try
                    {
                        response = t.Result;
                    }
                    catch (Exception e)
                    {
                        response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                        {
                            ReasonPhrase = e.Message
                        };
                    }

                    if (response.StatusCode == HttpStatusCode.MovedPermanently
                        || response.StatusCode == HttpStatusCode.Moved
                        || response.StatusCode == HttpStatusCode.Redirect
                        || response.StatusCode == HttpStatusCode.Found
                        || response.StatusCode == HttpStatusCode.SeeOther
                        || response.StatusCode == HttpStatusCode.RedirectKeepVerb
                        || response.StatusCode == HttpStatusCode.TemporaryRedirect
                        || (int)response.StatusCode == 308)
                    {
                        var newRequest = CopyRequest(response);

                        if (response.StatusCode == HttpStatusCode.Redirect
                            || response.StatusCode == HttpStatusCode.Found
                            || response.StatusCode == HttpStatusCode.SeeOther)
                        {
                            newRequest.Content = null;
                            newRequest.Method = HttpMethod.Get;
                        }

                        newRequest.RequestUri = response.Headers.Location;

                        base.SendAsync(newRequest, cancellationToken)
                            .ContinueWith(t2 => tcs.SetResult(t2.Result), cancellationToken);
                    }
                    else
                    {
                        tcs.SetResult(response);
                    }
                }, cancellationToken);

            return tcs.Task;
        }

        private static HttpRequestMessage CopyRequest(HttpResponseMessage response)
        {
            var oldRequest = response.RequestMessage;

            var newRequest = new HttpRequestMessage(oldRequest.Method, oldRequest.RequestUri);

            if (response.Headers.Location != null)
            {
                newRequest.RequestUri = response.Headers.Location.IsAbsoluteUri
                    ? response.Headers.Location
                    : new Uri(newRequest.RequestUri, response.Headers.Location);
            }

            foreach (var header in oldRequest.Headers)
            {
                if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) && !oldRequest.RequestUri.Host.Equals(newRequest.RequestUri.Host))
                {
                    //do not leak Authorization Header to other hosts
                    continue;
                }

                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var property in oldRequest.Properties)
            {
                newRequest.Properties.Add(property);
            }

            if (response.StatusCode == HttpStatusCode.Redirect
                || response.StatusCode == HttpStatusCode.Found
                || response.StatusCode == HttpStatusCode.SeeOther)
            {
                newRequest.Content = null;
                newRequest.Method = HttpMethod.Get;
            }
            else if (oldRequest.Content != null)
            {
                newRequest.Content = new StreamContent(oldRequest.Content.ReadAsStreamAsync().Result);
            }

            return newRequest;
        }
    }
}
