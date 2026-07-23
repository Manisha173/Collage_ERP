 using College_ERP.Models.HomeServices;
using College_ERP.Models.Library;
using College_ERP.Models.MailService;
using College_ERP.Models.SuperAdmin;
using LCMIS.Server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace College_ERP.Controllers
{
    public class HomeController : Controller
    {

		private readonly SuperAdminDataService superAdmin = new SuperAdminDataService();
        private readonly HomeService _home;
        public HomeController()
        {
            
            _home = new HomeService();
        }
        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult AboutUs()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        
		public ActionResult AboutIntitution()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
		public ActionResult VisionAndMission()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
		public ActionResult Activities()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
		public ActionResult Programs()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}

		public ActionResult AnnualFunction()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}

		public ActionResult AdmissionEnquiry()
		{
			ViewBag.Message = "Your contact page.";  

			return View();
		}

		public ActionResult AdmissionForm()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}

		public ActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public ActionResult checkCredential(string username,string password)
		{
			var res = superAdmin.CheckLoginCredential(username, password);
			if (res != null)
			{
				string redirecturl =string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]?.ToString())?null: Request.QueryString["ReturnUrl"]?.ToString();

				FormsAuthentication.SetAuthCookie(username, createPersistentCookie: true);
				if (res == "superadmin") { return Json(new { status = true, message = "Invalid Credential",route= redirecturl ?? "/superadmin/dashboard" }); }
				else if (res == "admin") { return Json(new { status = true, message = "Invalid Credential", route = redirecturl ?? "/admin/dashboard" }); }
				else if (res == "warden") { return Json(new { status = true, message = "Invalid Credential", route = redirecturl ?? "/warden/dashboard" }); }
				else if (res == "teacher") { return Json(new { status = true, message = "Invalid Credential", route = redirecturl ?? "/teacher/dashboard" }); }
				else if (res == "security") { return Json(new { status = true, message = "Invalid Credential", route = redirecturl ?? "/security/dashboard" }); }
				else if(res== "librarian") { return Json(new { status = true, message = "Invalid Credential", route = redirecturl ?? "/library/dashboard" }); }

            }
			return Json(new { status = false,message="Invalid Credential" });
		}
		public ActionResult Logout()
		{
			FormsAuthentication.SignOut();
			return RedirectToAction("Login");
		}

        [HttpPost]
        public JsonResult resetPasswordByUsername(string username)
        {
            int otp = _home.GenerateAndSendOtp(username,out string message);

            if (otp<=0)
                return Json(new { status = false, message = message });

            Session["Otp"] = otp;
            Session["OtpExpiry"] = DateTime.Now.AddMinutes(10);
            Session["OtpUsername"] = username;

            return Json(new { status = true, message = message });
        }

        [HttpPost]
        public JsonResult VerifyOtp(string username, int otp)
        {
            int sessionOtp = Convert.ToInt32(Session["Otp"]);
            DateTime? expiry = Session["OtpExpiry"] as DateTime?;

            if (sessionOtp == 0 || expiry == null || DateTime.Now > expiry.Value)
            {
                return Json(new { status = false, message = "OTP expired or not found." });
            }

            if (otp != sessionOtp)
            {
                return Json(new { status = false, message = "Invalid OTP." });
            }

            // Optional: log success, store token, etc.
            Session.Remove("Otp");
            Session.Remove("OtpExpiry");

            return Json(new { status = true, message = "OTP verified." });
        }


        [HttpPost]
        public JsonResult UpdatePasswordAfterOtp(string username, string newPassword)
        {
            try
            {
                bool status = _home.UpdateUserPassword(username, newPassword,out string message);

                return Json(new { status = status, message =message });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        public ActionResult NotFound()
        {
            return View("Error");
        }
        public ActionResult ServerError()
        {
            return View("Error505");
        }

    }
}