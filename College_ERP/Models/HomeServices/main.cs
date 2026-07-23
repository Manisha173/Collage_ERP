using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace College_ERP.Models.HomeServices
{
    public class main
    {
    }
    public class ResetPasswordResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}