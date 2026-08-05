using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace College_ERP.Models.Library
{
    public class main
    {
        public class LibrarianDetailsModel
        {
            public int Id { get; set; }
            public int EmployeeId { get; set; }
            public int UserId { get; set; }
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
        }
        public class AddBookCategoryModel
        {
            public int adminId { get; set; }
            public string categoryName { get; set; }
            public int id { get; set; }
        }
        public class AddBookModel
        {
            public string addedBy { get; set; }
            public int userId { get; set; }
            public int id { get; set; }
            public int adminId { get; set; }
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
        public class GetUserOrderModel
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
        public class OrderModel
        {
            public string addedBy { get; set; }
            public int adminId { get; set; }
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
        public class ShowOrderHistoryModel
        {
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
            public decimal lateFine { get; set; }
            public decimal damageFine { get; set; }
            public decimal lostFine { get; set; }
            public int quantity { get; set; }
            public decimal price { get; set; }
        }
        public class ReturnBookOrderModel
        {
            public int adminId { get; set; }
            public string recievedBy { get; set; }
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

        public class LibrarianDashboardCount
        {
            public int totalbooks { get; set; }
            public int orderedbooks { get; set; }
            public int receivedbooks { get; set; }
            public int notreceivedbooks { get; set; }
            public int totalrevenue { get; set; }
            public int availablestock { get; set; }
            public int notice { get; set; }
        }
    }

}