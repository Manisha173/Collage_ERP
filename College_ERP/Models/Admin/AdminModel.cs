using Antlr.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebGrease.Css.Ast.Selectors;
using System.Web.Mvc;
using System.Net.Mail;
using System.Diagnostics.Contracts;

namespace College_ERP.Models.Admin
{
    public class AdminDetails
    {
        public string adminName { get; set; }
        public int adminId { get; set; }
        public string adminImage { get; set; }
    }
    public class AdminModel
    {
        public string transactionid { get; set; }
        public int userid { get; set; }
        public int HostelId { get; set; }
        public string Block { get; set; }
        public int BlockId { get; set; }
        public int RoomNumber { get; set; }
        public int RoomId { get; set; }
        public string RoomTypes { get; set; }

        public int StudentId { get; set; }
        public int FeesSubmitted { get; set; }
        public int RemainingFees { get; set; }
        public string StudentName { get; set; }
        public string FeeType { get; set; }
        public string FeeSlip { get; set; }
        public string DueDate { get; set; }
        public HttpPostedFileBase FeeSlips { get; set; }
    }
    public class ExcelAttendanceRecordModel
    {
        public int id { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string presentStatus { get; set; }
        public TimeSpan? InTime { get; set; }
        public TimeSpan? OutTime { get; set; }
    }


    public class SubjectAssignModel
    {
        public int userid { get; set; }
        public int teacherId { get; set; }
        public int AssignedId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }

        public TimeSpan StartTime { get; set; }
        public string StartTimee { get; set; }
        public TimeSpan EndTime { get; set; }
        public string EndTimee { get; set; }

    }

    public class Holiday
    {
        public int HolidayId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime HolidayDate { get; set; }
        public DateTime HolidayDateTo { get; set; }

        [Required(ErrorMessage = "Please select a type")]
        public string HolidayType { get; set; }

        public string Description { get; set; }
        public int userid { get; set; }
        public int year { get; set; }
    }

    public class RegistrationModel
    {
        public int userid { get; set; }

        public List<int> OptionalSubjectIds { get; set; }
        public int StudentId { get; set; }
        public int classStreamId { get; set; }
        public string AdmissionNo { get; set; }
        public string Stream { get; set; }
        public string educationLevel { get; set; }
        public string parentEmail { get; set; }
        public string StudentName { get; set; }
        public string BloodGroup { get; set; }
        public string MotherTongue { get; set; }
        public DateTime? DateOfAdmission { get; set; }
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
        public string AcademicYear { get; set; }
        public string AdmissionStage { get; set; }

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
        public bool IsUpdate { get; set; }

        public ClassModel ClassModel { get; set; }
        public ClassStream ClassStream { get; set; }

        public string username { get; set; }
        public string password { get; set; }

    }

    public class TeacherModel
    {
        public int TeacherId { get; set; }
        public string EmployeeId { get; set; }
        public string RegistrationNo { get; set; }
        public string empCode { get; set; }


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
        public int designation { get; set; }
        public string designationName { get; set; }

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
        public string username { get; set; }
        public string password { get; set; }

    }

    public class ClassModel
    {
        public int Classstreamid { get; set; }
        public string ClassStream { get; set; }
        public int userid { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string ClassDescription { get; set; }
        public string InstitutionType { get; set; }
        public string EducationLevel { get; set; }
        public List<string> Class { get; set; }
        public string DiplomaType { get; set; }
        public string UgType { get; set; }
        public string PgType { get; set; }
        public string PhdType { get; set; }
        public List<string> Specialization { get; set; }
        public List<string> Stream { get; set; }
        public bool HasStream { get; set; }
    }

    public class SectionModel
    {
        public string ClassName { get; set; }
        public int ClassId { get; set; }
        public int classStreamId { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public string Stream { get; set; }
        public string SectionDescription { get; set; }
    }

    public class DesignationModel
    {
        public int DesignationId { get; set; }
        public string DesignationName { get; set; }
        public string DesignationDescription { get; set; }
    }

    public class SubjectModel
    {
        public int optionsub { get; set; }
        public string classstream { get; set; }
        public List<string> optionalsubject { get; set; }
        public int classStreamId { get; set; }
        public int SubjectId { get; set; }
        public int userid { get; set; }
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string Subject { get; set; }
        public List<string> Subjects { get; set; }
        public string Description { get; set; }
        public List<SubjectsModel> sb { get; set; }
    }
    public class SubjectsModel
    {
        public string Subject { get; set; }
        public int SubjectId { get; set; }
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
    public class AwardModel
    {
        public int userid { get; set; }
        public int AwardId { get; set; }
        public string AwardName { get; set; }
        public string AwardTitle { get; set; }
        public DateTime AwardDate { get; set; }
        public string AwardCertificate { get; set; }
        public HttpPostedFileBase AwardCertificates { get; set; }
        public string AwardDescription { get; set; }



    }
    public class StudentAwardModel
    {
        public int userid { get; set; }
        public int AwardId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }

        public string AwardTitle { get; set; }
        public int ClassId { get; set; }
        public int SectionId { get; set; }
        public string Session { get; set; }
        public int SessionId { get; set; }
        public DateTime AwardDate { get; set; }

        public string Description { get; set; }
        public HttpPostedFileBase Certificate { get; set; }
        public string CertificatePath { get; set; }

        public string AwardType { get; set; }

    }


    public class TeacherAwardModel
    {
        public int awardid { get; set; }
        public int userid { get; set; }
        public string teacherName { get; set; }
        public int TeacherId { get; set; }
        public DateTime awardDate { get; set; }
        public string awardSession { get; set; }
        public string awardTitle { get; set; }
        public HttpPostedFileBase certificate { get; set; }
        public string awardcertificate { get; set; }
        public string awardType { get; set; }
        public string awardDesc { get; set; }

    }
    public class AcademicVacationModel
    {
        public int userid { get; set; }
        public int VacationId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string VacationName { get; set; }
        public string VacationType { get; set; }
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public HttpPostedFileBase Images { get; set; }
        public string Image { get; set; }

    }

    public class FestivalHoliday
    {
        public string FestivalName { get; set; }
        public int userid { get; set; }
        public int FestivalId { get; set; }
        public string Day { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }

    public class RegistrationFeeModel
    {
        public int userid { get; set; }
        public int RegistrationId { get; set; }
        public string ClassName { get; set; }
        public int ClassId { get; set; }
        public int RegistrationFee { get; set; }
    }

    public class CollegeFeeModel
    {
        public int userid { get; set; }
        public int FeeId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string BillingPeriod { get; set; }
        public decimal AdmissionFee { get; set; }
        public decimal BuildingFee { get; set; }
        public decimal TutionFee { get; set; }
        public decimal SportsFee { get; set; }
        public decimal LibraryFee { get; set; }
        public decimal ActivityFee { get; set; }
        public decimal AnnualCharge { get; set; }
        public decimal TotalFee { get; set; }
    }

    public class DiscountFeeModel
    {
        public int userid { get; set; }
        public int FeeId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }

        public decimal Fee { get; set; }
        public int Discount { get; set; }
        public decimal AfterDiscountFee { get; set; }
        public DateTime DiscountStartDate { get; set; }
        public DateTime DiscountEndDate { get; set; }


    }

    public class BlockModel
    {
        public int TotalFlourInBlock { get; set; }
        public string blockType { get; set; }
        public string GenderType { get; set; }
        public int userid { get; set; }
        public int HostelId { get; set; }
        //public int WardenId { get; set; }
        //public string WardenName { get; set; }
        public string BlockName { get; set; }
        public string TotalRoomInBlock { get; set; }
    }

    public class RoomTypeModel
    {
        public int userid { get; set; }
        public int RoomId { get; set; }
        public string RoomTypes { get; set; }
    }

    public class RoomNumberModel
    {
        public int floor { get; set; }
        public int userid { get; set; }
        public int RoomId { get; set; }
        public string BlockName { get; set; }
        public int BlockId { get; set; }
        public string RoomTypes { get; set; }
        public int BedCount { get; set; }
        public string RoomFacilitate { get; set; }
        public int RoomNumber { get; set; }
        public int FeesPerPerson { get; set; }

    }
    public class EventCategory
    {
        public int userid { get; set; }
        public int Id { get; set; }
        public string CategoryName { get; set; }

        public HttpPostedFileBase CategoryImage { get; set; }

        public string CategoryDescription { get; set; }
        public string todate { get; set; }
        public string fromdate { get; set; }
        public string combineDate { get; set; }

        public string CategoryImg { get; set; }

        public string CreatedDate { get; set; }

    }
    public class CommonModel
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class Event
    {
        public int Id { get; set; }
        public int EventCategoryId { get; set; }
        public string EventTitle { get; set; }
        public string EventHeading { get; set; }
        public string EventDate { get; set; }
        public string EventDay { get; set; }
        public string EventDescription { get; set; }
        public HttpPostedFileBase EventImage { get; set; }
        public string CreatedAt { get; set; }
    }
    public class InventoryCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int userId { get; set; }


    }
    public class Material
    {
        public int userId { get; set; }
        public int materialId { get; set; }
        public string categoryName { get; set; }
        public string materialName { get; set; }
        public DateTime createdAt { get; set; }
        public int categoryId { get; set; }
    }
    public class StockMaterial
    {
        public int userId { get; set; }
        public int materialId { get; set; }
        public int stockMaterialId { get; set; }
        public string quantity { get; set; }
        public string categoryName { get; set; }
        public string materialName { get; set; }
        public DateTime createdAt { get; set; }
        public int categoryId { get; set; }
    }
    public class PurchaseMaterial
    {
        public int userId { get; set; }
        public int materialId { get; set; }
        public int purchaseId { get; set; }
        public string quantity { get; set; }
        public string categoryName { get; set; }
        public string materialName { get; set; }
        public string supplierName { get; set; }
        public DateTime createdAt { get; set; }
        public int categoryId { get; set; }
        public decimal purchasePrice { get; set; }
        public string purchaseMedium { get; set; }
        public string billNo { get; set; }
        public HttpPostedFileBase billSlip { get; set; }
        public string billSlipName { get; set; }
    }


    public class warden

    {
        public string EmployeeId { get; set; }
        public string cityName { get; set; }
        public int Id { get; set; }
        public string DOBstring { get; set; }
        public int userId { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public DateTime? DOB { get; set; }
        public string Gender { get; set; }
        public string Document { get; set; }
        public string ProfilePics { get; set; }
        public string BlockName1 { get; set; }
        public string BlockName { get; set; }
        public int BlockId { get; set; }

        public HttpPostedFileBase Documents { get; set; }
        public HttpPostedFileBase ProfilePic { get; set; }
        public int st_Id { get; set; }

        public string stateName { get; set; }
        public int city_Id { get; set; }
        public string Address { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
    public class ShortMaterial
    {
        public int userId { get; set; }
        public int materialId { get; set; }
        public int stockMaterialId { get; set; }
        public string quantity { get; set; }
        public string categoryName { get; set; }
        public string materialName { get; set; }
        public string description { get; set; }
    
        public DateTime createdAt { get; set; }
        public int categoryId { get; set; }
    }
    public class Buss
    {
        public int BusSeat { get; set; }
        public int userId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string No { get; set; }
        public string DriverId { get; set; }
        public string Fees { get; set; }
        public string Type { get; set; }
        public string PersonName { get; set; }
        public long ContactNo { get; set; }
        public string CompanyName { get; set; }
        public int BusCharges { get; set; }
        public string BusNo { get; set; }
        public string SeatLimit { get; set; }
        public HttpPostedFileBase image { get; set; }
        public string ImageName { get; set; }

    }

    public class Drivers
    {
        public int userId { get; set; }
        public int Id { get; set; }
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
        public HttpPostedFileBase File1 { get; set; }
        public HttpPostedFileBase File2 { get; set; }
        public HttpPostedFileBase File3 { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
        public class BusRoute
    {
        public int StateId { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public int CityId { get; set; }
        public string type { get; set; }
        public int userId { get; set; }
        public int Id { get; set; }
        public string Destination { get; set; }
        public string BussNo { get; set; }
        public int BussNoId { get; set; }
        public int BusCharges { get; set; }
        public string Route { get; set; }
        public List<PickupPointModel> ppm { get; set; }
    }
    public class PickupPointModel
    {
        public string busNO { get; set; }
        public int id { get; set; }
        public string pickupPoint { get; set; }
    }
    public class AssignBus
    {
        public DateTime createdAt { get; set; }
        public int userId { get; set; }
        public int id { get; set; }
        public int busId { get; set; }
        public int driverId { get; set; }
        public string driverName { get; set; }
        public string busNo { get; set; }
        public string type { get; set; }
    }
    public class AddStudentInBus
    {
        public string type { get; set; }
        public int userId { get; set; }
        public int id { get; set; }
        public int classId { get; set; }
        public string className { get; set; }
        public int sectionId { get; set; }
        public string sectionName { get; set; }
        public int studentId { get; set; }
        public string studentName { get; set; }
        public int busId { get; set; }
        public string bussNo { get; set; }
        public int pickUpPointId { get; set; }
        public string pickupPoint { get; set; }
        public long fee { get; set; }
    }
    public class Security
    {
        public int Id { get; set; }
        public int userId { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public long MobileNo { get; set; }
        public DateTime DOB { get; set; }
        public string State { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public long AdharNo { get; set; }
        public string Gender { get; set; }
        public string Category { get; set; }
        public int BlockId { get; set; }
        public string BlockName { get; set; }
        public string GateNo { get; set; }
        public string Address { get; set; }
        public HttpPostedFileBase Image { get; set; }
        public string ImageName { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }

    public class RoomInfo
    {
        public int RoomId { get; set; }
        public int RoomNumber { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int TotalFees { get; set; }
        public string RoomFacilitate { get; set; }
    }
    public class TimeTableModel
    {
        public int userId { get; set; }
        public int id { get; set; }
        public int classId { get; set; }
        public string className { get; set; }
        public string sectionName { get; set; }
        public int sectionId { get; set; }
        public string day { get; set; }
        public string[] dayList { get; set; }
        public List<SubjectTimeModel> sbm { get; set; }
        public string createdAt { get; set; }
    }
    public class SubjectTimeModel
    {
        public int upid { get; set; }
        public string attachment { get; set; }
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
    public class upLoadTimetableModel
    {
        public int upid { get; set; }
        public string attachment { get; set; }
        public List<timetableshowModel> timetable { get; set; }
    }
    public class timetableshowModel
    {
        public string attachment { get; set; }
        public int tid { get; set; }
        public int userId { get; set; }
        public int id { get; set; }
        public int classId { get; set; }
        public int sectionId { get; set; }
        public string day { get; set; }
        public List<SubjectTimeModel> ttdata { get; set; }
    }
    public class BookModel
    {
        public int userId { get; set; }
        public int id { get; set; }
        public string addedBy { get; set; }
        public string title { get; set; }

        public string subTitle { get; set; }

        public string author { get; set; }

        public string volume { get; set; }

        public string edition { get; set; }

        public string isbn { get; set; }

        public string publication { get; set; }

        public string issnPrint { get; set; }

        public string placeOfPublication { get; set; }

        public string deweyDecimalClass { get; set; }

        public int yearOfPublication { get; set; }

        public string printingDate { get; set; }

        public int numberOfCopies { get; set; }

        public bool isIssuable { get; set; }

        public int numberOfPages { get; set; }

        public string purchasingDate { get; set; }

        public string source { get; set; }

        public string bookRemarks { get; set; }

        public decimal price { get; set; }

        public string supplier { get; set; }

        public string bookContent { get; set; }

        public string accessionNumber { get; set; }

        public string bookLocation { get; set; }

        public string categoryName { get; set; }
        public int categoryId { get; set; }
        public string subject { get; set; }
        public bool IsISBN { get; set; }
        public bool IsISSN { get; set; }
        public bool IsAccession { get; set; }
        public string bookLanguages { get; set; }
        public int bookCount { get; set; }
    }
    public class BookCategoryModel
    {
        public int userId { get; set; }
        public string categoryName { get; set; }
        public int id { get; set; }
    }
    public class UserOrderModel
    {
        public int hostelId { get; set; }
        public string address { get; set; }
        public int userId { get; set; }
        public string userName { get; set; }
        public string className { get; set; }
        public string sectionName { get; set; }
        public string emailId { get; set; }
        public long mobileNo { get; set; }
        public int roomNo { get; set; }
    }
    public class AddOrderModal
    {
        public string orderId { get; set; }
        public DateTime returnDate { get; set; }
        public int bookId { get; set; }
        public int userId { get; set; }
        public int id { get; set; }
        public int buyerId { get; set; }
        public string userType { get; set; }
        public DateTime orderDate { get; set; }
        public decimal lateFine { get; set; }
        public decimal damageFine { get; set; }
        public decimal lostFine { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
    }
    public class OrderHistoryModel
    {
        public string shortorderid { get; set; }
        public int receiveQuantity { get; set; }
        public int damageQuantity { get; set; }
        public decimal totalLateFine { get; set; }
        public int totalDelayDaysCount { get; set; }
        public string addedBy { get; set; }
        public bool recieveStatus { get; set; }
        public string bookName { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public long mobile { get; set; }
        public string orderId { get; set; }
        public string returnDate { get; set; }
        public int bookId { get; set; }
        public int userId { get; set; }
        public int id { get; set; }
        public int buyerId { get; set; }
        public string userType { get; set; }
        public DateTime orderDate { get; set; }
        public string orderDateString { get; set; }
        public decimal lateFine { get; set; }
        public decimal damageFine { get; set; }
        public decimal lostFine { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public decimal pricePerBook { get; set; }
    }
    public class ReturnOrderModel
    {
        public int id { get; set; }
        public int buyerId { get; set; }
        public int bookId { get; set; }
        public int userId { get; set; }
        public int quantity { get; set; }
        public int damageQuantity { get; set; }
        public int lateDays { get; set; }
        public decimal lateFine { get; set; }
        public decimal lostFine { get; set; }
        public decimal damageFine { get; set; }
        public decimal extraCharges { get; set; }
    }
    public class AddTaskModel
    {
        public string teacherName { get; set; }
        public int userId { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public int teacherId { get; set; }
        public HttpPostedFileBase attachment { get; set; }
        public string attachmentName { get; set; }
        [AllowHtml]
        public string description { get; set; }
        public DateTime completionDate { get; set; }
        public string completionDateString { get; set; }
        public int taskStatus { get; set; }
        public string taskStatusString { get; set; }
    }
    public class LibrarianModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DOB { get; set; }
        public long MobileNo { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string Gender { get; set; }
        public long AdharNo { get; set; }
        public HttpPostedFileBase Document { get; set; }
        public string DocumentName { get; set; }
        public string Address { get; set; }
        public string UserAction { get; set; }
        public HttpPostedFileBase Profile { get; set; }
        public string ProfileName { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
    public class AdminDashboardCount
    {
        public int userId { get; set; }
        public int admissionEnquiry { get; set; }
        public int admissionInterview { get; set; }
        public int admissionExam { get; set; }
        public int admissionShortList { get; set; }
        public int admissionAdmitted { get; set; }
        public int admissionFormIssued { get; set; }
    }
    public class PromoteStudentModel
    {
        public int studentId { get; set; }
        public int classId { get; set; }
        public int sectionId { get; set; }
    }
    #region url management
    public class UrlManagement
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }
    #endregion

    public class AddSyllabusMoedel
    {
        public int id { get; set; }
        public int userId { get; set; }
        public int classId { get; set; }
        public int classStreamId { get; set; }
        public string className { get; set; }
        public string classstream { get; set; }
        public string institutionType { get; set; }
        public string educationLevel { get; set; }
        public int subjectId { get; set; }
        public string subjectName { get; set; }
        public string academicYear { get; set; }
        public HttpPostedFileBase attachment { get; set; }
        public string attachmentName { get; set; }
    }
    public class AddExamTimeTableModel
    {
        public string subjectName { get; set; }
        public string sectionName { get; set; }
        public string examName { get; set; }
        public int id { get; set; }
        public int userId { get; set; }
        public int classId { get; set; }
        public int sectionId { get; set; }
        public string className { get; set; }
        public string academicYear { get; set; }
        public HttpPostedFileBase attachment { get; set; }
        public string attachmentName { get; set; }
        public string description { get; set; }
    }
    #region StaffModels
    public class StaffModels
    {
        public int StaffId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string Designation { get; set; }
    }
    #endregion

    #region noticemodel
    public class NoticeModel
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

    #endregion

    public class LeaveRequestsModel
    {
        public string teacherName { get; set; }
        public int userId { get; set; }
        public int id { get; set; }
        public int teacherId { get; set; }
        public string leaveType { get; set; }
        public int leaveCount { get; set; }
        public DateTime fromDate { get; set; }
        public string fromDateString { get; set; }
        public DateTime toDate { get; set; }
        public string toDateString { get; set; }
        public string reason { get; set; }
        public HttpPostedFileBase attachment { get; set; }
        public string attachmentName { get; set; }
        public int approvalStatus { get; set; }
    }
    #region AllFeeRecordModel
    public class AllFeeRecordModel
    {
        public string transactionid { get; set; }
        public int FeeId { get; set; }
        public int userid { get; set; }

        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int SectionId { get; set; }
        public decimal? FeesPaid { get; set; }
        public decimal? RemainingFees { get; set; }
        public string BillingMonth { get; set; }
        public string SectionName { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string AcademicYear { get; set; }
        public string BillingPeriod { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; }
        public HttpPostedFileBase FeeSlip { get; set; }
        public string FeeSlips { get; set; }
        public DateTime PaymentDate { get; set; }
    }
    #endregion

    #region DashboardCount
    public class AdminDashboardCountResult
    {
        public int totalteacher { get; set; }
        public int totalbus { get; set; }
        public int totalhostelblocks { get; set; }
        public int totaladmittedstudent { get; set; }
        public int totalsecurity { get; set; }
        public int totalbooks { get; set; }
        public int totalleaverequest { get; set; }
        public int totalassignedtasks { get; set; }
    }

    #endregion


    public class StudentModel
    {
        public string Gender { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string MobileNo { get; set; }
    }

    public class FeeInfo
    {
        public string Amount { get; set; }
        public string Fee { get; set; }
        public string BillingPeriod { get; set; }
    }
    public class AppDetailModel
    {
        public int userid { get; set; }
        public string type { get; set; }
        [AllowHtml]
        public string content { get; set; }
        public string about { get; set; }
        public string contact { get; set; }
        public string role { get; set; }
        public PrivacyPolicyModel ppm { get; set; }
    }
    public class PrivacyPolicyModel
    {
        [AllowHtml]
        public string privacyPolicy { get; set; }
        public string role { get; set; }
    }
    public class TermsAndConditions
    {
        [AllowHtml]
        public string termsAndConditions { get; set; }
        public string role { get; set; }
    }
    public class AppVersionModel
    {
        public string Type { get; set; }
        public string CurrentVersion { get; set; }
        public string OldVersion { get; set; }
        public string UpdateUrl { get; set; }
        public int userid { get; set; }
    }
    public class StudentFeeStatusModel
    {
        public decimal Amount { get; set; }
        public decimal FeesPaid { get; set; }
        public decimal RemainingFees { get; set; }
        public string BillingPeriod { get; set; }
    }
    public class ExamModel
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public int userId { get; set; }


    }
    public class ScheduleExamModel
    {
        public int userId { get; set; }
        public int scheduleId { get; set; }
        public string examName { get; set; }
        public string academicYear { get; set; }
        public DateTime startExamDate { get; set; }
        public DateTime endExamDate { get; set; }
        public string description { get; set; }
        public DateTime createdAt { get; set; }
        public int examId { get; set; }
    }

    public class ExamMarksheetViewModel
    {
        public int ClassId { get; set; }
        public int ExamId { get; set; }
        public List<MarkEntry> Marks { get; set; }
    }

    public class MarkEntry
    {
        public int SubjectId { get; set; }
        public int? TheoryMarks { get; set; }
        public int? PracticalMarks { get; set; }
    }

    public class StudentMarksheetViewModel
    {
        public int classId { get; set; }
        public int sectionId { get; set; }
        public int subjectId { get; set; }
        public int examId { get; set; }
        public int userId { get; set; }

        public List<StudentMarkEntry> Marks { get; set; }
    }

    public class StudentMarkEntry
    {
        public int studentId { get; set; }
        public int? theoryMarks { get; set; }
        public int? practicalMarks { get; set; }
        public string studentName { get; set; }
    }

    public class HostelBlockOverviewModel
    {
        public int? totalFloors { get; set; }
        public int? totalRooms { get; set; }
        public int? totalBeds { get; set; }
        public int? occupiedBeds { get; set; }
        public int? remainingBeds { get; set; }
        public int? nonACRoomNonAttachedBathroom { get; set; }
        public int? ACRoomNonAttachedBathroom { get; set; }
        public int? nonACRoomAttachedBathroom { get; set; }
        public int? ACRoomAttachedBathroom { get; set; }
    }

    public class CategoryModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int BookCount { get; set; }
    }

    public class LibraryOverviewModel
    {
        public int? totalbooks { get; set; }
        public int? issuedbooks { get; set; }
    }

    public class ReportCardModel
    {
        public string Subject { get; set; }
        public string Grade { get; set; }
        public int? TheoryMarks { get; set; }
        public int? TotalTheoryMarks { get; set; }
        public int? PracticalMarks { get; set; }
        public int? TotalPracticalMarks { get; set; }
        public int TotalMarks { get; set; }
        public int TotalObtainedMarks { get; set; }
        public double Percentage { get; set; }
        public string ExamName { get; set; }
    }
    public class LastHostelFeeRecord
    {
        public decimal TotalFeeSubmitted { get; set; }
        public int studentHostelid { get; set; }
        public string DueDateString { get; set; }
        public string transactionid { get; set; }
        public DateTime DueDate { get; set; }
        public decimal RemainingFee { get; set; }
        public decimal TotalFee { get; set; }
        public int? id { get; set; }
        public int hostelId { get; set; }
        public int? FeesSubmitted { get; set; }
        public string FeeType { get; set; }
        public string FeeSlipPath { get; set; }
        public HttpPostedFileBase FeeSlip { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int? StudentId { get; set; }
        public string StudentName { get; set; }
        public string AdmissionNo { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int RoomNumber { get; set; }
    }
    public class DriverProblemModel
    {
        public int id { get; set; }
        public int busno { get; set; }
        public int driverid { get; set; }
        public string driverName { get; set; }
        public string problem { get; set; }
        public string createdAt { get; set; }
        public int problemStatus { get; set; }
        public string reason { get; set; }
    }

    public class BusFeeModel
    {
        public int id { get; set; }
        public int userId { get; set; }
        public int studentId { get; set; }
        public string studentName { get; set; }
        public string fatherName { get; set; }
        public string className { get; set; }
        public string sectionName { get; set; }
        public string billingMonth { get; set; }
        public string[] billingMonths { get; set; }
        public string academicYear { get; set; }
        public DateTime createdDate { get; set; }
        public string paymentDated { get; set; }
        public int busId { get; set; }
        public int classId { get; set; }
        public int classStreamId { get; set; }
        public int sectionId { get; set; }
        public decimal feeAmount { get; set; }
        public string feeSlip { get; set; }
        public HttpPostedFileBase feeSlips { get; set; }

    }
    public class FeeStatementViewModel
    {
        public List<AllFeeRecordModel> AcademicFeeRecords { get; set; }
        public List<BusFeeModel> BusFeeRecords { get; set; }
    }
    public class StudentFeeSummaryModel
    {
        public string billingMonth { get; set; }
        public decimal totalPaid { get; set; }
        public string feeStatus { get; set; }
        public DateTime paymentDate { get; set; }
    }
    public class ClassStream
    {
        public int Id { get; set; }
        public string stream { get; set; }
    }


    public class AdminCommunicationList
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Attachment { get; set; }
        public string Description { get; set; }
        public string WardenName { get; set; }
        public string EmailId { get; set; }
        public string Mobile { get; set; }
        public string BlockName { get; set; }
        public string BlockType { get; set; }
        public int TotalFloorInBlock { get; set; }
    }
}