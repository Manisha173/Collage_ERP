using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using College_ERP.Models;
using College_ERP.Models.Admin;
using System.Configuration;
using static College_ERP.Models.Security.main;
using PdfSharp.Charting;
using College_ERP.Models.Teacher;

namespace College_ERP.Models.Security
{
    public class SecurityService
    {
        private readonly AdminServices.AdminServices _adminService;
        private readonly SqlConnection conn;
        private SqlCommand cmd;
        public SecurityService()
        {
            _adminService = new AdminServices.AdminServices();
            conn = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
        }
        public SecurityDetails GetSecurityDetails(int userid)
        {
            try
            {
                conn.Open();
                cmd = new SqlCommand("sp_ManageSecurity", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "GetProfileDetails");
                cmd.Parameters.AddWithValue("@id", userid);
                SecurityDetails list = new SecurityDetails();
                var res = cmd.ExecuteReader();
                if(res!=null)
                {
                    while (res.Read())
                    {
                        list.securityId = Convert.ToInt32(res["id"]);
                        list.securityImage = res["image"].ToString();
                        list.securityName = res["name"].ToString();
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
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                cmd.Dispose();
            }
        }
        #region Visitor Management
        public List<RegistrationModel> GetAllStudents(int userid)
        {
            List<RegistrationModel> list = new List<RegistrationModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectAllStudent");
                cmd.Parameters.AddWithValue("@userid", userid);
                conn.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new RegistrationModel
                    {
                        StudentId = rdr["StudentId"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        AdmissionNo = rdr["AdmissionNo"]?.ToString(),
                        StudentName = rdr["StudentName"]?.ToString(),
                        MotherTongue = rdr["MotherTougue"]?.ToString(),
                        ClassName = rdr["ClassName"]?.ToString(),
                        ClassId = Convert.ToInt32(rdr["ClassId"]),
                        SectionName = rdr["SectionName"]?.ToString(),
                        SectionId = Convert.ToInt32(rdr["SectionId"]),
                        Gender = rdr["Gender"]?.ToString(),
                        Religion = rdr["Religion"]?.ToString(),
                        Caste = rdr["Caste"]?.ToString(),
                        PlaceOfBirth = rdr["PlaceOfBirth"]?.ToString(),
                        DOB = rdr["DOB"] != DBNull.Value ? Convert.ToDateTime(rdr["DOB"]) : DateTime.MinValue,
                        StateId = Convert.ToInt32(rdr["st_Id"]),
                        StateName = rdr["stateName"]?.ToString(),
                        CityId = Convert.ToInt32(rdr["city_Id"]),
                        ObtainedMarks = Convert.ToInt32(rdr["ObtainedMarks"]),
                        TotalMarks = Convert.ToInt32(rdr["TotalMarks"]),
                        CityName = rdr["City_Name"]?.ToString(),
                        Address = rdr["Address"]?.ToString(),
                        AadharNo = rdr["AadharNumber"]?.ToString(),
                        YearOfPassing = rdr["YearOfPassing"]?.ToString(),
                        CurrentAddress = rdr["CurrentAddress"]?.ToString(),
                        Hobbies = rdr["Hobbies"]?.ToString(),
                        Nationality = rdr["Nationality"]?.ToString(),
                        LastSchoolAttended = rdr["LastSchoolAttended"]?.ToString(),
                        DateOfAdmission = rdr["DateOfAdmission"] != DBNull.Value ? Convert.ToDateTime(rdr["DateOfAdmission"]) : DateTime.MinValue,
                        BloodGroup = rdr["BloodGroup"]?.ToString(),
                        FatherName = rdr["FatherName"]?.ToString(),
                        FatherQualification = rdr["FatherQualification"]?.ToString(),
                        MotherQualification = rdr["MotherQualification"]?.ToString(),
                        FatherOccupation = rdr["FatherOccupation"]?.ToString(),
                        MotherOccupation = rdr["MotherOccupation"]?.ToString(),
                        StudentEmail = rdr["StudentEmail"]?.ToString(),
                        FatherOfficeAddress = rdr["FatherOfficeAddress"]?.ToString(),
                        MotherName = rdr["MotherName"]?.ToString(),
                        MobileNo = rdr["MobileNo"]?.ToString(),
                        FatherOfficeMobileNo = Convert.ToInt64(rdr["FatherOfficeNo"]),
                        MotherOfficeMobileNo = Convert.ToInt64(rdr["MotherOfficeNo"]),
                        MotherOfficeAddress = rdr["MotherOfficeAddress"]?.ToString(),
                        StudentPhotos = rdr["StudentPhoto"]?.ToString(),
                        FatherPhotos = rdr["FatherPhoto"]?.ToString(),
                        MotherPhotos = rdr["MotherPhoto"]?.ToString(),
                        StudentAadharPhotos = rdr["StudentAadharPhoto"]?.ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if(conn.State == ConnectionState.Open)
                conn.Close();
            }

            return list;
        }
        public List<StaffModel> GetStaffList(string type,int adminid, string search = null)
        {
            List<StaffModel> list = new List<StaffModel>();

            try
            {
                cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectStaff");
                cmd.Parameters.AddWithValue("@visitorType", type);
                cmd.Parameters.AddWithValue("@userId", adminid);
                cmd.Parameters.AddWithValue("@search", search??null);
                conn.Open();
                var res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new StaffModel
                    {
                        staffId = Convert.ToInt32(res["staffId"]),
                        staffName = res["staffName"].ToString(),
                        staffmobile = Convert.ToInt64(res["staffMobile"])
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
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                cmd.Dispose();
            }
        }
        public bool InsertVisitor(VisitorModel bs)
        {
            string uniqueFileName = null;
            if (bs.image != null && bs.image.ContentLength > 0)
            {
                string fileName = bs.image.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                bs.imageName = "/Upload/" + uniqueFileName;
            }
            try
            {
                cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", bs.id > 0 ? "updateVisitor" : "insertVisitor");
                cmd.Parameters.AddWithValue("@id", bs.id);
                cmd.Parameters.AddWithValue("@userId", bs.userId);
                cmd.Parameters.AddWithValue("@name", bs.name);
                cmd.Parameters.AddWithValue("@mobile", bs.mobile);
                cmd.Parameters.AddWithValue("@email", bs.email);
                cmd.Parameters.AddWithValue("@address", bs.address);
                cmd.Parameters.AddWithValue("@image", bs.imageName);
                SqlParameter outputIdParam = new SqlParameter("@visitorId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputIdParam);
                conn.Open();
                int res = cmd.ExecuteNonQuery();
                bs.vid= (int)outputIdParam.Value;
                if (res > 0)
                {
                    InsertVisitorMeeting(bs);
                }
                if (res>0 && uniqueFileName != null)
                {
                    string filePath = HttpContext.Current.Server.MapPath("~/Upload/") + uniqueFileName;
                    bs.image.SaveAs(filePath);
                }
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
        public bool InsertVisitorMeeting(VisitorModel bs)
        {
            string uniqueFileName = null;
            if (bs.image != null && bs.image.ContentLength > 0)
            {
                string fileName = bs.image.FileName;
                uniqueFileName = Guid.NewGuid() + "_" + fileName;
                bs.imageName = "/Upload/" + uniqueFileName;
            }
            try
            {
                 cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "insertVisitorMeeting");
                cmd.Parameters.AddWithValue("@vid", bs.vid);
                //cmd.Parameters.AddWithValue("@userId", bs.userId);
                cmd.Parameters.AddWithValue("@meetingPersonId", bs.personId>0? bs.personId:bs.studentId);
                cmd.Parameters.AddWithValue("@visitorType", bs.userType!= "student"?bs.role: "student");
                cmd.Parameters.AddWithValue("@reason", bs.reason);
                cmd.Parameters.AddWithValue("@remark", bs.remark);
                cmd.Parameters.AddWithValue("@image", bs.imageName);

                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                int res = cmd.ExecuteNonQuery();
                if (res > 0 && !string.IsNullOrEmpty(uniqueFileName))
                {
                    string filePath = HttpContext.Current.Server.MapPath("~/Upload/") + uniqueFileName;
                    bs.image.SaveAs(filePath);
                }
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
                cmd.Dispose();
            }
        }
        public List<VisitorModel> GetAllVisitorsList(int userId, string search = null)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAllVisitor");
                //cmd.Parameters.AddWithValue("@loginState",SqlDbType.Int).Value=(object)loginStatus??DBNull.Value;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@search", search??null);
                conn.Open();
                List<VisitorModel> list = new List<VisitorModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new VisitorModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            name = res["name"].ToString(),
                            email = res["email"].ToString(),
                            mobile = Convert.ToInt64(res["mobile"]),
                            imageName = res["image"].ToString(),
                            address = res["address"].ToString(),
                            loginTime = res["loginTime"].ToString(),
                            logOutTime = res["logoutTime"].ToString()
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
                cmd.Dispose();

            }
        }
        public List<VisitorModel> GetAllVisitorsListForReport(int userId)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAllVisitorforrepo");
                cmd.Parameters.AddWithValue("@userId", userId);
                conn.Open();
                List<VisitorModel> list = new List<VisitorModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new VisitorModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            name = res["name"].ToString(),
                            email = res["email"].ToString(),
                            mobile = Convert.ToInt64(res["mobile"]),
                            imageName = res["image"].ToString(),
                            address = res["address"].ToString(),
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
                cmd.Dispose();

            }
        }
        public List<VisitorModel> GetAllVisitorsListByFilter(string filter,int userid)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAllVisitorByFilter");
                cmd.Parameters.AddWithValue("@filter", filter);
                cmd.Parameters.AddWithValue("@userid", userid);
                conn.Open();
                List<VisitorModel> list = new List<VisitorModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new VisitorModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            name = res["name"].ToString(),
                            email = res["email"].ToString(),
                            mobile = Convert.ToInt64(res["mobile"]),
                            imageName = res["image"].ToString(),
                            address = res["address"].ToString()
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
                cmd.Dispose();

            }
        } 
        public List<VisitorModel> GetLoginVisitorsList(int userid, string search = null)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectLoginVisitor");
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@search", search??null);
                conn.Open();
                List<VisitorModel> list = new List<VisitorModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new VisitorModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            vid = Convert.ToInt32(res["vid"]),
                            meetid = Convert.ToInt32(res["meetid"]),
                            name = res["name"].ToString(),
                            email = res["email"].ToString(),
                            mobile = Convert.ToInt64(res["mobile"]),
                            imageName = res["image"].ToString(),
                            address = res["address"].ToString(),
                            loginStatus = Convert.ToBoolean(res["loginStatus"]),
                            loginTime = res["loginTime"].ToString(),
                            logOutTime = res["logOutTime"].ToString()

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
                cmd.Dispose();

            }
        }
        public bool LogoutVisitor(int id)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "logoutVisitor");
                cmd.Parameters.AddWithValue("@id", id);
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
                cmd.Dispose();
            }
        }
        public List<VisitorModel> GetVisitorPreviousMeeting(int id)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectVisitorPreviousMeeting");
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                List<VisitorModel> list = new List<VisitorModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new VisitorModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            vid = Convert.ToInt32(res["vid"]),
                            classid = res["classid"]!=DBNull.Value?Convert.ToInt32(res["classid"]):0,
                            sectionid = res["sectionid"]!=DBNull.Value?Convert.ToInt32(res["sectionid"]):0,
                            meetid = Convert.ToInt32(res["meetid"]),
                            name = res["name"].ToString(),
                            email = res["email"].ToString(),
                            mobile = Convert.ToInt64(res["mobile"]),
                            imageName = res["image"].ToString(),
                            address = res["address"].ToString(),
                            loginStatus = Convert.ToBoolean(res["loginStatus"]),
                            loginTime = res["loginTime"].ToString(),
                            logOutTime = res["logOutTime"].ToString(),
                            reason = res["reason"].ToString(),
                            remark = res["remark"].ToString(),
                            userType = res["visitorType"].ToString() != "student" && res["visitorType"] != null ? "staff":"student",
                            staffRole = res["visitorType"].ToString(),
                            personId = Convert.ToInt32(res["meetingPersonId"]),
                            //roomNo = Convert.ToInt32(res["roomno"])
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
                cmd.Dispose();

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
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
        public List<VisitorModel> GetVisitorHistory(int visitorid, string search = null)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectvisitorhistory");
                cmd.Parameters.AddWithValue("@id", visitorid);
                cmd.Parameters.AddWithValue("@search", search??null);
                conn.Open();
                List<VisitorModel> list = new List<VisitorModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new VisitorModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            vid = Convert.ToInt32(res["vid"]),
                            classid = res["classid"]!=DBNull.Value?Convert.ToInt32(res["classid"]):0,
                            sectionid = res["sectionid"]!=DBNull.Value?Convert.ToInt32(res["sectionid"]):0,
                            meetid = Convert.ToInt32(res["meetid"]),
                            name = res["name"].ToString(),
                            email = res["email"].ToString(),
                            mobile = Convert.ToInt64(res["mobile"]),
                            imageName = res["imageName"].ToString(),
                            address = res["address"].ToString(),
                            loginStatus = Convert.ToBoolean(res["loginStatus"]),
                            loginTime = res["loginTime"].ToString(),
                            logOutTime = res["logOutTime"].ToString(),
                            reason = res["reason"].ToString(),
                            remark = res["remark"].ToString(),
                            userType = res["visitorType"].ToString() != "student" && res["visitorType"] != null ? "staff":"student",
                            staffRole = res["visitorType"].ToString(),
                            personId = Convert.ToInt32(res["meetingPersonId"]),
                            //roomNo = Convert.ToInt32(res["roomno"]),
                            personName = res["personname"].ToString()
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
                cmd.Dispose();

            }
        }
        public List<string> GetRoleOfStaff()
        {
            try
            {
                cmd = new SqlCommand("sp_ManageVisitor", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectstaffrole");
                List<string> role = new List<string>();
                conn.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        role.Add(res["role"].ToString());
                    }
                }
                return role;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                cmd.Dispose();

            }
        }
        public List<RoomModel> GetRoomsByBlockId(int securityid, string search = null)
        {
            List<RoomModel> rooms = new List<RoomModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageSecurity", conn);
                cmd.Parameters.AddWithValue("@Action", "SELECTROOMBYBLOCKID");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", securityid);
                cmd.Parameters.AddWithValue("@search", search ?? null);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    rooms.Add(new RoomModel
                    {
                        RoomId = Convert.ToInt32(dr["RoomId"]),
                        RoomNo = Convert.ToInt32(dr["RoomNumber"]),
                    });
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            return rooms;
        }
        public List<College_ERP.Models.Security.main.UserOrderModel> SelectUsersByRoomNo(int roomNo, int adminid, string search = null)
        {
            List<College_ERP.Models.Security.main.UserOrderModel> list = new List<College_ERP.Models.Security.main.UserOrderModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageVisitor", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GetStudentsByRoomNo");
                cmd.Parameters.AddWithValue("@RoomNo", roomNo);
                cmd.Parameters.AddWithValue("@userid", adminid);
                cmd.Parameters.AddWithValue("@search", search??null);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        list.Add(new College_ERP.Models.Security.main.UserOrderModel
                        {

                            userName = rd["name"].ToString(),
                            emailId = rd["email"] == DBNull.Value ? "" : rd["email"].ToString(),
                            blockId = rd["blockId"] == DBNull.Value ? 0 : Convert.ToInt32(rd["blockId"]),
                            hostelId = rd["hostelId"] == DBNull.Value ? 0 : Convert.ToInt32(rd["hostelId"]),
                            studentid = rd["id"] == DBNull.Value ? 0 : Convert.ToInt32(rd["id"]),

                            address = rd["address"] == DBNull.Value ? "" : rd["address"].ToString(),
                            roomNo = rd["roomNo"] == DBNull.Value ? 0 : Convert.ToInt32(rd["roomNo"])
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
        #endregion

        public List<NoticeModel> GetSecurityNotices(string userType, int userId,int adminid, string search = null)
        {
            var notices = new List<NoticeModel>();

            SqlCommand cmd = new SqlCommand("sp_NoticeManagement", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "GetNoticeByUsers");
            cmd.Parameters.AddWithValue("@UserType", userType);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@adminid", adminid);
            cmd.Parameters.AddWithValue("@search", search??null);

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
        public SecurityDashboardModel GetSecurityDashboard(int userId)
        {
            SecurityDashboardModel model = new SecurityDashboardModel();

            using (SqlCommand cmd = new SqlCommand("sp_ManageSecurityDashboard", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DashboardCount");
                cmd.Parameters.AddWithValue("@userId", userId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    model.totalvisitors = Convert.ToInt32(dr["totalvisitors"]);
                    model.loginvisitors = Convert.ToInt32(dr["loginvisitors"]);
                }
                conn.Close();
            }

            return model;
        }
        public SecurityDashboardModel GetNoticeCount(int AdminId,int userId)
        {
            SecurityDashboardModel model = new SecurityDashboardModel();

            using (SqlCommand cmd = new SqlCommand("sp_ManageSecurityDashboard", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "noticeCount");
                cmd.Parameters.AddWithValue("@AdminId", AdminId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    model.notice = Convert.ToInt32(dr["notice"]);
                }
                conn.Close();
            }

            return model;
        }
        public int GetAdminId(int userId)
        {
            try
            {
                int adminId = 0;
                cmd = new SqlCommand("sp_loginmanager", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectadminidbysecurity");
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        adminId = Convert.ToInt32(res["Userid"]);
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
        public List<College_ERP.Models.Teacher.StudentModel> StudentsByClassSection(int classId, int sectionId, int userid, int securityid)
        {
            List<College_ERP.Models.Teacher.StudentModel> students = new List<College_ERP.Models.Teacher.StudentModel>();

            SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "SelectStudentByClassSectionForsecurity");
            cmd.Parameters.AddWithValue("@ClassId", classId);
            cmd.Parameters.AddWithValue("@SectionId", sectionId);
            cmd.Parameters.AddWithValue("@userid", userid);
            cmd.Parameters.AddWithValue("@id", securityid);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new College_ERP.Models.Teacher.StudentModel
                {
                    StudentId = Convert.ToInt32(reader["StudentId"]),
                    StudentName = reader["StudentName"].ToString()
                });
            }


            return students;
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