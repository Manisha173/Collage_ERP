using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using College_ERP.Models.AdminServices;
using College_ERP.Models.ParentServices;
using College_ERP.Models.StudentServices;

namespace College_ERP.ApiService
{
    public class ParentApiController : ApiController
    {
        private readonly AdminServices _admin;
        private readonly StudentServices _student;
        private readonly ParentService _parent;
        public ParentApiController()
        {
            _admin = new AdminServices();
            _student = new StudentServices();
            _parent = new ParentService();
        }
        [Route("api/parentdetails")]
        [HttpGet]
        public IHttpActionResult GetParentDetails(int studentid)
        {
            try
            {
                var data = _parent.ParentProfile(studentid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/studentlist")]
        [HttpGet]
        public IHttpActionResult GetAllStudents(int studentid)
        {
            try
            {
                var data = _parent.GetStudentsList(studentid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/parentnotice")]
        [HttpGet]
        public IHttpActionResult GetNotice(int studentid)
        {
            try
            {
                int adminid = _student.GetAdminId(studentid);
                var data = _student.GetStudentNotices("parent", studentid, adminid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/parentcommuncation")]
        [HttpGet]
        public IHttpActionResult GetCommunication(int studentid)
        {
            try
            {
                var data = _student.GetCommunication(studentid, 2);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
    }
}
