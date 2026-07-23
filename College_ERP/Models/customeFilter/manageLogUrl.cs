using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace College_ERP.Models.customeFilter
{
    public class manageLogUrl : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            
            base.OnActionExecuting(filterContext);  // Make sure base action executes

            
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var url = filterContext.HttpContext.Request.Url.AbsolutePath;
            var routeValues = filterContext.RouteData;
            var result = filterContext.Result;
            var controller = routeValues.Values["controller"]?.ToString();
            var action = routeValues.Values["action"]?.ToString();
            if (string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(action))
            {
                return;
            }
            int urlLength = url.Split('/').Length;
            var controllerType = Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t =>
                typeof(Controller).IsAssignableFrom(t) &&
                t.Name.Equals(controller + "Controller", StringComparison.OrdinalIgnoreCase));
            if (controllerType == null || controller?.ToLower() != "admin" || !(result is ViewResult) || url.Contains("?") || urlLength >= 4)
            {             
                return;
            }
         
            SaveUrlToDatabase(url);

            
        }

        private void SaveUrlToDatabase(string url)
        {
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["myconn"].ConnectionString);
            try
            {
                int userId = 0;
                SqlCommand cmd = new SqlCommand("sp_loginmanager", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectUserID");
                cmd.Parameters.AddWithValue("@username", HttpContext.Current.User.Identity.Name);
                connection.Open();
                var res = cmd.ExecuteReader();
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        userId = Convert.ToInt32(res["Userid"]);
                    }
                }
                res.Close();
                if (url == "/") url = "/home/login";
                var routeArray = url.Split('?')[0].Split('/');
                string urlName = routeArray.Length >= 2 ? routeArray[2] : routeArray[1];
                cmd = new SqlCommand("sp_LogUrlManagement", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Action", "insertUrl");
                cmd.Parameters.AddWithValue("@userid", userId);
                cmd.Parameters.AddWithValue("@url", url.Split('?')[0]);
                cmd.Parameters.AddWithValue("@name", urlName);
                int res2 = cmd.ExecuteNonQuery();
            }
            catch
            {
                connection.Close();
            }
            finally
            {
                connection.Close();
            }
        }
    }

}