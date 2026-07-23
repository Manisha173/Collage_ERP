using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using College_ERP.Models.Admin;

namespace College_ERP.Models.Teacher
{
    public class main
    {
    }
    public class CircularModel
    {
        public int userid { get; set; }
        public int CircularId { get; set; }

        public string CircularTitle { get; set; }

        public DateTime CircularDate { get; set; }

        public HttpPostedFileBase Attachment { get; set; }

        public string UploadAttachment { get; set; }

        public string CircularDescription { get; set; }
    }
    public class TeacherModel
    {
        public string EmployeeId { get; set; }
        public int TeacherId { get; set; }
        public string RegistrationNo { get; set; }


        // Personal Information
        [Required]
        public string TeacherName { get; set; }
        [Required]
        public string address { get; set; }

        [Required]
        [EmailAddress]
        public string TeacherEmail { get; set; }

        [Required]
        [Phone]
        public string TeacherMobile { get; set; }

        [Required]
        public DateTime? TeacherDOB { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string MotherTongue { get; set; }

        [Required]
        public string BloodGroup { get; set; }

        [Required]
        public string Nationality { get; set; }

        [Required]
        public string Religion { get; set; }

        [Required]
        public string MaritalStatus { get; set; }

        [Required]
        public string PanNo { get; set; }

        [Required]
        public int Experience { get; set; }

        public string Subject { get; set; }

        [Required]
        public DateTime? JoinDate { get; set; }

        public string LastSchoolName { get; set; }

        public string LastEmpAddress { get; set; }

        [Required]
        public string Caste { get; set; }
        [Required(ErrorMessage = "Admin userid are required")]
        public int userid { get; set; }

        // File Uploads
        public HttpPostedFileBase ExperienceDocument { get; set; }
        public string ExperienceDocumentPath { get; set; }

        // Academic Qualifications
        public string TenthBoard { get; set; }
        public string TenthPassoutYear { get; set; }
        public string TenthPercent { get; set; }
        public HttpPostedFileBase TenthMarksheet { get; set; }

        public string TenthMarksheetPath { get; set; }

        public string TwelfthBoard { get; set; }
        public string TwelfthPassoutYear { get; set; }
        public string TwelfthPercent { get; set; }
        public HttpPostedFileBase TwelfthMarksheet { get; set; }
        public string TwelfthMarksheetPath { get; set; }

        public HttpPostedFileBase profileImage { get; set; }
        public string profileImagePath { get; set; }

        public string GraduationDegree { get; set; }
        public string GraduationStream { get; set; }
        public string GraduationYear { get; set; }
        public string GraduationPercent { get; set; }
        public HttpPostedFileBase GraduationMarksheet { get; set; }
        public string GraduationMarksheetPath { get; set; }
        public HttpPostedFileBase PostGraduationMarksheet { get; set; }
        public string PostGraduationMarksheetPath { get; set; }
        public string OtherDiplomaMarksheetPath { get; set; }

        public string PostGraduationDegree { get; set; }
        public string PostGraduationStream { get; set; }
        public string PostGraduationYear { get; set; }
        public string PostGraduationPercent { get; set; }

        // Other Diploma
        public string OtherDiplomaDegree { get; set; }
        public string OtherDiplomaStream { get; set; }
        public string OtherDiplomaYear { get; set; }
        public string OtherDiplomaPercent { get; set; }
        public HttpPostedFileBase OtherDiplomaMarksheet { get; set; }

        // Bank Details

        public string BankName { get; set; }

        public string AccountHolderName { get; set; }

        public long? BankAccountNumber { get; set; }

        public long? ReenterBankAccountNumber { get; set; }

        public string IfscCode { get; set; }

    }
    public class SubjectAssignModel
    {
        public int userid { get; set; }
        public int AssignedId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int StudentCount { get; set; }
        public TimeSpan StartTime { get; set; }
        public string StartTimee { get; set; }
        public TimeSpan EndTime { get; set; }
        public string EndTimee { get; set; }

    }
    public class StudentAttendance
    {
        public int StudentId { get; set; }
        public bool AttendanceStatus { get; set; }
    }

    public class AttendanceSubmitModel
    {
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public int SectionId { get; set; }
        public DateTime attendanceDate { get; set; }
        public List<StudentAttendance> AttendanceList { get; set; }
    }


    public class StudentModel
    {
        public int assid { get; set; }
        public string assigmentattachment { get; set; }
        public int userid { get; set; }
        public int StudentId { get; set; }
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
        public int StateId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string LastSchoolAttended { get; set; }

        public string YearOfPassing { get; set; }
        public int TotalMarks { get; set; }
        public int ObtainedMarks { get; set; }
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


    }
    public class Assignment
    {
        public int Id { get; set; }
        [Required]
        public int TeacherId { get; set; }
        [Required]
        public int ClassId { get; set; }
        [Required]
        public int SectionId { get; set; }
        [Required]
        public int SubjectId { get; set; }
        [Required]
        public string AcademicYear { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        public string classname { get; set; }
        public string sectionname { get; set; }
        public string subjectname { get; set; }

        public HttpPostedFileBase Attachment { get; set; }
        public string AttachmentUrl { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? AssignDate { get; set; }
    }

    public class StudentAssignmentModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string FilePath { get; set; }       
        public string Email { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public string MobileNo { get; set; }
        public string FatherName { get; set; }
        public string Status { get; set; }

    }

    public class LeaveRequestModel
    {
        public string remark { get; set; }
        public int userId { get; set; }
        public int id { get; set; }
        public int teacherId { get; set; }
        public string leaveType { get; set; }
        public DateTime fromDate { get; set; }
        public string fromDateString { get; set; }
        public DateTime toDate { get; set; }
        public string toDateString { get; set; }
        public string reason { get; set; }
        public HttpPostedFileBase attachment { get; set; }
        public string attachmentName { get; set; }
        public int approvalStatus { get; set; }
    }
    public class TeacherCommunicationModel
    {
        public string className { get; set; }
        public string secitonName { get; set; }
        public string student{ get; set; }
        public int id { get; set; }
        public int IsSendTo { get; set; }
        public int ClassId { get; set; }
        public string academicyear { get; set; }
        public string title { get; set; }
        public HttpPostedFileBase Attachment { get; set; }
        [AllowHtml]
        public string description { get; set; }
        public int SectionId { get; set; }
        public int StudentName { get; set; }
        public string attachmentName { get; set; }
        public int teacherId { get; set; }
    }
    public class EventCategoryModel
    {
        public int userid { get; set; }
        public int Id { get; set; }
        public string CategoryName { get; set; }

        public HttpPostedFileBase CategoryImage { get; set; }

        public string CategoryDescription { get; set; }
        public string CategoryImg { get; set; }

        public string CreatedDate { get; set; }

    }
    public class TeacherDashboardCountResult
    {
        public int assignedclass { get; set; }
        public int assignedtask { get; set; }
        public int communication { get; set; }
        public int circular { get; set; }
        public int assignments { get; set; }
        public int borrowedbooks { get; set; }
        public int notice { get; set; }
    }
    public class PendingTaskModel
    {
        public string TaskTitle { get; set; }
        public int TaskStatus { get; set; }
        public string CompletionDate { get; set; }
    }


    public class TodayScheduleModel
    {
        public string day { get; set; }
        public int id { get; set; }
        public string className { get; set; }
        public string sectionName { get; set; }
        public string subjectName { get; set; }
        public int subjectId { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string combineTime { get; set; }
    }
    public class FullWeakScheduleModel
    {
        public int classId { get; set; }
        public int tid { get; set; }
        public string day { get; set; }
        public int uid { get; set; }
        public int id { get; set; }
        public string subjectName { get; set; }
        public int subjectId { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string className { get; set; }
        public string sectionName { get; set; }
        public List<SubjectTimesModel> ttdata { get; set; }
    }
    public class SubjectTimesModel
    {
        public int classId { get; set; }
        public int tid { get; set; }
        public string day { get; set; }
        public int uid { get; set; }
        public int id { get; set; }
        public string subjectName { get; set; }
        public int subjectId { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string className { get; set; }
        public string sectionName { get; set; }
    }
    public class NoteModel
    {
        public int UserId { get; set; }
        public int NoteId { get; set; }
        public int ClassId { get; set; }
        public int SectionId { get; set; }
        public int SubjectId { get; set; }
        public string AcademicYear { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SubjectName { get; set; }
        public string Attachmentpath { get; set; }  
        public HttpPostedFileBase Attachment { get; set; }  
    }
    public class SubmitGradeModel
    {
        public int studentId { get; set; }
        public int assignmentId { get; set; }
        public string grade { get; set; }
        public string remark { get; set; }
    }
    public class HolidayModel
    {
        public int HolidayId { get; set; }
        public string Title { get; set; }
        public string HolidayType { get; set; }
        public DateTime HolidayDateFrom { get; set; }
        public DateTime HolidayDateTo { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
    }

}