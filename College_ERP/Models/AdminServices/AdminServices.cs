using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Policy;
using System.Web;
using System.Web.Helpers;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.Razor.Generator;
using System.Web.Razor.Tokenizer;
using System.Xml.Linq;
using College_ERP.Models.Admin;
using College_ERP.Models.SuperAdmin;
using LCMIS.Server.Model;
using Newtonsoft.Json;
using PdfSharp.Pdf.Content.Objects;
using static System.Collections.Specialized.BitVector32;
using static College_ERP.Models.AdminServices.AdminServices;
using static College_ERP.Models.Security.main;


namespace College_ERP.Models.AdminServices
{
    public class AdminServices
    {
        private readonly SqlConnection connection;
        private readonly College_ERP.Models.MailService.MailService _mail;

        public AdminServices()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
            _mail = new College_ERP.Models.MailService.MailService();
        }

        public AdminDetails GetAdminDetails(int userid)
        {
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("sp_CreateAdmin", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "GetProfileDetails");
                cmd.Parameters.AddWithValue("@Id", userid);
                AdminDetails list = new AdminDetails();
                var res = cmd.ExecuteReader();
                if (res != null)
                {
                    while (res.Read())
                    {
                        list.adminId = Convert.ToInt32(res["id"]);
                        list.adminImage = res["Image"].ToString();
                        list.adminName = res["Name"].ToString();
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
                {
                    connection.Close();
                }
                connection.Dispose();
            }
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
        public string GenerateAdmissionNo(string studentName, string mobileNo)
        {
            if (string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(mobileNo) || mobileNo.Length < 4)
                return string.Empty;

            var firstName = studentName.Split(' ')[0];
            var last4Digits = mobileNo.Substring(mobileNo.Length - 4);
            var currentYear = DateTime.Now.Year;

            return $"{firstName}-{last4Digits}-{currentYear}";
        }

        public bool InsertClass(ClassModel cs,out string msg)
        {
            msg = "";
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            List<string> skippedClasses = new List<string>();

            try
            {
                foreach (var item in cs.Class)
                {
                    SqlCommand cmd = new SqlCommand("sp_ClassManagement", connection, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Actions", "InsertClass");
                    cmd.Parameters.AddWithValue("@userid", cs.userid);
                    cmd.Parameters.AddWithValue("@InstitutionType", cs.InstitutionType);
                    cmd.Parameters.AddWithValue("@EducationLevel", cs.EducationLevel);
                    cmd.Parameters.AddWithValue("@ClassName", item);

                    SqlParameter classid = new SqlParameter("@outClassId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    SqlParameter returnVal = new SqlParameter("@returnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(classid);
                    cmd.Parameters.Add(returnVal);

                    cmd.ExecuteNonQuery();
                    int result = (int)returnVal.Value;

                    if (result == 0)
                    {
                        skippedClasses.Add(item);
                        continue;
                    }

                    int csid = (int)classid.Value;

                    if (cs.Stream != null)
                    {
                        foreach (var st in cs.Stream)
                        {
                            SqlCommand cmd1 = new SqlCommand("sp_ClassManagement", connection, transaction);
                            cmd1.CommandType = CommandType.StoredProcedure;
                            cmd1.Parameters.AddWithValue("@Actions", "insertclassstream");
                            cmd1.Parameters.AddWithValue("@ClassId", csid);
                            cmd1.Parameters.AddWithValue("@stream", st);
                            cmd1.Parameters.AddWithValue("@status", 1);
                            cmd1.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();

                if (skippedClasses.Count > 0)
                {
                    string skipped = string.Join(", ", skippedClasses);
                    msg = $"Success! Some classes were already present and skipped: {skipped}";
                    return true;
                }

                msg= "Success! All classes inserted successfully.";
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                msg = ex.Message;
                return false;
            }
            finally
            {
                connection.Close();
            }
        }


        public List<ClassModel> GetAllClasses(int userid)
        {
            List<ClassModel> list = new List<ClassModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ClassManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetAllClass");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ClassModel
                    {
                        ClassId = Convert.ToInt32(dr["ClassId"]),
                        ClassName = dr["ClassName"].ToString(),
                        InstitutionType = dr["InstitutionType"].ToString(),
                        EducationLevel = dr["EducationLevel"].ToString(),
                        HasStream = dr["HasStream"] != null ? Convert.ToBoolean(dr["HasStream"]) : false
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
        public List<ClassModel> GetStreamByClassId(int classid)
        {
            List<ClassModel> list = new List<ClassModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ClassManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetClassStream");
                cmd.Parameters.AddWithValue("@ClassId", classid);
                connection.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ClassModel
                    {
                        Classstreamid = Convert.ToInt32(dr["id"]),
                        ClassStream = dr["stream"].ToString(),
                        ClassName = dr["ClassName"].ToString(),
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
        public List<string> GetEducationLevel(string institutiontype)
        {
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ClassManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "geteducationlevel");
                cmd.Parameters.AddWithValue("@InstitutionType", institutiontype);
                connection.Open();
                List<string> edulevel = new List<string>();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    edulevel.Add(dr["educationlevel"].ToString());
                }
                return edulevel;
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
        public List<ClassModel> GetClassByEducationLevel(string edulevel,int userid)
        {
            try
            {
                List<ClassModel> list = new List<ClassModel>();
                SqlCommand cmd = new SqlCommand("sp_ClassManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "getclassbyeducation");
                cmd.Parameters.AddWithValue("@EducationLevel", edulevel);
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ClassModel
                    {
                        ClassId = Convert.ToInt32(dr["ClassId"]),
                        ClassName = dr["ClassName"].ToString()
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
        public List<ClassModel> GetStreamByClass(int classid)
        {
            try
            {
                List<ClassModel> list = new List<ClassModel>();
                SqlCommand cmd = new SqlCommand("sp_ClassManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "getstreambyclass");
                cmd.Parameters.AddWithValue("@ClassId", classid);
                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ClassModel
                    {
                        ClassId = Convert.ToInt32(dr["classid"]),
                        ClassStream = dr["stream"].ToString()
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

        public string DeleteClass(int classid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ClassManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "DeleteClass");
                cmd.Parameters.AddWithValue("@ClassId", classid);
                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public ClassModel GetClassById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ClassManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ClassId", id);
                cmd.Parameters.AddWithValue("@Actions", "GetClassById");
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new ClassModel
                    {
                        ClassId = Convert.ToInt32(reader["ClassId"]),
                        ClassName = reader["ClassName"].ToString(),
                        ClassDescription = reader["Description"].ToString()
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

        public string UpdateClass(int classId, string className, string classDescription)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ClassManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@ClassName", className);
                cmd.Parameters.AddWithValue("@ClassDescription", classDescription);
                cmd.Parameters.AddWithValue("@Actions", "UpdateClass");
                connection.Open();

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return "Success";
        }

        public string InsertDesignation(string DesignationName, string DesignationDescription, int userid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_DesignationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "InsertDesignation");
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@DesignationName", DesignationName);
                cmd.Parameters.AddWithValue("@DesignationDescription", DesignationDescription);
                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public List<DesignationModel> GetAllDesignations(int userid)
        {
            List<DesignationModel> list = new List<DesignationModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_DesignationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetAllDesignation");
                cmd.Parameters.AddWithValue("@userid", userid);

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new DesignationModel
                    {
                        DesignationId = Convert.ToInt32(dr["DesignationId"]),
                        DesignationName = dr["DesignationName"].ToString(),
                        DesignationDescription = dr["Description"].ToString()
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

        public string DeleteDesignation(int Designationid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_DesignationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "DeleteDesignation");
                cmd.Parameters.AddWithValue("@DesignationId", Designationid);
                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public DesignationModel GetDesignationById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_DesignationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DesignationId", id);
                cmd.Parameters.AddWithValue("@Actions", "GetDesignationById");

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new DesignationModel
                    {
                        DesignationId = Convert.ToInt32(reader["DesignationId"]),
                        DesignationName = reader["DesignationName"].ToString(),
                        DesignationDescription = reader["Description"].ToString()
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

        public string UpdateDesignation(int DesignationId, string DesignationName, string DesignationDescription)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_DesignationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DesignationId", DesignationId);
                cmd.Parameters.AddWithValue("@DesignationName", DesignationName);
                cmd.Parameters.AddWithValue("@DesignationDescription", DesignationDescription);
                cmd.Parameters.AddWithValue("@Actions", "UpdateDesignation");

                connection.Open();
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return "Success";
        }
        public string InsertSection(int classId, string sectionName,int classStreamId, string sectionDescription, int userid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_SectionManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "InsertSection");
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@ClassId", classId);
                if(classStreamId!=0)
                    cmd.Parameters.AddWithValue("@classStreamId", classStreamId);
                cmd.Parameters.AddWithValue("@SectionName", sectionName);
                cmd.Parameters.AddWithValue("@SectionDescription", sectionDescription);

                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }



        public List<SectionModel> GetAllSections(int userid)
        {
            List<SectionModel> sections = new List<SectionModel>();

            try
            {

                SqlCommand cmd = new SqlCommand("sp_SectionManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetAllSections");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    SectionModel section = new SectionModel
                    {
                        SectionId = Convert.ToInt32(reader["SectionId"]),
                        ClassName = reader["ClassName"].ToString(),
                        SectionName = reader["SectionName"].ToString(),
                        Stream = reader["stream"].ToString(),
                        SectionDescription = reader["Description"].ToString()
                    };

                    sections.Add(section);
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

            return sections;
        }


        public string DeleteSection(int sectionid)
        {
            string result = "";

            try
            {


                SqlCommand cmd = new SqlCommand("sp_SectionManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "DeleteSection");
                cmd.Parameters.AddWithValue("@SectionId", sectionid);

                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public SectionModel GetSectionById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_SectionManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SectionId", id);
                cmd.Parameters.AddWithValue("@Actions", "GetSectionById");

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new SectionModel
                    {
                        SectionId = Convert.ToInt32(reader["SectionId"]),
                        ClassName = reader["ClassName"].ToString(),
                        Stream = reader["stream"].ToString(),
                        ClassId = Convert.ToInt32(reader["ClassId"]),
                        SectionName = reader["SectionName"].ToString(),
                        SectionDescription = reader["Description"].ToString()
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

        public string UpdateSection(int sectionId, int classId, string sectionName, string sectionDescription)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_SectionManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@SectionName", sectionName);
                cmd.Parameters.AddWithValue("@SectionDescription", sectionDescription);
                cmd.Parameters.AddWithValue("@Actions", "Update Section");
                connection.Open();

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return "Success";
        }

        public List<SubjectModel> GetAllSubject(int userid)
        {
            List<SubjectModel> list = new List<SubjectModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_SubjectManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetAllSubject");
                cmd.Parameters.AddWithValue("@userid", userid);
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new SubjectModel
                    {
                        SubjectId = rdr["SubjectId"] != DBNull.Value ? Convert.ToInt32(rdr["SubjectId"]) : 0,
                        ClassId = rdr["ClassName"].ToString(),
                        Subject = rdr["Subject"]?.ToString(),
                        Description = rdr["Description"]?.ToString(),
                        classstream = rdr["stream"].ToString(),
                        optionsub = rdr["optional"]!=null?Convert.ToInt32(rdr["optional"]):2
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

        public bool InsertSubject(SubjectModel model,out string errormsg)
        {
            errormsg = "";
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                foreach(var item in model.Subjects)
                {
                    SqlCommand command = new SqlCommand("sp_SubjectManagement", connection,transaction);

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Actions", "InsertSubject");
                    command.Parameters.AddWithValue("@userid", model.userid);
                    command.Parameters.AddWithValue("@ClassId", model.ClassId);
                    command.Parameters.AddWithValue("@classStreamId", model.classStreamId);
                    command.Parameters.AddWithValue("@Subject", item);
                    int res = command.ExecuteNonQuery();
                    command.Dispose();
                    if (res <= 0)
                    {
                        errormsg = "Some error occured while processing your request.";
                        transaction.Rollback();
                        return false;
                    }
                }
                if (model.optionalsubject!=null)
                {
                    foreach (var item in model.optionalsubject)
                    {
                        SqlCommand command = new SqlCommand("sp_SubjectManagement", connection, transaction);

                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Actions", "InsertSubject");
                        command.Parameters.AddWithValue("@userid", model.userid);
                        command.Parameters.AddWithValue("@ClassId", model.ClassId);
                        command.Parameters.AddWithValue("@optionalsubject", 1);
                        command.Parameters.AddWithValue("@classStreamId", model.classStreamId);
                        command.Parameters.AddWithValue("@Subject", item);
                        int res = command.ExecuteNonQuery();
                        command.Dispose();
                        if (res <= 0)
                        {
                            errormsg = "Some error occured while processing your request.";
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                errormsg = ex.Message;
                transaction.Rollback();
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public string DeleteSubject(int subjectid)
        {

            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_SubjectManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "DeleteSubject");
                cmd.Parameters.AddWithValue("@SubjectId", subjectid);
                connection.Open();

                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }
        #region Get subjectby id
        public List<SubjectModel> GetSubjectById(int? id)
        {
            List<SubjectModel> list = new List<SubjectModel>();
            try
            {


                SqlCommand cmd = new SqlCommand("sp_SubjectManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetSubjectById");
                cmd.Parameters.AddWithValue("@SubjectId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new SubjectModel
                    {
                        SubjectId = rdr["SubjectId"] != DBNull.Value ? Convert.ToInt32(rdr["SubjectId"]) : 0,
                        Subject = rdr["Subject"]?.ToString(),
                        Description = rdr["Description"]?.ToString(),
                        ClassId = rdr["ClassId"].ToString(),
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
        #endregion

        public void UpdateSubject(SubjectModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_SubjectManagement", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Actions", "UpdateSubject");
                command.Parameters.AddWithValue("@SubjectId", model.SubjectId);
                command.Parameters.AddWithValue("@ClassId", model.ClassId);
                command.Parameters.AddWithValue("@Subject", model.Subject);
                command.Parameters.AddWithValue("@Description", model.Description);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<CircularModel> GetAllCirculars(int userid)
        {
            List<CircularModel> list = new List<CircularModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_CircularManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetAllCircular");
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

        public bool InsertCircular(CircularModel model, out string errorMessage)
        {
            int result = 0;
            try
            {
                errorMessage = "";
                string attachmentPath = string.Empty;
                if (model.Attachment != null)
                {
                    attachmentPath = UploadImageToServer(model.Attachment);
                }
                SqlCommand command = new SqlCommand("sp_CircularManagement", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Actions", "InsertCircular");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@CircularTitle", model.CircularTitle);
                command.Parameters.AddWithValue("@CircularDate", model.CircularDate);
                command.Parameters.AddWithValue("@Description", model.CircularDescription);
                if (model.Attachment != null)
                {
                    command.Parameters.AddWithValue("@UploadAttachment", attachmentPath);
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

        public string DeleteCircular(int circularid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_CircularManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "DeleteCircular");
                cmd.Parameters.AddWithValue("@CircularId", circularid);
                connection.Open();

                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }


        public List<CircularModel> GetCircularById(int? id)
        {
            List<CircularModel> list = new List<CircularModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_CircularManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetCircularById");
                cmd.Parameters.AddWithValue("@CircularId", Convert.ToInt32(id));
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new CircularModel
                    {
                        CircularId = Convert.ToInt32(rdr["CircularId"]),
                        CircularTitle = rdr["CircularTitle"].ToString(),
                        CircularDate = Convert.ToDateTime(rdr["CircularDate"]),
                        CircularDescription = rdr["Description"].ToString(),
                        UploadAttachment = rdr["UploadAttachment"].ToString(),
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

        public bool UpdateCircular(CircularModel model)
        {
            int result = 0;
            try
            {
                SqlCommand cmd = new SqlCommand("sp_CircularManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "UpdateCircular");
                cmd.Parameters.AddWithValue("@CircularId", model.CircularId);
                cmd.Parameters.AddWithValue("@CircularTitle", model.CircularTitle);
                cmd.Parameters.AddWithValue("@CircularDate", model.CircularDate);
                cmd.Parameters.AddWithValue("@Description", model.CircularDescription);
                if (model.Attachment != null)
                {
                    cmd.Parameters.AddWithValue("@UploadAttachment", model.Attachment.FileName);
                }
                connection.Open();
                result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    UploadImageToServer(model.Attachment);
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
            return result > 0;

        }

        public List<AwardModel> GetAllSchoolAwards(int userid)
        {
            List<AwardModel> awards = new List<AwardModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_SchoolAwardManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetAllSchoolAward");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    awards.Add(new AwardModel
                    {
                        AwardId = Convert.ToInt32(dr["SchoolId"]),
                        AwardName = dr["AwardName"].ToString(),
                        AwardTitle = dr["AwardTitle"].ToString(),
                        AwardDate = Convert.ToDateTime(dr["AwardDate"]),
                        AwardCertificate = dr["AwardCertificate"].ToString(),
                        AwardDescription = dr["AwardDescription"].ToString()
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

            return awards;
        }

        public string InsertSchoolAward(AwardModel model)
        {
            int result = 0;

            try
            {
                string certificatePath = string.Empty;

                if (model.AwardCertificates != null)
                {
                    string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
                    string fileExtension = Path.GetExtension(model.AwardCertificates.FileName)?.ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return "Invalid file type. Only PDF, JPG, JPEG, and PNG files are allowed.";
                    }

                    certificatePath = UploadImageToServer(model.AwardCertificates);
                }

                using (SqlCommand cmd = new SqlCommand("sp_SchoolAwardManagement", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Actions", "InsertSchoolAward");
                    cmd.Parameters.AddWithValue("@userid", model.userid);
                    cmd.Parameters.AddWithValue("@AwardName", model.AwardName);
                    cmd.Parameters.AddWithValue("@AwardTitle", model.AwardTitle);
                    cmd.Parameters.AddWithValue("@AwardDate", model.AwardDate);
                    cmd.Parameters.AddWithValue("@AwardDescription", model.AwardDescription);

                    if (!string.IsNullOrEmpty(certificatePath))
                    {
                        cmd.Parameters.AddWithValue("@AwardCertificate", certificatePath);
                    }

                    connection.Open();
                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                return "An error occurred: " + ex.Message;
            }
            finally
            {
                connection.Close();
            }

            return result > 0 ? "Inserted successfully" : "Insertion failed";
        }




        public bool DeleteSchoolAward(int id)
        {
            int result = 0;
            try
            {
                SqlCommand cmd = new SqlCommand("sp_SchoolAwardManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "DeleteSchoolAward");
                cmd.Parameters.AddWithValue("@SchoolAwardId", id);
                connection.Open();
                result = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return result > 0;
        }

        public List<AwardModel> GetSchoolAwardById(int? id)
        {
            List<AwardModel> list = new List<AwardModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_SchoolAwardManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetSchoolAwardById");
                cmd.Parameters.AddWithValue("@SchoolAwardId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new AwardModel
                    {
                        AwardId = Convert.ToInt32(rdr["SchoolId"]),
                        AwardName = rdr["AwardName"].ToString(),
                        AwardTitle = rdr["AwardTitle"].ToString(),
                        AwardDate = Convert.ToDateTime(rdr["AwardDate"]),
                        AwardCertificate = rdr["AwardCertificate"].ToString(),
                        AwardDescription = rdr["AwardDescription"].ToString()
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

        public string UpdateSchoolAward(AwardModel model)
        {
            int res = 0;

            try
            {
                string certificatePath = string.Empty;

                if (model.AwardCertificates != null)
                {
                    // Validate file extension
                    string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
                    string fileExtension = Path.GetExtension(model.AwardCertificates.FileName)?.ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return "Invalid file type. Only PDF, JPG, JPEG, and PNG are allowed.";
                    }

                    // Upload the file
                    certificatePath = UploadImageToServer(model.AwardCertificates);
                }

                using (SqlCommand command = new SqlCommand("sp_SchoolAwardManagement", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Actions", "UpdateSchoolAward");

                    command.Parameters.AddWithValue("@SchoolAwardId", model.AwardId);
                    command.Parameters.AddWithValue("@AwardName", model.AwardName);
                    command.Parameters.AddWithValue("@AwardTitle", model.AwardTitle);
                    command.Parameters.AddWithValue("@AwardDate", model.AwardDate);
                    command.Parameters.AddWithValue("@AwardDescription", model.AwardDescription);

                    if (!string.IsNullOrEmpty(certificatePath))
                    {
                        command.Parameters.AddWithValue("@AwardCertificate", certificatePath);
                    }

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
            finally
            {
                connection.Close();
            }

            return res > 0 ? "School award updated successfully." : "Update failed.";
        }



        public List<StudentAwardModel> GetAllStudentAwards(int userid)
        {
            List<StudentAwardModel> awards = new List<StudentAwardModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentAwardManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetAllStudentAward");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    awards.Add(new StudentAwardModel
                    {
                        AwardId = Convert.ToInt32(dr["AwardId"]),
                        StudentId = Convert.ToInt32(dr["StudentId"]),
                        StudentName = dr["StudentName"].ToString(),
                        AwardTitle = dr["AwardTitle"].ToString(),
                        ClassName = dr["ClassName"].ToString(),
                        SectionName = dr["SectionName"].ToString(),
                        Session = dr["Session"].ToString(),
                        AwardDate = Convert.ToDateTime(dr["AwardDate"]),
                        Description = dr["Description"].ToString(),
                        CertificatePath = dr["CertificatePath"].ToString(),
                        AwardType = dr["AwardType"].ToString()
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
            return awards;
        }

        public string InsertStudentAward(StudentAwardModel model)
        {
            int result = 0;
            try
            {
                string certificatePath = string.Empty;

                if (model.Certificate != null)
                {
                    // Optional: Validate file extension here (pdf, jpg, png, etc.)
                    string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
                    string fileExtension = Path.GetExtension(model.Certificate.FileName)?.ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return "Invalid file type. Only PDF, JPG, JPEG, and PNG files are allowed.";
                    }

                    // Upload file to server, get saved path
                    certificatePath = UploadImageToServer(model.Certificate);
                }

                using (SqlCommand cmd = new SqlCommand("sp_StudentAwardManagement", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Actions", "InsertStudentAward");
                    cmd.Parameters.AddWithValue("@userid", model.userid);
                    cmd.Parameters.AddWithValue("@StudentId", model.StudentName);
                    cmd.Parameters.AddWithValue("@AwardTitle", model.AwardTitle);
                    cmd.Parameters.AddWithValue("@Class", model.ClassName);
                    cmd.Parameters.AddWithValue("@Section", model.SectionName);
                    cmd.Parameters.AddWithValue("@Session", model.Session);
                    cmd.Parameters.AddWithValue("@AwardDate", model.AwardDate);
                    cmd.Parameters.AddWithValue("@Description", model.Description);
                    cmd.Parameters.AddWithValue("@AwardType", model.AwardType);

                    if (!string.IsNullOrEmpty(certificatePath))
                    {
                        cmd.Parameters.AddWithValue("@CertificatePath", certificatePath);
                    }

                    connection.Open();
                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log error or handle accordingly
                return "An error occurred: " + ex.Message;
            }
            finally
            {
                connection.Close();
            }

            return result > 0 ? "Inserted successfully" : "Insertion failed";
        }


        public string DeleteStudentAward(int awardid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentAwardManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "DeleteStudentAward");
                cmd.Parameters.AddWithValue("@AwardId", awardid);

                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public List<StudentAwardModel> GetStudentAwardById(int? id)
        {
            List<StudentAwardModel> list = new List<StudentAwardModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentAwardManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetStudentAwardById");
                cmd.Parameters.AddWithValue("@AwardId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new StudentAwardModel
                    {
                        AwardId = Convert.ToInt32(rdr["AwardId"]),
                        StudentId = Convert.ToInt32(rdr["StudentId"]),
                        StudentName = rdr["StudentName"].ToString(),
                        AwardTitle = rdr["AwardTitle"].ToString(),
                        ClassName = rdr["ClassName"].ToString(),
                        ClassId = Convert.ToInt32(rdr["ClassId"]),
                        SectionName = rdr["SectionName"].ToString(),
                        SectionId = Convert.ToInt32(rdr["SectionId"]),
                        Session = rdr["Session"].ToString(),
                        AwardDate = Convert.ToDateTime(rdr["AwardDate"]),
                        Description = rdr["Description"].ToString(),
                        CertificatePath = rdr["CertificatePath"].ToString(),
                        AwardType = rdr["AwardType"].ToString()
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

        public bool UpdateStudentAward(StudentAwardModel model)
        {
            int result = 0;
            try
            {
                string certificatePath = string.Empty;

                if (model.Certificate != null)
                {
                    // Optional: validate file type here
                    string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
                    string fileExtension = Path.GetExtension(model.Certificate.FileName)?.ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        // You can handle this as needed; here I return false
                        return false;
                    }

                    // Upload the file first
                    certificatePath = UploadImageToServer(model.Certificate);
                }

                using (SqlCommand cmd = new SqlCommand("sp_StudentAwardManagement", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Actions", "UpdateStudentAward");
                    cmd.Parameters.AddWithValue("@AwardId", model.AwardId);
                    cmd.Parameters.AddWithValue("@StudentId", model.StudentName);
                    cmd.Parameters.AddWithValue("@AwardTitle", model.AwardTitle);
                    cmd.Parameters.AddWithValue("@Class", model.ClassName);
                    cmd.Parameters.AddWithValue("@Section", model.SectionName);
                    cmd.Parameters.AddWithValue("@Session", model.Session);
                    cmd.Parameters.AddWithValue("@AwardDate", model.AwardDate);
                    cmd.Parameters.AddWithValue("@Description", model.Description);
                    cmd.Parameters.AddWithValue("@AwardType", model.AwardType);

                    if (!string.IsNullOrEmpty(certificatePath))
                    {
                        cmd.Parameters.AddWithValue("@CertificatePath", certificatePath);
                    }

                    connection.Open();
                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw; // preserve original stack trace
            }
            finally
            {
                connection.Close();
            }
            return result > 0;
        }



        public List<TeacherAwardModel> GetAllTeacherAwards(int userid)
        {
            List<TeacherAwardModel> awards = new List<TeacherAwardModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTeacherAwards", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTALL");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    awards.Add(new TeacherAwardModel
                    {
                        awardid = Convert.ToInt32(dr["AwardId"]),
                        TeacherId = Convert.ToInt32(dr["TeacherId"]),
                        teacherName = dr["TeacherName"].ToString(),
                        awardTitle = dr["Title"].ToString(),
                        awardSession = dr["Session"].ToString(),
                        awardDate = Convert.ToDateTime(dr["AwardDate"]),
                        awardDesc = dr["Description"].ToString(),
                        awardcertificate = dr["CertificatePath"].ToString(),
                        awardType = dr["AwardType"].ToString()
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
            return awards;
        }


        public string InsertTeacherAward(TeacherAwardModel model)
        {
            int result = 0;
            try
            {
                string certificatePath = string.Empty;

                if (model.certificate != null)
                {
                    string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
                    string fileExtension = Path.GetExtension(model.certificate.FileName)?.ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return "Invalid file type. Only PDF, JPG, JPEG, and PNG files are allowed.";
                    }

                    certificatePath = UploadImageToServer(model.certificate);
                }

                using (SqlCommand cmd = new SqlCommand("sp_ManageTeacherAwards", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "INSERT");
                    cmd.Parameters.AddWithValue("@userid", model.userid);
                    cmd.Parameters.AddWithValue("@TeacherId", model.teacherName);
                    cmd.Parameters.AddWithValue("@Title", model.awardTitle);
                    cmd.Parameters.AddWithValue("@Session", model.awardSession);
                    cmd.Parameters.AddWithValue("@AwardDate", model.awardDate);
                    cmd.Parameters.AddWithValue("@Description", model.awardDesc);
                    cmd.Parameters.AddWithValue("@AwardType", model.awardType);

                    if (!string.IsNullOrEmpty(certificatePath))
                    {
                        cmd.Parameters.AddWithValue("@CertificatePath", certificatePath);
                    }

                    connection.Open();
                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Consider logging ex here
                return "An error occurred: " + ex.Message;
            }
            finally
            {
                connection.Close();
            }
            return result > 0 ? "Inserted successfully" : "Insertion failed";
        }


        public string DeleteTeacherAward(int awardid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTeacherAwards", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@AwardId", awardid);

                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public List<TeacherAwardModel> GetTeacherAwardById(int? id)
        {
            List<TeacherAwardModel> list = new List<TeacherAwardModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTeacherAwards", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@AwardId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new TeacherAwardModel
                    {
                        awardid = Convert.ToInt32(dr["AwardId"]),
                        TeacherId = Convert.ToInt32(dr["TeacherId"]),
                        teacherName = dr["TeacherName"].ToString(),
                        awardTitle = dr["Title"].ToString(),
                        awardSession = dr["Session"].ToString(),
                        awardDate = Convert.ToDateTime(dr["AwardDate"]),
                        awardDesc = dr["Description"].ToString(),
                        awardcertificate = dr["CertificatePath"].ToString(),
                        awardType = dr["AwardType"].ToString()
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

        public string UpdateTeacherAward(TeacherAwardModel model)
        {
            try
            {
                string certificatePath = string.Empty;

                if (model.certificate != null)
                {
                    string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
                    string fileExtension = Path.GetExtension(model.certificate.FileName)?.ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return "Invalid file type. Only PDF, JPG, JPEG, and PNG are allowed.";
                    }

                    certificatePath = UploadImageToServer(model.certificate);
                }

                using (SqlCommand cmd = new SqlCommand("sp_ManageTeacherAwards", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "UPDATE");
                    cmd.Parameters.AddWithValue("@AwardId", model.awardid);
                    cmd.Parameters.AddWithValue("@TeacherId", model.teacherName);
                    cmd.Parameters.AddWithValue("@Title", model.awardTitle);
                    cmd.Parameters.AddWithValue("@Session", model.awardSession);
                    cmd.Parameters.AddWithValue("@AwardDate", model.awardDate);
                    cmd.Parameters.AddWithValue("@Description", model.awardDesc);
                    cmd.Parameters.AddWithValue("@AwardType", model.awardType);

                    if (!string.IsNullOrEmpty(certificatePath))
                    {
                        cmd.Parameters.AddWithValue("@CertificatePath", certificatePath);
                    }

                    connection.Open();
                    int result = cmd.ExecuteNonQuery();

                    return result > 0 ? "Updated successfully" : "Update failed.";
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
            finally
            {
                connection.Close();
            }
        }





        public RegistrationModel GetStudentById(int id)
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
                    return new RegistrationModel
                    {
                        StudentId = rdr["StudentId"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        AdmissionNo = rdr["AdmissionNo"]?.ToString(),
                        StudentName = rdr["StudentName"]?.ToString(),
                        MotherTongue = rdr["MotherTougue"]?.ToString(),
                        //Stream = rdr["stream"]?.ToString(),
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
                        LastSchoolAttended = rdr["LastSchoolAttended"]?.ToString(),
                        DateOfAdmission = rdr["DateOfAdmission"] != DBNull.Value ? Convert.ToDateTime(rdr["DateOfAdmission"]) : DateTime.MinValue,
                        BloodGroup = rdr["BloodGroup"]?.ToString(),
                        FatherName = rdr["FatherName"]?.ToString(),
                        FatherQualification = rdr["FatherQualification"]?.ToString(),
                        Nationality = rdr["Nationality"]?.ToString(),
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
                        StudentPhotos =rdr["StudentPhoto"] !=DBNull.Value? rdr["StudentPhoto"]?.ToString(): "~/Content/images/default-avatar.png",
                        FatherPhotos = rdr["FatherPhoto"] != DBNull.Value ? rdr["FatherPhoto"]?.ToString(): "~/Content/images/default-avatar.png",
                        MotherPhotos = rdr["MotherPhoto"] != DBNull.Value ? rdr["MotherPhoto"]?.ToString(): "~/Content/images/default-avatar.png",
                        StudentAadharPhotos =rdr["StudentAadharPhoto"] !=DBNull.Value? rdr["StudentAadharPhoto"]?.ToString(): "~/Content/images/default-avatar.png",
                        AcademicYear = rdr["AcademicYear"]?.ToString(),
                        AdmissionStage = rdr["AdmissionStage"]?.ToString(),
                        parentEmail = rdr["parentEmail"]?.ToString()
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
        public List<RegistrationModel> GetAllStudents(int userid)
        {
            List<RegistrationModel> list = new List<RegistrationModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTALL");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new RegistrationModel
                    {
                        StudentId = rdr["StudentId"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        AdmissionNo = rdr["AdmissionNo"]?.ToString(),
                        StudentName = rdr["StudentName"]?.ToString(),
                        MotherTongue = rdr["MotherTougue"]?.ToString(),
                        Stream = rdr["stream"]?.ToString(),
                        ClassName = rdr["ClassName"]?.ToString(),
                        ClassId = Convert.ToInt32(rdr["ClassId"]),
                        SectionName = rdr["SectionName"]?.ToString(),
                        SectionId = rdr["SectionId"]!=DBNull.Value? Convert.ToInt32(rdr["SectionId"]):0,
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
                        AcademicYear = rdr["AcademicYear"]?.ToString(),
                        AdmissionStage = rdr["AdmissionStage"]?.ToString(),
                        MobileNo = rdr["MobileNo"]?.ToString(),
                        FatherOfficeMobileNo = Convert.ToInt64(rdr["FatherOfficeNo"]),
                        MotherOfficeMobileNo = Convert.ToInt64(rdr["MotherOfficeNo"]),
                        MotherOfficeAddress = rdr["MotherOfficeAddress"]?.ToString(),
                        StudentPhotos = rdr["StudentPhoto"]?.ToString(),
                        FatherPhotos = rdr["FatherPhoto"]?.ToString(),
                        MotherPhotos = rdr["MotherPhoto"]?.ToString(),
                        StudentAadharPhotos = rdr["StudentAadharPhoto"]?.ToString(),
                        ClassModel = !string.IsNullOrEmpty(rdr["classmodel"]?.ToString())
                        ? JsonConvert.DeserializeObject<ClassModel>(rdr["classmodel"].ToString())
                        : null,

                        ClassStream = !string.IsNullOrEmpty(rdr["classstream"]?.ToString())
                          ? JsonConvert.DeserializeObject<ClassStream>(rdr["classstream"].ToString())
                        : null,
                        username = rdr["username"] != DBNull.Value ? rdr["username"].ToString():null,
                        password = rdr["password"] != DBNull.Value ? rdr["password"].ToString():null
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

        private readonly string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };

        public bool InsertStudent(RegistrationModel registration, out string errorMessage)
        {
            errorMessage = string.Empty;
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // Validate file extensions before uploading
                if (registration.StudentPhoto != null && !IsAllowedFile(registration.StudentPhoto.FileName))
                {
                    errorMessage = "Invalid file type for Student Photo. Only .jpg, .jpeg, .png, .pdf allowed.";
                    return false;
                }
                if (registration.FatherPhoto != null && !IsAllowedFile(registration.FatherPhoto.FileName))
                {
                    errorMessage = "Invalid file type for Father Photo. Only .jpg, .jpeg, .png, .pdf allowed.";
                    return false;
                }
                if (registration.MotherPhoto != null && !IsAllowedFile(registration.MotherPhoto.FileName))
                {
                    errorMessage = "Invalid file type for Mother Photo. Only .jpg, .jpeg, .png, .pdf allowed.";
                    return false;
                }
                if (registration.StudentAadharPhoto != null && !IsAllowedFile(registration.StudentAadharPhoto.FileName))
                {
                    errorMessage = "Invalid file type for Student Aadhar Photo. Only .jpg, .jpeg, .png, .pdf allowed.";
                    return false;
                }

                string sphoto = null;
                string fphoto = null;
                string mphoto = null;
                string aphoto = null;

                if (registration.StudentPhoto != null)
                    sphoto = UploadImageToServer(registration.StudentPhoto);

                if (registration.FatherPhoto != null)
                    fphoto = UploadImageToServer(registration.FatherPhoto);

                if (registration.MotherPhoto != null)
                    mphoto = UploadImageToServer(registration.MotherPhoto);

                if (registration.StudentAadharPhoto != null)
                    aphoto = UploadImageToServer(registration.StudentAadharPhoto);

                SqlCommand command = new SqlCommand("sp_StudentRegistrationManagement", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", registration.userid);
                if (registration.classStreamId != 0)
                    command.Parameters.AddWithValue("@ClassStreamId", registration.classStreamId);
                command.Parameters.AddWithValue("@AdmissionNo", registration.AdmissionNo);
                command.Parameters.AddWithValue("@StudentName", registration.StudentName);
                command.Parameters.AddWithValue("@MotherTougue", registration.MotherTongue);
                command.Parameters.AddWithValue("@ClassId", registration.ClassId);
                command.Parameters.AddWithValue("@SectionId", registration.SectionId);
                command.Parameters.AddWithValue("@Gender", registration.Gender);
                command.Parameters.AddWithValue("@Religion", registration.Religion);
                command.Parameters.AddWithValue("@Caste", registration.Caste);
                command.Parameters.AddWithValue("@PlaceOfBirth", registration.PlaceOfBirth);
                command.Parameters.AddWithValue("@DOB", registration.DOB);
                command.Parameters.AddWithValue("@StateId", registration.StateId);
                command.Parameters.AddWithValue("@CityId", registration.CityId);
                command.Parameters.AddWithValue("@parentEmail", registration.parentEmail);
                command.Parameters.AddWithValue("@DateOfAdmission", registration.DateOfAdmission);
                command.Parameters.AddWithValue("@MobileNo", registration.MobileNo);
                command.Parameters.AddWithValue("@BloodGroup", registration.BloodGroup);
                command.Parameters.AddWithValue("@Nationality", registration.Nationality);
                command.Parameters.AddWithValue("@Address", registration.Address);
                command.Parameters.AddWithValue("@CurrentAddress", registration.CurrentAddress);
                command.Parameters.AddWithValue("@Hobbies", registration.Hobbies);
                command.Parameters.AddWithValue("@StudentEmail", registration.StudentEmail);
                command.Parameters.AddWithValue("@AadharNumber", registration.AadharNo);
                command.Parameters.AddWithValue("@LastSchoolAttended", registration.LastSchoolAttended);
                command.Parameters.AddWithValue("@YearOfPassing", registration.YearOfPassing);
                command.Parameters.AddWithValue("@ObtainedMarks", registration.ObtainedMarks);
                command.Parameters.AddWithValue("@TotalMarks", registration.TotalMarks);
                command.Parameters.AddWithValue("@FatherName", registration.FatherName);
                command.Parameters.AddWithValue("@MotherName", registration.MotherName);
                command.Parameters.AddWithValue("@academicyear", registration.AcademicYear);
                command.Parameters.AddWithValue("@admissionStage", registration.AdmissionStage);
                command.Parameters.AddWithValue("@FatherQualification", registration.FatherQualification);
                command.Parameters.AddWithValue("@MotherQualification", registration.MotherQualification);
                command.Parameters.AddWithValue("@FatherOccupation", registration.FatherOccupation);
                command.Parameters.AddWithValue("@MotherOccupation", registration.MotherOccupation);
                command.Parameters.AddWithValue("@FatherOfficeAddress", registration.FatherOfficeAddress);
                command.Parameters.AddWithValue("@MotherOfficeAddress", registration.MotherOfficeAddress);
                command.Parameters.AddWithValue("@FatherOfficeNo", registration.FatherOfficeMobileNo);
                command.Parameters.AddWithValue("@MotherOfficeNo", registration.MotherOfficeMobileNo);

                if (registration.StudentPhoto != null)
                {
                    command.Parameters.AddWithValue("@StudentPhoto", sphoto);
                }
                if (registration.FatherPhoto != null)
                    command.Parameters.AddWithValue("@FatherPhoto", fphoto);

                if (registration.MotherPhoto != null)
                    command.Parameters.AddWithValue("@MotherPhoto", mphoto);

                if (registration.StudentAadharPhoto != null)
                    command.Parameters.AddWithValue("@StudentAadharPhoto", aphoto);

                int id = 0;
                string eres = command.ExecuteScalar()?.ToString();
                bool parseres = int.TryParse(eres, out id);
                if (parseres) id = Convert.ToInt32(eres);

                if (id > 0)
                {
                    //insert optional subject daata
                    if (registration.OptionalSubjectIds != null && registration.OptionalSubjectIds.Any())
                    {
                        foreach (int subjectId in registration.OptionalSubjectIds)
                        {
                            SqlCommand insertCmd = new SqlCommand(@"
                INSERT INTO tbl_StudentOptionalSubject
                (streamId, studentId, classId, subjectId, status, createdOn)
                VALUES (@streamId, @studentId, @classId, @subjectId, 1, GETDATE())", connection, transaction);

                            insertCmd.Parameters.AddWithValue("@streamId", registration.classStreamId);
                            insertCmd.Parameters.AddWithValue("@studentId", id); // returned from ExecuteScalar()
                            insertCmd.Parameters.AddWithValue("@classId", registration.ClassId);
                            insertCmd.Parameters.AddWithValue("@subjectId", subjectId);

                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    if (registration.AdmissionStage == "admitted")
                    {
                        string studentUsername = registration.StudentName.Trim().Split(' ')[0].Contains('.') ? registration.StudentName.Split('.')[1].Trim().Split(' ')[0] : registration.StudentName.Split(' ')[0];

                        string mobilePart = registration.MobileNo.ToString();
                        mobilePart = mobilePart.Substring(6);

                        string username = $"{studentUsername}@{mobilePart}";
                        string randomCharacter = "ABCDEFGHIJKLMNOPQRSTUVWabcdefghijklmnopqrst1234567890";
                        Random rmd = new Random();
                        string password = string.Empty;
                        for (int i = 0; i < 6; i++)
                        {
                            int rcount = rmd.Next(randomCharacter.Length - 1);
                            password += randomCharacter[rcount];
                        }

                        command = new SqlCommand("sp_loginmanager", connection, transaction);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@action", "insertlogincredential");
                        command.Parameters.AddWithValue("@userId", id);
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@role", "student");

                        int res = command.ExecuteNonQuery();
                        if (res > 0)
                        {
                            string subject = "Login Credential";
                            string body = $" <p>We are pleased to inform you that your account has been successfully created on the portal.</p><p><strong>Your login credentials are as follows:</strong></p><ul><li><strong>Username:</strong> {username} </li><li><strong>Password:</strong> {password} </li></ul>";
                            CommonMessage mailres = _mail.SendEmail(subject, body, registration.StudentEmail);
                            if (mailres.status)
                            {

                                command = new SqlCommand("sp_loginmanager", connection, transaction);
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@action", "insertlogincredential");
                                command.Parameters.AddWithValue("@userId", id);
                                command.Parameters.AddWithValue("@username", registration.FatherOfficeMobileNo > 0 ? registration.FatherOfficeMobileNo : registration.MotherOfficeMobileNo);
                                command.Parameters.AddWithValue("@password", password);
                                command.Parameters.AddWithValue("@role", "parent");

                                object res2 = command.ExecuteScalar();
                                if (res2 != null && res2?.ToString() != "exist")
                                {
                                    string body2 = $" <p>We are pleased to inform you that your account has been successfully created on the portal.</p><p><strong>Your login credentials are as follows:</strong></p><ul><li><strong>Username:</strong> {(registration.FatherOfficeMobileNo > 0 ? registration.FatherOfficeMobileNo : registration.MotherOfficeMobileNo)} </li><li><strong>Password:</strong> {password} </li></ul>";
                                    CommonMessage mailres2 = _mail.SendEmail(subject, body, registration.parentEmail);
                                    if (mailres2.status)
                                    {
                                        transaction.Commit();
                                        return true;
                                    }
                                }
                                else if (res2?.ToString() == "exist")
                                {
                                    transaction.Commit();
                                    return true;
                                }
                            }
                        }
                        transaction.Rollback();
                        errorMessage = "Some error occured!";
                        return false;
                    }
                    transaction.Commit();
                    return true;
                }
                errorMessage = "Some error occured!";
                transaction.Rollback();
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                transaction.Rollback();
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        // Helper method to check allowed extensions
        private bool IsAllowedFile(string fileName)
        {
            string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }




        public string DeleteStudent(int studentid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@StudentId", studentid);
                connection.Open();

                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                connection.Close();
            }


            return result;
        }

        private bool IsValidFileExtension(string fileName)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
            string extension = Path.GetExtension(fileName)?.ToLower();
            return allowedExtensions.Contains(extension);
        }

        public bool UpdateStudent(RegistrationModel registration, out string errorMessage)
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            try
            {
                string sphoto = null;
                string fphoto = null;
                string mphoto = null;
                string aphoto = null;

                // Validate and upload StudentPhoto
                if (registration.StudentPhoto != null)
                {
                    if (!IsValidFileExtension(registration.StudentPhoto.FileName))
                    {
                        errorMessage = "Invalid Student Photo file type. Allowed types: .jpg, .jpeg, .png, .pdf";
                        transaction.Rollback();
                        connection.Close();
                        return false;
                    }
                    sphoto = UploadImageToServer(registration.StudentPhoto);
                }

                // Validate and upload FatherPhoto
                if (registration.FatherPhoto != null)
                {
                    if (!IsValidFileExtension(registration.FatherPhoto.FileName))
                    {
                        errorMessage = "Invalid Father Photo file type. Allowed types: .jpg, .jpeg, .png, .pdf";
                        transaction.Rollback();
                        connection.Close();
                        return false;
                    }
                    fphoto = UploadImageToServer(registration.FatherPhoto);
                }

                // Validate and upload MotherPhoto
                if (registration.MotherPhoto != null)
                {
                    if (!IsValidFileExtension(registration.MotherPhoto.FileName))
                    {
                        errorMessage = "Invalid Mother Photo file type. Allowed types: .jpg, .jpeg, .png, .pdf";
                        transaction.Rollback();
                        connection.Close();
                        return false;
                    }
                    mphoto = UploadImageToServer(registration.MotherPhoto);
                }

                // Validate and upload StudentAadharPhoto
                if (registration.StudentAadharPhoto != null)
                {
                    if (!IsValidFileExtension(registration.StudentAadharPhoto.FileName))
                    {
                        errorMessage = "Invalid Aadhar Photo file type. Allowed types: .jpg, .jpeg, .png, .pdf";
                        transaction.Rollback();
                        connection.Close();
                        return false;
                    }
                    aphoto = UploadImageToServer(registration.StudentAadharPhoto);
                }

                errorMessage = "";
                SqlCommand command = new SqlCommand("sp_StudentRegistrationManagement", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "UPDATE");
                command.Parameters.AddWithValue("@userid", registration.userid);
                command.Parameters.AddWithValue("@StudentId", registration.StudentId);
                command.Parameters.AddWithValue("@StudentName", registration.StudentName);
                command.Parameters.AddWithValue("@MotherTougue", registration.MotherTongue);
                command.Parameters.AddWithValue("@ClassId", registration.ClassId);
                command.Parameters.AddWithValue("@SectionId", registration.SectionId);
                command.Parameters.AddWithValue("@Gender", registration.Gender);
                command.Parameters.AddWithValue("@Religion", registration.Religion);
                command.Parameters.AddWithValue("@Caste", registration.Caste);
                command.Parameters.AddWithValue("@PlaceOfBirth", registration.PlaceOfBirth);
                command.Parameters.AddWithValue("@DOB", registration.DOB);
                command.Parameters.AddWithValue("@StateId", registration.StateId);
                command.Parameters.AddWithValue("@CityId", registration.CityId);
                command.Parameters.AddWithValue("@DateOfAdmission", registration.DateOfAdmission);
                command.Parameters.AddWithValue("@MobileNo", registration.MobileNo);
                command.Parameters.AddWithValue("@BloodGroup", registration.BloodGroup);
                command.Parameters.AddWithValue("@Address", registration.Address);
                command.Parameters.AddWithValue("@CurrentAddress", registration.CurrentAddress);
                command.Parameters.AddWithValue("@Hobbies", registration.Hobbies);
                command.Parameters.AddWithValue("@StudentEmail", registration.StudentEmail);
                command.Parameters.AddWithValue("@Nationality", registration.Nationality);
                command.Parameters.AddWithValue("@AadharNumber", registration.AadharNo);
                command.Parameters.AddWithValue("@LastSchoolAttended", registration.LastSchoolAttended);
                command.Parameters.AddWithValue("@YearOfPassing", registration.YearOfPassing);
                command.Parameters.AddWithValue("@ObtainedMarks", registration.ObtainedMarks);
                command.Parameters.AddWithValue("@TotalMarks", registration.TotalMarks);
                command.Parameters.AddWithValue("@FatherName", registration.FatherName);
                command.Parameters.AddWithValue("@MotherName", registration.MotherName);
                command.Parameters.AddWithValue("@academicyear", registration.AcademicYear);
                command.Parameters.AddWithValue("@admissionstage", registration.AdmissionStage);
                command.Parameters.AddWithValue("@FatherQualification", registration.FatherQualification);
                command.Parameters.AddWithValue("@MotherQualification", registration.MotherQualification);
                command.Parameters.AddWithValue("@FatherOccupation", registration.FatherOccupation);
                command.Parameters.AddWithValue("@MotherOccupation", registration.MotherOccupation);
                command.Parameters.AddWithValue("@parentEmail", registration.parentEmail);
                command.Parameters.AddWithValue("@FatherOfficeAddress", registration.FatherOfficeAddress);
                command.Parameters.AddWithValue("@MotherOfficeAddress", registration.MotherOfficeAddress);
                command.Parameters.AddWithValue("@FatherOfficeNo", registration.FatherOfficeMobileNo);
                command.Parameters.AddWithValue("@MotherOfficeNo", registration.MotherOfficeMobileNo);

                if (registration.StudentPhoto != null)
                    command.Parameters.AddWithValue("@StudentPhoto", sphoto);

                if (registration.FatherPhoto != null)
                    command.Parameters.AddWithValue("@FatherPhoto", fphoto);

                if (registration.MotherPhoto != null)
                    command.Parameters.AddWithValue("@MotherPhoto", mphoto);

                if (registration.StudentAadharPhoto != null)
                    command.Parameters.AddWithValue("@StudentAadharPhoto", aphoto);

                object result = command.ExecuteScalar();

                if (result != null)
                {
                    if (result.ToString() != "admitted" && registration.AdmissionStage == "admitted")
                    {
                        string schoolPart = registration.StudentName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                        schoolPart = schoolPart.Length >= 6 ? schoolPart.Substring(0, 6) : schoolPart.PadRight(6, 'x');

                        string mobilePart = registration.MobileNo.ToString();
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

                        command = new SqlCommand("sp_loginmanager", connection, transaction);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@action", "insertlogincredential");
                        command.Parameters.AddWithValue("@userId", registration.StudentId);
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@role", "student");

                        int res = command.ExecuteNonQuery();
                        if (res > 0)
                        {
                            string subject = "Login Credential";
                            string body = $" <p>We are pleased to inform you that your account has been successfully created on the portal.</p><p><strong>Your login credentials are as follows:</strong></p><ul><li><strong>Username:</strong> {username} </li><li><strong>Password:</strong> {password} </li></ul>";
                            CommonMessage mailres = _mail.SendEmail(subject, body, registration.StudentEmail);
                            if (mailres.status)
                            {
                                command = new SqlCommand("sp_loginmanager", connection, transaction);
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@action", "insertlogincredential");
                                command.Parameters.AddWithValue("@userId", registration.StudentId);
                                command.Parameters.AddWithValue("@username", registration.FatherOfficeMobileNo > 0 ? registration.FatherOfficeMobileNo : registration.MotherOfficeMobileNo);
                                command.Parameters.AddWithValue("@password", password);
                                command.Parameters.AddWithValue("@role", "parent");

                                object res2 = command.ExecuteScalar();
                                if (res2 != null && res2?.ToString() != "exist")
                                {
                                    string body2 = $" <p>We are pleased to inform you that your account has been successfully created on the portal.</p><p><strong>Your login credentials are as follows:</strong></p><ul><li><strong>Username:</strong> {(registration.FatherOfficeMobileNo > 0 ? registration.FatherOfficeMobileNo : registration.MotherOfficeMobileNo)} </li><li><strong>Password:</strong> {password} </li></ul>";
                                    CommonMessage mailres2 = _mail.SendEmail(subject, body, registration.parentEmail);
                                    if (mailres2.status)
                                    {
                                        transaction.Commit();
                                        return true;
                                    }
                                }
                                else if (res2?.ToString() == "exist")
                                {
                                    transaction.Commit();
                                    return true;
                                }
                            }
                        }
                        errorMessage = "Some error occured";
                        transaction.Rollback();
                        return false;
                    }
                    transaction.Commit();
                    return true;
                }

                errorMessage = "Some error occured";
                transaction.Rollback();
                return false;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                connection.Close();
            }
        }


        public List<SubjectModel> GetSubjectsByClassId(int classId)
        {
            List<SubjectModel> list = new List<SubjectModel>();
            try
            {
                SqlCommand command = new SqlCommand("sp_SubjectManagement", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Actions", "GetSubjectsByClassId");
                command.Parameters.AddWithValue("@ClassId", classId);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new SubjectModel
                    {
                        SubjectId = Convert.ToInt32(reader["SubjectId"]),
                        Subject = reader["Subject"].ToString()
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

        public List<SectionModel> GetSectionsByClassId(int classId)
        {
            List<SectionModel> list = new List<SectionModel>();
            try
            {
                SqlCommand command = new SqlCommand("sp_SectionManagement", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Actions", "GetSectionsByClassId");
                command.Parameters.AddWithValue("@ClassId", classId);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new SectionModel
                    {
                        SectionId = Convert.ToInt32(reader["SectionId"]),
                        SectionName = reader["SectionName"].ToString()
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

        public List<SubjectAssignModel> GetAllSubjectAssigned(int id)
        {
            List<SubjectAssignModel> list = new List<SubjectAssignModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_SubjectAssignManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetAll");
                cmd.Parameters.AddWithValue("@id", id);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new SubjectAssignModel
                    {
                        AssignedId = rdr["AssignedId"] != DBNull.Value ? Convert.ToInt32(rdr["AssignedId"]) : 0,
                        ClassName = rdr["ClassName"].ToString(),
                        SectionName = rdr["SectionName"].ToString(),
                        SubjectName = rdr["Subject"].ToString(),
                        ClassId = Convert.ToInt32(rdr["ClassId"]),
                        SectionId = Convert.ToInt32(rdr["SectionId"]),
                        SubjectId = Convert.ToInt32(rdr["SubjectId"]),
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

        public void InsertSubjectAssigned(SubjectAssignModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_SubjectAssignManagement", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Actions", "Insert");
                command.Parameters.AddWithValue("@teacherId", model.teacherId);
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@ClassId", model.ClassId);
                command.Parameters.AddWithValue("@SectionId", model.SectionId);
                command.Parameters.AddWithValue("@SubjectId", model.SubjectId);
                connection.Open();
                command.ExecuteNonQuery();
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

        public string DeleteSubjectAssigned(int assignid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_SubjectAssignManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "Delete");
                cmd.Parameters.AddWithValue("@AssignedId", assignid);
                connection.Open();

                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }
        #region Get subjectby id
        public List<SubjectAssignModel> GetSubjectAssignedById(int? id)
        {
            List<SubjectAssignModel> list = new List<SubjectAssignModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_SubjectAssignManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetById");
                cmd.Parameters.AddWithValue("@AssignedId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new SubjectAssignModel
                    {
                        AssignedId = rdr["AssignedId"] != DBNull.Value ? Convert.ToInt32(rdr["AssignedId"]) : 0,
                        ClassId = Convert.ToInt32(rdr["ClassId"]),
                        SectionId = Convert.ToInt32(rdr["SectionId"]),
                        SubjectId = Convert.ToInt32(rdr["SubjectId"]),
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

        public void UpdateSubjectAssigned(SubjectAssignModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_SubjectAssignManagement", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Actions", "Update");
                command.Parameters.AddWithValue("@AssignedId", model.AssignedId);
                command.Parameters.AddWithValue("@ClassId", model.ClassId);
                command.Parameters.AddWithValue("@SubjectId", model.SubjectId);
                command.Parameters.AddWithValue("@SectionId", model.SectionId);
                connection.Open();
                command.ExecuteNonQuery();
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

        public bool InsertTeacher(TeacherModel teacher, out string errorMessage)
        {
            string[] arr = new[] { ".jpg", ".jpeg", ".png",".pdf" };
            errorMessage = "";
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            string tenthmarksheet = null;
            string TwelfthMarksheet = null;
            string GraduationMarksheet = null;
            string PostGraduationMarksheet = null;
            string OtherDiplomaMarksheet = null;
            string ExperienceDocument = null;
            string profileImage = null;
            try
            {
                if (teacher.TenthMarksheet != null)
                {
                    if (arr.Contains(Path.GetExtension(teacher.TenthMarksheet.FileName)))
                        {
                        tenthmarksheet = UploadImageToServer(teacher.TenthMarksheet);
                    }
                    else
                    {
                        errorMessage = "Tenth Marksheet should be jpg, jpeg, png, pdf";
                        return false;
                    }
                }
                if (teacher.TwelfthMarksheet != null)
                {
                    if(arr.Contains(Path.GetExtension(teacher.TwelfthMarksheet.FileName)))
                    {
                        TwelfthMarksheet = UploadImageToServer(teacher.TwelfthMarksheet);
                    }
                    else
                    {
                        errorMessage = "Twelth Marksheet should be jpg, jpeg, png, pdf";
                        return false;
                    }
                }
                if (teacher.GraduationMarksheet != null)
                {
                    if(arr.Contains(Path.GetExtension(teacher.GraduationMarksheet.FileName)))
                    {
                        GraduationMarksheet = UploadImageToServer(teacher.GraduationMarksheet);
                    }
                    else
                    {
                        errorMessage = "Graduation Marksheet should be jpg, jpeg, png, pdf";
                        return false;
                    }
                }
                if (teacher.OtherDiplomaMarksheet != null)
                {
                    if (arr.Contains(Path.GetExtension(teacher.OtherDiplomaMarksheet.FileName)))
                    {
                        OtherDiplomaMarksheet = UploadImageToServer(teacher.OtherDiplomaMarksheet);
                    }
                    else
                    {
                        errorMessage = "OtherDiploma Marksheet should be jpg, jpeg, png, pdf";
                        return false;
                    }
                    
                }
                if (teacher.ExperienceDocument != null)
                {
                    if (arr.Contains(Path.GetExtension(teacher.ExperienceDocument.FileName)))
                    {
                        ExperienceDocument = UploadImageToServer(teacher.ExperienceDocument);
                    }
                    else
                    {
                        errorMessage = "Experience Document should be jpg, jpeg, png, pdf";
                        return false;
                    }
                    
                }
                if (teacher.PostGraduationMarksheet != null)
                {
                    if (arr.Contains(Path.GetExtension(teacher.PostGraduationMarksheet.FileName)))
                    {
                        PostGraduationMarksheet = UploadImageToServer(teacher.PostGraduationMarksheet);
                    }
                    else
                    {
                        errorMessage = "PostGraduation Marksheet  should be jpg, jpeg, png, pdf";
                        return false;
                    }
                   
                }
                if (teacher.profileImage != null)
                {
                    if (arr.Contains(Path.GetExtension(teacher.profileImage.FileName)))
                    {
                        profileImage = UploadImageToServer(teacher.profileImage);
                    }
                    else
                    {
                        errorMessage = "profile Image  should be jpg, jpeg, png, pdf";
                        return false;
                    }
                  
                }

                string trimmedName = teacher.TeacherName.Trim().Split(' ')[0].Contains('.')? teacher.TeacherName.Split('.')[1].Trim().Split(' ')[0]: teacher.TeacherName.Trim().Split(' ')[0];
                string namePart = (trimmedName.Length >= 6
                    ? trimmedName.Substring(0, 6)
                    : trimmedName.PadRight(6, 'x'));

                string mobileNumPart = teacher.TeacherMobile.Substring(4);
             

                string registrationno = namePart + mobileNumPart + DateTime.Now.Year;
                SqlCommand command = new SqlCommand("sp_TeacherRegistrationManagement", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Actions", teacher.TeacherId > 0 ? "UpdateTeacher" : "InsertTeacher");
                command.Parameters.AddWithValue("@RegistrationNo", registrationno);
                command.Parameters.AddWithValue("@EmployeeId", teacher.EmployeeId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TeacherName", teacher.TeacherName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TeacherId", teacher.TeacherId);
                command.Parameters.AddWithValue("@TeacherEmail", teacher.TeacherEmail ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TeacherMobile", teacher.TeacherMobile ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TeacherDOB", teacher.TeacherDOB);
                command.Parameters.AddWithValue("@Gender", teacher.Gender ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MotherTongue", teacher.MotherTongue ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@BloodGroup", teacher.BloodGroup ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nationality", teacher.Nationality ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Religion", teacher.Religion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MaritalStatus", teacher.MaritalStatus ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PanNo", teacher.PanNo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Experience", teacher.Experience);
                command.Parameters.AddWithValue("@Subject", teacher.Subject ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@JoinDate", teacher.JoinDate);
                command.Parameters.AddWithValue("@LastSchoolName", teacher.LastSchoolName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastEmpAddress", teacher.LastEmpAddress ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Caste", teacher.Caste ?? (object)DBNull.Value);

                command.Parameters.AddWithValue("@ExperienceDocument", ExperienceDocument);
                command.Parameters.AddWithValue("@designation", teacher.designation);

                command.Parameters.AddWithValue("@TenthBoard", teacher.TenthBoard ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TenthPassoutYear", teacher.TenthPassoutYear ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TenthPercent", teacher.TenthPercent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TenthMarksheet", tenthmarksheet);

                command.Parameters.AddWithValue("@TwelfthBoard", teacher.TwelfthBoard ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TwelfthPassoutYear", teacher.TwelfthPassoutYear ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TwelfthPercent", teacher.TwelfthPercent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TwelfthMarksheet", TwelfthMarksheet);

                command.Parameters.AddWithValue("@GraduationDegree", teacher.GraduationDegree ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@GraduationStream", teacher.GraduationStream ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@GraduationYear", teacher.GraduationYear ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@GraduationPercent", teacher.GraduationPercent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@GraduationMarksheet", GraduationMarksheet);

                command.Parameters.AddWithValue("@PostGraduationDegree", teacher.PostGraduationDegree ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PostGraduationStream", teacher.PostGraduationStream ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PostGraduationYear", teacher.PostGraduationYear ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PostGraduationPercent", teacher.PostGraduationPercent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PostGraduationmarksheetpath", PostGraduationMarksheet);
                command.Parameters.AddWithValue("@profileImage", profileImage);

                command.Parameters.AddWithValue("@OtherDiplomaDegree", teacher.OtherDiplomaDegree ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@OtherDiplomaStream", teacher.OtherDiplomaStream ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@OtherDiplomaYear", teacher.OtherDiplomaYear ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@OtherDiplomaPercent", teacher.OtherDiplomaPercent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@OtherDiplomaMarksheet", OtherDiplomaMarksheet);

                command.Parameters.AddWithValue("@BankName", teacher.BankName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AccountHolderName", teacher.AccountHolderName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@BankAccountNumber", teacher.BankAccountNumber);
                command.Parameters.AddWithValue("@ReenterBankAccountNumber", teacher.ReenterBankAccountNumber);
                command.Parameters.AddWithValue("@IfscCode", teacher.IfscCode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@address", teacher.address ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@userid", teacher.userid);


                int id = 0;
                string eres = command.ExecuteScalar()?.ToString();
                bool parseres = int.TryParse(eres, out id);
                if (parseres) id = Convert.ToInt32(eres);

                if (id > 0 && teacher.TeacherId <= 0)
                {
                  
                    string teacherUsername = teacher.TeacherName.Trim().Split(' ')[0].Contains('.') ? teacher.TeacherName.Split('.')[1].Trim().Split(' ')[0] : teacher.TeacherName.Split(' ')[0];

                    string mobilePart = teacher.TeacherMobile.ToString();
                    mobilePart = mobilePart.Substring(6);

                    string username = $"{teacherUsername}@{mobilePart}";
                    string randomCharacter = "ABCDEFGHIJKLMNOPQRSTUVWabcdefghijklmnopqrst1234567890";
                    Random rmd = new Random();
                    string password = string.Empty;
                    for (int i = 0; i < 6; i++)
                    {
                        int rcount = rmd.Next(randomCharacter.Length - 1);
                        password += randomCharacter[rcount];
                    }

                    command = new SqlCommand("sp_loginmanager", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@action", "insertlogincredential");
                    command.Parameters.AddWithValue("@userId", id);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@role", "teacher");

                    int res = command.ExecuteNonQuery();
                    if (res > 0)
                    {
                        string subject = "Login Credential";
                        string body = $" <p>We are pleased to inform you that your account has been successfully created on the portal.</p><p><strong>Your login credentials are as follows:</strong></p><ul><li><strong>Username:</strong> {username} </li><li><strong>Password:</strong> {password} </li></ul>";
                        CommonMessage mailres = _mail.SendEmail(subject, body, teacher.TeacherEmail);
                        if (mailres.status)
                        {
                            transaction.Commit();
                            return true;
                        }
                        transaction.Rollback();
                        errorMessage = "Something Went wrong";
                        return false;
                    }

                }
                else if (teacher.TeacherId > 0 && id > 0)
                {
                    transaction.Commit();
                    return true;
                }

                if (!string.IsNullOrEmpty(tenthmarksheet))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + tenthmarksheet);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(ExperienceDocument))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + ExperienceDocument);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(TwelfthMarksheet))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + TwelfthMarksheet);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(GraduationMarksheet))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + GraduationMarksheet);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(PostGraduationMarksheet))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + PostGraduationMarksheet);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(OtherDiplomaMarksheet))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + OtherDiplomaMarksheet);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(profileImage))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + profileImage);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                transaction.Rollback();
                errorMessage = "some error occurred";
                return false;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(tenthmarksheet))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + tenthmarksheet);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(ExperienceDocument))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + ExperienceDocument);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(TwelfthMarksheet))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + TwelfthMarksheet);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(GraduationMarksheet))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + GraduationMarksheet);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(PostGraduationMarksheet))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + PostGraduationMarksheet);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(OtherDiplomaMarksheet))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + OtherDiplomaMarksheet);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                if (!string.IsNullOrEmpty(profileImage))
                {
                    string path = HttpContext.Current.Server.MapPath("~" + profileImage);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                transaction.Rollback();
                errorMessage = ex.Message;
                return false;
            }
          
            finally
            {
                connection.Close();
            }
        }

        public List<TeacherModel> GetAllTeachers(int id,int? academicYear=null)
        {
            List<TeacherModel> list = new List<TeacherModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_TeacherRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "SelectAllTeacher");
                cmd.Parameters.Add("@academicYear", SqlDbType.Int).Value = (object)academicYear ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@userid", id);
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new TeacherModel
                    {
                        TeacherId = Convert.ToInt32(rdr["TeacherId"] ?? 0),
                        RegistrationNo = rdr["RegistrationNo"]?.ToString(),
                        EmployeeId = rdr["EmployeeId"]?.ToString(),
                        TeacherName = rdr["TeacherName"]?.ToString(),
                        TeacherEmail = rdr["TeacherEmail"]?.ToString(),
                        TeacherMobile = rdr["TeacherMobile"]?.ToString(),
                        TeacherDOB = rdr["TeacherDOB"] != DBNull.Value ? Convert.ToDateTime(rdr["TeacherDOB"]) : (DateTime?)null,
                        Gender = rdr["Gender"]?.ToString(),
                        MotherTongue = rdr["MotherTongue"]?.ToString(),
                        BloodGroup = rdr["BloodGroup"]?.ToString(),
                        Nationality = rdr["Nationality"]?.ToString(),
                        Religion = rdr["Religion"]?.ToString(),
                        MaritalStatus = rdr["MaritalStatus"]?.ToString(),
                        PanNo = rdr["PanNo"]?.ToString(),
                        Experience = Convert.ToInt32(rdr["Experience"] ?? 0),
                        Subject = rdr["Subject"]?.ToString(),
                        JoinDate = rdr["JoinDate"] != DBNull.Value ? Convert.ToDateTime(rdr["JoinDate"]) : (DateTime?)null,
                        LastSchoolName = rdr["LastSchoolName"]?.ToString(),
                        LastEmpAddress = rdr["LastEmpAddress"]?.ToString(),
                        Caste = rdr["Caste"]?.ToString(),
                        ExperienceDocumentPath = rdr["ExperienceDocumentPath"]?.ToString(),

                        TenthBoard = rdr["TenthBoard"]?.ToString(),
                        TenthPassoutYear = rdr["TenthPassoutYear"] != DBNull.Value ? rdr["TenthPassoutYear"]?.ToString() : null,
                        TenthPercent = rdr["TenthPercent"]?.ToString(),
                        TenthMarksheetPath = rdr["TenthMarksheetPath"]?.ToString(),

                        TwelfthBoard = rdr["TwelfthBoard"]?.ToString(),
                        TwelfthPassoutYear = rdr["TwelfthPassoutYear"] != DBNull.Value ? rdr["TwelfthPassoutYear"]?.ToString() : null,
                        TwelfthPercent = rdr["TwelfthPercent"]?.ToString(),
                        TwelfthMarksheetPath = rdr["TwelfthMarksheetPath"]?.ToString(),

                        GraduationDegree = rdr["GraduationDegree"]?.ToString(),
                        GraduationStream = rdr["GraduationStream"]?.ToString(),
                        GraduationYear = rdr["GraduationYear"] != DBNull.Value ? rdr["GraduationYear"]?.ToString() : null,
                        GraduationPercent = rdr["GraduationPercent"]?.ToString(),
                        GraduationMarksheetPath = rdr["GraduationMarksheetPath"]?.ToString(),
                        designationName = rdr["DesignationName"]?.ToString(),

                        PostGraduationDegree = rdr["PostGraduationDegree"]?.ToString(),
                        PostGraduationStream = rdr["PostGraduationStream"]?.ToString(),
                        PostGraduationYear = rdr["PostGraduationYear"] != DBNull.Value ? rdr["PostGraduationYear"]?.ToString() : null,
                        PostGraduationPercent = rdr["PostGraduationPercent"]?.ToString(),

                        OtherDiplomaDegree = rdr["OtherDiplomaDegree"]?.ToString(),
                        OtherDiplomaStream = rdr["OtherDiplomaStream"]?.ToString(),
                        OtherDiplomaYear = rdr["OtherDiplomaYear"] != DBNull.Value ? rdr["OtherDiplomaYear"].ToString() : null,
                        OtherDiplomaPercent = rdr["OtherDiplomaPercent"]?.ToString(),
                        OtherDiplomaMarksheetPath = rdr["OtherDiplomaMarksheetPath"]?.ToString(),

                        BankName = rdr["BankName"]?.ToString(),
                        AccountHolderName = rdr["AccountHolderName"]?.ToString(),
                        BankAccountNumber = rdr["BankAccountNumber"] != DBNull.Value ? Convert.ToInt64(rdr["BankAccountNumber"]) : 0,
                        ReenterBankAccountNumber = rdr["ReenterBankAccountNumber"] != DBNull.Value ? Convert.ToInt64(rdr["ReenterBankAccountNumber"]) : 0,

                        IfscCode = rdr["IFSCCode"]?.ToString(),
                        username =rdr["Username"] !=DBNull.Value? rdr["Username"].ToString():null,
                        password = rdr["Password"] != DBNull.Value ? rdr["Password"].ToString():null

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

        public TeacherModel GetTeachersByTeacherId(int id)
        {
            TeacherModel list = new TeacherModel();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_TeacherRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "SelectTeacherById");
                cmd.Parameters.AddWithValue("@teacherId", id);
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list = new TeacherModel
                    {
                        TeacherId = Convert.ToInt32(rdr["TeacherId"] ?? 0),
                        EmployeeId = rdr["EmployeeId"]?.ToString(),
                        RegistrationNo = rdr["RegistrationNo"]?.ToString(),
                        TeacherName = rdr["TeacherName"]?.ToString(),
                        TeacherEmail = rdr["TeacherEmail"]?.ToString(),
                        TeacherMobile = rdr["TeacherMobile"]?.ToString(),
                        TeacherDOB = rdr["TeacherDOB"] != DBNull.Value ? Convert.ToDateTime(rdr["TeacherDOB"]) : (DateTime?)null,
                        Gender = rdr["Gender"]?.ToString(),
                        MotherTongue = rdr["MotherTongue"]?.ToString(),
                        BloodGroup = rdr["BloodGroup"]?.ToString(),
                        Nationality = rdr["Nationality"]?.ToString(),
                        Religion = rdr["Religion"]?.ToString(),
                        MaritalStatus = rdr["MaritalStatus"]?.ToString(),
                        PanNo = rdr["PanNo"]?.ToString(),
                        Experience = Convert.ToInt32(rdr["Experience"] ?? 0),
                        Subject = rdr["Subject"]?.ToString(),
                        JoinDate = rdr["JoinDate"] != DBNull.Value ? Convert.ToDateTime(rdr["JoinDate"]) : (DateTime?)null,
                        LastSchoolName = rdr["LastSchoolName"]?.ToString(),
                        LastEmpAddress = rdr["LastEmpAddress"]?.ToString(),
                        Caste = rdr["Caste"]?.ToString(),
                        ExperienceDocumentPath = rdr["ExperienceDocumentPath"]?.ToString(),

                        TenthBoard = rdr["TenthBoard"]?.ToString(),
                        designation = Convert.ToInt32(rdr["designation"] != DBNull.Value ? rdr["designation"] : 0),
                        designationName = rdr["DesignationName"]?.ToString(),
                        TenthPassoutYear = rdr["TenthPassoutYear"] != DBNull.Value ? rdr["TenthPassoutYear"]?.ToString() : null,
                        TenthPercent = rdr["TenthPercent"]?.ToString(),
                        TenthMarksheetPath = rdr["TenthMarksheetPath"]?.ToString(),

                        TwelfthBoard = rdr["TwelfthBoard"]?.ToString(),
                        TwelfthPassoutYear = rdr["TwelfthPassoutYear"] != DBNull.Value ? rdr["TwelfthPassoutYear"]?.ToString() : null,
                        TwelfthPercent = rdr["TwelfthPercent"]?.ToString(),
                        TwelfthMarksheetPath = rdr["TwelfthMarksheetPath"]?.ToString(),

                        GraduationDegree = rdr["GraduationDegree"]?.ToString(),
                        GraduationStream = rdr["GraduationStream"]?.ToString(),
                        GraduationYear = rdr["GraduationYear"] != DBNull.Value ? rdr["GraduationYear"]?.ToString() : null,
                        GraduationPercent = rdr["GraduationPercent"]?.ToString(),
                        GraduationMarksheetPath = rdr["GraduationMarksheetPath"]?.ToString(),

                        PostGraduationDegree = rdr["PostGraduationDegree"]?.ToString(),
                        PostGraduationStream = rdr["PostGraduationStream"]?.ToString(),
                        PostGraduationYear = rdr["PostGraduationYear"] != DBNull.Value ? rdr["PostGraduationYear"]?.ToString() : null,
                        PostGraduationPercent = rdr["PostGraduationPercent"]?.ToString(),
                        PostGraduationMarksheetPath = rdr["PostGraduationmarksheetpath"]?.ToString(),
                        profileImagePath = rdr["profileImage"]?.ToString(),

                        OtherDiplomaDegree = rdr["OtherDiplomaDegree"]?.ToString(),
                        OtherDiplomaStream = rdr["OtherDiplomaStream"]?.ToString(),
                        OtherDiplomaYear = rdr["OtherDiplomaYear"] != DBNull.Value ? rdr["OtherDiplomaYear"].ToString() : null,
                        OtherDiplomaPercent = rdr["OtherDiplomaPercent"]?.ToString(),
                        OtherDiplomaMarksheetPath = rdr["OtherDiplomaMarksheetPath"]?.ToString(),
                        address = rdr["address"]?.ToString(),

                        BankName = rdr["BankName"]?.ToString(),
                        AccountHolderName = rdr["AccountHolderName"]?.ToString(),
                        BankAccountNumber = rdr["BankAccountNumber"] != DBNull.Value ? Convert.ToInt64(rdr["BankAccountNumber"]) : 0,
                        ReenterBankAccountNumber = rdr["ReenterBankAccountNumber"] != DBNull.Value ? Convert.ToInt64(rdr["ReenterBankAccountNumber"]) : 0,

                        IfscCode = rdr["IFSCCode"]?.ToString(),
                        userid = Convert.ToInt32(rdr["userid"])

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
            return list;
        }

        public string usernameByuserid(int id,string role)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_loginmanager", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "getusernamebyid");
                cmd.Parameters.AddWithValue("@role", role);
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        return rd["username"]?.ToString();
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public string DeleteTeacher(int TeacherId)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_TeacherRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "DeleteTeacher");
                cmd.Parameters.AddWithValue("@TeacherId", TeacherId);
                connection.Open();

                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public List<AcademicVacationModel> GetAllAcademicVacation(int userid)
        {
            List<AcademicVacationModel> list = new List<AcademicVacationModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageAcademicVacation", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTAll");
                cmd.Parameters.AddWithValue("@userid", userid);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new AcademicVacationModel
                    {
                        VacationId = rdr["VacationId"] != DBNull.Value ? Convert.ToInt32(rdr["VacationId"]) : 0,
                        ClassName = rdr["ClassName"].ToString(),
                        VacationName = rdr["VacationName"].ToString(),
                        Day = rdr["Day"].ToString(),
                        VacationType = rdr["Type"].ToString(),
                        Image = rdr["Image"].ToString(),
                        Date = rdr["VacationDate"] != DBNull.Value ? Convert.ToDateTime(rdr["VacationDate"]) : DateTime.MinValue,
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

        public void InsertAcademicVacation(AcademicVacationModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageAcademicVacation", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@ClassId", model.ClassName);
                command.Parameters.AddWithValue("@VacationName", model.VacationName);
                command.Parameters.AddWithValue("@VacationDate", model.Date);
                command.Parameters.AddWithValue("@Type", model.VacationType);
                command.Parameters.AddWithValue("@Day", model.Day);
                command.Parameters.AddWithValue("@Image", model.Image ?? "");
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex) { throw ex; }
            finally
            {
                connection.Close();
            }
        }

        public string DeleteAcademicVacation(int vacid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageAcademicVacation", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@VacationId", vacid);
                connection.Open();

                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public List<AcademicVacationModel> GetAcademicVacationById(int? id)
        {

            List<AcademicVacationModel> list = new List<AcademicVacationModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageAcademicVacation", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@VacationId", Convert.ToInt32(id));
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new AcademicVacationModel
                    {
                        VacationId = rdr["VacationId"] != DBNull.Value ? Convert.ToInt32(rdr["VacationId"]) : 0,
                        ClassName = rdr["ClassId"].ToString(),
                        VacationName = rdr["VacationName"].ToString(),
                        Day = rdr["Day"].ToString(),
                        VacationType = rdr["Type"].ToString(),
                        Image = rdr["Image"].ToString(),
                        Date = rdr["Date"] != DBNull.Value ? Convert.ToDateTime(rdr["Date"]) : DateTime.MinValue,
                    });
                }
            }
            catch (Exception ex) { throw ex; }
            finally
            {
                connection.Close();
            }

            return list;
        }

        public void UpdateAcademicVacation(AcademicVacationModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_SubjectAssignManagement", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "UPDATE");
                command.Parameters.AddWithValue("@VacationId", model.VacationId);
                command.Parameters.AddWithValue("@ClassId", model.ClassId);
                command.Parameters.AddWithValue("@VacationName", model.VacationName);
                command.Parameters.AddWithValue("@VacationDate", model.Date);
                command.Parameters.AddWithValue("@Type", model.VacationType);
                command.Parameters.AddWithValue("@Image", model.Image);

                connection.Open();
                command.ExecuteNonQuery();
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

        public List<FestivalHoliday> GetAllFestivalHoliday(int userid)
        {
            List<FestivalHoliday> list = new List<FestivalHoliday>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageFestivalHoliday", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTAll");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new FestivalHoliday
                    {
                        FestivalId = rdr["FestivalId"] != DBNull.Value ? Convert.ToInt32(rdr["FestivalId"]) : 0,
                        FestivalName = rdr["FestivalName"].ToString(),
                        Day = rdr["Day"].ToString(),
                        StartDate = rdr["StartDate"] != DBNull.Value ? Convert.ToDateTime(rdr["StartDate"]) : DateTime.MinValue,
                        EndDate = rdr["EndDate"] != DBNull.Value ? Convert.ToDateTime(rdr["EndDate"]) : DateTime.MinValue,
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

        public void InsertFestivalHoliday(FestivalHoliday model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageFestivalHoliday", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@FestivalName", model.FestivalName);
                command.Parameters.AddWithValue("@Day", model.Day);
                command.Parameters.AddWithValue("@StartDate", model.StartDate);
                command.Parameters.AddWithValue("@EndDate", model.EndDate);

                connection.Open();
                command.ExecuteNonQuery();
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

        public string DeleteFestivalHoliday(int festid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageFestivalHoliday", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@FestivalId", festid);

                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return result;
        }

        public List<FestivalHoliday> GetFestivalHolidayById(int? id)
        {
            List<FestivalHoliday> list = new List<FestivalHoliday>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageFestivalHoliday", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@FestivalId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new FestivalHoliday
                    {
                        FestivalId = rdr["FestivalId"] != DBNull.Value ? Convert.ToInt32(rdr["FestivalId"]) : 0,
                        FestivalName = rdr["FestivalName"].ToString(),
                        Day = rdr["Day"].ToString(),
                        StartDate = rdr["StartDate"] != DBNull.Value ? Convert.ToDateTime(rdr["StartDate"]) : DateTime.MinValue,
                        EndDate = rdr["EndDate"] != DBNull.Value ? Convert.ToDateTime(rdr["EndDate"]) : DateTime.MinValue,
                    });
                }

            }
            catch (Exception ex) { throw ex; }
            finally
            {
                connection.Close();
            }
            return list;
        }

        public void UpdateFestivalHoliday(FestivalHoliday model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageFestivalHoliday", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "UPDATE");
                command.Parameters.AddWithValue("@FestivalId", model.FestivalId);
                command.Parameters.AddWithValue("@FestivalName", model.FestivalName);
                command.Parameters.AddWithValue("@Day", model.Day);
                command.Parameters.AddWithValue("@StartDate", model.StartDate);
                command.Parameters.AddWithValue("@EndDate", model.EndDate);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<RegistrationFeeModel> GetAllRegistrationFee(int userid)
        {
            List<RegistrationFeeModel> list = new List<RegistrationFeeModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageRegistrationFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTALL");
                cmd.Parameters.AddWithValue("@userid", userid);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new RegistrationFeeModel
                    {
                        RegistrationId = rdr["RegistrationId"] != DBNull.Value ? Convert.ToInt32(rdr["RegistrationId"]) : 0,
                        ClassName = rdr["ClassName"].ToString(),
                        RegistrationFee = Convert.ToInt32(rdr["RegistrationFee"])
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

        public void InsertRegistrationFee(RegistrationFeeModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageRegistrationFee", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@ClassId", model.ClassName);
                command.Parameters.AddWithValue("@RegistrationFee", model.RegistrationFee);

                connection.Open();

                command.ExecuteNonQuery();
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
        public string DeleteRegistrationFee(int? feeid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageRegistrationFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@RegistrationId", feeid);
                connection.Open();

                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return result;
        }
        public List<RegistrationFeeModel> GetRegistrationFeeById(int? id)
        {
            List<RegistrationFeeModel> list = new List<RegistrationFeeModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageRegistrationFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@RegistrationId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new RegistrationFeeModel
                    {
                        RegistrationId = rdr["RegistrationId"] != DBNull.Value ? Convert.ToInt32(rdr["RegistrationId"]) : 0,
                        ClassName = rdr["ClassId"].ToString(),
                        RegistrationFee = Convert.ToInt32(rdr["RegistrationFee"]),
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


        public void UpdateRegistrationFee(RegistrationFeeModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageRegistrationFee", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "UPDATE");
                command.Parameters.AddWithValue("@RegistrationId", model.RegistrationId);
                command.Parameters.AddWithValue("@ClassId", model.ClassName);
                command.Parameters.AddWithValue("@RegistrationFee", model.RegistrationFee);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<CollegeFeeModel> GetAllCollegeFee(int userid)
        {
            List<CollegeFeeModel> list = new List<CollegeFeeModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageSchoolFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECT");
                cmd.Parameters.AddWithValue("@userid", userid);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new CollegeFeeModel
                    {
                        FeeId = rdr["FeeId"] != DBNull.Value ? Convert.ToInt32(rdr["FeeId"]) : 0,
                        ClassName = rdr["ClassName"].ToString(),
                        BillingPeriod = rdr["BillingPeriod"].ToString(),
                        AdmissionFee = Convert.ToDecimal(rdr["AdmissionFee"]),
                        BuildingFee = Convert.ToDecimal(rdr["BuildingFee"]),
                        TutionFee = Convert.ToDecimal(rdr["TutionFee"]),
                        SportsFee = Convert.ToDecimal(rdr["SportsFee"]),
                        LibraryFee = Convert.ToDecimal(rdr["LibraryFee"]),
                        ActivityFee = Convert.ToDecimal(rdr["ActivityFee"]),
                        AnnualCharge = Convert.ToDecimal(rdr["AnnualCharge"]),
                        TotalFee = Convert.ToDecimal(rdr["TotalFee"]),
                    });
                }
            }
            catch (Exception ex)
            {
                return list;
            }
            finally
            {
                connection.Close();
            }
            return list;
        }

        public void InsertCollegeFee(CollegeFeeModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageSchoolFee", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@ClassId", model.ClassName);
                command.Parameters.AddWithValue("@BillingPeriod", model.BillingPeriod);
                command.Parameters.AddWithValue("@AdmissionFee", model.AdmissionFee);
                command.Parameters.AddWithValue("@BuildingFee", model.BuildingFee);
                command.Parameters.AddWithValue("@TutionFee", model.TutionFee);
                command.Parameters.AddWithValue("@SportsFee", model.SportsFee);
                command.Parameters.AddWithValue("@LibraryFee", model.LibraryFee);
                command.Parameters.AddWithValue("@ActivityFee", model.ActivityFee);
                command.Parameters.AddWithValue("@AnnualCharge", model.AnnualCharge);
                command.Parameters.AddWithValue("@TotalFee", model.TotalFee);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<CollegeFeeModel> GetCollegeFeeById(int? id)
        {
            List<CollegeFeeModel> list = new List<CollegeFeeModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageSchoolFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@FeeId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new CollegeFeeModel
                    {
                        FeeId = rdr["FeeId"] != DBNull.Value ? Convert.ToInt32(rdr["FeeId"]) : 0,
                        ClassName = rdr["ClassId"].ToString(),
                        BillingPeriod = rdr["BillingPeriod"].ToString(),
                        AdmissionFee = Convert.ToDecimal(rdr["AdmissionFee"]),
                        BuildingFee = Convert.ToDecimal(rdr["BuildingFee"]),
                        TutionFee = Convert.ToDecimal(rdr["TutionFee"]),
                        SportsFee = Convert.ToDecimal(rdr["SportsFee"]),
                        LibraryFee = Convert.ToDecimal(rdr["LibraryFee"]),
                        ActivityFee = Convert.ToDecimal(rdr["ActivityFee"]),
                        AnnualCharge = Convert.ToDecimal(rdr["AnnualCharge"]),
                        TotalFee = Convert.ToDecimal(rdr["TotalFee"]),
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
        public string DeleteCollegeFee(int? feeid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageSchoolFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@FeeId", feeid);

                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public void UpdateCollegeFee(CollegeFeeModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageSchoolFee", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "UPDATE");
                command.Parameters.AddWithValue("@FeeId", model.FeeId);
                command.Parameters.AddWithValue("@ClassId", model.ClassName);
                command.Parameters.AddWithValue("@BillingPeriod", model.BillingPeriod);
                command.Parameters.AddWithValue("@AdmissionFee", model.AdmissionFee);
                command.Parameters.AddWithValue("@BuildingFee", model.BuildingFee);
                command.Parameters.AddWithValue("@TutionFee", model.TutionFee);
                command.Parameters.AddWithValue("@SportsFee", model.SportsFee);
                command.Parameters.AddWithValue("@LibraryFee", model.LibraryFee);
                command.Parameters.AddWithValue("@ActivityFee", model.ActivityFee);
                command.Parameters.AddWithValue("@AnnualCharge", model.AnnualCharge);
                command.Parameters.AddWithValue("@TotalFee", model.TotalFee);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<DiscountFeeModel> GetAllDiscountFee(int userid)
        {
            List<DiscountFeeModel> list = new List<DiscountFeeModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageDiscountFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTALL");
                cmd.Parameters.AddWithValue("@userid", userid);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new DiscountFeeModel
                    {
                        FeeId = rdr["FeeId"] != DBNull.Value ? Convert.ToInt32(rdr["FeeId"]) : 0,
                        ClassName = rdr["ClassName"].ToString(),
                        Discount = Convert.ToInt32(rdr["Discount"]),
                        Fee = Convert.ToDecimal(rdr["Fee"]),
                        AfterDiscountFee = Convert.ToDecimal(rdr["AfterDiscountFee"]),
                        DiscountStartDate = Convert.ToDateTime(rdr["DiscountStartDate"]),
                        DiscountEndDate = Convert.ToDateTime(rdr["DiscountEndDate"]),
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

        public void InsertDiscountFee(DiscountFeeModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageDiscountFee", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@ClassId", model.ClassName);
                command.Parameters.AddWithValue("@Discount", model.Discount);
                command.Parameters.AddWithValue("@Fee", model.Fee);
                command.Parameters.AddWithValue("@AfterDiscountFee", model.AfterDiscountFee);
                command.Parameters.AddWithValue("@DiscountStartDate", model.DiscountStartDate);
                command.Parameters.AddWithValue("@DiscountEndDate", model.DiscountEndDate);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        public List<DiscountFeeModel> GetDiscountFeeById(int? id)
        {
            List<DiscountFeeModel> list = new List<DiscountFeeModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageDiscountFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@FeeId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new DiscountFeeModel
                    {
                        FeeId = rdr["FeeId"] != DBNull.Value ? Convert.ToInt32(rdr["FeeId"]) : 0,
                        ClassName = rdr["ClassId"].ToString(),
                        Discount = Convert.ToInt32(rdr["Discount"]),
                        Fee = Convert.ToDecimal(rdr["Fee"]),
                        AfterDiscountFee = Convert.ToDecimal(rdr["AfterDiscountFee"]),
                        DiscountStartDate = Convert.ToDateTime(rdr["DiscountStartDate"]),
                        DiscountEndDate = Convert.ToDateTime(rdr["DiscountEndDate"]),
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
        public string DeleteDiscountFee(int? feeid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageDiscountFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@FeeId", feeid);

                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public void UpdateDiscountFee(DiscountFeeModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageDiscountFee", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "UPDATE");
                command.Parameters.AddWithValue("@FeeId", model.FeeId);
                command.Parameters.AddWithValue("@ClassId", model.ClassName);
                command.Parameters.AddWithValue("@Fee", model.Fee);
                command.Parameters.AddWithValue("@AfterDiscountFee", model.AfterDiscountFee);
                command.Parameters.AddWithValue("@Discount", model.Discount);
                command.Parameters.AddWithValue("@DiscountStartDate", model.DiscountStartDate);
                command.Parameters.AddWithValue("@DiscountEndDate", model.DiscountEndDate);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<BlockModel> GetAllBlock(int userid,int? academicYear=null)
        {
            List<BlockModel> list = new List<BlockModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageHostelBlock", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTALL");
                cmd.Parameters.Add("@academicYear", SqlDbType.Int).Value = (object)academicYear ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new BlockModel
                    {
                        HostelId = rdr["HostelId"] != DBNull.Value ? Convert.ToInt32(rdr["HostelId"]) : 0,
                        BlockName = rdr["BlockName"].ToString(),
                        TotalRoomInBlock = rdr["TotalRoomInBlock"].ToString(),
                        GenderType = rdr["genderType"].ToString(),
                        blockType = rdr["blockType"].ToString(),
                        TotalFlourInBlock = rdr["TotalFlourInBlock"]!=DBNull.Value? Convert.ToInt32(rdr["TotalFlourInBlock"]):0,
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

        public void InsertBlock(BlockModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageHostelBlock", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@BlockName", model.BlockName);
                //command.Parameters.AddWithValue("@WardenId", model.WardenName);
                command.Parameters.AddWithValue("@TotalRoomInBlock", model.TotalRoomInBlock);
                command.Parameters.AddWithValue("@genderType", model.GenderType);
                command.Parameters.AddWithValue("@blockType", model.blockType);
                command.Parameters.AddWithValue("@TotalFlourInBlock", model.TotalFlourInBlock);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<BlockModel> GetBlockById(int? id)
        {
            List<BlockModel> list = new List<BlockModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageHostelBlock", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@HostelId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new BlockModel
                    {
                        HostelId = rdr["HostelId"] != DBNull.Value ? Convert.ToInt32(rdr["HostelId"]) : 0,
                        BlockName = rdr["BlockName"].ToString(),
                        TotalRoomInBlock = rdr["TotalRoomInBlock"].ToString(),
                        GenderType = rdr["genderType"].ToString(),
                        blockType = rdr["blockType"].ToString(),
                        TotalFlourInBlock = rdr["TotalFlourInBlock"] != DBNull.Value ? Convert.ToInt32(rdr["TotalFlourInBlock"]) : 0,
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
        public int GetFloorByBlockId(int id)
        {
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageHostelBlock", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectfloorbyid");
                cmd.Parameters.AddWithValue("@HostelId", id);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    return Convert.ToInt32(rdr["floor"]);
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
        public List<int> GetFloorByBlockIdToAddStd(int id)
        {
            try
            {
                List<int> floors = new List<int>();
                SqlCommand cmd = new SqlCommand("sp_ManageHostelBlock", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectfloorbyidforstd");
                cmd.Parameters.AddWithValue("@HostelId", id);

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
        public string DeleteBlock(int? hostelid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageHostelBlock", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@HostelId", hostelid);

                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public void UpdateBlock(BlockModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageHostelBlock", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "UPDATE");
                command.Parameters.AddWithValue("@HostelId", model.HostelId);
                command.Parameters.AddWithValue("@BlockName", model.BlockName);
                command.Parameters.AddWithValue("@TotalRoomInBlock", model.TotalRoomInBlock);
                command.Parameters.AddWithValue("@genderType", model.GenderType);
                command.Parameters.AddWithValue("@blockType", model.blockType);
                command.Parameters.AddWithValue("@TotalFlourInBlock", model.TotalFlourInBlock);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<RoomTypeModel> GetAllRoomTypes(int userid)
        {
            List<RoomTypeModel> list = new List<RoomTypeModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageRoomTypes", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTALL");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new RoomTypeModel
                    {
                        RoomId = rdr["RoomId"] != DBNull.Value ? Convert.ToInt32(rdr["RoomId"]) : 0,
                        RoomTypes = rdr["RoomTypes"].ToString(),
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

        public void InsertRoomTypes(RoomTypeModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageRoomTypes", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@RoomTypes", model.RoomTypes);
                connection.Open();

                command.ExecuteNonQuery();
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

        public List<RoomTypeModel> GetRoomTypesById(int? id)
        {
            List<RoomTypeModel> list = new List<RoomTypeModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageRoomTypes", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@RoomId", Convert.ToInt32(id));
                connection.Open();



                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new RoomTypeModel
                    {
                        RoomId = rdr["RoomId"] != DBNull.Value ? Convert.ToInt32(rdr["RoomId"]) : 0,
                        RoomTypes = rdr["RoomTypes"].ToString(),
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
        public string DeleteRoomTypes(int? roomid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageRoomTypes", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@RoomId", roomid);

                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return result;
        }

        public void UpdateRoomTypes(RoomTypeModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageRoomTypes", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "UPDATE");
                command.Parameters.AddWithValue("@RoomId", model.RoomId);
                command.Parameters.AddWithValue("@RoomTypes", model.RoomTypes);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<College_ERP.Models.Teacher.StudentModel> GetStudentsByClassSection(int userid, int classId, int sectionId, string academicyear)
        {
            List<College_ERP.Models.Teacher.StudentModel> list = new List<College_ERP.Models.Teacher.StudentModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentInHostel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYCLASSSECTIONForRoom");
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                cmd.Parameters.AddWithValue("@academicyear", academicyear);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new College_ERP.Models.Teacher.StudentModel
                    {
                        StudentId = rdr["StudentId"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        ClassId = rdr["ClassId"] != DBNull.Value ? Convert.ToInt32(rdr["ClassId"]) : 0,
                        SectionId = rdr["SectionId"] != DBNull.Value ? Convert.ToInt32(rdr["SectionId"]) : 0,
                        StudentName = rdr["StudentName"]?.ToString(),
                        Address = rdr["Address"]?.ToString(),
                        FatherName = rdr["FatherName"]?.ToString(),
                        MobileNo = rdr["MobileNo"]?.ToString(),
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

        public List<RoomNumberModel> GetAllRoomNumber(int userid)
        {
            List<RoomNumberModel> list = new List<RoomNumberModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageRoomNumber", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTALL");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new RoomNumberModel
                    {
                        RoomId = rdr["RoomId"] != DBNull.Value ? Convert.ToInt32(rdr["RoomId"]) : 0,
                        BlockName = rdr["BlockName"].ToString(),
                        BlockId = rdr["BlockId"] != DBNull.Value ? Convert.ToInt32(rdr["BlockId"]) : 0,
                        RoomTypes = rdr["RoomTypes"].ToString(),
                        BedCount = rdr["BedCount"]!=DBNull.Value? Convert.ToInt32(rdr["BedCount"]):0,
                        RoomFacilitate = string.IsNullOrEmpty(rdr["RoomFacilitate"].ToString())?"Non Air Conditioner": rdr["RoomFacilitate"].ToString(),
                        RoomNumber = Convert.ToInt32(rdr["RoomNumber"]),
                        FeesPerPerson = rdr["FeesPerPerson"]!=DBNull.Value? Convert.ToInt32(rdr["FeesPerPerson"]):0,
                        floor = rdr["floor"]!=DBNull.Value? Convert.ToInt32(rdr["floor"]):0
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

        public void InsertRoomNumber(RoomNumberModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageRoomNumber", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@BlockId", model.BlockName);
                command.Parameters.AddWithValue("@BedCount", model.BedCount);
                command.Parameters.AddWithValue("@RoomFacilitate", model.RoomFacilitate);
                command.Parameters.AddWithValue("@RoomNumber", model.RoomNumber);
                command.Parameters.AddWithValue("@FeesPerPerson", model.FeesPerPerson);
                command.Parameters.AddWithValue("@floor", model.floor);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<RoomNumberModel> GetRoomNumberById(int? id)
        {
            List<RoomNumberModel> list = new List<RoomNumberModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageRoomNumber", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@RoomId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new RoomNumberModel
                    {
                        RoomId = rdr["RoomId"] != DBNull.Value ? Convert.ToInt32(rdr["RoomId"]) : 0,
                        BlockName = rdr["BlockName"].ToString(),
                        BlockId = Convert.ToInt32(rdr["HostelId"]),
                        BedCount = Convert.ToInt32(rdr["BedCount"]),
                        RoomFacilitate = rdr["RoomFacilitate"].ToString(),
                        RoomNumber = Convert.ToInt32(rdr["RoomNumber"]),
                        FeesPerPerson = Convert.ToInt32(rdr["FeesPerPerson"]),
                        floor = rdr["floor"] != DBNull.Value ? Convert.ToInt32(rdr["floor"]) : 0
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
        public string DeleteRoomNumber(int? roomid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageRoomNumber", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@RoomId", roomid);

                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public void UpdateRoomNumber(RoomNumberModel model)
        {
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageRoomNumber", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "UPDATE");
                command.Parameters.AddWithValue("@RoomId", model.RoomId);
                command.Parameters.AddWithValue("@BlockId", model.BlockName);
                command.Parameters.AddWithValue("@BedCount", model.BedCount);
                command.Parameters.AddWithValue("@RoomFacilitate", model.RoomFacilitate);
                command.Parameters.AddWithValue("@RoomNumber", model.RoomNumber);
                command.Parameters.AddWithValue("@FeesPerPerson", model.FeesPerPerson);
                command.Parameters.AddWithValue("@floor", model.floor);
                connection.Open();
                command.ExecuteNonQuery();
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

        public List<AdminModel> GetAllStudentsInHostel(int userid)
        {
            List<AdminModel> list = new List<AdminModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentInHostel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTALL");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new AdminModel
                    {
                        HostelId = rdr["HostelId"] != DBNull.Value ? Convert.ToInt32(rdr["HostelId"]) : 0,
                        Block = rdr["BlockName"].ToString(),
                        RoomNumber = Convert.ToInt32(rdr["RoomNumber"]),
                        StudentName = rdr["StudentName"].ToString(),
                        FeeType = rdr["FeeType"].ToString(),
                        FeeSlip = rdr["FeeSlip"].ToString(),
                        DueDate = rdr["duedate"].ToString(),
                        FeesSubmitted = Convert.ToInt32(rdr["feesperperson"]),
                        RemainingFees = rdr["RemainingFees"] != DBNull.Value ? Convert.ToInt32(rdr["RemainingFees"]) : 0,
                        transactionid = rdr["transactionid"].ToString(),

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

        public bool InsertStudentsInHostel(AdminModel model, out string errorMsg)
        {
            string certificatePath = string.Empty;

            if (model.FeeSlips != null)
            {
                string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
                string fileExtension = Path.GetExtension(model.FeeSlips.FileName)?.ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    errorMsg = "Invalid file type. Only PDF, JPG, JPEG, and PNG files are allowed.";
                    return false;
                }

                certificatePath = UploadImageToServer(model.FeeSlips);
            }
            errorMsg = "";
            int res = 0;
            try
            {
                SqlCommand command = new SqlCommand("sp_ManageStudentInHostel", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@BlockId", model.Block);
                command.Parameters.AddWithValue("@RoomId", model.RoomNumber);
                command.Parameters.AddWithValue("@StudentId", model.StudentName);
                command.Parameters.AddWithValue("@transactionid", model.transactionid);

                command.Parameters.AddWithValue("@FeeType", model.FeeType);

                if (!string.IsNullOrEmpty(certificatePath))
                {
                    command.Parameters.AddWithValue("@FeeSlip",certificatePath);
                }
                else
                {
                    command.Parameters.AddWithValue("@FeeSlip", DBNull.Value);
                }


                command.Parameters.AddWithValue("@FeesSubmitted", model.FeesSubmitted);

                command.Parameters.AddWithValue("@RemainingFees", model.RemainingFees);
                command.Parameters.AddWithValue("@DueDate", model.DueDate);

                connection.Open();
                res = command.ExecuteNonQuery();

                if (res > 0 && !string.IsNullOrEmpty(certificatePath))
                {
                    UploadImageToServer(model.FeeSlips);
                }
                if (res <= 0)
                {
                    errorMsg = "Server error occured. Please try again!";
                }
                return res > 0;

            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }


        public RoomInfo GetRoomInfo(int roomId,int userid)
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
        


        public string DeleteStudentsInHostel(int? hostelid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentInHostel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@HostelId", hostelid);
                connection.Open();
                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
        public AdminModel GetStudentInHostelById(int? id)
        {
            AdminModel student = null;
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageStudentInHostel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@HostelId", Convert.ToInt32(id));

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    student = new AdminModel
                    {
                        HostelId = rdr["HostelId"] != DBNull.Value ? Convert.ToInt32(rdr["HostelId"]) : 0,
                        BlockId = rdr["BlockId"] != DBNull.Value ? Convert.ToInt32(rdr["BlockId"]) : 0,
                        RoomId = rdr["RoomId"] != DBNull.Value ? Convert.ToInt32(rdr["RoomId"]) : 0,
                        StudentId = rdr["StudentId"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        FeeType = rdr["FeeType"]?.ToString(),
                        FeeSlip = rdr["FeeSlip"]?.ToString(),
                        RemainingFees = rdr["RemainingFees"] != DBNull.Value ? Convert.ToInt32(rdr["RemainingFees"]) : 0,
                        FeesSubmitted = rdr["FeesSubmitted"] != DBNull.Value ? Convert.ToInt32(rdr["FeesSubmitted"]) : 0,
                        DueDate = rdr["DueDate"]!=DBNull.Value? Convert.ToDateTime(rdr["DueDate"]).ToString("yyyy-MM-dd"):null,
                        transactionid = rdr["transactionid"].ToString(),
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

            return student;
        }


        public bool UpdateStudentsInHostel(AdminModel model)
        {
            int res = 0;
            try
            {
                using (SqlCommand command = new SqlCommand("sp_ManageStudentInHostel", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Action", "UPDATE");


                    command.Parameters.AddWithValue("@HostelId", model.HostelId);
                    command.Parameters.AddWithValue("@StudentId", model.StudentName);
                    command.Parameters.AddWithValue("@BlockId", model.Block);
                    command.Parameters.AddWithValue("@RoomId", model.RoomNumber);
                    command.Parameters.AddWithValue("@FeeType", model.FeeType);
                    command.Parameters.AddWithValue("@transactionid", model.transactionid);


                    command.Parameters.AddWithValue("@FeesSubmitted", model.FeesSubmitted);
                    command.Parameters.AddWithValue("@RemainingFees", model.RemainingFees);

                    if (!string.IsNullOrEmpty(model.DueDate))
                    {
                        if (DateTime.TryParse(model.DueDate, out DateTime dueDateValue))
                        {
                            command.Parameters.AddWithValue("@DueDate", dueDateValue);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@DueDate", DBNull.Value);
                        }
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@DueDate", DBNull.Value);
                    }


                    if (model.FeeSlips != null)
                        command.Parameters.AddWithValue("@FeeSlip", model.FeeSlips.FileName);
                    else
                        command.Parameters.AddWithValue("@FeeSlip", DBNull.Value);


                    connection.Open();
                    res = command.ExecuteNonQuery();


                    if (res > 0 && model.FeeSlips != null)
                    {
                        UploadImageToServer(model.FeeSlips);
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

            return res > 0;
        }


        public bool Addcategory(EventCategory category)
        {
            string categoryImage = null;
            try
            {
                if (category.CategoryImage != null)
                {
                    categoryImage = UploadImageToServer(category.CategoryImage);
                }

                SqlCommand cmd = new SqlCommand("Sp_Event", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                cmd.Parameters.AddWithValue("@userid", category.userid);
                if (category.CategoryImage != null)
                {
                    cmd.Parameters.AddWithValue("@CategoryImage", categoryImage);
                }
                cmd.Parameters.AddWithValue("@CategoryDescription", category.CategoryDescription);
                cmd.Parameters.AddWithValue("@fromdate", category.fromdate);
                cmd.Parameters.AddWithValue("@todate", category.todate);
                cmd.Parameters.AddWithValue("@Action", "AddEventCategory");
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    if (!string.IsNullOrEmpty(categoryImage))
                    {
                        string serverpath = HttpContext.Current.Server.MapPath("~" + categoryImage);
                        if (Directory.Exists(serverpath))
                        {
                            System.IO.File.Delete(serverpath);
                        }
                    }
                }
                return res > 0;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(categoryImage))
                {
                    string serverpath = HttpContext.Current.Server.MapPath("~" + categoryImage);
                    if (Directory.Exists(serverpath))
                    {
                        System.IO.File.Delete(serverpath);
                    }
                }

                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        public bool DeleteCategory(int id)
        {

            try
            {
                SqlCommand cmd = new SqlCommand("Sp_Event", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Action", "DeleteCategory");
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

        public bool UpdateCategory(EventCategory category)
        {
            string categoryImage = null;

            try
            {
                if (category.CategoryImage != null)
                {
                    categoryImage = UploadImageToServer(category.CategoryImage);
                }
                SqlCommand cmd = new SqlCommand("Sp_Event", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                cmd.Parameters.AddWithValue("@Id", category.Id);
                if (category.CategoryImage != null)
                {
                    cmd.Parameters.AddWithValue("@CategoryImage", categoryImage);
                }
                cmd.Parameters.AddWithValue("@CategoryDescription", category.CategoryDescription);
                cmd.Parameters.AddWithValue("@fromdate", category.fromdate);
                cmd.Parameters.AddWithValue("@todate", category.todate);
                cmd.Parameters.AddWithValue("@Action", "UpdateCategory");
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    if (!string.IsNullOrEmpty(categoryImage))
                    {
                        string serverpath = HttpContext.Current.Server.MapPath("~" + categoryImage);
                        if (Directory.Exists(serverpath))
                        {
                            System.IO.File.Delete(serverpath);
                        }
                    }
                }
                return res > 0;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(categoryImage))
                {
                    string serverpath = HttpContext.Current.Server.MapPath("~" + categoryImage);
                    if (Directory.Exists(serverpath))
                    {
                        System.IO.File.Delete(serverpath);
                    }
                }
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        public bool InsertWarden(warden model, out string eror)
        {
            eror = "";
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            try
            {
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

                string documentPath = "";
                if (model.Documents != null)
                {
                    string docExt = Path.GetExtension(model.Documents.FileName)?.ToLower();
                    if (!allowedExtensions.Contains(docExt))
                    {
                        eror = "Invalid document file type. Only JPG, JPEG, PNG, or PDF files are allowed.";
                        return false;
                    }
                    documentPath = UploadImageToServer(model.Documents);
                    if (string.IsNullOrEmpty(documentPath))
                    {
                        eror = "Document upload failed.";
                        return false;
                    }
                }

                string profilePicPath = "";
                if (model.ProfilePic != null)
                {
                    string picExt = Path.GetExtension(model.ProfilePic.FileName)?.ToLower();
                    if (!allowedExtensions.Contains(picExt))
                    {
                        eror = "Invalid profile picture file type. Only JPG, JPEG, PNG, or PDF files are allowed.";
                        return false;
                    }
                    profilePicPath = UploadImageToServer(model.ProfilePic);
                    if (string.IsNullOrEmpty(profilePicPath))
                    {
                        eror = "Profile picture upload failed.";
                        return false;
                    }
                }

                SqlCommand cmd = new SqlCommand("sp_InsertWarden", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "insertWarden");
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@EmployeeId", model.EmployeeId);
                cmd.Parameters.AddWithValue("@userId", model.userId);
                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
                cmd.Parameters.AddWithValue("@DOB", model.DOB);
                cmd.Parameters.AddWithValue("@EmailId", model.EmailId);
                cmd.Parameters.AddWithValue("@st_Id", model.st_Id);
                cmd.Parameters.AddWithValue("@city_Id", model.city_Id);
                cmd.Parameters.AddWithValue("@BlockId", model.BlockId);
                cmd.Parameters.AddWithValue("@Gender", model.Gender);
                cmd.Parameters.AddWithValue("@Document", documentPath);
                cmd.Parameters.AddWithValue("@ProfilePic", profilePicPath);
                cmd.Parameters.AddWithValue("@Address", model.Address);

                int id = 0;
                string eres = cmd.ExecuteScalar()?.ToString();
                bool parseres = int.TryParse(eres, out id);
                if (parseres) id = Convert.ToInt32(eres);

                if (id > 0)
                {
                    string wardenUsername = model.Name.Trim().Split(' ')[0].Contains('.') ? model.Name.Split('.')[1].Trim().Split(' ')[0] : model.Name.Split(' ')[0];

                    string mobilePart = model.MobileNo.ToString();
                    mobilePart = mobilePart.Substring(6);

                    string username = $"{wardenUsername}@{mobilePart}";
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
                    cmd.Parameters.AddWithValue("@role", "warden");

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
                        transaction.Rollback();
                        eror = "Something went wrong while sending email.";
                        return false;
                    }
                }

                transaction.Rollback();
                eror = "Something went wrong while inserting the warden.";
                return false;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                eror = ex.Message;
                return false;
            }
            finally
            {
                connection.Close();
            }
        }


        public List<warden> GetAllWarden(int? id)
        {
            List<warden> list = new List<warden>();
            SqlDataReader rdr = null;

            try
            {
                SqlCommand cmd = new SqlCommand("sp_InsertWarden", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAllWarden");
                cmd.Parameters.AddWithValue("@userId", id);

                connection.Open();
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    warden model = new warden();

                    model.Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0;
                    model.EmployeeId = rdr["EmployeeId"].ToString();
                    model.Name = rdr["Name"].ToString();
                    model.MobileNo = rdr["MobileNo"].ToString();
                    model.DOB = Convert.ToDateTime(rdr["DOB"]);
                    model.EmailId = rdr["Email_id"].ToString();
                    model.stateName = rdr["stateName"].ToString();
                    model.BlockName = rdr["BlockName"].ToString();
                    model.st_Id = rdr["StateId"] != DBNull.Value ? Convert.ToInt32(rdr["StateId"]) : 0;
                    model.city_Id = rdr["CityId"] != DBNull.Value ? Convert.ToInt32(rdr["CityId"]) : 0;
                    model.Gender = rdr["Gender"].ToString();
                    model.BlockId = rdr["BlockId"] != DBNull.Value ? Convert.ToInt32(rdr["BlockId"]) : 0;
                    model.Document = rdr["Document"].ToString();
                    model.ProfilePics = rdr["ProfilePic"].ToString();
                    model.Address = rdr["Address"].ToString();
                    model.username = rdr["username"] != DBNull.Value ? rdr["username"].ToString():null;
                    model.password =rdr["password"] !=DBNull.Value? rdr["password"].ToString():null;
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

        public string deleteWarden(int Id)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_InsertWarden", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "deleteWarden");
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

        public warden GetWardenById(int id)
        {
            try
            {

                SqlCommand cmd = new SqlCommand("sp_InsertWarden", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);


                cmd.Parameters.AddWithValue("@action", "getWardenById");

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new warden
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        EmployeeId = reader["EmployeeId"].ToString(),
                        Name = reader["Name"].ToString(),
                        MobileNo = reader["MobileNo"].ToString(),
                        DOB = Convert.ToDateTime(reader["DOB"]),
                        DOBstring = Convert.ToDateTime(reader["DOB"]).ToString("yyyy-MM-dd"),
                        EmailId = reader["Email_id"].ToString(),
                        st_Id = reader["StateId"] != DBNull.Value ? Convert.ToInt32(reader["StateId"]) : 0,
                        city_Id = reader["CityId"] != DBNull.Value ? Convert.ToInt32(reader["CityId"]) : 0,
                        Gender = reader["Gender"].ToString(),
                        Document = reader["Document"].ToString(),
                        ProfilePics = reader["ProfilePic"].ToString(),
                        BlockName1 = reader["BlockName"].ToString(),
                        BlockId = reader["BlockId"] != DBNull.Value ? Convert.ToInt32(reader["BlockId"]) : 0,
                        Address = reader["Address"].ToString(),
                        stateName = reader["stateName"].ToString(),
                        cityName = reader["City_Name"].ToString()
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

        public string UpdateWarden(warden data)
        {
            int result = 0;
            try
            {
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
                string profilePicPath = string.Empty;
                string documentPath = string.Empty;

                if (data.ProfilePic != null)
                {
                    string profileExt = Path.GetExtension(data.ProfilePic.FileName)?.ToLower();
                    if (!allowedExtensions.Contains(profileExt))
                    {
                        return "Invalid profile picture file type. Only JPG, JPEG, PNG, or PDF files are allowed.";
                    }

                    profilePicPath = UploadImageToServer(data.ProfilePic);
                    if (string.IsNullOrEmpty(profilePicPath))
                        return "Profile picture upload failed.";
                }

                if (data.Documents != null)
                {
                    string docExt = Path.GetExtension(data.Documents.FileName)?.ToLower();
                    if (!allowedExtensions.Contains(docExt))
                    {
                        return "Invalid document file type. Only JPG, JPEG, PNG, or PDF files are allowed.";
                    }

                    documentPath = UploadImageToServer(data.Documents);
                    if (string.IsNullOrEmpty(documentPath))
                        return "Document upload failed.";
                }

                SqlCommand cmd = new SqlCommand("sp_InsertWarden", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", data.Id);
                cmd.Parameters.AddWithValue("@action", "UpdateWarden");
                cmd.Parameters.AddWithValue("@EmployeeId", data.EmployeeId);
                cmd.Parameters.AddWithValue("@Name", data.Name);
                cmd.Parameters.AddWithValue("@MobileNo", data.MobileNo);
                cmd.Parameters.AddWithValue("@DOB", data.DOB);
                cmd.Parameters.AddWithValue("@EmailId", data.EmailId);
                cmd.Parameters.AddWithValue("@st_Id", data.st_Id);
                cmd.Parameters.AddWithValue("@city_Id", data.city_Id);
                cmd.Parameters.AddWithValue("@Gender", data.Gender);
                cmd.Parameters.AddWithValue("@BlockId", data.BlockId);
                cmd.Parameters.AddWithValue("@Address", data.Address);

                if (!string.IsNullOrEmpty(profilePicPath))
                {
                    cmd.Parameters.AddWithValue("@ProfilePic", profilePicPath);
                }

                if (!string.IsNullOrEmpty(documentPath))
                {
                    cmd.Parameters.AddWithValue("@Document", documentPath);
                }

                connection.Open();
                result = cmd.ExecuteNonQuery();

                return result > 0 ? "Success" : "Failed";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
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

        public List<EventCategory> ShowAllcategory(int userid)
        {
            try
            {
                List<EventCategory> list = new List<EventCategory>();
                SqlCommand cmd = new SqlCommand("Sp_Event", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "ShowCategory");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    EventCategory category = new EventCategory
                    {
                        Id = Convert.ToInt32(rd["Id"]),
                        CategoryName = rd["CategoryName"].ToString(),
                        CategoryDescription = rd["CategoryDescription"].ToString(),
                        fromdate = Convert.ToDateTime(rd["fromdate"]).ToString("dd-MM-yyyy"),
                        todate = Convert.ToDateTime(rd["todate"]).ToString("dd-MM-yyyy"),
                        CreatedDate = Convert.ToDateTime(rd["CreatedDate"]).ToString("dd-MM-yyyy"),
                        CategoryImg = rd["CategoryImage"].ToString()
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

        public FeeInfo getTotalFeeByClassId(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageDiscountFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selecttotalFeeByClass");
                cmd.Parameters.AddWithValue("@ClassId", id);
                connection.Open();

                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    return new FeeInfo
                    {
                        Amount = rd["totalfee"]?.ToString(),
                        BillingPeriod = rd["BillingPeriod"]?.ToString()
                    };
                }

                return null; 
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public string getTotalFeeByClass(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageDiscountFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selecttotalFeeByClass");
                cmd.Parameters.AddWithValue("@ClassId", id);
                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    return rd["totalfee"]?.ToString();
                }

                return "0";
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                connection.Close();
            }

        }

        #region Inventory Services
        public bool InsertInventoryCategory(InventoryCategory invcat)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "insertcategory");
                cmd.Parameters.AddWithValue("@userId", invcat.userId);
                cmd.Parameters.AddWithValue("@categoryName", invcat.CategoryName);
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
        public List<InventoryCategory> GetInventCategory(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectCategory");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<InventoryCategory> list = new List<InventoryCategory>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new InventoryCategory
                        {
                            CategoryId = Convert.ToInt32(res["id"]),
                            CategoryName = res["categoryName"].ToString()
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
        public bool InsertInventoryMaterial(Material invcat,out string errormsg)
        {
            errormsg = "";
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", invcat.materialId > 0 ? "updatematerial" : "insertmaterial");
                cmd.Parameters.AddWithValue("@id", invcat.materialId);
                cmd.Parameters.AddWithValue("@userId", invcat.userId);
                cmd.Parameters.AddWithValue("@categoryId", invcat.categoryName);
                cmd.Parameters.AddWithValue("@materialName", invcat.materialName);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    errormsg = "Some error occured";
                    return false;
                }
                return res > 0;
            }
            catch(Exception ex)
            {
                errormsg = ex.Message;
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public List<Material> GetInventoryMaterial(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectmaterial");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<Material> list = new List<Material>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Material
                        {
                            materialId = Convert.ToInt32(res["id"]),
                            categoryName = res["categoryName"].ToString(),
                            categoryId = Convert.ToInt32(res["categoryId"]),
                            materialName = res["materialName"].ToString(),
                            createdAt = Convert.ToDateTime(res["createdAt"])
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
        public List<Material> GetInventoryMaterialById(int userId, int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectmaterialbyid");
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@categoryId", id);
                connection.Open();
                List<Material> list = new List<Material>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Material
                        {
                            materialId = Convert.ToInt32(res["id"]),
                            categoryName = res["categoryName"].ToString(),
                            categoryId = Convert.ToInt32(res["categoryId"]),
                            materialName = res["materialName"].ToString(),
                            createdAt = Convert.ToDateTime(res["createdAt"])
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
        public bool DeleteInventoryMaterial(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteInventoryMaterial");
                cmd.Parameters.AddWithValue("@id", id);
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
        public List<Material> GetInventoryMaterialById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectmaterialbyid");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                List<Material> list = new List<Material>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Material
                        {
                            materialId = Convert.ToInt32(res["id"]),
                            categoryName = res["categoryName"].ToString(),
                            categoryId = Convert.ToInt32(res["categoryId"]),
                            materialName = res["materialName"].ToString()
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
        public bool InsertStockMaterial(StockMaterial sm , out string errormsg)
        {
            errormsg = "";
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", sm.stockMaterialId > 0 ? "updatestockmaterial" : "insertstockmaterial");
                cmd.Parameters.AddWithValue("@id", sm.stockMaterialId);
                cmd.Parameters.AddWithValue("@userId", sm.userId);
                cmd.Parameters.AddWithValue("@categoryId", sm.categoryName);
                cmd.Parameters.AddWithValue("@materialId", sm.materialName);
                cmd.Parameters.AddWithValue("@quantity", sm.quantity);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if(res<=0)
                {
                    errormsg = "Some error occured";
                    return false;
                }
                return res > 0;
            }
            catch(Exception ex)
            {
                errormsg = ex.Message;
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public List<StockMaterial> GetStockList(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectstocklist");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<StockMaterial> list = new List<StockMaterial>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new StockMaterial
                        {
                            stockMaterialId = Convert.ToInt32(res["id"]),
                            materialId = Convert.ToInt32(res["materialId"]),
                            categoryName = res["categoryName"].ToString(),
                            categoryId = Convert.ToInt32(res["categoryId"]),
                            materialName = res["materialName"].ToString(),
                            createdAt = Convert.ToDateTime(res["createdAt"]),
                            quantity = res["quantity"].ToString()
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
        public bool DeleteInventoryStock(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteInventoryStock");
                cmd.Parameters.AddWithValue("@id", id);
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
        public bool InsertPurchaseMaterial(PurchaseMaterial sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", sm.purchaseId > 0 ? "updatepurchasematerial" : "insertpurchasematerial");
                cmd.Parameters.AddWithValue("@id", sm.purchaseId);
                cmd.Parameters.AddWithValue("@userId", sm.userId);
                cmd.Parameters.AddWithValue("@categoryId", sm.categoryName);
                cmd.Parameters.AddWithValue("@materialId", sm.materialName);
                cmd.Parameters.AddWithValue("@supplierName", sm.supplierName);
                cmd.Parameters.AddWithValue("@quantity", sm.quantity);
                cmd.Parameters.AddWithValue("@purchasePrice", sm.purchasePrice);
                cmd.Parameters.AddWithValue("@purchaseMedium", sm.purchaseMedium);
                cmd.Parameters.AddWithValue("@billNo", sm.billNo);
                cmd.Parameters.AddWithValue("@billSlip", sm.billSlipName);
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
        public List<PurchaseMaterial> GetPurchaseList(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectallpurchase");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<PurchaseMaterial> list = new List<PurchaseMaterial>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new PurchaseMaterial
                        {
                            purchaseId = Convert.ToInt32(res["id"]),
                            materialId = Convert.ToInt32(res["materialId"]),
                            categoryName = res["categoryName"].ToString(),
                            supplierName = res["supplierName"].ToString(),
                            categoryId = Convert.ToInt32(res["categoryId"]),
                            materialName = res["materialName"].ToString(),
                            purchasePrice = Convert.ToDecimal(res["purchasePrice"]),
                            purchaseMedium = res["purchaseMedium"].ToString(),
                            billNo = res["billNo"].ToString(),
                            billSlipName = res["billSlip"].ToString(),
                            createdAt = Convert.ToDateTime(res["createdAt"]),
                            quantity = res["quantity"].ToString()
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
        public bool DeletePurchase(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeletePurchase");
                cmd.Parameters.AddWithValue("@id", id);
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
        public bool InsertShortMaterial(ShortMaterial sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", sm.stockMaterialId > 0 ? "updateshortmaterial" : "insertshortmaterial");
                cmd.Parameters.AddWithValue("@id", sm.stockMaterialId);
                cmd.Parameters.AddWithValue("@userId", sm.userId);
                cmd.Parameters.AddWithValue("@categoryId", sm.categoryName);
                cmd.Parameters.AddWithValue("@materialId", sm.materialName);
                cmd.Parameters.AddWithValue("@description", sm.description);
                cmd.Parameters.AddWithValue("@quantity", sm.quantity);
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
        public List<ShortMaterial> GetShortList(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectshortlist");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<ShortMaterial> list = new List<ShortMaterial>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new ShortMaterial
                        {
                            stockMaterialId = Convert.ToInt32(res["id"]),
                            materialId = Convert.ToInt32(res["materialId"]),
                            categoryName = res["categoryName"].ToString(),
                            categoryId = Convert.ToInt32(res["categoryId"]),
                            materialName = res["materialName"].ToString(),
                            description = res["description"].ToString(),
                            createdAt = Convert.ToDateTime(res["createdAt"]),
                            quantity = res["quantity"].ToString()
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
        public bool DeleteInventoryShort(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageInventory", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteInventoryShort");
                cmd.Parameters.AddWithValue("@id", id);
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
        #region Transport Service
        public bool AddBuss(Buss bs)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", bs.Id > 0 ? "updateBuss" : "insertBuss");
                cmd.Parameters.AddWithValue("@id", bs.Id);
                cmd.Parameters.AddWithValue("@userId", bs.userId);
                cmd.Parameters.AddWithValue("@type", bs.Type);
                cmd.Parameters.AddWithValue("@bussNo", bs.BusNo);
                cmd.Parameters.AddWithValue("@bussSeatCapacity", bs.BusSeat);
                cmd.Parameters.AddWithValue("@image", bs.ImageName);
                cmd.Parameters.AddWithValue("@travelCompanyName", bs.CompanyName);
                cmd.Parameters.AddWithValue("@contactNo", bs.ContactNo);
                cmd.Parameters.AddWithValue("@contactPerson", bs.PersonName);
                cmd.Parameters.AddWithValue("@bussCharge", bs.BusCharges);
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
        public List<Buss> GetBusList(int userId,int? academicYear=null)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBuss");
                cmd.Parameters.Add("@academicYear", SqlDbType.Int).Value = (object)academicYear ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<Buss> list = new List<Buss>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Buss
                        {
                            Id = Convert.ToInt32(res["id"]),
                            Type = res["type"].ToString(),
                            BusNo = res["bussNo"].ToString(),
                            BusSeat = Convert.ToInt32(res["bussSeatCapacity"]),
                            ImageName = res["image"].ToString(),
                            CompanyName = res["travelCompanyName"].ToString(),
                            PersonName = res["contactPerson"].ToString(),
                            ContactNo = Convert.ToInt64(res["contactNo"]),
                            BusCharges = Convert.ToInt32(res["bussCharge"])
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
        public List<Buss> GetBusListNotRoute(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBussNotRoute");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<Buss> list = new List<Buss>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Buss
                        {
                            Id = Convert.ToInt32(res["id"]),
                            Type = res["type"].ToString(),
                            BusNo = res["bussNo"].ToString(),
                            BusSeat = Convert.ToInt32(res["bussSeatCapacity"]),
                            ImageName = res["image"].ToString(),
                            CompanyName = res["travelCompanyName"].ToString(),
                            PersonName = res["contactPerson"].ToString(),
                            ContactNo = Convert.ToInt64(res["contactNo"]),
                            BusCharges = Convert.ToInt32(res["bussCharge"])
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
        public bool DeleteBuss(int id, int userid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteBuss");
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@userId", userid);
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
        public List<Buss> GetBussById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBussById");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                List<Buss> list = new List<Buss>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Buss
                        {
                            Id = Convert.ToInt32(res["id"]),
                            Type = res["type"].ToString(),
                            BusNo = res["bussNo"].ToString(),
                            BusSeat = Convert.ToInt32(res["bussSeatCapacity"]),
                            ImageName = res["image"].ToString(),
                            CompanyName = res["travelCompanyName"].ToString(),
                            PersonName = res["contactPerson"].ToString(),
                            ContactNo = Convert.ToInt64(res["contactNo"]),
                            BusCharges = Convert.ToInt32(res["bussCharge"])
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
        public bool InsertDriver(Drivers bs,out string errorMessage)
        {
            errorMessage = null;
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", bs.Id > 0 ? "updateDriver" : "insertDriver");
                cmd.Parameters.AddWithValue("@id", bs.Id);
                cmd.Parameters.AddWithValue("@userId", bs.userId);
                cmd.Parameters.AddWithValue("@EmployeeId", bs.EmployeeId);
                cmd.Parameters.AddWithValue("@name", bs.Name);
                cmd.Parameters.AddWithValue("@contactNo", bs.ContactNo);
                cmd.Parameters.AddWithValue("@fatherName", bs.FatherName);
                cmd.Parameters.AddWithValue("@dlNo", bs.DLNo);
                cmd.Parameters.AddWithValue("@adharCardNo", bs.AdharCardNo);
                cmd.Parameters.AddWithValue("@address", bs.Address);
                cmd.Parameters.AddWithValue("@salary", bs.Salary);
                cmd.Parameters.AddWithValue("@driverFile", bs.DriverFileName);
                cmd.Parameters.AddWithValue("@adharCardFile", bs.AdharCardFileName);
                cmd.Parameters.AddWithValue("@dlFile", bs.DLFileName);
                cmd.Parameters.AddWithValue("@email", bs.Email);
                object res = cmd.ExecuteScalar();

                if (res == null)
                {
                    errorMessage = "Some error occured while processing your request.";
                    return false;
                }

                int id = Convert.ToInt32(res);
                if (id>0 && bs.Id<=0)
                {
                    string driverName = bs.Name.Trim().Split(' ')[0].Contains('.') ? bs.Name.Split('.')[1].Trim().Split(' ')[0] : bs.Name.Split(' ')[0];

                    string mobilePart = bs.ContactNo.ToString();
                    mobilePart = mobilePart.Substring(6);

                    string username = $"{driverName}@{mobilePart}";
                    string randomCharacter = "ABCDEFGHIJKLMNOPQRSTUVWabcdefghijklmnopqrst1234567890";
                    Random rmd = new Random();
                    string password = string.Empty;
                    for (int i = 0; i < 6; i++)
                    {
                        int rcount = rmd.Next(randomCharacter.Length - 1);
                        password += randomCharacter[rcount];
                    }

                    string subject = "Login Credential";
                    string body = $" <p>We are pleased to inform you that your account has been successfully created on the portal.</p><p><strong>Your login credentials are as follows:</strong></p><ul><li><strong>Username:</strong> {username} </li><li><strong>Password:</strong> {password} </li></ul>";
                    CommonMessage mailres = _mail.SendEmail(subject, body, bs.Email);
                    if (mailres.status)
                    {

                        cmd = new SqlCommand("sp_loginmanager", connection, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@action", "insertlogincredential");
                        cmd.Parameters.AddWithValue("@userId", id);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@role", "driver");
                        int res2 = cmd.ExecuteNonQuery();
                        if (res2 > 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                     
                    }
                }else if (id > 0 && bs.Id>0)
                {
                    transaction.Commit();
                    return true;
                }
                transaction.Rollback();
                errorMessage = "Some error occured while processing your request.";
                return false;
            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
                transaction.Rollback();
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public List<Drivers> GetDriverList(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectDrivers");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<Drivers> list = new List<Drivers>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Drivers
                        {
                            Id = Convert.ToInt32(res["id"]),
                            EmployeeId = res["EmployeeId"].ToString(),
                            Name = res["name"].ToString(),
                            ContactNo = Convert.ToInt64(res["contactNo"]),
                            FatherName = res["fatherName"].ToString(),
                            DLNo = res["dlNo"].ToString(),
                            AdharCardNo = Convert.ToInt64(res["adharCardNo"]),
                            Address = res["address"].ToString(),
                            Salary = Convert.ToInt64(res["salary"]),
                            DriverFileName = res["driverFile"].ToString(),
                            AdharCardFileName = res["adharCardFile"].ToString(),
                            DLFileName = res["dlFile"].ToString(),
                            Email = res["email"].ToString(),
                            username = res["username"] != DBNull.Value ? res["username"].ToString():null,
                            password = res["password"] != DBNull.Value ? res["password"].ToString():null
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
        public List<Drivers> GetDriverById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectDriversById");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                List<Drivers> list = new List<Drivers>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Drivers
                        {
                            Id = Convert.ToInt32(res["id"]),
                            EmployeeId = res["EmployeeId"].ToString(),
                            Name = res["name"].ToString(),
                            ContactNo = Convert.ToInt64(res["contactNo"]),
                            FatherName = res["fatherName"].ToString(),
                            DLNo = res["dlNo"].ToString(),
                            AdharCardNo = Convert.ToInt64(res["adharCardNo"]),
                            Address = res["address"].ToString(),
                            Salary = Convert.ToInt64(res["salary"]),
                            DriverFileName = res["driverFile"].ToString(),
                            AdharCardFileName = res["adharCardFile"].ToString(),
                            DLFileName = res["dlFile"].ToString(),
                            Email = res["email"].ToString(),
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
        public bool DeleteDriver(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteDriver");
                cmd.Parameters.AddWithValue("@id", id);
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
        public bool InsertBusRoute(BusRoute bs)
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection ,transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", bs.Id > 0 ? "updateBusRoute" : "insertBusRoute");
                cmd.Parameters.AddWithValue("@id", bs.Id);
                cmd.Parameters.AddWithValue("@userId", bs.userId);
                cmd.Parameters.AddWithValue("@stateId", bs.StateId);
                cmd.Parameters.AddWithValue("@cityId", bs.CityId);
                cmd.Parameters.AddWithValue("@bussNo", bs.BussNo);
                cmd.Parameters.AddWithValue("@route", bs.Route);
                cmd.Parameters.AddWithValue("@bussCharge", bs.BusCharges);
                SqlParameter outputparam = new SqlParameter("@brid", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputparam);
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    transaction.Rollback();
                    return false;
                }
                if (res > 0)
                {
                    int brid = outputparam.Value!=DBNull.Value? Convert.ToInt32(outputparam.Value):0;
                    foreach(var item in bs.ppm)
                    {
                        SqlCommand cmd1 = new SqlCommand("sp_ManageTransport", connection,transaction);
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.AddWithValue("@action", item.id>0? "updatepickupPoint" : "insertpickupPoint");
                        cmd1.Parameters.AddWithValue("@id", item.id);
                        cmd1.Parameters.AddWithValue("@busrouteid", brid>0?brid:bs.Id);
                        cmd1.Parameters.AddWithValue("@pickupPoint", item.pickupPoint);
                        if (connection.State == ConnectionState.Closed)
                            connection.Open();
                        int res1 = cmd1.ExecuteNonQuery();
                        if (res1 < 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
                transaction.Commit();
                return res > 0;
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public List<BusRoute> GetRouteist(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBusRouteList");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<BusRoute> list = new List<BusRoute>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new BusRoute
                        {
                            Id = Convert.ToInt32(res["id"]),
                            BussNoId = Convert.ToInt32(res["busNo"]),
                            StateName = res["stateName"].ToString(),
                            CityName = res["City_Name"].ToString(),
                            StateId = Convert.ToInt32(res["stateId"]),
                            CityId = Convert.ToInt32(res["cityId"]),
                            BussNo = res["bussNo"].ToString(),
                            type = res["type"].ToString(),
                            Route = res["route"].ToString(),
                            BusCharges = Convert.ToInt32(res["busCharge"])
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
        public bool DeleteBusRoute(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteBusRoute");
                cmd.Parameters.AddWithValue("@id", id);
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
        public List<BusRoute> GetBusRouteById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBusRouteById");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                List<BusRoute> list = new List<BusRoute>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new BusRoute
                        {
                            Id = Convert.ToInt32(res["id"]),
                            BussNoId = Convert.ToInt32(res["busNo"]),
                            BussNo = res["bussNo"].ToString(),
                            StateId = Convert.ToInt32(res["stateId"]),
                            StateName =res["stateName"].ToString(),
                            CityName = res["City_Name"].ToString(),
                            CityId = Convert.ToInt32(res["cityId"]),
                            type = res["type"].ToString(),
                            Route = res["route"].ToString(),
                            BusCharges = Convert.ToInt32(res["busCharge"]),
                            ppm = JsonConvert.DeserializeObject<List<PickupPointModel>>(res["pickup"].ToString())
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
        public List<PickupPointModel> GetPickupPointBus(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectpickuppoints");
                cmd.Parameters.AddWithValue("@busrouteid", id);
                connection.Open();
                List<PickupPointModel> list = new List<PickupPointModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new PickupPointModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            busNO =res["bussNo"].ToString(),
                            pickupPoint = res["pickupPoint"].ToString()
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
        public List<Drivers> GetDriverToAssign(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectDriverToAssign");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<Drivers> list = new List<Drivers>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Drivers
                        {
                            Id = Convert.ToInt32(res["id"]),
                            Name = res["name"].ToString(),
                            ContactNo = Convert.ToInt64(res["contactNo"]),
                            FatherName = res["fatherName"].ToString(),
                            DLNo = res["dlNo"].ToString(),
                            AdharCardNo = Convert.ToInt64(res["adharCardNo"]),
                            Address = res["address"].ToString(),
                            Salary = Convert.ToInt64(res["salary"]),
                            DriverFileName = res["driverFile"]!=null? res["driverFile"].ToString():"",
                            AdharCardFileName = res["adharCardFile"]!=null? res["adharCardFile"].ToString():"",
                            DLFileName = res["dlFile"] != null ? res["dlFile"].ToString() : "",
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
        public List<Buss> GetBusToAssign(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBusToAssign");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<Buss> list = new List<Buss>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Buss
                        {
                            Id = Convert.ToInt32(res["id"]),
                            Type = res["type"].ToString(),
                            BusNo = res["bussNo"].ToString(),
                            BusSeat = Convert.ToInt32(res["bussSeatCapacity"]),
                            ImageName = res["image"].ToString(),
                            CompanyName = res["travelCompanyName"].ToString(),
                            PersonName = res["contactPerson"].ToString(),
                            ContactNo = Convert.ToInt64(res["contactNo"]),
                            BusCharges = Convert.ToInt32(res["bussCharge"])
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
        public bool InsertAssignBus(AssignBus bs)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", bs.id > 0 ? "updateAssignBus" : "insertAssignBus");
                cmd.Parameters.AddWithValue("@id", bs.id);
                cmd.Parameters.AddWithValue("@userId", bs.userId);
                cmd.Parameters.AddWithValue("@bussId", bs.busId);
                cmd.Parameters.AddWithValue("@driverId", bs.driverId);
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
        public List<AssignBus> GetAssignedBus(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAssignedBus");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<AssignBus> list = new List<AssignBus>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new AssignBus
                        {
                            id = Convert.ToInt32(res["id"]),
                            busId = Convert.ToInt32(res["bussId"]),
                            busNo = res["bussNo"].ToString(),
                            driverId = Convert.ToInt32(res["driverId"]),
                            driverName = res["name"].ToString(),
                            createdAt = Convert.ToDateTime(res["createdAt"]),
                            type = res["type"].ToString()
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
        public bool DeleteAssignedBus(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteAssignedBus");
                cmd.Parameters.AddWithValue("@id", id);
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
        public bool InsertStudentInBus(AddStudentInBus bs , out string errormsg)
        {
            errormsg = "";
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", bs.id > 0 ? "updatestudentinbus" : "insertstudentinbus");
                cmd.Parameters.AddWithValue("@id", bs.id);
                cmd.Parameters.AddWithValue("@userId", bs.userId);
                cmd.Parameters.AddWithValue("@classId", bs.classId);
                cmd.Parameters.AddWithValue("@sectionId", bs.sectionId);
                cmd.Parameters.AddWithValue("@studentId", bs.studentId);
                cmd.Parameters.AddWithValue("@busId", bs.busId);
                cmd.Parameters.AddWithValue("@pickUpPoint", bs.pickUpPointId);
                cmd.Parameters.AddWithValue("@fee", bs.fee);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    errormsg = "Server error occured";
                }
                return res > 0;
            }
            catch(Exception ex)
            {
                errormsg = ex.Message;
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public List<AddStudentInBus> GetStudentInBus(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectStudentInBus");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<AddStudentInBus> list = new List<AddStudentInBus>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new AddStudentInBus
                        {
                            id = Convert.ToInt32(res["id"]),
                            busId = Convert.ToInt32(res["busId"]),
                            bussNo = res["bussNo"].ToString(),
                            type = res["type"].ToString(),
                            studentId = Convert.ToInt32(res["studentId"]),
                            studentName = res["StudentName"].ToString(),
                            classId = Convert.ToInt32(res["classId"]),
                            className = res["ClassName"].ToString(),
                            sectionId = Convert.ToInt32(res["sectionId"]),
                            sectionName = res["SectionName"].ToString(),
                            pickupPoint = res["pickupPoint"].ToString(),
                            fee = Convert.ToInt64(res["fee"]),
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
        public bool DeleteStudentInBus(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "deleteStudentInBus");
                cmd.Parameters.AddWithValue("@id", id);
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
        public List<AddStudentInBus> GetStudentInBusById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectStudentInBusById");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                List<AddStudentInBus> list = new List<AddStudentInBus>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new AddStudentInBus
                        {
                            id = Convert.ToInt32(res["id"]),
                            busId = Convert.ToInt32(res["busId"]),
                            bussNo = res["bussNo"].ToString(),
                            type = res["type"].ToString(),
                            studentId = Convert.ToInt32(res["studentId"]),
                            studentName = res["StudentName"].ToString(),
                            classId = Convert.ToInt32(res["classId"]),
                            className = res["ClassName"].ToString(),
                            sectionId = Convert.ToInt32(res["sectionId"]),
                            sectionName = res["SectionName"].ToString(),
                            pickUpPointId = Convert.ToInt32(res["pickUpPoint"]),
                            fee = Convert.ToInt64(res["fee"]),
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
        #region Security Service
        public bool InsertSecurity(College_ERP.Models.Admin.Security bs, out string errorMessage)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
            errorMessage = "";

            // Validate file extension
            if (!string.IsNullOrEmpty(bs.ImageName))
            {
                string ext = Path.GetExtension(bs.ImageName)?.ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    errorMessage = "Invalid file type. Only JPG, JPEG, PNG, files are allowed.";
                    return false;
                }
            }

            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageSecurity", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", bs.Id > 0 ? "updateSecurity" : "insertSecurity");
                cmd.Parameters.AddWithValue("@id", bs.Id);
                cmd.Parameters.AddWithValue("@EmployeeId", bs.EmployeeId);
                cmd.Parameters.AddWithValue("@userId", bs.userId);
                cmd.Parameters.AddWithValue("@name", bs.Name);
                cmd.Parameters.AddWithValue("@email", bs.Email);
                cmd.Parameters.AddWithValue("@mobileNo", bs.MobileNo);
                cmd.Parameters.AddWithValue("@dob", bs.DOB);
                cmd.Parameters.AddWithValue("@stateId", bs.StateId);
                cmd.Parameters.AddWithValue("@cityId", bs.CityId);
                cmd.Parameters.AddWithValue("@adharNo", bs.AdharNo);
                cmd.Parameters.AddWithValue("@gender", bs.Gender);
                cmd.Parameters.AddWithValue("@category", bs.Category);
                cmd.Parameters.AddWithValue("@blockId", bs.BlockId);
                cmd.Parameters.AddWithValue("@gateNo", bs.GateNo);
                cmd.Parameters.AddWithValue("@address", bs.Address);
                cmd.Parameters.AddWithValue("@image", bs.ImageName);

                int id = 0;
                string eres = cmd.ExecuteScalar()?.ToString();
                bool parseres = int.TryParse(eres, out id);
                if (parseres) id = Convert.ToInt32(eres);

                if (id > 0 && bs.Id <= 0)
                {
                    
                    string secrityUsername = bs.Name.Trim().Split(' ')[0].Contains('.') ? bs.Name.Split('.')[1].Trim().Split(' ')[0] : bs.Name.Split(' ')[0];

                    string mobilePart = bs.MobileNo.ToString();
                    mobilePart = mobilePart.Substring(6);

                    string username = $"{secrityUsername}@{mobilePart}";
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
                    cmd.Parameters.AddWithValue("@role", "security");

                    int res = cmd.ExecuteNonQuery();
                    if (res > 0)
                    {
                        string subject = "Login Credential";
                        string body = $" <p>We are pleased to inform you that your account has been successfully created on the portal.</p><p><strong>Your login credentials are as follows:</strong></p><ul><li><strong>Username:</strong> {username} </li><li><strong>Password:</strong> {password} </li></ul>";
                        CommonMessage mailres = _mail.SendEmail(subject, body, bs.Email);
                        if (mailres.status)
                        {
                            transaction.Commit();
                            return true;
                        }
                        transaction.Rollback();
                        return false;
                    }

                }
                else if (bs.Id > 0 && id > 0)
                {
                    transaction.Commit();
                    return true;
                }

                errorMessage = "Some error occured while processing your request";
                transaction.Rollback();
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                transaction.Rollback();
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public List<College_ERP.Models.Admin.Security> GetAllSecurityList(int userId,int? academicYear=null)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageSecurity", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAllSecurity");
                cmd.Parameters.AddWithValue("@academicYear",SqlDbType.Int).Value=(object)academicYear??DBNull.Value;
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<College_ERP.Models.Admin.Security> list = new List<College_ERP.Models.Admin.Security>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new College_ERP.Models.Admin.Security
                        {
                            Id = Convert.ToInt32(res["id"]),
                            EmployeeId = res["EmployeeId"].ToString(),
                            Name = res["name"].ToString(),
                            Email = res["email"].ToString(),
                            MobileNo = Convert.ToInt64(res["mobileNo"]),
                            DOB = Convert.ToDateTime(res["dob"]),
                            State = res["stateName"].ToString(),
                            StateId = Convert.ToInt32(res["stateId"]),
                            City = res["City_Name"].ToString(),
                            CityId = Convert.ToInt32(res["cityId"]),
                            AdharNo = Convert.ToInt64(res["adharNo"]),
                            Gender = res["gender"].ToString(),
                            Category = res["category"].ToString(),
                            BlockId = res["blockId"] != DBNull.Value ? Convert.ToInt32(res["blockId"]) : 0,
                            BlockName = res["BlockName"].ToString(),
                            GateNo = res["gateNo"].ToString(),
                            Address = res["address"].ToString(),
                            ImageName = res["image"].ToString(),
                            username =res["username"] !=DBNull.Value? res["username"].ToString():null,
                            password = res["password"] != DBNull.Value ? res["password"].ToString():null,

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
        public List<College_ERP.Models.Admin.Security> GetAllSecurityById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageSecurity", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectSecurityById");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                List<College_ERP.Models.Admin.Security> list = new List<College_ERP.Models.Admin.Security>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new College_ERP.Models.Admin.Security
                        {
                            Id = Convert.ToInt32(res["id"]),
                            EmployeeId = res["EmployeeId"].ToString(),
                            Name = res["name"].ToString(),
                            Email = res["email"].ToString(),
                            MobileNo = Convert.ToInt64(res["mobileNo"]),
                            DOB = Convert.ToDateTime(res["dob"]),
                            State = res["stateName"].ToString(),
                            StateId = Convert.ToInt32(res["stateId"]),
                            City = res["City_Name"].ToString(),
                            CityId = Convert.ToInt32(res["cityId"]),
                            AdharNo = Convert.ToInt64(res["adharNo"]),
                            Gender = res["gender"].ToString(),
                            Category = res["category"].ToString(),
                            BlockId = res["blockId"] != DBNull.Value ? Convert.ToInt32(res["blockId"]) : 0,
                            BlockName = res["BlockName"].ToString(),
                            GateNo = res["gateNo"].ToString(),
                            Address = res["address"].ToString(),
                            ImageName = res["image"].ToString(),

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
        public bool DeleteSecurity(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageSecurity", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "deleteSecurity");
                cmd.Parameters.AddWithValue("@id", id);
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

        #region holiday management
        public bool InsertHoliday(Holiday holiday, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_Holiday", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", holiday.HolidayId);
                    cmd.Parameters.AddWithValue("@userid", holiday.userid);
                    cmd.Parameters.AddWithValue("@Title", holiday.Title ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", holiday.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@HolidayDate", holiday.HolidayDate);
                    cmd.Parameters.AddWithValue("@Year", holiday.year);
                    cmd.Parameters.AddWithValue("@HolidayDateTo", holiday.HolidayDateTo);
                    cmd.Parameters.AddWithValue("@HolidayType", holiday.HolidayType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", holiday.HolidayId > 0 ? "UPDATE" : "insert");

                    connection.Open();
                    int res = cmd.ExecuteNonQuery();
                    return res > 0;
                }
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

        public List<Holiday> selectAllHoliday(int userid, out string errorMessage)
        {
            errorMessage = "";
            List<Holiday> hlist = new List<Holiday>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_Holiday", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@Action", "GETALL");

                    connection.Open();
                    SqlDataReader res = cmd.ExecuteReader();
                    if (res.HasRows)
                    {
                        while (res.Read())
                        {
                            hlist.Add(new Holiday
                            {
                                HolidayId = Convert.ToInt32(res["holidayid"]),
                                year = res["year"]!=DBNull.Value? Convert.ToInt32(res["year"]):Convert.ToInt32(DateTime.Now.Year),
                                Title = res["title"]?.ToString(),
                                Description = res["description"]?.ToString(),
                                HolidayDate = Convert.ToDateTime(res["holidayDate"]),
                                HolidayDateTo = res["holidayDateTo"]!=DBNull.Value? Convert.ToDateTime(res["holidayDateTo"]):DateTime.MinValue,
                                HolidayType = res["holidayType"]?.ToString()

                            });

                        }
                    }
                }
                return hlist;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return hlist;
            }
            finally
            {
                connection.Close();
            }
        }

        public Holiday selectHolidayById(int id, out string errorMessage)
        {
            errorMessage = "";
            Holiday holiday = new Holiday();
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_Holiday", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@Action", "GETById");

                    connection.Open();
                    SqlDataReader res = cmd.ExecuteReader();
                    if (res.HasRows)
                    {
                        while (res.Read())
                        {
                            holiday = new Holiday
                            {
                                HolidayId = Convert.ToInt32(res["holidayid"]),
                                year = res["year"] != DBNull.Value ? Convert.ToInt32(res["year"]) : Convert.ToInt32(DateTime.Now.Year),
                                Title = res["title"]?.ToString(),
                                Description = res["description"]?.ToString(),
                                HolidayDate = Convert.ToDateTime(res["holidayDate"]),
                                HolidayDateTo = res["holidayDateTo"] != DBNull.Value ? Convert.ToDateTime(res["holidayDateTo"]) : DateTime.MinValue,
                                HolidayType = res["holidayType"]?.ToString()

                            };

                        }
                    }
                }
                return holiday;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return holiday;
            }
            finally
            {
                connection.Close();
            }
        }

        public bool deleteHoliday(int id, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_Holiday", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@Action", "DELETE");

                    connection.Open();
                    int res = cmd.ExecuteNonQuery();
                    if (res <= 0)
                    {
                        errorMessage = "Something wen wrong while processing your request";
                    }
                    return res > 0;
                }
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
        #endregion
        #region TimeTable Management
        public bool InsertTimeTable(TimeTableModel tm, out string errorMessage)
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                errorMessage = "";
                foreach (var day in tm.dayList)
                {
                    SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@userId", tm.userId);
                    cmd.Parameters.AddWithValue("@classId", tm.classId);
                    cmd.Parameters.AddWithValue("@sectionId", tm.sectionId);
                    cmd.Parameters.AddWithValue("@day", day);
                    cmd.Parameters.AddWithValue("@action", "insertTimeTale");
                    SqlParameter outputIdParam = new SqlParameter("@tid", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputIdParam);
                    int res = cmd.ExecuteNonQuery();
                    if (res <= 0)
                    {
                        transaction.Rollback();
                        return false;
                    }
                    int id = (int)outputIdParam.Value;
                    if (res > 0)
                    {
                        foreach (var item in tm.sbm)
                        {
                            SqlCommand cmdd = new SqlCommand("sp_ManageTimeTable", connection, transaction);
                            cmdd.CommandType = CommandType.StoredProcedure;
                            cmdd.Parameters.Clear();
                            cmdd.Parameters.AddWithValue("@action", "insertTimeTableDetails");
                            cmdd.Parameters.AddWithValue("@id", id);
                            cmdd.Parameters.AddWithValue("@subjectId", item.subjectId);
                            cmdd.Parameters.AddWithValue("@fromTime", item.from);
                            cmdd.Parameters.AddWithValue("@toTime", item.to);
                            int res2 = cmdd.ExecuteNonQuery();
                            if (res2 <= 0)
                            {
                                transaction.Rollback();
                                return false;
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
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public bool UpdateTimeTable(timetableshowModel tm)
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "updateStatusOfSubject");
                cmd.Parameters.AddWithValue("@tid", tm.tid);
                int res = cmd.ExecuteNonQuery();
                if (res > 0)
                {
                    foreach (var item in tm.ttdata)
                    {
                        SqlCommand cmdd = new SqlCommand("sp_ManageTimeTable", connection, transaction);
                        cmdd.CommandType = CommandType.StoredProcedure;
                        cmdd.Parameters.AddWithValue("@action", "updateTimeTableDetails");
                        cmdd.Parameters.AddWithValue("@id", item.uid);
                        cmdd.Parameters.AddWithValue("@subjectId", item.subjectId);
                        cmdd.Parameters.AddWithValue("@fromTime", item.from);
                        cmdd.Parameters.AddWithValue("@toTime", item.to);
                        int res2 = cmdd.ExecuteNonQuery();
                        res = res + res2;
                    }
                }
                if (res < 0)
                    transaction.Rollback();
                else
                    transaction.Commit();
                return res > 0;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public List<TimeTableModel> ShowAllTimeTable(int userId)
        {
            try
            {
                List<TimeTableModel> list = new List<TimeTableModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectTimeTable");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    TimeTableModel category = new TimeTableModel
                    {
                        classId = Convert.ToInt32(res["classId"]),
                        className = res["ClassName"].ToString(),
                        sectionId = Convert.ToInt32(res["sectionId"]),
                        sectionName = res["SectionName"].ToString(),
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
        public bool DeleteTimeTable(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "deleteTimetable");
                cmd.Parameters.AddWithValue("@id", id);
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
        public List<SubjectTimeModel> ShowAllTimeTableDetails(int classid, int sectionid, string search = null)
        {
            try
            {
                List<SubjectTimeModel> list = new List<SubjectTimeModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectTimeTableDetails");
                cmd.Parameters.AddWithValue("@classId", classid);
                cmd.Parameters.AddWithValue("@sectionId", sectionid);
                cmd.Parameters.AddWithValue("@search", search??null);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    SubjectTimeModel category = new SubjectTimeModel
                    {
                        id = res["id"] != DBNull.Value ? Convert.ToInt32(res["id"]) : 0,
                        className = res["ClassName"] != DBNull.Value ? res["ClassName"].ToString() : "",
                        sectionName = res["sectionName"] != DBNull.Value ? res["sectionName"].ToString() : "",
                        subjectName = res["subject"] != DBNull.Value ? res["subject"].ToString() : "",
                        subjectId = res["subjectId"] != DBNull.Value ? Convert.ToInt32(res["subjectId"]) : 0,
                        from = res["fromTime"] != DBNull.Value ? DateTime.Today.Add((TimeSpan)res["fromTime"]).ToString("hh:mm tt") : "",
                        to = res["toTime"] != DBNull.Value ? DateTime.Today.Add((TimeSpan)res["toTime"]).ToString("hh:mm tt") : "",
                        day = res["day"] != DBNull.Value ? res["day"].ToString() : "",
                        tid = res["tid"] != DBNull.Value ? Convert.ToInt32(res["tid"]) : 0,
                        attachment = res["attachment"] != DBNull.Value ? res["attachment"].ToString() : "",
                        upid = res["upid"] != DBNull.Value ? Convert.ToInt32(res["upid"]) : 0,
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
        public List<SubjectTimeModel> GetTimeTableDataById(int id)
        {
            try
            {
                List<SubjectTimeModel> list = new List<SubjectTimeModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectTimeTableDetailsById");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    SubjectTimeModel category = new SubjectTimeModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        subjectName = res["subject"].ToString(),
                        subjectId = Convert.ToInt32(res["subjectId"]),
                        from = DateTime.Today.Add((TimeSpan)res["fromTime"]).ToString("hh:mm"),
                        to = DateTime.Today.Add((TimeSpan)res["toTime"]).ToString("hh:mm"),
                        day = res["day"].ToString(),
                        tid = Convert.ToInt32(res["tid"]),
                        className = res["ClassName"].ToString(),
                        sectionName = res["sectionName"].ToString(),
                        classId = Convert.ToInt32(res["classId"])
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
        #region Library Management
        public bool InsertBookCategory(BookCategoryModel cat)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "insertcategory");
                cmd.Parameters.AddWithValue("@userId", cat.userId);
                cmd.Parameters.AddWithValue("@addedBy", "admin");
                cmd.Parameters.AddWithValue("@categoryName", cat.categoryName);
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
        public List<BookCategoryModel> GetAllBookCategory(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectCategory");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<BookCategoryModel> list = new List<BookCategoryModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new BookCategoryModel
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();

            }
        }
        public bool checkISBN(string isbn, string actionType)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "checkIsbn");
                cmd.Parameters.AddWithValue("@isbn", isbn);
                cmd.Parameters.AddWithValue("@actionType", actionType);
                connection.Open();
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();

            }
        }
        public bool checkISSN(string issn, string actionType)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "checkIssn");
                cmd.Parameters.AddWithValue("@issnprint", issn);
                cmd.Parameters.AddWithValue("@actionType", actionType);
                connection.Open();
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();

            }
        }
        public bool checkAccession(string accession, string actionType)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "checkAccession");
                cmd.Parameters.AddWithValue("@accessionnumber", accession);
                cmd.Parameters.AddWithValue("@actionType", actionType);
                connection.Open();
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();

            }
        }
        public bool InsertBook(BookModel book)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userId", book.userId);
                cmd.Parameters.AddWithValue("@addedBy", "admin");
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
        public List<BookModel> GetAllBooks(int userId,int? academicYear=null)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAllBooks");
                cmd.Parameters.AddWithValue("@academicYear", SqlDbType.Int).Value=(object)academicYear ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@userId", userId);

                connection.Open();
                List<BookModel> list = new List<BookModel>();
                var res = cmd.ExecuteReader();

                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new BookModel
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public List<BookModel> GetBookById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBookById");
                cmd.Parameters.AddWithValue("@id", id);

                connection.Open();
                List<BookModel> list = new List<BookModel>();
                var res = cmd.ExecuteReader();

                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new BookModel
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public List<College_ERP.Models.Admin.UserOrderModel> SeletUserForLibrary(string userNo, string userType, int userId)
        {
            List<College_ERP.Models.Admin.UserOrderModel> list = new List<College_ERP.Models.Admin.UserOrderModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectuserByNo");
                cmd.Parameters.AddWithValue("@userNo", userNo);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@userType", userType);
                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {

                    while (rd.Read())
                    {
                        list.Add(new College_ERP.Models.Admin.UserOrderModel
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
                connection.Close();
            }
        }
        public bool InsertBookOrder(AddOrderModal model, out string errorMessage)
        {
            int result = 0;
            try
            {
                errorMessage = "";
                SqlCommand command = new SqlCommand("sp_ManageLibrary", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@action", "insertBookOrder");
                command.Parameters.AddWithValue("@userId", model.userId);
                command.Parameters.AddWithValue("@bookId", model.bookId);
                command.Parameters.AddWithValue("@addedBy", "admin");
                command.Parameters.AddWithValue("@buyerId", model.buyerId);
                command.Parameters.AddWithValue("@userType", model.userType);
                command.Parameters.AddWithValue("@orderDate", model.orderDate);
                command.Parameters.AddWithValue("@lateFine", model.lateFine);
                command.Parameters.AddWithValue("@damageFine", model.damageFine);
                command.Parameters.AddWithValue("@lostFine", model.lostFine);
                command.Parameters.AddWithValue("@quantity", model.quantity);
                command.Parameters.AddWithValue("@price", model.price);
                command.Parameters.AddWithValue("@returnDate", model.returnDate);
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
        public List<OrderHistoryModel> GetBookOrders(int userId)
        {
            List<OrderHistoryModel> orders = new List<OrderHistoryModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBookOrder");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        orders.Add(new OrderHistoryModel
                        {
                            id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id"]),

                            addedBy = reader["addedBy"] == DBNull.Value ? "" : reader["addedBy"].ToString(),

                            orderId = reader["shortOrderId"] == DBNull.Value ? "" : reader["shortOrderId"].ToString(),

                            userId = reader["userId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["userId"]),

                            bookId = reader["bookId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bookId"]),

                            buyerId = reader["buyerId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["buyerId"]),


                            userType = reader["userType"] == DBNull.Value ? "" : reader["userType"].ToString(),


                            orderDate = reader["orderDate"] == DBNull.Value
                ? DateTime.MinValue
                : Convert.ToDateTime(reader["orderDate"]),


                            orderDateString = reader["orderDate"] == DBNull.Value
                ? ""
                : Convert.ToDateTime(reader["orderDate"]).ToString("dd-MMM-yyyy"),


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
                : Convert.ToDateTime(reader["returnDate"]).ToString("dd-MMM-yyyy"),


                            name = reader["userType"] != DBNull.Value
           && reader["userType"].ToString().ToLower() == "student"
           ? (reader["StudentName"] == DBNull.Value ? "" : reader["StudentName"].ToString())

           : reader["userType"] != DBNull.Value
           && reader["userType"].ToString().ToLower() == "teacher"
           ? (reader["TeacherName"] == DBNull.Value ? "" : reader["TeacherName"].ToString())

           : "",


                            email = reader["userType"] != DBNull.Value
           && reader["userType"].ToString().ToLower() == "student"
           ? (reader["StudentEmail"] == DBNull.Value ? "" : reader["StudentEmail"].ToString())

           : reader["userType"] != DBNull.Value
           && reader["userType"].ToString().ToLower() == "teacher"
           ? (reader["TeacherEmail"] == DBNull.Value ? "" : reader["TeacherEmail"].ToString())

           : "",


                            mobile = reader["userType"] != DBNull.Value
             && reader["userType"].ToString().ToLower() == "student"

             ? (reader["MobileNo"] == DBNull.Value ? 0 : Convert.ToInt64(reader["MobileNo"]))

             : reader["userType"] != DBNull.Value
             && reader["userType"].ToString().ToLower() == "teacher"

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
                connection.Close();
            }
        }
        public List<OrderHistoryModel> GetBookOrderById(int id, int userId)
        {
            List<OrderHistoryModel> orders = new List<OrderHistoryModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBookOrderById");
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        orders.Add(new OrderHistoryModel
                        {
                            id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id"]),
                            addedBy = reader["addedBy"] == DBNull.Value ? "" : reader["addedBy"].ToString(),

                            orderId = reader["shortOrderId"] == DBNull.Value ? "" : reader["shortOrderId"].ToString(),

                            userId = reader["userId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["userId"]),

                            bookId = reader["bookId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bookId"]),

                            buyerId = reader["buyerId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["buyerId"]),

                            userType = reader["userType"] == DBNull.Value ? "" : reader["userType"].ToString(),

                            orderDate = reader["orderDate"] == DBNull.Value
                ? DateTime.MinValue
                : Convert.ToDateTime(reader["orderDate"]),

                            orderDateString = reader["orderDate"] == DBNull.Value
                ? ""
                : Convert.ToDateTime(reader["orderDate"]).ToString("dd-MMM-yyyy"),


                            lateFine = reader["lateFine"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["lateFine"]),

                            damageFine = reader["damageFine"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damageFine"]),

                            lostFine = reader["lostFine"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["lostFine"]),

                            quantity = reader["quantity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["quantity"]),

                            price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]),


                            returnDate = reader["returnDate"] == DBNull.Value
                ? ""
                : Convert.ToDateTime(reader["returnDate"]).ToString("dd-MMM-yyyy"),


                            name = reader["userType"]?.ToString().ToLower() == "student"
            ? (reader["StudentName"] == DBNull.Value ? "" : reader["StudentName"].ToString())
            : reader["userType"]?.ToString().ToLower() == "teacher"
            ? (reader["TeacherName"] == DBNull.Value ? "" : reader["TeacherName"].ToString())
            : "",


                            email = reader["userType"]?.ToString().ToLower() == "student"
            ? (reader["StudentEmail"] == DBNull.Value ? "" : reader["StudentEmail"].ToString())
            : reader["userType"]?.ToString().ToLower() == "teacher"
            ? (reader["TeacherEmail"] == DBNull.Value ? "" : reader["TeacherEmail"].ToString())
            : "",


                            mobile = reader["userType"]?.ToString().ToLower() == "student"
            ? (reader["MobileNo"] == DBNull.Value ? 0 : Convert.ToInt64(reader["MobileNo"]))
            : reader["userType"]?.ToString().ToLower() == "teacher"
            ? (reader["TeacherMobile"] == DBNull.Value ? 0 : Convert.ToInt64(reader["TeacherMobile"]))
            : 0,


                            recieveStatus = reader["recieveStatus"] == DBNull.Value
                    ? false
                    : Convert.ToBoolean(reader["recieveStatus"]),


                            bookName = reader["title"] == DBNull.Value ? "" : reader["title"].ToString()
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
        public List<OrderHistoryModel> DownloadRecieptOfBook(int id)
        {
            List<OrderHistoryModel> orders = new List<OrderHistoryModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBookOrderReciept");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string userType = reader["userType"] == DBNull.Value
     ? ""
     : reader["userType"].ToString();

                        orders.Add(new OrderHistoryModel
                        {
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

                            orderDateString = reader["orderDate"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(reader["orderDate"]).ToString("dd-MMM-yyyy"),

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

                            pricePerBook = reader["priceperbook"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["priceperbook"]),

                            returnDate = reader["returnDate"] == DBNull.Value
                                ? ""
                                : Convert.ToDateTime(reader["returnDate"]).ToString("dd-MMM-yyyy"),

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
                                : reader["title"].ToString(),

                            totalLateFine = reader["totalfinerc"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["totalfinerc"]),

                            totalDelayDaysCount = reader["latedaysrc"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["latedaysrc"]),

                            damageQuantity = reader["damagequantityrc"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["damagequantityrc"]),

                            receiveQuantity = reader["recievequantityrc"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["recievequantityrc"])
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
        public bool ReturnBookOrder(ReturnOrderModel model, out string errorMessage)
        {
            int result = 0;
            try
            {
                errorMessage = "";
                SqlCommand command = new SqlCommand("sp_ManageLibrary", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@action", "returnBookOrder");
                command.Parameters.AddWithValue("@userId", model.userId);
                command.Parameters.AddWithValue("@recievedBy", "admin");
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
        #endregion

        #region url management

        public List<UrlManagement> getUrlByUserInput(string input)
        {
            List<UrlManagement> urlmng = new List<UrlManagement>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_LogUrlManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectUrlByUserInput");
                cmd.Parameters.AddWithValue("@url", input);
                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {

                    while (rd.Read())
                    {
                        urlmng.Add(new UrlManagement
                        {
                            name = rd["name"]?.ToString(),
                            url = rd["url"]?.ToString()
                        });
                    }

                }
                return urlmng;
            }
            catch (Exception ex)
            {
                return urlmng;
            }
            finally
            {
                connection.Close();
            }
        }

        public List<UrlManagement> getUrlByUserFavorite(int userid)
        {
            List<UrlManagement> urlmng = new List<UrlManagement>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_LogUrlManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectTop5LogUrl");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {

                    while (rd.Read())
                    {
                        urlmng.Add(new UrlManagement
                        {
                            name = rd["name"]?.ToString(),
                            url = rd["url"]?.ToString()
                        });
                    }

                }
                return urlmng;
            }
            catch (Exception ex)
            {
                return urlmng;
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion
        #region Task Management
        public bool InsertTask(AddTaskModel sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTask", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", sm.id > 0 ? "updateTask" : "insertTask");
                cmd.Parameters.AddWithValue("@id", sm.id);
                cmd.Parameters.AddWithValue("@userId", sm.userId);
                cmd.Parameters.AddWithValue("@title", sm.title);
                cmd.Parameters.AddWithValue("@teacherId", sm.teacherId);
                cmd.Parameters.AddWithValue("@description", sm.description);
                cmd.Parameters.AddWithValue("@completionDate", sm.completionDate);
                cmd.Parameters.AddWithValue("@attachment", sm.attachmentName);
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
        public List<AddTaskModel> GetAllTaskList(int userId,int? academicYear=null)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTask", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAllTask");
                cmd.Parameters.AddWithValue("@academicYear", SqlDbType.Int).Value=(object)academicYear??DBNull.Value;
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<AddTaskModel> list = new List<AddTaskModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        int taskStatusValue = Convert.ToInt32(res["Taskstatus"]);

                        list.Add(new AddTaskModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            title = res["title"].ToString(),
                            description = res["description"].ToString(),
                            teacherId = Convert.ToInt32(res["teacherId"]),
                            teacherName = res["teachername"].ToString(),
                            attachmentName = res["attachment"].ToString(),
                            completionDateString = Convert.ToDateTime(res["completionDate"]).ToString("dd-MMM-yyyy"),
                            taskStatus = taskStatusValue,
                            taskStatusString = taskStatusValue == 1 ? "Submitted" : "Pending"
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
        public List<AddTaskModel> GetTaskById(int id, int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTask", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectTaskById");
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                List<AddTaskModel> list = new List<AddTaskModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new AddTaskModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            title = res["title"].ToString(),
                            description = res["description"].ToString(),
                            teacherId = Convert.ToInt32(res["teacherId"]),
                            teacherName = res["teachername"].ToString(),
                            attachmentName = res["attachment"].ToString(),
                            completionDateString = Convert.ToDateTime(res["completionDate"]).ToString("yyyy-MM-dd")
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

        #region Add Librarian       

        public bool InsertLibrarian(LibrarianModel bs, out string errorMessage)
        {
            errorMessage = "";

            // Validate document file extension
            if (!string.IsNullOrEmpty(bs.DocumentName) && !IsAllowedFile(bs.DocumentName))
            {
                errorMessage = "Invalid Document file type. Only .jpg, .jpeg, .png, .pdf files are allowed.";
                return false;
            }

            // Validate profile file extension
            if (!string.IsNullOrEmpty(bs.ProfileName) && !IsAllowedFile(bs.ProfileName))
            {
                errorMessage = "Invalid Profile file type. Only .jpg, .jpeg, .png, .pdf files are allowed.";
                return false;
            }

            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrarianRegistration", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", bs.Id > 0 ? "updateLibrarian" : "insertLibrarian");
                cmd.Parameters.AddWithValue("@id", bs.Id);
                cmd.Parameters.AddWithValue("@EmployeeId", bs.EmployeeId);
                cmd.Parameters.AddWithValue("@userId", bs.UserId);
                cmd.Parameters.AddWithValue("@name", bs.Name);
                cmd.Parameters.AddWithValue("@email", bs.Email);
                cmd.Parameters.AddWithValue("@mobileNo", bs.MobileNo);
                cmd.Parameters.AddWithValue("@dob", bs.DOB);
                cmd.Parameters.AddWithValue("@stateId", bs.StateId);
                cmd.Parameters.AddWithValue("@cityId", bs.CityId);
                cmd.Parameters.AddWithValue("@adharNo", bs.AdharNo);
                cmd.Parameters.AddWithValue("@gender", bs.Gender);
                cmd.Parameters.AddWithValue("@address", bs.Address);
                cmd.Parameters.AddWithValue("@document", bs.DocumentName);
                cmd.Parameters.AddWithValue("@profile", bs.ProfileName);
                cmd.Parameters.AddWithValue("@userAction", bs.UserAction);

                int id = 0;
                string eres = cmd.ExecuteScalar()?.ToString();
                bool parseres = int.TryParse(eres, out id);
                if (parseres) id = Convert.ToInt32(eres);

                if (id > 0 || eres == "insert")
                {

                    string librarianUsername = bs.Name.Trim().Split(' ')[0].Contains('.') ? bs.Name.Split('.')[1].Trim().Split(' ')[0] : bs.Name.Split(' ')[0];

                    string mobilePart = bs.MobileNo.ToString();
                    mobilePart = mobilePart.Substring(6);

                    string username = $"{librarianUsername}@{mobilePart}";
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
                    cmd.Parameters.AddWithValue("@userId", id > 0 ? id : bs.Id);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@role", "librarian");

                    int res2 = cmd.ExecuteNonQuery();
                    if (res2 > 0)
                    {
                        string subject = "Login Credential";
                        string body = $"<p>We are pleased to inform you that your account has been successfully created on the portal.</p><p><strong>Your login credentials are as follows:</strong></p><ul><li><strong>Username:</strong> {username}</li><li><strong>Password:</strong> {password}</li></ul>";
                        CommonMessage mailres = _mail.SendEmail(subject, body, bs.Email);
                        if (mailres.status)
                        {
                            transaction.Commit();
                            return true;
                        }
                    }

                    transaction.Rollback();
                    errorMessage = "Some error occurred";
                    return false;
                }
                else if (bs.Id > 0 && eres == "update")
                {
                    transaction.Commit();
                    return true;
                }

                transaction.Rollback();
                errorMessage = "Some error occurred";
                return false;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

     

        public List<LibrarianModel> GetLibrarian(int userid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrarianRegistration", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectLibrarian");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                List<LibrarianModel> list = new List<LibrarianModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new LibrarianModel
                        {
                            Id = Convert.ToInt32(res["id"]),
                            EmployeeId = res["EmployeeId"].ToString(),
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
                            ProfileName = res["profile"].ToString(),
                            username = res["username"] != DBNull.Value ? res["username"].ToString():null,
                            password = res["password"] != DBNull.Value ? res["password"].ToString():null,
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
        public List<LibrarianModel> GetLibrarianById(int id, int userid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrarianRegistration", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectLibrarianById");
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                List<LibrarianModel> list = new List<LibrarianModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new LibrarianModel
                        {
                            Id = Convert.ToInt32(res["id"]),
                            EmployeeId = res["EmployeeId"].ToString(),
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();

            }
        }
        #endregion
        #region Admin Dashboard Count
        public AdminDashboardCount GetAdminDashboardCount( int userid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_AdminDashboardCount", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "AdmindashboardCount");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                AdminDashboardCount data = new AdminDashboardCount();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        data.admissionEnquiry = Convert.ToInt32(res["admissionEnquiry"]);
                        data.admissionInterview = Convert.ToInt32(res["admissionInterview"]);
                        data.admissionExam = Convert.ToInt32(res["admissionExam"]);
                        data.admissionShortList = Convert.ToInt32(res["admissionShortList"]);
                        data.admissionAdmitted = Convert.ToInt32(res["admissionAdmitted"]);
                        data.admissionFormIssued = Convert.ToInt32(res["admissionFormIssued"]);
                    }
                }
                return data;
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

        #region notice
        public List<StaffModels> GetStaffByDepartment(string department, int userid)
        {
            List<StaffModels> staffList = new List<StaffModels>();
            string designation = "Staff";
            switch (department)
            {
                case "Teacher":
                    designation = "Teacher";
                    break;
                case "Warden":
                    designation = "Hostel Warden";
                    break;
                case "Security":
                    designation = "Security Guard";
                    break;
                case "BusDriver":
                    designation = "Bus Driver";
                    break;
                case "Librarian":
                    designation = "Library Incharge";
                    break;
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_GetStaffByDepartment", connection);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Department", department);
                cmd.Parameters.AddWithValue("@userid", userid);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    staffList.Add(new StaffModels
                    {
                        StaffId = Convert.ToInt32(rdr["StaffId"]),
                        UserId = Convert.ToInt32(rdr["userid"]),
                        Name = rdr["Name"]?.ToString(),
                        Mobile = rdr["Mobile"]?.ToString(),
                        Address = rdr["Address"]?.ToString(),
                        Designation = designation
                    });
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            

            return staffList;
        }

        public List<College_ERP.Models.Teacher.StudentModel> StudentsByClassAndSection(int classId, int sectionId, string academicYear,int userid)
        {
            List<College_ERP.Models.Teacher.StudentModel> students = new List<College_ERP.Models.Teacher.StudentModel>();
            
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action","SelectStudentByClassAndSection");
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                cmd.Parameters.AddWithValue("@academicyear", academicYear);
                cmd.Parameters.AddWithValue("@userid", userid);

                connection.Open();
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
        public List<College_ERP.Models.Teacher.StudentModel> StudentsByClassSection(int classId, int sectionId,int userid)
        {
            List<College_ERP.Models.Teacher.StudentModel> students = new List<College_ERP.Models.Teacher.StudentModel>();
            
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SelectStudentByClassSection");
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                cmd.Parameters.AddWithValue("@userid", userid);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    students.Add(new College_ERP.Models.Teacher.StudentModel
                    {
                        StudentId = Convert.ToInt32(reader["StudentId"]),
                        StudentName = reader["StudentName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        StudentEmail = reader["StudentEmail"].ToString(),
                        FatherName = reader["FatherName"].ToString(),
                    });
                }
            

            return students;
        }
        //public List<College_ERP.Models.Teacher.StudentModel> StudentsByClassSectionForBus(int classId, int sectionId,int userid)
        //{
        //    List<College_ERP.Models.Teacher.StudentModel> students = new List<College_ERP.Models.Teacher.StudentModel>();
            
        //        SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@Action", "SelectStudentByClassSectionforBus");
        //        cmd.Parameters.AddWithValue("@ClassId", classId);
        //        cmd.Parameters.AddWithValue("@SectionId", sectionId);
        //        cmd.Parameters.AddWithValue("@userid", userid);

        //        connection.Open();
        //        SqlDataReader reader = cmd.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            students.Add(new College_ERP.Models.Teacher.StudentModel
        //            {
        //                StudentId = Convert.ToInt32(reader["StudentId"]),
        //                StudentName = reader["StudentName"].ToString(),
        //                Gender = reader["Gender"].ToString(),
        //                StudentEmail = reader["StudentEmail"].ToString(),
        //                FatherName = reader["FatherName"].ToString(),
        //            });
        //        }
            

        //    return students;
        //}

        public bool InsertNotice(NoticeModel model,int userid, HttpPostedFileBase Attachment, out string errorMessage)
        {
            int res = 0;
            errorMessage = "";

            try
            {
                model.Attachment = Attachment;

                string attachmentPath = string.Empty;
                if (model.Attachment != null)
                {
                    attachmentPath = UploadImageToServer(model.Attachment);
                }

                SqlCommand command = new SqlCommand("sp_NoticeManagement", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "Insert Notice");
                command.Parameters.AddWithValue("@UserId", userid);
                command.Parameters.AddWithValue("@ReceiverId", (object)model.ReceiverId ?? DBNull.Value);
                
                command.Parameters.AddWithValue("@UserType", model.UserType);
                command.Parameters.AddWithValue("@Title", model.Title);
                command.Parameters.AddWithValue("@Description", model.Description);
                command.Parameters.AddWithValue("@AllTeacherStatus", model.AllTeacherStatus);
                command.Parameters.AddWithValue("@AllWardenStatus", model.AllWardenStatus);
                command.Parameters.AddWithValue("@AllSecurityStatus", model.AllSecurityStatus);
                command.Parameters.AddWithValue("@AllDriverStatus", model.AllDriverStatus);
                command.Parameters.AddWithValue("@AllLibrarianStatus", model.AllLibrarianStatus);
                command.Parameters.AddWithValue("@AllStudentStatus", model.AllStudentStatus);
                command.Parameters.AddWithValue("@AllParentStatus", model.AllParentStatus);
                command.Parameters.AddWithValue("@IsSentToBothStudentParent", model.IsSentToBothStudentParent);
                command.Parameters.AddWithValue("@Attachment", (object)attachmentPath ?? DBNull.Value);

                connection.Open();
                res = command.ExecuteNonQuery();

                if (res <= 0)
                    errorMessage = "Something went wrong";

                return res > 0;
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

        public List<NoticeModel> GetAllNotices(int userid)
        {
            List<NoticeModel> list = new List<NoticeModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_NoticeManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "Select All");
                cmd.Parameters.AddWithValue("@UserId", userid);

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    NoticeModel notice = new NoticeModel
                    {
                        NoticeId = Convert.ToInt32(dr["noticeId"]),
                        Title = dr["title"].ToString(),
                        Description = dr["description"].ToString(),
                        Attachments = dr["attachment"]?.ToString(),
                        UserType = dr["usertype"].ToString(),
                        ReceiverId = dr["ReceiverId"] != DBNull.Value ? Convert.ToInt32(dr["ReceiverId"]) : (int?)null,
                        ReceiverName = dr["ReceiverName"]?.ToString(),


                    };
                    list.Add(notice);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
           
            return list;
        }

        public string DeleteNotice(int noticeid)
        {

            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_NoticeManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "Delete Notice");
                cmd.Parameters.AddWithValue("@NoticeId", noticeid);
                connection.Open();

                cmd.ExecuteNonQuery();
                result = "Success";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public NoticeModel GetNoticeById(int noticeId)
        {
            NoticeModel model = new NoticeModel();

            try
            {
                SqlCommand command = new SqlCommand("sp_NoticeManagement", connection);                
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Action", "GetNoticeById");
                    command.Parameters.AddWithValue("@NoticeId", noticeId);

                    connection.Open();
                SqlDataReader dr = command.ExecuteReader();
                    if (dr.Read())
                    {
                            model.NoticeId = Convert.ToInt32(dr["NoticeId"]);
                            model.Title = dr["Title"].ToString();
                            model.Description = dr["Description"].ToString();
                            model.UserType = dr["UserType"].ToString();
                            model.ReceiverId = dr["ReceiverId"] != DBNull.Value ? Convert.ToInt32(dr["ReceiverId"]) : (int?)null;
                            model.AllTeacherStatus = Convert.ToBoolean(dr["AllTeacherStatus"]);
                            model.AllWardenStatus = Convert.ToBoolean(dr["AllWardenStatus"]);
                            model.AllSecurityStatus = Convert.ToBoolean(dr["AllSecurityStatus"]);
                            model.AllDriverStatus = Convert.ToBoolean(dr["AllDriverStatus"]);
                            model.AllLibrarianStatus = Convert.ToBoolean(dr["AllLibrarianStatus"]);
                            model.AllStudentStatus = Convert.ToBoolean(dr["AllStudentStatus"]);
                            model.AllParentStatus = Convert.ToBoolean(dr["AllParentStatus"]);
                            model.IsSentToBothStudentParent = dr["IsSentToBothStudentParent"] != DBNull.Value ? Convert.ToInt32(dr["IsSentToBothStudentParent"]): 0;
                            model.Attachments = dr["Attachment"]?.ToString();
                            model.ClassId = dr["ClassId"] != DBNull.Value ? Convert.ToInt32(dr["ClassId"]) : 0;
                             model.SectionId = dr["SectionId"] != DBNull.Value ? Convert.ToInt32(dr["SectionId"]) : 0;
                             model.AcademicYear = dr["AcademicYear"]?.ToString();
                             model.ReceiverName = dr["ReceiverName"]?.ToString();

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

        public bool UpdateNotice(NoticeModel model, out string errorMessage)
        {
            int res = 0;
            errorMessage = "";

            try
            {
              
                string attachmentPath = string.Empty;

                if (model.Attachment != null)
                {
                    attachmentPath = UploadImageToServer(model.Attachment);
                }

                using (SqlCommand command = new SqlCommand("sp_NoticeManagement", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Action", "UpdateNotice");
                    command.Parameters.AddWithValue("@NoticeId", model.NoticeId);
                    command.Parameters.AddWithValue("@ReceiverId", (object)model.ReceiverId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UserType", model.UserType);
                    command.Parameters.AddWithValue("@Title", model.Title);
                    command.Parameters.AddWithValue("@Description", model.Description);
                    command.Parameters.AddWithValue("@AllTeacherStatus", model.AllTeacherStatus);
                    command.Parameters.AddWithValue("@AllWardenStatus", model.AllWardenStatus);
                    command.Parameters.AddWithValue("@AllSecurityStatus", model.AllSecurityStatus);
                    command.Parameters.AddWithValue("@AllDriverStatus", model.AllDriverStatus);
                    command.Parameters.AddWithValue("@AllLibrarianStatus", model.AllLibrarianStatus);
                    command.Parameters.AddWithValue("@AllStudentStatus", model.AllStudentStatus);
                    command.Parameters.AddWithValue("@AllParentStatus", model.AllParentStatus);
                    command.Parameters.AddWithValue("@IsSentToBothStudentParent", model.IsSentToBothStudentParent);
                    command.Parameters.AddWithValue("@Attachment", (object)attachmentPath ?? DBNull.Value);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }

                if (res <= 0)
                {
                    errorMessage = "Something went wrong while updating.";
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
                connection.Close();
            }
        }


        #endregion
        public bool PromoteStudent(PromoteStudentModel data)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StudentId", data.studentId);
                cmd.Parameters.AddWithValue("@ClassId", data.classId);
                if(data.sectionId>0)
                cmd.Parameters.AddWithValue("@SectionId", data.sectionId);
                cmd.Parameters.AddWithValue("@action", "PromoteStudent");
                connection.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
        public List<SubjectTimeModel> ShowTimeScheduleToTeacher(int classid, int sectionid, int subjectid)
        {
            try
            {
                List<SubjectTimeModel> list = new List<SubjectTimeModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "SelectTimeForTeacher");
                cmd.Parameters.AddWithValue("@classId", classid);
                cmd.Parameters.AddWithValue("@sectionId", sectionid);
                cmd.Parameters.AddWithValue("@subjectId", subjectid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    SubjectTimeModel category = new SubjectTimeModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        subjectName = res["subject"].ToString(),
                        subjectId = Convert.ToInt32(res["subjectId"]),
                        from = DateTime.Today.Add((TimeSpan)res["fromTime"]).ToString("hh:mm tt"),
                        to = DateTime.Today.Add((TimeSpan)res["toTime"]).ToString("hh:mm tt"),
                        day = res["day"].ToString(),
                        tid = Convert.ToInt32(res["tid"])
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
        public List<RegistrationModel> GetAdmittedStudents(int userid,string year=null)
        {
            List<RegistrationModel> list = new List<RegistrationModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectAdmittedStudents");
                //cmd.Parameters.Add("@year",(object)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();

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
                        SectionId = rdr["SectionId"]!=DBNull.Value? Convert.ToInt32(rdr["SectionId"]):0,
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
                        AcademicYear = rdr["AcademicYear"]?.ToString(),
                        AdmissionStage = rdr["AdmissionStage"]?.ToString(),
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
        #region Syllabus Management
        public bool InsertSyllabus(AddSyllabusMoedel sm ,out string errormsg)
        {
            errormsg = "";
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageSyllabus", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", sm.id > 0 ? "updateSyllabus" : "insertSyllabus");
                cmd.Parameters.AddWithValue("@id", sm.id);
                cmd.Parameters.AddWithValue("@userId", sm.userId);
                cmd.Parameters.AddWithValue("@classId", sm.classId);
                cmd.Parameters.AddWithValue("@classStreamId", sm.classStreamId);
                cmd.Parameters.AddWithValue("@subjectId", sm.subjectId);
                cmd.Parameters.AddWithValue("@academicYear", sm.academicYear);
                cmd.Parameters.AddWithValue("@attachment", sm.attachmentName);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    errormsg = "Some error occured while processing your request.";
                    return false;
                }
                return res > 0;
            }
            catch(Exception ex)
            {
                errormsg = ex.Message;
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public List<AddSyllabusMoedel> GetAllSyllabus(int userid)
        {
            try
            {
                List<AddSyllabusMoedel> list = new List<AddSyllabusMoedel>();
                SqlCommand cmd = new SqlCommand("sp_ManageSyllabus", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectallsyllabus");
                cmd.Parameters.AddWithValue("@userId", userid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add( new AddSyllabusMoedel
                    {
                        id = Convert.ToInt32(res["id"]),
                        subjectName = res["subject"].ToString(),
                        subjectId = Convert.ToInt32(res["subjectId"]),
                        classId = Convert.ToInt32(res["classId"]),
                        className = res["classname"].ToString(),
                        classstream = res["stream"].ToString(),
                        institutionType = res["InstitutionType"].ToString(),
                        educationLevel = res["EducationLevel"].ToString(),
                        academicYear = res["academicYear"].ToString(),
                        attachmentName = res["attachment"].ToString(),
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
        public bool DeleteSyllabus(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageSyllabus", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteSyllabus");
                cmd.Parameters.AddWithValue("@id", id);
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
        public List<AddSyllabusMoedel> GetSyllabusById(int id)
        {
            try
            {
                List<AddSyllabusMoedel> list = new List<AddSyllabusMoedel>();
                SqlCommand cmd = new SqlCommand("sp_ManageSyllabus", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectsyllabusbyid");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new AddSyllabusMoedel
                    {
                        id = Convert.ToInt32(res["id"]),
                        subjectName = res["subject"].ToString(),
                        subjectId = Convert.ToInt32(res["subjectId"]),
                        institutionType = res["InstitutionType"].ToString(),
                        educationLevel = res["EducationLevel"].ToString(),
                        classId = Convert.ToInt32(res["classId"]),
                        className = res["classname"].ToString(),
                        academicYear = res["academicYear"].ToString(),
                        attachmentName = res["attachment"].ToString(),
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
        #region Exam TimeTable Management
        public bool InsertExamTimeTable(AddExamTimeTableModel sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageExamTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", sm.id > 0 ? "updateExamTimeTable" : "insertExamTimeTable");
                cmd.Parameters.AddWithValue("@id", sm.id);
                cmd.Parameters.AddWithValue("@userId", sm.userId);
                cmd.Parameters.AddWithValue("@classId", sm.classId);
                cmd.Parameters.AddWithValue("@academicYear", sm.academicYear);
                cmd.Parameters.AddWithValue("@attachment", sm.attachmentName);
                cmd.Parameters.AddWithValue("@description", sm.description);
                cmd.Parameters.AddWithValue("@examName", sm.examName);
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
        public List<ExamModel> GetAllExamName(int userid)
        {
            try
            {
                List<ExamModel> list = new List<ExamModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageExamTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectscheduledexams");
                cmd.Parameters.AddWithValue("@userId", userid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new ExamModel
                    {
                        ExamId = Convert.ToInt32(res["id"]),
                        ExamName = res["examname"].ToString()
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
        public List<AddExamTimeTableModel> GetAllExamTimeTable(int userid)
        {
            try
            {
                List<AddExamTimeTableModel> list = new List<AddExamTimeTableModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageExamTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectallExamTimeTable");
                cmd.Parameters.AddWithValue("@userId", userid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new AddExamTimeTableModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        classId = Convert.ToInt32(res["classId"]),
                        className = res["classname"].ToString(),
                        academicYear = res["academicYear"].ToString(),
                        attachmentName = res["attachment"].ToString(),
                        description = res["description"].ToString(),
                        examName = res["examname"].ToString()
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
        public bool DeleteExamTimeTable(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageExamTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteExamTimeTable");
                cmd.Parameters.AddWithValue("@id", id);
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
        public List<AddExamTimeTableModel> GetExamTimeTableById(int id)
        {
            try
            {
                List<AddExamTimeTableModel> list = new List<AddExamTimeTableModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageExamTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectExamTimeTablebyid");
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new AddExamTimeTableModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        classId = Convert.ToInt32(res["classId"]),
                        className = res["classname"].ToString(),
                        academicYear = res["academicYear"].ToString(),
                        attachmentName = res["attachment"].ToString(),
                        description = res["description"].ToString(),
                        examName = res["examName"].ToString(),
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
        #region Leave Management

       public int TotalLeaveCount(int id)
        {
            SqlDataReader res = null;
            try
            {
                List<LeaveRequestsModel> list = new List<LeaveRequestsModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageLeaveRequests", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectleavedataForleaveCount");
                cmd.Parameters.AddWithValue("@teacherId", id);
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                res = cmd.ExecuteReader();
                int leaveCount = 0;
                while (res.Read())
                {
                    int Count = (Convert.ToDateTime(res["toDate"]) - Convert.ToDateTime(res["fromDate"])).Days;
                    leaveCount += Count;
                }

                return leaveCount;
            }catch(Exception ex)
            {
                return 0;
            }
            finally
            {
                if (res != null)
                    res.Close();
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public List<LeaveRequestsModel> GetAllLeaveRequst(int userid,int? academicYear=null)
        {
            try
            {
                List<LeaveRequestsModel> list = new List<LeaveRequestsModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageLeaveRequests", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectallleaveRequestforadmin");
                cmd.Parameters.AddWithValue("@academicYear",SqlDbType.Int).Value=(object)academicYear?? DBNull.Value;
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
         
                while (res.Read())
                {
                 
                    list.Add(new LeaveRequestsModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        teacherId = Convert.ToInt32(res["teacherId"]),
                        teacherName = res["teacherName"].ToString(),
                        reason = res["reason"].ToString(),
                        approvalStatus = Convert.ToInt32(res["approvalStatus"]),
                        attachmentName = res["attachment"].ToString(),
                        fromDate = Convert.ToDateTime(res["fromDate"]),
                        fromDateString = Convert.ToDateTime(res["fromDate"]).ToString("dd-MMM-yyyy"),
                        toDateString = Convert.ToDateTime(res["toDate"]).ToString("dd-MMM-yyyy"),
                        toDate = Convert.ToDateTime(res["toDate"]),
                        leaveType = res["leaveType"].ToString()
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
        public List<LeaveRequestsModel> LeaveRequstHistoryOfTeacher(int id)
        {
            try
            {
                List<LeaveRequestsModel> list = new List<LeaveRequestsModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageLeaveRequests", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectallofteacher");
                cmd.Parameters.AddWithValue("@teacherId", id);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
         
                while (res.Read())
                {
                 
                    list.Add(new LeaveRequestsModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        teacherId = Convert.ToInt32(res["teacherId"]),
                        teacherName = res["teacherName"].ToString(),
                        reason = res["reason"].ToString(),
                        approvalStatus = Convert.ToInt32(res["approvalStatus"]),
                        attachmentName = res["attachment"].ToString(),
                        fromDate = Convert.ToDateTime(res["fromDate"]),
                        fromDateString = Convert.ToDateTime(res["fromDate"]).ToString("dd-MMM-yyyy"),
                        toDateString = Convert.ToDateTime(res["toDate"]).ToString("dd-MMM-yyyy"),
                        toDate = Convert.ToDateTime(res["toDate"]),
                        leaveType = res["leaveType"].ToString()
                    });
           
                }
                if (res != null)
                {
                    res.Close();
                }
                int count = TotalLeaveCount(id);
                foreach (var item in list)
                {
                    item.leaveCount = count;
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
        public bool AcceptRejectLeave(int userid,int id,int status,string remark)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLeaveRequests", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "acceptrejectleave");
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@userId",userid);
                cmd.Parameters.AddWithValue("@approvalStatus", status);
                cmd.Parameters.AddWithValue("@remark", remark!=null?remark:null);
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

        #region AllFeesManagement
        public bool InsertAllFeeRecord(AllFeeRecordModel model, out string errorMessage)
        {
            int res = 0;
            errorMessage = "";
            try
            {
                string attachmentPath = string.Empty;
                if (model.FeeSlip != null)
                {
                    attachmentPath = UploadImageToServer(model.FeeSlip);
                }
                SqlCommand command = new SqlCommand("sp_ManageAllFeeRecord", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@userid", model.userid);
                command.Parameters.AddWithValue("@ClassId", model.ClassId);
                command.Parameters.AddWithValue("@SectionId", model.SectionId);
                command.Parameters.AddWithValue("@StudentId", model.StudentId);
                command.Parameters.AddWithValue("@AcademicYear", model.AcademicYear);
                command.Parameters.AddWithValue("@BillingPeriod", model.BillingPeriod);
                command.Parameters.AddWithValue("@BillingMonth", model.BillingMonth);
                if (model.BillingMonth!=null)
                {
                    command.Parameters.AddWithValue("@FeesPaid", model.Amount);
                }
                else
                {
                    command.Parameters.AddWithValue("@FeesPaid", model.FeesPaid);
                }
                    command.Parameters.AddWithValue("@RemainingFees", model.RemainingFees);
                command.Parameters.AddWithValue("@Amount", model.Amount);
                command.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);
                command.Parameters.AddWithValue("@PaidDate", model.PaymentDate);
                command.Parameters.AddWithValue("@TransactionId", model.transactionid);
                if (model.FeeSlip != null)
                {
                    command.Parameters.AddWithValue("@FeeSlip", attachmentPath);
                }
                connection.Open();
                res = command.ExecuteNonQuery();

                if (res <= 0)
                    errorMessage = "Something went wrong";

                return res > 0;
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
        public List<AllFeeRecordModel> GetAllFeeRecord(int userid)
        {
            try
            {
                List<AllFeeRecordModel> list = new List<AllFeeRecordModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageAllFeeRecord", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTAll");
                cmd.Parameters.AddWithValue("@UserId", userid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new AllFeeRecordModel
                    {
                        FeeId = Convert.ToInt32(res["FeeId"]),
                        StudentId = Convert.ToInt32(res["StudentId"]),
                        ClassId = Convert.ToInt32(res["ClassId"]),
                        SectionId = Convert.ToInt32(res["SectionId"]),
                        Amount = Convert.ToDecimal(res["Amount"]),
                        FeesPaid = res["FeesPaid"] != DBNull.Value ? Convert.ToDecimal(res["FeesPaid"]) : (decimal?)null,
                        RemainingFees = res["RemainingFees"] != DBNull.Value ? Convert.ToDecimal(res["RemainingFees"]) : (decimal?)null,

                        AcademicYear = res["AcademicYear"].ToString(),
                        BillingMonth = res["BillingMonth"].ToString(),
                        BillingPeriod = res["BillingPeriod"].ToString(),
                        ClassName = res["ClassName"].ToString(),
                        SectionName = res["SectionName"].ToString(),
                        StudentName = res["StudentName"].ToString(),
                        FatherName = res["FatherName"].ToString(),
                        PaymentMode = res["PaymentMode"].ToString(),
                        FeeSlips = res["FeeSlip"].ToString(),
                        PaymentDate = res["PaidDate"] != DBNull.Value ? Convert.ToDateTime(res["PaidDate"]) : DateTime.MinValue,
                        transactionid = res["TransactionId"].ToString()
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
        public List<AllFeeRecordModel> GetStudentTransactionHistory(int studentid)
        {
            try
            {
                List<AllFeeRecordModel> list = new List<AllFeeRecordModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageAllFeeRecord", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selecttransactions");
                cmd.Parameters.AddWithValue("@StudentId", studentid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new AllFeeRecordModel
                    {
                        FeeId = Convert.ToInt32(res["FeeId"]),
                        StudentId = Convert.ToInt32(res["StudentId"]),
                        ClassId = Convert.ToInt32(res["ClassId"]),
                        SectionId = Convert.ToInt32(res["SectionId"]),
                        Amount = Convert.ToDecimal(res["Amount"]),
                        FeesPaid = res["FeesPaid"] != DBNull.Value ? Convert.ToDecimal(res["FeesPaid"]) : (decimal?)null,
                        RemainingFees = res["RemainingFees"] != DBNull.Value ? Convert.ToDecimal(res["RemainingFees"]) : (decimal?)null,
                        AcademicYear = res["AcademicYear"].ToString(),
                        BillingMonth = res["BillingMonth"].ToString(),
                        BillingPeriod = res["BillingPeriod"].ToString(),
                        StudentName = res["studentname"].ToString(),
                        FeeSlips = res["FeeSlip"].ToString(),
                        PaymentDate = res["PaidDate"] != DBNull.Value ? Convert.ToDateTime(res["PaidDate"]) : DateTime.MinValue,
                        transactionid = res["TransactionId"].ToString()
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
        public List<AllFeeRecordModel> GetLatestFeeRecordOfAllStudents(int userid)
        {
            try
            {
                List<AllFeeRecordModel> list = new List<AllFeeRecordModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageAllFeeRecord", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectLatestFeeRecordOfAllStudents");
                cmd.Parameters.AddWithValue("@UserId", userid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new AllFeeRecordModel
                    {
                        FeeId = Convert.ToInt32(res["FeeId"]),
                        StudentId = Convert.ToInt32(res["StudentId"]),
                        ClassId = Convert.ToInt32(res["ClassId"]),
                        SectionId = Convert.ToInt32(res["SectionId"]),
                        Amount = Convert.ToDecimal(res["Amount"]),
                        FeesPaid = res["TotalFeesPaid"] != DBNull.Value ? Convert.ToDecimal(res["TotalFeesPaid"]) : (decimal?)null,
                        RemainingFees = res["RemainingFees"] != DBNull.Value ? Convert.ToDecimal(res["RemainingFees"]) : (decimal?)null,

                        AcademicYear = res["AcademicYear"].ToString(),
                        BillingMonth = res["BillingMonth"].ToString(),
                        BillingPeriod = res["BillingPeriod"].ToString(),
                        ClassName = res["ClassName"].ToString(),
                        SectionName = res["SectionName"].ToString(),
                        StudentName = res["StudentName"].ToString(),
                        FatherName = res["FatherName"].ToString(),
                        PaymentMode = res["PaymentMode"].ToString(),
                        FeeSlips = res["FeeSlip"].ToString(),
                        PaymentDate = res["PaidDate"] != DBNull.Value ? Convert.ToDateTime(res["PaidDate"]) : DateTime.MinValue,
                        transactionid = res["TransactionId"].ToString()
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

        public AllFeeRecordModel GetAllFeeRecordById(int id)
        {
            AllFeeRecordModel model = null;
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageAllFeeRecord", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SelectById");
                cmd.Parameters.AddWithValue("@FeeId", id);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    model = new AllFeeRecordModel
                    {
                        FeeId = Convert.ToInt32(reader["FeeId"]),
                        ClassId = Convert.ToInt32(reader["ClassId"]),
                        SectionId = Convert.ToInt32(reader["SectionId"]),
                        StudentId = Convert.ToInt32(reader["StudentId"]),
                        AcademicYear = reader["AcademicYear"].ToString(),
                        BillingPeriod = reader["BillingPeriod"].ToString(),
                        BillingMonth = reader["BillingMonth"].ToString(),
                        Amount = Convert.ToDecimal(reader["Amount"]),
                        FeesPaid = Convert.ToDecimal(reader["FeesPaid"]),
                        RemainingFees = Convert.ToDecimal(reader["RemainingFees"]),
                        PaymentMode = reader["PaymentMode"].ToString(),
                        FeeSlips = reader["FeeSlip"].ToString(),
                        PaymentDate = Convert.ToDateTime(reader["PaidDate"]),
                        transactionid = reader["TransactionId"].ToString()
                    };
                    
                }
                reader.Close();
            }
            finally
            {
                connection.Close();
            }
            return model;
        }

        public bool UpdateAllFeeRecord(AllFeeRecordModel model, out string errorMessage)
        {
            int res = 0;
            errorMessage = "";
            try
            {
                string attachmentPath = string.Empty;
                if (model.FeeSlip != null)
                {
                    attachmentPath = UploadImageToServer(model.FeeSlip);
                }
                SqlCommand command = new SqlCommand("sp_ManageAllFeeRecord", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", "update");
                command.Parameters.AddWithValue("@FeeId", model.FeeId);
                command.Parameters.AddWithValue("@ClassId", model.ClassId);
                command.Parameters.AddWithValue("@SectionId", model.SectionId);
                command.Parameters.AddWithValue("@StudentId", model.StudentId);
                command.Parameters.AddWithValue("@AcademicYear", model.AcademicYear);
                command.Parameters.AddWithValue("@BillingPeriod", model.BillingPeriod);
                command.Parameters.AddWithValue("@BillingMonth", model.BillingMonth);
                command.Parameters.AddWithValue("@Amount", model.Amount);
                command.Parameters.AddWithValue("@FeesPaid", model.FeesPaid);
                command.Parameters.AddWithValue("@RemainingFees", model.RemainingFees);
                command.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);
                command.Parameters.AddWithValue("@PaidDate", model.PaymentDate);
                command.Parameters.AddWithValue("@TransactionId", model.transactionid);
                if (model.FeeSlip != null)
                {
                    command.Parameters.AddWithValue("@FeeSlip", attachmentPath);
                }
                connection.Open();
                res = command.ExecuteNonQuery();

                if (res <= 0)
                    errorMessage = "Something went wrong";

                return res > 0;
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
        #endregion

        #region DashboardCount

        public AdminDashboardCountResult GetDashboardCounts(int userId, int? year)
        {
            AdminDashboardCountResult dash = new AdminDashboardCountResult();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_AdminDashboardCount", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userid", userId);
                cmd.Parameters.AddWithValue("@year", (object)year ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@action", "AdminDashboard");

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    dash= new AdminDashboardCountResult
                    {
                        totalteacher = Convert.ToInt32(reader["totalteacher"]),
                        totalbus = Convert.ToInt32(reader["totalbus"]),
                        totalhostelblocks = Convert.ToInt32(reader["totalhostelblocks"]),
                        totaladmittedstudent = Convert.ToInt32(reader["totaladmittedstudent"]),
                        totalsecurity = Convert.ToInt32(reader["totalsecurity"]),
                        totalbooks = Convert.ToInt32(reader["totalbooks"]),
                        totalleaverequest = Convert.ToInt32(reader["totalleaverequest"]),
                        totalassignedtasks = Convert.ToInt32(reader["totalassignedtasks"])
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

            return dash;
        }

        #endregion


        public List<SubjectModel> GetSubjectsByClassId(int teacherId, int classId)
        {
            var subjects = new List<SubjectModel>();


            SqlCommand cmd = new SqlCommand("sp_SubjectAssignManagement", connection);
               
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Actions", "GetSubjectsByClassId");
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    cmd.Parameters.AddWithValue("@ClassId", classId);

            connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            subjects.Add(new SubjectModel
                            {
                                SubjectId = Convert.ToInt32(reader["SubjectId"]),
                                Subject = reader["Subject"].ToString()
                            });
                        }
                    }
                
            

            return subjects;
        }
        public List<ClassModel> GetUnassignedClasses(int userId)
        {
            List<ClassModel> classList = new List<ClassModel>();

                SqlCommand cmd = new SqlCommand("sp_ManageRegistrationFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "SelectClassByRegistrationId");
                cmd.Parameters.AddWithValue("@userid", userId);

            connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    classList.Add(new ClassModel
                    {
                        ClassId = Convert.ToInt32(rdr["ClassId"]),
                        ClassName = rdr["ClassName"].ToString()
                    });
                }
            

            return classList;
        }
        public List<StudentModel> GetUnassignedStudentsByClassSection( int classId, int sectionId, string academicYear, int userId)
        {
            List<StudentModel> students = new List<StudentModel>();

           
                SqlCommand cmd = new SqlCommand("sp_ManageStudentInHostel", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "SelectClassByBlock");
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                cmd.Parameters.AddWithValue("@UserId", userId);

            connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                students.Add(new StudentModel
                {
                    StudentId = Convert.ToInt32(rdr["StudentId"]),
                    StudentName = rdr["StudentName"].ToString(),
                    MobileNo = rdr["MobileNo"].ToString(),
                    Gender = rdr["gender"].ToString()
                });
                }

                connection.Close();
            

            return students;
        }

        public List<BlockModel> GetBlockByWardenId(int userid)
        {
            List<BlockModel> list = new List<BlockModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_InsertWarden", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GetBlockByWardenId");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new BlockModel
                    {
                        HostelId = rdr["HostelId"] != DBNull.Value ? Convert.ToInt32(rdr["HostelId"]) : 0,
                        BlockName = rdr["BlockName"].ToString()
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

        #region Uplaod TimeTable 
        public bool UploadTimeTable(AddExamTimeTableModel sm)
        {
            string uniquefilename = null;
            if(sm.attachment!=null && sm.attachment.ContentLength > 0)
            {
                string filename = sm.attachment.FileName;
                uniquefilename = Guid.NewGuid() + "_" + filename;
                sm.attachmentName = "/Upload/" + uniquefilename;
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", sm.id > 0 ? "updateuploadTimeTable" : "uploadTimeTable");
                cmd.Parameters.AddWithValue("@id", sm.id);
                cmd.Parameters.AddWithValue("@userId", sm.userId);
                cmd.Parameters.AddWithValue("@classId", sm.classId);
                cmd.Parameters.AddWithValue("@sectionId", sm.sectionId);
                cmd.Parameters.AddWithValue("@academicYear", sm.academicYear);
                cmd.Parameters.AddWithValue("@attachment", sm.attachmentName);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res > 0 && uniquefilename != null)
                {
                    string filepath = HttpContext.Current.Server.MapPath("~/Upload/") + uniquefilename;
                    sm.attachment.SaveAs(filepath);
                }
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
        public List<AddExamTimeTableModel> GetUploadedTimeTableById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectuploadedtimetablebyid");
                cmd.Parameters.AddWithValue("@id", id);
                List<AddExamTimeTableModel> list = new List<AddExamTimeTableModel>();
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new AddExamTimeTableModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            academicYear = res["academicYear"].ToString(),
                            className = res["ClassName"].ToString(),
                            sectionName = res["sectionName"].ToString(),
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

        public bool updateUploadedTimeTable(AddExamTimeTableModel sm)
        {
            try
            {
                string filename = null;
                if (sm.attachment != null)
                {

                    filename = UploadImageToServer(sm.attachment);
                }

                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "updateUploadedTimeTable");
                cmd.Parameters.AddWithValue("@id", sm.id);
                cmd.Parameters.AddWithValue("@academicYear", sm.academicYear);
                cmd.Parameters.AddWithValue("@attachment", filename);
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


        public bool DeleteUploadedTimetable(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "Deleteuploadedtimetable");
                cmd.Parameters.AddWithValue("@id", id);
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
        public List<PickupPointModel> GetPickupPointsByBusId(int busid,int userid)
        {
            List<PickupPointModel> list = new List<PickupPointModel>();

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectroutebybusno");
                    cmd.Parameters.AddWithValue("@bussId", busid);
                    cmd.Parameters.AddWithValue("@userid", userid);

                    connection.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            PickupPointModel point = new PickupPointModel
                            {
                                id = Convert.ToInt32(rdr["id"]),
                                pickupPoint = rdr["pickupPoint"].ToString()
                            };
                            list.Add(point);
                        }
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

        #region graphs
        public List<Object> GetStudentYearWise(int id)
        {
                List<Object> list = new List<Object>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_AdminDashboardCount", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectStudentRecordYearWise");
                    cmd.Parameters.AddWithValue("@userid", id);

                    connection.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new
                            {
                                year = rdr["year"]?.ToString(),
                                totalStudent = rdr["totalStudent"]?.ToString()
                            });
                        }
                    }
                }
                return list;
            }catch(Exception ex)
            {
                return list;
            }
            finally
            {
                connection.Close();
            }
        }

        public List<Object> selectStudentClassWise(int id)
        {
            List<Object> list = new List<Object>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_AdminDashboardCount", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectStudentClassWise");
                    cmd.Parameters.AddWithValue("@userid", id);

                    connection.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new
                            {
                                className = rdr["className"]?.ToString(),
                                totalStudent = rdr["totalStudent"]?.ToString()
                            });
                        }
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
                connection.Close();
            }
        }

        public List<Object> selectStudentBlockWise(int id)
        {
            List<Object> list = new List<Object>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_AdminDashboardCount", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectStudentBlockWise");
                    cmd.Parameters.AddWithValue("@userid", id);

                    connection.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new
                            {
                                blockName = rdr["blockName"]?.ToString(),
                                totalStudent = rdr["totalStudent"]?.ToString()
                            });
                        }
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
                connection.Close();
            }
        }

        #endregion
        #region App Details
        public bool InsertAppDetail(AppDetailModel sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageAppDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@content", sm.content);
                cmd.Parameters.AddWithValue("@userid", sm.userid);
                cmd.Parameters.AddWithValue("@role", sm.role);
                cmd.Parameters.AddWithValue("@action",sm.type == "About"? "insertabout" : sm.type == "Contact"? "insertcontact" :sm.type == "PrivacyPolicy"? "insertprivacypolicy" : sm.type == "TermsAndConditions" ?"inserttermsandconditions":"");
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
        public AppDetailModel GetAppDetails(int userid)
        {
            try
            {
                AppDetailModel adm=null;
                using (SqlCommand cmd = new SqlCommand("sp_ManageAppDetails", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectappdetails");
                    cmd.Parameters.AddWithValue("@userid", userid);
                    connection.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            adm = new AppDetailModel
                            {
                                contact = rdr["contact"].ToString(),
                                about = rdr["about"].ToString(),
                            };
                        }
                    }
                }
                return adm;
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
        public AppDetailModel GetAppAbout(int userid)
        {
            try
            {
                AppDetailModel adm=null;
                using (SqlCommand cmd = new SqlCommand("sp_ManageAppDetails", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectabout");
                    cmd.Parameters.AddWithValue("@userid", userid);
                    connection.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            adm = new AppDetailModel
                            {
                                about = rdr["about"].ToString(),
                            };
                        }
                    }
                }
                return adm;
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
        public AppDetailModel GetAppContact(int userid)
        {
            try
            {
                AppDetailModel adm=null;
                using (SqlCommand cmd = new SqlCommand("sp_ManageAppDetails", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectcontact");
                    cmd.Parameters.AddWithValue("@userid", userid);
                    connection.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            adm = new AppDetailModel
                            {
                                contact = rdr["contact"].ToString(),
                            };
                        }
                    }
                }
                return adm;
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
        public PrivacyPolicyModel GetPrivacyPolicy(int userid,string role)
        {
            try
            {
                PrivacyPolicyModel adm = null;
                using (SqlCommand cmd = new SqlCommand("sp_ManageAppDetails", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectprivacypolicy");
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@role", role);
                    connection.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            adm = new PrivacyPolicyModel
                            {
                                privacyPolicy = rdr["privacyPolicy"].ToString(),
                                role = rdr["role"].ToString(),
                            };
                        }
                    }
                }
                return adm;
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
        public TermsAndConditions GetTermsAndConditions(int userid,string role)
        {
            try
            {
                TermsAndConditions adm = null;
                using (SqlCommand cmd = new SqlCommand("sp_ManageAppDetails", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selecttermsandconditions");
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@role", role);
                    connection.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            adm = new TermsAndConditions
                            {
                                termsAndConditions = rdr["termsandconditions"].ToString(),
                                role = rdr["role"].ToString(),
                            };
                        }
                    }
                }
                return adm;
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
        public bool InsertAppVersionDetail(AppVersionModel sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageAppDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@type", sm.Type);
                cmd.Parameters.AddWithValue("@userid", sm.userid);
                cmd.Parameters.AddWithValue("@oldVersion", sm.OldVersion);
                cmd.Parameters.AddWithValue("@currentVersion", sm.CurrentVersion);
                cmd.Parameters.AddWithValue("@updateUrl", sm.UpdateUrl);
                cmd.Parameters.AddWithValue("@action", "insertversiondetail");
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
        public AppVersionModel GetAppVersionDetail(int userid)
        {
            try
            {
                AppVersionModel adm = null;
                using (SqlCommand cmd = new SqlCommand("sp_ManageAppDetails", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectversiondetails");
                    cmd.Parameters.AddWithValue("@userid", userid);
                    connection.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            adm = new AppVersionModel
                            {
                                Type = rdr["type"].ToString(),
                                OldVersion = rdr["oldVersion"].ToString(),
                                CurrentVersion = rdr["currentVersion"].ToString(),
                                UpdateUrl = rdr["updateUrl"].ToString(),
                            };
                        }
                    }
                }
                return adm;
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


        #region excelAttendanceUpload

        public List<ExcelAttendanceRecordModel> GetAttendaceRecordByExcel(int id)
        {
            List<ExcelAttendanceRecordModel> attendanceList = new List<ExcelAttendanceRecordModel>();

            try
            {

                using (SqlCommand cmd = new SqlCommand("sp_InsertAttendanceRecord", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectAttendance");
                    cmd.Parameters.AddWithValue("@userid", id);
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        attendanceList.Add(new ExcelAttendanceRecordModel
                        {

                            DepartmentName = reader["DepartmentName"].ToString(),
                            EmployeeCode = reader["EmployeeCode"].ToString(),
                            EmployeeName = reader["EmployeeName"].ToString(),

                        });
                    }
                }

                return attendanceList;
            }catch(Exception ex)
            {
                return attendanceList;
            }
            finally
            {
                connection.Close();
            }
        }

        public List<ExcelAttendanceRecordModel>  GetAttendaceRecordById(int userid,string department, string empcode, string startDate, string endDate)
        {
            List<ExcelAttendanceRecordModel> attendanceList = new List<ExcelAttendanceRecordModel>();

            try
            {

                using (SqlCommand cmd = new SqlCommand("sp_InsertAttendanceRecord", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectAttendanceById");
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@DepartmentName", department);
                    cmd.Parameters.AddWithValue("@EmployeeCode", empcode);

                    cmd.Parameters.AddWithValue("@StartDate",
            string.IsNullOrEmpty(startDate) ? (object)DBNull.Value : DateTime.Parse(startDate));
                    cmd.Parameters.AddWithValue("@EndDate",
                        string.IsNullOrEmpty(endDate) ? (object)DBNull.Value : DateTime.Parse(endDate));
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        attendanceList.Add(new ExcelAttendanceRecordModel
                        {

                            DepartmentName = reader["DepartmentName"].ToString(),
                            EmployeeCode = reader["EmployeeCode"].ToString(),
                            EmployeeName = reader["EmployeeName"].ToString(),
                            AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]),
                            presentStatus = reader["PresentStatus"].ToString(),
                            InTime = reader.GetTimeSpan(reader.GetOrdinal("InTime")),
                            OutTime = reader.GetTimeSpan(reader.GetOrdinal("OutTime")),                            
                        });
                    }
                }

                return attendanceList;
            }
            catch (Exception ex)
            {
                return attendanceList;
            }
            finally
            {
                connection.Close();
            }
        }

        public List<ExcelAttendanceRecordModel> GetAttendaceRecordByIdWithoutDept(int userid, string empcode, string startDate, string endDate)
        {
            List<ExcelAttendanceRecordModel> attendanceList = new List<ExcelAttendanceRecordModel>();

            try
            {

                using (SqlCommand cmd = new SqlCommand("sp_InsertAttendanceRecord", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectAttendanceByIdWithoutDept");
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@EmployeeCode", empcode);

                    cmd.Parameters.AddWithValue("@StartDate",
            string.IsNullOrEmpty(startDate) ? (object)DBNull.Value : DateTime.Parse(startDate));
                    cmd.Parameters.AddWithValue("@EndDate",
                        string.IsNullOrEmpty(endDate) ? (object)DBNull.Value : DateTime.Parse(endDate));
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        attendanceList.Add(new ExcelAttendanceRecordModel
                        {
                            id = Convert.ToInt32(reader["Id"]),
                            DepartmentName = reader["DepartmentName"].ToString(),
                            EmployeeCode = reader["EmployeeCode"].ToString(),
                            EmployeeName = reader["EmployeeName"].ToString(),
                            AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]),
                            presentStatus = reader["PresentStatus"].ToString(),
                            InTime = reader.GetTimeSpan(reader.GetOrdinal("InTime")),
                            OutTime = reader.GetTimeSpan(reader.GetOrdinal("OutTime")),
                        });
                    }
                }

                return attendanceList;
            }
            catch (Exception ex)
            {
                return attendanceList;
            }
            finally
            {
                connection.Close();
            }
        }


        #endregion



        public AllFeeRecordModel GetRemainingFeeByStudentId(int studentid)
        {
            AllFeeRecordModel feeRecord = null;

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_ManageSchoolFee", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetStudentRemainingFee");
                    cmd.Parameters.AddWithValue("@studentid", studentid);

                    connection.Open();
                    using (SqlDataReader res = cmd.ExecuteReader())
                    {
                        if (res.Read())
                        {
                            feeRecord = new AllFeeRecordModel
                            {
                                FeesPaid = res["FeesPaid"] != DBNull.Value ? Convert.ToDecimal(res["FeesPaid"]) : (decimal?)null,
                                RemainingFees = res["RemainingFees"] != DBNull.Value ? Convert.ToDecimal(res["RemainingFees"]) : (decimal?)null
                            };
                        }
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

            return feeRecord;
        }

        #region Exam
        public bool InsertExam(ExamModel model)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageExam", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "insertExam");
                cmd.Parameters.AddWithValue("@userId", model.userId);
                cmd.Parameters.AddWithValue("@examName", model.ExamName);
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

        public List<ExamModel> GetExam(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageExam", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectExam");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<ExamModel> list = new List<ExamModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new ExamModel
                        {
                            ExamId = Convert.ToInt32(res["id"]),
                            ExamName = res["examName"].ToString()
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
        public List<ExamModel> GetScheduledExamForMarkSheet(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageExam", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectscheduledExam");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<ExamModel> list = new List<ExamModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new ExamModel
                        {
                            ExamId = Convert.ToInt32(res["id"]),
                            ExamName = res["examName"].ToString()
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

        public bool InsertScheduleExam(ScheduleExamModel model)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageExam", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", model.scheduleId > 0 ? "updateScheduleExam" : "insertScheduleExam");
                cmd.Parameters.AddWithValue("@id", model.scheduleId);
                cmd.Parameters.AddWithValue("@userId", model.userId);
                cmd.Parameters.AddWithValue("@examId", model.examName);
                cmd.Parameters.AddWithValue("@academicYear", model.academicYear);
                cmd.Parameters.AddWithValue("@examStartDate", model.startExamDate);
                cmd.Parameters.AddWithValue("@examEndDate", model.endExamDate);
                cmd.Parameters.AddWithValue("@description", model.description);
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

        public List<ScheduleExamModel> GetScheduledExam(int userId, string search = null)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageExam", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectScheduleExam");
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@search", search??null);
                connection.Open();
                List<ScheduleExamModel> list = new List<ScheduleExamModel>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new ScheduleExamModel
                        {
                            scheduleId = Convert.ToInt32(res["id"]),
                            examName = res["examName"].ToString(),
                            examId = Convert.ToInt32(res["examId"]),
                            academicYear = res["academicYear"].ToString(),
                            startExamDate = Convert.ToDateTime(res["examStartDate"]),
                            endExamDate = Convert.ToDateTime(res["examEndDate"]),
                            description = res["description"].ToString(),
                            createdAt = Convert.ToDateTime(res["createdAt"])
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

        public bool DeleteScheduledExam(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageExam", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteScheduleExam");
                cmd.Parameters.AddWithValue("@id", id);
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
      
        public class ExamService
        {
            private readonly SqlConnection connection;

            public ExamService()
            {
                connection = new SqlConnection(ConfigurationManager.ConnectionStrings["YourConnectionString"].ConnectionString);
            }

            public bool InsertExamMarksheet(ExamMarksheetViewModel model, out string message)
            {
                int markId = 0;
                try
                {
                    // Insert or update ExamMarksheet1
                    using (SqlCommand cmd = new SqlCommand("sp_InsertExamMarksheet", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@classId", model.ClassId);
                        cmd.Parameters.AddWithValue("@ExamId", model.ExamId);
                        cmd.Parameters.AddWithValue("@action", "InsertMarks");
                        cmd.Parameters.Add("@markId", SqlDbType.Int).Direction = ParameterDirection.Output;

                        connection.Open();
                        cmd.ExecuteNonQuery();
                        markId = Convert.ToInt32(cmd.Parameters["@markId"].Value);
                        connection.Close();
                    }

                    // Insert subject marks into ExamMarksheet2
                    foreach (var mark in model.Marks)
                    {
                        using (SqlCommand cmd2 = new SqlCommand("sp_InsertExamMarksheet", connection))
                        {
                            cmd2.CommandType = CommandType.StoredProcedure;
                            cmd2.Parameters.AddWithValue("@id", markId);
                            cmd2.Parameters.AddWithValue("@subjectId", mark.SubjectId);
                            cmd2.Parameters.AddWithValue("@theoryMarks", mark.TheoryMarks ?? (object)DBNull.Value);
                            cmd2.Parameters.AddWithValue("@practicalMarks", mark.PracticalMarks ?? (object)DBNull.Value);
                            cmd2.Parameters.AddWithValue("@action", "insertMarksDetails");

                            connection.Open();
                            cmd2.ExecuteNonQuery();
                            connection.Close();
                        }
                    }

                    message = "Marks inserted successfully.";
                    return true;
                }
                catch (Exception ex)
                {
                    connection.Close();
                    message = "Error: " + ex.Message;
                    return false;
                }
            }
        }


        public bool InsertExamMarksheet(ExamMarksheetViewModel model,int userid, out string message)
        {
            int markId = 0;
            SqlTransaction transaction = null;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                //  Insert or update ExamMarksheet1
                using (SqlCommand cmd = new SqlCommand("sp_InsertExamMarksheet", connection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@classId", model.ClassId);
                    cmd.Parameters.AddWithValue("@ExamId", model.ExamId);
                    cmd.Parameters.AddWithValue("@action", "InsertMarks");
                    cmd.Parameters.Add("@markId", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    markId = Convert.ToInt32(cmd.Parameters["@markId"].Value);
                }

                //Insert or update subject marks into ExamMarksheet2
                foreach (var mark in model.Marks)
                {
                    using (SqlCommand cmd2 = new SqlCommand("sp_InsertExamMarksheet", connection, transaction))
                    {
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.AddWithValue("@id", markId);
                        cmd2.Parameters.AddWithValue("@userid", userid);
                        cmd2.Parameters.AddWithValue("@subjectId", mark.SubjectId);
                        cmd2.Parameters.AddWithValue("@theoryMarks", mark.TheoryMarks ?? (object)DBNull.Value);
                        cmd2.Parameters.AddWithValue("@practicalMarks", mark.PracticalMarks ?? (object)DBNull.Value);
                        cmd2.Parameters.AddWithValue("@action", "insertMarksDetails");
                        cmd2.Parameters.Add("@markId", SqlDbType.Int).Value = DBNull.Value;
                        cmd2.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                message = "Marks inserted successfully.";
                return true;
            }
            catch (Exception ex)
            {
                transaction?.Rollback(); 
                message = "Error: " + ex.Message;
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        public List<MarkEntry> GetExistingMarks(int classId, int examId,int userid,int subjectId)
        {
            List<MarkEntry> marks = new List<MarkEntry>();

                SqlCommand cmd = new SqlCommand("sp_InsertExamMarksheet", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@action", "SelectPreInsertedData");
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@classId", classId);
                cmd.Parameters.AddWithValue("@examId", examId);
                cmd.Parameters.AddWithValue("@subjectId", subjectId);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    marks.Add(new MarkEntry
                    {
                        SubjectId = Convert.ToInt32(reader["subjectId"]),
                        TheoryMarks = Convert.ToInt32(reader["theoryMarks"]),
                        PracticalMarks = Convert.ToInt32(reader["practicalMarks"])
                    });
                }
                connection.Close();
            

            return marks;
        }

        public bool InsertMarksOfStudent(StudentMarksheetViewModel model, int userid, out string message)
        {
            int markId = 0;
            SqlTransaction transaction = null;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();



                //  Insert or update tbl_StudentNumberAllocation1
                using (SqlCommand cmd = new SqlCommand("sp_ManageStudentNumberAllocation", connection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@classId", model.classId);
                    cmd.Parameters.AddWithValue("@sectionId", model.sectionId);
                    cmd.Parameters.AddWithValue("@subjectId", model.subjectId);
                    cmd.Parameters.AddWithValue("@ExamId", model.examId);
                    cmd.Parameters.AddWithValue("@action", "InsertMarks");
                    cmd.Parameters.Add("@markId", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    markId = Convert.ToInt32(cmd.Parameters["@markId"].Value);
                }

                //Insert or update subject marks into tbl_StudentNumberAllocation2
                foreach (var mark in model.Marks)
                {
                    using (SqlCommand cmd2 = new SqlCommand("sp_ManageStudentNumberAllocation", connection, transaction))
                    {
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.AddWithValue("@id", markId);
                        cmd2.Parameters.AddWithValue("@userid", userid);
                        cmd2.Parameters.AddWithValue("@studentId", mark.studentId);
                        cmd2.Parameters.AddWithValue("@theoryMarks", mark.theoryMarks ?? (object)DBNull.Value);
                        cmd2.Parameters.AddWithValue("@practicalMarks", mark.practicalMarks ?? (object)DBNull.Value);
                        cmd2.Parameters.AddWithValue("@action", "insertMarksDetails");
                        cmd2.Parameters.Add("@markId", SqlDbType.Int).Value = DBNull.Value;
                        cmd2.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                message = "Marks inserted successfully.";
                return true;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                message = "Error: " + ex.Message;
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        public List<StudentMarkEntry> GetPreInsertedData(int classId,int sectionId, int userid, int subjectId, int examId)
        {
            List<StudentMarkEntry> marks = new List<StudentMarkEntry>();

            SqlCommand cmd = new SqlCommand("sp_ManageStudentNumberAllocation", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@action", "SelectPreInsertedData");
            cmd.Parameters.AddWithValue("@userid", userid);
            cmd.Parameters.AddWithValue("@classId", classId);
            cmd.Parameters.AddWithValue("@sectionId", sectionId);
            cmd.Parameters.AddWithValue("@examId", examId);
            cmd.Parameters.AddWithValue("@subjectId", subjectId);

            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                marks.Add(new StudentMarkEntry
                {
                    studentId = Convert.ToInt32(reader["studentId"]),
                    studentName = reader["studentName"].ToString(),
                    theoryMarks = Convert.ToInt32(reader["theoryMarks"]),
                    practicalMarks = Convert.ToInt32(reader["practicalMarks"])
                });
            }
            connection.Close();


            return marks;
        }
        #endregion
        #region HostelBlockOverview
        public string GetWardenByBlockId(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_HostelBlocksOverview", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "GetWardenOfBlock");
                cmd.Parameters.AddWithValue("@blockId", id);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    return rdr["Name"].ToString();
                }

                return null; 
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        public HostelBlockOverviewModel HostelBlockOverview(int userId, int blockId)
        {
            HostelBlockOverviewModel model = new HostelBlockOverviewModel();

            using (SqlCommand cmd = new SqlCommand("sp_HostelBlocksOverview", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userid", userId);
                cmd.Parameters.AddWithValue("@blockId", blockId);

                cmd.Parameters.AddWithValue("@action", "GetBlockOverviewData");

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    model.totalFloors = dr["totalFloors"] != DBNull.Value ? Convert.ToInt32(dr["totalFloors"]) : (int?)null;
                    model.totalRooms = dr["totalbeds"] != DBNull.Value ? Convert.ToInt32(dr["totalRooms"]) : (int?)null;
                    model.totalBeds = dr["occupiedbeds"] != DBNull.Value ? Convert.ToInt32(dr["totalBeds"]) : (int?)null;
                    model.occupiedBeds = dr["remainingbeds"] != DBNull.Value ? Convert.ToInt32(dr["occupiedBeds"]) : (int?)null;
                    model.remainingBeds = dr["remainingBeds"] != DBNull.Value ? Convert.ToInt32(dr["remainingBeds"]) : (int?)null;
                    model.nonACRoomNonAttachedBathroom = dr["nonACRoomNonAttachedBathroom"] != DBNull.Value ? Convert.ToInt32(dr["nonACRoomNonAttachedBathroom"]) : (int?)null;
                    model.ACRoomNonAttachedBathroom = dr["ACRoomNonAttachedBathroom"] != DBNull.Value ? Convert.ToInt32(dr["ACRoomNonAttachedBathroom"]) : (int?)null;
                    model.nonACRoomAttachedBathroom = dr["nonACRoomAttachedBathroom"] != DBNull.Value ? Convert.ToInt32(dr["nonACRoomAttachedBathroom"]) : (int?)null;
                    model.ACRoomAttachedBathroom = dr["ACRoomAttachedBathroom"] != DBNull.Value ? Convert.ToInt32(dr["ACRoomAttachedBathroom"]) : (int?)null;

                }

                connection.Close();
            }

            return model;
        }
        #endregion

        #region LibraryOverview
        public List<CategoryModel> GetBookCategoriesByUser(int userId)
        {
            var categories = new List<CategoryModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_ManageLibraryOverview", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "SelectCategory");
                cmd.Parameters.AddWithValue("@userId", userId);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        categories.Add(new CategoryModel
                        {
                            CategoryId = Convert.ToInt32(reader["id"]),
                            CategoryName = reader["categoryName"].ToString(),
                            BookCount = reader["bookcount"] != DBNull.Value ? Convert.ToInt32(reader["bookcount"]) : 0
                        });
                    }
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {

                connection.Close();
            }
            return categories;
        }

        public LibraryOverviewModel GetLibraryCount(int userId)
        {
            LibraryOverviewModel model = new LibraryOverviewModel();

            using (SqlCommand cmd = new SqlCommand("sp_ManageLibraryOverview", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userid", userId);

                cmd.Parameters.AddWithValue("@action", "GetCount");

                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    model.totalbooks = dr["totalbooks"] != DBNull.Value ? Convert.ToInt32(dr["totalbooks"]) : (int?)null;
                    model.issuedbooks = dr["issuedbooks"] != DBNull.Value ? Convert.ToInt32(dr["issuedbooks"]) : (int?)null;

                }

                connection.Close();
            }

            return model;
        }




        #endregion

        #region Report Card
        public List<ReportCardModel> GetStudentReportCard(int studentId, string academicYear, int examId, out double totalPercentage)
        {

            float overallMarks=0;
            float totaloverallmarks = 0;
            var reportCard = new List<ReportCardModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageExam", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "ReportCard");
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                cmd.Parameters.AddWithValue("@ExamId", examId);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int theoryMarks = Convert.ToInt32(reader["TheoryMarks"]);
                    int practicalMarks = Convert.ToInt32(reader["PracticalMarks"]);
                    int totalTheory = Convert.ToInt32(reader["totaltheorymarks"]);
                    int totalPractical = Convert.ToInt32(reader["totalpracticalmarks"]);

                    int totalObtained = theoryMarks + practicalMarks;
                    int totalMax = totalTheory + totalPractical;
                    double percentage = (totalMax > 0) ? (totalObtained * 100.0 / totalMax) : 0;

                    string grade = GetGrade(percentage);
                    overallMarks += totalObtained;
                    totaloverallmarks += totalMax;
                    reportCard.Add(new ReportCardModel
                    {
                        Subject = reader["subject"].ToString(),
                        ExamName = reader["examName"].ToString(),
                        TheoryMarks = theoryMarks,
                        PracticalMarks = practicalMarks,
                        TotalTheoryMarks = totalTheory,
                        TotalPracticalMarks = totalPractical,
                        TotalObtainedMarks = totalObtained,
                        TotalMarks = totalMax,
                        Grade = grade

                    });
                }
                double overallPercentage = (totaloverallmarks > 0) ? (overallMarks * 100.0 / totaloverallmarks) : 0;
                totalPercentage = overallPercentage;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
          

            return reportCard;
        }
        private string GetGrade(double percentage)
        {
            if (percentage >= 90) return "A+";
            else if (percentage >= 80) return "A";
            else if (percentage >= 70) return "B+";
            else if (percentage >= 60) return "B";
            else if (percentage >= 50) return "C";
            else if (percentage >= 40) return "D";
            else return "E";
        }

        #endregion
        #region Updated Hostel Management
        public List<LastHostelFeeRecord> GetLastFeeRecords(int userid)
        {
            try
            {
                List<LastHostelFeeRecord> feeRecords = new List<LastHostelFeeRecord>();
                SqlCommand cmd = new SqlCommand("sp_HostelStudentFeeRecordManagement", connection);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectlastpaymentofall");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    LastHostelFeeRecord record = new LastHostelFeeRecord
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
        #endregion
        #region Driver Problems
        public List<DriverProblemModel> GetDriverProblem(int adminid)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageDriverProblem", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.AddWithValue("@action", "selectforadmin");
                List<DriverProblemModel> list = new List<DriverProblemModel>();
                connection.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new DriverProblemModel
                        {
                            id = Convert.ToInt32(res["id"]),
                            busno = Convert.ToInt32(res["bussno"]),
                            problem = res["problem"].ToString(),
                            problemStatus = Convert.ToInt32(res["problemStatus"]),
                            createdAt = Convert.ToDateTime(res["createdAt"]).ToString("dd-MMM-yyyy"),
                            driverName = res["name"].ToString(),
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

        public bool CompleteRejectDriverProblem(int id, int status, string reason)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageDriverProblem", connection);
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
                    list.Name = rdr["Name"]?.ToString();
                    list.MobileNo = rdr["MobileNo"] != DBNull.Value ? Convert.ToInt64(rdr["MobileNo"]) : 0;
                    list.EmailId = rdr["EmailId"]?.ToString();
                    list.Images = rdr["Image"]?.ToString();
                    list.schoolAddress = rdr["school_address"]?.ToString();
                    list.CompanyId = Convert.ToInt32(rdr["companyId"] ?? "0");
                    list.CompanyName = rdr["schoolName"]?.ToString();
                    list.AuthorizedPersonName= rdr["AuthorizedPersonName"]?.ToString();
                    list.AuthorizedPersonEmail= rdr["Email"]?.ToString();
                    list.AuthorizedPersonState = rdr["stateName"]?.ToString();
                    list.AuthorizedPersonCity = rdr["City_Name"]?.ToString();
                    list.SchoolLogo = rdr["School_Logo"]?.ToString();
                    list.AuthorizedSign = rdr["Authorized_Sign"]?.ToString();
                    list.AuthorizedPersonMobileNo = Convert.ToInt64(rdr["Mobile"]);
                    list.AuthorizedPersonLandlineNo = Convert.ToInt64(rdr["LandLineNo"]);

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

        public List<Buss> GetAllBusListAndCharge(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectBusAndBusCharge");
                cmd.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                List<Buss> list = new List<Buss>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new Buss
                        {
                            Id = Convert.ToInt32(res["id"]),
                            Type = res["type"].ToString(),
                            BusNo = res["bussNo"].ToString(),
                            BusCharges = Convert.ToInt32(res["busCharge"])
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
        public List<AddStudentInBus> GetStudentByBusId(int userId,int bussNo)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTransport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectStudentByBusId");
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@bussId", bussNo);
                connection.Open();
                List<AddStudentInBus> list = new List<AddStudentInBus>();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        list.Add(new AddStudentInBus
                        {
                            id = Convert.ToInt32(res["id"]),
                            busId = Convert.ToInt32(res["busId"]),
                            bussNo = res["bussNo"].ToString(),
                            type = res["type"].ToString(),
                            studentId = Convert.ToInt32(res["studentId"]),
                            studentName = res["StudentName"].ToString(),
                            classId = Convert.ToInt32(res["classId"]),
                            className = res["ClassName"].ToString(),
                            sectionId = Convert.ToInt32(res["sectionId"]),
                            sectionName = res["SectionName"].ToString(),
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

        public bool InsertBusFee(BusFeeModel model, out string errorMessage)
        {
            object result = null;
            errorMessage = "";
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            try
            {
                string attachmentPath = string.Empty;
                if (model.feeSlips != null)
                {
                    attachmentPath = UploadImageToServer(model.feeSlips);
                }

                int totalCount = 0;

                foreach (var month in model.billingMonths)
                {
                    SqlCommand command = new SqlCommand("sp_SubmitBusFee", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@action", "Insert");
                    command.Parameters.AddWithValue("@userId", model.userId);
                    command.Parameters.AddWithValue("@studentId", model.studentId);
                    command.Parameters.AddWithValue("@busId", model.busId);

                    if (model.classId > 0)
                        command.Parameters.AddWithValue("@classId", model.classId);
                    else
                        command.Parameters.AddWithValue("@classId", DBNull.Value);

                    if (model.classStreamId > 0)
                        command.Parameters.AddWithValue("@classStreamId", model.classStreamId);
                    else
                        command.Parameters.AddWithValue("@classStreamId", DBNull.Value);

                    if (model.sectionId > 0)
                        command.Parameters.AddWithValue("@sectionId", model.sectionId);
                    else
                        command.Parameters.AddWithValue("@sectionId", DBNull.Value);


                    command.Parameters.AddWithValue("@feeAmount", model.feeAmount);
                    command.Parameters.AddWithValue("@billingMonth", month);
                    command.Parameters.AddWithValue("@paymentDate", model.paymentDated);

                    if (!string.IsNullOrEmpty(attachmentPath))
                        command.Parameters.AddWithValue("@feeSlip", attachmentPath);

                
                    result = command.ExecuteScalar();
                  

                    if (result?.ToString()!="added" && result?.ToString()!= "already added")
                    {
                        transaction.Rollback();
                        errorMessage = "Failed to insert fee";
                        return false;
                    }

                    if(result?.ToString()=="already added")
                    {
                        totalCount++;
                    }

                }

                if (model.billingMonths.Length == totalCount)
                {
                    transaction.Rollback();
                    errorMessage = $"These months fee are already added";
                    return false;
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                transaction.Rollback();
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public List<BusFeeModel> GetStudentBusFees(int userid)
        {
            List<BusFeeModel> list = new List<BusFeeModel>();
            SqlCommand cmd = new SqlCommand("sp_SubmitBusFee", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@action", "selectAll");
            cmd.Parameters.AddWithValue("@userid", userid);

            connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new BusFeeModel
                {
                    id = Convert.ToInt32(dr["Id"]),
                    studentId = Convert.ToInt32(dr["StudentId"]),
                    studentName = dr["StudentName"].ToString(),
                    fatherName = dr["FatherName"].ToString(),
                    className = dr["ClassName"].ToString(),
                    billingMonth = dr["billingMonth"].ToString(),
                    sectionName = dr["SectionName"].ToString(),
                    academicYear = dr["AcademicYear"].ToString(),
                    feeAmount = Convert.ToDecimal(dr["FeeAmount"]),
                    createdDate = Convert.ToDateTime(dr["CreatedDate"]),
                    paymentDated = string.IsNullOrEmpty(dr["paymentDate"].ToString()) ? null: Convert.ToDateTime(dr["paymentDate"]).ToString("dd-MMM-yyyy"),

                    feeSlip = dr["FeeSlip"].ToString()
                });
            }
            connection.Close();
            return list;
        }
        public List<BusFeeModel> GetStudentMonthlyFeeSummary(int studentId,int userid)
        {
            List<BusFeeModel> list = new List<BusFeeModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_SubmitBusFee", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "GetStudentMonthlySummary");
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.Parameters.AddWithValue("@userid", userid);


                connection.Open();
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new BusFeeModel
                    {
                        studentName = dr["StudentName"]?.ToString(),
                        className = dr["ClassName"]?.ToString(),
                        sectionName = dr["SectionName"]?.ToString(),
                        billingMonth = dr["billingMonth"]?.ToString(),
                        feeAmount = dr["feeAmount"] != DBNull.Value ? Convert.ToDecimal(dr["FeeAmount"]) : 0,
                        paymentDated = dr["paymentDate"] != DBNull.Value ? Convert.ToDateTime(dr["paymentDate"]).ToString("dd-MMM-yyyy")   : null,

                        feeSlip = dr["feeSlip"]?.ToString()
                    });
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            
            return list;
        }
        public List<SubjectModel> GetOptionalSubjectByStreamId(int streamId)
        {
            List<SubjectModel> list = new List<SubjectModel>();
            try
            {

                SqlCommand cmd = new SqlCommand("sp_SubjectManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetOptionalSubjectByStreamId");
                cmd.Parameters.AddWithValue("@classStreamId", streamId);
                connection.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new SubjectModel
                    {
                        classStreamId = Convert.ToInt32(dr["classStreamId"]),
                        SubjectId = Convert.ToInt32(dr["SubjectId"]),
                        Subject = dr["Subject"].ToString(),
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


        public List<AdminCommunicationList> AdminCommunicationListByWarden(int userid)
        {
            List<AdminCommunicationList> data = new List<AdminCommunicationList>();
            try
            {
               using(SqlCommand cmd=new SqlCommand("sp_ManageCommunication", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@action", "showcommunicationforadmin");
                    connection.Open();
                    SqlDataReader sdr = cmd.ExecuteReader();
                    if(sdr.HasRows)
                    {
                        while(sdr.Read())
                        {
                            data.Add(new AdminCommunicationList
                            {
                                Id = Convert.ToInt32(sdr["Id"]),
                                Title = sdr["Title"].ToString(),
                                Attachment = sdr["Attachment"].ToString(),
                                Description = sdr["Description"].ToString(),
                                WardenName = sdr["Name"].ToString(),
                                EmailId = sdr["Email_Id"].ToString(),
                                Mobile = sdr["MobileNo"].ToString(),
                                BlockName = sdr["BlockName"].ToString(),
                                BlockType = sdr["blockType"].ToString(),
                                TotalFloorInBlock = Convert.ToInt32(sdr["TotalFlourInBlock"])
                            });
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
            }
            return data;
        }




    }
}
