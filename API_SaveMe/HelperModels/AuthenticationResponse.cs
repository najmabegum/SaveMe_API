using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SaveMe.HelperModels
{
    public class AuthenticationResponse
    {
        public bool IsAuthenticated { get; set; }
        public User User { get; set; }
    }
}