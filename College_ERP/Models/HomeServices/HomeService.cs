using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using LCMIS.Server.Model;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using College_ERP.Models.MailService;
using College_ERP.Models.Login;

namespace College_ERP.Models.HomeServices
{
    public class HomeService
    {
        private readonly SqlConnection con;
        private SqlCommand cmd;
        public HomeService()
        {
            con = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
        }
        public int GetUserId(string username)
        {
            try
            {
                int userId = 0;
                cmd = new SqlCommand("sp_loginmanager", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectUserID");
                cmd.Parameters.AddWithValue("@username", username);
                con.Open();
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
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                    cmd.Dispose();
                }

            }
        }
        public bool IsInHostelornot(int userid)
        {
            try
            {
                bool isInHostel = false;

                cmd = new SqlCommand("sp_loginmanager", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "IsInHostel");
                cmd.Parameters.AddWithValue("@userid", userid);

                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        isInHostel = Convert.ToBoolean(reader["IsInHostel"]);
                    }
                }

                return isInHostel;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();

                cmd?.Dispose();
            }
        }

        #region jwt token

        public bool SaveRefreshToken(int userId, string refreshToken, DateTime expiryDate)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_loginmanager", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@action", "saverefreshtoken");
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@RefreshToken", refreshToken);
                cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }


        public RefreshTokenModel GetRefreshToken(string refreshToken)
        {
            RefreshTokenModel model = null;

            try
            {
                SqlCommand cmd = new SqlCommand("sp_loginmanager", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "GetByToken");
                cmd.Parameters.AddWithValue("@RefreshToken", refreshToken);
                if (con.State == ConnectionState.Closed)
                    con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    model = new RefreshTokenModel
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        UserId = Convert.ToInt32(dr["UserId"]),
                        Username = dr["Username"].ToString(),
                        RefreshToken = dr["RefreshToken"].ToString(),
                        ExpiryDate = Convert.ToDateTime(dr["ExpiryDate"]),
                        IsRevoked = Convert.ToBoolean(dr["IsRevoked"])
                    };
                }

                return model;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        public bool UpdateRefreshToken(int userId, string refreshToken, DateTime expiryDate)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_loginmanager", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "Update");
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@RefreshToken", refreshToken);
                cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                if (con.State == ConnectionState.Closed)
                    con.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        public string GetRoleByUserId(string username)
        {
            try
            {
                string role = null;

                SqlCommand cmd = new SqlCommand("sp_loginmanager", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "GetRoleByUserid");
                cmd.Parameters.AddWithValue("@username", username);

                if (con.State == ConnectionState.Closed)
                    con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    role = dr["role"].ToString();
                }

                dr.Close();

                return role;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }



        #endregion
        public byte[] PDFConverter(string htmlContent)
        {
            // Generate  the PDF document from HTML
            PdfDocument pdf = PdfGenerator.GeneratePdf(htmlContent, PdfSharp.PageSize.A4);
            MemoryStream ms = new MemoryStream();

            // Save the PDF to the MemoryStream
            pdf.Save(ms);
            //  ms.Position = 0;  // Reset stream position to the beginning

            // Return the PDF file to the client for download
            return ms.ToArray();

        }

        public ResetPasswordResponse GetUserResetDetailsByUsername(string username)
        {
            var result = new ResetPasswordResponse();


            SqlCommand cmd = new SqlCommand("sp_ResetPasswordByUsername", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Username", username);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                result.Status = reader["Status"] != DBNull.Value && Convert.ToBoolean(reader["Status"]);
                result.Message = reader["Message"].ToString();
                result.Email = reader["Email"]?.ToString();
                result.FullName = reader["FullName"]?.ToString();
            }
            else
            {
                result.Status = false;
                result.Message = "No data returned from stored procedure.";
            }
            return result;
        }
        public int GenerateAndSendOtp(string username, out string message)
        {
            try
            {
                ResetPasswordResponse result = new ResetPasswordResponse();
                SqlCommand cmd = new SqlCommand("sp_ResetPasswordByUsername", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    result.Status = reader["Status"] != DBNull.Value && Convert.ToBoolean(reader["Status"]);
                    result.Message = reader["Message"].ToString();
                    result.Email = reader["Email"]?.ToString();
                    result.FullName = reader["FullName"]?.ToString();
                }
                if (!result.Status)
                {
                    message = result.Message;
                    return 0;
                }

                Random random = new Random();
                int otp = random.Next(1000, 9999);
                string subject = "Reset Your Password";
                string body = $"Hello {result.FullName},<br/><br/>" +
                              $"Use the following OTP to reset your password:<br/>" +
                              $"<h2 style='color: #2e6c80;'>{otp}</h2><br/>" +
                              $"This OTP is valid for 10 minutes.<br/><br/>" +
                              $"If you did not request this, please ignore this email.";

                College_ERP.Models.MailService.MailService mailService = new College_ERP.Models.MailService.MailService();
                CommonMessage emailResult = mailService.SendEmail(subject, body, result.Email);
                if (!emailResult.status)
                {
                    message = emailResult.message;
                    return -1;
                }
                message = "OTP sent successfully to your email.";
                return otp;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return -1;
            }
        }
        public bool UpdateUserPassword(string username, string newPassword, out string message)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateUserPassword", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@NewPassword", newPassword);

                con.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows < 0)
                {
                    message = "Failed to update password.";
                    return false;
                }
                var user = new HomeService().GetUserResetDetailsByUsername(username);
                if (!user.Status)
                {
                    message = "Password updated but failed to send email.";
                    return false;
                }

                string subject = "Your Password Has Been Changed!";
                string body = $"Hello {user.FullName},<br/><br/>" +
                              $"Your password has been successfully changed.<br/>" +
                              $"<strong>Username:</strong> {username}<br/>" +
                              $"<strong>New Password:</strong> {newPassword}<br/><br/>" +
                              $"If you did not make this change, please contact support immediately.";

                College_ERP.Models.MailService.MailService mailService = new College_ERP.Models.MailService.MailService();
                var emailResult = mailService.SendEmail(subject, body, user.Email);

                if (!emailResult.status)
                {
                    message = "Password updated, but failed to send email.";
                    return true;
                }
                message = "Password updated and email sent successfully.";
                return rows > 0;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
            finally
            {
                con.Close();
            }
        }


    }
}