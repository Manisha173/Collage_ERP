using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using College_ERP.Models.Admin;

namespace College_ERP.Models.StudentServices
{
    public class main
    {
        public class StudentProfileModel
        {
            public bool IsInHostel { get; set; }
            public string parentEmail { get; set; }
            public string AcademicYear { get; set; }
            public string AdmissionStage { get; set; }
            public string assigmentattachment { get; set; }
            public int? userid { get; set; }
            public int? StudentId { get; set; }
            public string AdmissionNo { get; set; }
            public string StudentName { get; set; }
            public string BloodGroup { get; set; }
            public string MotherTongue { get; set; }
            public bool AttendaceStatus { get; set; }
            public DateTime DateOfAdmission { get; set; }
            public string Gender { get; set; }
            public string Religion { get; set; }
            public string Nationality { get; set; }
            public DateTime DOB { get; set; }
            public string Caste { get; set; }
            public string AadharNo { get; set; }
            public string StudentEmail { get; set; }

            public string CurrentAddress { get; set; }
            public string Address { get; set; }
            public string Hobbies { get; set; }
            public string ClassName { get; set; }
            public int ClassId { get; set; }
            public int SectionId { get; set; }
            public string SectionName { get; set; }

            public string PlaceOfBirth { get; set; }
            public string StateName { get; set; }
            public int? StateId { get; set; }
            public int? CityId { get; set; }
            public string CityName { get; set; }
            public string LastSchoolAttended { get; set; }

            public string YearOfPassing { get; set; }
            public int? TotalMarks { get; set; }
            public int? ObtainedMarks { get; set; }
            public decimal Percentage { get; set; }
            public string MobileNo { get; set; }



            public string FatherName { get; set; }
            public string MotherName { get; set; }
            public long FatherOfficeMobileNo { get; set; }
            public long MotherOfficeMobileNo { get; set; }
            public string FatherQualification { get; set; }
            public string MotherQualification { get; set; }
            public string FatherOccupation { get; set; }
            public string MotherOccupation { get; set; }
            public string FatherOfficeAddress { get; set; }
            public string MotherOfficeAddress { get; set; }

            public HttpPostedFileBase StudentPhoto { get; set; }
            public string StudentPhotos { get; set; }

            public HttpPostedFileBase FatherPhoto { get; set; }
            public string FatherPhotos { get; set; }

            public HttpPostedFileBase MotherPhoto { get; set; }
            public string MotherPhotos { get; set; }
            public HttpPostedFileBase StudentAadharPhoto { get; set; }
            public string StudentAadharPhotos { get; set; }

            public string AdminName { get; set; }
            public string SchoolName { get; set; }
            public string SchoolauthorizedPersonName { get; set; }
            public string schoolMobile { get; set; }
            public string LandLineNo { get; set; }
            public string website { get; set; }


        }
        public class SubmitAssignmentModel
        {
            public int? id { get; set; }
            public int? studentId { get; set; }
            public HttpPostedFileBase attachment { get; set; }
            public string attachmentUrl { get; set; }
        }
        public class StudentAssignmentModel
        {
            public int? id { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public int? StudentId { get; set; }
            public int? ClassId { get; set; }
            public int? SectionId { get; set; }
            public string StudentName { get; set; }
            public string assigmentattachment { get; set; }
            public string assignmentDate { get; set; }
            public string completionDate { get; set; }
        }
        public class CircularModel
        {
            public int? userid { get; set; }
            public int? CircularId { get; set; }

            public string CircularTitle { get; set; }

            public DateTime CircularDate { get; set; }

            public HttpPostedFileBase Attachment { get; set; }

            public string UploadAttachment { get; set; }

            public string CircularDescription { get; set; }
        }
        public class CourseModel
        {
            public string subjectName { get; set; }
            public string teacherName { get; set; }
        }
        public class LibraryModel
        {
            public int? receiveQuantity { get; set; }
            public int? damageQuantity { get; set; }
            public decimal totalLateFine { get; set; }
            public int? totalDelayDaysCount { get; set; }
            public string addedBy { get; set; }
            public bool recieveStatus { get; set; }
            public string bookName { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public long mobile { get; set; }
            public string orderId { get; set; }
            public string returnDate { get; set; }
            public int? bookId { get; set; }
            public int? userId { get; set; }
            public int? id { get; set; }
            public int? buyerId { get; set; }
            public string userType { get; set; }
            public DateTime orderDate { get; set; }
            public string orderDateString { get; set; }
            public decimal lateFine { get; set; }
            public decimal damageFine { get; set; }
            public decimal lostFine { get; set; }
            public int? quantity { get; set; }
            public decimal price { get; set; }
            public decimal pricePerBook { get; set; }
        }
        public class ExamTimeTableModel
        {
            public string sectionName { get; set; }
            public string examName { get; set; }
            public int? id { get; set; }
            public int? userId { get; set; }
            public int? classId { get; set; }
            public int? sectionId { get; set; }
            public string className { get; set; }
            public string academicYear { get; set; }
            public HttpPostedFileBase attachment { get; set; }
            public string attachmentName { get; set; }
            public string description { get; set; }
        }
        public class TodaySchedulesModel
        {
            public string teacherName { get; set; }
            public string day { get; set; }
            public int? id { get; set; }
            public string className { get; set; }
            public string sectionName { get; set; }
            public string subjectName { get; set; }
            public int? subjectId { get; set; }
            public string from { get; set; }
            public string to { get; set; }
            public string combineTime { get; set; }
        }
        public class NoticesModel
        {
            public int? NoticeId { get; set; }
            public int? UserId { get; set; }
            public int? ClassId { get; set; }
            public int? SectionId { get; set; }
            public string AcademicYear { get; set; }
            public int? ReceiverId { get; set; }
            public string UserType { get; set; }
            public string Title { get; set; }
            [AllowHtml]
            public string Description { get; set; }
            public HttpPostedFileBase Attachment { get; set; }
            public string Attachments { get; set; }
            public string ReceiverName { get; set; }
            public bool AllTeacherStatus { get; set; }
            public bool AllWardenStatus { get; set; }
            public bool AllSecurityStatus { get; set; }
            public bool AllDriverStatus { get; set; }
            public bool AllLibrarianStatus { get; set; }
            public bool AllStudentStatus { get; set; }
            public bool AllParentStatus { get; set; }
            public int? IsSentToBothStudentParent { get; set; }
            public DateTime CreatedOn { get; set; }
        }
        public class TrasportDetailsModel
        {
            public decimal busCharge { get; set; }
            public long ContactNo { get; set; }
            public string Address { get; set; }
            public string DLFileName { get; set; }
            public string pickupPoint { get; set; }
            public string driverName { get; set; }
            public string bussNo { get; set; }
        }
        public class stCommunicationModel
        {
            public string teacherName { get; set; }
            public int? id { get; set; }
            public string title { get; set; }
            [AllowHtml]
            public string description { get; set; }
            public string attachmentName { get; set; }
        }
        public class  FeeModel
        {
            public string className { get; set; }
            public List<FeeDetailModel> feeDetails { get; set; }    
        }
        public class FeeDetailModel
        {
            public decimal totalFee { get; set; }
            public decimal amount { get; set; }
        }
        public class HostelDetailModel
        {
            public string feeSlip { get; set; }
            public string feeType { get; set; }
            public string roomNo { get; set; }
            public string blockName { get; set; }
            public string roomType { get; set; }
            public decimal totalFee { get; set; }
            public decimal paidFee { get; set; }
            public decimal remainingFee { get; set; }
            public string wardenName { get; set; }
            public string wardenEmail { get; set; }
            public long wardenMobile { get; set; }
            public int? roommates { get; set; }
        }
        public class MealMenuModel
        {
            public string menu { get; set; }
        }
        public class MealMode
        {
            public string starttime { get; set; }
            public string endtime { get; set; }
            public string Day { get; set; }
            public List<MealMenuModel> menus { get; set; }
        }
        public class AttendanceModel
        {
            public bool attendanceStatus { get; set; }
            public string attendanceDate { get; set; }
        }
        public class HostelProblemModel
        {
            public int? studentid { get; set; }
            public string problem { get; set; }
            public string createdAt { get; set; }
            public int? problemStatus { get; set; }
            public string reason { get; set; }
        }
        public class WardenCommunicationModel
        {
            public int? CommunicationId { get; set; }
            public int? userid { get; set; }
            public string Title { get; set; }
            public string Attachment { get; set; }
            public HttpPostedFileBase Attachments { get; set; }

            [AllowHtml]
            public string Description { get; set; }
        }

        public class StudentAttendanceModel
        {
            public int? studentId { get; set; }
            public int? examId { get; set; }
            public string examName { get; set; }
            public string studentName { get; set; }
            public int? theoryMarks { get; set; }
            public int? practicalMarks { get; set; }           
            public int? subjectid { get; set; }
            public string subjectName { get; set; }
        }


        public class StudentDashboardCount
        {
            public int? TimeTableCount { get; set; }
            public int? AssignmentCount { get; set; }
            public int? HostelProblemCount { get; set; }
        }

        public class StudentICardDetails
        {
            public int studentid { get; set; }
            public string schoolname { get; set; }
            public string studentname { get; set; }
            public int classid { get; set; }
            public string classname { get; set; }
            public int sectionid { get; set; }
            public string sectionname { get; set; }
            public string address { get; set; }
            public string studentemail { get; set; }
            public string fathername { get; set; }
            public string mothername { get; set; }
            public string motherofficeno { get; set; }
            public string fatherofficeno { get; set; }
            public string studentphoto { get; set; }
            public string motherphoto { get; set; }
            public string fatherphoto { get; set; }
        }
    }
}