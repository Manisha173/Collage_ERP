using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using College_ERP.Models.AdminServices;
using College_ERP.Models.DriverServices;
using static College_ERP.Models.DriverServices.main;
using static College_ERP.Models.StudentServices.main;

namespace College_ERP.ApiService
{
    public class DriverApiController : ApiController
    {
        private readonly AdminServices _admin;
        private readonly DriverService _driver;
        public DriverApiController()
        {
            _admin = new AdminServices();
            _driver = new DriverService();
        }
        #region driverprofile
        [Route("api/driverprofile")]
        [HttpGet]
        public IHttpActionResult GetDriverDetails(int driverid)
        {
            try
            {
                var data = _driver.GetDriverProfile(driverid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region GetDriverAnnouncements
        [Route("api/GetDriverAnnouncements")]
        [HttpGet]
        public IHttpActionResult GetDriverAnnouncements(int driverId)
        {
            try
            {
                int adminid = _driver.GetAdminId(driverId);
                var data = _driver.GetAllCirculars(adminid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Notice
        [Route("api/GetDriverNotice")]
        [HttpGet]
        public IHttpActionResult GetDriverNotice(int driverid)
        {
            try
            {
                int adminid = _driver.GetAdminId(driverid);
                var data = _driver.GetDriverNotices("BusDriver", driverid, adminid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Driver Problem
        [Route("api/InsertDriverProblem")]
        [HttpPost]
        public IHttpActionResult InsertDriverProblem()
        {
            try
            {
                string error = "";
                var request = HttpContext.Current.Request;
                var formdata = new DriverProblemModel
                {
                    driverid = Convert.ToInt32(request.Form.Get("driverid")),
                    problem = request.Form.Get("problem").ToString()
                };
                bool res = _driver.InsertDriverProblem(formdata, out error);
                return Ok(new { status = res, message = res ? "Problem Added Successfully" : error });
            }
            catch
            {
                return Ok(new { status = false, message = "Server error occured" });
            }
        }
        [Route("api/GetDriverProblems")]
        [HttpGet]
        public IHttpActionResult GetDriverProblems(int driverid)
        {
            try
            {
                var data = _driver.GetDriverProblem(driverid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region Student List InBus
        [Route("api/GetStudentsInBus")]
        [HttpGet]
        public IHttpActionResult GetStudentsInBus(int driverid)
        {
            try
            {
                var data = _driver.GetStudentListInBus(driverid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region PickupPoints
        [Route("api/GetPickupPoints")]
        [HttpGet]
        public IHttpActionResult GetPickupPoints(int driverid)
        {
            try
            {
                var data = _driver.GetPickupPoints(driverid);
                return Ok(new { status = data != null, message = data != null ? "Data received." : "Data not found", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion

    }
}
