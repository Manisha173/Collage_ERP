using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Mvc;
using System.Web.Services.Description;
using College_ERP.Models.Admin;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.Teacher;
using College_ERP.Models.Warden;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace College_ERP.Controllers
{
    [Authorize(Roles ="teacher")]
    public class TeacherController : Controller
    {
        private readonly TeacherService teacherService;
        private readonly HomeService homeService;
        private readonly AdminServices admin;
        public TeacherController()
        {
            teacherService = new TeacherService();
            homeService = new HomeService();
            admin = new AdminServices();
        }
        public ActionResult Dashboard()
        {
            int userId = teacherService.GetUserId(User.Identity.Name);
            int circularCount = teacherService.GetTotalCircularCount(userId);
            ViewBag.TotalCirculars = circularCount;
            return View();
        }
        public JsonResult GetTeacherDetails()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var teacher = teacherService.GetTeacherByTeacherId(userId);

            if (teacher != null)
            {
                var result = new
                {
                    TeacherId = teacher.TeacherId,
                    TeacherName = teacher.TeacherName,
                    ProfileImage =  teacher.profileImagePath
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAssignedSubjects()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var assignedList = teacherService.GetSubjectAssignedById(userId);

            var result = new
            {
                Date = DateTime.Now.ToString("dd-MM-yyyy"),
                Assignments = assignedList.Select(x => new
                {
                    x.ClassId,
                    x.ClassName,
                    x.SectionId,
                    x.SectionName,
                    x.SubjectId,
                    x.SubjectName
                }).ToList()
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult GetStudentsForAttendance(int ClassId, int SectionId,string academicyear)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var students = teacherService.GetStudentsByClassSection(userId,ClassId, SectionId, academicyear);
            return Json(students, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetStudentsForAttendanceReport(int ClassId, int SectionId,int subjectId,DateTime attendanceDate, string academicyear)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var students = teacherService.GetStudentsByClassSectionForReport(userId, ClassId, SectionId,subjectId, attendanceDate, academicyear);
            return Json(students, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SubmitAttendance(AttendanceSubmitModel model)
        {
            int userId = homeService.GetUserId(User.Identity.Name);

           bool res= teacherService.InsertAttendance(userId, model.ClassId, model.SubjectId, model.SectionId,model.attendanceDate, model.AttendanceList,out string error);
            if (res)
            {
                return Json(new { status = true,message="Attendence submitted successfully" });

            }
            return Json(new { status = false,message=error });
        }


        public ActionResult ClassManagement()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var assignments = teacherService.GetSubjectAssignedById(userId);
            var assignmentsWithCount = teacherService.GetSubjectAssignmentsWithStudentCount(userId);
            var combinedAssignments = (from a in assignments
                                       join ac in assignmentsWithCount
                                       on new { a.ClassId, a.SectionId, a.SubjectId }
                                       equals new { ac.ClassId, ac.SectionId, ac.SubjectId }
                                       into gj
                                       from subAc in gj.DefaultIfEmpty()
                                       select new College_ERP.Models.Teacher.SubjectAssignModel
                                       {
                                           AssignedId = a.AssignedId,
                                           ClassId = a.ClassId,
                                           ClassName = a.ClassName,
                                           SectionId = a.SectionId,
                                           SectionName = a.SectionName,
                                           SubjectId = a.SubjectId,
                                           SubjectName = a.SubjectName,
                                           StudentCount = subAc?.StudentCount ?? 0
                                       }).ToList();


            ViewBag.AssignedSubjects = combinedAssignments;
            return View(combinedAssignments);
        }

        public ActionResult StudentDetails(int classId, int sectionId)
        {
          
            var students = teacherService.GetStudents(classId, sectionId);

            return View(students);
        }

        public ActionResult TaskManagement()
        {
            int teacherId = homeService.GetUserId(User.Identity.Name);
            List<AddTaskModel> tasks = teacherService.GetTasksByTeacher(teacherId);  
            return View(tasks);
        }
        [HttpGet]
        public ActionResult GetTaskDescription(int id)
        {
            int teacherId = homeService.GetUserId(User.Identity.Name);

            // Breakpoint yahan lagao
            var allData = teacherService.GetAllCirculars(teacherId);

            var data = allData.FirstOrDefault(x => x.CircularId == id);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateTaskStatus(int id)
        {
            bool result = teacherService.UpdateTaskStatus(id);

            return Json(new { success = result });
        }

        public ActionResult TeacherProfile()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var teacherDetails = teacherService.GetTeacherByTeacherId(userId);
          
            if (teacherDetails == null)
            {               
                return View("Error");
            }
            return View(teacherDetails);
        }

        public ActionResult Circulars()
        {
            int userId = teacherService.GetUserId(User.Identity.Name);
            var circulars=teacherService.GetAllCirculars(userId);
            return View(circulars);
            
        }
     
        public ActionResult Attendance()
        {
            int teacherid = homeService.GetUserId(User.Identity.Name);
            ViewData["classList"] = teacherService.GetClassFromTeacher(teacherid);
            return View();
        }
        
        public ActionResult Assignments()
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            var data = teacherService.selectAssignment(userid);
            ViewData["classList"] = teacherService.GetClassFromTeacher(userid);
            return View(data);
        }

        [HttpPost]
        public ActionResult insertAssignment(Assignment ass)
        {
            if (!ModelState.IsValid)
            {
                string[] err = ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage).Where(e => !string.IsNullOrEmpty(e)).ToArray();
                return Json(new { status = false, message = "some error occured.", errorList= err }, JsonRequestBehavior.AllowGet);
            }
            int userid = homeService.GetUserId(User.Identity.Name);
            ass.TeacherId = userid;
            var res = teacherService.insertAssingment(ass, out string error);
            if (res)
            {
                return Json(new {status=true,message=$"Assignment {(ass.Id>0?"Updated":"Added")} Successfully." },JsonRequestBehavior.AllowGet);

            }
            return Json(new {status=false,message=error, errorList = new string[0] },JsonRequestBehavior.AllowGet);
        }

        public ActionResult StudentAssignmentList(int? classid,int? sectionid,int? assid)
        {         
            if(classid.HasValue && sectionid.HasValue)
            {
                var students = teacherService.GetStudentsForAssignment(Convert.ToInt32(classid), Convert.ToInt32(sectionid), Convert.ToInt32(assid));
                return View(students);
            }
            else
            {
                return RedirectToAction("Assignments");
            }
        }
       
        public ActionResult Exam()
        {
            return View();
        }

        //get timetable by class and section
        public JsonResult GetTimeTableByClassAndSection(int classId,int sectionId)
        {
            var data = admin.ShowAllTimeTableDetails(classId, sectionId);
            List<timetableshowModel> list = new List<timetableshowModel>();
            string[] days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            foreach (var day in days)
            {
                list.Add(new timetableshowModel { day = day, ttdata = data.Where(d => d.day == day.ToLower()).ToList() });
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AttendanceReport()
        {
            int teacherid = homeService.GetUserId(User.Identity.Name);
            ViewData["classList"] = teacherService.GetClassFromTeacher(teacherid);
            return View();
        }

        [HttpGet]
        public ActionResult GetSectionByClass(int classid)
        {
            int teacherid = homeService.GetUserId(User.Identity.Name);
            var data = teacherService.GetSectionFromTeacher(teacherid, classid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetSubjectBySection(int classid, int sectionid)
        {
            int teacherid = homeService.GetUserId(User.Identity.Name);
            var data = teacherService.GetSubjectFromTeacher(teacherid, classid, sectionid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ApplyLeave()
        {
            int teacherid = homeService.GetUserId(User.Identity.Name);
            int userId = teacherService.GetUserId(User.Identity.Name);
            ViewData["leaveRequestList"] = teacherService.GetAllLeaveRequst(userId,teacherid);
            return View();
        }
        [HttpPost]
        public ActionResult ApplyLeave(LeaveRequestModel sm)
        {
           int userId = teacherService.GetUserId(User.Identity.Name);
            int adminid = teacherService.GetAdminId(userId);
            sm.userId = adminid;
            sm.teacherId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string uniqueFileName = null;
            if (sm.attachment != null && sm.attachment.ContentLength > 0)
            {
                string fileName = sm.attachment.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                sm.attachmentName = "/Upload/" + uniqueFileName;
            }
            bool res = teacherService.InsertLeaveRequest(sm,out string error);


            if (res && uniqueFileName != null)
            {
                string filePath = Server.MapPath("~/Upload/") + uniqueFileName;
                sm.attachment.SaveAs(filePath);
            }

            return Json(new
            {
                status = res,
                message = res ? "Leave Requested Successfully": error
            });
        }
        public ActionResult TeacherCommunication()
        {
            int teacherid = homeService.GetUserId(User.Identity.Name);
            ViewData["ClassList"] =  teacherService.GetClassFromTeacher(teacherid);
            ViewData["teachercomlist"] = teacherService.GetAllTeacherCommunication(teacherid);
            return View();
        }
        [HttpPost]
        public ActionResult AddTeacherCommunication(TeacherCommunicationModel sm)
        {
            sm.teacherId = homeService.GetUserId(User.Identity.Name);
            string uniqueFileName = null;
            if (sm.Attachment != null && sm.Attachment.ContentLength > 0)
            {
                string fileName = sm.Attachment.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                sm.attachmentName = "/Upload/" + uniqueFileName;
            }
            bool res = teacherService.InsertCommunication(sm);


            if (res && uniqueFileName != null)
            {
                string filePath = Server.MapPath("~/Upload/") + uniqueFileName;
                sm.Attachment.SaveAs(filePath);
            }

            return Json(new
            {
                status = res,
                message = res ? "Communication Added Successfully" : "Some error occured"
            });
        }
        public ActionResult ShowAward()
        {
            int teacherId = homeService.GetUserId(User.Identity.Name);
            ViewData["awardlist"] = teacherService.GetAllAward(teacherId);
            return View();
        }

        public ActionResult Notice()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            int adminid = teacherService.GetAdminId(userId);
            var notices = teacherService.GetTeacherNotices("Teacher", userId,adminid);
            return View(notices);
        }
        public ActionResult ShowEvent()
        {
            int teacherid = homeService.GetUserId(User.Identity.Name);
            int userid = teacherService.GetAdminId(teacherid);
            var data = teacherService.ShowAllEventcategory(userid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetTeacherDashboardCounts()
        {
            int teacherid = homeService.GetUserId(User.Identity.Name);
            int userid = teacherService.GetAdminId(teacherid);
            var result = teacherService.GetDashboardCounts(userid, teacherid);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetTeacherPendingTasks()
        {
            int teacherId = homeService.GetUserId(User.Identity.Name);
            int userId = teacherService.GetAdminId(teacherId);
            var result = teacherService.GetTeacherPendingTasks(userId, teacherId).Take(5);      

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetTodaySchedule()
        {
            try
            {
                int teacherId = homeService.GetUserId(User.Identity.Name);
                string day = DateTime.Now.DayOfWeek.ToString(); 
                var schedule = teacherService.TodayScheduleOfTeacher(teacherId, day);

                return Json(schedule, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetUpcomingEvents()
        {
            int teacherId = homeService.GetUserId(User.Identity.Name);
            int userId = teacherService.GetAdminId(teacherId);
            var data = teacherService.ShowEvents(userId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddNotes()
        {
            int teacherid = homeService.GetUserId(User.Identity.Name);
            ViewData["classList"] = teacherService.GetClassFromTeacher(teacherid);
            var notes = teacherService.GetAllNotes(teacherid);
            return View(notes);
        }
        [HttpPost]
        public JsonResult AddOrUpdateNote(NoteModel model)
        {
            string errorMessage;
            model.UserId = homeService.GetUserId(User.Identity.Name);
            bool result = teacherService.InsertAndUpdateNotice(model, out errorMessage);

            return Json(new
            {
                success = result,
                message = result ? "Note saved successfully." : errorMessage,JsonRequestBehavior.AllowGet
            });
        }
        [HttpPost]
        public JsonResult DeleteNote(int Id)
        {
            string resultMessage = teacherService.DeleteNote(noteid: Id);

            if (resultMessage == "Success")
            {
                return Json(new { status = true, message = "Note deleted successfully!" });
            }
            else
            {
                return Json(new { status = false, message = resultMessage });
            }
        }
        [HttpGet]
        public JsonResult GetNoteById(int id)
        {
            try
            {
                var data = teacherService.GetNoteById(id); 
                if (data != null)
                {
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Note not found." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BookOrders()
        {
            int teacherId = homeService.GetUserId(User.Identity.Name);
            var bookOrders = teacherService.GetLibraryDetails(teacherId);
            return View(bookOrders);
        }
        [HttpPost]
        public ActionResult SubmitGradeOfStudent(SubmitGradeModel sg)
        {
            try
            {
                bool res = teacherService.UpdateGrade(sg);
                return Json(new
                {
                    status = res,
                    message = res ? "Grade submitted successfully" : "Some error occured"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Holidays()
        {
            int teacherId = homeService.GetUserId(User.Identity.Name);
            int userId = teacherService.GetAdminId(teacherId);
            var holidays = teacherService.GetHolidaysForAll(userId);
            return View(holidays);
        }
        public ActionResult GetSyllabus()
        {
            int teacherId = homeService.GetUserId(User.Identity.Name);
            ViewData["syllabusList"] = teacherService.GetSyllabusByClassAndSection(teacherId);
            return View();
        }
        public ActionResult ExamTimtable()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            int adminid = teacherService.GetAdminId(userId);
            var exams = new AdminServices().GetScheduledExam(adminid);
            ViewBag.academicyears = exams.Select(d => d.academicYear).Distinct();
            return View(exams);
        }
        [HttpGet]
        public ActionResult ShowTimetable(int scheduledid)
        {
            int teacherid = homeService.GetUserId(User.Identity.Name);
            var data = teacherService.GetExamTimeTableForTeacher(teacherid,scheduledid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AttendanceHistoryReport(string startDate, string endDate)
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            string empcode = teacherService.GetTeacherByTeacherId(userid).EmployeeId;
            int adminid = teacherService.GetAdminId(userid);
            var data = admin.GetAttendaceRecordByIdWithoutDept(adminid, empcode, startDate, endDate);
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return View(data);
        }
    }
}