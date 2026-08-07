using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace College_ERP.Models.DriverServices
{
    public class main
    {
        public class DriverProfileModel
        {
            public int userId { get; set; }
            public int Id { get; set; }
            public int BusId { get; set; }
            public string Name { get; set; }         
            public string EmployeeId { get; set; }
            public long ContactNo { get; set; }
            public string FatherName { get; set; }
            public string Email { get; set; }
            public string DLNo { get; set; }
            public long AdharCardNo { get; set; }
            public string Address { get; set; }
            public long Salary { get; set; }
            public string DriverFileName { get; set; }
            public string AdharCardFileName { get; set; }
            public string DLFileName { get; set; }
            public string Type { get; set; }
            public int BusNo { get; set; }
            public int BusSeatCapacity { get; set; }
            public string BusImage { get; set; }
            public string TravelCompanyName { get; set; }
            public string ContactPerson { get; set; }
            public long ContactNumber { get; set; }
            public int BussCharge { get; set; }
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
        public class NoticesModel
        {
            public int NoticeId { get; set; }
            public int UserId { get; set; }
            public int ClassId { get; set; }
            public int SectionId { get; set; }
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
            public int IsSentToBothStudentParent { get; set; }
            public DateTime CreatedOn { get; set; }
        }
        public class DriverProblemModel
        {
            public int driverid { get; set; }
            public string problem { get; set; }
            public string createdAt { get; set; }
            public int problemStatus { get; set; }
            public string reason { get; set; }
        }

        public class StudentListInBusModel
        {
            public int driverid { get; set; }
            public int busId { get; set; }
            public string busNo { get; set; }
            public string pickupPoint { get; set; }
            public int studentid { get; set; }
            public int classid { get; set; }
            public int sectionid { get; set; }
            public string studentName { get; set; }
            public string studentPhoto { get; set; }
            public string className { get; set; }
            public string sectionName { get; set; }
            public string gender { get; set; }
            public string address { get; set; }
            public string currentAddress { get; set; }
            public long mobileNo { get; set; }
            public string fatherName { get; set; }
            public long fatherMobileNo { get; set; }
            
        }

        public class PickupPointModel
        {
            public int driverid { get; set; }
            public int busrouteid { get; set; }
            public string busNo { get; set; }
            public int busCharge { get; set; }
            public string route { get; set; }
            public string stateName { get; set; }
            public string cityName { get; set; }
            public string pickupPoint { get; set; }

        }

        public class driverdashboardcount
        {
            public int TotalStudents { get; set; }
            public int MaleStudents { get; set; }
            public int FemaleStudents { get; set; }
            public int PickupPointCount { get; set; }
        }
    }
}