using Antlr.Runtime.Tree;
using College_ERP.Models.Admin;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.SuperAdmin;
using College_ERP.Models.Warden;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace College_ERP.ApiService
{
    public class AdminApiController : ApiController
    {

        MemoryCache _cache = MemoryCache.Default;

        private readonly AdminServices _admin;
        private readonly SuperAdminDataService _superAdmin;
        private readonly HomeService _home;
        public AdminApiController()
        {
            _admin = new AdminServices();
            _home = new HomeService();
            _superAdmin = new SuperAdminDataService();
        }

        //[Route("api/login")]
        //[HttpGet]
        //public IHttpActionResult Login(string username, string password)
        //{
        //    var res = _superAdmin.CheckLoginCredential(username,password);
        //    if (res != null)
        //    {
        //        int userid = _home.GetUserId(username);
        //    return Ok(new
        //    {
        //        status = true,
        //        data = new {id= userid ,role=res},
        //        message="Data received."
        //    });

        //    }
        //    return Ok(new
        //    {
        //        status = false,
        //        data = new { },
        //        message = "Invalid credential."
        //    });
        //}
        //#region Forget Password
        //[Route("api/generateotp")]
        //[HttpGet]
        //public IHttpActionResult GenerateOtp(string username)
        //{
        //    try
        //    {
        //        string message = null;
        //        if (string.IsNullOrWhiteSpace(username))
        //            return Ok(new { status = false, message = "Username is required." });

        //        string key = username;
        //        var data = _cache.Get(key);
        //        _cache.Remove(key);
        //        data = null;
        //        if (data == null)
        //        {
        //            data = _home.GenerateAndSendOtp(username, out message);
        //            _cache.Set(key, data, DateTimeOffset.Now.AddMinutes(10));
        //        }
        //        if(Convert.ToInt32(data) ==0)
        //        {
        //            return Ok(new { status = false, message = message });
        //        }
        //        else if (Convert.ToInt32(data) == -1)
        //        {
        //            return Ok(new { status = false, message = message });
        //        }
        //        return Ok(new { status = true, message = message });
        //    }
        //    catch(Exception ex)
        //    {
        //        return Ok(new { status = false, message = ex.Message });
        //    }
        //}

        //[Route("api/verifyotp")]
        //[HttpGet]
        //public IHttpActionResult VerifyOtp(string username ,int otp)
        //{
        //    try
        //    {
        //        int genratedotp = Convert.ToInt32(_cache.Get(username));
        //        if (genratedotp < 0)
        //            return Ok(new { status = false, message = "Otp Expired" });

        //        if (genratedotp != otp)
        //            return Json(new { status = false, message = "Invalid OTP." });

        //        return Json(new { status = true, message = "OTP verified." });
        //    }
        //    catch(Exception ex)
        //    {
        //        return Json(new { status = false, message = ex.Message });
        //    }
        //}
        //[Route("api/resetpassword")]
        //[HttpGet]
        //public IHttpActionResult ResetPassword(string username, string newpassword)
        //{
        //    try
        //    {
        //        bool res = _home.UpdateUserPassword(username, newpassword, out string message);

        //        return Json(new { status = res, message = message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { status = false, message = ex.Message });
        //    }   
        //}
        //#endregion

        //#region HostelProblems
        //[Route("api/GetDriverProblem")]
        //[HttpGet]
        //public IHttpActionResult GetDriverProblem(int adminid)
        //{
        //    try
        //    {
        //        List<DriverProblemModel> list = _admin.GetDriverProblem(adminid);
        //        return Ok(new { status = true, data = list, message = "data retrieved!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new { status = false, message = ex.Message });
        //    }
        //}

        //[Route("api/CompleteRejectDriverProblem")]
        //[HttpPost]
        //public IHttpActionResult CompleteRejectDriverProblem(int id, int status, string reason)
        //{
        //    try
        //    {
        //        bool res = _admin.CompleteRejectDriverProblem(id, status, reason);
        //        return Ok(new
        //        {
        //            status = res,
        //            StatusCode = res ? 200 : 400,
        //            message = res ? (status == 1 ? "Completed Successfully!" : "Rejected Successfully!") : "Failed to update!",
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new { status = false, message = ex.Message });
        //    }
        //}
        //#endregion

    }
}
