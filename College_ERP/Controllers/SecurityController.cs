using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using Antlr.Runtime.Tree;
using College_ERP.Models.Admin;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.Security;
using College_ERP.Models.Teacher;
using College_ERP.Models.Warden;
using static College_ERP.Models.Security.main;

namespace College_ERP.Controllers
{
    [Authorize(Roles ="security")]
    public class SecurityController : Controller
    {
        private readonly SecurityService _securityService;
        private readonly HomeService homeService;
        private readonly AdminServices adminServices;
        public SecurityController()
        {
            _securityService = new SecurityService();
            homeService = new HomeService();
            adminServices = new AdminServices();
        }
        // GET: Security
        public ActionResult Dashboard()
        {
            int userid = homeService.GetUserId(User.Identity.Name);

            var model = _securityService.GetSecurityDashboard(userid);

            return View(model);
        }
        [HttpGet]
        public JsonResult GetNoticeCount()
        {

           
            int UserId = homeService.GetUserId(User.Identity.Name);
            int AdminId = _securityService.GetAdminId(UserId);
            var result = _securityService.GetNoticeCount(AdminId, UserId);
            return Json(new { notice = result.notice }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetSecurityDetails()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var security = _securityService.GetSecurityDetails(userId);

            if (security != null)
            {
                var result = new
                {
                    securityId = security.securityId,
                    securityName = security.securityName,
                    securityImage = security.securityImage
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetSectionByClassId(int id)
        {
            var data = adminServices.GetSectionsByClassId(id);
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult GetStudentsByClassAndSection(int classId, int sectionId)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            int adminid = _securityService.GetAdminId(userid);
            var students = _securityService.StudentsByClassSection(classId, sectionId, adminid,userid);
            return Json(students, JsonRequestBehavior.AllowGet);
        }
        public ActionResult VisitorManagement()
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            int adminid = _securityService.GetAdminId(userid);
            ViewBag.ClassList = adminServices.GetAllClasses(adminid);
            ViewData["visitorsList"] = _securityService.GetAllVisitorsList(userid);
            ViewData["roles"] = _securityService.GetRoleOfStaff();
            ViewData["rooms"] = _securityService.GetRoomsByBlockId(userid);
            return View();
        }
        [HttpGet]
        public JsonResult GetStudentsByRoomNo(int roomNo)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            int adminid = _securityService.GetAdminId(userId);
            var data = _securityService.SelectUsersByRoomNo(roomNo, adminid);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetStaffList(string type)
        {
            int securityid = homeService.GetUserId(User.Identity.Name);
            int adminid = _securityService.GetAdminId(securityid);
            var data = _securityService.GetStaffList(type,adminid);
            return Json(data,JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult AddVistor(VisitorModel sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
           
            bool res = sm.vid>0? _securityService.InsertVisitorMeeting(sm):_securityService.InsertVisitor(sm);

            return Json(new
            {
                status = res,
                message = res ? "Visitor Logged In Successfully" : "Something went wrong"
            });
        }
        [HttpGet]
        public JsonResult LogoutVisitor(int Id)
        {
            bool res = _securityService.LogoutVisitor(Id);
            return Json(new
            {
                status = res,
                message = res ? "LogOut Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetVisitorPreviousMeeting(int id)
        {
            var data = _securityService.GetVisitorPreviousMeeting(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult GetAllVisitorByFilter(string filt)
        {
           if(User.IsInRole("security") || User.IsInRole("admin"))
            {
                int userid = homeService.GetUserId(User.Identity.Name);
                var data = _securityService.GetAllVisitorsListByFilter(filt, userid);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }
        [HttpGet]
        public ActionResult GetLoginVisitor()
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            var data = _securityService.GetLoginVisitorsList(userid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Notice()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            int adminid = _securityService.GetAdminId(userId);
            var notices = _securityService.GetSecurityNotices("Security", userId,adminid);
            return View(notices);
        }

        public ActionResult Holidays()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            int adminid = _securityService.GetAdminId(userId);
            var holidays = _securityService.GetHolidaysForAll(adminid);
            return View(holidays);
        }

        public ActionResult VisitorReport()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewData["visitorlistrepo"] = _securityService.GetAllVisitorsListForReport(userId);
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult GetVisitorsHistory(int id)
        {
            if (User.IsInRole("security") || User.IsInRole("admin"))
            {
                var data = _securityService.GetVisitorHistory(id);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }
        public ActionResult NoticeDescById(int id)
        {
            var res = _securityService.GetNoticeDscById(id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SecurityProfile(int id)
        {
            var data = adminServices.GetAllSecurityById(id);
            return View(data);
        }
    }
}