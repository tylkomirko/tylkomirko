using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WykopSDK.Utils
{
    public class RetryHandler : DelegatingHandler
    {
        private const int MaxRetries = 4;

        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            for (int i = 0; i < MaxRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response != null && response.IsSuccessStatusCode)
                    return response;
            }

            return response;
        }
    }
}
