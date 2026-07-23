using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Mvc;
using College_ERP.Models.Admin;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.Library;
using College_ERP.Models.Security;
using College_ERP.Models.Teacher;
using static College_ERP.Models.Library.main;

namespace College_ERP.Controllers
{
    [Authorize(Roles ="librarian")]
    public class LibraryController : Controller
    {
        private readonly LibraryService _library;
        private readonly HomeService _home;
        public LibraryController()
        {
            _library = new LibraryService();
            _home = new HomeService();
        }
        // GET: Library
        public ActionResult Dashboard()
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminId = _library.GetAdminId(userId);
            var count = _library.GetDashboardCount(userId,adminId);
            return View(count);
        }
        [HttpGet]
        public ActionResult GetUserDetails()
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminId = _library.GetAdminId(userId);
            var data = _library.GetLibrarianDetails(userId,adminId);
            return Json(data,JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddBookCategory(AddBookCategoryModel cat)
        {
            int userId = _home.GetUserId(User.Identity.Name);
            cat.adminId = _library.GetAdminId(userId);
            bool res = _library.InsertBookCategory(cat);
            return Json(new
            {
                status = res,
                message = res ? (cat.id > 0 ? "Category Updated Successfully" : "Category Added Successfully") : "Some error Occured"
            });
        }
        public ActionResult AddBook()
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminId = _library.GetAdminId(userId);
            ViewData["categories"] = _library.GetAllBookCategory(adminId);
            ViewData["BookList"] = _library.GetAllBooks(adminId);
            return View();
        }
        [HttpGet]
        public ActionResult CheckISBN(string isbn, string actiontype)
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminId = _library.GetAdminId(userId);
            bool IsISBN = _library.checkISBN(isbn, actiontype, adminId);
            return Json(IsISBN, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult CheckISSN(string issn, string actiontype)
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminId = _library.GetAdminId(userId);
            bool IsISSN = _library.checkISSN(issn, actiontype, adminId);
            return Json(IsISSN, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckAccession(string accession, string actiontype)
        {
            bool IsAccession = _library.checkAccession(accession, actiontype);
            return Json(IsAccession, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddBook(AddBookModel book)
        {
            int userId = _home.GetUserId(User.Identity.Name);
            book.adminId = _library.GetAdminId(userId);
            if (book.IsISBN || book.IsISSN || book.IsAccession)
            {
                return Json(new
                {
                    status = book.IsISBN ? book.IsISBN : book.IsISSN ? book.IsISSN : book.IsAccession ? book.IsAccession : false,
                    message = book.IsISBN ? "ISBN number already exist" : book.IsISSN ? "ISSN print already exist" : book.IsAccession ? "Accession number already exist" : ""
                });
            }
            bool res = _library.InsertBook(book);
            return Json(new
            {
                status = res,
                message = res ? (book.id > 0 ? "Book Details Updated Successfully" : "Book Added Successfully") : "Some error Occured"
            });
        }
        [HttpGet]
        public ActionResult GetBookById(int id)
        {
            var data = _library.GetBookById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult OrderManagement()
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminId = _library.GetAdminId(userId);
            ViewData["BookList"] = _library.GetAllBooks(adminId).Select(d => d).Take(10).ToList();
            ViewData["categories"] = _library.GetAllBookCategory(adminId);
            return View();
        }
        [HttpGet]
        public ActionResult FilterForBook(string filter, string filterType)
        {
            int user = _home.GetUserId(User.Identity.Name);
            int userId = _library.GetAdminId(user);
            var data = new List<College_ERP.Models.Library.main.AddBookModel>();
            if (!String.IsNullOrEmpty(filter))
            {
                data = filterType == "category" ? _library.GetAllBooks(userId).Where(d => d.categoryId == Convert.ToInt32(filter)).ToList() : filterType == "isbn" ? _library.GetAllBooks(userId).Where(d => d.isbn == filter).ToList() : filterType == "issn" ? _library.GetAllBooks(userId).Where(d => d.issnPrint == filter).ToList() : filterType == "accession" ? _library.GetAllBooks(userId).Where(d => d.accessionNumber == filter).ToList() : _library.GetAllBooks(userId).Select(d => d).Take(10).ToList();
            }
            else
            {
                data = _library.GetAllBooks(userId).Select(d => d).Take(10).ToList();
            }
                return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetUserForOrderBook(string userNo, string type)
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminId = _library.GetAdminId(userId);
            var data = _library.SeletUserForLibrary(userNo, type, adminId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult AddBookOrder(OrderModel model)
        {
           int userId = _home.GetUserId(User.Identity.Name);
            model.adminId = _library.GetAdminId(userId);
            bool res = _library.InsertBookOrder(model, out string error);
            return Json(new
            {
                status = res,
                message = res && model.id > 0 ? "Order Updated Successfully" : res && model.id == 0 ? "Order Added Successfully" : error
            });
        }
        public ActionResult OrderHistory()
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminId = _library.GetAdminId(userId);
            ViewData["orderList"] = _library.GetBookOrders(adminId);
            return View();
        }
        [HttpGet]
        public JsonResult GetBookOrderById(int id)
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminId = _library.GetAdminId(userId);
            var data = _library.GetBookOrderById(id, adminId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ReturnBookOrder(ReturnBookOrderModel model)
        {
            int userId = Convert.ToInt32(_home.GetUserId(User.Identity.Name));
            model.adminId = _library.GetAdminId(userId);
            var data = _library.GetBookOrderById(model.id, model.adminId);
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
            bool res = _library.ReturnBookOrder(model, out string error);
            return Json(new
            {
                status = res,
                message = res ? "Order Return Successfully" : error
            });
        }


        public ActionResult Notice()
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminId = _library.GetAdminId(userId);
            var notices = _library.GetLibraryNotices("Librarian", userId,adminId);
            return View(notices);
        }
        public ActionResult LibrarianProfile()
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminid = _library.GetAdminId(userId);
            var list = _library.GetLibrarian(adminid);
            var data = list.FirstOrDefault(); 
            return View(data); 
        }
        public ActionResult Holidays()
        {
            int userId = _home.GetUserId(User.Identity.Name);
            int adminid = _library.GetAdminId(userId);
            var holidays = _library.GetHolidaysForAll(adminid);
            return View(holidays);
        }
        public ActionResult NoticeDescById(int id)
        {
            var res = _library.GetNoticeDscById(id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowBookDetailsPage(int? id)
        {
            if (id.HasValue)
            {
                var data = _library.GetBookById(Convert.ToInt32(id));
                return View(data);
            }
            return RedirectToAction("AddBook");
        }
    }
}