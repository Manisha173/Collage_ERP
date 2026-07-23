using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Web;
using Antlr.Runtime.Tree;

namespace College_ERP.Models.Security
{
    public class main
    {
        public class SecurityDetails
        {
            public string securityName { get; set; }
            public int securityId { get; set; }
            public string securityImage { get; set; }
        }
        public class StaffModel
        {
            public int staffId { get; set; }
            public string staffName { get; set; }
            public long staffmobile { get; set; }
        }
        public class VisitorModel
        {
            public string personName { get; set; }
            public int classid { get; set; }
            public int sectionid { get; set; }
            public string staffRole { get; set; }
            public int meetid { get; set; }
            public string loginTime { get; set; }
            public string logOutTime { get; set; }
            public int id { get; set; }
            public int vid { get; set; }
            public int userId { get; set; }
            public HttpPostedFileBase image { get; set; }
            public string imageName { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public long mobile { get; set; }
            public string address { get; set; }
            public string userType { get; set; }
            public string role { get; set; }
            public int personId { get; set; }
            public int studentId { get; set; }
            public string reason { get; set; }
            public string remark { get; set; }
            public Boolean loginStatus { get; set; }
            public int roomNo { get; set; }
        }
        public class SecurityDashboardModel
        {
            public int totalvisitors { get; set; }
            public int loginvisitors { get; set; }
            public int notice { get; set; }
        }
        public class RoomModel
        {
            public int RoomId { get; set; }
            public int RoomNo { get; set; }
        }
        public class UserOrderModel
        {
            public int userId { get; set; }
            public int studentid { get; set; }
            public string userName { get; set; }
            public string className { get; set; }
            public string sectionName { get; set; }
            public string emailId { get; set; }
            public long mobileNo { get; set; }
            public int roomNo { get; set; }
            public int hostelId { get; set; }
            public int blockId { get; set; }
            public int feesperperson { get; set; }
            public string address { get; set; }
        }
    }
}