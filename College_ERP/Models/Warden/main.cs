using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace College_ERP.Models.Warden
{
    public class main
    {

    }

    public class UserData
    {
        public int userId { get; set; }
        public int Id { get; set; }
    }
    public class CommonModel
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class warden

        {
            public int Id { get; set; }
            public string DOBstring { get; set; }
            public int userId { get; set; }
            public string Name { get; set; }
            public string EmailId { get; set; }
            public string MobileNo { get; set; }
            public DateTime? DOB { get; set; }
            public string Gender { get; set; }
            public string Document { get; set; }
            public string BlockName1 { get; set; }
            public string BlockName { get; set; }
            public int BlockId { get; set; }
        public string cityName { get; set; }
        public string ProfilePics { get; set; }
           public HttpPostedFileBase Documents { get; set; }
            public int st_Id { get; set; }
            public string stateName { get; set; }
            public int city_Id { get; set; }
            public string Address { get; set; }
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
        public List<CommonModel> students { get; set; }
    }
    public class WardenDashboardModel
    {
        public int? totalroom { get; set; }
        public int? totalbeds { get; set; }
        public int? occupiedbeds { get; set; }
        public int? remainingbeds { get; set; }
        public int? nonACRoomNonAttachedBathroom { get; set; }
        public int? ACRoomNonAttachedBathroom { get; set; }
        public int? nonACRoomAttachedBathroom { get; set; }
        public int? ACRoomAttachedBathroom { get; set; }
        public int? communication { get; set; }
        public int? notice { get; set; }
    }


    public class RoomFacilityCount
    {
      
        public string FacilityName { get; set; }

        public int RoomCount { get; set; }
    }

    public class StudentDetailModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string MobileNo { get; set; }
        public string Gender { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string AdmissionNo { get; set; }
    }
    public class MealViewModel
    {
        public int Id { get; set; }
        public int MealId { get; set; }
        public int UserId { get; set; }  
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<MenuViewModel> Menus { get; set; }
    }

    public class MenuViewModel
    {
        public int MenuId { get; set; }
        public int MealId { get; set; }
        public string Menu { get; set; }
    }

    public class MealUpdateRequest
    {
        public int userid { get; set; }
        public List<MealViewModel> Meals { get; set; }
    }
   
    public class CommunicationModel
    {
        public int CommunicationId { get; set; }
        public int userid { get; set; }
        public string Title { get; set; }
        public string Attachment { get; set; }
        public HttpPostedFileBase Attachments { get; set; }

        [AllowHtml]
        public string Description { get; set; }
    }
    public class UserOrdersModel
    {
        public int roomnumber { get; set; }
        public decimal totalFee { get; set; }
        public string transactionid { get; set; }
        public int BlockId { get; set; }
        public decimal feeSubmitted { get; set; }
        public int floor { get; set; }
        public string feeType { get; set; }
        public decimal remainingFee { get; set; }
        public string dueDateString { get; set; }
        public string feeSlip { get; set; }
        public string studentName { get; set; }
        public int userId { get; set; }
        public int studentid { get; set; }
        public string userName { get; set; }
        public string className { get; set; }
        public string sectionName { get; set; }
        public string emailId { get; set; }
        public long mobileNo { get; set; }
        public int roomNo { get; set; }
        public int hostelId { get; set; }
        public int blockId { get; set; }
        public int feesperperson { get; set; }
        public string address { get; set; }
    }

    public class RoomModel
    {
        public string RoomFacilitate { get; set; }
        public int NoOfBeds { get; set; }
        public int RoomId { get; set; }
        public int RoomNo { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int FeesPerPerson { get; set; }
    }

    public class ReallocateRoomModel
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
        public HttpPostedFileBase FeeSlip { get; set; }
        public string FeeSlipPath { get; set; }
        public string DueDate { get; set; }
    }

    public class FeeRecordsModel
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
    public class HostelProblemsModel
    {
        public int id { get; set; }
        public int studentid { get; set; }
        public string problem { get; set; }
        public string createdAt { get; set; }
        public int problemStatus { get; set; }
        public string blockName { get; set; }
        public string roomNo { get; set; }
        public string studentName { get; set; }
    }
    public class BlockDetailModel
    {
        public int ?floors { get; set; }
        public int? rooms { get; set; }
    }
    public class RoomListModel
    {
        public int totalfloors { get; set; }
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
        public int TotalFloors { get; set; }
        public int OccupiedBeds { get; set; }
        public int RemainingBeds { get; set; }
        public string StudentName { get; set; }

    }
}