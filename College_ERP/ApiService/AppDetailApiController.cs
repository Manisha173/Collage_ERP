using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Web.Http;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.Security;
using College_ERP.Models.StudentServices;
using College_ERP.Models.SuperAdmin;
using College_ERP.Models.Teacher;
using College_ERP.Models.Warden;

namespace College_ERP.ApiService
{
    public class AppDetailApiController : ApiController
    {
        private readonly AdminServices _admin;
        private readonly WardenService _warden;
        private readonly TeacherService _teacher;
        private readonly StudentServices _student;
        private readonly SecurityService _security;
        public AppDetailApiController()
        {
            _admin = new AdminServices();
            _warden = new WardenService();
            _teacher = new TeacherService();
            _student = new StudentServices();
            _security = new SecurityService();
        }
        [Route("api/getabout")]
        [HttpGet]
        public IHttpActionResult GetAppAbout(int userid,string role)
        {
            try
            {
                int adminid = 0;
                if (role == "warden")
                {
                    adminid = _warden.GetAdminId(userid);
                }
                else if (role == "teacher")
                {
                    adminid = _teacher.GetAdminId(userid);
                }
                else if (role == "student")
                {
                    adminid = _student.GetAdminId(userid);
                }
                else if (role == "security")
                {
                    adminid = _security.GetAdminId(adminid);
                }
                var data = _admin.GetAppAbout(adminid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getcontact")]
        [HttpGet]
        public IHttpActionResult GetAppContact(int userid,string role)
        {
            try
            {
                int adminid = 0;
                if (role == "warden")
                {
                    adminid = _warden.GetAdminId(userid);
                }
                else if (role == "teacher")
                {
                    adminid = _teacher.GetAdminId(userid);
                }
                else if (role == "student")
                {
                    adminid = _student.GetAdminId(userid);
                }
                else if (role == "security")
                {
                    adminid = _security.GetAdminId(userid);
                }
                var data = _admin.GetAppContact(adminid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getprivacypolicy")]
        [HttpGet]
        public IHttpActionResult GetAppPrivacyPolicy(int userid,string role)
        {
            try
            {
                int adminid = 0;
                if (role == "warden")
                {
                    adminid = _warden.GetAdminId(userid);
                }
                else if (role == "teacher")
                {
                    adminid = _teacher.GetAdminId(userid);
                }
                else if (role == "student")
                {
                    adminid = _student.GetAdminId(userid);
                }
                else if (role == "security")
                {
                    adminid = _security.GetAdminId(userid);
                }
                var data = _admin.GetPrivacyPolicy(adminid,role);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/gettermsandconditions")]
        [HttpGet]
        public IHttpActionResult GetTermsAndConditions(int userid,string role)
        {
            try
            {
                int adminid = 0;
                if (role == "warden")
                {
                    adminid = _warden.GetAdminId(userid);
                }
                else if (role == "teacher")
                {
                    adminid = _teacher.GetAdminId(userid);
                }
                else if (role == "student")
                {
                    adminid = _student.GetAdminId(userid);
                }
                else if (role == "security")
                {
                    adminid = _security.GetAdminId(userid);
                }
                var data = _admin.GetTermsAndConditions(adminid,role);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getversiondetail")]
        [HttpGet]
        public IHttpActionResult GetAppVersionDetail(int userid, string role)
        {
            try
            {
                int adminid = 0;
                if (role == "warden")
                {
                    adminid = _warden.GetAdminId(userid);
                }
                else if (role == "teacher")
                {
                    adminid = _teacher.GetAdminId(userid);
                }
                else if (role == "student")
                {
                    adminid = _student.GetAdminId(userid);
                }
                else if (role == "security")
                {
                    adminid = _security.GetAdminId(userid);
                }
                var data = _admin.GetAppVersionDetail(adminid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
    }
}
