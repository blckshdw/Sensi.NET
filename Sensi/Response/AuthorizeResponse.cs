using System;
using System.Collections.Generic;
using System.Text;

namespace Sensi.Response
{
    public class AuthorizeResponse
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool PasswordResetRequired { get; set; }
        public bool AlertOptIn { get; set; }
        public bool OffersOptIn { get; set; }
        public bool ShowEula { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
