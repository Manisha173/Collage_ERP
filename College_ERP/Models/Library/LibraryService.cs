using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using static College_ERP.Models.Library.main;
using System.Configuration;
using System.Drawing;
using College_ERP.Models.Admin;
using System.Web.Mvc;
using College_ERP.Models.Teacher;

namespace College_ERP.Models.Library
{
    public class LibraryService
    {
        private readonly SqlConnection conn;
        private SqlCommand cmd;
        public LibraryService()
        {
            conn = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
        }
        public List<LibrarianDetailsModel> GetLibrarianDetails(int id,int userId)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageLibrarianRegistration", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectLibrarianById");
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@userId", userId);
                conn.Open();
                List<LibrarianDetailsModel> list = new List<LibrarianDetailsModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new LibrarianDetailsModel
                        {
                            Id = Convert.ToInt32(res["id"]),
                            Name = res["name"].ToString(),
                            Email = res["email"].ToString(),
                            MobileNo = Convert.ToInt64(res["mobileNo"]),
                            DOB = Convert.ToDateTime(res["dob"]),
                            StateName = res["stateName"].ToString(),
                            StateId = Convert.ToInt32(res["stateId"]),
                            CityName = res["City_Name"].ToString(),
                            CityId = Convert.ToInt32(res["cityId"]),
                            AdharNo = Convert.ToInt64(res["adharNo"]),
                            Gender = res["gender"].ToString(),
                            Address = res["address"].ToString(),
                            DocumentName = res["document"].ToString(),
                            ProfileName = res["profile"].ToString()
                        });
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

            }
        }
        public int GetAdminId(int userId)
        {
            try
            {
                int adminId = 0;
                cmd = new SqlCommand("sp_ManageLibrarianRegistration", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAdminId");
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        adminId = Convert.ToInt32(res["userId"]);
                    }
                }
                return adminId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                cmd.Dispose();

            }
        }
        #region Library Management
        public bool InsertBookCategory(AddBookCategoryModel cat)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "insertcategory");
                cmd.Parameters.AddWithValue("@userId", cat.adminId);
                cmd.Parameters.AddWithValue("@addedBy", "librarian");
                cmd.Parameters.AddWithValue("@categoryName", cat.categoryName);
                conn.Open();
                int res = cmd.ExecuteNonQuery();
                return res > 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
        public List<AddBookCategoryModel> GetAllBookCategory(int userId)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectCategory");
                cmd.Parameters.AddWithValue("@userId", userId);
                conn.Open();
                List<AddBookCategoryModel> list = new List<AddBookCategoryModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new AddBookCategoryModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            categoryName = res["categoryName"].ToString()
                        });
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

            }
        }
        public bool checkISBN(string isbn, string actionType,int userid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "checkIsbn");
                cmd.Parameters.AddWithValue("@isbn", isbn);
                cmd.Parameters.AddWithValue("@userId", userid);
                cmd.Parameters.AddWithValue("@actionType", actionType);
                conn.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

            }
        }
        public bool checkISSN(string issn, string actionType,int userid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "checkIssn");
                cmd.Parameters.AddWithValue("@issnprint", issn);
                cmd.Parameters.AddWithValue("@userId", userid);
                cmd.Parameters.AddWithValue("@actionType", actionType);
                conn.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

            }
        }
        public bool checkAccession(string accession, string actionType)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "checkAccession");
                cmd.Parameters.AddWithValue("@accessionnumber", accession);
                cmd.Parameters.AddWithValue("@actionType", actionType);
                conn.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

            }
        }
        public bool InsertBook(AddBookModel book)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userId", book.adminId);
                cmd.Parameters.AddWithValue("@addedBy", "librarian");
                cmd.Parameters.AddWithValue("@title", book.title);
                cmd.Parameters.AddWithValue("@subTitle", book.subTitle);
                cmd.Parameters.AddWithValue("@author", book.author);
                cmd.Parameters.AddWithValue("@volume", book.volume);
                cmd.Parameters.AddWithValue("@edition", book.edition);
                cmd.Parameters.AddWithValue("@isbn", book.isbn);
                cmd.Parameters.AddWithValue("@publication", book.publication);
                cmd.Parameters.AddWithValue("@issnPrint", book.issnPrint);
                cmd.Parameters.AddWithValue("@placeOfPublication", book.placeOfPublication);
                cmd.Parameters.AddWithValue("@deweyDecimalClass", book.deweyDecimalClass);
                cmd.Parameters.AddWithValue("@yearOfPublication", book.yearOfPublication);
                cmd.Parameters.AddWithValue("@printingDate", book.printingDate);
                cmd.Parameters.AddWithValue("@numberOfCopies", book.numberOfCopies);
                cmd.Parameters.AddWithValue("@isIssuable", book.isIssuable);
                cmd.Parameters.AddWithValue("@numberOfPages", book.numberOfPages);
                cmd.Parameters.AddWithValue("@purchasingDate", book.purchasingDate);
                cmd.Parameters.AddWithValue("@source", book.source);
                cmd.Parameters.AddWithValue("@bookRemarks", book.bookRemarks);
                cmd.Parameters.AddWithValue("@price", book.price);
                cmd.Parameters.AddWithValue("@supplier", book.supplier);
                cmd.Parameters.AddWithValue("@bookContent", book.bookContent);
                cmd.Parameters.AddWithValue("@accessionNumber", book.accessionNumber);
                cmd.Parameters.AddWithValue("@bookLocation", book.bookLocation);
                cmd.Parameters.AddWithValue("@categoryId", book.categoryId);
                cmd.Parameters.AddWithValue("@subject", book.subject);
                cmd.Parameters.AddWithValue("@bookLanguages", book.bookLanguages);
                cmd.Parameters.AddWithValue("@id", book.id);
                cmd.Parameters.AddWithValue("@action", book.id > 0 ? "updateBook" : "insertBook");

                conn.Open();
                int res = cmd.ExecuteNonQuery();

                return res > 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
        public List<AddBookModel> GetAllBooks(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAllBooks");
                cmd.Parameters.AddWithValue("@userId", userId);

                conn.Open();
                List<AddBookModel> list = new List<AddBookModel>();
                var res = cmd.ExecuteReader();

                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new AddBookModel
                        {
                            addedBy = res["addedBy"].ToString(),
                            id = Convert.ToInt32(res["id"]),
                            userId = Convert.ToInt32(res["userId"]),
                            title = res["title"].ToString(),
                            subTitle = res["subTitle"].ToString(),
                            author = res["author"].ToString(),
                            volume = res["volume"].ToString(),
                            edition = res["edition"].ToString(),
                            isbn = res["isbn"].ToString(),
                            publication = res["publication"].ToString(),
                            issnPrint = res["issnPrint"].ToString(),
                            placeOfPublication = res["placeOfPublication"].ToString(),
                            deweyDecimalClass = res["deweyDecimalClass"].ToString(),
                            yearOfPublication = Convert.ToInt32(res["yearOfPublication"]),
                            printingDate = Convert.ToDateTime(res["printingDate"]).ToString("dd-MMM-yyyy"),
                            numberOfCopies = Convert.ToInt32(res["numberOfCopies"]),
                            isIssuable = Convert.ToBoolean(res["isIssuable"]),
                            numberOfPages = Convert.ToInt32(res["numberOfPages"]),
                            purchasingDate = Convert.ToDateTime(res["purchasingDate"]).ToString("dd-MMM-yyyy"),
                            source = res["source"].ToString(),
                            bookRemarks = res["bookRemarks"].ToString(),
                            price = Convert.ToDecimal(res["price"]),
                            supplier = res["supplier"].ToString(),
                            bookContent = res["bookContent"].ToString(),
                            accessionNumber = res["accessionNumber"].ToString(),
                            bookLocation = res["bookLocation"].ToString(),
                            categoryId = Convert.ToInt32(res["categoryId"]),
                            categoryName = res["categoryName"].ToString(),
                            subject = res["subject"].ToString(),
                            bookLanguages = res["bookLanguages"].ToString(),
                            bookCount = Convert.ToInt32(res["bookCount"])
                        });
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
        public List<AddBookModel> GetBookById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBookById");
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                List<AddBookModel> list = new List<AddBookModel>();
                var res = cmd.ExecuteReader();

                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new AddBookModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            userId = Convert.ToInt32(res["userId"]),
                            title = res["title"].ToString(),
                            subTitle = res["subTitle"].ToString(),
                            author = res["author"].ToString(),
                            volume = res["volume"].ToString(),
                            edition = res["edition"].ToString(),
                            isbn = res["isbn"].ToString(),
                            publication = res["publication"].ToString(),
                            issnPrint = res["issnPrint"].ToString(),
                            placeOfPublication = res["placeOfPublication"].ToString(),
                            deweyDecimalClass = res["deweyDecimalClass"].ToString(),
                            yearOfPublication = Convert.ToInt32(res["yearOfPublication"]),
                            printingDate = Convert.ToDateTime(res["printingDate"]).ToString("yyyy-MM-dd"),
                            numberOfCopies = Convert.ToInt32(res["numberOfCopies"]),
                            isIssuable = Convert.ToBoolean(res["isIssuable"]),
                            numberOfPages = Convert.ToInt32(res["numberOfPages"]),
                            purchasingDate = Convert.ToDateTime(res["purchasingDate"]).ToString("yyyy-MM-dd"),
                            source = res["source"].ToString(),
                            bookRemarks = res["bookRemarks"].ToString(),
                            price = Convert.ToDecimal(res["price"]),
                            supplier = res["supplier"].ToString(),
                            bookContent = res["bookContent"].ToString(),
                            accessionNumber = res["accessionNumber"].ToString(),
                            bookLocation = res["bookLocation"].ToString(),
                            categoryId = Convert.ToInt32(res["categoryId"]),
                            categoryName = res["categoryName"].ToString(),
                            subject = res["subject"].ToString(),
                            bookLanguages = res["bookLanguages"].ToString(),
                            bookCount = Convert.ToInt32(res["bookCount"])
                        });
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
        public List<GetUserOrderModel> SeletUserForLibrary(string userNo, string userType, int userId)
        {
            List<GetUserOrderModel> list = new List<GetUserOrderModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectuserByNo");
                cmd.Parameters.AddWithValue("@userNo", userNo);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@userType", userType);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {

                    while (rd.Read())
                    {
                        list.Add(new GetUserOrderModel
                        {
                            userName = rd["name"].ToString(),
                            className = userType == "student" ? rd["class"].ToString() : "",
                            sectionName = userType == "student" ? rd["section"].ToString() : "",
                            userId = rd["id"] == DBNull.Value ? 0 : Convert.ToInt32(rd["id"]),
                            emailId = userType == "teacher" ? rd["email"].ToString() : "",
                            mobileNo = userType == "teacher" ? rd["mobile"] == DBNull.Value ? 0 : Convert.ToInt64(rd["mobile"]) : 0,
                            roomNo = userType == "student" ? rd["roomNo"] == DBNull.Value ? 0 : Convert.ToInt32(rd["roomNo"]) : 0,
                            address = userType == "student" ? rd["address"].ToString() : "",
                            hostelId = userType == "student" ? rd["hostelId"] == DBNull.Value ? 0 : Convert.ToInt32(rd["hostelId"]) : 0,
                        });
                    }

                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }
        public bool InsertBookOrder(OrderModel model, out string errorMessage)
        {
            int result = 0;
            try
            {
                errorMessage = "";
                SqlCommand command = new SqlCommand("sp_ManageLibrary", conn);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@action", "insertBookOrder");
                command.Parameters.AddWithValue("@userId", model.adminId);
                command.Parameters.AddWithValue("@bookId", model.bookId);
                command.Parameters.AddWithValue("@addedBy", "librarian");
                command.Parameters.AddWithValue("@buyerId", model.buyerId);
                command.Parameters.AddWithValue("@userType", model.userType);
                command.Parameters.AddWithValue("@orderDate", model.orderDate);
                command.Parameters.AddWithValue("@lateFine", model.lateFine);
                command.Parameters.AddWithValue("@damageFine", model.damageFine);
                command.Parameters.AddWithValue("@lostFine", model.lostFine);
                command.Parameters.AddWithValue("@quantity", model.quantity);
                command.Parameters.AddWithValue("@price", model.price);
                command.Parameters.AddWithValue("@returnDate", model.returnDate);
                conn.Open();
                result = command.ExecuteNonQuery();
                if (result <= 0) errorMessage = "Something went wrong";
                return result > 0;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
        public List<ShowOrderHistoryModel> GetBookOrders(int userId)
        {
            List<ShowOrderHistoryModel> orders = new List<ShowOrderHistoryModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBookOrder");
                cmd.Parameters.AddWithValue("@userId", userId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        // Safe userType extraction once
                        string userType = reader["userType"] == DBNull.Value
                            ? ""
                            : reader["userType"].ToString();

                        orders.Add(new ShowOrderHistoryModel
                        {
                            addedBy = reader["addedBy"] == DBNull.Value
                                ? ""
                                : reader["addedBy"].ToString(),

                            id = reader["id"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["id"]),

                            orderId = reader["shortOrderId"] == DBNull.Value
                                ? ""
                                : reader["shortOrderId"].ToString(),

                            userId = reader["userId"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["userId"]),

                            bookId = reader["bookId"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["bookId"]),

                            buyerId = reader["buyerId"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["buyerId"]),

                            userType = userType,

                            orderDate = reader["orderDate"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(reader["orderDate"]),

                            lateFine = reader["lateFine"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["lateFine"]),

                            damageFine = reader["damageFine"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["damageFine"]),

                            lostFine = reader["lostFine"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["lostFine"]),

                            quantity = reader["quantity"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["quantity"]),

                            price = reader["price"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["price"]),

                            returnDate = reader["returnDate"] == DBNull.Value
                                ? ""
                                : Convert.ToDateTime(reader["returnDate"]).ToString("dd-MM-yyyy"),

                            name = userType == "student"
                                ? (reader["StudentName"] == DBNull.Value ? "" : reader["StudentName"].ToString())
                                : userType == "teacher"
                                    ? (reader["TeacherName"] == DBNull.Value ? "" : reader["TeacherName"].ToString())
                                    : "",

                            email = userType == "student"
                                ? (reader["StudentEmail"] == DBNull.Value ? "" : reader["StudentEmail"].ToString())
                                : userType == "teacher"
                                    ? (reader["TeacherEmail"] == DBNull.Value ? "" : reader["TeacherEmail"].ToString())
                                    : "",

                            mobile = userType == "student"
                                ? (reader["MobileNo"] == DBNull.Value ? 0 : Convert.ToInt64(reader["MobileNo"]))
                                : userType == "teacher"
                                    ? (reader["TeacherMobile"] == DBNull.Value ? 0 : Convert.ToInt64(reader["TeacherMobile"]))
                                    : 0,

                            recieveStatus = reader["recieveStatus"] == DBNull.Value
                                ? false
                                : Convert.ToBoolean(reader["recieveStatus"]),

                            bookName = reader["title"] == DBNull.Value
                                ? ""
                                : reader["title"].ToString()
                        });
                    }
                }

                return orders;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }
        public List<ShowOrderHistoryModel> GetBookOrderById(int id, int userId)
        {
            List<ShowOrderHistoryModel> orders = new List<ShowOrderHistoryModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBookOrderById");
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string userType = reader["userType"] == DBNull.Value
    ? ""
    : reader["userType"].ToString();

                        orders.Add(new ShowOrderHistoryModel
                        {
                            addedBy = reader["addedBy"] == DBNull.Value
                                ? ""
                                : reader["addedBy"].ToString(),

                            id = reader["id"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["id"]),

                            orderId = reader["shortOrderId"] == DBNull.Value
                                ? ""
                                : reader["shortOrderId"].ToString(),

                            userId = reader["userId"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["userId"]),

                            bookId = reader["bookId"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["bookId"]),

                            buyerId = reader["buyerId"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["buyerId"]),

                            userType = userType,

                            orderDate = reader["orderDate"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(reader["orderDate"]),

                            lateFine = reader["lateFine"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["lateFine"]),

                            damageFine = reader["damageFine"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["damageFine"]),

                            lostFine = reader["lostFine"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["lostFine"]),

                            quantity = reader["quantity"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["quantity"]),

                            price = reader["price"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["price"]),

                            returnDate = reader["returnDate"] == DBNull.Value
                                ? ""
                                : Convert.ToDateTime(reader["returnDate"]).ToString("dd-MM-yyyy"),

                            name = userType == "student"
                                ? (reader["StudentName"] == DBNull.Value ? "" : reader["StudentName"].ToString())
                                : userType == "teacher"
                                    ? (reader["TeacherName"] == DBNull.Value ? "" : reader["TeacherName"].ToString())
                                    : "",

                            email = userType == "student"
                                ? (reader["StudentEmail"] == DBNull.Value ? "" : reader["StudentEmail"].ToString())
                                : userType == "teacher"
                                    ? (reader["TeacherEmail"] == DBNull.Value ? "" : reader["TeacherEmail"].ToString())
                                    : "",

                            mobile = userType == "student"
                                ? (reader["MobileNo"] == DBNull.Value ? 0 : Convert.ToInt64(reader["MobileNo"]))
                                : userType == "teacher"
                                    ? (reader["TeacherMobile"] == DBNull.Value ? 0 : Convert.ToInt64(reader["TeacherMobile"]))
                                    : 0,

                            recieveStatus = reader["recieveStatus"] == DBNull.Value
                                ? false
                                : Convert.ToBoolean(reader["recieveStatus"]),

                            bookName = reader["title"] == DBNull.Value
                                ? ""
                                : reader["title"].ToString()
                        });
                    }
                }

                return orders;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }
        public bool ReturnBookOrder(ReturnBookOrderModel model, out string errorMessage)
        {
            int result = 0;
            try
            {
                errorMessage = "";
                SqlCommand command = new SqlCommand("sp_ManageLibrary", conn);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@action", "returnBookOrder");
                command.Parameters.AddWithValue("@userId", model.adminId);
                command.Parameters.AddWithValue("@recievedBy", "librarian");
                command.Parameters.AddWithValue("@bookId", model.bookId);
                command.Parameters.AddWithValue("@buyerId", model.buyerId);
                command.Parameters.AddWithValue("@obId", model.id);
                command.Parameters.AddWithValue("@lateDays", model.lateDays);
                command.Parameters.AddWithValue("@lateFine", model.lateFine);
                command.Parameters.AddWithValue("@damageFine", model.damageFine);
                command.Parameters.AddWithValue("@lostFine", model.lostFine);
                command.Parameters.AddWithValue("@recieveQuantity", model.quantity);
                command.Parameters.AddWithValue("@damageQuantity", model.damageQuantity);
                command.Parameters.AddWithValue("@extraCharges", model.extraCharges);
                conn.Open();
                result = command.ExecuteNonQuery();
                if (result <= 0) errorMessage = "Something went wrong";
                return result > 0;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        public List<LibrarianDetailsModel> GetLibrarian(int adminid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrarianRegistration", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectLibrarian");
                cmd.Parameters.AddWithValue("@userid", adminid);
                conn.Open();
                List<LibrarianDetailsModel> list = new List<LibrarianDetailsModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new LibrarianDetailsModel
                        {
                            Id = Convert.ToInt32(res["id"]),
                            EmployeeId = Convert.ToInt32(res["EmployeeId"]),
                            Name = res["name"].ToString(),
                            Email = res["email"].ToString(),
                            MobileNo = Convert.ToInt64(res["mobileNo"]),
                            DOB = Convert.ToDateTime(res["dob"]),
                            StateName = res["stateName"].ToString(),
                            StateId = Convert.ToInt32(res["stateId"]),
                            CityName = res["City_Name"].ToString(),
                            CityId = Convert.ToInt32(res["cityId"]),
                            AdharNo = Convert.ToInt64(res["adharNo"]),
                            Gender = res["gender"].ToString(),
                            Address = res["address"].ToString(),
                            DocumentName = res["document"].ToString(),
                            ProfileName = res["profile"].ToString()
                        });
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

            }
        }
        public List<NoticeModel> GetLibraryNotices(string userType, int userId,int adminid)
        {
            try
            {
                var notices = new List<NoticeModel>();

                SqlCommand cmd = new SqlCommand("sp_NoticeManagement", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GetNoticeByUsers");
                cmd.Parameters.AddWithValue("@UserType", userType);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@adminid", adminid);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    notices.Add(new NoticeModel
                    {
                        NoticeId = Convert.ToInt32(dr["noticeId"]),
                        Title = dr["title"].ToString(),
                        Description = dr["description"].ToString(),
                        Attachments = dr["attachment"]?.ToString(),
                        UserType = dr["usertype"].ToString(),
                        ReceiverId = dr["ReceiverId"] != DBNull.Value ? Convert.ToInt32(dr["ReceiverId"]) : (int?)null,
                        CreatedOn = Convert.ToDateTime(dr["CreatedOn"])

                    });
                }
                return notices;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
        public string GetNoticeDscById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_NoticeManagement", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GetNoticeDescById");
                cmd.Parameters.AddWithValue("@NoticeId", id);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    return dr["description"].ToString();
                }
                return null;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public LibrarianDashboardCount GetDashboardCount(int userId, int adminId)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageLibrarianDashboard", conn);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@userid", userId);     
                command.Parameters.AddWithValue("@AdminId", adminId);   
                command.Parameters.AddWithValue("@action", "dashboardCount");
                conn.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new LibrarianDashboardCount
                    {
                        totalbooks = reader["totalbooks"] != DBNull.Value ? Convert.ToInt32(reader["totalbooks"]) : 0,
                        orderedbooks = reader["orderedbooks"] != DBNull.Value ? Convert.ToInt32(reader["orderedbooks"]) : 0,
                        receivedbooks = reader["receivedbooks"] != DBNull.Value ? Convert.ToInt32(reader["receivedbooks"]) : 0,
                        notreceivedbooks = reader["notreceivedbooks"] != DBNull.Value ? Convert.ToInt32(reader["notreceivedbooks"]) : 0,
                        totalrevenue = reader["totalrevenue"] != DBNull.Value ? Convert.ToInt32(reader["totalrevenue"]) : 0,
                        availablestock = reader["availablestock"] != DBNull.Value ? Convert.ToInt32(reader["availablestock"]) : 0,
                        notice = reader["notice"] != DBNull.Value ? Convert.ToInt32(reader["notice"]) : 0,
                    };
                }
                return new LibrarianDashboardCount();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }
        public List<HolidayModel> GetHolidaysForAll(int userId)
        {
            List<HolidayModel> holidays = new List<HolidayModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_Holiday", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "HolidaysForAll");
                cmd.Parameters.AddWithValue("@userid", userId);

                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    holidays.Add(new HolidayModel
                    {
                        Title = rdr["Title"].ToString(),
                        HolidayType = rdr["HolidayType"].ToString(),
                        HolidayDateFrom = Convert.ToDateTime(rdr["HolidayDate"]),
                        HolidayDateTo = Convert.ToDateTime(rdr["HolidayDateTo"]),
                        Description = rdr["Description"].ToString(),
                    });
                }
                return holidays;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

        }

        public List<HolidayModel> GetHolidaysTodayAndTomorrow(int userId)
        {
            List<HolidayModel> holidays = new List<HolidayModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_Holiday", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "HolidaysTodayAndTomorrow");
                cmd.Parameters.AddWithValue("@userid", userId);

                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    holidays.Add(new HolidayModel
                    {
                        Title = rdr["Title"].ToString(),
                        HolidayType = rdr["HolidayType"].ToString(),
                        HolidayDateFrom = Convert.ToDateTime(rdr["HolidayDate"]),
                        HolidayDateTo = Convert.ToDateTime(rdr["HolidayDateTo"]),
                        Description = rdr["Description"].ToString(),
                    });
                }
                return holidays;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }


    }
}