using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.IO;
using College_ERP.Models.Admin;
using LCMIS.Server.Model;
using System.Reflection;
using static College_ERP.Models.Security.main;

namespace College_ERP.Models.SuperAdmin
{
	public class SuperAdminDataService
	{
        private readonly SqlConnection connection;
		private readonly College_ERP.Models.MailService.MailService _mail;
        public SuperAdminDataService()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
			_mail = new College_ERP.Models.MailService.MailService();
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

        public string CheckLoginCredential(string username,string password)
		{
			try
			{
				SqlCommand cmd = new SqlCommand("sp_loginmanager", connection);
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.AddWithValue("@action", "verifyCredential");
				cmd.Parameters.AddWithValue("@username", username);
				connection.Open();
				var res = cmd.ExecuteReader();
				if (res.HasRows)
				{
					while (res.Read())
					{
						if (res["username"]?.ToString() == username && res["password"]?.ToString() == password)
						{
							return res["role"]?.ToString();
						}
					}
				}
				return null;
			}catch(Exception ex)
			{
				return null;
			}
			finally
			{
				connection.Close();
			}
        }

		public bool InsertCompanyRegistration(SuperAdminModel model)
		{
            try
            {
				string schoollogopath = string.Empty;
				string authImagePath = string.Empty;

                if (model.School_Logo1 != null)
				{
					schoollogopath = UploadImageToServer(model.School_Logo1);
                }
				if (model.Authorized_Sign1 != null)
				{
					authImagePath = UploadImageToServer(model.Authorized_Sign1);
                }

				SqlCommand cmd = new SqlCommand("SP_CompanyRegistration", connection);
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.AddWithValue("@action", model.Id>0? "updateCompany" : "Insert");
				cmd.Parameters.AddWithValue("@Id", model.Id);
				cmd.Parameters.AddWithValue("@SchoolName", model.SchoolName);
				cmd.Parameters.AddWithValue("@AuthorizedPersonName", model.AuthorizedPersonName);
				cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
				cmd.Parameters.AddWithValue("@LandLineNo", model.LandLineNo);
				cmd.Parameters.AddWithValue("@EmailId", model.EmailId);
				cmd.Parameters.AddWithValue("@State", model.State);
				cmd.Parameters.AddWithValue("@City", model.City);
				cmd.Parameters.AddWithValue("@Website", model.Website);
				cmd.Parameters.AddWithValue("@School_Logo", schoollogopath);
				cmd.Parameters.AddWithValue("@Duration", model.Duration);
				cmd.Parameters.AddWithValue("@Authorized_Sign", authImagePath);
				cmd.Parameters.AddWithValue("@School_Address", model.School_Address);
            connection.Open();
				int res = cmd.ExecuteNonQuery();

                return res > 0;

            }
			catch (Exception ex)
			{
				return false;
			}
			finally
			{
				connection.Close();
			}
		}

		public SuperAdminModel GetCompanyById(int id)
		{
           SuperAdminModel list =new SuperAdminModel();
            SqlDataReader rdr = null;

            try
            {
                SqlCommand cmd = new SqlCommand("SP_CompanyRegistration", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "SelectCompanyByUserid");
                cmd.Parameters.AddWithValue("@userid", id);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                rdr = cmd.ExecuteReader();

                if (rdr.HasRows)
                {

                    while (rdr.Read())
                    {
                        list = new SuperAdminModel();

                        list.Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0;
                        list.SchoolName = rdr["SchoolName"]?.ToString();
                        list.AuthorizedPersonName = rdr["AuthorizedPersonName"]?.ToString();
                        list.MobileNo = rdr["MobileNo"] != DBNull.Value ? Convert.ToInt64(rdr["MobileNo"]) : (long?)null;
                        list.LandLineNo = rdr["LandLineNo"] != DBNull.Value ? Convert.ToInt64(rdr["LandLineNo"]) : (long?)null;
                        list.EmailId = rdr["EmailId"]?.ToString();
                        list.stateName = rdr["stateName"]?.ToString();
                        list.State = Convert.ToInt32(rdr["state"] ?? 0);
                        list.City = Convert.ToInt32(rdr["city"] ?? 0);
                        list.City_Name = rdr["City_Name"]?.ToString();
                        list.Website = rdr["Website"]?.ToString();
                        list.School_Logo = rdr["School_Logo"]?.ToString();
                        list.Duration = rdr["Duration"]?.ToString();
                        list.Authorized_Sign = rdr["Authorized_Sign"]?.ToString();
                        list.School_Address = rdr["School_Address"]?.ToString();

                    }
                }
            return list;
            }
            catch (Exception ex)
            {
                return list;
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (connection.State == ConnectionState.Open) connection.Close();
            }

        }
		public List<SuperAdminModel> GetAllCompanyReg()
		{
			List<SuperAdminModel> list = new List<SuperAdminModel>();
			SqlDataReader rdr = null;

			try
			{
				SqlCommand cmd = new SqlCommand("SP_CompanyRegistration", connection);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@action", "selectAllCompanyReg");

				connection.Open();
				rdr = cmd.ExecuteReader();

				while (rdr.Read())
				{
					SuperAdminModel model = new SuperAdminModel();

					model.Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0;
					model.SchoolName = rdr["SchoolName"]?.ToString();
					model.AuthorizedPersonName = rdr["AuthorizedPersonName"]?.ToString();
					model.MobileNo = rdr["MobileNo"] != DBNull.Value ? Convert.ToInt64(rdr["MobileNo"]) : (long?)null;
					model.LandLineNo = rdr["LandLineNo"] != DBNull.Value ? Convert.ToInt64(rdr["LandLineNo"]) : (long?)null;
					model.EmailId = rdr["EmailId"]?.ToString();
					model.stateName = rdr["stateName"]?.ToString();
					model.City_Name = rdr["City_Name"]?.ToString();
					model.Website = rdr["Website"]?.ToString();
					model.School_Logo = rdr["School_Logo"]?.ToString();
					model.Duration = rdr["Duration"]?.ToString();
					model.Authorized_Sign = rdr["Authorized_Sign"]?.ToString();
					model.School_Address = rdr["School_Address"]?.ToString();
					list.Add(model);
				}
			}
			catch (Exception ex)
			{
				
				Console.WriteLine("Error: " + ex.Message);
			}
			finally
			{
				if (rdr != null) rdr.Close();
				if (connection.State == ConnectionState.Open) connection.Close();
			}

			return list;
		}


		public string deleteCompanyReg(int Id)
		{
			string result = "";

			try
			{
				SqlCommand cmd = new SqlCommand("SP_CompanyRegistration", connection);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@action", "deleteCompanyReg");
				cmd.Parameters.AddWithValue("@Id", Id);

				connection.Open();
				cmd.ExecuteNonQuery();
				result = "Success";
			}
			catch (Exception ex)
			{
				result = "Error: " + ex.Message;
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}

			return result;
		}


        public bool InsertAdmin(CreateAdmin model)
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            string uploadPath = string.Empty;
            try
            {
                if (model.Image != null)
                {
                    
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
                    string fileExtension = Path.GetExtension(model.Image.FileName)?.ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return false; 
                    }

                    uploadPath = UploadImageToServer(model.Image);
                    if (uploadPath == null)
                    {
                        return false;
                    }
                }

                SqlCommand cmd = new SqlCommand("sp_CreateAdmin", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "Insert");
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
                cmd.Parameters.AddWithValue("@EmailId", model.EmailId);
                cmd.Parameters.AddWithValue("@CompanyId", model.CompanyId);
                cmd.Parameters.AddWithValue("@Image", model.Image != null ? uploadPath : (object)DBNull.Value);

                int id = 0;
                string eres = cmd.ExecuteScalar()?.ToString();
                bool parseres = int.TryParse(eres, out id);
                if (parseres) id = Convert.ToInt32(eres);

                if (id > 0)
                {
                    string schoolPart = model.Name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    schoolPart = schoolPart.Length >= 6 ? schoolPart.Substring(0, 6) : schoolPart.PadRight(6, 'x');

                    string mobilePart = model.MobileNo.ToString();
                    mobilePart = mobilePart.Length >= 10 ? mobilePart.Substring(6) : mobilePart.PadLeft(10, '0').Substring(6);

                    string username = $"{schoolPart}@{mobilePart}";
                    string randomCharacter = "ABCDEFGHIJKLMNOPQRSTUVWabcdefghijklmnopqrst1234567890";
                    Random rmd = new Random();
                    string password = string.Empty;
                    for (int i = 0; i < 6; i++)
                    {
                        int rcount = rmd.Next(randomCharacter.Length - 1);
                        password += randomCharacter[rcount];
                    }

                    cmd = new SqlCommand("sp_loginmanager", connection, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "insertlogincredential");
                    cmd.Parameters.AddWithValue("@userId", id);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@role", "admin");

                    int res = cmd.ExecuteNonQuery();
                    if (res > 0)
                    {
                        string subject = "Login Credential";
                        string body = $" <p>We are pleased to inform you that your account has been successfully created on the portal.</p><p><strong>Your login credentials are as follows:</strong></p><ul><li><strong>Username:</strong> {username} </li><li><strong>Password:</strong> {password} </li></ul>";
                        CommonMessage mailres = _mail.SendEmail(subject, body, model.EmailId);
                        if (mailres.status)
                        {
                            transaction.Commit();
                            return true;
                        }

                        string deletepaths = "~" + uploadPath;
                        if (Directory.Exists(HttpContext.Current.Server.MapPath(deletepaths)))
                        {
                            System.IO.File.Delete(deletepaths);
                        }
                        transaction.Rollback();
                        return false;
                    }
                }

                string deletepath = "~" + uploadPath;
                if (Directory.Exists(HttpContext.Current.Server.MapPath(deletepath)))
                {
                    System.IO.File.Delete(deletepath);
                }
                transaction.Rollback();
                return false;
            }
            catch (Exception ex)
            {
                string deletepath = "~" + uploadPath;
                if (Directory.Exists(HttpContext.Current.Server.MapPath(deletepath)))
                {
                    System.IO.File.Delete(deletepath);
                }
                transaction.Rollback();
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }


        public List<CreateAdmin> GetAllAdmin()
		{
			List<CreateAdmin> list = new List<CreateAdmin>();
			SqlDataReader rdr = null;

			try
			{
				SqlCommand cmd = new SqlCommand("sp_CreateAdmin", connection);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@action", "selectAllAdmin");

				connection.Open();
				rdr = cmd.ExecuteReader();

				while (rdr.Read())
				{
					CreateAdmin model = new CreateAdmin();

					model.Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0;
					model.CompanyId = Convert.ToInt32(rdr["companyId"] ?? "0");
					model.CompanyName =rdr["schoolName"]?.ToString();
					model.Name = rdr["Name"]?.ToString();
					model.MobileNo = rdr["MobileNo"] != DBNull.Value ? Convert.ToInt64(rdr["MobileNo"]) :0;
					model.EmailId = rdr["EmailId"]?.ToString();
                    //model.Images = rdr["Images"]?.ToString(); 
                    model.UserName = rdr["username"].ToString();
                    model.Password = rdr["password"].ToString();

					list.Add(model);
				}
			}
			catch (Exception ex)
			{
				
				Console.WriteLine("Error: " + ex.Message);
			}
			finally
			{
				if (rdr != null) rdr.Close();
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}

			return list;
		}

		public string deleteAdmin(int Id)
		{
			string result = "";

			try
			{
				SqlCommand cmd = new SqlCommand("sp_CreateAdmin", connection);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@action", "deleteAdmin");
				cmd.Parameters.AddWithValue("@Id", Id);

				connection.Open();
				cmd.ExecuteNonQuery();
				result = "Success";
			}
			catch (Exception ex)
			{
				result = "Error: " + ex.Message;
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}

			return result;
		}

        public bool UpdateAdmin(CreateAdmin model)
        {
            try
            {
                string uploadPath = string.Empty;

                if (model.Image != null)
                {
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
                    string fileExtension = Path.GetExtension(model.Image.FileName)?.ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return false;
                    }

                    uploadPath = UploadImageToServer(model.Image);
                    if (uploadPath == null)
                    {
                        return false;
                    }
                }

                SqlCommand cmd = new SqlCommand("sp_CreateAdmin", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "updateAdmin");
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
                cmd.Parameters.AddWithValue("@EmailId", model.EmailId);
                cmd.Parameters.AddWithValue("@companyId", model.CompanyId);
                cmd.Parameters.AddWithValue("@Image", string.IsNullOrEmpty(uploadPath) ? (object)DBNull.Value : uploadPath);

                connection.Open();
                int res = cmd.ExecuteNonQuery();
                return res > 0;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }


        public List<masterState> GetAllState()
		{
			List<masterState> list = new List<masterState>();
			try
			{

				SqlCommand cmd = new SqlCommand("SP_CompanyRegistration", connection);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@action", "GetAllState");
				connection.Open();

				SqlDataReader dr = cmd.ExecuteReader();
				while (dr.Read())
				{
					list.Add(new masterState
					{
						st_Id = Convert.ToInt32(dr["st_Id"]),
						stateName = dr["stateName"].ToString(),
					
					});
				}
                return list;

            }
            catch (Exception ex)
			{
                return list;

            }
            finally
			{
				connection.Close();
			}
		}

		public List<masterCity> GetCityByState(int id)
		{
            List<masterCity> list = new List<masterCity>();
            try
            {

                SqlCommand cmd = new SqlCommand("SP_CompanyRegistration", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "GetAllCityById");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new masterCity
                    {
                        city_Id = Convert.ToInt32(dr["city_id"]),
                        City_Name = dr["city_name"].ToString(),

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

		public CreateAdmin GetAdminById(int id)
		{
            CreateAdmin list = new CreateAdmin();
            SqlDataReader rdr = null;

            try
            {
                SqlCommand cmd = new SqlCommand("sp_CreateAdmin", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAdminById");
                cmd.Parameters.AddWithValue("@Id", id);

                connection.Open();
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list = new CreateAdmin();

                   list.Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0;
                   list.CompanyId = Convert.ToInt32(rdr["companyId"] ?? "0");
                   list.CompanyName = rdr["schoolName"]?.ToString();
                   list.Name = rdr["Name"]?.ToString();
                   list.MobileNo = rdr["MobileNo"] != DBNull.Value ? Convert.ToInt64(rdr["MobileNo"]) : 0;
                    list.EmailId = rdr["EmailId"]?.ToString();
                    list.Images = rdr["Image"]?.ToString();
					list.schoolAddress = rdr["school_address"]?.ToString()
;
               
                }
				return list;
            }
            catch (Exception ex)
            {

                return list;
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

        }

        //public List<masterState> GetAllState()
        //{
        //	List<masterState> list = new List<masterState>();
        //	try
        //	{

        //		SqlCommand cmd = new SqlCommand("sp_ClassManagement", connection);
        //		cmd.CommandType = CommandType.StoredProcedure;
        //		cmd.Parameters.AddWithValue("@Actions", "GetAllState");
        //		connection.Open();

        //		SqlDataReader dr = cmd.ExecuteReader();
        //		while (dr.Read())
        //		{
        //			list.Add(new masterState
        //			{
        //				ClassId = Convert.ToInt32(dr["ClassId"]),
        //				ClassName = dr["ClassName"].ToString(),
        //				ClassDescription = dr["Description"].ToString()
        //			});
        //		}

        //	}
        //	catch (Exception ex)
        //	{
        //		throw ex;
        //	}
        //	finally
        //	{
        //		connection.Close();
        //	}
        //	return list;
        //}

        public SuperAdminDashboardModel GetSuperadminDashboard()
        {
            SuperAdminDashboardModel model = new SuperAdminDashboardModel();

            using (SqlCommand cmd = new SqlCommand("sp_ManageSuperadminDashboard", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DashboardCount");

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    model.totalcompanys = Convert.ToInt32(dr["totalcompanys"]);
                    model.totaladmins = Convert.ToInt32(dr["totaladmins"]);
                }
                connection.Close();
            }

            return model;
        }
    }
}