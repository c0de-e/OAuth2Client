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
    public class Client
    {
        private string AuthorizationBaseURL;
        private string TokenURL;
        private string RefreshURL;
        private string ClientID;
        private string ClientSecret;
        private string RedirectURI;
        private string[] Scope;
        private string ScopeSeperator;
        private List<string> Params;
        private string GrantType;

        static readonly HttpClient client = new HttpClient();

        private Credentials credentials;
        public Credentials Credentials
        {
            get
            {
                if (credentials == null) throw new FieldAccessException("Please initialize credentials before attempting to access.");
                if (credentials.IsExpired)
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

        public Client SetTokenURL(string url)
        {
            TokenURL = url;
            return this;
        }

        public Client SetRefreshURL(string url)
        {
            RefreshURL = url;
            return this;
        }

        public Client SetClientID(string client_id)
        {
            ClientID = client_id;
            return this;
        }

        public Client SetClientSecret(string client_secret)
        {
            ClientSecret = client_secret;
            return this;
        }

        public Client SetRedirectURI(string redirect_uri)
        {
            RedirectURI = redirect_uri;
            return this;
        }

        public Client SetScope(string[] scope, string seperator = " ")
        {
            Scope = scope;
            ScopeSeperator = seperator;
            return this;
        }

        public Client SetParam(string param)
        {
            if (Params == null) Params = new List<string>();
            Params.Add(param);
            return this;
        }

        private Client SetGrantType(string grant_type)
        {
            GrantType = grant_type;
            return this;
        }

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
            if (Scope != null) extraParams.Add(new KeyValuePair<string, string>("scope", string.Join(ScopeSeperator, Scope)));
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