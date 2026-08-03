using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using College_ERP.Models.Warden;
using Newtonsoft.Json;
using static College_ERP.Models.StudentServices.main;

namespace College_ERP.Models.StudentServices
{
    public class StudentServices
    {
        private readonly SqlConnection connection;
        private SqlCommand cmd;
        public StudentServices()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
        }
        public int GetAdminId(int studentid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StudentId", studentid);
                cmd.Parameters.AddWithValue("@Action", "selectAdminId");
                connection.Open();
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
                return 0; // Return 0 if no admin ID is found
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        #region Get Warden
        public int GetWardenId(int studentid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StudentId", studentid);
                cmd.Parameters.AddWithValue("@Action", "selectWardenId");
                connection.Open();
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
                return 0; 
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion
        #region Student Profile
        public StudentProfileModel GetStudentById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StudentId", id);
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    return new StudentProfileModel
                    {
                        StudentId = rdr["StudentId"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        AdmissionNo = rdr["AdmissionNo"] != DBNull.Value ? rdr["AdmissionNo"].ToString() : "",
                        StudentName = rdr["StudentName"] != DBNull.Value ? rdr["StudentName"].ToString() : "",
                        MotherTongue = rdr["MotherTougue"] != DBNull.Value ? rdr["MotherTougue"].ToString() : "",
                        ClassName = rdr["ClassName"] != DBNull.Value ? rdr["ClassName"].ToString() : "",
                        ClassId = rdr["ClassId"] != DBNull.Value ? Convert.ToInt32(rdr["ClassId"]) : 0,
                        SectionName = rdr["SectionName"] != DBNull.Value ? rdr["SectionName"].ToString() : "",
                        SectionId = rdr["SectionId"] != DBNull.Value ? Convert.ToInt32(rdr["SectionId"]) : 0,
                        Gender = rdr["Gender"] != DBNull.Value ? rdr["Gender"].ToString() : "",
                        Religion = rdr["Religion"] != DBNull.Value ? rdr["Religion"].ToString() : "",
                        Caste = rdr["Caste"] != DBNull.Value ? rdr["Caste"].ToString() : "",
                        PlaceOfBirth = rdr["PlaceOfBirth"] != DBNull.Value ? rdr["PlaceOfBirth"].ToString() : "",
                        DOB = rdr["DOB"] != DBNull.Value ? Convert.ToDateTime(rdr["DOB"]) : DateTime.MinValue,
                        StateId = rdr["st_Id"] != DBNull.Value ? Convert.ToInt32(rdr["st_Id"]) : 0,
                        StateName = rdr["stateName"] != DBNull.Value ? rdr["stateName"].ToString() : "",
                        CityId = rdr["city_Id"] != DBNull.Value ? Convert.ToInt32(rdr["city_Id"]) : 0,
                        CityName = rdr["City_Name"] != DBNull.Value ? rdr["City_Name"].ToString() : "",
                        ObtainedMarks = rdr["ObtainedMarks"] != DBNull.Value ? Convert.ToInt32(rdr["ObtainedMarks"]) : 0,
                        TotalMarks = rdr["TotalMarks"] != DBNull.Value ? Convert.ToInt32(rdr["TotalMarks"]) : 0,
                        Address = rdr["Address"] != DBNull.Value ? rdr["Address"].ToString() : "",
                        AadharNo = rdr["AadharNumber"] != DBNull.Value ? rdr["AadharNumber"].ToString() : "",
                        YearOfPassing = rdr["YearOfPassing"] != DBNull.Value ? rdr["YearOfPassing"].ToString() : "",
                        CurrentAddress = rdr["CurrentAddress"] != DBNull.Value ? rdr["CurrentAddress"].ToString() : "",
                        Hobbies = rdr["Hobbies"] != DBNull.Value ? rdr["Hobbies"].ToString() : "",
                        LastSchoolAttended = rdr["LastSchoolAttended"] != DBNull.Value ? rdr["LastSchoolAttended"].ToString() : "",
                        DateOfAdmission = rdr["DateOfAdmission"] != DBNull.Value ? Convert.ToDateTime(rdr["DateOfAdmission"]) : DateTime.MinValue,
                        BloodGroup = rdr["BloodGroup"] != DBNull.Value ? rdr["BloodGroup"].ToString() : "",
                        FatherName = rdr["FatherName"] != DBNull.Value ? rdr["FatherName"].ToString() : "",
                        FatherQualification = rdr["FatherQualification"] != DBNull.Value ? rdr["FatherQualification"].ToString() : "",
                        Nationality = rdr["Nationality"] != DBNull.Value ? rdr["Nationality"].ToString() : "",
                        MotherQualification = rdr["MotherQualification"] != DBNull.Value ? rdr["MotherQualification"].ToString() : "",
                        FatherOccupation = rdr["FatherOccupation"] != DBNull.Value ? rdr["FatherOccupation"].ToString() : "",
                        MotherOccupation = rdr["MotherOccupation"] != DBNull.Value ? rdr["MotherOccupation"].ToString() : "",
                        StudentEmail = rdr["StudentEmail"] != DBNull.Value ? rdr["StudentEmail"].ToString() : "",
                        FatherOfficeAddress = rdr["FatherOfficeAddress"] != DBNull.Value ? rdr["FatherOfficeAddress"].ToString() : "",
                        MotherName = rdr["MotherName"] != DBNull.Value ? rdr["MotherName"].ToString() : "",
                        MobileNo = rdr["MobileNo"] != DBNull.Value ? rdr["MobileNo"].ToString() : "",
                        FatherOfficeMobileNo = rdr["FatherOfficeNo"] != DBNull.Value ? Convert.ToInt64(rdr["FatherOfficeNo"]) : 0,
                        MotherOfficeMobileNo = rdr["MotherOfficeNo"] != DBNull.Value ? Convert.ToInt64(rdr["MotherOfficeNo"]) : 0,
                        MotherOfficeAddress = rdr["MotherOfficeAddress"] != DBNull.Value ? rdr["MotherOfficeAddress"].ToString() : "",
                        StudentPhotos = rdr["StudentPhoto"] != DBNull.Value ? rdr["StudentPhoto"].ToString() : "",
                        FatherPhotos = rdr["FatherPhoto"] != DBNull.Value ? rdr["FatherPhoto"].ToString() : "",
                        MotherPhotos = rdr["MotherPhoto"] != DBNull.Value ? rdr["MotherPhoto"].ToString() : "",
                        StudentAadharPhotos = rdr["StudentAadharPhoto"] != DBNull.Value ? rdr["StudentAadharPhoto"].ToString() : "",
                        AcademicYear = rdr["AcademicYear"] != DBNull.Value ? rdr["AcademicYear"].ToString() : "",
                        AdmissionStage = rdr["AdmissionStage"] != DBNull.Value ? rdr["AdmissionStage"].ToString() : "",
                        parentEmail = rdr["parentEmail"] != DBNull.Value ? rdr["parentEmail"].ToString() : "",
                        IsInHostel = rdr["IsInHostel"] != DBNull.Value ? Convert.ToBoolean(rdr["IsInHostel"]) : false,
                        AdminName = rdr["AdminName"] != DBNull.Value ? rdr["AdminName"].ToString():"",
                        SchoolauthorizedPersonName = rdr["authorizedPersonName"] != DBNull.Value ? rdr["authorizedPersonName"].ToString():"",
                        schoolMobile = rdr["schoolMobile"] != DBNull.Value ? rdr["schoolMobile"].ToString():"",
                        website = rdr["website"] != DBNull.Value ? rdr["website"].ToString():"",
                        LandLineNo = rdr["LandLineNo"] != DBNull.Value ? rdr["LandLineNo"].ToString():"",
                        SchoolName = rdr["SchoolName"] != DBNull.Value ? rdr["SchoolName"].ToString():"",

                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return null;
        }
        #endregion
        #region Assignment Management
        public List<StudentAssignmentModel> GetStudentAssignmentById(int studentid)
        {
            List<StudentAssignmentModel> list = new List<StudentAssignmentModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentAssignment", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectassignment");
                cmd.Parameters.AddWithValue("@studentid", studentid);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new StudentAssignmentModel
                    {
                        id = rdr["id"] != DBNull.Value ? Convert.ToInt32(rdr["id"]) : 0,
                        StudentId = rdr["studentid"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        title = rdr["title"].ToString(),
                        description = rdr["descr"].ToString(),
                        assigmentattachment = rdr["attachment"].ToString(),
                        assignmentDate = Convert.ToDateTime(rdr["assigndate"]).ToString("dd-MMM-yyyy"),
                        completionDate = Convert.ToDateTime(rdr["completiondate"]).ToString("dd-MMM-yyyy"),
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return list;
        }
        public bool insertAssingment(SubmitAssignmentModel assg)
        {
            string uniqueFileName = null;
            if (assg.attachment != null && assg.attachment.ContentLength > 0)
            {
                string fileName = assg.attachment.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                assg.attachmentUrl = "/Upload/" + uniqueFileName;
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentAssignment", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action","submitassignment");
                cmd.Parameters.AddWithValue("@id", assg.id);
                cmd.Parameters.AddWithValue("@studentId", assg.studentId);
                cmd.Parameters.AddWithValue("@attachment", assg.attachmentUrl);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    if (!string.IsNullOrEmpty(assg.attachmentUrl))
                    {
                        string filePath = HttpContext.Current.Server.MapPath("~/Upload/") + uniqueFileName;
                        assg.attachment.SaveAs(filePath);
                    }
                }
                return res > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        #endregion
        #region Circular Management
        public List<CircularModel> GetAllCirculars(int adminid)
        {
            List<CircularModel> list = new List<CircularModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_CircularManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetAllCircular");
                cmd.Parameters.AddWithValue("@userid", adminid);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new CircularModel
                    {
                        CircularId = rdr["CircularId"] != DBNull.Value ? Convert.ToInt32(rdr["CircularId"]) : 0,
                        CircularTitle = rdr["CircularTitle"]?.ToString(),

                        CircularDate = rdr["CircularDate"] != DBNull.Value ? Convert.ToDateTime(rdr["CircularDate"]) : DateTime.MinValue,
                        UploadAttachment = rdr["UploadAttachment"]?.ToString(),
                        CircularDescription = rdr["Description"]?.ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return list;
        }
        #endregion

        #region Circular Management
        public List<CourseModel> GetCourse(int classid,int sectionid,int adminid)
        {
            List<CourseModel> list = new List<CourseModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectcoursedetails");
                cmd.Parameters.AddWithValue("@classid", classid);
                cmd.Parameters.AddWithValue("@sectionid", sectionid);
                cmd.Parameters.AddWithValue("@adminid", adminid);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new CourseModel
                    {
                        subjectName = rdr["Subject"]?.ToString(),
                        teacherName = rdr["TeacherName"]?.ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return list;
        }
        #endregion
        #region Library
        public List<LibraryModel> GetLibraryDetails(int studentid)
        {
            List<LibraryModel> orders = new List<LibraryModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectorderofteacher");
                cmd.Parameters.AddWithValue("@buyerId", studentid);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        orders.Add(new LibraryModel
                        {
                            id = Convert.ToInt32(reader["id"]),
                            addedBy = reader["addedBy"].ToString(),
                            orderId = reader["orderId"].ToString(),
                            userId = reader["userId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["userId"]),
                            bookId = reader["bookId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bookId"]),
                            buyerId = reader["buyerId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["buyerId"]),
                            userType = reader["userType"].ToString(),
                            orderDate = reader["orderDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["orderDate"]),
                            lateFine = reader["lateFine"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["lateFine"]),
                            damageFine = reader["damageFine"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damageFine"]),
                            lostFine = reader["lostFine"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["lostFine"]),
                            quantity = reader["quantity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["quantity"]),
                            price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]),
                            returnDate = reader["returnDate"] == null ? "" : Convert.ToDateTime(reader["returnDate"]).ToString("dd-MMM-yyyy"),
                            totalLateFine = !Convert.ToBoolean(reader["recieveStatus"]) && Convert.ToDateTime(reader["returnDate"]) < DateTime.Now ? (DateTime.Now.Date - Convert.ToDateTime(reader["returnDate"]).Date).Days * Convert.ToDecimal(reader["lateFine"]) : 0,
                            totalDelayDaysCount = !Convert.ToBoolean(reader["recieveStatus"]) && Convert.ToDateTime(reader["returnDate"]).Date < DateTime.Now.Date ? (DateTime.Now.Date - Convert.ToDateTime(reader["returnDate"]).Date).Days : 0,
                            name = reader["userType"].ToString().Equals("student") ? reader["StudentName"].ToString() : reader["userType"].ToString().Equals("teacher") ? reader["TeacherName"].ToString() : null,
                            email = reader["userType"].ToString().Equals("student") ? reader["StudentEmail"].ToString() : reader["userType"].ToString().Equals("teacher") ? reader["TeacherEmail"].ToString() : null,
                            mobile = reader["userType"].ToString().Equals("student") ? Convert.ToInt64(reader["MobileNo"]) : reader["userType"].ToString().Equals("teacher") ? Convert.ToInt64(reader["TeacherMobile"]) : 0,
                            recieveStatus = Convert.ToBoolean(reader["recieveStatus"]),
                            bookName = reader["title"].ToString()
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
                connection.Close();
            }
        }
        #endregion
        #region TimeTables

        public List<ExamTimeTableModel> GetExamTimeTableForStudent(int studentid, int scheduledid)
        {
            try
            {
                //List<ExamTimeTableModel> returnList = new List<ExamTimeTableModel>();
                List<ExamTimeTableModel> list = new List<ExamTimeTableModel>();
                string StudentAcademic = GetStudentById(studentid).AcademicYear;
                string[] stacYears = StudentAcademic.Split('-');
                int stStart = Convert.ToInt32(stacYears[0]);
                int stEnd = Convert.ToInt32(stacYears[1]);
                SqlCommand cmd = new SqlCommand("sp_ManageExamTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectexamtimetableforstudent");
                cmd.Parameters.AddWithValue("@studentid", studentid);
                cmd.Parameters.AddWithValue("@scheduledid", scheduledid);
               
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        string ScheduleAcademicYear = res["academicYear"].ToString();
                        string[] scacYears = ScheduleAcademicYear.Split('-');
                        int scStart = Convert.ToInt32(scacYears[0]);
                        int scEnd = Convert.ToInt32(scacYears[1]);

                        if (stStart <= scStart && stEnd <= scEnd)
                        {
                            list.Add(new ExamTimeTableModel
                            {
                                id = Convert.ToInt32(res["id"]),
                                classId = Convert.ToInt32(res["classId"]),
                                className = res["classname"].ToString(),
                                academicYear = res["academicYear"].ToString(),
                                attachmentName = res["attachment"].ToString(),
                                description = res["description"].ToString(),
                                examName = res["examname"].ToString(),
                            });
                        }
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
                connection.Close();
            }
        }
        public List<TodaySchedulesModel> GetTodayScheduleOfStudent(int classid,int sectionid, string day)
        {
            try
            {
                List<TodaySchedulesModel> list = new List<TodaySchedulesModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageStudentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "SelectTimetableofstudentperDay");
                cmd.Parameters.AddWithValue("@classid", classid);
                cmd.Parameters.AddWithValue("@sectionid", sectionid);
                cmd.Parameters.AddWithValue("@day", day);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    TodaySchedulesModel category = new TodaySchedulesModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        subjectName = res["subject"].ToString(),
                        className = res["ClassName"].ToString(),
                        sectionName = res["sectionName"].ToString(),
                        subjectId = Convert.ToInt32(res["subjectId"]),
                        from = DateTime.Today.Add((TimeSpan)res["fromTime"]).ToString("hh:mm tt"),
                        to = DateTime.Today.Add((TimeSpan)res["toTime"]).ToString("hh:mm tt"),
                        day = res["day"].ToString(),
                        combineTime = $"{DateTime.Today.Add((TimeSpan)res["fromTime"]).ToString("hh:mm tt")} - {DateTime.Today.Add((TimeSpan)res["toTime"]).ToString("hh:mm tt")}",
                        teacherName = res["teachername"].ToString()
                    };
                    list.Add(category);
                }

                return list;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

        }
        #endregion
        #region Notice
        public List<NoticesModel> GetStudentNotices(string userType, int userId, int adminid)
        {
            try
            {
                var notices = new List<NoticesModel>();

                cmd = new SqlCommand("sp_NoticeManagement", connection);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "GetNoticeByUsers");
                cmd.Parameters.AddWithValue("@UserType", userType);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@adminid", adminid);

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    notices.Add(new NoticesModel
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
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                cmd.Dispose();
            }
        }
        #endregion
        #region Transport
        public List<TrasportDetailsModel> GetTransportDetails(int studentid)
        {
            List<TrasportDetailsModel> list = new List<TrasportDetailsModel>();

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_ManageStudentPanel", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selecttransportdata");
                    cmd.Parameters.AddWithValue("@studentid", studentid);

                    connection.Open();

                    using (SqlDataReader res = cmd.ExecuteReader())
                    {
                        while (res.Read())
                        {
                            list.Add(new TrasportDetailsModel
                            {
                                bussNo = res["bussno"]?.ToString() ?? "",
                                driverName = res["name"]?.ToString() ?? "",
                                ContactNo = res["contactno"] != DBNull.Value ? Convert.ToInt64(res["contactno"]) : 0,
                                Address = res["address"]?.ToString() ?? "",
                                DLFileName = res["driverfile"]?.ToString() ?? "",
                                pickupPoint = res["pickuppoint"]?.ToString() ?? "",
                                busCharge = res["buscharge"] != DBNull.Value ? Convert.ToDecimal(res["buscharge"]) : 0
                            });
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return list;
        }
        #endregion
        #region Communication
        public List<stCommunicationModel> GetCommunication(int studentid,int isSendTo)
        {
            try
            {
                List<stCommunicationModel> list = new List<stCommunicationModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageStudentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectcommunication");
                cmd.Parameters.AddWithValue("@studentid", studentid);
                cmd.Parameters.AddWithValue("@isSendto", isSendTo);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new stCommunicationModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        attachmentName = res["attachment"].ToString(),
                        title = res["title"].ToString(),
                        description = res["description"].ToString(),
                        teacherName = res["teachername"].ToString()
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        public List<WardenCommunicationModel> GetWardenCommunication(int studentid)
        {
            List<WardenCommunicationModel> list = new List<WardenCommunicationModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectwardencommunication");
                cmd.Parameters.AddWithValue("@studentid", studentid);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    WardenCommunicationModel communication = new WardenCommunicationModel
                    {
                        CommunicationId = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0,
                        Title = rdr["Title"]?.ToString(),
                        Attachment = rdr["Attachment"]?.ToString(),
                        Description = rdr["Description"]?.ToString()
                    };

                    list.Add(communication);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion
        #region Manage Fee
        public List<FeeModel> GetFeeRecord(int studentid)
        {
            try
            {
                List<FeeModel> list = new List<FeeModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageStudentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectfee");
                cmd.Parameters.AddWithValue("@studentid", studentid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new FeeModel
                    {
                        className = res["classname"].ToString(),
                        feeDetails = JsonConvert.DeserializeObject<List<FeeDetailModel>>(res["feeDetails"].ToString())
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion
        #region Manage Hostel
        public List<HostelDetailModel> GetHostelDetails(int studentid)
        {
            try
            {
                List<HostelDetailModel> list = new List<HostelDetailModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageStudentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selecthosteldetails");
                cmd.Parameters.AddWithValue("@studentid", studentid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new HostelDetailModel
                    {
                        feeSlip = res["FeeSlip"].ToString(),
                        feeType = res["FeeType"].ToString(),
                        roomNo = res["roomnumber"].ToString(),
                        blockName = res["blockname"].ToString(),
                        roomType = res["roomtype"].ToString(),
                        totalFee = Convert.ToDecimal(res["totalfee"]),
                        paidFee = Convert.ToDecimal(res["feessubmitted"]),
                        remainingFee = Convert.ToDecimal(res["remainingfees"]),
                        roommates = Convert.ToInt32(res["roommates"]),
                        wardenName = res["name"].ToString(),
                        wardenEmail = res["email_id"].ToString(),
                        wardenMobile = Convert.ToInt64(res["mobileno"]),
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        public List<MealMode> GetMealSchedule(string day,int studentid)
        {
            try
            {
                List<MealMode> list = new List<MealMode>();
                SqlCommand cmd = new SqlCommand("sp_ManageStudentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectmeals");
                cmd.Parameters.AddWithValue("@studentid", studentid);
                cmd.Parameters.AddWithValue("@day", day);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new MealMode
                    {
                        Day = res["day"].ToString(),
                        starttime = res["starttime"].ToString(),
                        endtime = res["endtime"].ToString(),
                        menus = JsonConvert.DeserializeObject < List < MealMenuModel >> (res["menus"].ToString())
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion
        #region Attendance
        public List<AttendanceModel> GetAttendanceReport(int studentid,int classid,int sectionid)
        {
            try
            {
                List<AttendanceModel> list = new List<AttendanceModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageStudentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectattendancerepo");
                cmd.Parameters.AddWithValue("@studentid", studentid);
                cmd.Parameters.AddWithValue("@classid", classid);
                cmd.Parameters.AddWithValue("@sectionid", sectionid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new AttendanceModel
                    {
                        attendanceStatus = Convert.ToBoolean(res["AttendanceStatus"]),
                        attendanceDate = Convert.ToDateTime(res["attendanceDate"]).ToString("dd-MMM-yyyy")
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion
        #region Hostel Problem
        public bool InsertHostelProblem(HostelProblemModel hm,out string errorMessage)
        {
            errorMessage = "";
            try
            {
                cmd = new SqlCommand("sp_ManageHostelProblem", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@studentid", hm.studentid);
                cmd.Parameters.AddWithValue("@problem", hm.problem);
                cmd.Parameters.AddWithValue("@action", "insertproblem");
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res<=0)
                {
                    errorMessage = "Something went wrong!";
                }
                return res > 0;
            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
                return false;

            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                cmd.Dispose();
            }
        }
        public List<HostelProblemModel> GetHostelProblem(int studentid)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageHostelProblem", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@studentid", studentid);
                cmd.Parameters.AddWithValue("@action", "selectproblem");
                List<HostelProblemModel> list = new List<HostelProblemModel>();
                connection.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new HostelProblemModel
                        {
                            problem = res["problem"].ToString(),
                            problemStatus = Convert.ToInt32(res["problemStatus"]),
                            createdAt = Convert.ToDateTime(res["createdAt"]).ToString("dd-MMM-yyyy"),
                            reason = res["reason"].ToString()
                        });
                    }
                }
                return list;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                cmd.Dispose();
            }
        }
        #endregion

        #region Exam
        public List<StudentAttendanceModel> GetStudentMarks(int studentid,int examId)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageStudentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@studentid", studentid);
                cmd.Parameters.AddWithValue("@examid", examId);
                cmd.Parameters.AddWithValue("@action", "marksofstudent");
                List<StudentAttendanceModel> list = new List<StudentAttendanceModel>();
                connection.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new StudentAttendanceModel
                        {
                            studentName = res["studentName"].ToString(),
                            subjectName = res["subject"].ToString(),
                            examName = res["examName"].ToString(),
                            subjectid = Convert.ToInt32(res["subjectid"]),
                            examId = Convert.ToInt32(res["examId"]),
                            theoryMarks = Convert.ToInt32(res["theoryMarks"]),
                            practicalMarks = Convert.ToInt32(res["practicalMarks"]),
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                cmd.Dispose();
            }
        }
        #endregion

    }
}