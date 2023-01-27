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

        public int SecondsUntilExpiration => (int)(expires_in - (DateTime.Now - timeCreated).TotalSeconds - bufferSeconds);
        public bool IsExpired => ((int)(expires_in - (DateTime.Now - timeCreated).TotalSeconds - bufferSeconds)) <= 0;
       
        private readonly DateTime timeCreated;
        private readonly int bufferSeconds = 10;

        public Credentials() { timeCreated = DateTime.Now; }

        public override string ToString() { return JsonConvert.SerializeObject(this, Formatting.Indented); }
    }
}
