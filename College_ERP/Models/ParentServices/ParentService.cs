using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data;
using static College_ERP.Models.ParentServices.main;

namespace College_ERP.Models.ParentServices
{
    public class ParentService
    {
        private readonly SqlConnection connection;
        private SqlCommand cmd;
        public ParentService()
        {
            connection = connection = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
        }
        public List<ParentProfileModel> ParentProfile(int studentid)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageParentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectparentdetails");
                cmd.Parameters.AddWithValue("@studentid", studentid);
                connection.Open();
                var res = cmd.ExecuteReader();
                List<ParentProfileModel> list = new List<ParentProfileModel>();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        ParentProfileModel model = new ParentProfileModel
                        {
                            FatherName = res["FatherName"].ToString(),
                            FatherOfficeAddress = res["FatherOfficeAddress"].ToString(),
                            FatherQualification = res["FatherQualification"].ToString(),
                            FatherOccupation = res["FatherOccupation"].ToString(),
                            FatherNo = res["FatherOfficeNo"].ToString(),
                            FatherPhoto = res["FatherPhoto"].ToString(),
                            MotherName = res["MotherName"].ToString(),
                            MotherOfficeAddress = res["MotherOfficeAddress"].ToString(),
                            MotherQualification = res["MotherQualification"].ToString(),
                            MotherOccupation = res["MotherOccupation"].ToString(),
                            MotherNo = res["MotherOfficeNo"].ToString(),
                            MotherPhoto = res["MotherPhoto"].ToString(),
                            ParentEmail = res["parentEmail"].ToString()
                        };
                        list.Add(model);
                    }
                }
                return list;
            }
            catch(Exception ex)
            {
                throw new Exception("Error fetching parent profile: " + ex.Message);
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
                cmd.Dispose();
            }
        }
        public List<StudentModel> GetStudentsList(int studentid)
        {
            try
            {
                cmd = new SqlCommand("sp_ManageParentPanel", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectstudents");
                cmd.Parameters.AddWithValue("@studentid", studentid);
                connection.Open();
                var res = cmd.ExecuteReader();
                List<StudentModel> list = new List<StudentModel>();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        StudentModel model = new StudentModel
                        {
                           studentid = Convert.ToInt32(res["studentid"]),
                           studentname = res["studentname"].ToString(),
                           studentImage = res["StudentPhoto"].ToString(),
                           className = res["classname"].ToString(),
                           admissionstage = res["admissionStage"].ToString(),
                           classId = res["classid"] != DBNull.Value ? Convert.ToInt32(res["classid"]):0,
                            sectionId = res["SectionId"] != DBNull.Value ? Convert.ToInt32(res["SectionId"]):0,
                        };
                        list.Add(model);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching parent profile: " + ex.Message);
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
                cmd.Dispose();
            }
        }
    }
}