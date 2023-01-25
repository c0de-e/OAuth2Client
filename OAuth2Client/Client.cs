using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Windows.Media.Protection.PlayReady;

namespace OAuth2Client
{
    public class Client
    {
        public string AuthorizationBaseURL;
        public string TokenURL;
        public string ClientID;
        public string ClientSecret;
        public string RedirectURI;
        public string[] Scope;
        public string ScopeSeperator;
        public string[] Params;
        public string GrantType;

        public Credentials Credentials;
        static readonly HttpClient client = new HttpClient();

        public Client() { GrantType = "authorization_code"; }

        public Client SetAuthorizationBaseURL(string url)
        {
            this.AuthorizationBaseURL = url;
            return this;
        }

        public Client SetTokenURL(string url)
        {
            this.TokenURL = url;
            return this;
        }

        public Client SetClientID(string client_id)
        {
            this.ClientID = client_id;
            return this;
        }

        public Client SetClientSecret(string client_secret)
        {
            this.ClientSecret = client_secret;
            return this;
        }

        public Client SetRedirectURI(string redirect_uri)
        {
            this.RedirectURI = redirect_uri;
            return this;
        }

        public Client SetScope(string[] scope, string seperator = " ")
        {
            this.Scope = scope;
            this.ScopeSeperator = seperator;
            return this;
        }

        public Client SetParams(string[] param)
        {
            this.Params = param;
            return this;
        }

        public Client SetGrantType(string grant_type)
        {
            this.GrantType = grant_type;
            return this;
        }

        public Credentials Build()
        {
            List<string> errors = new List<string>();
            if (AuthorizationBaseURL == null) errors.Add("AuthorizationBaseURL");
            if (TokenURL == null) errors.Add("TokenURL");
            if (ClientID == null) errors.Add("client_id");
            if (ClientSecret == null) errors.Add("client_secret");
            if (errors.Count > 0)
                throw new Exception($"Must include {string.Join(", ", errors)}");

            StartClientForm();
            return this.Credentials;
        }

        private async void StartClientForm()
        {
            try
            {
                string authURL = await GetAuthorizationURL();
                using (var clientForm = new OAuth2ClientForm(authURL))
                {
                    clientForm.ShowDialog();
                    if (clientForm.DialogResult == DialogResult.OK)
                    {
                        Credentials = await ExchangeCodeForToken(clientForm.Code);
                    }
                }
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
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
            string res = string.Empty; ;
            var body = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", ClientID),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("grant_type", GrantType),
                new KeyValuePair<string, string>("redirect_uri", RedirectURI),
                new KeyValuePair<string, string>("code", code)
            };
            // TODO add params and scope
            //body.AddRange()
            using (HttpResponseMessage response = await client.PostAsync(TokenURL, new FormUrlEncodedContent(body)))
            {
                res = await response.Content.ReadAsStringAsync();
            }
            return JsonConvert.DeserializeObject<Credentials>(res);
        }
    }
}
