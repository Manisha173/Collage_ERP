using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Web.Http;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.Security;
using College_ERP.Models.SuperAdmin;
using System.Web;
using static College_ERP.Models.Security.main;

namespace College_ERP.ApiService
{
    public class SecurityApiController : ApiController
    {
        private readonly AdminServices _admin;
        private readonly SuperAdminDataService _superAdmin;
        private readonly HomeService _home;
        private readonly SecurityService _security;
        public SecurityApiController()
        {
            _admin = new AdminServices();
            _superAdmin = new SuperAdminDataService();
            _home = new HomeService();
            _security = new SecurityService();
        }
        [Route("api/securityProfile")]
        [HttpGet]
        public IHttpActionResult GetSecurityProfile(int securityid)
        {
            try
            {
                var data = _admin.GetAllSecurityById(securityid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/notice")]
        [HttpGet]
        public IHttpActionResult GetNotice(int securityid, string search = null)
        {
            try
            {
                int adminid = _security.GetAdminId(securityid);
                var data = _security.GetSecurityNotices("security", securityid,adminid,search);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #region Visitor Management
        [Route("api/insertvisitor")]
        [HttpPost]
        public IHttpActionResult InsertVisitor()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var visitor = new VisitorModel
                {
                    userId = Convert.ToInt32(request.Form.Get("securityId")),
                    name = request.Form["name"],
                    email = request.Form["email"],
                    mobile = Convert.ToInt64(request.Form["mobile"]),
                    address = request.Form["address"],
                    userType = request.Form["userType"],
                    role = request.Form["role"],
                    personId = Convert.ToInt32(request.Form["personId"]),
                    studentId = Convert.ToInt32(request.Form["studentId"]),
                    reason = request.Form["reason"],
                    remark = request.Form["remark"],
                };
                HttpPostedFile file = request.Files["image"];
                if (file != null)
                {
                    HttpPostedFileWrapper cfile = new HttpPostedFileWrapper(file);
                    visitor.image = cfile;
                }
                var result = _security.InsertVisitor(visitor);
                return Ok(new { status = result, message = result ? "Visitor Added successfully." : "Failed to add visitor." });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/visitorlist")]
        [HttpGet]
        public IHttpActionResult GetVisitorList(int securityid, string search = null)
        {
            try
            {
                var data = _security.GetAllVisitorsList(securityid,search);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/loginvisitor")]
        [HttpPost]
        public IHttpActionResult InsertVisitorMeeting()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var visitor = new VisitorModel
                {
                    userType = request.Form["userType"],
                    role = request.Form["role"],
                    personId = Convert.ToInt32(request.Form["personId"]),
                    studentId = Convert.ToInt32(request.Form["studentId"]),
                    vid = Convert.ToInt32(request.Form["visitorId"]),
                    reason = request.Form["reason"],
                    remark = request.Form["remark"],
                };
                HttpPostedFile file = request.Files["image"];
                if (file != null)
                {
                    HttpPostedFileWrapper cfile = new HttpPostedFileWrapper(file);
                    visitor.image = cfile;
                }
                var result = _security.InsertVisitorMeeting(visitor);
                return Ok(new { status = result, message = result ? "Visitor Login successfully." : "Failed to login visitor." });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/visitorlogout")]
        [HttpGet]
        public IHttpActionResult VisitorLogout(int meetingid)
        {
            try
            {
                var result = _security.LogoutVisitor(meetingid);
                return Ok(new { status = result, message = result ? "Visitor Logout successfully." : "Failed to logout visitor." });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/previousvisitormeeting")]
        [HttpGet]
        public IHttpActionResult GetPreviousVisitorMeeting(int visitoid)
        {
            try
            {
                var data = _security.GetVisitorPreviousMeeting(visitoid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/loginvisitorlist")]
        [HttpGet]
        public IHttpActionResult GetLoginVisitorList(int securityid, string search = null)
        {
            try
            {
                var data = _security.GetLoginVisitorsList(securityid, search);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/visitorfilter")]
        [HttpGet]
        public IHttpActionResult GetVisitorByFilter(string filter,int securityid)
        {
            try
            {
                var data = _security.GetAllVisitorsListByFilter(filter,securityid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getrolesofstaff")]
        [HttpGet]
        public IHttpActionResult GetRolesOfStaff()
        {
            try
            {
                var data = _security.GetRoleOfStaff();
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getstafflist")]
        [HttpGet]
        public IHttpActionResult GetStaffList(string role,int securityid, string search = null)
        {
            try
            {
                int adminid = _security.GetAdminId(securityid);
                var data = _security.GetStaffList(role,adminid,search);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getrooms")]
        [HttpGet]
        public IHttpActionResult GetRoomNo(int securityid, string search = null)
        {
            try
            {
                var data = _security.GetRoomsByBlockId(securityid, search);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getstudentinroom")]
        [HttpGet]
        public IHttpActionResult GetStudentOfRoom(int roomid,int securityid,string search = null)
        {
            try
            {
                int adminid = _security.GetAdminId(securityid);
                var data = _security.SelectUsersByRoomNo(roomid,adminid,search);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/visitorhistory")]
        [HttpGet]
        public IHttpActionResult GetVisitorHistory(int visitorid, string search = null)
        {
            try
            {
                var data = _security.GetVisitorHistory(visitorid,search);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion 
        [Route("api/securitydashboard")]
        [HttpGet]
        public IHttpActionResult GetSecurityDashboard(int securityid)
        {
            try
            {
                var data = _security.GetSecurityDashboard(securityid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/noticecount")]
        [HttpGet]
        public IHttpActionResult GetSecurityNoticeCount(int securityid)
        {
            try
            {
                int adminid = _security.GetAdminId(securityid);
                var data = _security.GetNoticeCount(adminid,securityid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        

    }
}
