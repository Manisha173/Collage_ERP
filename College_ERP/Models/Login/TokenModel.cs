using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace College_ERP.Models.Login
{
    public class TokenModel
    {
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; }
    }

}