using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using College_ERP.Models.Admin;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.StudentServices;
using College_ERP.Models.SuperAdmin;
using College_ERP.Models.Warden;
using Microsoft.Ajax.Utilities;
using static Antlr.Runtime.Tree.TreeWizard;
using static College_ERP.Models.StudentServices.main;

namespace College_ERP.ApiService
{
    public class StudentApiController : ApiController
    {
        private readonly AdminServices _admin;
        private readonly StudentServices _student;
        private readonly WardenService _warden;
        public StudentApiController()
        {
            _admin = new AdminServices();
            _student = new StudentServices();
            _warden = new WardenService();
        }
        #region Student Profile
        [Route("api/studentprofile")]
        [HttpGet]
        public IHttpActionResult GetStudentDetails(int studentid)
        {
            try
            {
                var data = _student.GetStudentById(studentid);
                return Ok(new { status = data!=null, message = data != null?"Data received.":"Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Student Assingments
        [Route("api/studentassignmets")]
        [HttpGet]
        public IHttpActionResult GetStudentAssignments(int studentid)
        {
            try
            {
                var data = _student.GetStudentAssignmentById(studentid);
                return Ok(new { status = data!=null, message = data != null?"Data received.":"Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/submitassignment")]
        [HttpPost]
        public IHttpActionResult SubmitAssignment()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var formdata = new SubmitAssignmentModel
                {
                    id = Convert.ToInt32(request.Form.Get("assignmentid")),
                    studentId = Convert.ToInt32(request.Form.Get("studentid")),
                };
                HttpPostedFile file = request.Files["attachment"];
                if (file != null)
                {
                    HttpPostedFileWrapper cfile = new HttpPostedFileWrapper(file);
                    formdata.attachment = cfile;
                }
                var result = _student.insertAssingment(formdata);
                return Ok(new { status = result, message = result ? "Assigment submitted successfully." : "Failed to submit assignment." });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Announcements
        [Route("api/studentannouncements")]
        [HttpGet]
        public IHttpActionResult GetStudentAnnouncements(int studentid)
        {
            try
            {
                int adminid = _student.GetAdminId(studentid);
                var data = _student.GetAllCirculars(adminid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Course
        [Route("api/studentcourse")]
        [HttpGet]
        public IHttpActionResult GetStudentCourse(int classid,int sectionid,int studentid)
        {
            try
            {
                int adminid = _student.GetAdminId(studentid);
                var data = _student.GetCourse(classid,sectionid,adminid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Library
        [Route("api/studentlibrarydetails")]
        [HttpGet]
        public IHttpActionResult GetLibraryDetails(int studentid)
        {
            try
            {
                var data = _student.GetLibraryDetails(studentid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region TimeTable
        [Route("api/scheduledexams")]
        [HttpGet]
        public IHttpActionResult GetScheduledExams(int studentid)
        {
            try
            {
                int adminid = _student.GetAdminId(studentid);
                var data = _admin.GetScheduledExam(adminid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/studenexamtimetable")]
        [HttpGet]
        public IHttpActionResult GetExamTimeTable(int studentid,int scheduleid)
        {
            try
            {
                var data = _student.GetExamTimeTableForStudent(studentid,scheduleid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/studenttodayschedule")]
        [HttpGet]
        public IHttpActionResult GetTodaySchedule(int classid,int sectionid,string day)
        {
            try
            {
                var data = _student.GetTodayScheduleOfStudent(classid,sectionid,day );
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/studentclasstimetable")]
        [HttpGet]
        public IHttpActionResult GetStudentTimeTable(int classid, int sectionid)
        {
            try
            {
                var data = _admin.ShowAllTimeTableDetails(classid, sectionid);
                List<timetableshowModel> list = new List<timetableshowModel>();
                string[] days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                foreach (var day in days)
                {
                    list.Add(new timetableshowModel { day = day, ttdata = data.Where(d => d.day == day.ToLower()).ToList() });
                }
                return Ok(new { status = true, message = "Data received.", data = list });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Notice
        [Route("api/studentnotice")]
        [HttpGet]
        public IHttpActionResult GetNotice(int studentid)
        {
            try
            {
                int adminid = _student.GetAdminId(studentid);
                var data = _student.GetStudentNotices("student", studentid, adminid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region TransPort

        [Route("api/transportdetail")]
        [HttpGet]
     
        public IHttpActionResult GetTransportDetails(int studentid)
        {
            try
            {
                if (studentid <= 0)
                {
                    return BadRequest("Invalid student id.");
                }

                var data = _student.GetTransportDetails(studentid);

                if (data == null || data.Count == 0)
                {
                    return Content(System.Net.HttpStatusCode.NotFound, new
                    {
                        status = false,
                        message = "Transport details not found for this student."
                    });
                }

                return Ok(new
                {
                    status = true,
                    message = "Data received.",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion
        #region Communication
        [Route("api/communcation")]
        [HttpGet]
        public IHttpActionResult GetCommunication(int studentid)
        {
            try
            {
                var data = _student.GetCommunication(studentid,1);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/wardencommuncation")]
        [HttpGet]
        public IHttpActionResult GetWardenCommunication(int studentid)
        {
            try
            {
                var data = _student.GetWardenCommunication(studentid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Student Fee
        [Route("api/studentfee")]
        [HttpGet]
        public IHttpActionResult GetStudentFee(int studentid)
        {
            try
            {
                var data = _student.GetFeeRecord(studentid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Hostel Manage
        [Route("api/studenthosteldetails")]
        [HttpGet]
        public IHttpActionResult GetHostelDetails(int studentid)
        {
            try
            {
                var data = _student.GetHostelDetails(studentid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/mealschedule")]
        [HttpGet]
        public IHttpActionResult GetHostelMealSchedule(string day,int studentid)
        { 
            try
            {
                var data = _student.GetMealSchedule(day,studentid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/weeklymealschedule")]
        [HttpGet]
        public IHttpActionResult GetHostelWeeklyMealSchedule(int studentid)
        { 
            try
            {
                int wardenid = _student.GetWardenId(studentid);
                List<MealViewModel> res = _warden.GetAllMeals(wardenid);
                var newMeal = res.Select(e => new
                {
                    day = e.Day,
                    details = res.Where(d => d.Day == e.Day).Select(f => new
                    {
                        startTime = f.StartTime,
                        endTime = f.EndTime,
                        createdDate = f.CreatedDate,
                        menus = f.Menus
                    })
                }).DistinctBy(e => e.day);
                return Ok(new { status = true, data = newMeal, message = "data retrieved" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Attendance
        [Route("api/studentattendance")]
        [HttpGet]
        public IHttpActionResult GetAttendanceReport(int studentid,int classid,int sectionid)
        {
            try
            {
                var data = _student.GetAttendanceReport(studentid,classid,sectionid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Hostel Problem Management
        [Route("api/addhostelproblem")]
        [HttpPost]
        public IHttpActionResult InsertHostelProblem()
        {
            try
            {
                string error = "";
                var request = HttpContext.Current.Request;
                var formdata = new HostelProblemModel
                {
                    studentid = Convert.ToInt32(request.Form.Get("studentid")),
                    problem = request.Form.Get("problem").ToString()
                };
                bool res = _student.InsertHostelProblem(formdata,out error);
                return Ok(new { status = res, message = res ? "Problem Added Successfully" : error });
            }
            catch
            {
                return Ok(new { status = false, message = "Server error occured" });
            }
        }
        [Route("api/hostelproblems")]
        [HttpGet]
        public IHttpActionResult GetStudentHostelProblems(int studentid)
        {
            try
            {
                var data = _student.GetHostelProblem(studentid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Exam
        [Route("api/GetStudentMarks")]
        [HttpGet]
        public IHttpActionResult GetStudentMarks(int studentId,int examId)
        {
            try
            {
                var data = _student.GetStudentMarks(studentId, examId);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/GetReportCard")]
        [HttpGet]
        public IHttpActionResult GetReportCard(int studentId, int examId,string academicYear)
        {
            try
            {

                var data = _admin.GetStudentReportCard(studentId, academicYear,examId, out double totalpercentage);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found",overallPercentage=totalpercentage, data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
    }
}