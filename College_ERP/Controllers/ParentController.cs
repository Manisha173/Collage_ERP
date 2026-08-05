using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.ParentServices;
using College_ERP.Models.StudentServices;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static College_ERP.Models.ParentServices.main;
using static College_ERP.Models.StudentServices.main;

namespace College_ERP.Controllers
{
    [Authorize(Roles = "Parent")]
    public class ParentController : Controller
    {
        private readonly HomeService homeService;
        private readonly StudentServices studentService;
        private readonly ParentService parentService;
        private readonly AdminServices adminServices;

        private const string ActiveStudentSessionKey = "ActiveStudentId";

        public ParentController()
        {
            homeService = new HomeService();
            studentService = new StudentServices();
            parentService = new ParentService();
            adminServices = new AdminServices();
        }
        private int GetActiveStudentId()
        {
            if (Session[ActiveStudentSessionKey] != null)
            {
                return Convert.ToInt32(Session[ActiveStudentSessionKey]);
            }

            int defaultStudentId = homeService.GetUserId(User.Identity.Name);
            Session[ActiveStudentSessionKey] = defaultStudentId;
            return defaultStudentId;
        }

        [HttpGet]
        public JsonResult GetparentDetails()
        {
            int studentId = GetActiveStudentId();

            if (studentId == 0)
                return Json(new { parentName = "Parent", ProfileImage = "" }, JsonRequestBehavior.AllowGet);

            var profile = parentService.ParentProfile(studentId).FirstOrDefault();
            var activeStudent = parentService.GetStudentsList(studentId)
                                              .FirstOrDefault(s => s.studentid == studentId);

            return Json(new
            {
                parentId = studentId,
                parentName = profile != null ? profile.FatherName : "Parent",
                ProfileImage = profile != null ? profile.FatherPhoto : "",
                activeStudentName = activeStudent != null ? activeStudent.studentname : "",
                activeStudentClass = activeStudent != null ? activeStudent.className : ""
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetChildren()
        {
            int activeStudentId = GetActiveStudentId();

            var children = parentService.GetStudentsList(activeStudentId)
                .Select(s => new
                {
                    studentId = s.studentid,
                    studentName = s.studentname,
                    className = s.className,
                    photo = s.studentImage,
                    isActive = s.studentid == activeStudentId
                })
                .ToList();

            return Json(new { status = true, data = children }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SetActiveStudent(int studentId)
        {
            int currentId = GetActiveStudentId();

            var siblings = parentService.GetStudentsList(currentId);
            bool isValidChild = siblings.Any(s => s.studentid == studentId);

            if (!isValidChild)
                return Json(new { status = false, message = "Invalid selection." });

            Session[ActiveStudentSessionKey] = studentId;
            return Json(new { status = true, message = "Student switched." });
        }
        public ActionResult Dashboard()
        {
            int studentId = GetActiveStudentId();

            ViewBag.StudentId = studentId;
            ViewBag.StudentProfile = studentService.GetStudentById(studentId);

            return View();
        }

        public ActionResult Timetable(int? userId)
        {
            ViewBag.StudentId = GetActiveStudentId();
            userId = ViewBag.StudentId;

            return View();
        }

        public ActionResult Assignment()
        {
            int studentId = GetActiveStudentId();
            var assignments = studentService.GetStudentAssignmentById(studentId);
            return View(assignments);
        }

        public ActionResult Parentcommunication()
        {
            int studentId = GetActiveStudentId();

            var CommunicationDetails = studentService.GetCommunication(studentId, 2);

            ViewBag.Communication = CommunicationDetails;

            return View();
        }

        public ActionResult Announcement()
        {
            int studentId = GetActiveStudentId();
            int adminId = studentService.GetAdminId(studentId);

            var announcements = studentService.GetAllCirculars(adminId);

            return View(announcements);
        }
        public ActionResult Result()
        {
            ViewBag.StudentId = GetActiveStudentId();
            return View();
        }
        public ActionResult Notice()
        {
            int studentId = GetActiveStudentId();
            int adminid = studentService.GetAdminId(studentId);
            var Notice = studentService.GetStudentNotices("parent", studentId, adminid);
            return View(Notice);
        }

        public ActionResult ExamSchedule()
        {
            int studentId = GetActiveStudentId();
            int adminId = studentService.GetAdminId(studentId);
            var scheduleExam = adminServices.GetScheduledExam(adminId);

            return View(scheduleExam);
        }

        [HttpGet]
        public JsonResult GetExamTimeTable(int scheduleId)
        {
            int studentId = GetActiveStudentId();

            var data = studentService.GetExamTimeTableForStudent(studentId, scheduleId);

            return Json(new
            {
                status = true,
                data = data
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Fees()
        {
            int studentId = GetActiveStudentId();

            var fee = studentService.GetFeeRecord(studentId).FirstOrDefault();

            var profile = studentService.GetStudentById(studentId);

            ViewBag.StudentName = profile?.StudentName;

            ViewBag.AcademicYear = profile?.AcademicYear;

            ViewBag.FeesDetails = fee;

            return View();
        }
        public ActionResult Hostel()
        {
            int studentId = GetActiveStudentId();

            var hostel = studentService.GetHostelDetails(studentId).FirstOrDefault();

            ViewBag.HostelDetails = hostel;

            return View();
        }
        public ActionResult Transport()
        {
            int studentId = GetActiveStudentId();

            var transport = studentService.GetTransportDetails(studentId).FirstOrDefault();

            return View(transport);
        }
        public ActionResult Profile()
        {
            int studentId = GetActiveStudentId();

            var ParentDetails = parentService.ParentProfile(studentId).FirstOrDefault();

            ViewBag.Parent = ParentDetails;

            return View();
        }
    }
}