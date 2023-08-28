using OKX.Api;

namespace OkxSurfer.Builder
{
    public class ClientFactory
    {
        public static OKXRestApiClient GetRestApiClient(string userName = "")
        {
            var client = new OKXRestApiClient();
            if (!string.IsNullOrEmpty(userName))
            {
                //Get apiKey, apiSecret, passPhrase from user and then put it into below
                client.SetApiCredentials("apiKey", "apiSecret", "passPhrase");
            }
            return client;
        }

        public static OKXStreamClient GetStreamClient(string userName = "")
        {
            var client = new OKXStreamClient();
            if (!string.IsNullOrEmpty(userName))
            {
                //Get apiKey, apiSecret, passPhrase from user and then put it into below
                client.SetApiCredentials("apiKey", "apiSecret", "passPhrase");
            }
            return client;
        }
    }
}