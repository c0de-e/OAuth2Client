using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth2Client
{
    public struct Credentials
    {
        public string access_token;
        public string token_type;
        public string expires_in;
        public string refresh_token;
        public string scope;
    }
}
