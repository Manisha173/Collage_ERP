using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using static College_ERP.Models.DriverServices.main;

namespace College_ERP.Models.DriverServices
{
    public class DriverService
    {
        private readonly SqlConnection connection;
        private SqlCommand cmd;
        public DriverService()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
        }
        #region get admin id
        public int GetAdminId(int driverId)
        {
            try
            {
                int adminId = 0;
                cmd = new SqlCommand("sp_loginmanager", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectadminidbydriver");
                cmd.Parameters.AddWithValue("@id", driverId);
                connection.Open();
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
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
                cmd.Dispose();

            }
        }
        #endregion
        #region Driver Profile
        public List<DriverProfileModel> GetDriverProfile(int driverid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageDriverPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectdriverprofile");
                cmd.Parameters.AddWithValue("@driverid", driverid);
                connection.Open();
                List<DriverProfileModel> list = new List<DriverProfileModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new DriverProfileModel
                        {
                            Id = res["id"] != DBNull.Value ? Convert.ToInt32(res["id"]) : 0,
                            BusId = res["busId"] != DBNull.Value ? Convert.ToInt32(res["busId"]) : 0,
                            EmployeeId = res["EmployeeId"] != DBNull.Value ? res["EmployeeId"].ToString() : "",
                            Name = res["name"] != DBNull.Value ? res["name"].ToString() : "",
                            ContactNo = res["contactNo"] != DBNull.Value ? Convert.ToInt64(res["contactNo"]) : 0,
                            FatherName = res["fatherName"] != DBNull.Value ? res["fatherName"].ToString() : "",
                            DLNo = res["dlNo"] != DBNull.Value ? res["dlNo"].ToString() : "",
                            AdharCardNo = res["adharCardNo"] != DBNull.Value ? Convert.ToInt64(res["adharCardNo"]) : 0,
                            Address = res["address"] != DBNull.Value ? res["address"].ToString() : "",
                            Salary = res["salary"] != DBNull.Value ? Convert.ToInt64(res["salary"]) : 0,
                            DriverFileName = res["driverFile"] != DBNull.Value ? res["driverFile"].ToString() : "",
                            AdharCardFileName = res["adharCardFile"] != DBNull.Value ? res["adharCardFile"].ToString() : "",
                            DLFileName = res["dlFile"] != DBNull.Value ? res["dlFile"].ToString() : "",
                            Email = res["email"] != DBNull.Value ? res["email"].ToString() : "",
                            Type = res["type"] != DBNull.Value ? res["type"].ToString() : "",
                            BusImage = res["image"] != DBNull.Value ? res["image"].ToString() : "",
                            TravelCompanyName = res["travelCompanyName"] != DBNull.Value ? res["travelCompanyName"].ToString() : "",
                            ContactPerson = res["contactPerson"] != DBNull.Value ? res["contactPerson"].ToString() : "",
                            BusNo = res["bussNo"] != DBNull.Value ? Convert.ToInt32(res["bussNo"]) : 0,
                            BusSeatCapacity = res["bussSeatCapacity"] != DBNull.Value ? Convert.ToInt32(res["bussSeatCapacity"]) : 0,
                            BussCharge = res["bussCharge"] != DBNull.Value ? Convert.ToInt32(res["bussCharge"]) : 0,
                            ContactNumber = res["contactNo"] != DBNull.Value ? Convert.ToInt64(res["contactNo"]) : 0
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

            }
        }
        #endregion
        #region Announcements
        public List<CircularModel> GetAllCirculars(int userid)
        {
            List<CircularModel> list = new List<CircularModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_TeacherRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "SelectAllCircular");
                cmd.Parameters.AddWithValue("@userid", userid);
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
        #region Notice
        public List<NoticesModel> GetDriverNotices(string userType, int userId, int adminid)
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
        #region Driver Problem
        public bool InsertDriverProblem(DriverProblemModel hm, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                cmd = new SqlCommand("sp_ManageDriverProblem", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@driverid", hm.driverid);
                cmd.Parameters.AddWithValue("@problem", hm.problem);
                cmd.Parameters.AddWithValue("@action", "insertproblem");
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    errorMessage = "Something went wrong!";
                }
                return res > 0;
            }
            catch (Exception ex)
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
        public List<DriverProblemModel> GetDriverProblem(int driverid)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageDriverProblem", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@driverid", driverid);
                cmd.Parameters.AddWithValue("@action", "selectproblem");
                List<DriverProblemModel> list = new List<DriverProblemModel>();
                connection.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new DriverProblemModel
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
        #region StudentList In Bus
        public List<StudentListInBusModel> GetStudentListInBus(int driverid)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageDriverPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@driverid", driverid);
                cmd.Parameters.AddWithValue("@action", "selectStudentlistInBus");
                List<StudentListInBusModel> list = new List<StudentListInBusModel>();
                connection.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new StudentListInBusModel
                        {
                            busId = Convert.ToInt32(res["busId"]),
                            busNo = res["bussNo"].ToString(),
                            studentid = Convert.ToInt32(res["studentid"]),
                            studentName = res["studentName"].ToString(),
                            studentPhoto = res["studentPhoto"].ToString(),
                            classid = Convert.ToInt32(res["classid"]),
                            className = res["className"].ToString(),
                            sectionid = Convert.ToInt32(res["sectionid"]),
                            sectionName = res["sectionName"].ToString(),
                            gender = res["gender"].ToString(),
                            address = res["address"].ToString(),
                            currentAddress = res["currentAddress"].ToString(),
                            fatherName = res["fatherName"].ToString(),
                            pickupPoint = res["pickupPoint"].ToString(),
                            mobileNo = Convert.ToInt64(res["mobileNo"]),
                            fatherMobileNo = Convert.ToInt64(res["FatherOfficeNo"]),
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
        #region GetPickupPoints
        public List<PickupPointModel> GetPickupPoints(int driverid)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageDriverPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@driverid", driverid);
                cmd.Parameters.AddWithValue("@action", "GetPickPoint");
                List<PickupPointModel> list = new List<PickupPointModel>();
                connection.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new PickupPointModel
                        {
                            busNo = res["bussNo"].ToString(),
                            busrouteid = Convert.ToInt32(res["busrouteid"]),
                            busCharge = Convert.ToInt32(res["busCharge"]),
                            route = res["route"].ToString(),
                            stateName = res["stateName"].ToString(),
                            cityName = res["City_Name"].ToString(),
                            pickupPoint = res["pickupPoint"].ToString(),
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