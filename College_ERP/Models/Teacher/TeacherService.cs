using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using Antlr.Runtime;
using College_ERP.Models.Admin;
using static System.Collections.Specialized.BitVector32;

namespace College_ERP.Models.Teacher
{
    public class TeacherService
    {
        private readonly SqlConnection connection;
        private SqlCommand cmd;
        public TeacherService()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
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
        public int GetUserId(string username)
        {
            try
            {
                int userId = 0;
                cmd = new SqlCommand("sp_loginmanager", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectadminIdbyteacherusername");
                cmd.Parameters.AddWithValue("@username", username);
                connection.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        userId = Convert.ToInt32(res["Userid"]);
                    }
                }
                return userId;
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
        public int GetAdminId(int userId)
        {
            try
            {
                int adminId = 0;
                cmd = new SqlCommand("sp_loginmanager", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectadminidbyuserid");
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
        public int GetTotalCircularCount(int userId)
        {
            int totalCirculars = 0;
            SqlCommand cmd = new SqlCommand("sp_TeacherRegistrationManagement", connection);
                
                    cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Actions", "GetTotalCircularCount");
            cmd.Parameters.AddWithValue("@userid", userId);
            connection.Open();
            totalCirculars = Convert.ToInt32(cmd.ExecuteScalar());


            return totalCirculars;
        }

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

        public TeacherModel GetTeachers(int userid)
        {
            TeacherModel list = new TeacherModel();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_TeacherRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "SelectAllTeacher");
                cmd.Parameters.AddWithValue("@userid",userid);
             
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list = new TeacherModel
                    {
                        TeacherId = Convert.ToInt32(rdr["TeacherId"] ?? 0),
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
                        IfscCode = rdr["IFSCCode"]?.ToString()
                      

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
        public TeacherModel GetTeacherByTeacherId(int id)
        {
            TeacherModel list = new TeacherModel();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_TeacherRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "SelectTeacherById");
                cmd.Parameters.AddWithValue("@TeacherId", id);
             
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list = new TeacherModel
                    {
                        TeacherId = Convert.ToInt32(rdr["TeacherId"] ?? 0),
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

                        EmployeeId = rdr["EmployeeId"]?.ToString()


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


        public List<SubjectAssignModel> GetSubjectAssignedById(int userid)
        {
            List<SubjectAssignModel> list = new List<SubjectAssignModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_SubjectAssignManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "GetAll");
                cmd.Parameters.AddWithValue("@id", userid);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new SubjectAssignModel
                    {
                        AssignedId = rdr["AssignedId"] != DBNull.Value ? Convert.ToInt32(rdr["AssignedId"]) : 0,
                        ClassId = Convert.ToInt32(rdr["ClassId"]),
                        ClassName = rdr["ClassName"].ToString(),
                        SectionId = Convert.ToInt32(rdr["SectionId"]),
                        SectionName = rdr["SectionName"].ToString(),
                        SubjectId = Convert.ToInt32(rdr["SubjectId"]),
                        SubjectName = rdr["Subject"].ToString(),
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

        public List<StudentModel> GetStudentsByClassSection(int userid, int classId, int sectionId,string academicyear)
        {
            List<StudentModel> list = new List<StudentModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYCLASSSECTION");
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                cmd.Parameters.AddWithValue("@academicyear", academicyear);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new StudentModel
                    {
                        StudentId = rdr["StudentId"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        ClassId = rdr["ClassId"] != DBNull.Value ? Convert.ToInt32(rdr["ClassId"]) : 0,
                        SectionId = rdr["SectionId"] != DBNull.Value ? Convert.ToInt32(rdr["SectionId"]) : 0,
                        StudentName = rdr["StudentName"]?.ToString(),
                        Address = rdr["Address"]?.ToString(),
                        FatherName = rdr["FatherName"]?.ToString(),
                        MobileNo = rdr["MobileNo"]?.ToString(),
                        Gender = rdr["Gender"].ToString(),
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

        public List<StudentModel> GetStudentsByClassSectionForReport(int userid, int classId, int sectionId,int subjectId,DateTime attendanceDate, string academicyear)
        {
            List<StudentModel> list = new List<StudentModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SelectStudentForReport");
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                cmd.Parameters.AddWithValue("@academicyear", academicyear);
                cmd.Parameters.AddWithValue("@attendanceDate", attendanceDate);
                cmd.Parameters.AddWithValue("@subjectId", subjectId);
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new StudentModel
                    {
                        StudentId = rdr["StudentId"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        ClassId = rdr["ClassId"] != DBNull.Value ? Convert.ToInt32(rdr["ClassId"]) : 0,
                        SectionId = rdr["SectionId"] != DBNull.Value ? Convert.ToInt32(rdr["SectionId"]) : 0,
                        StudentName = rdr["StudentName"]?.ToString(),
                        Address = rdr["Address"]?.ToString(),
                        FatherName = rdr["FatherName"]?.ToString(),
                        AttendaceStatus = (bool)rdr["AttendanceStatus"],
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

        public List<SubjectAssignModel> GetSubjectAssignmentsWithStudentCount(int userId)
        {
            List<SubjectAssignModel> list = new List<SubjectAssignModel>();

            SqlCommand cmd = new SqlCommand("sp_SubjectAssignManagement", connection);
                
                    cmd.CommandType = CommandType.StoredProcedure;        
                    cmd.Parameters.AddWithValue("@Actions", "GetCount");
                        cmd.Parameters.AddWithValue("@id", userId);

            connection.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        list.Add(new SubjectAssignModel
                        {
                            AssignedId = Convert.ToInt32(dr["AssignedId"]),
                            ClassName = dr["ClassName"].ToString(),
                            ClassId = Convert.ToInt32(dr["ClassId"]),
                            SectionName = dr["SectionName"].ToString(),
                            SectionId = Convert.ToInt32(dr["SectionId"]),
                            SubjectName = dr["Subject"].ToString(),
                            SubjectId = Convert.ToInt32(dr["SubjectId"]),
                            StudentCount = Convert.ToInt32(dr["StudentCount"])
                        });
                    }
                
            

            return list;
        }
        public bool InsertAttendance(int userId, int classId, int subjectId, int sectionId,DateTime attendanceDate,List<StudentAttendance> attendanceList,out string errorMessage)
        {
            try
            {
                errorMessage = "";
                DataTable attendanceTable = new DataTable();
                attendanceTable.Columns.Add("UserId", typeof(int));
                attendanceTable.Columns.Add("StudentId", typeof(int));
                attendanceTable.Columns.Add("ClassId", typeof(int));
                attendanceTable.Columns.Add("SectionId", typeof(int));
                attendanceTable.Columns.Add("SubjectId", typeof(int));
                attendanceTable.Columns.Add("AttendanceStatus", typeof(string));
                attendanceTable.Columns.Add("AttendanceDate", typeof(DateTime));

                // Populate DataTable
                foreach (var student in attendanceList)
                {
                    attendanceTable.Rows.Add(
                        userId,
                        student.StudentId,
                        classId,
                        sectionId, // ensure you have this variable
                        subjectId,
                        student.AttendanceStatus,
                        attendanceDate

                    );
                }


                using (SqlCommand cmd = new SqlCommand("sp_AttendanceManagement", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@AttendanceData", attendanceTable);
                    cmd.Parameters.AddWithValue("@Action", "insertIntoBulk");
                    tvpParam.SqlDbType = SqlDbType.Structured;

                    connection.Open();
                    int res = cmd.ExecuteNonQuery();
                    if (res <= 0)
                    {
                        errorMessage = "Some error occured";
                    }
                    return res > 0;
                }


              
            }catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        public List<StudentModel> GetStudents( int classId, int sectionId)
        {
            List<StudentModel> list = new List<StudentModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_StudentRegistrationManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTStudents");
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new StudentModel
                    {
                        StudentId = rdr["StudentId"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        ClassId = rdr["ClassId"] != DBNull.Value ? Convert.ToInt32(rdr["ClassId"]) : 0,
                        SectionId = rdr["SectionId"] != DBNull.Value ? Convert.ToInt32(rdr["SectionId"]) : 0,
                        StudentName = rdr["StudentName"]?.ToString(),
                        Address = rdr["Address"]?.ToString(),
                        FatherName = rdr["FatherName"]?.ToString(),
                        MotherName = rdr["MotherName"]?.ToString(),
                        MobileNo = rdr["MobileNo"]?.ToString(),
                        StudentEmail = rdr["StudentEmail"].ToString(),
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


        #region assignment management

        public bool insertAssingment(Assignment assg,out string errorMessage)
        {
            try
            {
                errorMessage = "";
                string attachmentUrl = null;
                if (assg.Attachment != null)
                    attachmentUrl = UploadImageToServer(assg.Attachment);

                SqlCommand cmd = new SqlCommand("sp_assignmentManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", assg.Id > 0 ? "updateassignment" : "insertAssignment");
                cmd.Parameters.AddWithValue("@id", assg.Id);
                cmd.Parameters.AddWithValue("@teacherId", assg.TeacherId);
                cmd.Parameters.AddWithValue("@classId", assg.ClassId);
                cmd.Parameters.AddWithValue("@sectionId", assg.SectionId);
                cmd.Parameters.AddWithValue("@subjectId", assg.SubjectId);
                cmd.Parameters.AddWithValue("@academicyear", assg.AcademicYear);
                cmd.Parameters.AddWithValue("@title", assg.Title);
                cmd.Parameters.AddWithValue("@descr", assg.Description);
                cmd.Parameters.AddWithValue("@attachment", attachmentUrl);
                cmd.Parameters.AddWithValue("@completiondate", assg.CompletionDate);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    errorMessage = "some error occured";
                    if (!string.IsNullOrEmpty(attachmentUrl))
                    {
                        string serverpath = HttpContext.Current.Server.MapPath("~" + attachmentUrl);
                        if (Directory.Exists(serverpath))
                        {
                            System.IO.File.Delete(serverpath);
                        }
                    }
                }
                return res > 0;
            }catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        public List<Assignment> selectAssignment(int teacherId)
        {
                List<Assignment> assignment = new List<Assignment>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_assignmentManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAllAssignment");
                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        assignment.Add(new Assignment
                        {
                            Id = Convert.ToInt32(rd["id"]),
                            classname = rd["className"]?.ToString(),
                            sectionname = rd["sectionName"]?.ToString(),
                            subjectname = rd["subject"]?.ToString(),
                            Title = rd["title"]?.ToString(),
                            Description = rd["descr"]?.ToString(),
                            AttachmentUrl = rd["attachment"]?.ToString(),
                            AcademicYear = rd["AcademicYear"]?.ToString(),
                            CompletionDate = Convert.ToDateTime(rd["CompletionDate"] != DBNull.Value ? rd["CompletionDate"] : (DateTime?)null),
                            AssignDate = Convert.ToDateTime(rd["assigndate"] != DBNull.Value ? rd["assigndate"] : (DateTime?)null),
                            ClassId = Convert.ToInt32(rd["classId"] != DBNull.Value ? rd["classId"] : 0),
                            SectionId = Convert.ToInt32(rd["sectionId"] != DBNull.Value ? rd["sectionId"] : 0),
                            SubjectId = Convert.ToInt32(rd["subjectId"] != DBNull.Value ? rd["subjectId"] : 0)
                        });
                    }
                }
                return assignment;
            }catch(Exception ex)
            {
                return assignment;
            }
            finally
            {
                connection.Close();
            }
        }
        public List<StudentModel> GetStudentsForAssignment(int classId, int sectionId,int assid)
        {
            List<StudentModel> list = new List<StudentModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_assignmentManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "selectAllStudent");
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                cmd.Parameters.AddWithValue("@id", assid);
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new StudentModel
                    {
                        assid = assid,
                        StudentId = rdr["StudentId"] != DBNull.Value ? Convert.ToInt32(rdr["StudentId"]) : 0,
                        ClassId = rdr["ClassId"] != DBNull.Value ? Convert.ToInt32(rdr["ClassId"]) : 0,
                        SectionId = rdr["SectionId"] != DBNull.Value ? Convert.ToInt32(rdr["SectionId"]) : 0,
                        StudentName = rdr["StudentName"]?.ToString(),
                        Address = rdr["Address"]?.ToString(),
                        FatherName = rdr["FatherName"]?.ToString(),
                        MotherName = rdr["MotherName"]?.ToString(),
                        MobileNo = rdr["MobileNo"]?.ToString(),
                        StudentEmail = rdr["StudentEmail"].ToString(),
                        assigmentattachment = rdr["attachment"].ToString(),
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

        public List<StudentAssignmentModel> GetStudentAssignments(int assignmentId)
        {
            var students = new List<StudentAssignmentModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_assignmentManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectStudentassignment");
                cmd.Parameters.AddWithValue("@id", assignmentId);

                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    

                    students.Add(new StudentAssignmentModel
                    {
                        StudentId = Convert.ToInt32(rdr["studentId"]),
                        StudentName = rdr["StudentName"].ToString(),
                        FilePath = rdr["attachment"]?.ToString(),
                        Email = rdr["StudentEmail"]?.ToString(),
                        MobileNo = rdr["MobileNo"]?.ToString(),
                        FatherName = rdr["FatherName"]?.ToString(),

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

            return students;
        }

        public List<AddTaskModel> GetTasksByTeacher(int teacherId)
        {
            List<AddTaskModel> list = new List<AddTaskModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageTask", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectTaskByTeacher");
                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                connection.Open();
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
                            completionDateString = Convert.ToDateTime(res["completionDate"]).ToString("dd-MMM-yyyy"),
                            taskStatus = Convert.ToInt32(res["taskStatus"])
                        });
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

        public bool UpdateTaskStatus(int taskId)
        {
            try
            {               
                {
                    SqlCommand cmd = new SqlCommand("sp_ManageTask", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "updateTaskStatus");
                    cmd.Parameters.AddWithValue("@id", taskId);
                    cmd.Parameters.AddWithValue("@taskStatus", 1);
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
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
        public List<SubjectAssignModel> GetClassFromTeacher(int teacherid)
        {
            try
            {
                List<SubjectAssignModel> list = new List<SubjectAssignModel>();
                SqlCommand cmd = new SqlCommand("sp_SubjectAssignManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "selectClassofteacher");
                cmd.Parameters.AddWithValue("@teacherId", teacherid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new SubjectAssignModel
                    {
                        ClassId = Convert.ToInt32(res["classid"]),
                        ClassName = res["classname"].ToString(),
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
        public List<SubjectAssignModel> GetSectionFromTeacher(int teacherid, int classid)
        {
            try
            {
                List<SubjectAssignModel> list = new List<SubjectAssignModel>();
                SqlCommand cmd = new SqlCommand("sp_SubjectAssignManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "selectsectionofteacher");
                cmd.Parameters.AddWithValue("@ClassId", classid);
                cmd.Parameters.AddWithValue("@teacherId", teacherid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    if (res["SectionId"] != DBNull.Value)
                    {
                        list.Add(new SubjectAssignModel
                        {
                            SectionId = Convert.ToInt32(res["SectionId"] != DBNull.Value ? res["sectionId"] : 0),
                            SectionName = res["sectionname"]?.ToString(),
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
        public List<SubjectAssignModel> GetSubjectFromTeacher(int teacherid, int classid, int sectionid)
        {
            try
            {
                List<SubjectAssignModel> list = new List<SubjectAssignModel>();
                SqlCommand cmd = new SqlCommand("sp_SubjectAssignManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Actions", "selectsubjectofteacher");
                cmd.Parameters.AddWithValue("@teacherId", teacherid);
                cmd.Parameters.AddWithValue("@ClassId", classid);
                cmd.Parameters.AddWithValue("@SectionId", sectionid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new SubjectAssignModel
                    {
                        SubjectId = Convert.ToInt32(res["subjectid"]),
                        SubjectName = res["Subject"].ToString(),
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

        #region Leave Management
        public bool InsertLeaveRequest(LeaveRequestModel sm ,out string errorMessage)
        {
            errorMessage = "";
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLeaveRequests", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", sm.id > 0 ? "updateapplyleave" : "insertapplyleave");
                cmd.Parameters.AddWithValue("@id", sm.id);
                cmd.Parameters.AddWithValue("@userId", sm.userId);
                cmd.Parameters.AddWithValue("@teacherId", sm.teacherId);
                cmd.Parameters.AddWithValue("@leaveType", sm.leaveType);
                cmd.Parameters.AddWithValue("@fromDate", sm.fromDate);
                cmd.Parameters.AddWithValue("@toDate", sm.toDate);
                cmd.Parameters.AddWithValue("@reason", sm.reason);
                cmd.Parameters.AddWithValue("@attachment", sm.attachmentName);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    errorMessage = "Some Error Occured";
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
            }
        }
        public List<LeaveRequestModel> GetAllLeaveRequst(int userid,int teacherId)
        {
            try
            {
                List<LeaveRequestModel> list = new List<LeaveRequestModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageLeaveRequests", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectallleaveRequest");
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new LeaveRequestModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        teacherId = Convert.ToInt32(res["teacherId"]),
                        reason = res["reason"].ToString(),
                        approvalStatus = Convert.ToInt32(res["approvalStatus"]),
                        attachmentName = res["attachment"].ToString(),
                        fromDate = Convert.ToDateTime(res["fromDate"]),
                        fromDateString = Convert.ToDateTime(res["fromDate"]).ToString("dd-MMM-yyyy"),
                        toDateString = Convert.ToDateTime(res["toDate"]).ToString("dd-MMM-yyyy"),
                        toDate = Convert.ToDateTime(res["toDate"]),
                        leaveType = res["leaveType"].ToString(),
                        remark = res["remark"] != null ? res["remark"].ToString():null
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

        #region
        public bool InsertCommunication(TeacherCommunicationModel sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageCommunication", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "insertCommunication");
                cmd.Parameters.AddWithValue("@teacherId", sm.teacherId);
                cmd.Parameters.AddWithValue("@isSendTo", sm.IsSendTo);
                cmd.Parameters.AddWithValue("@studentId", sm.StudentName);
                cmd.Parameters.AddWithValue("@title", sm.title);
                cmd.Parameters.AddWithValue("@description", sm.description);
                cmd.Parameters.AddWithValue("@attachment", sm.attachmentName);
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public List<TeacherCommunicationModel> GetAllTeacherCommunication(int teacherId)
        {
            try
            {
                List<TeacherCommunicationModel> list = new List<TeacherCommunicationModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageCommunication", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectAllTeacherCommunication");
                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new TeacherCommunicationModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        teacherId = Convert.ToInt32(res["teacherId"]),
                        attachmentName = res["attachment"].ToString(),
                        title = res["title"].ToString(),
                        description = res["description"].ToString(),
                        className = res["ClassName"].ToString(),
                        secitonName = res["sectionname"].ToString(),
                        student = res["studentname"].ToString(),
                        IsSendTo = Convert.ToInt32(res["isSendTo"])
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
        public List<TeacherAwardModel> GetAllAward(int teacherId)
        {
            try
            {
                List<TeacherAwardModel> list = new List<TeacherAwardModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageTeacherAwards", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectawardbyteacher");
                cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new TeacherAwardModel
                    {
                        awardTitle = res["Title"].ToString(),
                        awardDate = Convert.ToDateTime(res["AwardDate"]),
                        awardDesc = res["Description"].ToString(),
                        awardcertificate = res["CertificatePath"].ToString(),
                        awardType = res["AwardType"].ToString(),
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

        public List<NoticeModel> GetTeacherNotices(string userType, int userId ,int adminid)
        {
            var notices = new List<NoticeModel>();

            SqlCommand cmd = new SqlCommand("sp_NoticeManagement", connection);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "GetNoticeByUsers");
            cmd.Parameters.AddWithValue("@UserType", userType);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@adminid", adminid);

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
        public List<EventCategoryModel> ShowAllEventcategory(int userid)
        {
            try
            {
                List<EventCategoryModel> list = new List<EventCategoryModel>();
                SqlCommand cmd = new SqlCommand("Sp_Event", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "ShowCategory");
                cmd.Parameters.AddWithValue("@userid", userid);
                connection.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    EventCategoryModel category = new EventCategoryModel
                    {
                        Id = Convert.ToInt32(rd["Id"]),
                        CategoryName = rd["CategoryName"].ToString(),
                        CategoryDescription = rd["CategoryDescription"].ToString(),
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

        public TeacherDashboardCountResult GetDashboardCounts(int userId, int teacherId)
        {
            try
            {

                SqlCommand cmd = new SqlCommand("sp_TeacherDashboardCount", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                cmd.Parameters.AddWithValue("@action", "TeacherDashboard");

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new TeacherDashboardCountResult
                    {
                        assignedclass = Convert.ToInt32(reader["assignedclass"]),
                        assignedtask = Convert.ToInt32(reader["assignedtask"]),
                        communication = Convert.ToInt32(reader["communication"]),
                        circular = Convert.ToInt32(reader["circular"]),
                        assignments = Convert.ToInt32(reader["assignments"]),
                        borrowedbooks = Convert.ToInt32(reader["borrowedbooks"]),
                        notice = Convert.ToInt32(reader["notice"]),
                    };
                }
                return new TeacherDashboardCountResult();
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
            public List<TodayScheduleModel> TodayScheduleOfTeacher(int teacherid, string day)
             {
            try
            {
                List<TodayScheduleModel> list = new List<TodayScheduleModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "SelectTimeByTeacherIdperDay");
                cmd.Parameters.AddWithValue("@teacherId", teacherid);
                cmd.Parameters.AddWithValue("@day", day);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    TodayScheduleModel category = new TodayScheduleModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        subjectName = res["subject"].ToString(),
                        className = res["ClassName"].ToString(),
                        sectionName = res["sectionName"].ToString(),
                        subjectId = Convert.ToInt32(res["subjectId"]),
                        from = DateTime.Today.Add((TimeSpan)res["fromTime"]).ToString("hh:mm tt"),
                        to = DateTime.Today.Add((TimeSpan)res["toTime"]).ToString("hh:mm tt"),
                        day = res["day"].ToString(),
                        combineTime = $"{DateTime.Today.Add((TimeSpan)res["fromTime"]).ToString("hh:mm tt")} - {DateTime.Today.Add((TimeSpan)res["toTime"]).ToString("hh:mm tt")}"
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

        public List<PendingTaskModel> GetTeacherPendingTasks(int userId, int teacherId)
        {
            List<PendingTaskModel> tasks = new List<PendingTaskModel>();

            SqlCommand cmd = new SqlCommand("sp_TeacherDashboardCount", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@teacherId", teacherId);
            cmd.Parameters.AddWithValue("@action", "PendingTasks");

            try
            {
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tasks.Add(new PendingTaskModel
                    {
                        TaskTitle = reader["Title"].ToString(),
                        TaskStatus = Convert.ToInt32(reader["TaskStatus"]),
                        CompletionDate = reader["CompletionDate"].ToString()
                    });
                }
                return new List<PendingTaskModel>(tasks);
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
        public List<SubjectTimesModel> FullWeakSchedule(int teacherId)
        {
            try
            {
                List<SubjectTimesModel> list = new List<SubjectTimesModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectTimeTableDetailsByTeacherId");
                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    SubjectTimesModel category = new SubjectTimesModel
                    {
                        id = Convert.ToInt32(res["id"]),
                        subjectName = res["subject"].ToString(),
                        className = res["ClassName"].ToString(),
                        sectionName = res["sectionName"].ToString(),
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

        public List<AddExamTimeTableModel> GetExamTimeTableForTeacher(int teacherid,int scheduledid)
        {
            try
            {
                List<AddExamTimeTableModel> list = new List<AddExamTimeTableModel>();
                SqlCommand cmd = new SqlCommand("sp_ManageExamTimeTable", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectExamTimeTableForTeacher");
                cmd.Parameters.AddWithValue("@teacherId", teacherid);
                cmd.Parameters.AddWithValue("@scheduledid", scheduledid);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                SqlDataReader res = cmd.ExecuteReader();
                if (res.HasRows)
                {
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
                            examName = res["examname"].ToString(),
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
        public List<AddSyllabusMoedel> GetSyllabusForteacher(int teacherid)
        {
            try
            {
                List<AddSyllabusMoedel> list = new List<AddSyllabusMoedel>();
                SqlCommand cmd = new SqlCommand("sp_ManageSyllabus", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectsyllabusforteacher");
                cmd.Parameters.AddWithValue("@teacherId", teacherid);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new AddSyllabusMoedel
                    {
                        id = Convert.ToInt32(res["id"]),
                        subjectName = res["subject"].ToString(),
                        subjectId = Convert.ToInt32(res["subjectId"]),
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
        public List<OrderHistoryModel> GetLibraryDetails(int teacherid)
        {
            List<OrderHistoryModel> orders = new List<OrderHistoryModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ManageLibrary", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectorderofteacher");
                cmd.Parameters.AddWithValue("@buyerId", teacherid);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        orders.Add(new OrderHistoryModel
                        {
                            shortorderid = reader["shortOrderId"].ToString(),
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
                          totalLateFine =!Convert.ToBoolean(reader["recieveStatus"]) && Convert.ToDateTime(reader["returnDate"]) < DateTime.Now? (DateTime.Now.Date - Convert.ToDateTime(reader["returnDate"]).Date).Days * Convert.ToDecimal(reader["lateFine"]) :0,
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
        public List<EventCategory> ShowEvents(int userId)
        {
            List<EventCategory> list = new List<EventCategory>();
            SqlCommand cmd = new SqlCommand("sp_TeacherDashboardCount", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@action", "ShowEvents");
            cmd.Parameters.AddWithValue("@userid", userId);

            connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                EventCategory e = new EventCategory
                {
                    Id = Convert.ToInt32(dr["Id"]),
                    CategoryName = dr["CategoryName"].ToString(),
                    CategoryDescription = dr["CategoryDescription"].ToString(),
                    fromdate = Convert.ToDateTime(dr["fromdate"]).ToString("dd-MM-yyyy"),
                    todate = Convert.ToDateTime(dr["todate"]).ToString("dd-MM-yyyy"),
                    combineDate = Convert.ToDateTime(dr["fromdate"]).Date == Convert.ToDateTime(dr["todate"]).Date
    ? Convert.ToDateTime(dr["fromdate"]).ToString("dd-MM-yyyy")  
    : $"{Convert.ToDateTime(dr["fromdate"]).ToString("dd-MM-yyyy")} - {Convert.ToDateTime(dr["todate"]).ToString("dd-MM-yyyy")}"
                };
                list.Add(e);
            }
            connection.Close();
            return list;
        }

        public bool InsertAndUpdateNotice(NoteModel assg, out string errorMessage)
        {
            try
            {
                errorMessage = "";
                string attachmentUrl = null;
                if (assg.Attachment != null)
                    attachmentUrl = UploadImageToServer(assg.Attachment);

                SqlCommand cmd = new SqlCommand("sp_NotesManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", assg.NoteId > 0 ? "UpdateNote" : "InsertNotes");
                cmd.Parameters.AddWithValue("@noteId", assg.NoteId);
                cmd.Parameters.AddWithValue("@userid", assg.UserId);
                cmd.Parameters.AddWithValue("@classId", assg.ClassId);
                cmd.Parameters.AddWithValue("@sectionId", assg.SectionId);
                cmd.Parameters.AddWithValue("@subjectId", assg.SubjectId);
                cmd.Parameters.AddWithValue("@academicyear", assg.AcademicYear);
                cmd.Parameters.AddWithValue("@attachment", attachmentUrl);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                if (res <= 0)
                {
                    errorMessage = "some error occured";
                    if (!string.IsNullOrEmpty(attachmentUrl))
                    {
                        string serverpath = HttpContext.Current.Server.MapPath("~" + attachmentUrl);
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
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
        public List<NoteModel> GetAllNotes(int teacherid)
        {
            List<NoteModel> notes = new List<NoteModel>();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_NotesManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "GetAllNotes");
                cmd.Parameters.AddWithValue("@userid", teacherid);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    notes.Add(new NoteModel
                    {
                        NoteId = Convert.ToInt32(reader["noteId"]),
                        ClassId = Convert.ToInt32(reader["classId"]),
                        ClassName = reader["className"].ToString(),
                        SectionId = Convert.ToInt32(reader["sectionId"]),
                        SectionName = reader["sectionName"].ToString(),
                        SubjectId = Convert.ToInt32(reader["subjectId"]),
                        SubjectName = reader["subject"].ToString(),
                        AcademicYear = reader["academicYear"].ToString(),
                        Attachmentpath = reader["attachment"].ToString(),
                    });
                }

                reader.Close();
            }
            finally
            {
                connection.Close();
            }

            return notes;
        }
        public string DeleteNote(int noteid)
        {
            string result = "";

            try
            {
                SqlCommand cmd = new SqlCommand("sp_NotesManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "DeleteNotes");
                cmd.Parameters.AddWithValue("@noteid", noteid);
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

        public NoteModel GetNoteById(int noteId)
        {
            NoteModel note = null;

            SqlCommand cmd = new SqlCommand("sp_NotesManagement", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@action", "GetNoteById");
            cmd.Parameters.AddWithValue("@noteId", noteId);

            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                note = new NoteModel
                {
                    NoteId = Convert.ToInt32(reader["noteId"]),
                    UserId = Convert.ToInt32(reader["userid"]),
                    ClassId = Convert.ToInt32(reader["classId"]),
                    SectionId = Convert.ToInt32(reader["sectionId"]),
                    SubjectId = Convert.ToInt32(reader["subjectId"]),
                    AcademicYear = reader["academicYear"].ToString(),
                    Attachmentpath = reader["attachment"].ToString()
                };
            }
            connection.Close();
            return note;
        }
        public bool UpdateGrade(SubmitGradeModel sg)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_assignmentManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "updategradeofstudent");
                cmd.Parameters.AddWithValue("@id", sg.assignmentId);
                cmd.Parameters.AddWithValue("@studentId", sg.studentId);
                cmd.Parameters.AddWithValue("@remark", sg.remark);
                cmd.Parameters.AddWithValue("@grade", sg.grade);
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                return res>0;
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

        public List<HolidayModel> GetHolidaysForAll(int userId)
        {
            List<HolidayModel> holidays = new List<HolidayModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_Holiday", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "HolidaysForAll");
                cmd.Parameters.AddWithValue("@userid", userId);

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

        public List<HolidayModel> GetHolidaysTodayAndTomorrow(int userId)
        {
            List<HolidayModel> holidays = new List<HolidayModel>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_Holiday", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "HolidaysTodayAndTomorrow");
                cmd.Parameters.AddWithValue("@userid", userId);

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

        internal object GetSyllabusByClassAndSection(int teacherId)
        {
            try
            {
                List<AddSyllabusMoedel> list = new List<AddSyllabusMoedel>();
                SqlCommand cmd = new SqlCommand("sp_ManageSyllabus", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectallsyllabusforteacher");
                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                connection.Open();
                SqlDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    list.Add(new AddSyllabusMoedel
                    {
                        id = Convert.ToInt32(res["id"]),
                        subjectName = res["subject"].ToString(),
                        subjectId = Convert.ToInt32(res["subjectId"]),
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
    }
}