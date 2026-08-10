using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using static College_ERP.Models.Warden.main;
using College_ERP.Models.Admin;
using System.Reflection;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Web.Http.Results;
using System.Web.Mvc;
using College_ERP.Models.Teacher;
using static College_ERP.Models.StudentServices.main;
using System.Drawing;
using Antlr.Runtime.Tree;

namespace College_ERP.Models.Warden
{
    public class WardenService
    {
        private readonly SqlConnection connection;
        private SqlCommand cmd;
        public WardenService()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
        }
        public int GetAdminId(int userId)
        {
            try
            {
                int adminId = 0;
                cmd = new SqlCommand("sp_loginmanager", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectadminidbywarden");
                cmd.Parameters.AddWithValue("@id", userId);
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
        public UserData GetUserId(string username)
        {
            UserData data = null;

            try
            {

                using (SqlCommand cmd = new SqlCommand("sp_loginmanager", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectadminIdbywardenusername");
                    cmd.Parameters.AddWithValue("@username", username);

                    connection.Open();

                    using (SqlDataReader res = cmd.ExecuteReader())
                    {
                        if (res.Read())
                        {
                            data = new UserData
                            {
                                userId = res.GetInt32(res.GetOrdinal("userId")),
                                Id = res.GetInt32(res.GetOrdinal("id"))
                            };
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Optional: log the exception
                throw new Exception("Database error occurred while fetching user data.", ex);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }


            return data;
        }


        public string UploadImageToServer(HttpPostedFileBase imageFile)
        {
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string fileName = Guid.NewGuid() + DateTime.Now.ToString("ddMMyyyy") + Path.GetFileName(imageFile.FileName);
                string uploadPath = HttpContext.Current.Server.MapPath("~/Upload");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, fileName);
                imageFile.SaveAs(filePath);

                return "/Upload/" + fileName;
            }

            return null;
        }

        public warden GetWarden(int userid)
        {
            warden list = null;

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_InsertWarden", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectAllWarden");
                    cmd.Parameters.AddWithValue("@userId", userid);

                    connection.Open();

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())   // Only expecting one record
                        {
                            list = new warden
                            {
                                Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0,
                                Name = rdr["Name"]?.ToString() ?? "",
                                MobileNo = rdr["MobileNo"]?.ToString() ?? "",
                                DOB = rdr["DOB"] != DBNull.Value ? Convert.ToDateTime(rdr["DOB"]) : (DateTime?)null,
                                EmailId = rdr["Email_id"]?.ToString() ?? "",
                                stateName = rdr["stateName"]?.ToString() ?? "",
                                cityName = rdr["city_Name"]?.ToString() ?? "",
                                BlockName = rdr["BlockName"]?.ToString() ?? "",
                                ProfilePics = rdr["ProfilePic"]?.ToString() ?? "",
                                st_Id = rdr["StateId"] != DBNull.Value ? Convert.ToInt32(rdr["StateId"]) : 0,
                                city_Id = rdr["CityId"] != DBNull.Value ? Convert.ToInt32(rdr["CityId"]) : 0,
                                Gender = rdr["Gender"]?.ToString() ?? "",
                                BlockId = rdr["BlockId"] != DBNull.Value ? Convert.ToInt32(rdr["BlockId"]) : 0,
                                Document = rdr["Document"]?.ToString() ?? "",
                                Address = rdr["Address"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log properly instead of Console
                throw new Exception("Error fetching warden data: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return list;   // will return null if no data found
        }

        public warden GetWardenById(int wardenId)
        {
            warden list = new warden();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_InsertWarden", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "getWardenById");
                cmd.Parameters.AddWithValue("@Id", wardenId);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list = new warden
                    {
                        Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0,
                        Name = rdr["Name"]?.ToString(),
                        MobileNo = rdr["MobileNo"]?.ToString(),
                        DOB = rdr["DOB"] != DBNull.Value ? Convert.ToDateTime(rdr["DOB"]) : DateTime.MinValue,
                        EmailId = rdr["Email_id"]?.ToString(),
                        stateName = rdr["stateName"]?.ToString(),
                        BlockName = rdr["BlockName"]?.ToString(),
                        st_Id = rdr["StateId"] != DBNull.Value ? Convert.ToInt32(rdr["StateId"]) : 0,
                        city_Id = rdr["CityId"] != DBNull.Value ? Convert.ToInt32(rdr["CityId"]) : 0,
                        cityName = rdr["city_Name"]?.ToString(),
                        Gender = rdr["Gender"]?.ToString(),
                        BlockId = rdr["BlockId"] != DBNull.Value ? Convert.ToInt32(rdr["BlockId"]) : 0,
                        ProfilePics = rdr["ProfilePic"] != DBNull.Value? rdr["ProfilePic"].ToString() : "~/Content/images/default-avatar.png",
                        Document = rdr["Document"]?.ToString(),
                        Address = rdr["Address"]?.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }

            return list;
        }

        public List<StudentDetailModel> GetStudentsInBlock(int userId,string search=null)
        {
            List<StudentDetailModel> students = new List<StudentDetailModel>();

            using (SqlCommand cmd = new SqlCommand("sp_ManageStudentInHostel", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "GetStudentsInBlock");
                cmd.Parameters.AddWithValue("@userid", userId);
                cmd.Parameters.AddWithValue("@search", search??null);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    students.Add(new StudentDetailModel
                    {
                        StudentId = Convert.ToInt32(rdr["StudentId"]),
                        StudentName = rdr["StudentName"].ToString(),
                        MobileNo = rdr["MobileNo"].ToString(),
                        Gender = rdr["Gender"].ToString(),
                        ClassName = rdr["ClassName"].ToString(),
                        SectionName = rdr["SectionName"].ToString(),
                        RoomId = Convert.ToInt32(rdr["RoomId"]),
                        RoomNumber = rdr["RoomNumber"].ToString(),
                        AdmissionNo = rdr["AdmissionNo"].ToString()
                    });
                }
                connection.Close();
            }

            return students;
        }

        public int InsertMeal(List<MealViewModel> models, int userId)
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            try
            {
                int lastMealId = 0;

                if (models != null && models.Count > 0)
                {
                    foreach (var model in models)
                    {
                        model.UserId = userId;
                        using (SqlCommand cmd = new SqlCommand("sp_MealMenuManagement", connection, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@Action", "InsertMeal");
                            cmd.Parameters.AddWithValue("@UserId", model.UserId);
                            cmd.Parameters.AddWithValue("@Day", model.Day);
                            cmd.Parameters.AddWithValue("@StartTime", model.StartTime);
                            cmd.Parameters.AddWithValue("@EndTime", model.EndTime);

                            var result = cmd.ExecuteScalar();
                            if (result == null)
                            {
                                transaction.Rollback();
                                return -1;
                            }
                            int mealId = Convert.ToInt32(result);
                            lastMealId = mealId;

                            if (model.Menus != null && model.Menus.Count > 0)
                            {
                                foreach (var menu in model.Menus)
                                {
                                    using (SqlCommand cmd2 = new SqlCommand("sp_MealMenuManagement", connection, transaction))
                                    {
                                        cmd2.CommandType = CommandType.StoredProcedure;

                                        cmd2.Parameters.AddWithValue("@Action", "InsertMenu");
                                        cmd2.Parameters.AddWithValue("@MealId", mealId);
                                        cmd2.Parameters.AddWithValue("@Menu", menu.Menu);

                                        int res = cmd2.ExecuteNonQuery();
                                        if (res <= 0)
                                        {
                                            transaction.Rollback();
                                            return -1;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }

                return lastMealId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return -1;
            }
            finally
            {
                connection.Close();
            }
        }

        public List<MealViewModel> GetAllMeals(int userId, string search = null)
        {
            List<MealViewModel> meals = new List<MealViewModel>();

            SqlCommand cmd = new SqlCommand("sp_MealMenuManagement", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", "GetAllMeals");
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@search", search?? null);

            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            var mealDict = new Dictionary<int, MealViewModel>();

            while (reader.Read())
            {
                int mealId = Convert.ToInt32(reader["MealId"]);
                if (!mealDict.ContainsKey(mealId))
                {
                    var meal = new MealViewModel
                    {
                        MealId = mealId,
                        Day = reader["Day"].ToString(),
                        CreatedDate = reader["CreatedOn"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedOn"]) : DateTime.MinValue,

                        StartTime = DateTime.Today.Add((TimeSpan)reader["StartTime"]).ToString("hh:mm tt"),
                        EndTime = DateTime.Today.Add((TimeSpan)reader["EndTime"]).ToString("hh:mm tt"),


                        Menus = new List<MenuViewModel>()
                    };

                    mealDict.Add(mealId, meal);
                }
                if (reader["MenuId"] != DBNull.Value)
                {
                    var menu = new MenuViewModel
                    {
                        MenuId = Convert.ToInt32(reader["MenuId"]),
                        Menu = reader["Menu"].ToString()
                    };

                    mealDict[mealId].Menus.Add(menu);
                }
            }

            reader.Close();
            connection.Close();

            meals = mealDict.Values.ToList();
            return meals;
        }

        public MealViewModel GetMealById(int mealId)
        {
            MealViewModel meal = new MealViewModel();
            SqlCommand cmd = new SqlCommand("sp_MealMenuManagement", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", "GetMealById");
            cmd.Parameters.AddWithValue("@MealId", mealId);

            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                meal.MealId = Convert.ToInt32(reader["MealId"]);
                meal.Day = reader["Day"].ToString();
                meal.StartTime = DateTime.Today.Add((TimeSpan)reader["StartTime"]).ToString("hh:mm tt");
                meal.EndTime = DateTime.Today.Add((TimeSpan)reader["EndTime"]).ToString("hh:mm tt");
            }
            connection.Close();


            meal.Menus = new List<MenuViewModel>();
            SqlCommand cmd2 = new SqlCommand("sp_MealMenuManagement", connection);
            cmd2.CommandType = CommandType.StoredProcedure;
            cmd2.Parameters.AddWithValue("@Action", "GetMenusByMealId");
            cmd2.Parameters.AddWithValue("@MealId", mealId);

            connection.Open();
            SqlDataReader rdr = cmd2.ExecuteReader();
            while (rdr.Read())
            {
                meal.Menus.Add(new MenuViewModel
                {
                    Menu = rdr["Menu"].ToString()
                });
            }
            connection.Close();

            return meal;
        }


        public bool DeleteMealSchedule(int mealId)
        {
            SqlCommand cmd = new SqlCommand("sp_MealMenuManagement", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", "DeleteMealSchedule");
            cmd.Parameters.AddWithValue("@MealId", mealId);

            connection.Open();
            int result = cmd.ExecuteNonQuery();
            connection.Close();

            return result > 0;
        }

        public bool UpdateMeal(MealUpdateRequest request)
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                if (request != null)
                {

                    using (SqlCommand deleteCmd = new SqlCommand("sp_MealMenuManagement", connection, transaction))
                    {
                        deleteCmd.CommandType = CommandType.StoredProcedure;
                        deleteCmd.Parameters.AddWithValue("@Action", "DeleteMenusByDay");
                        deleteCmd.Parameters.AddWithValue("@Day", request.Meals[0].Day);
                        deleteCmd.Parameters.AddWithValue("@UserId", request.userid);

                        int res = deleteCmd.ExecuteNonQuery();
                        if (res <= 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    foreach (var item in request.Meals)
                    {
                        object res = string.Empty;
                        using (SqlCommand cmd = new SqlCommand("sp_MealMenuManagement", connection, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Action", item.MealId > 0 ? "UpdateMeal" : "InsertMeal");
                            cmd.Parameters.AddWithValue("@mealId", item.MealId);
                            cmd.Parameters.AddWithValue("@userid", request.userid);
                            cmd.Parameters.AddWithValue("@day", item.Day);
                            cmd.Parameters.AddWithValue("@StartTime", item.StartTime);
                            cmd.Parameters.AddWithValue("@EndTime", item.EndTime);

                            res = cmd.ExecuteScalar();
                            if (string.IsNullOrEmpty(res?.ToString()))
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }
                        item.MealId = Convert.ToInt32(res);


                        if (item.Menus != null && item.Menus.Count > 0)
                        {
                            foreach (var menu in item.Menus)
                            {
                                using (SqlCommand insertCmd = new SqlCommand("sp_MealMenuManagement", connection, transaction))
                                {
                                    insertCmd.CommandType = CommandType.StoredProcedure;
                                    insertCmd.Parameters.AddWithValue("@Action", menu.MenuId > 0 ? "updateMenu" : "InsertMenu");
                                    insertCmd.Parameters.AddWithValue("@menuId", menu.MenuId);
                                    insertCmd.Parameters.AddWithValue("@mealId", item.MealId);
                                    insertCmd.Parameters.AddWithValue("@Menu", menu.Menu);

                                    int res2 = insertCmd.ExecuteNonQuery();
                                    if (res2 <= 0)
                                    {
                                        transaction.Rollback();
                                        return false;
                                    }
                                }
                            }
                        }

                    }
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        public MealViewModel GetMealScheduleById(int mealId, int userid)
        {
            MealViewModel meal = null;

            SqlCommand cmd = new SqlCommand("sp_MealMenuManagement", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", "GetMealScheduleById");
            cmd.Parameters.AddWithValue("@MealId", mealId);
            cmd.Parameters.AddWithValue("@userid", userid);

            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                if (meal == null)
                {
                    meal = new MealViewModel
                    {
                        MealId = Convert.ToInt32(reader["MealId"]),
                        Day = reader["Day"].ToString(),
                        StartTime = DateTime.Today.Add((TimeSpan)reader["StartTime"]).ToString("hh:mm tt"),
                        EndTime = DateTime.Today.Add((TimeSpan)reader["EndTime"]).ToString("hh:mm tt"),
                        Menus = new List<MenuViewModel>()
                    };
                }

                if (reader["Id"] != DBNull.Value)
                {
                    var menu = new MenuViewModel
                    {
                        MenuId = Convert.ToInt32(reader["Id"]),
                        Menu = reader["Menu"].ToString()
                    };

                    meal.Menus.Add(menu);
                }
            }

            reader.Close();
            connection.Close();

            return meal;
        }

        public List<MealViewModel> GetMealsByDay(string day, int userid)
        {
            List<MealViewModel> meals = new List<MealViewModel>();

            SqlCommand cmd = new SqlCommand("sp_MealMenuManagement", connection);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "GetMealsByDay");
            cmd.Parameters.AddWithValue("@Day", day);
            cmd.Parameters.AddWithValue("@userid", userid);

            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                meals.Add(new MealViewModel
                {
                    MealId = Convert.ToInt32(reader["MealId"]),
                    Day = reader["Day"].ToString(),
                    StartTime = DateTime.Today.Add((TimeSpan)reader["StartTime"]).ToString("hh:mm tt"),
                    EndTime = DateTime.Today.Add((TimeSpan)reader["EndTime"]).ToString("hh:mm tt"),
                });
            }

            connection.Close();


            return meals;
        }
        public List<MealViewModel> GetMenusByDay(string day, int userid)
        {
            List<MealViewModel> menus = new List<MealViewModel>();

            SqlCommand cmd = new SqlCommand("sp_MealMenuManagement", connection);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "GetMenusByDay");
            cmd.Parameters.AddWithValue("@Day", day);
            cmd.Parameters.AddWithValue("@userid", userid);

            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                menus.Add(new MealViewModel
                {
                    MealId = Convert.ToInt32(reader["MealId"]),
                    Day = reader["day"].ToString(),
                    StartTime = reader["StartTime"]?.ToString(),
                    EndTime = reader["endTime"]?.ToString(),
                    Menus = JsonConvert.DeserializeObject<List<MenuViewModel>>(reader["menus"]?.ToString())

                });
            }

            connection.Close();


            return menus;
        }

        public List<MealViewModel> GetMealDetailsByDayAndUser(string day, int userid,string search=null)
        {
            Dictionary<int, MealViewModel> mealDict = new Dictionary<int, MealViewModel>();

            SqlCommand cmd = new SqlCommand("sp_MealMenuManagement", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "GetMealsByDayAndUser");
            cmd.Parameters.AddWithValue("@Day", day);
            cmd.Parameters.AddWithValue("@userid", userid);
            cmd.Parameters.AddWithValue("@search", search??null);

            connection.Open();
            SqlDataReader rdr = cmd.ExecuteReader();


            while (rdr.Read())
            {
                int mealId = Convert.ToInt32(rdr["MealId"]);
                if (!mealDict.ContainsKey(mealId))
                {
                    var meal = new MealViewModel
                    {
                        Id = Convert.ToInt32(rdr["MealId"]),
                        MealId = mealId,
                        UserId = Convert.ToInt32(rdr["UserId"]),
                        Day = rdr["Day"].ToString(),
                        StartTime = rdr["StartTime"].ToString(),
                        EndTime = rdr["EndTime"].ToString(),
                        Menus = new List<MenuViewModel>()
                    };
                    mealDict.Add(mealId, meal);
                }

                mealDict[mealId].Menus.Add(new MenuViewModel
                {
                    MenuId = Convert.ToInt32(rdr["Id"]),
                    MealId = mealId,
                    Menu = rdr["Menu"].ToString()
                });
            }

            connection.Close();
            return mealDict.Values.ToList();
        }

        public bool InsertCommunication(CommunicationModel model, out string errorMessage, int userid)
        {
            int result = 0;
            try
            {
                errorMessage = "";
                string attachmentPath = string.Empty;
                if (model.Attachments != null)
                {
                    attachmentPath = UploadImageToServer(model.Attachments);
                }
                SqlCommand command = new SqlCommand("sp_InsertCommunication", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "InsertCommunication");
                command.Parameters.AddWithValue("@userid", userid);
                command.Parameters.AddWithValue("@Title", model.Title);
                command.Parameters.AddWithValue("@Description", model.Description);
                if (model.Attachments != null && model.Attachments.ContentLength > 0)
                {
                    command.Parameters.AddWithValue("@Attachment", attachmentPath);
                }
                connection.Open();
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
                connection.Close();
            }

        }

        public List<CommunicationModel> GetAllCommunication(int userid,string search=null)
        {
            List<CommunicationModel> list = new List<CommunicationModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_InsertCommunication", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GetAllCommunication");
                cmd.Parameters.AddWithValue("@userId", userid);
                cmd.Parameters.AddWithValue("@search", search??null);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    CommunicationModel communication = new CommunicationModel
                    {
                        CommunicationId = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0,
                        Title = rdr["Title"]?.ToString(),
                        Attachment = rdr["Attachment"]?.ToString(),
                        Description = rdr["Description"]?.ToString()
                    };

                    list.Add(communication);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }

            return list;
        }

        public bool DeleteCommunicationById(int communicationId)
        {
            bool isDeleted = false;
            try
            {
                SqlCommand cmd = new SqlCommand("sp_InsertCommunication", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DeleteCommunication");
                cmd.Parameters.AddWithValue("@Id", communicationId);

                connection.Open();
                int rows = cmd.ExecuteNonQuery();
                isDeleted = rows > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return isDeleted;
        }

        public CommunicationModel GetCommunicationById(int communicationId)
        {
            CommunicationModel model = null;

            try
            {
                SqlCommand cmd = new SqlCommand("sp_InsertCommunication", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GetCommunicationById");
                cmd.Parameters.AddWithValue("@Id", communicationId);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    model = new CommunicationModel
                    {
                        CommunicationId = Convert.ToInt32(rdr["Id"]),
                        Title = rdr["Title"]?.ToString(),
                        Description = rdr["Description"]?.ToString(),
                        Attachment = rdr["Attachment"]?.ToString()
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

            return model;
        }
        public bool UpdateCommunication(CommunicationModel model, out string errorMessage)
        {
            errorMessage = string.Empty;
            int result = 0;
            try
            {
                string attachmentPath = string.Empty;
                if (model.Attachments != null && model.Attachments.ContentLength > 0)
                {
                    attachmentPath = UploadImageToServer(model.Attachments);
                }

                using (SqlCommand cmd = new SqlCommand("sp_InsertCommunication", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "UpdateCommunication");
                    cmd.Parameters.AddWithValue("@Id", model.CommunicationId);
                    cmd.Parameters.AddWithValue("@Title", model.Title);
                    cmd.Parameters.AddWithValue("@Description", model.Description);

                    if (!string.IsNullOrEmpty(attachmentPath))
                    {
                        cmd.Parameters.AddWithValue("@Attachment", attachmentPath);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Attachment", DBNull.Value);
                    }

                    connection.Open();
                    result = cmd.ExecuteNonQuery();
                }

                if (result <= 0)
                {
                    errorMessage = "Something went wrong while updating the communication.";
                    return false;
                }

                return true;
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
            }
        }

        public List<UserOrdersModel> SeletUserForRoomAllocation(string userNo, int userId)
        {
            List<UserOrdersModel> list = new List<UserOrdersModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectstudentdetails");
                cmd.Parameters.AddWithValue("@userNo", userNo);
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        list.Add(new UserOrdersModel
                        {
                            studentName = rd["studentname"].ToString(),
                            studentid = rd["id"] == DBNull.Value ? 0 : Convert.ToInt32(rd["id"]),
                            totalFee = rd["totalfee"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["totalfee"]),
                            emailId = rd["email"].ToString(),
                            roomNo = rd["roomnumber"] == DBNull.Value ? 0 : Convert.ToInt32(rd["roomnumber"]),
                            remainingFee = rd["remainingFee"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["remainingFee"]),
                            hostelId = rd["HostelId"] == DBNull.Value ? 0 : Convert.ToInt32(rd["HostelId"]),
                            blockId = rd["BlockId"] == DBNull.Value ? 0 : Convert.ToInt32(rd["BlockId"])
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
                connection.Close();
            }
        }
        public List<UserOrdersModel> GetDetailForRoomAllocation(int studentid)
        {
            List<UserOrdersModel> list = new List<UserOrdersModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentInHostel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectstudentdetailforrellocate");
                cmd.Parameters.AddWithValue("@studentid", studentid);
                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        list.Add(new UserOrdersModel
                        {
                            studentid = Convert.ToInt32(rd["StudentId"]),
                            roomNo = Convert.ToInt32(rd["RoomId"]),
                            studentName = rd["StudentName"].ToString(),
                            floor = rd["floor"] != DBNull.Value ? Convert.ToInt32(rd["floor"]) : -1,
                            feeType = rd["FeeType"].ToString(),
                            remainingFee = rd["remainingFee"] != DBNull.Value ? Convert.ToDecimal(rd["remainingFee"]) : 0,
                            dueDateString = rd["DueDate"] != DBNull.Value ? Convert.ToDateTime(rd["DueDate"]).ToString("yyyy-MM-dd") : null,
                            feeSlip = rd["FeeSlip"].ToString(),
                            feeSubmitted = rd["FeesSubmitted"] != DBNull.Value ? Convert.ToDecimal(rd["FeesSubmitted"]) : 0,
                            BlockId = Convert.ToInt32(rd["blockid"]),
                            transactionid = rd["transactionid"].ToString(),
                            totalFee = rd["feesperperson"] != DBNull.Value ? Convert.ToDecimal(rd["feesperperson"]) : 0,
                            emailId = rd["studentemail"].ToString(),
                            roomnumber = rd["roomnumber"] != DBNull.Value ? Convert.ToInt32(rd["roomnumber"]) : 0,
                            hostelId = rd["HostelId"] != DBNull.Value ? Convert.ToInt32(rd["hostelid"]) : 0
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
                connection.Close();
            }
        }

        public List<UserOrdersModel> SelectUsersByRoomNo(int roomNo, int userId)
        {
            List<UserOrdersModel> list = new List<UserOrdersModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentInHostel", connection);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GetStudentsByRoomNo");
                cmd.Parameters.AddWithValue("@RoomNo", roomNo);
                cmd.Parameters.AddWithValue("@userid", userId);

                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        list.Add(new UserOrdersModel
                        {

                            userName = rd["name"].ToString(),
                            emailId = rd["email"] == DBNull.Value ? "" : rd["email"].ToString(),
                            blockId = rd["blockId"] == DBNull.Value ? 0 : Convert.ToInt32(rd["blockId"]),
                            hostelId = rd["hostelId"] == DBNull.Value ? 0 : Convert.ToInt32(rd["hostelId"]),
                            studentid = rd["id"] == DBNull.Value ? 0 : Convert.ToInt32(rd["id"]),
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public RoomInfo GetRoomInfo(int roomId, int userid)
        {
            RoomInfo roomInfo = null;

            try
            {

                SqlCommand command = new SqlCommand("sp_ManageStudentInHostel", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Action", "GetRoomInfo");
                command.Parameters.AddWithValue("@RoomId", roomId);
                command.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        roomInfo = new RoomInfo
                        {
                            RoomId = Convert.ToInt32(reader["RoomId"]),
                            RoomNumber = Convert.ToInt32(reader["RoomNumber"]),
                            TotalBeds = Convert.ToInt32(reader["TotalBeds"]),
                            OccupiedBeds = Convert.ToInt32(reader["OccupiedBeds"]),
                            AvailableBeds = Convert.ToInt32(reader["AvailableBeds"]),
                            RoomFacilitate = reader["RoomFacilitate"].ToString(),
                            TotalFees = Convert.ToInt32(reader["FeesPerPerson"]),
                        };
                    }
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

            return roomInfo;
        }
        public RoomInfo GetRoomInfoByRoomNo(int roomno, int userid, int wardenid)
        {
            RoomInfo roomInfo = null;

            try
            {

                SqlCommand command = new SqlCommand("sp_ManageStudentInHostel", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Action", "GetRoomInfoByRoomNo");
                command.Parameters.AddWithValue("@RoomNo", roomno);
                command.Parameters.AddWithValue("@userid", userid);
                command.Parameters.AddWithValue("@wardenid", wardenid);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        roomInfo = new RoomInfo
                        {
                            RoomId = Convert.ToInt32(reader["RoomId"]),
                            RoomNumber = Convert.ToInt32(reader["RoomNumber"]),
                            TotalBeds = Convert.ToInt32(reader["TotalBeds"]),
                            OccupiedBeds = Convert.ToInt32(reader["OccupiedBeds"]),
                            AvailableBeds = Convert.ToInt32(reader["AvailableBeds"]),
                            RoomFacilitate = reader["RoomFacilitate"].ToString(),
                            TotalFees = Convert.ToInt32(reader["FeesPerPerson"]),
                            students = JsonConvert.DeserializeObject<List<CommonModel>>(reader["students"].ToString())
                        };
                    }
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

            return roomInfo;
        }
        public List<RoomModel> GetRoomsByBlockId(int blockId, int userid,string search=null)
        {
            List<RoomModel> rooms = new List<RoomModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentInHostel", connection);
                cmd.Parameters.AddWithValue("@Action", "SELECTROOMBYBLOCKID");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BlockId", blockId);
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@search", search??null);

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    rooms.Add(new RoomModel
                    {
                        RoomId = Convert.ToInt32(dr["RoomId"]),
                        RoomNo = Convert.ToInt32(dr["RoomNumber"]),
                        TotalBeds = Convert.ToInt32(dr["TotalBeds"]),
                        OccupiedBeds = Convert.ToInt32(dr["OccupiedBeds"]),
                        AvailableBeds = Convert.ToInt32(dr["AvailableBeds"]),
                        FeesPerPerson = Convert.ToInt32(dr["FeesPerPerson"]),
                        RoomFacilitate = dr["RoomFacilitate"].ToString(),
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

            return rooms;
        }
        public List<RoomModel> GetRoomsByFloor(int floorno, int userid,string search =null)
        {
            List<RoomModel> rooms = new List<RoomModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentInHostel", connection);
                cmd.Parameters.AddWithValue("@Action", "SELECTROOMBYfloor");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@floorno", floorno);
                cmd.Parameters.AddWithValue("@wardenid", userid);
                cmd.Parameters.AddWithValue("@search", search??null);

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    rooms.Add(new RoomModel
                    {
                        RoomId = Convert.ToInt32(dr["RoomId"]),
                        RoomNo = Convert.ToInt32(dr["RoomNumber"]),
                        NoOfBeds = Convert.ToInt32(dr["BedCount"]),
                        RoomFacilitate = dr["RoomFacilitate"].ToString(),
                        AvailableBeds = dr["AvailableBeds"] != DBNull.Value ? Convert.ToInt32(dr["AvailableBeds"]) : 0
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

            return rooms;
        }

        public bool InsertReallocatedRoom(ReallocateRoomModel model, out string errorMessage)
        {
            int result = 0;
            errorMessage = string.Empty;

            try
            {
                string attachmentPath = string.Empty;

                if (model.FeeSlip != null && model.FeeSlip.ContentLength > 0)
                {
                    attachmentPath = UploadImageToServer(model.FeeSlip);
                }

                using (SqlCommand command = new SqlCommand("sp_InsertReallocatedRoom", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Action", "ReallocateRoom");
                    command.Parameters.AddWithValue("@RoomId", model.RoomId);
                    command.Parameters.AddWithValue("@StudentId", model.StudentId);
                    command.Parameters.AddWithValue("@RemainingFees", model.RemainingFees);
                    connection.Open();
                    result = command.ExecuteNonQuery();
                }
                if (result <= 0)
                {
                    errorMessage = "Something went wrong during room reallocation.";
                    return false;
                }
                return true;
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
            }
        }
        public List<NoticeModel> GetWardenNotices(string userType, int userId, int adminid,string search=null)
        {
            var notices = new List<NoticeModel>();

            SqlCommand cmd = new SqlCommand("sp_NoticeManagement", connection);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "GetNoticeByUsers");
            cmd.Parameters.AddWithValue("@UserType", userType);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@adminid", adminid);
            cmd.Parameters.AddWithValue("@search", search ?? null);

            connection.Open();
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

        public bool InsertFeesRecord(FeeRecordsModel model, out string errorMessage)
        {
            int result = 0;
            errorMessage = string.Empty;

            try
            {
                string attachmentPath = string.Empty;

                if (model.FeeSlip != null && model.FeeSlip.ContentLength > 0)
                {
                    attachmentPath = UploadImageToServer(model.FeeSlip);
                }

                SqlCommand command = new SqlCommand("sp_HostelStudentFeeRecordManagement", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "Insert");
                command.Parameters.AddWithValue("@FeesType", model.FeeType ?? string.Empty);
                command.Parameters.AddWithValue("@FeesSubmitted", model.FeesSubmitted);
                command.Parameters.AddWithValue("@StudentHostelId", model.hostelId);
                command.Parameters.AddWithValue("@CreatedOn", model.PaymentDate);
                command.Parameters.AddWithValue("@RemainingFee", model.RemainingFee);
                command.Parameters.AddWithValue("@transactionid", model.transactionid);
                command.Parameters.AddWithValue("@DueDate", model.DueDate >= DateTime.Now.Date ? model.DueDate : (object)DBNull.Value);
                if (!string.IsNullOrEmpty(attachmentPath))
                {
                    command.Parameters.AddWithValue("@FeeSlip", attachmentPath);
                }
                else
                {
                    command.Parameters.AddWithValue("@FeeSlip", DBNull.Value);
                }

                connection.Open();
                result = command.ExecuteNonQuery();


                if (result <= 0)
                {
                    errorMessage = "Something went wrong.";
                    return false;
                }

                return true;
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
            }
        }
        public List<FeeRecordsModel> GetAllFeeRecords(int userid)
        {
            List<FeeRecordsModel> feeRecords = new List<FeeRecordsModel>();

            SqlCommand cmd = new SqlCommand("sp_HostelStudentFeeRecordManagement", connection);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "SelectAll");
            cmd.Parameters.AddWithValue("@userid", userid);
            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                FeeRecordsModel record = new FeeRecordsModel
                {
                    id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : (int?)null,
                    FeeType = reader["FeeType"] != DBNull.Value ? reader["FeeType"].ToString() : null,
                    FeeSlipPath = reader["FeeSlip"] != DBNull.Value ? reader["FeeSlip"].ToString() : null,
                    FeesSubmitted = reader["FeesSubmitted"] != DBNull.Value ? Convert.ToInt32(reader["FeesSubmitted"]) : (int?)null,
                    PaymentDate = reader["CreatedOn"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedOn"]).Date : (DateTime?)null,

                    StudentId = reader["StudentId"] != DBNull.Value ? Convert.ToInt32(reader["StudentId"]) : (int?)null,
                    StudentName = reader["StudentName"] != DBNull.Value ? reader["StudentName"].ToString() : null,

                };
                feeRecords.Add(record);
            }
            return feeRecords;
        }
        public List<FeeRecordsModel> GetLastFeeRecords(int userid,string search=null)
        {
            try
            {
                List<FeeRecordsModel> feeRecords = new List<FeeRecordsModel>();

                SqlCommand cmd = new SqlCommand("sp_HostelStudentFeeRecordManagement", connection);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectlastpaymentofall");
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@search", search??null);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    FeeRecordsModel record = new FeeRecordsModel
                    {
                        id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                        studentHostelid = reader["HostelId"] != DBNull.Value ? Convert.ToInt32(reader["HostelId"]) : 0,
                        LastPaymentDate = reader["lastpaydate"] != DBNull.Value ? Convert.ToDateTime(reader["lastpaydate"]).Date : (DateTime?)null,
                        StudentId = reader["StudentId"] != DBNull.Value ? Convert.ToInt32(reader["StudentId"]) : (int?)null,
                        StudentName = reader["studentname"] != DBNull.Value ? reader["studentname"].ToString() : null,
                        RoomNumber = reader["roomnumber"] != DBNull.Value ? Convert.ToInt32(reader["roomnumber"]) : 0,
                        TotalFee = reader["feesperperson"] != DBNull.Value ? Convert.ToDecimal(reader["feesperperson"]) : 0,
                        RemainingFee = reader["remainingFee"] != DBNull.Value ? Convert.ToDecimal(reader["remainingFee"]) : 0,
                        TotalFeeSubmitted = reader["totalFeeSubmitted"] != DBNull.Value ? Convert.ToDecimal(reader["totalFeeSubmitted"]) : 0
                    };
                    feeRecords.Add(record);
                }
                return feeRecords;
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
        public List<FeeRecordsModel> GetFeeHistoryOfStudent(int studenthostelid,string search=null)
        {
            try
            {
                List<FeeRecordsModel> feeRecords = new List<FeeRecordsModel>();

                SqlCommand cmd = new SqlCommand("sp_HostelStudentFeeRecordManagement", connection);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectfeehistoryofstudent");
                cmd.Parameters.AddWithValue("@StudentHostelId", studenthostelid);
                cmd.Parameters.AddWithValue("@search", search);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    FeeRecordsModel record = new FeeRecordsModel
                    {
                        StudentName = reader["studentname"] != DBNull.Value ? reader["studentname"].ToString() : null,
                        RoomNumber = reader["roomnumber"] != DBNull.Value ? Convert.ToInt32(reader["roomnumber"]) : 0,
                        TotalFee = reader["feesperperson"] != DBNull.Value ? Convert.ToDecimal(reader["feesperperson"]) : 0,
                        RemainingFee = reader["RemainingFees"] != DBNull.Value ? Convert.ToDecimal(reader["RemainingFees"]) : 0,
                        FeeType = reader["FeeType"].ToString(),
                        FeesSubmitted = reader["FeesSubmitted"] != DBNull.Value ? Convert.ToInt32(reader["FeesSubmitted"]) : 0,
                        FeeSlipPath = reader["FeeSlip"].ToString(),
                        DueDateString = reader["DueDate"] != DBNull.Value ? Convert.ToDateTime(reader["DueDate"]).ToString("dd-MMM-yyyy") : string.Empty,
                        transactionid = reader["transactionid"].ToString()
                    };
                    feeRecords.Add(record);
                }
                return feeRecords;
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
        public FeeRecordsModel GetFeeRecordById(int id)
        {
            FeeRecordsModel record = null;

            using (SqlCommand cmd = new SqlCommand("sp_HostelStudentFeeRecordManagement", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SelectById");
                cmd.Parameters.AddWithValue("@id", id);

                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        record = new FeeRecordsModel
                        {
                            id = Convert.ToInt32(reader["id"]),
                            hostelId = Convert.ToInt32(reader["StudentHostelId"]),
                            RoomNumber = Convert.ToInt32(reader["RoomNumber"]),
                            FeeType = reader["FeeType"]?.ToString(),
                            FeeSlipPath = reader["FeeSlip"]?.ToString(),
                            FeesSubmitted = reader["FeesSubmitted"] != DBNull.Value ? Convert.ToInt32(reader["FeesSubmitted"]) : (int?)null,
                            PaymentDate = reader["CreatedOn"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedOn"]) : (DateTime?)null,
                            StudentId = reader["StudentId"] != DBNull.Value ? Convert.ToInt32(reader["StudentId"]) : (int?)null,
                            StudentName = reader["StudentName"]?.ToString(),
                            AdmissionNo = reader["AdmissionNo"]?.ToString(),
                            Email = reader["StudentEmail"]?.ToString(),
                            Address = reader["Address"]?.ToString(),
                            RemainingFee = reader["remainingfee"] != DBNull.Value ? Convert.ToDecimal(reader["remainingfee"]) : 0,
                            transactionid = reader["transactionid"].ToString(),
                            DueDateString = Convert.ToDateTime(reader["CreatedOn"]).ToString("yyyy-MM-dd"),
                            TotalFee = reader["totalfee"] != DBNull.Value ? Convert.ToInt32(reader["totalfee"]) : 0
                        };
                    }
                }
            }

            return record;
        }

        public bool UpdateFeeRecord(FeeRecordsModel model, out string errorMessage)
        {
            int result = 0;
            errorMessage = string.Empty;

            try
            {
                string attachmentPath = string.Empty;

                if (model.FeeSlip != null && model.FeeSlip.ContentLength > 0)
                {
                    attachmentPath = UploadImageToServer(model.FeeSlip);
                }

                SqlCommand command = new SqlCommand("sp_HostelStudentFeeRecordManagement", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "Update");
                command.Parameters.AddWithValue("@Id", model.id);
                command.Parameters.AddWithValue("@FeesType", model.FeeType ?? string.Empty);
                command.Parameters.AddWithValue("@FeesSubmitted", model.FeesSubmitted);
                command.Parameters.AddWithValue("@StudentHostelId", model.hostelId);
                command.Parameters.AddWithValue("@CreatedOn", model.PaymentDate);
                if (!string.IsNullOrEmpty(attachmentPath))
                {
                    command.Parameters.AddWithValue("@FeeSlip", attachmentPath);
                }
                else
                {
                    command.Parameters.AddWithValue("@FeeSlip", DBNull.Value);
                }

                connection.Open();
                result = command.ExecuteNonQuery();


                if (result <= 0)
                {
                    errorMessage = "Something went wrong during updating fees record.";
                    return false;
                }

                return true;
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
            }
        }

        public WardenDashboardModel GetWardenDashboard(int userId, int wardenId)
        {
            WardenDashboardModel model = new WardenDashboardModel();

            using (SqlCommand cmd = new SqlCommand("sp_ManageDashboard", connection))

            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userid", userId);
                cmd.Parameters.AddWithValue("@wardenid", wardenId);

                cmd.Parameters.AddWithValue("@action", "WardenDashboard");

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    model.totalroom = dr["totalroom"] != DBNull.Value ? Convert.ToInt32(dr["totalroom"]) : (int?)null;
                    model.totalbeds = dr["totalbeds"] != DBNull.Value ? Convert.ToInt32(dr["totalbeds"]) : (int?)null;
                    model.occupiedbeds = dr["occupiedbeds"] != DBNull.Value ? Convert.ToInt32(dr["occupiedbeds"]) : (int?)null;
                    model.remainingbeds = dr["remainingbeds"] != DBNull.Value ? Convert.ToInt32(dr["remainingbeds"]) : (int?)null;
                    model.nonACRoomNonAttachedBathroom = dr["nonACRoomNonAttachedBathroom"] != DBNull.Value ? Convert.ToInt32(dr["nonACRoomNonAttachedBathroom"]) : (int?)null;
                    model.ACRoomNonAttachedBathroom = dr["ACRoomNonAttachedBathroom"] != DBNull.Value ? Convert.ToInt32(dr["ACRoomNonAttachedBathroom"]) : (int?)null;
                    model.nonACRoomAttachedBathroom = dr["nonACRoomAttachedBathroom"] != DBNull.Value ? Convert.ToInt32(dr["nonACRoomAttachedBathroom"]) : (int?)null;
                    model.ACRoomAttachedBathroom = dr["ACRoomAttachedBathroom"] != DBNull.Value ? Convert.ToInt32(dr["ACRoomAttachedBathroom"]) : (int?)null;
                    model.communication = dr["communication"] != DBNull.Value ? Convert.ToInt32(dr["communication"]) : (int?)null;

                }

                connection.Close();
            }

            return model;
        }

        public WardenDashboardModel GetNoticeCount(int AdminId, int UserId)
        {
            WardenDashboardModel model = new WardenDashboardModel();

            using (SqlCommand cmd = new SqlCommand("sp_ManageDashboard", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AdminId", AdminId);
                cmd.Parameters.AddWithValue("@UserId", UserId);

                cmd.Parameters.AddWithValue("@action", "noticeCount");

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    model.notice = Convert.ToInt32(dr["notice"]);
                }

                connection.Close();
            }

            return model;
        }
        public List<HolidayModel> GetHolidaysForAll(int userId, string search = null)
        {
            List<HolidayModel> holidays = new List<HolidayModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_Holiday", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "HolidaysForAll");
                cmd.Parameters.AddWithValue("@userid", userId);
                cmd.Parameters.AddWithValue("@search", search);

                connection.Open();
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
                connection.Close();
            }

        }

        public List<HolidayModel> GetHolidaysTodayAndTomorrow(int userId, string search = null)
        {
            List<HolidayModel> holidays = new List<HolidayModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_Holiday", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "HolidaysTodayAndTomorrow");
                cmd.Parameters.AddWithValue("@userid", userId);
                cmd.Parameters.AddWithValue("@search", search??null);

                connection.Open();
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
                connection.Close();
            }
        }
        public List<int> GetFloorByWardenId(int wardenid)
        {
            try
            {
                List<int> floors = new List<int>();
                SqlCommand cmd = new SqlCommand("sp_ManageHostelBlock", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectallfloorofblock");
                cmd.Parameters.AddWithValue("@wardenid", wardenid);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    floors.Add(rdr["floor"] != DBNull.Value ? Convert.ToInt32(rdr["floor"]) : -1);
                }
                return floors;
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
        #region Hostel Problems Management
        public List<HostelProblemsModel> GetHostelProblem(int wardenid, string search = null)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageHostelProblem", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@wardenid", wardenid);
                cmd.Parameters.AddWithValue("@action", "selectforwarden");
                cmd.Parameters.AddWithValue("@search", search??null);
                List<HostelProblemsModel> list = new List<HostelProblemsModel>();
                connection.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new HostelProblemsModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            problem = res["problem"].ToString(),
                            problemStatus = Convert.ToInt32(res["problemStatus"]),
                            createdAt = Convert.ToDateTime(res["createdAt"]).ToString("dd-MMM-yyyy"),
                            blockName = res["blockname"].ToString(),
                            studentName = res["studentname"].ToString(),
                            roomNo = res["roomnumber"].ToString(),
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
        public bool CompleteRejectHostelProblem(int id, int status, string reason)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageHostelProblem", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "updateproblemstatus");
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@problemStatus", status);
                cmd.Parameters.AddWithValue("@reason", reason != null ? reason : null);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                return res > 0;
            }
            catch
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
        #region Block Details
        public List<BlockDetailModel> GetBlockDetails(int wardenid)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageBlockDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@wardenid", wardenid);
                cmd.Parameters.AddWithValue("@action", "selectblockdetails");
                List<BlockDetailModel> list = new List<BlockDetailModel>();
                connection.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new BlockDetailModel
                        {
                            floors = res["floors"] != DBNull.Value ? Convert.ToInt32(res["floors"]) : (int?)null,
                            rooms = res["rooms"] != DBNull.Value ? Convert.ToInt32(res["rooms"]) : (int?)null,

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
        public List<RoomListModel> GetRoomList(int wardenid,string search=null)
        {
            List<RoomListModel> list = new List<RoomListModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageBlockDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectroomlist");
                cmd.Parameters.AddWithValue("@wardenid", wardenid);
                cmd.Parameters.AddWithValue("@search", search??null);
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        list.Add(new RoomListModel
                        {
                            RoomId = rdr["RoomId"] != DBNull.Value ? Convert.ToInt32(rdr["RoomId"]) : 0,
                            RoomTypes = rdr["RoomTypes"].ToString(),
                            BedCount = rdr["BedCount"] != DBNull.Value ? Convert.ToInt32(rdr["BedCount"]) : 0,
                            RoomFacilitate = string.IsNullOrEmpty(rdr["RoomFacilitate"].ToString()) ? "Non Air Conditioner" : rdr["RoomFacilitate"].ToString(),
                            RoomNumber = Convert.ToInt32(rdr["RoomNumber"]),
                            FeesPerPerson = rdr["FeesPerPerson"] != DBNull.Value ? Convert.ToInt32(rdr["FeesPerPerson"]) : 0,
                            floor = rdr["floor"] != DBNull.Value ? Convert.ToInt32(rdr["floor"]) : 0,
                            totalfloors = rdr["totalfloors"] != DBNull.Value ? Convert.ToInt32(rdr["totalfloors"]) : -1,
                            OccupiedBeds = rdr["OccupiedBeds"] != DBNull.Value ? Convert.ToInt32(rdr["OccupiedBeds"]) : 0,
                            RemainingBeds = rdr["RemainingBeds"] != DBNull.Value ? Convert.ToInt32(rdr["RemainingBeds"]) : 0,
                            TotalFloors = rdr["TotalFloors"] != DBNull.Value ? Convert.ToInt32(rdr["TotalFloors"]) : 0,
                            StudentName = rdr["StudentNames"] != DBNull.Value ? rdr["StudentNames"].ToString() : "",

                        });
                    }
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

    }
}