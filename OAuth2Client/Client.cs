using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace OAuth2Client
{
    /// <summary> Simple OAuth2 client for easy third party authentication </summary>
    public class Client
    {
        private string AuthorizationBaseURL;
        private string TokenURL;
        private string RefreshURL;
        private string ClientID;
        private string ClientSecret;
        private string RedirectURI;
        private IList<string> Scopes;
        private string ScopeSeperator;
        private List<KeyValuePair<string, string>> AdditionalParams;
        private string GrantType;

        static readonly HttpClient client = new HttpClient();

        private Credentials credentials;
        /// <summary> The token return from the Token URL after authorization </summary>
        /// <exception cref="FieldAccessException"> The credentials need to be initialized before they are accessed </exception>
        public Credentials Credentials
        {
            get
            {
                if (credentials == null) throw new FieldAccessException("Please initialize credentials before attempting to access.");
                if (RefreshURL != null && credentials.IsExpired)
                    credentials = Task.Run(() => GetRefreshToken(credentials.refresh_token)).Result;
                return credentials;
            }
            set { credentials = value; }
        }

        /// <summary> Sets the clients's authorization base URL (required) </summary>
        /// <param name="url"> The authorization endpoint base URL </param>
        /// <returns> The current client, for chaining </returns>
        public Client SetAuthorizationBaseURL(string url)
        {
            AuthorizationBaseURL = url;
            return this;
        }

        /// <summary> Sets the client's token URL (required) </summary>
        /// <param name="url"> The token endpoint URL </param>
        /// <returns> The current client, for chaining </returns>
        public Client SetTokenURL(string url)
        {
            TokenURL = url;
            return this;
        }

        /// <summary> Sets the clients's refresh token endpoint URL (optional) </summary>
        /// <param name="url"> The refresh token endpoint URL </param>
        /// <returns> The current client, for chaining </returns>
        public Client SetRefreshURL(string url)
        {
            RefreshURL = url;
            return this;
        }

        /// <summary> Sets the client ID to use (required) </summary>
        /// <param name="client_id"> The client ID to use </param>
        /// <returns> The current client, for chaining </returns>
        public Client SetClientID(string client_id)
        {
            ClientID = client_id;
            return this;
        }

        /// <summary> Sets the client secret to use (required) </summary>
        /// <param name="client_secret"> The client secret to use </param>
        /// <returns> The current client, for chaining </returns>
        public Client SetClientSecret(string client_secret)
        {
            ClientSecret = client_secret;
            return this;
        }

        /// <summary> Sets the redirect URI to use after authenticating (required) </summary>
        /// <param name="redirect_uri"> The redirect URI to use </param>
        /// <returns> The current client, for chaining </returns>
        public Client SetRedirectURI(string redirect_uri)
        {
            RedirectURI = redirect_uri;
            return this;
        }

        /// <summary> Sets the scope(s) to use (optional) <br/><br/>
        /// Will replace any previously set individual scopes </summary>
        /// <param name="scope"> The array of requested scopes </param>
        /// <param name="seperator"> The optional seperator to use when joining scopes </param>
        /// <returns> The current client, for chaining </returns>
        public Client SetScope(string[] scope, string seperator = " ")
        {
            Scopes = scope;
            ScopeSeperator = seperator;
            return this;
        }
        /// <summary> Sets a scope to use (optional) <br/><br/>
        /// May chain to set multiple scopes </summary>
        /// <param name="scope"> The requested scope </param>
        /// <returns> The current client, for chaining </returns>
        public Client SetScope(string scope)
        {
            if (Scopes == null) Scopes = new List<string>();
            Scopes.Add(scope);
            return this;
        }

        /// <summary> Sets an additional parameter to use when constructing the authorization URL (optional) </summary>
        /// <param name="name"> The parameter name </param>
        /// <param name="value"> The parameter value </param>
        /// <returns> The current client, for chaining </returns>
        public Client SetParam(string name, string value)
        {
            if (AdditionalParams == null) AdditionalParams = new List<KeyValuePair<string, string>>();
            AdditionalParams.Add(new KeyValuePair<string, string>(name, value));
            return this;
        }

        private Client SetGrantType(string grant_type)
        {
            GrantType = grant_type;
            return this;
        }

        /// <summary> Uses the given parameters to build an authorization URL and start a webpage to authenticate </summary>
        /// <returns> The token, if successful </returns>
        /// <exception cref="Exception"> Must include required parameters / values </exception>
        public async Task<Credentials> Build()
        {
            List<string> errors = new List<string>();
            if (AuthorizationBaseURL == null) errors.Add("AuthorizationBaseURL");
            if (RedirectURI == null) errors.Add("redirect_uri");
            if (TokenURL == null) errors.Add("TokenURL");
            if (ClientID == null) errors.Add("client_id");
            if (ClientSecret == null) errors.Add("client_secret");
            if (errors.Count > 0)
                throw new Exception($"Must include {string.Join(", ", errors)}");

            return await StartClientForm();
        }

        private async Task<Credentials> StartClientForm()
        {
            string authURL = await GetAuthorizationURL();
            using (OAuth2ClientForm clientForm = new OAuth2ClientForm(authURL))
            {
                clientForm.ShowDialog();
                if (clientForm.DialogResult == DialogResult.OK)
                    Credentials = await ExchangeCodeForToken(clientForm.Code);
            }
            return Credentials;
        }

        private async Task<string> GetAuthorizationURL()
        {
            string url = $"{AuthorizationBaseURL}?";
            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", ClientID),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("response_type", "code"),
                new KeyValuePair<string, string>("redirect_uri", RedirectURI),
            });
            url += await body.ReadAsStringAsync();

            var extraParams = new List<KeyValuePair<string, string>> { };
            if (Scopes != null)
                extraParams.Add(new KeyValuePair<string, string>("scope", string.Join(ScopeSeperator ?? " ", Scopes)));
            if (AdditionalParams != null)
                extraParams.AddRange(AdditionalParams);
            if (extraParams.Count > 0) url += "&" + await new FormUrlEncodedContent(extraParams).ReadAsStringAsync();
            return url;
        }

        private async Task<Credentials> ExchangeCodeForToken(string code)
        {
            string res = string.Empty;
            SetGrantType("authorization_code");
            var body = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", ClientID),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("grant_type", GrantType),
                new KeyValuePair<string, string>("redirect_uri", RedirectURI),
                new KeyValuePair<string, string>("code", code)
            };

            using (HttpResponseMessage response = await client.PostAsync(TokenURL, new FormUrlEncodedContent(body)))
            {
                res = await response.Content.ReadAsStringAsync();
            }
            return JsonConvert.DeserializeObject<Credentials>(res);
        }

        private async Task<Credentials> GetRefreshToken(string refresh_token)
        {
            string res = string.Empty;
            SetGrantType("refresh_token");
            var body = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", ClientID),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("grant_type", GrantType),
                new KeyValuePair<string, string>("refresh_token", refresh_token),
            };

            using (HttpResponseMessage response = await client.PostAsync(RefreshURL, new FormUrlEncodedContent(body)))
            {
                res = await response.Content.ReadAsStringAsync();
            }
            return JsonConvert.DeserializeObject<Credentials>(res);
        }

        public static void ShowAssemblyVersion()
        {
            MessageBox.Show(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }
}
// TODO add loopback option