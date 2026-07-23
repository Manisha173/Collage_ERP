
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Antlr.Runtime.Tree;
using College_ERP.Models.Admin;
using College_ERP.Models.AdminServices;
using College_ERP.Models.customeFilter;
using College_ERP.Models.DriverServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.Security;
using College_ERP.Models.SuperAdmin;
using ExcelDataReader;
using Microsoft.Ajax.Utilities;
using static System.Net.Mime.MediaTypeNames;
using static College_ERP.Models.AdminServices.AdminServices;


namespace College_ERP.Controllers
{
    [manageLogUrl]
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        AdminServices adminservices = new AdminServices();
        HomeService homeService = new HomeService();
        SecurityService _securityService = new SecurityService();
        SuperAdminDataService superAdminDataService = new SuperAdminDataService();
        DriverService driverService = new DriverService();

        public ActionResult Dashboard()
        {
            int id = homeService.GetUserId(User.Identity.Name);
            ViewBag.yearWise = adminservices.GetStudentYearWise(id);
            ViewBag.classWise = adminservices.selectStudentClassWise(id);
            ViewBag.blockWise = adminservices.selectStudentBlockWise(id);
            return View();
        }
        [HttpGet]
        public JsonResult GetAdminDetails()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var admin = adminservices.GetAdminDetails(userId);

            if (admin != null)
            {
                var result = new
                {
                    adminId = admin.adminId,
                    adminName = admin.adminName,
                    adminImage = admin.adminImage
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult StudentAward()
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewBag.ClassList = adminservices.GetAllClasses(userid);
            ViewBag.SectionList = adminservices.GetAllSections(userid);
            ViewBag.StudentList = adminservices.GetAllStudents(userid);
            var award = adminservices.GetAllStudentAwards(userid);
            return View(award);

        }
        [HttpPost]
        public ActionResult StudentAward(StudentAwardModel model, HttpPostedFileBase certificate)
        {
            if (certificate != null && certificate.ContentLength > 0)
            {
                string filename = Path.GetFileName(certificate.FileName);
                string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
                string fileExtension = Path.GetExtension(filename)?.ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["msg"] = "Invalid file type. Only PDF, JPG, JPEG, and PNG files are allowed.";
                    TempData["msgType"] = "error";
                    return RedirectToAction("StudentAward");
                }

                string path = Path.Combine(Server.MapPath("~/Upload/"), filename);
                certificate.SaveAs(path);
                model.CertificatePath = filename;
            }

            model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string result = adminservices.InsertStudentAward(model);

            TempData["msg"] = result;

            TempData["msgType"] = result.Contains("successfully") ? "success" : "error";

            return RedirectToAction("StudentAward");
        }


        [HttpPost]
        public JsonResult DeleteStudentAward(int AwardId)
        {
            string resultMessage = adminservices.DeleteStudentAward(awardid: AwardId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Award deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        public JsonResult GetStudentAwardById(int id)
        {
            var awards = adminservices.GetStudentAwardById(id);
            var award = awards.FirstOrDefault();

            if (award != null)
            {
                return Json(new
                {
                    award.AwardId,
                    award.StudentId,
                    award.AwardTitle,
                    award.ClassId,
                    award.SectionId,
                    award.Session,
                    AwardDate = award.AwardDate.ToString("yyyy-MM-dd"),
                    award.AwardType,
                    award.Description,
                    award.CertificatePath
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateStudentAward(StudentAwardModel model)
        {
            if (ModelState.IsValid)
            {
                var res = adminservices.UpdateStudentAward(model);
                if (res)
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }




        public ActionResult TeacherAward()
        {
            int id = homeService.GetUserId(User.Identity.Name);

            List<TeacherModel> teachers = adminservices.GetAllTeachers(id);
            ViewData["teachers"] = teachers;
            ViewBag.TeacherList = teachers;
            List<TeacherAwardModel> list = new List<TeacherAwardModel>();
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            list = adminservices.GetAllTeacherAwards(userid);
            return View(list);

        }
        [HttpPost]
        public JsonResult TeacherAward(TeacherAwardModel model)
        {
            try
            {
                model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                string result = adminservices.InsertTeacherAward(model);

                bool isSuccess = result.Contains("successfully");

                return Json(new
                {
                    success = isSuccess,
                    message = result
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        [HttpPost]
        public JsonResult DeleteTeacherAward(int AwardId)
        {
            try
            {

                string result = adminservices.DeleteTeacherAward(AwardId);

                if (result == "Success")
                {
                    return Json(new { success = true, message = "Award deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete award." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public JsonResult GetTeacherAwardById(int id)
        {
            var awards = adminservices.GetTeacherAwardById(id);
            var award = awards.FirstOrDefault();

            if (award != null)
            {
                return Json(new
                {
                    award.awardid,
                    award.TeacherId,
                    award.teacherName,
                    award.awardTitle,
                    award.awardSession,
                    awardDate = award.awardDate.ToString("yyyy-MM-dd"),
                    award.awardType,
                    award.awardDesc,
                    award.awardcertificate
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateTeacherAward(TeacherAwardModel model)
        {
            string result = adminservices.UpdateTeacherAward(model);
            bool isSuccess = result.Contains("successfully");

            return Json(new
            {
                success = isSuccess,
                message = result
            });
        }



        public ActionResult SchoolAward()
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var data = adminservices.GetAllSchoolAwards(userid);
            return View(data);
        }

        [HttpPost]
        public ActionResult SchoolAward(AwardModel model, HttpPostedFileBase certificate)
        {
            if (certificate != null && certificate.ContentLength > 0)
            {
                string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
                string ext = Path.GetExtension(certificate.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                {
                    return Content("Invalid file type. Only PDF, JPG, JPEG, and PNG are allowed.");
                }

                string filename = Path.GetFileName(certificate.FileName);
                string path = Path.Combine(Server.MapPath("~/Upload/"), filename);
                certificate.SaveAs(path);
                model.AwardCertificate = filename;
            }

            model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string result = adminservices.InsertSchoolAward(model);

            return Content(result);
        }


        public JsonResult DeleteAward(int id)
        {
            var result = adminservices.DeleteSchoolAward(id);
            return Json(new { success = result }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSchoolAwardById(int id)
        {
            var awards = adminservices.GetSchoolAwardById(id);
            var award = awards.FirstOrDefault();

            if (award != null)
            {
                return Json(new
                {
                    award.AwardId,
                    award.AwardName,
                    award.AwardTitle,
                    AwardDate = award.AwardDate.ToString("yyyy-MM-dd"),

                    award.AwardDescription,
                    award.AwardCertificate
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateSchoolAward(AwardModel model)
        {
            string result = adminservices.UpdateSchoolAward(model);
            bool isSuccess = result.Contains("successfully");

            return Json(new
            {
                success = isSuccess,
                message = result
            });
        }



        public ActionResult AttendanceManagement()
        {
            return View();
        }
        public ActionResult AddClass()
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<ClassModel> list = adminservices.GetAllClasses(userid);

            return View(list);

        }
        [HttpPost]
        public JsonResult AddClass(ClassModel cs)
        {
            try
            {
                cs.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                bool result = adminservices.InsertClass(cs,out string msg);
                
                    return Json(new { success = result, message = msg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpGet]
        public ActionResult GetEducationLevelByInstitution(string institution)
        {
            var data = adminservices.GetEducationLevel(institution);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetClassByEducationLevel(string educationlevel)
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            var data = adminservices.GetClassByEducationLevel(educationlevel,userid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetStreamByClassId(int classid)
        {
            var data = adminservices.GetStreamByClassId(classid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteClass(int ClassId)
        {
            string resultMessage = adminservices.DeleteClass(classid: ClassId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Class deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        [HttpGet]
        public JsonResult GetClassById(int id)
        {
            try
            {
                var classData = adminservices.GetClassById(id);
                return Json(classData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateClass(int classId, string className, string classDescription)
        {
            if (classId > 0)
            {
                var res = adminservices.UpdateClass(classId, className, classDescription);
                if (res == "Success")
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }




        public ActionResult AddSection()
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewBag.ClassList = adminservices.GetAllClasses(userid);
            var sections = adminservices.GetAllSections(userid);
            return View(sections);

        }

        [HttpGet]
        public JsonResult GetSectionByClassId(int id)
        {
            var data = adminservices.GetSectionsByClassId(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddSection(int classId, string sectionName, string sectionDescription, int? classStreamId)
        {

            try
            {
                int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                var result = adminservices.InsertSection(classId, sectionName, Convert.ToInt32(classStreamId), sectionDescription, userid);

                if (result == "Success")
                {
                    return Json(new { success = true, message = "Section added successfully." },JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add class." },JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message },JsonRequestBehavior.AllowGet);
            }


        }

        [HttpPost]
        public JsonResult DeleteSection(int SectionId)
        {
            string resultMessage = adminservices.DeleteSection(sectionid: SectionId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Section deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        [HttpGet]
        public JsonResult GetSectionById(int id)
        {
            try
            {
                var sectionData = adminservices.GetSectionById(id);
                return Json(sectionData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateSection(int sectionId, int classId, string sectionName, string sectionDescription)
        {
            if (classId > 0)
            {
                var res = adminservices.UpdateSection(sectionId, classId, sectionName, sectionDescription);
                if (res == "Success")
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }
        public ActionResult AddDesignation()
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<DesignationModel> list = adminservices.GetAllDesignations(userid);

            return View(list);

        }
        [HttpPost]
        public JsonResult AddDesignation(string DesignationName, string DesignationDescription)
        {
            {
                try
                {
                    int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                    var result = adminservices.InsertDesignation(DesignationName, DesignationDescription, userid);

                    if (result == "Success")
                    {
                        return Json(new { success = true, message = "Designation added successfully." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to add class." });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
        }

        public JsonResult DeleteDesignation(int DesignationId)
        {
            string resultMessage = adminservices.DeleteDesignation(Designationid: DesignationId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Designation deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        [HttpGet]
        public JsonResult GetDesignationById(int id)
        {
            try
            {
                var designationData = adminservices.GetDesignationById(id);
                return Json(designationData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateDesignation(int DesignationId, string DesignationName, string DesignationDescription)
        {
            if (DesignationId > 0)
            {
                var res = adminservices.UpdateDesignation(DesignationId, DesignationName, DesignationDescription);
                if (res == "Success")
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }

        [HttpGet]
        public ActionResult AddSubject(int? subId)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<ClassModel> classList = adminservices.GetAllClasses(userid);
            var data = adminservices.GetSubjectById(subId);
            if (data != null)
            {
                ViewData["dataById"] = data;
            }
            ViewData["classlist"] = classList;
            ViewBag.ClassList = adminservices.GetAllClasses(userid);
            var subjects = adminservices.GetAllSubject(userid);
            return View(subjects);
        }
        [HttpPost]
        public JsonResult AddSubject(SubjectModel model)
        {
            try
            {
                    model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                    bool res = adminservices.InsertSubject(model,out string errormsg);
                    return Json(new { success = res, message =res? "Subject Registered Successfully!": errormsg });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
               
            } 

        }

       
        [HttpPost]
        public JsonResult DeleteSubject(int SubjectId)
        {
            string resultMessage = adminservices.DeleteSubject(subjectid: SubjectId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Subject deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        [HttpGet]
        public JsonResult GetSubjectById(int id)
        {
            try
            {
                var feedata = adminservices.GetSubjectById(id);
                var singleFee = feedata.FirstOrDefault();
                return Json(singleFee, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult UpdateSubject(SubjectModel model)
        {
            if (ModelState.IsValid)
            {
                adminservices.UpdateSubject(model);

                return Json(new { success = true });

            }
            return Json(new { success = false });
        }

        public ActionResult AddCirculars()
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var circulars = adminservices.GetAllCirculars(userid);
            return View(circulars);

        }
        [HttpPost]

        public JsonResult AddCirculars(CircularModel model)
        {
            try
            {
                model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                model.UploadAttachment = adminservices.UploadImageToServer(model.Attachment);
                bool res = adminservices.InsertCircular(model, out string error);
                if (res)
                {

                    return Json(new { success = true, message = "Circular Added Successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = error });

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteCircular(int CircularId)
        {
            string resultMessage = adminservices.DeleteCircular(circularid: CircularId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Circular deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        public JsonResult GetCircularById(int id)
        {
            var circulars = adminservices.GetCircularById(id);
            var circular = circulars.FirstOrDefault();

            if (circular != null)
            {
                return Json(new
                {
                    circular.CircularId,
                    circular.CircularTitle,
                    CircularDate = circular.CircularDate.ToString("yyyy-MM-dd"),
                    circular.CircularDescription,
                    circular.UploadAttachment,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateCircular(CircularModel model)
        {
            if (ModelState.IsValid)
            {
                var res = adminservices.UpdateCircular(model);
                if (res)
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }


        public ActionResult AddTeacherList()
        {
            int id = homeService.GetUserId(User.Identity.Name);
            var teachers = adminservices.GetAllTeachers(id);
            return View(teachers);
        }
        [HttpGet]
        public ActionResult SubjectAssign(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("addteacherlist");
            }
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<ClassModel> classList = adminservices.GetAllClasses(userid);
            ViewData["classlist"] = classList;
            ViewData["teacherid"] = id;
            ViewBag.ClassList = classList;
            List<SectionModel> sectionList = adminservices.GetAllSections(userid);
            ViewData["sectionlist"] = sectionList;
            List<SubjectModel> subjectList = adminservices.GetAllSubject(userid);
            ViewData["subjectlist"] = subjectList;

            //var data = adminservices.GetSubjectAssignedById(id);

            var subjectsassign = adminservices.GetAllSubjectAssigned((int)id);
            return View(subjectsassign);


        }
        [HttpGet]
        public JsonResult GetSubjectandSectionByClassId(int id)
        {
            var subjects = adminservices.GetSubjectsByClassId(id);
            var sections = adminservices.GetSectionsByClassId(id);
            return Json(new
            {
                subject = subjects,
                section = sections
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSubjectsByClassId(int classId, int teacherId)
        {
            try
            {
                var subjects = adminservices.GetSubjectsByClassId(teacherId, classId);
                return Json(subjects, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult GetSectionsByClassId(int id)
        {
            try
            {
                var subjects = adminservices.GetSectionsByClassId(id);
                return Json(subjects, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SubjectAssign(List<SubjectAssignModel> list)
        {
            if (list == null || list.Count == 0)
            {
                return Json(new { success = false, message = "No assignments submitted." });
            }

            try
            {

                foreach (var item in list)
                {
                    item.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                    item.teacherId = Convert.ToInt32(Request.QueryString["id"]);
                    if (item.userid == 0 || item.ClassId == 0 || item.SectionId == 0 || item.SubjectId == 0)
                    {
                        return Json(new { success = false, message = "Invalid data in one or more entries." });
                    }

                    adminservices.InsertSubjectAssigned(item);
                }

                return Json(new { success = true, message = "Subject assignments saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        [HttpPost]
        public JsonResult DeleteSubjectAssigned(int AssignedId)
        {
            string resultMessage = adminservices.DeleteSubjectAssigned(assignid: AssignedId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Subject deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        public JsonResult GetSubjectAssignedById(int id)
        {
            var subjects = adminservices.GetSubjectAssignedById(id);
            var subject = subjects.FirstOrDefault();

            if (subject != null)
            {
                return Json(new
                {
                    subject.AssignedId,
                    subject.SubjectId,
                    subject.ClassId,
                    subject.SectionId,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult UpdateSubjectAssign(SubjectAssignModel model)
        {
            if (model == null || model.AssignedId == 0 || model.ClassId == 0 || model.SectionId == 0 || model.SubjectId == 0)
            {
                return Json(new { success = false, message = "Invalid data submitted." });
            }

            try
            {
                adminservices.UpdateSubjectAssigned(model);

                return Json(new { success = true, message = "Subject assignment updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating subject assignment: " + ex.Message });
            }
        }

        public ActionResult EventsManagement()
        {
            return View();
        }


        public ActionResult RegistrationFee(int? feeid)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<ClassModel> classList = adminservices.GetAllClasses(userid);
            ViewData["classlist"] = classList;
            ViewBag.ClassList = adminservices.GetAllClasses(userid);
            var data = adminservices.GetRegistrationFeeById(feeid);
            if (data != null)
            {
                ViewData["dataById"] = data;
            }

            var registrationFees = adminservices.GetAllRegistrationFee(userid);
            return View(registrationFees);
        }

        [HttpPost]
        public JsonResult RegistrationFee(RegistrationFeeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                    adminservices.InsertRegistrationFee(model);

                    return Json(new { success = true, message = "Registration fee saved successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid data submitted." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        [HttpPost]
        public JsonResult DeleteRegistrationFee(int RegistrationId)
        {
            string resultMessage = adminservices.DeleteRegistrationFee(feeid: RegistrationId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Registration fee deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        [HttpGet]
        public JsonResult GetRegistrationFeeById(int id)
        {
            try
            {
                var feedata = adminservices.GetRegistrationFeeById(id);
                var singleFee = feedata.FirstOrDefault();

                return Json(singleFee, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateRegistrationFee(RegistrationFeeModel model)
        {
            if (ModelState.IsValid)
            {
                adminservices.UpdateRegistrationFee(model);

                return Json(new { success = true });

            }
            return Json(new { success = false });
        }

        public ActionResult AddCollegeFee(int? feeid)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<ClassModel> classList = adminservices.GetAllClasses(userid);
            ViewData["classlist"] = classList;
            ViewBag.ClassList = adminservices.GetAllClasses(userid);

            var data = adminservices.GetCollegeFeeById(feeid);
            if (data != null)
            {
                ViewData["dataById"] = data;
            }

            var collegeFees = adminservices.GetAllCollegeFee(userid);
            return View(collegeFees);

        }
        [HttpPost]
        public JsonResult AddCollegeFee(CollegeFeeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                    adminservices.InsertCollegeFee(model);

                    return Json(new { success = true, message = "College fee saved successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid data submitted." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteCollegeFee(int FeeId)
        {
            string resultMessage = adminservices.DeleteCollegeFee(feeid: FeeId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "CollegeFee deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        [HttpGet]
        public JsonResult GetCollegeFeeById(int id)
        {
            try
            {
                var feedata = adminservices.GetCollegeFeeById(id);
                var singleFee = feedata.FirstOrDefault();

                return Json(singleFee, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateCollegeFee(CollegeFeeModel model)
        {
            if (ModelState.IsValid)
            {
                adminservices.UpdateCollegeFee(model);

                return Json(new { success = true });

            }
            return Json(new { success = false });
        }

        public ActionResult AddDiscountFee(int? feeid)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<ClassModel> classList = adminservices.GetAllClasses(userid);
            ViewData["classlist"] = classList;
            ViewBag.ClassList = adminservices.GetAllClasses(userid);


            var discountFees = adminservices.GetAllDiscountFee(userid);
            return View(discountFees);
            //return View();

        }
        [HttpPost]
        public JsonResult AddDiscountFee(DiscountFeeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                    adminservices.InsertDiscountFee(model);

                    return Json(new { success = true, message = "Discount fee saved successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid data submitted." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteDiscountFee(int FeeId)
        {
            string resultMessage = adminservices.DeleteDiscountFee(feeid: FeeId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "DiscountFee deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        [HttpGet]
        public JsonResult GetDiscountFeeById(int id)
        {
            try
            {
                var feedata = adminservices.GetDiscountFeeById(id);
                var singleFee = feedata.FirstOrDefault();
                return Json(singleFee, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AddWarden()
        {
            List<masterState> state = adminservices.GetAllState();
            ViewBag.state = state;
            int id = homeService.GetUserId(User.Identity.Name);
            ViewBag.warden = adminservices.GetAllWarden(id);
            ViewBag.BlockList = adminservices.GetBlockByWardenId(id);
            return View();
        }

        [HttpPost]
        public JsonResult DeleteWarden(int Id)
        {
            string resultMessage = adminservices.deleteWarden(Id: Id);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Data deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        [HttpPost]

        public JsonResult wardenRegistration(warden model)
        {
            int id = homeService.GetUserId(User.Identity.Name);

            try
            {
                if (model.Documents != null)
                {
                    model.Document = adminservices.UploadImageToServer(model.Documents);
                }
                model.userId = id;
                bool res = adminservices.InsertWarden(model, out string eror);
                if (res)
                {
                    return Json(new { success = true, message = "Warden registered successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = eror });
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetWardenById(int id)
        {
            try
            {
                var sectionData = adminservices.GetWardenById(id);
                return Json(sectionData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateWardens(warden data)
        {
            string result = adminservices.UpdateWarden(data);

            if (result == "Success")
                return Json(new { success = true, message = "Warden updated successfully." });

            return Json(new { success = false, message = result });
        }


        public ActionResult GetTotalFeeByClassId(int id)
        {
            var res = adminservices.getTotalFeeByClassId(id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTotalFeeByClass(int id)
        {
            var res = adminservices.getTotalFeeByClass(id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateDiscountFee(DiscountFeeModel model)
        {
            if (ModelState.IsValid)
            {
                adminservices.UpdateDiscountFee(model);

                return Json(new { success = true });

            }
            return Json(new { success = false });
        }



        [HttpGet]
        public JsonResult GetCityById(int id)
        {
            var data = adminservices.GetCityByState(id);
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        public ActionResult AddBlock(int? hostelid, int? id, int? academicYear = null)
        {
            ViewBag.wardenList = adminservices.GetAllWarden(id);

            var data = adminservices.GetBlockById(hostelid);
            if (data != null)
            {
                ViewData["dataById"] = data;
            }
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var blocks = adminservices.GetAllBlock(userid, academicYear);
            return View(blocks);

        }
        [HttpPost]
        public JsonResult AddBlock(BlockModel model)
        {
            try
            {
                if (model.HostelId > 0)
                {
                    adminservices.UpdateBlock(model);
                    return Json(new { success = true, message = "Block Updated Successfully!" });
                }
                else
                {
                    model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                    adminservices.InsertBlock(model);
                    return Json(new { success = true, message = "Block Added Successfully!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpGet]
        public ActionResult GetFloorByBlock(int id)
        {
            int data = adminservices.GetFloorByBlockId(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetFloorByBlockForAddStudent(int id)
        {
            var data = adminservices.GetFloorByBlockIdToAddStd(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetBlockByStudentGender(string gendertype)
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            var blocks = adminservices.GetAllBlock(userid).Where(d => d.GenderType == gendertype.ToLower()).ToList();
            return Json(blocks, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteBlock(int HostelId)
        {
            string resultMessage = adminservices.DeleteBlock(hostelid: HostelId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Block deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }


        public ActionResult AddRoomTypes(int? roomid)
        {

            var data = adminservices.GetRoomTypesById(roomid);
            if (data != null)

            {
                ViewData["dataById"] = data;
            }
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var roomtypes = adminservices.GetAllRoomTypes(userid);
            return View(roomtypes);
            // return View();

        }
        [HttpPost]
        public JsonResult AddRoomTypes(RoomTypeModel model)
        {
            if (ModelState.IsValid)
            {
                model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                adminservices.InsertRoomTypes(model);

                return Json(new { success = true });

            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult DeleteRoomTypes(int RoomId)
        {
            string resultMessage = adminservices.DeleteRoomTypes(roomid: RoomId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Room Type deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        [HttpGet]
        public JsonResult GetRoomTypesById(int id)
        {
            try
            {
                var data = adminservices.GetRoomTypesById(id);
                var singledata = data.FirstOrDefault();

                return Json(singledata, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateRoomTypes(RoomTypeModel model)
        {
            if (ModelState.IsValid)
            {
                adminservices.UpdateRoomTypes(model);

                return Json(new { success = true });

            }
            return Json(new { success = false });
        }

        public ActionResult AddRoomNumber(int? roomid)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<RoomTypeModel> roomTypes = adminservices.GetAllRoomTypes(userid);
            ViewData["roomTypes"] = roomTypes;
            ViewBag.RoomTypesList = adminservices.GetAllRoomTypes(userid);
            List<BlockModel> blocks = adminservices.GetAllBlock(userid);
            ViewData["blocks"] = blocks;
            ViewBag.BlockList = adminservices.GetAllBlock(userid);

            var roomnumber = adminservices.GetAllRoomNumber(userid);
            return View(roomnumber);

        }

        [HttpPost]
        public JsonResult AddRoomNumber(RoomNumberModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                    adminservices.InsertRoomNumber(model);

                    return Json(new { success = true });

                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                 .Select(e => e.ErrorMessage)
                                                 .ToList();
                    return Json(new { success = false, message = "Validation failed", errors });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetRoomNumberById(int id)
        {
            try
            {
                var data = adminservices.GetRoomNumberById(id);
                var singledata = data.FirstOrDefault();

                return Json(singledata, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateRoomNumber(RoomNumberModel model)
        {
            if (ModelState.IsValid)
            {
                adminservices.UpdateRoomNumber(model);

                return Json(new { success = true });

            }
            return Json(new { success = false });
        }
        [HttpPost]
        public JsonResult DeleteRoomNumber(int RoomId)
        {
            string resultMessage = adminservices.DeleteRoomNumber(roomid: RoomId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Room No deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        public ActionResult AddStudentInHostel(int? hostelid)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<RoomTypeModel> roomTypes = adminservices.GetAllRoomTypes(userid);
            ViewData["roomTypes"] = roomTypes;
            ViewBag.RoomTypesList = adminservices.GetAllRoomTypes(userid);
            List<BlockModel> blocks = adminservices.GetAllBlock(userid);
            ViewData["blocks"] = blocks;
            ViewBag.BlockList = adminservices.GetAllBlock(userid);
            //List<RegistrationModel> studentName = adminservices.GetAllStudents(userid);
            //ViewData["studentName"] = studentName;
            ViewBag.StudentList = adminservices.GetAllStudents(userid);
            List<RoomNumberModel> roomno = adminservices.GetAllRoomNumber(userid);
            ViewData["roomno"] = roomno;
            ViewBag.RoomNumberList = adminservices.GetAllRoomNumber(userid);
            var data = adminservices.GetStudentInHostelById(hostelid);
            ViewData["ClassList"] = adminservices.GetAllClasses(userid);
            if (data != null)
            {
                ViewData["dataById"] = data;
            }

            var students = adminservices.GetLastFeeRecords(userid);
            return View(students);


        }

        [HttpPost]
        public JsonResult AddStudentInHostel(AdminModel model)
        {
            try
            {

                model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                 bool res = adminservices.InsertStudentsInHostel(model,out string errormsg);
                return Json(new { success = res, message =res? "Student Registered Successfully!": errormsg });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult DeleteStudentInHostel(int HostelId)
        {
            string resultMessage = adminservices.DeleteStudentsInHostel(hostelid: HostelId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Student deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }


        [HttpGet]
        public JsonResult GetRoomDetails(int roomId)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var result = adminservices.GetRoomInfo(roomId, userid);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetStudentById(int studentId)
        {

            try
            {
                var student = adminservices.GetStudentInHostelById(studentId);
                return Json(student, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        [HttpPost]
        public JsonResult UpdateStudentInHostel(AdminModel model)
        {
            if (ModelState.IsValid)
            {
                var res = adminservices.UpdateStudentsInHostel(model);
                if (res)
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }

        [HttpGet]
        public ActionResult AcademicHoliday(int? vacid)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<ClassModel> classList = adminservices.GetAllClasses(userid);
            ViewData["classlist"] = classList;

            var data = adminservices.GetAcademicVacationById(vacid);
            if (data != null)
            {
                ViewData["dataById"] = data;
            }

            var academicVacations = adminservices.GetAllAcademicVacation(userid);
            return View(academicVacations);


        }

        [HttpPost]
        public JsonResult AcademicHoliday(AcademicVacationModel model)
        {
            try
            {
                if (model.VacationId > 0)
                {
                    model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                    adminservices.UpdateAcademicVacation(model);
                    return Json(new { success = true, message = "Vacation Updated Successfully!" });
                }
                else
                {
                    model.Image = adminservices.UploadImageToServer(model.Images);

                    adminservices.InsertAcademicVacation(model);
                    return Json(new { success = true, message = "Vacation Registered Successfully!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteAcademicVacation(int VacationId)
        {
            string resultMessage = adminservices.DeleteAcademicVacation(vacid: VacationId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Vacation deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        [HttpGet]
        public ActionResult FestivalHoliday(int? festid)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var data = adminservices.GetFestivalHolidayById(festid);

            if (data != null)
            {
                ViewData["dataById"] = data;
            }

            var festivalHolidays = adminservices.GetAllFestivalHoliday(userid);
            return View(festivalHolidays);

        }

        [HttpPost]
        public JsonResult FestivalHoliday(FestivalHoliday model)
        {
            try
            {
                if (model.FestivalId > 0)
                {
                    model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                    adminservices.UpdateFestivalHoliday(model);
                    return Json(new { success = true, message = "Festival Updated Successfully!" });
                }
                else
                {


                    adminservices.InsertFestivalHoliday(model);
                    return Json(new { success = true, message = "Festival Registered Successfully!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult DeleteFestivalHoliday(int FestivalId)
        {
            string resultMessage = adminservices.DeleteFestivalHoliday(festid: FestivalId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Festival deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        public ActionResult InventoryManagement()
        {
            return View();
        }
        public ActionResult LibraryManagement()
        {
            return View();
        }
        public ActionResult NewsAndBlogManagement()
        {
            return View();
        }
        public ActionResult ResultManagement()
        {
            return View();
        }

        public ActionResult StudentRegistrationList(string year, string stage)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var students = adminservices.GetAllStudents(userid).ToList();

            if (!string.IsNullOrEmpty(year) && year != "null")
            {
                students = students.Where(e => e.AcademicYear == year).ToList();
            }

            if (!string.IsNullOrEmpty(stage) && stage != "null")
            {
                students = students.Where(e => e.AdmissionStage == stage).ToList();
            }

            return View(students);
        }
        [HttpPost]
        public JsonResult DeleteStudent(int StudentId)
        {
            string resultMessage = adminservices.DeleteStudent(studentid: StudentId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Student deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }

        [HttpPost]
        public JsonResult UpdateStudent(RegistrationModel model)
        {
            if (model.StudentId > 0)
            {
                bool res = adminservices.UpdateStudent(model, out string error);
                if (res)
                {
                    return Json(new { success = true, message = "Student updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = error });
                }
            }
            return Json(new { success = false, message = "Invalid Student ID." });
        }

        public ActionResult StudentRegistration(int id = 0)
        {
            RegistrationModel rs = new RegistrationModel();
            if (id > 0)
            {
                rs = adminservices.GetStudentById(id);
                rs.IsUpdate = true;
            }
            else
            {
                rs.IsUpdate = false; 
            }
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<ClassModel> ClassList = adminservices.GetAllClasses(userid);
            ViewData["ClassList"] = ClassList;
            List<masterState> state = adminservices.GetAllState();
            ViewBag.state = state;
            return View(rs);
        }

        public string GenerateAdmissionNo(string studentName, string mobileNo)
        {
            if (string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(mobileNo) || mobileNo.Length < 4)
                return string.Empty;

            var firstName = studentName.Split(' ')[0];
            var last4Digits = mobileNo.Substring(mobileNo.Length - 4);
            var currentYear = DateTime.Now.Year;

            return $"{firstName}-{last4Digits}-{currentYear}";
        }

        [HttpPost]
        public JsonResult StudentRegistration(RegistrationModel model)
        {
            try
            {
                model.AdmissionNo = GenerateAdmissionNo(model.StudentName, model.MobileNo);
                model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                bool res = adminservices.InsertStudent(model, out string error);
                if (res)
                {
                    return Json(new { success = true, message = "Student Registered Successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = error });
                }
            }

            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
        }

        public ActionResult StudentDetails(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var student = adminservices.GetStudentById(id.Value);

            if (student == null) 
            {
                return HttpNotFound("Student not found");
            }

            return View(student);
        }

        public JsonResult UpdateStudent(RoomTypeModel model)
        {
            if (ModelState.IsValid)
            {
                adminservices.UpdateRoomTypes(model);

                return Json(new { success = true });

            }
            return Json(new { success = false });
        }

        [HttpGet]
        public JsonResult GetSectionsByClass(int classId)
        {
            List<SectionModel> sections = adminservices.GetSectionsByClassId(classId);
            return Json(sections, JsonRequestBehavior.AllowGet);

        }

        public ActionResult TeacherRegistration(int? id)
        {
            if (id.HasValue)
            {
                ViewBag.teacher = adminservices.GetTeachersByTeacherId((int)id);
            }
            int userid = homeService.GetUserId(User.Identity.Name);
            ViewBag.designationlist = adminservices.GetAllDesignations(userid);
            return View();
        }

        public ActionResult teacherProfile(int id)
        {
            var data = adminservices.GetTeachersByTeacherId((int)id);
            return View(data);
        }

        [HttpPost]
        public JsonResult TeacherRegistration(TeacherModel model)
        {

            try
            {
                model.userid = homeService.GetUserId(User.Identity.Name);
                if (!ModelState.IsValid)
                {

                    var errors = ModelState
                        .Where(ms => ms.Value.Errors.Count > 0)
                        .Select(ms => new
                        {
                            Field = ms.Key,
                            Errors = ms.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        })
                        .ToList();
                    return Json(new { success = false, message = "Some Data are missing", invalidFields = errors });

                }

                bool res = adminservices.InsertTeacher(model, out string errorMessage);
                if (res)
                {

                    return Json(new
                    {
                        success = true,
                        type = $"{(model.TeacherId > 0 ? "update" : "add")}",
                        message = $"Teacher {(model.TeacherId > 0 ? "Updated" : "registered")} Successfully!"
                    });
                }
                else
                {
                    return Json(new { success = false, message = errorMessage });

                }


            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }
        public ActionResult TeacherRegistrationList(int? academicYear)
        {
            int id = homeService.GetUserId(User.Identity.Name);
            var teachers = adminservices.GetAllTeachers(id,academicYear);

            return View(teachers);

        }

        [HttpPost]
        public JsonResult DeleteTeacher(int TeacherId)
        {
            string resultMessage = adminservices.DeleteTeacher(TeacherId: TeacherId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Teacher deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        public ActionResult SecurityManagement()
        {
            return View();
        }
        public ActionResult TCManagement()
        {
            return View();
        }
        public ActionResult TransportManagement()
        {
            return View();
        }
        public ActionResult VisitorsManagement()
        {
            return View();
        }
        public ActionResult SubscriptionPlans()
        {
            return View();
        }
        public ActionResult ExamPreparation()
        {
            return View();
        }
        public ActionResult Reports()
        {
            return View();
        }
        public ActionResult MailCommunication()
        {
            return View();
        }
        [HttpPost]
        public JsonResult eventcategories(EventCategory category)
        {
            category.userid = homeService.GetUserId(User.Identity.Name);
            if (!string.IsNullOrWhiteSpace(category.CategoryName))
            {
                bool res = adminservices.Addcategory(category);
                if (res)
                {
                    return Json(new { status = true, message = "Event has been added successfully." });
                }
                else
                {
                    return Json(new { status = false, message = "Something went wrong. Could not add the Event." });

                }
            }
            else
            {
                return Json(new { status = false, message = "Please fill out all required fields." });
            }
        }

        public ActionResult addeventcategories()
        {
            List<EventCategory> list = new List<EventCategory>();
            int userid = homeService.GetUserId(User.Identity.Name);
            list = adminservices.ShowAllcategory(userid);
            return View(list);
        }

        public JsonResult DeleteCategory(int id)
        {

            bool res = adminservices.DeleteCategory(id);
            if (res)
            {
                return Json(new { status = true, message = "Event Deleted successfully!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = false, message = "Failed to Delete" }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public JsonResult UpdateCategory(EventCategory category)
        {
            if (!string.IsNullOrWhiteSpace(category.CategoryName))
            {
                bool res = adminservices.UpdateCategory(category);
                if (res)
                {
                    return Json(new { status = true, message = "Event has been updated successfully." });
                }
                else
                {
                    return Json(new { status = false, message = "Something went wrong. Could not update the Event." });
                }
            }
            else
            {
                return Json(new { status = false, message = "Please fill out all required fields." });
            }

        }

        public ActionResult AddEvent()
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            List<EventCategory> list = adminservices.ShowAllcategory(userid);
            ViewBag.EventCategories = list;
            return View();
        }

        public ActionResult AddMaterial()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewData["categories"] = adminservices.GetInventCategory(userId);
            ViewData["materialList"] = adminservices.GetInventoryMaterial(userId);
            return View();
        }
        [HttpPost]
        public ActionResult AddMaterial(Material mat)
        {
            try
            {
                mat.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                bool res = adminservices.InsertInventoryMaterial(mat,out string mesage);

                return Json(new
                {
                    status = res,
                    message = res
                        ? (mat.materialId > 0 ? "Updated Successfully" : "Added Successfully")
                        : mesage
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }
        [HttpGet]
        public JsonResult DeleteMaterial(int id)
        {
            bool res = adminservices.DeleteInventoryMaterial(id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddInventoryCategory(InventoryCategory incat)
        {
            incat.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertInventoryCategory(incat);
            return Json(new
            {
                status = res,
                message = res ? "Added Successfully" : "Category Already Present"
            });
        }
        public ActionResult AddStockMaterial()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewData["categories"] = adminservices.GetInventCategory(userId);
            ViewData["stockList"] = adminservices.GetStockList(userId);
            return View();
        }
        [HttpPost]
        public ActionResult AddStockMaterial(StockMaterial sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertStockMaterial(sm,out string errormsg);
            return Json(new
            {
                status = res,
                message = res ? (sm.stockMaterialId > 0 ? "Stock Updated Successfully" : "Stock Added Successfully") : errormsg
            });
        }
        [HttpGet]
        public JsonResult DeleteStock(int id)
        {
            bool res = adminservices.DeleteInventoryStock(id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetMaterial(int id)
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var data = adminservices.GetInventoryMaterialById(userId, id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddPurchase()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewData["categories"] = adminservices.GetInventCategory(userId);
            ViewData["purchaseList"] = adminservices.GetPurchaseList(userId);
            return View();
        }
        [HttpPost]
        public ActionResult AddPurchase(PurchaseMaterial sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string uniqueFileName = null;
            if (sm.billSlip != null && sm.billSlip.ContentLength > 0)
            {
                string fileName = sm.billSlip.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                sm.billSlipName = "/Upload/" + uniqueFileName;
            }
            bool res = adminservices.InsertPurchaseMaterial(sm);


            if (res && uniqueFileName != null)
            {
                string filePath = Server.MapPath("~/Upload/") + uniqueFileName;
                sm.billSlip.SaveAs(filePath);
            }

            return Json(new
            {
                status = res,
                message = res ? (sm.purchaseId > 0 ? "Updated Successfully" : "Added Successfully") : "Some issue occurred"
            });
        }
        [HttpGet]
        public JsonResult DeletePurchase(int id)
        {
            bool res = adminservices.DeletePurchase(id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddShortMaterial()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewData["categories"] = adminservices.GetInventCategory(userId);
            ViewData["shortList"] = adminservices.GetShortList(userId);
            return View();
        }
        [HttpPost]
        public ActionResult AddShortMaterial(ShortMaterial sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertShortMaterial(sm);
            return Json(new
            {
                status = res,
                message = res ? (sm.stockMaterialId > 0 ? "Updated Successfully" : "Added Successfully") : "Some issue occurred"
            });
        }
        [HttpGet]
        public JsonResult DeleteShort(int id)
        {
            bool res = adminservices.DeleteInventoryShort(id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddBus(int? academicYear=null)
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewBag.list = adminservices.GetBusList(userId, academicYear);

            return View();
        }
        [HttpPost]
        public JsonResult AddBuss(Buss sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string uniqueFileName = null;
            if (sm.image != null && sm.image.ContentLength > 0)
            {
                string fileName = sm.image.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                sm.ImageName = "/Upload/" + uniqueFileName;
            }
            bool res = adminservices.AddBuss(sm);


            if (res && uniqueFileName != null)
            {
                string filePath = Server.MapPath("~/Upload/") + uniqueFileName;
                sm.image.SaveAs(filePath);
            }

            return Json(new
            {
                status = res,
                message = res ? (sm.Id > 0 ? "Updated Successfully" : "Added Successfully") : "Some issue occurred"
            });
        }
        [HttpGet]
        public JsonResult DeleteBuss(int Id)
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.DeleteBuss(Id, userId);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult EditBus(int id)
        {
            var data = adminservices.GetBussById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddDriver()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewData["driverList"] = adminservices.GetDriverList(userId);
            return View();
        }
        [HttpPost]
        public ActionResult AddDriver(Drivers sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));

            // Save File1 (Driver Photo)
            if (sm.File1 != null && sm.File1.ContentLength > 0)
            {
                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(sm.File1.FileName);
                string path = Server.MapPath("~/Upload/" + uniqueFileName);
                sm.File1.SaveAs(path);
                sm.DriverFileName = "/Upload/" + uniqueFileName;
            }

            // Save File2 (Aadhar Card)
            if (sm.File2 != null && sm.File2.ContentLength > 0)
            {
                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(sm.File2.FileName);
                string path = Server.MapPath("~/Upload/" + uniqueFileName);
                sm.File2.SaveAs(path);
                sm.AdharCardFileName = "/Upload/" + uniqueFileName;
            }

            // Save File3 (Driving License)
            if (sm.File3 != null && sm.File3.ContentLength > 0)
            {
                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(sm.File3.FileName);
                string path = Server.MapPath("~/Upload/" + uniqueFileName);
                sm.File3.SaveAs(path);
                sm.DLFileName = "/Upload/" + uniqueFileName;
            }

            bool res = adminservices.InsertDriver(sm, out string errorMessage);

            return Json(new
            {
                status = res,
                message = res ? (sm.Id > 0 ? "Driver Details Updated Successfully" : "Driver Added Successfully") : errorMessage
            });
        }
        [HttpGet]
        public JsonResult DeleteDriver(int Id)
        {
            bool res = adminservices.DeleteDriver(Id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult EditDriver(int id)
        {
            var data = adminservices.GetDriverById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddBusRoute()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewBag.list = adminservices.GetBusListNotRoute(userId);
            ViewBag.state = adminservices.GetAllState();
            ViewData["routeList"] = adminservices.GetRouteist(userId);
            return View();
        }
        [HttpPost]
        public ActionResult AddBusRoute(BusRoute sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertBusRoute(sm);
            return Json(new
            {
                status = res,
                message = res ? (sm.Id > 0 ? "Updated Successfully" : "Added Successfully") : "Some issue occurred"
            });
        }
        [HttpGet]
        public JsonResult DeleteBusRoute(int Id)
        {
            bool res = adminservices.DeleteBusRoute(Id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetAllPickupPointOfBus(int id)
        {
            var data = adminservices.GetPickupPointBus(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult EditBusRoute(int id)
        {
            var data = adminservices.GetBusRouteById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AssignBusToDriver()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewBag.busList = adminservices.GetBusToAssign(userId);
            ViewBag.driverList = adminservices.GetDriverToAssign(userId);
            ViewData["assignList"] = adminservices.GetAssignedBus(userId);
            return View();
        }
        [HttpPost]
        public ActionResult AssignBusToDriver(AssignBus sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertAssignBus(sm);
            return Json(new
            {
                status = res,
                message = res ? (sm.id > 0 ? "Updated Successfully" : "Added Successfully") : "Some issue occurred"
            });
        }
        [HttpGet]
        public JsonResult DeleteAssignedBus(int Id)
        {
            bool res = adminservices.DeleteAssignedBus(Id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddStudentRouteWise()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewBag.list = adminservices.GetBusList(userId);
            ViewBag.ClassList = adminservices.GetAllClasses(userId);
            ViewBag.SectionList = adminservices.GetAllSections(userId);
            ViewBag.StudentList = adminservices.GetAllStudents(userId);
            ViewData["stlist"] = adminservices.GetStudentInBus(userId);
            return View();
        }

        public ActionResult HolidayManagement(string year)
        {
            int id = homeService.GetUserId(User.Identity.Name);
            string error = "";
            int parsedYear;
            bool isValidYear = int.TryParse(year, out parsedYear);

            var allHolidays = adminservices.selectAllHoliday(id, out error);
            var res = isValidYear
                ? allHolidays.Where(d => d.year == parsedYear).ToList()
                : allHolidays.ToList();


            ViewBag.error = error;

            return View(res);
        }

        [HttpPost]
        public ActionResult addHoliday(Holiday h)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    h.userid = homeService.GetUserId(User.Identity.Name);
                    bool res = adminservices.InsertHoliday(h, out string error);
                    if (res)
                    {

                        return Json(new { success = true, message = $"Holiday {(h.HolidayId > 0 ? "updated" : "added")} successfully." });
                    }
                    else
                    {
                        return Json(new { success = false, message = error });

                    }
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 500; // Triggers `xhr.error`
                    return Json(new { success = false, message = ex.Message });
                }
            }

            Response.StatusCode = 400; // Triggers `xhr.error`
            return Json(new { success = false, message = "Validation failed" });
        }

        [HttpPost]
        public ActionResult AddStudentRouteWise(AddStudentInBus sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertStudentInBus(sm,out string errormsg);
            return Json(new
            {
                status = res,
                message = res ? (sm.id > 0 ? "Updated Successfully" : "Added Successfully") : errormsg
            });
        }
        [HttpGet]
        public JsonResult DeleteStudentRouteWise(int Id)
        {
            bool res = adminservices.DeleteStudentInBus(Id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult EditStudentInBus(int id)
        {
            var data = adminservices.GetStudentInBusById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddSecurity(int? academicYear = null)
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            ViewBag.state = adminservices.GetAllState();
            ViewBag.BlockList = adminservices.GetAllBlock(userid);
            ViewData["securityList"] = adminservices.GetAllSecurityList(userid, academicYear);
            return View();
        }
        [HttpPost]
        public JsonResult AddSecurity(Security sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string uniqueFileName = null;
            if (sm.Image != null && sm.Image.ContentLength > 0)
            {
                string fileName = sm.Image.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                sm.ImageName = "/Upload/" + uniqueFileName;
            }
            bool res = adminservices.InsertSecurity(sm, out string errorMessage);


            if (res && uniqueFileName != null)
            {
                string filePath = Server.MapPath("~/Upload/") + uniqueFileName;
                sm.Image.SaveAs(filePath);
            }

            return Json(new
            {
                status = res,
                message = res ? (sm.Id > 0 ? "Security Details Updated Successfully" : "Security Added Successfully") : errorMessage
            });
        }
        public JsonResult DeleteSecurity(int Id)
        {
            bool res = adminservices.DeleteSecurity(Id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult EditSecurity(int id)
        {
            var data = adminservices.GetAllSecurityById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteHoliday(int id)
        {
            bool res = adminservices.deleteHoliday(id, out string error);
            if (res)
            {
                return Json(new { status = true, message = "Holiday removed successfully." }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { status = false, message = error }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult SecurityProfile(int? id)
        {
            if (id != null && id > 0)
            {
                var data = adminservices.GetAllSecurityById(Convert.ToInt32(id));
                return View(data);
            }
            return RedirectToAction("addsecurity");
        }
        public ActionResult AddTimeTable()
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewBag.ClassList = adminservices.GetAllClasses(userid);
            ViewData["timetablesList"] = adminservices.ShowAllTimeTable(userid);
            return View();
        }
        [HttpPost]
        public ActionResult AddTimeTable(TimeTableModel tm)
        {
            tm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertTimeTable(tm, out string errorMessage);
            return Json(new
            {
                status = res,
                message = res ? (tm.id > 0 ? "TimeTable Updated Successfully" : "TimeTable Added Successfully") : errorMessage
            });
        }
        [HttpPost]
        public ActionResult UpdateTimeTable(timetableshowModel tm)
        {
            tm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.UpdateTimeTable(tm);
            return Json(new
            {
                status = res,
                message = res ? "TimeTable Updated Successfully" : "Some error occured"
            });
        }
        [HttpGet]
        public JsonResult DeleteTimeTable(int Id)
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.DeleteTimeTable(Id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult ShowTimeTable(int classid, int sectionid)
        {
            var data = adminservices.ShowAllTimeTableDetails(classid, sectionid);
            List<timetableshowModel> list = new List<timetableshowModel>();
            string[] days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            foreach (var day in days)
            {
                list.Add(new timetableshowModel { day = day, ttdata = data.Where(d => d.day == day.ToLower()).ToList() });
            }
            upLoadTimetableModel dt = new upLoadTimetableModel();
            dt.upid = data[0].upid;
            dt.timetable = list;
            dt.attachment = data[0].attachment;
            return Json(dt, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetTimeTableDataById(int id)
        {
            var data = adminservices.GetTimeTableDataById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult updateUploadedTimeTable(AddExamTimeTableModel sm)
        {
            bool data = adminservices.updateUploadedTimeTable(sm);
            return Json(new { status = data, message = data ? "Record Updated Successfully" : "Some error occured" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult getUrlByUserInput(string input)
        {
            var data = adminservices.getUrlByUserInput(input);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult getUrlByUserFavorite()
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            var data = adminservices.getUrlByUserFavorite(userid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult GeneratePdf(string html)
        {
            var file = homeService.PDFConverter(html);
            return File(file, "application/pdf", "timetable.pdf");
        }
        public ActionResult AddBook(int? academicYear=null)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewData["categories"] = adminservices.GetAllBookCategory(userId);
            ViewData["BookList"] = adminservices.GetAllBooks(userId, academicYear);
            return View();
        }
        [HttpPost]
        public ActionResult AddBookCategory(BookCategoryModel cat)
        {
            cat.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertBookCategory(cat);
            return Json(new
            {
                status = res,
                message = res ? (cat.id > 0 ? "Category Updated Successfully" : "Category Added Successfully") : "Some error Occured"
            });
        }
        [HttpGet]
        public ActionResult CheckISBN(string isbn, string actiontype)
        {
            bool IsISBN = adminservices.checkISBN(isbn, actiontype);
            return Json(IsISBN, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult CheckISSN(string issn, string actiontype)
        {
            bool IsISSN = adminservices.checkISSN(issn, actiontype);
            return Json(IsISSN, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckAccession(string accession, string actiontype)
        {
            bool IsAccession = adminservices.checkAccession(accession, actiontype);
            return Json(IsAccession, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddBook(BookModel book)
        {
            book.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            if (book.IsISBN || book.IsISSN || book.IsAccession)
            {
                return Json(new
                {
                    status = book.IsISBN ? book.IsISBN : book.IsISSN ? book.IsISSN : book.IsAccession ? book.IsAccession : false,
                    message = book.IsISBN ? "ISBN number already exist" : book.IsISSN ? "ISSN print already exist" : book.IsAccession ? "Accession number already exist" : ""
                });
            }
            bool res = adminservices.InsertBook(book);
            return Json(new
            {
                status = res,
                message = res ? (book.id > 0 ? "Book Details Updated Successfully" : "Book Added Successfully") : "Some error Occured"
            });
        }
        [HttpGet]
        public ActionResult GetBookById(int id)
        {
            var data = adminservices.GetBookById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult OrderManagement(int? categoryId)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
           
            ViewData["categories"] = adminservices.GetAllBookCategory(userId);
            if(categoryId != null && categoryId > 0)
            {
                ViewData["BookList"] = adminservices.GetAllBooks(userId).Where(data=>data.categoryId== categoryId).ToList().Select(d => d).Take(10).ToList();

            }
            else
            {
                ViewData["BookList"] = adminservices.GetAllBooks(userId).Select(d => d).Take(10).ToList();
            }
                return View();
        }
        [HttpGet]
        public ActionResult FilterForBook(string filter, string filterType)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var data = new List<BookModel>();
            if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(filterType))
            {
                data = filterType == "category" ? adminservices.GetAllBooks(userId).Where(d => d.categoryId == Convert.ToInt32(filter)).ToList() : filterType == "isbn" ? adminservices.GetAllBooks(userId).Where(d => d.isbn == filter).ToList() : filterType == "issn" ? adminservices.GetAllBooks(userId).Where(d => d.issnPrint == filter).ToList() : filterType == "accession" ? adminservices.GetAllBooks(userId).Where(d => d.accessionNumber == filter).ToList() : adminservices.GetAllBooks(userId).Select(d => d).Take(10).ToList();
            }
            else
            {
                data = adminservices.GetAllBooks(userId).Select(d => d).Take(10).ToList();
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowBookDetailsPage(int? id)
        {
            if (id.HasValue)
            {
                var data = adminservices.GetBookById(Convert.ToInt32(id));
                return View(data);
            }
            return RedirectToAction("AddBook");
        }
        public ActionResult GetUserForOrderBook(string userNo, string type)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var data = adminservices.SeletUserForLibrary(userNo, type, userId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult AddBookOrder(AddOrderModal model)
        {
            model.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertBookOrder(model, out string error);
            return Json(new
            {
                status = res,
                message = res && model.id > 0 ? "Order Updated Successfully" : res && model.id == 0 ? "Order Added Successfully" : error
            });
        }
        public ActionResult OrderHistory()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewData["orderList"] = adminservices.GetBookOrders(userId);
            return View();
        }
        [HttpGet]
        public JsonResult GetBookOrderById(int id)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var data = adminservices.GetBookOrderById(id, userId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ReturnBookOrder(ReturnOrderModel model)
        {
            model.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var data = adminservices.GetBookOrderById(model.id, model.userId);
            foreach (var item in data)
            {
                model.lostFine = (item.quantity - model.quantity) * item.lostFine;
                model.damageFine = item.damageFine * model.damageQuantity;
                if (DateTime.ParseExact(item.returnDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date < DateTime.Now.Date)
                {
                    model.lateDays = (DateTime.Now.Date - DateTime.ParseExact(item.returnDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date).Days;
                    model.lateFine = item.lateFine;
                }
            }
            model.extraCharges = model.lateFine + model.damageFine + model.lostFine;
            bool res = adminservices.ReturnBookOrder(model, out string error);
            return Json(new
            {
                status = res,
                message = res ? "Order Received Successfully" : error
            });
        }
        public ActionResult AddTask(int? academicYear = null)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewData["teachers"] = adminservices.GetAllTeachers(userId);
            ViewData["taskList"] = adminservices.GetAllTaskList(userId, academicYear);
            return View();
        }
        [HttpPost]
        public ActionResult AddTask(AddTaskModel sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string uniqueFileName = null;
            if (sm.attachment != null && sm.attachment.ContentLength > 0)
            {
                string fileName = sm.attachment.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                sm.attachmentName = "/Upload/" + uniqueFileName;
            }
            bool res = adminservices.InsertTask(sm);


            if (res && uniqueFileName != null)
            {
                string filePath = Server.MapPath("~/Upload/") + uniqueFileName;
                sm.attachment.SaveAs(filePath);
            }

            return Json(new
            {
                status = res,
                message = res ? (sm.id > 0 ? "Task Updated Successfully" : "Task Added Successfully") : "Some issue occurred"
            });
        }
        [HttpGet]
        public ActionResult GetTaskById(int id)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var data = adminservices.GetTaskById(id, userId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddLibrarian()
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            ViewBag.state = adminservices.GetAllState();
            ViewBag.BlockList = adminservices.GetAllBlock(userid);
            ViewData["librarianList"] = adminservices.GetLibrarian(userid);
            return View();
        }
        [HttpPost]
        public JsonResult AddLibrarian(LibrarianModel sm)
        {
            sm.UserId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string uniqueFileName = null;
            if (sm.Document != null && sm.Document.ContentLength > 0)
            {
                string fileName = sm.Document.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                sm.DocumentName = "/Upload/" + uniqueFileName;
            }
            if (sm.Profile != null && sm.Profile.ContentLength > 0)
            {
                string fileName = sm.Profile.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                sm.ProfileName = "/Upload/" + uniqueFileName;
            }
            bool res = adminservices.InsertLibrarian(sm, out string errorMessage);


            if (res && sm.DocumentName != null)
            {
                string filePath = Server.MapPath("~/Upload/") + uniqueFileName;
                sm.Document.SaveAs(filePath);
            }
            if (res && sm.ProfileName != null)
            {
                string filePath = Server.MapPath("~/Upload/") + uniqueFileName;
                sm.Profile.SaveAs(filePath);
            }

            return Json(new
            {
                status = res,
                message = res ? (sm.Id > 0 ? "Librarian Details Updated Successfully" : "Librarian Added Successfully") : errorMessage
            });
        }
        [HttpGet]
        public ActionResult EditLibrarian(int id)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var data = adminservices.GetLibrarianById(id, userId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AdmissionManagement()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            AdminDashboardCount data = adminservices.GetAdminDashboardCount(userId);
            return View(data);
        }



        [HttpGet]
        public JsonResult GetStaffByDepartment(string department)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var staffList = adminservices.GetStaffByDepartment(department, userid);
            return Json(staffList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStudentsByClassSection(int classId, int sectionId, string academicYear)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var students = adminservices.StudentsByClassAndSection(classId, sectionId, academicYear, userid);
            return Json(students, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStudentsByClassAndSection(int classId, int sectionId)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var students = adminservices.StudentsByClassSection(classId, sectionId, userid);
            return Json(students, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStudentsByClassAndSectionForBus(int classId, int sectionId)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var students = adminservices.StudentsByClassSection(classId, sectionId, userid);
            return Json(students, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddNotice()
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<ClassModel> ClassList = adminservices.GetAllClasses(userid);
            ViewData["ClassList"] = ClassList;
            var notices = adminservices.GetAllNotices(userid);
            return View(notices);
        }
        [HttpPost]
        public JsonResult AddNotice(NoticeModel model, HttpPostedFileBase Attachment)
        {
            try
            {
                string errorMessage;
                int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                bool isSuccess = adminservices.InsertNotice(model, userid, Attachment, out errorMessage);

                return Json(new
                {
                    success = isSuccess,
                    message = isSuccess ? "Notice inserted successfully!" : errorMessage
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult DeleteNotice(int NoticeId)
        {
            string resultMessage = adminservices.DeleteNotice(noticeid: NoticeId);

            if (resultMessage == "Success")
            {
                return Json(new { success = true, message = "Notice deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        [HttpPost]
        public JsonResult GetNoticeById(int id)
        {
            try
            {
                var notice = adminservices.GetNoticeById(id);
                return Json(notice, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]

        public JsonResult UpdateNotice(NoticeModel model)
        {
            try
            {
                string errorMessage;
                bool isSuccess = adminservices.UpdateNotice(model, out errorMessage);

                return Json(new
                {
                    success = isSuccess,
                    message = isSuccess ? "Notice updated successfully!" : errorMessage
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult AdmittedStudent(string year=null)
        {
         
            int userId = homeService.GetUserId(User.Identity.Name);
            var data = adminservices.GetAdmittedStudents(userId).ToList();
            if (!string.IsNullOrEmpty(year) && year != "null")
            {
                data = data.Where(e => e.AcademicYear == year).ToList();
            }
            ViewData["admittedStudents"] = data;
            ViewBag.ClassList = adminservices.GetAllClasses(userId);
            return View();
        }
        [HttpGet]
        public ActionResult PromoteStudent(int id)
        {
            var data = adminservices.GetStudentById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult PromoteStudent(PromoteStudentModel pm)
        {
            bool res = adminservices.PromoteStudent(pm);
            return Json(new
            {
                status = res,
                message = res ? "Promote Student Successfully" : "Server error occurred"
            });
        }
        [HttpGet]
        public ActionResult ShowTimeScheduleToTeacher(int classId, int sectionId, int subjectId)
        {
            var data = adminservices.ShowTimeScheduleToTeacher(classId, sectionId, subjectId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WardenProfile(int id)
        {
            var data = adminservices.GetWardenById(id);
            return View(data);
        }
        public ActionResult AddSyllabus()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewData["syllabusList"] = adminservices.GetAllSyllabus(userId);
            ViewData["classList"] = adminservices.GetAllClasses(userId);
            return View();
        }
        public ActionResult AddExamTimeTable()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewData["examList"] = adminservices.GetAllExamTimeTable(userId);
            ViewData["classList"] = adminservices.GetAllClasses(userId);
            ViewData["scexamslist"] = adminservices.GetAllExamName(userId);
            return View();
        }
        [HttpPost]
        public ActionResult AddSyllabus(AddSyllabusMoedel sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string uniqueFileName = null;
            if (sm.attachment != null && sm.attachment.ContentLength > 0)
            {
                string fileName = sm.attachment.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                sm.attachmentName = "/Upload/" + uniqueFileName;
            }
            bool res = adminservices.InsertSyllabus(sm,out string errormsg);


            if (res && uniqueFileName != null)
            {
                string filePath = Server.MapPath("~/Upload/") + uniqueFileName;
                sm.attachment.SaveAs(filePath);
            }

            return Json(new
            {
                status = res,
                message = res ? (sm.id > 0 ? "Syllabus Updated Successfully" : "Syllabus Added Successfully") : errormsg
            });
        }
        [HttpGet]
        public JsonResult DeleteSyllabus(int id)
        {
            bool res = adminservices.DeleteSyllabus(id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetSyllabusById(int id)
        {
            var data = adminservices.GetSyllabusById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddExamTimeTable(AddExamTimeTableModel sm)
        {
            sm.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string uniqueFileName = null;
            if (sm.attachment != null && sm.attachment.ContentLength > 0)
            {
                string fileName = sm.attachment.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                sm.attachmentName = "/Upload/" + uniqueFileName;
            }
            bool res = adminservices.InsertExamTimeTable(sm);


            if (res && uniqueFileName != null)
            {
                string filePath = Server.MapPath("~/Upload/") + uniqueFileName;
                sm.attachment.SaveAs(filePath);
            }

            return Json(new
            {
                status = res,
                message = res ? (sm.id > 0 ? "ExamTimeTable Updated Successfully" : "ExamTimeTable Added Successfully") : "Some issue occurred"
            });
        }
        [HttpGet]
        public JsonResult DeleteExamTimeTable(int id)
        {
            bool res = adminservices.DeleteExamTimeTable(id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetExamTimeTableById(int id)
        {
            var data = adminservices.GetExamTimeTableById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LeaveRequest(int? academicYear = null)
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            ViewData["leaveRequestList"] = adminservices.GetAllLeaveRequst(userid, academicYear);
            return View();
        }
        [HttpPost]
        public ActionResult ApproveOrREject(int id, int status, string remark)
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            bool res = adminservices.AcceptRejectLeave(userid, id, status, remark);
            return Json(new
            {
                status = res,
                message = res && status == 1 ? "Accepted Successfully" : res && status == 2 ? "Rejected Successfully" : "Some error occured"
            });
        }
        [HttpGet]
        public ActionResult GetLeaveHistoryOfTeacher(int id)
        {
            var data = adminservices.LeaveRequstHistoryOfTeacher(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AllFeeRecords()
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            List<ClassModel> ClassList = adminservices.GetAllClasses(userid);
            ViewData["ClassList"] = ClassList;
            var records = adminservices.GetLatestFeeRecordOfAllStudents(userid);
            var busFeeRecords = adminservices.GetStudentBusFees(userid);
            var latestBus = busFeeRecords
            .GroupBy(b => b.studentId)
            .Select(g => g.OrderByDescending(x => x.paymentDated).First())
            .ToList();
            var viewModel = new FeeStatementViewModel
            {
                AcademicFeeRecords = records,
                BusFeeRecords = latestBus
            };
            return View(viewModel);
        }
        [HttpGet]
        public ActionResult StudentTransactionHistory(int? studentid)
        {
            if (studentid.HasValue)
            {
                var data = adminservices.GetStudentTransactionHistory(Convert.ToInt32(studentid));
                return View(data);
            }
            else
            {
                return RedirectToAction("AllFeeRecords");
            }
        }
        [HttpPost]
        public JsonResult AllFeeRecords(AllFeeRecordModel model)
        {
            try
            {
                string errorMessage;
                model.userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                bool isSuccess = adminservices.InsertAllFeeRecord(model, out errorMessage);

                return Json(new
                {
                    success = isSuccess,
                    message = isSuccess ? "Fee submitted successfully!" : errorMessage
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetAllFeeRecordById(int id)
        {
            var data = adminservices.GetAllFeeRecordById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateFeeRecord(AllFeeRecordModel model)
        {
            string errorMessage;
            bool isUpdated = adminservices.UpdateAllFeeRecord(model, out errorMessage);

            if (isUpdated)
            {
                return Json(new { success = true, message = "Fee updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = errorMessage });
            }
        }
        [HttpGet]
        public JsonResult GetDashboardCounts(int? year)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var result = adminservices.GetDashboardCounts(userid, year);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ShowRecieptOfBookOrder(int id)
        {
            if (User.IsInRole("admin") || User.IsInRole("librarian"))
            {
                var data = adminservices.DownloadRecieptOfBook(id);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            return new HttpUnauthorizedResult();
        }
        [HttpPost]
        public JsonResult SelectClassByRegistrationId()
        {
            int userid = homeService.GetUserId(User.Identity.Name);

            List<ClassModel> classList = adminservices.GetUnassignedClasses(userid);

            return Json(classList);
        }

        [HttpGet]
        public JsonResult GetStudentsForBlock(int classId, int sectionId, string academicYear)
        {
            int userid = homeService.GetUserId(User.Identity.Name);

            var students = adminservices.GetUnassignedStudentsByClassSection(classId, sectionId, academicYear, userid);

            return Json(students, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UploadTimeTable(AddExamTimeTableModel ut)
        {
            if (ut.attachment != null && ut.attachment.ContentLength > 0)
            {
                string extension = Path.GetExtension(ut.attachment.FileName).ToLower();
                string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new { status = false, message = "Only PDF, JPG, JPEG, or PNG files are allowed." });
                }
            }
            ut.userId = homeService.GetUserId(User.Identity.Name);
            bool res = adminservices.UploadTimeTable(ut);
            return Json(new
            {
                status = res,
                message = res ? (ut.id > 0 ? "TimeTable Updated Successfully" : "TimeTable Uploaded Successfully") : "Some issue occurred"
            });
        }
        [HttpGet]
        public ActionResult GetUploadedTimeTableById(int? id)
        {
            var data = adminservices.GetUploadedTimeTableById(Convert.ToInt32(id));
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult DeleteUploadedTimeTable(int? Id)
        {
            bool res = adminservices.DeleteUploadedTimetable(Convert.ToInt32(Id));

            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPickupPointsByBusId(int busId)
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            var pickupPoints = adminservices.GetPickupPointsByBusId(busId, userid);
            return Json(pickupPoints, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SecurityReport()
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            ViewData["securityList"] = adminservices.GetAllSecurityList(userid);
            return View();
        }
        public ActionResult VisitorList(int id)
        {
            ViewData["visitorsList"] = _securityService.GetAllVisitorsListForReport(id);
            return View();
        }
        public ActionResult AppDetail()
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            var data = adminservices.GetAppDetails(userid);
            ViewBag.ppm = adminservices.GetPrivacyPolicy(userid, "teacher");
            ViewBag.tac = adminservices.GetTermsAndConditions(userid, "teacher");
            ViewBag.avd = adminservices.GetAppVersionDetail(userid);
            if (data == null)
                data = new AppDetailModel();
            return View(data);
        }
        [HttpPost]
        public ActionResult AddAppDetails(AppDetailModel adm)
        {
            adm.userid = homeService.GetUserId(User.Identity.Name);
            bool res = adminservices.InsertAppDetail(adm);
            return Json(new
            {
                status = res,
                message = res ? $"{adm.type} Updated Successfully" : "Some error occured"
            });
        }
        [HttpGet]
        public ActionResult PrivacyPolicyByRole(string role)
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            var data = adminservices.GetPrivacyPolicy(userid, role);
            return Json(new { status = data != null, data = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult TermsAndConditions(string role)
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            var data = adminservices.GetTermsAndConditions(userid, role);
            return Json(new { status = data != null, data = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddAppVersionDetails(AppVersionModel adm)
        {
            adm.userid = homeService.GetUserId(User.Identity.Name);
            bool res = adminservices.InsertAppVersionDetail(adm);
            return Json(new
            {
                status = res,
                message = res ? "App Version Updated Successfully" : "Some error occured"
            });
        }


        #region Attendance excel Upload

        [HttpGet]
        public ActionResult UploadAttendaceReportForStaff(string department)
        {
            string username = User.Identity.Name;
            int userid = homeService.GetUserId(username);
            var query = new List<ExcelAttendanceRecordModel>();

            ViewBag.Department = adminservices
     .GetAttendaceRecordByExcel(userid)
     .Select(e => e.DepartmentName).Distinct();

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = adminservices
     .GetAttendaceRecordByExcel(userid)
     .ToList();
            }

            // only filter by department if the parameter is non‐empty
            if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(e => e.DepartmentName == department).ToList();


            // finally materialize
            var data = query.ToList();


            return View(data);
        }
        [HttpGet]
        public ActionResult ShowHistory(string department,string empcode,string startDate,string endDate)
        {

            string username = User.Identity.Name;
            int userid = homeService.GetUserId(username);

                ViewBag.EmployeeCode = empcode;
                ViewBag.DepartmentName = department;
            var history = new List<ExcelAttendanceRecordModel>();
            if(!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                history = adminservices.GetAttendaceRecordById(userid, department, empcode, startDate, endDate);
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
            }

            return View(history);

        }
        [HttpPost]
        public ActionResult UploadAttendanceExcel(HttpPostedFileBase file)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                {
                    TempData["Error"] = "Please select an Excel file.";
                    return RedirectToAction("UploadAttendaceReportForStaff");
                }

                var username = User.Identity.Name;
                var userid = homeService.GetUserId(username);
                System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // 1) read entire sheet into a DataTable
                DataTable sheet;
                using (var stream = file.InputStream)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var cfg = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = false
                        }
                    };
                    sheet = reader.AsDataSet(cfg).Tables[0];
                }

                // 2) grab Department and StartDate (your existing logic)
                string department = "";
                DateTime startDate = DateTime.MinValue;
                for (int r = 0; r < Math.Min(sheet.Rows.Count, 10); r++)
                {
                    var row = sheet.Rows[r];
                    for (int c = 0; c < sheet.Columns.Count - 1; c++)
                    {
                        var cell = row[c]?.ToString().Trim().ToLower();
                        if (cell?.Contains("department:") == true)
                        {
                            department = sheet.Rows[r][c + 3]?.ToString().Trim();
                        }
                        if (cell?.Contains("to") == true)
                        {
                            var parts = cell.Split(new[] { "to" }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 0
                             && DateTime.TryParseExact(parts[0].Trim(), "MMM dd yyyy",
                                                       CultureInfo.InvariantCulture,
                                                       DateTimeStyles.None,
                                                       out DateTime dts))
                            {
                                startDate = dts;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(department) && startDate != DateTime.MinValue)
                        break;
                }
                if (startDate == DateTime.MinValue)
                    throw new Exception("Could not locate the date-range cell.");

                // 3) find the “Days” header row
                int daysRow = -1;
                for (int r = 0; r < sheet.Rows.Count; r++)
                {
                    if (sheet.Rows[r][0]?.ToString().Trim().Equals("Days", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        daysRow = r;
                        break;
                    }
                }
                if (daysRow < 0)
                    throw new Exception("Could not locate the ‘Days’ row.");

                // 4) build your date → column map
                var dateCols = new List<(int Col, DateTime Date)>();
                for (int c = 2; c < sheet.Columns.Count; c++)
                {
                    var hdr = sheet.Rows[daysRow][c]?.ToString().Trim();
                    if (int.TryParse(hdr?.Split(' ')[0], out var dayNum))
                    {
                        dateCols.Add((c, startDate.AddDays(dayNum - 1)));
                    }
                }
                if (!dateCols.Any())
                    throw new Exception("No day-columns found.");

                // 5) Prepare your target DataTable
                var dt = new DataTable();
                dt.Columns.Add("userid", typeof(int));
                dt.Columns.Add("DepartmentName", typeof(string));
                dt.Columns.Add("EmployeeCode", typeof(string));
                dt.Columns.Add("EmployeeName", typeof(string));
                dt.Columns.Add("AttendanceDate", typeof(DateTime));
                dt.Columns.Add("presentStatus", typeof(string));
                dt.Columns.Add("InTime", typeof(TimeSpan));
                dt.Columns.Add("OutTime", typeof(TimeSpan));
                dt.Columns.Add("status", typeof(bool));

                // 6) Loop every row and look for “Emp. Code”
                for (int r = daysRow + 3; r < sheet.Rows.Count; r++)
                {
                    var firstCell = sheet.Rows[r][0]?.ToString().Trim();
                    if (!firstCell?.StartsWith("Emp. Code", StringComparison.OrdinalIgnoreCase) == true)
                        continue;

                    // parse emp code & name
                    var empCode = sheet.Rows[r][3]?.ToString().Trim();
                    var empName = sheet.Rows[r][13]?.ToString().Trim();

                    // row+1 = Status, +2 = InTime, +3 = OutTime, +4 = Total
                    var statusRow = sheet.Rows[r + 1];
                    var inRow = sheet.Rows[r + 2];
                    var outRow = sheet.Rows[r + 3];
                    // var totalRow  = sheet.Rows[r + 4];  // if you need Total

                    // flatten each date‐column
                    foreach (var (col, date) in dateCols)
                    {
                        var status = statusRow[col]?.ToString().Trim();
                        var inStr = string.IsNullOrWhiteSpace(inRow[col]?.ToString()) ? "00:00" : inRow[col]?.ToString().Trim();
                        var outStr = string.IsNullOrWhiteSpace(outRow[col]?.ToString()) ? "00:00" : outRow[col]?.ToString().Trim(); ;

                        if (string.IsNullOrEmpty(status)
                         && string.IsNullOrEmpty(inStr)
                         && string.IsNullOrEmpty(outStr))
                            continue;

                        TimeSpan.TryParse(inStr, out var inTs);
                        TimeSpan.TryParse(outStr, out var outTs);

                        dt.Rows.Add(
                          userid,
                          department,
                          empCode,
                          empName,
                          date,
                          status,
                          inTs,
                          outTs,
                          true
                        );
                    }

                    // skip past this employee’s 5-row block
                    r += 4;
                }

                // 7) Bulk‐insert just like before
                using (var conn = new SqlConnection(
                       ConfigurationManager.ConnectionStrings["myconn"].ConnectionString))
                {
                    conn.Open();
                    using (var bulk = new SqlBulkCopy(conn))
                    {
                        bulk.DestinationTableName = "tbl_AttendanceRecordsByExcel";
                        bulk.ColumnMappings.Add("userid", "userid");
                        bulk.ColumnMappings.Add("DepartmentName", "DepartmentName");
                        bulk.ColumnMappings.Add("EmployeeCode", "EmployeeCode");
                        bulk.ColumnMappings.Add("EmployeeName", "EmployeeName");
                        bulk.ColumnMappings.Add("AttendanceDate", "AttendanceDate");

                        // camelCase → PascalCase
                        bulk.ColumnMappings.Add("presentStatus", "PresentStatus");

                        bulk.ColumnMappings.Add("InTime", "InTime");
                        bulk.ColumnMappings.Add("OutTime", "OutTime");
                        bulk.ColumnMappings.Add("status", "status");
                        bulk.WriteToServer(dt);
                    }
                }

                TempData["Message"] = $"Inserted {dt.Rows.Count} records successfully.";
                return RedirectToAction("UploadAttendaceReportForStaff");
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Could not locate the date-range cell.";
                return RedirectToAction("UploadAttendaceReportForStaff");
            }
        }

        #endregion


        public ActionResult FeeReport()
        {
            int userid = homeService.GetUserId(User.Identity.Name);
            var feerecord = adminservices.GetAllFeeRecord(userid);
            return View(feerecord);
        }

        [HttpPost]
        public JsonResult GetStudentRemainingFee(int studentId)
        {
            try
            {
                var feeInfo = adminservices.GetRemainingFeeByStudentId(studentId); 

                if (feeInfo != null)
                {
                    return Json(new
                    {
                        success = true,
                        feesPaid = feeInfo.FeesPaid,
                        remainingFee = feeInfo.RemainingFees
                    });
                }
                else
                {
                    return Json(new { success = false, message = "No record found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult AddExam(ExamModel model)
        {
            model.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertExam(model);
            return Json(new
            {
                status = res,
                message = res ? "Added Successfully" : "Already Added!"
            });
        }
        public ActionResult ScheduleExam()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewData["exams"] = adminservices.GetExam(userId);
            var exams = adminservices.GetScheduledExam(userId);
            ViewBag.academicyears = exams.Select(d => d.academicYear).Distinct();
            return View(exams);
        }
        [HttpPost]
        public ActionResult ScheduleExam(ScheduleExamModel model)
        {
            model.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            bool res = adminservices.InsertScheduleExam(model);
            return Json(new
            {
                status = res,
                message = res ? (model.scheduleId > 0 ? "Updated Successfully" : "Added Successfully") : "Some issue occurred"
            });
        }
        [HttpGet]
        public JsonResult DeleteScheduledExam(int id)
        {
            bool res = adminservices.DeleteScheduledExam(id);
            return Json(new
            {
                status = res,
                message = res ? "Deleted Successfully" : "Some issue occurred"
            }, JsonRequestBehavior.AllowGet);
        }

      public ActionResult ExamMarkSheet()
      {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewData["exams"] = adminservices.GetScheduledExamForMarkSheet(userId);
            ViewData["classes"] = adminservices.GetAllClasses(userId);
            return View();
      }
        [HttpGet]
        public JsonResult GetSubjectsByClass(int id)
        {
            try
            {
                var subjects = adminservices.GetSubjectsByClassId(id);
                return Json(subjects, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult InsertExamMarksheet(ExamMarksheetViewModel model)
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string message;
            bool success = adminservices.InsertExamMarksheet(model, userId, out message);

            return Json(new { success = success, message = message });
        }

        [HttpGet]
        public JsonResult GetExistingMarks(int classId, int examId,int subjectId)
        {
            try
            {
                int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                var data = adminservices.GetExistingMarks(classId, examId, userId, subjectId);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
     

        public ActionResult StudentNumberAllocation()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewData["exams"] = adminservices.GetScheduledExamForMarkSheet(userId);
            ViewData["classes"] = adminservices.GetAllClasses(userId);
            return View();
        }
        [HttpPost]
        public JsonResult InsertStudentMarks(StudentMarksheetViewModel model)
        {
            if (model.Marks == null)
            {
                return Json(new { success = false, message = "Please select at least one student." });
            }
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            string message;
            bool success = adminservices.InsertMarksOfStudent(model, userId, out message);

            return Json(new { success = success, message = message });
        }

        [HttpGet]
        public JsonResult GetPreInsertedStudentMarks(int classId,int sectionId, int subjectId, int examId)
        {
            try
            {
                int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                var data = adminservices.GetPreInsertedData(classId, sectionId, userId, subjectId, examId);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult HostelBlockOverview()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewData["blocks"] = adminservices.GetAllBlock(userId);
            return View();
        }
        [HttpGet]
        public JsonResult GetBlockOverviewData(int blockId)
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            HostelBlockOverviewModel model = adminservices.HostelBlockOverview(userId, blockId);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetWardenByBlock(int blockId)
        {
            string wardenName = adminservices.GetWardenByBlockId(blockId);

            if (!string.IsNullOrEmpty(wardenName))
            {
                return Json(new { success = true, wardenName = wardenName }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LibraryOverview()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var data = adminservices.GetLibraryCount(userId);
            return View(data);
        }

        [HttpGet]
        public JsonResult GetBookCategoriesByUser()
        {
            try
            {
                int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                var categories = adminservices.GetBookCategoriesByUser(userId);

                return Json(new { success = true, data = categories }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error fetching categories." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult StudentReportCard()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            ViewData["exams"] = adminservices.GetScheduledExamForMarkSheet(userId);
            ViewData["classes"] = adminservices.GetAllClasses(userId);
            return View();
        }
        [HttpGet]
        public JsonResult GetStudentReportCard(int studentId, string academicYear, int examId)
        {
            try
            {
                var reportCard = adminservices.GetStudentReportCard(studentId, academicYear, examId, out double totalpercentage);
                return Json(reportCard, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ReportCard(int studentId, int examId, string academicYear)
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var reportData = adminservices.GetStudentReportCard(studentId, academicYear, examId,out double totalpercentage);
            var studentInfo = adminservices.GetStudentById(studentId);
            var companyInfo = superAdminDataService.GetCompanyById(userId);
           
            ViewBag.StudentInfo = studentInfo;
            ViewBag.CompanyInfo = companyInfo;
            ViewBag.ReportData = reportData;
            return View(reportData);
        }
        public ActionResult DriverProfile(int? id)
        {
            if (id != null && id > 0)
            {
                var data = driverService.GetDriverProfile(Convert.ToInt32(id));
                return View(data);
            }
            return RedirectToAction("adddriver");
        }

        public ActionResult AdminProfile()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var data = adminservices.GetAdminById(userId);
            return View(data);
        }

        public JsonResult GetBusNumbers()
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var data = adminservices.GetAllBusListAndCharge(userId);
            return Json(data,JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStudentsByBusId(int bussNo)
        {
            int userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var data = adminservices.GetStudentByBusId(userId, bussNo);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SubmitBusFee(BusFeeModel model, string[] billingMonth)
        {
            try
            {
                model.userId = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
                model.billingMonths = billingMonth;
                model.feeSlip = adminservices.UploadImageToServer(model.feeSlips);
                bool res = adminservices.InsertBusFee(model, out string error);
                if (res)
                {

                    return Json(new { success = true, message = "Bus Fee Submitted Successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = error });

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public ActionResult StudentFeesTransactionHistory(int studentId)
        {
            int userid = Convert.ToInt32(homeService.GetUserId(User.Identity.Name));
            var records = adminservices.GetStudentMonthlyFeeSummary(studentId, userid);
            return View(records);
        }
        [HttpGet]
        public ActionResult GetOptionalSubjectByStreamId(int streamId)
        {
            var data = adminservices.GetOptionalSubjectByStreamId(streamId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}