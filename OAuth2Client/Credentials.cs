using System;
using Newtonsoft.Json;

namespace OAuth2Client
{
    public class Credentials
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public string refresh_token;
        public string scope;

        public bool IsExpired => (expires_in - DateTime.Now.Second - bufferSeconds) <= 0;
        private readonly int bufferSeconds = 10;

        public override string ToString() { return JsonConvert.SerializeObject(this, Formatting.Indented); }
    }
}
