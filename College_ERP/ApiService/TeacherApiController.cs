using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Http;
using Antlr.Runtime.Tree;
using College_ERP.Models.Admin;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.SuperAdmin;
using College_ERP.Models.Teacher;
using Newtonsoft.Json;
using PdfSharp.Charting;
using static System.Collections.Specialized.BitVector32;

namespace College_ERP.ApiService
{
    public class TeacherApiController : ApiController
    {
        private readonly AdminServices _admin;
        private readonly SuperAdminDataService _superAdmin;
        private readonly HomeService _home;
        private readonly TeacherService _teacher;
        public TeacherApiController()
        {
            _admin = new AdminServices();
            _home = new HomeService();
            _superAdmin = new SuperAdminDataService();
            _teacher = new TeacherService();
        }

        [Route("api/teacherProfile")]
        [HttpGet]
        public IHttpActionResult TeacherProfile(int id)
        {
            try
            {
                var data = _admin.GetTeachersByTeacherId(id);
                return Ok(new { status = true, message = "Data received.", data = data });
            }catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/circular")]
        [HttpGet]
        public IHttpActionResult Circular(int id)
        {
            try
            {
                string username = _admin.usernameByuserid(id, "teacher");
                int userid=_teacher.GetUserId(username);
                var data = _teacher.GetAllCirculars(userid);
                return Ok(new { status = true, message = "Data received.", data = data });
            }catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/getAssignedSubject")]
        [HttpGet]
        public IHttpActionResult GetSubjectAssigned(int id)
        {
            try
            {
                var assignments = _teacher.GetSubjectAssignedById(id);
                var assignmentsWithCount = _teacher.GetSubjectAssignmentsWithStudentCount(id);
                var combinedAssignments = assignments.Join(assignmentsWithCount,
                    a => new { a.ClassId, a.SectionId, a.SubjectId },
                    ac => new { ac.ClassId, ac.SectionId, ac.SubjectId },
                    (a, ac) => new College_ERP.Models.Teacher.SubjectAssignModel
                    {
                        AssignedId = a.AssignedId,
                        ClassId = a.ClassId,
                        ClassName = a.ClassName,
                        SectionId = a.SectionId,
                        SectionName = a.SectionName,
                        SubjectId = a.SubjectId,
                        SubjectName = a.SubjectName,
                        StudentCount = ac.StudentCount
                    }).ToList();
                return Ok(new { status = true, message = "Data received.", data = combinedAssignments });
            }catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/getStudentForAttendence")]
        [HttpGet]
        public IHttpActionResult GetStudentsByClassSection(int userid, int classId, int sectionId, string academicyea)
        {
            try
            {
             
                var data = _teacher.GetStudentsByClassSection( userid,  classId,  sectionId, academicyea);
                return Ok(new { status = true, message = "Data received.", data = data });
            }catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/getAttendanceReport")]
        [HttpGet]
        public IHttpActionResult GetStudentsByClassSectionForReport(int userid, int classId, int sectionId, int subjectId, DateTime attendanceDate, string academicyear)
        {
            try
            {
             
                var data = _teacher.GetStudentsByClassSectionForReport( userid,  classId,  sectionId,  subjectId,  attendanceDate,  academicyear);
                return Ok(new { status = true, message = "Data received.", data = data });
            }catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/getStudent")]
        [HttpGet]
        public IHttpActionResult GetStudents( int classId, int sectionId)
        {
            try
            {
                var data = _teacher.GetStudents( classId,  sectionId);
                return Ok(new { status = true, message = "Data received.", data = data });
            }catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/insertAttendace")]
        [HttpPost]
        public IHttpActionResult InsertAttendance()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                int userId = Convert.ToInt32(httpRequest.Form.Get("userid"));
                int classId = Convert.ToInt32(httpRequest.Form.Get("classId"));
                int subjectId = Convert.ToInt32(httpRequest.Form.Get("subjectId"));
                int sectionId = Convert.ToInt32(httpRequest.Form.Get("sectionId"));
                DateTime attendanceDate = Convert.ToDateTime(httpRequest.Form.Get("attendanceDate"));
                string attendanceJson = httpRequest.Form.Get("attendanceList");
                List<StudentAttendance> attendanceList = JsonConvert.DeserializeObject<List<StudentAttendance>>(attendanceJson);
                bool res = _teacher.InsertAttendance( userId,  classId,  subjectId,  sectionId,  attendanceDate, attendanceList, out string error);
                if (res)
                {

                return Ok(new { status = true, message = "Attendance Added Successfully." });
                }
                else
                {
                    return Ok(new { status = false, message = error});
                }
            }catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getClass")]
        [HttpGet]
        public IHttpActionResult GetClass(int teacherid)
        {
            try
            {
                var data = _teacher.GetClassFromTeacher(teacherid);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getSection")]
        [HttpGet]
        public IHttpActionResult GetSection(int teacherid, int classid)
        {
            try
            {
                var data = _teacher.GetSectionFromTeacher(teacherid, classid);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getSubject")]
        [HttpGet]
        public IHttpActionResult GetSubject(int teacherid, int classid, int sectionid)
        {
            try
            {
                var data = _teacher.GetSubjectFromTeacher(teacherid, classid, sectionid);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/GetTimeTableByClassSection")]
        [HttpGet]
        public IHttpActionResult GetTimeTableByClassAndSection( int classid, int sectionid)
        {
            try
            {
                var data = _admin.ShowAllTimeTableDetails( classid, sectionid);
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

        #region Task
        [Route("api/GetTask")]
        [HttpGet]
        public IHttpActionResult GetTask(int teacherId, string search = null)
        {
            try
            {
                var data = _teacher.GetTasksByTeacher(teacherId, search);

                return Ok(new
                {
                    status=true,
                    data=data,
                    message="data retrieved"
                });
            }catch(Exception ex)
            {
                return Ok(new
                {
                    status=false,
                    message=ex.Message
                });
            }
            }

        [Route("api/UpdateTaskStatus")]
        [HttpGet]
        public IHttpActionResult UpdateTaskStatus(int taskId)
        {
            try
            {
                bool data = _teacher.UpdateTaskStatus(taskId);
                if (data)
                {
                    return Ok(new
                    {
                        status = true,
                        message = "Task Updated Successfully"
                    });
                }
                return Ok(new
                {
                    status=false,
                    message="Something went wrong"
                });
            }catch(Exception ex)
            {
                return Ok(new
                {
                    status=false,
                    message=ex.Message
                });
            }
            }

        [Route("api/InsertOrUpdateAssignment")]
        [HttpPost]
        public IHttpActionResult InsertOrUpdateAssingment()
        {
            try
            {
                var httprequest = HttpContext.Current.Request;
                HttpPostedFile Attachment = httprequest.Files["Attachment"];
                Assignment assg = new Assignment
                {
                    Id= Convert.ToInt32(httprequest.Form.Get("Id") ?? "0"),
                    TeacherId= Convert.ToInt32(httprequest.Form.Get("teacherId") ?? "0"),
                    ClassId=Convert.ToInt32(httprequest.Form.Get("classId")?? "0"),
                    SectionId= Convert.ToInt32(httprequest.Form.Get("sectionId") ?? "0"),
                    SubjectId=Convert.ToInt32(httprequest.Form.Get("subjectId") ?? "0"),
                    AcademicYear=httprequest.Form.Get("academicyear"),
                    Title=httprequest.Form.Get("title"),
                    Description=httprequest.Form.Get("description"),
                    CompletionDate=Convert.ToDateTime(httprequest.Form.Get("CompletionDate")),

                };

                if(Attachment!=null || Attachment?.ContentLength > 0)
                {
                    assg.Attachment = new HttpPostedFileWrapper(Attachment);
                }
                bool data = _teacher.insertAssingment(assg,out string error);
                if (data)
                {
                    return Ok(new
                    {
                        status = true,
                        message = $"Assignment {(assg.Id>0?"Updated":"Added")} Successfully"
                    });
                }
                return Ok(new
                {
                    status=false,
                    message=error
                });
            }catch(Exception ex)
            {
                return Ok(new
                {
                    status=false,
                    message=ex.Message
                });
            }
            }

        [Route("api/SelectAssignment")]
        [HttpGet]
        public IHttpActionResult SelectAssignment(int teacherId)
        {
            try
            {
                var data = _teacher.selectAssignment(teacherId);
               
                    return Ok(new
                    {
                        status = true,
                        data=data,
                        message = "data retrieved"
                    });
                
            }catch(Exception ex)
            {
                return Ok(new
                {
                    status=false,
                    message=ex.Message
                });
            }
            }


        [Route("api/GetStudentSubmittedAssignment")]
        [HttpGet]
        public IHttpActionResult GetStudentAssignments(int assignmentId)
        {
            try
            {
                var data = _teacher.GetStudentAssignments(assignmentId);
               
                    return Ok(new
                    {
                        status = true,
                        data=data,
                        message = "data retrieved"
                    });
                
            }catch(Exception ex)
            {
                return Ok(new
                {
                    status=false,
                    message=ex.Message
                });
            }
            }

        #endregion

        #region Leave
        [Route("api/applyLeave")]
        [HttpPost]
        public IHttpActionResult InsertLeave()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var attachment = request.Files["attachment"];
                string uniqueFileName = null;
                string attachmentName = null;
                if (attachment != null && attachment.ContentLength > 0)
                {
                    string fileName = attachment.FileName;
                    uniqueFileName = Guid.NewGuid() + "_" + fileName;
                    attachmentName = "/Upload/" + uniqueFileName;
                }
                var leave = new LeaveRequestModel
                {
                    teacherId = Convert.ToInt32(request.Form.Get("teacherId")),  
                    userId = _teacher.GetAdminId(Convert.ToInt32(request.Form.Get("teacherId"))),
                    leaveType = request.Form.Get("leaveType"),
                    fromDate = Convert.ToDateTime(request.Form.Get("fromDate")),
                    toDate = Convert.ToDateTime(request.Form.Get("toDate")),
                    reason = request.Form.Get("reason"),
                    attachmentName = attachmentName,    
                };
                bool res = _teacher.InsertLeaveRequest(leave, out string error);
                if (res && uniqueFileName != null)
                {
                    string filePath =HttpContext.Current.Server.MapPath("~/Upload/") + uniqueFileName;
                    attachment.SaveAs(filePath);
                }
                return Ok(new
                {
                    status = res,
                    StatusCode = res ? 200 : 400,
                    MessageProcessingHandler = res ? "Leave Request Submitted Successfully" : error
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }   
        }
        [Route("api/getLeaveRequest")]
        [HttpGet]
        public IHttpActionResult GetLeaveRequest(int teacherId, string search = null)
        {
            try
            {
                int adminId = _teacher.GetAdminId(teacherId);
                var data = _teacher.GetAllLeaveRequst(adminId, teacherId, search);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Communication
        [Route("api/insertCommunication")]
        [HttpPost]
        public IHttpActionResult InsertCommunication()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var attachment = request.Files["attachment"];
                string uniqueFileName = null;
                string attachmentName = null;
                if (attachment != null && attachment.ContentLength > 0)
                {
                    string fileName = attachment.FileName;
                    uniqueFileName = Guid.NewGuid() + "_" + fileName;
                    attachmentName = "/Upload/" + uniqueFileName;
                }
                var data = new TeacherCommunicationModel
                {
                    teacherId = Convert.ToInt32(request.Form.Get("teacherId")),
                    IsSendTo = _teacher.GetAdminId(Convert.ToInt32(request.Form.Get("isSendTo"))),
                    StudentName = Convert.ToInt32(request.Form.Get("studentId")),
                    title = request.Form.Get("title").ToString(),
                    description = request.Form.Get("description").ToString(),
                    attachmentName = attachmentName,
                };
                bool res = _teacher.InsertCommunication(data);
                if (res && uniqueFileName != null)
                {
                    string filePath = HttpContext.Current.Server.MapPath("~/Upload/") + uniqueFileName;
                    attachment.SaveAs(filePath);
                }
                return Ok(new
                {
                    status = res,
                    StatusCode = res ? 200 : 400,
                    MessageProcessingHandler = res ? "Communication Added Successfully" : "Server error occured"
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/getCommunication")]
        [HttpGet]
        public IHttpActionResult GetCommunication(int teacherId, string search = null)
        {
            try
            {
                var data = _teacher.GetAllTeacherCommunication(teacherId, search);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Award
        [Route("api/getAward")]
        [HttpGet]
        public IHttpActionResult GetAward(int teacherId, string search = null)
        {
            try
            {
                var data = _teacher.GetAllAward(teacherId, search);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion

        #region Event 
        [Route("api/getEvent")]
        [HttpGet]
        public IHttpActionResult GetEvent(int teacherid, string search = null)
        {
            try
            {
                int adminid = _teacher.GetAdminId(teacherid);
                var data = _teacher.ShowAllEventcategory(adminid, search);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Schedule Of Teacher
        [Route("api/gettodayschedule")]
        [HttpGet]
        public IHttpActionResult GetTodaySchedule(int teacherid,string day, string search = null)
        {
            try
            {
                var data = _teacher.TodayScheduleOfTeacher(teacherid, day, search);
                string[] days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getfullweakschedule")]
        [HttpGet]
        public IHttpActionResult GetFullWeakSchedule(int teacherid, string search = null)
        {
            try
            {
                var data = _teacher.FullWeakSchedule(teacherid, search);
                List<FullWeakScheduleModel> list = new List<FullWeakScheduleModel>();
                string[] days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                foreach (var day in days)
                {
                    list.Add(new FullWeakScheduleModel { day = day, ttdata = data.Where(d => d.day == day.ToLower()).ToList() });
                }
                return Ok(new { status = true, message = "Data received.", data = list });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region  exam & syllabus
        [Route("api/getexamtimetable")]
        [HttpGet]
        public IHttpActionResult GetExamTimeTable(int teacherid,int scheduledid, string search = null)
        {
            try
            {
                var data = _teacher.GetExamTimeTableForTeacher(teacherid, scheduledid, search);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getscheduledexam")]
        [HttpGet]
        public IHttpActionResult GetScheduledExam(int teacherid, string search = null)
        {
            try
            {
                int adminid = _teacher.GetAdminId(teacherid);
                var data = new AdminServices().GetScheduledExam(adminid,search);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getsyllabus")]
        [HttpGet]
        public IHttpActionResult GetSyllabus(int teacherid, string search = null)
        {
            try
            {
                var data = _teacher.GetSyllabusForteacher(teacherid, search);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getlibrarydetails")]
        [HttpGet]
        public IHttpActionResult GetLibraryDetails(int teacherid, string search = null)
        {
            try
            {
                var data = _teacher.GetLibraryDetails(teacherid, search);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Notice
        [Route("api/getnotice")]
        [HttpGet]
        public IHttpActionResult Notice(int teacherid, string search = null)
        {
            try
            {
                int adminid = _teacher.GetAdminId(teacherid);
                var data = _teacher.GetTeacherNotices("Teacher",teacherid,adminid, search);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Notes
        [Route("api/insertandupdatenotes")]
        [HttpPost]
        public IHttpActionResult InsertNotes()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var formdata = new NoteModel
                {
                    NoteId = Convert.ToInt32(request.Form.Get("noteId")),
                    UserId = Convert.ToInt32(request.Form.Get("teaherId")),
                    ClassId = Convert.ToInt32(request.Form.Get("classId")),
                    SectionId = Convert.ToInt32(request.Form.Get("sectionId")),
                    SubjectId = Convert.ToInt32(request.Form.Get("subjectId")),
                    AcademicYear = request.Form.Get("academicYear").ToString(),
                };
                HttpPostedFile file = request.Files["attachment"];
                if (file != null)
                {
                    HttpPostedFileWrapper cfile = new HttpPostedFileWrapper(file);
                    formdata.Attachment = cfile;
                }
                bool res = _teacher.InsertAndUpdateNotice(formdata,out string error);
                return Ok(new
                {
                    status = res,
                    StatusCode = res ? 200 : 400,
                    message = res && formdata.NoteId>0?"Notes Updated Successfully":res && formdata.NoteId==0 ? "Data Added Successfully" :error
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/getnotes")]
        [HttpGet]
        public IHttpActionResult GetNotes(int teacherid, string search = null)
        {
            try
            {
                var data = _teacher.GetAllNotes(teacherid,search);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/deletenotes")]
        [HttpPost]
        public IHttpActionResult DeleteNotes(int id)
        {
            try
            {
                string res = _teacher.DeleteNote(id);
                return Ok(new
                {
                    status = res == "Success" ? true : false,
                    StatusCode = res == "Success" ? 200 : 400,
                    message = res == "Success" ? "Notes Deleted Successfully" : "Some issue occured"
                });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion

        #region attendenceHistory
        [Route("api/GetAttendaceRecordById")]
        [HttpGet]
        public IHttpActionResult GetAttendaceRecordById(string empcode,int userid,string startDate, string endDate)
        {
            try
            {
                int adminid = _teacher.GetAdminId(userid);
                var data = _admin.GetAttendaceRecordByIdWithoutDept(adminid, empcode, startDate, endDate);
                return Ok(new { status = true, message = "Data received.", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion


        #region holiday
        [Route("api/GetHolidayByUserid")]
        [HttpGet]
        public IHttpActionResult GetHolidateByUserid(int userid)
        {
            try
            {
                int userId = _teacher.GetAdminId(userid);
                var holidays = _teacher.GetHolidaysForAll(userId);
                return Ok(new
                {
                    status = true,
                    message = "Holiday List!!",
                    Holiday = holidays
                });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        #endregion

        [Route("api/teacherdashboard-count")]
        [HttpGet]
        public IHttpActionResult teacherdashboardcount(int teacherid)
        {
            try
            {
                int userId = _teacher.GetAdminId(teacherid);
                var result = _teacher.GetDashboardCounts(userId, teacherid);
                return Ok(new
                {
                    status = true,
                    message = "teacher dashboard count!",
                    data = result
                });
            }
            catch(Exception ex)
            {
                return Ok(new { Status = false, message = ex.Message });
            }
        }


    }
}
