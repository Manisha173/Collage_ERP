using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using College_ERP.Helpers;
using College_ERP.Models.Admin;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.Login;
using College_ERP.Models.SuperAdmin;

namespace College_ERP.ApiService
{
    public class LoginApiController : ApiController
    {

        MemoryCache _cache = MemoryCache.Default;

        private readonly AdminServices _admin;
        private readonly SuperAdminDataService _superAdmin;
        private readonly HomeService _home;
        public LoginApiController()
        {
            _admin = new AdminServices();
            _home = new HomeService();
            _superAdmin = new SuperAdminDataService();
        }

        [Route("api/login")]
        [HttpGet]
        public IHttpActionResult Login(string username, string password)
        {
            var res = _superAdmin.CheckLoginCredential(username, password);
            if (res != null)
            {
                int userid = _home.GetUserId(username);
                bool IsInHostel = _home.IsInHostelornot(userid);
                var role = _home.GetRoleByUserId(username);
                var userdetails = _home.GetUserResetDetailsByUsername(username);
                string accessToken = JwtTokenHelper.GenerateAccessToken(userid, res);

                string refreshToken = JwtTokenHelper.GenerateRefreshToken();

                _home.SaveRefreshToken(userid,refreshToken,DateTime.Now.AddDays(30));

                return Ok(new
                {
                    status = true,
                    access_token = accessToken,
                    refresh_token = refreshToken,
                    id=userid,
                    Role= role,
                    IsInHostel= IsInHostel,
                    name = userdetails.FullName,
                    expires_in = 1800
                });


            }
            return Ok(new
            {
                status = false,
                data = new { },
                message = "Invalid credential."
            });
        }


        [HttpPost]
        [Route("api/refresh")]
        public IHttpActionResult Refresh(string RefreshToken)
        {
            if (RefreshToken == null || string.IsNullOrWhiteSpace(RefreshToken))
            {
                return BadRequest("Refresh token is required.");
            }

            var token = _home.GetRefreshToken(RefreshToken);

            if (token == null)
            {
                return Unauthorized();
            }

            if (token.IsRevoked)
            {
                return Unauthorized();
            }

            if (token.ExpiryDate < DateTime.Now)
            {
                return Unauthorized();
            }

            string role = _home.GetRoleByUserId(token.Username);


            string accessToken = JwtTokenHelper.GenerateAccessToken(token.UserId, role);
            string newRefreshToken = JwtTokenHelper.GenerateRefreshToken();

            bool updated = _home.UpdateRefreshToken(token.UserId,newRefreshToken,DateTime.Now.AddDays(30));

            if (!updated)
            {
                return InternalServerError();
            }

            return Ok(new
            {
                status = true,
                access_token = accessToken,
                refresh_token = newRefreshToken,
                expires_in = 1800
            });
        }


        #region Forget Password
        [Route("api/generateotp")]
        [HttpGet]
        public IHttpActionResult GenerateOtp(string username)
        {
            try
            {
                string message = null;
                if (string.IsNullOrWhiteSpace(username))
                    return Ok(new { status = false, message = "Username is required." });

                string key = username;
                var data = _cache.Get(key);
                _cache.Remove(key);
                data = null;
                if (data == null)
                {
                    data = _home.GenerateAndSendOtp(username, out message);
                    _cache.Set(key, data, DateTimeOffset.Now.AddMinutes(10));
                }
                if (Convert.ToInt32(data) == 0)
                {
                    return Ok(new { status = false, message = message });
                }
                else if (Convert.ToInt32(data) == -1)
                {
                    return Ok(new { status = false, message = message });
                }
                return Ok(new { status = true, message = message });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/verifyotp")]
        [HttpGet]
        public IHttpActionResult VerifyOtp(string username, int otp)
        {
            try
            {
                int genratedotp = Convert.ToInt32(_cache.Get(username));
                if (genratedotp < 0)
                    return Ok(new { status = false, message = "Otp Expired" });

                if (genratedotp != otp)
                    return Json(new { status = false, message = "Invalid OTP." });

                return Json(new { status = true, message = "OTP verified." });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        [Route("api/resetpassword")]
        [HttpGet]
        public IHttpActionResult ResetPassword(string username, string newpassword)
        {
            try
            {
                bool res = _home.UpdateUserPassword(username, newpassword, out string message);

                return Json(new { status = res, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        #endregion

        #region HostelProblems
        [Route("api/GetDriverProblem")]
        [HttpGet]
        public IHttpActionResult GetDriverProblem(int adminid)
        {
            try
            {
                List<DriverProblemModel> list = _admin.GetDriverProblem(adminid);
                return Ok(new { status = true, data = list, message = "data retrieved!" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/CompleteRejectDriverProblem")]
        [HttpPost]
        public IHttpActionResult CompleteRejectDriverProblem(int id, int status, string reason)
        {
            try
            {
                bool res = _admin.CompleteRejectDriverProblem(id, status, reason);
                return Ok(new
                {
                    status = res,
                    StatusCode = res ? 200 : 400,
                    message = res ? (status == 1 ? "Completed Successfully!" : "Rejected Successfully!") : "Failed to update!",
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
    }
}
