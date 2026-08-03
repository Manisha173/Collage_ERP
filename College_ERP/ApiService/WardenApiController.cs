using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
using System.Web.Http;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.SuperAdmin;
using College_ERP.Models.Teacher;
using College_ERP.Models.Warden;
using College_ERP.Models.Admin;
using Newtonsoft.Json;
using System.Reflection;
using Microsoft.Ajax.Utilities;
using College_ERP.Models.MailService;

namespace College_ERP.ApiService
{
    public class WardenApiController : ApiController
    {
        private readonly AdminServices _admin;
        private readonly SuperAdminDataService _superAdmin;
        private readonly HomeService _home;
        private readonly WardenService _warden;
        public WardenApiController()
        {
            _admin = new AdminServices();
            _home = new HomeService();
            _superAdmin = new SuperAdminDataService();
            _warden = new WardenService();
        }

        [Route("api/GetStudentsInBlock")]
        [HttpGet]
        public IHttpActionResult GetStudentsInBlock(int id)
         {
            try
            {
                List<StudentDetailModel> res = _warden.GetStudentsInBlock(id);
                return Ok(new { status = true, data = res, message = "data retrieve" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #region MealManagement
        [Route("api/InsertMeal")]
        [HttpPost]
        public IHttpActionResult InsertMeal()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                int userid = Convert.ToInt32(httpRequest.Form.Get("userid"));

                List<MealViewModel> models = new List<MealViewModel>();
                string datamodel= httpRequest.Form.Get("models");
                models=JsonConvert.DeserializeObject<List<MealViewModel>>(datamodel);
                int res = _warden.InsertMeal(models, userid);
                if (res > 0)
                {
                    return Ok(new { status = true, message = "Meal inserted successfully." });
                }
                else
                {
                    return Ok(new { status = false, message = "Failed to insert meal." });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/InsertAndUpdateMeal")]
        [HttpPost]
        public IHttpActionResult InsertAndUpdateMeal()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                int userid = Convert.ToInt32(httpRequest.Form.Get("wardenid"));
                string meals = httpRequest.Form.Get("meals"); 

                if (string.IsNullOrEmpty(meals))
                    return BadRequest("Meals data is required.");

                var mealss = JsonConvert.DeserializeObject<List<MealViewModel>>(meals);
                
                if (mealss == null || mealss.Count == 0)
                    return BadRequest("Invalid meal data.");

                var request = new MealUpdateRequest
                {
                    userid = userid,
                    Meals = mealss
                };
                
                bool result = _warden.UpdateMeal(request);
                
                if (result)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = result ? 200 : 400,
                        MessageProcessingHandler = result ? (request.Meals[0].MealId > 0 && request.Meals[0].Menus[0].MenuId>0? "Meal updated successfully!" : "Meal Inserted successfully!") : "Server Error Occurred!",
                    });
                }
                else
                {
                    return Ok(new { status = false, message = "Update failed!. Transaction rolled back." });
                }
                    
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/GetAllMeals")]
        [HttpGet]

        public IHttpActionResult GetAllMeals(int wardenId)
        {
            try
            {
                List<MealViewModel> res = _warden.GetAllMeals(wardenId);
                var newMeal = res.Select(e => new
                {
                    day=e.Day,
                    details=res.Where(d=>d.Day==e.Day).Select(f=>new
                    {
                        mealId=f.MealId,
                        startTime=f.StartTime,
                        endTime=f.EndTime,
                        createdDate=f.CreatedDate,
                        menus=f.Menus
                    })
                }).DistinctBy(e=>e.day);
                return Ok(new { status = true, data = newMeal, message = "data retrieved" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/DeleteMealSchedule")]
        [HttpGet]

        public IHttpActionResult DeleteMealSchedule(int mealId)
        {
            try
            {
                bool res = _warden.DeleteMealSchedule(mealId);
                return Ok(new { status = true, message = "data deleted" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region WardenCommunication
        [Route("api/InsertWardenCommunication")]
        [HttpPost]

        public IHttpActionResult InsertWardenCommunication()
        {
            try
            {
                string errorMessage = "";
                var httpRequest = HttpContext.Current.Request;
                int userid = Convert.ToInt32(httpRequest.Form.Get("wardenId"));
                var data = new CommunicationModel
                {
                    Title = httpRequest.Form.Get("Title").ToString(),
                    Description=httpRequest.Form.Get("Description").ToString(),
                };
                HttpPostedFile file = httpRequest.Files["Attachment"];
                if(file!=null)
                {
                    HttpPostedFileWrapper cfile = new HttpPostedFileWrapper(file);
                    data.Attachments = cfile;
                }
                bool res = _warden.InsertCommunication(data,out errorMessage, userid);
                return Ok(new
                {
                    status = res,
                    StatusCode = res ? 200 : 400,
                    MessageProcessingHandler = res ? "Communication Added Successfully" :errorMessage
                });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }

        }

        [Route("api/GetAllWardenCommunication")]
        [HttpGet]
        public IHttpActionResult GetAllWardenCommunication(int wardenid)
        {
            try
            {
                List<CommunicationModel> res = _warden.GetAllCommunication(wardenid);
                return Ok(new { status = true, data = res, message = "data retrieved" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/DeleteWardenCommunication")]
        [HttpPost]

        public IHttpActionResult DeleteWardenCommunication(int communicationId)
        {
            try
            {
                bool res = _warden.DeleteCommunicationById(communicationId);
                return Ok(new { status = true, message = "Communication Deleted Successfully!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }


        [Route("api/GetCommunicationById")]
        [HttpGet]
        public IHttpActionResult GetCommunicationById(int communicationId)
        {
            try
            {
                CommunicationModel res = _warden.GetCommunicationById(communicationId);
                return Ok(new { status = true, data = res, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/UpdateWardenCommunication")]
        [HttpPost]
        public IHttpActionResult UpdateWardenCommunication()
        {
            try
            {
                string errorMessage = "";
                var httpRequest = HttpContext.Current.Request;
                int communicationId = Convert.ToInt32(httpRequest.Form.Get("communicationId"));
                var data = new CommunicationModel
                {
                    Title = httpRequest.Form.Get("Title").ToString(),
                    Description = httpRequest.Form.Get("Description").ToString(),
                    CommunicationId = communicationId
                };
                HttpPostedFile file = httpRequest.Files["Attachment"];
                if (file != null)
                {
                    HttpPostedFileWrapper cfile = new HttpPostedFileWrapper(file);
                    data.Attachments = cfile;
                }
                bool res = _warden.UpdateCommunication(data, out errorMessage);
                return Ok(new
                {
                    status = res,
                    StatusCode = res ? 200 : 400,
                    MessageProcessingHandler = res ? "Communication Updated Successfully" : errorMessage
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
    }
        #endregion
        #region WardenNotices
        [Route("api/GetWardenNotices")]
        [HttpGet]
    
         public IHttpActionResult GetWardenNotices(int wardenId)
        {
            try
            {
                int adminid = _warden.GetAdminId(wardenId);
                List<NoticeModel> res = _warden.GetWardenNotices("Warden", wardenId, adminid);
                return Ok(new { status = true, data = res, message = "data retrieved.." });
            }
            catch(Exception ex)
            {
                return Ok(new { status = true, message = ex.Message });
            }
        }
        #endregion
        #region FeesRecord
        [Route("api/InsertFeesRecord")]
        [HttpPost]
        public IHttpActionResult InsertFeesRecord()
        {
            try
            {
                string errorMessage = "";
                var httpRequest = HttpContext.Current.Request;
                var data = new FeeRecordsModel
                {
                    FeeType = httpRequest.Form.Get("FeeType").ToString(),
                    FeesSubmitted=Convert.ToInt32(httpRequest.Form.Get("FeesSubmitted")),
                    hostelId=Convert.ToInt32(httpRequest.Form.Get("hostelId")),
                    PaymentDate=Convert.ToDateTime(httpRequest.Form.Get("PaymentDate")),
                    RemainingFee = Convert.ToDecimal(httpRequest.Form.Get("RemainingFee")),
                    transactionid = httpRequest.Form.Get("TransactionId").ToString(),
                    DueDate = Convert.ToDateTime(httpRequest.Form.Get("DueDate"))
                };
                HttpPostedFile FeeSlip = httpRequest.Files["FeeSlip"];
                if(FeeSlip!=null)
                {
                    HttpPostedFileWrapper cfile = new HttpPostedFileWrapper(FeeSlip);
                    data.FeeSlip = cfile;
                }
                bool res = _warden.InsertFeesRecord(data, out errorMessage);
                return Ok(new
                    {

                        status = true,
                        StatusCode=res?200:400,
                        MessageProcessingHandler=res? "Fee Records Inserted Successfully!":errorMessage,
                    });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
           
        }

        [Route("api/GetAllStudentInBlock")]
        [HttpGet]
        public IHttpActionResult GetAllStudentInBlock(int wardenid)
        {
            try
            {
                var res = _warden.GetStudentsInBlock(wardenid);
                return Ok(new { status=true,data=res,message="data retrieved!"});
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/GetBlockDetails")]
        [HttpGet]
        public IHttpActionResult GetBlockDetails(int wardenid)
        {
            try
            {
                var res = _warden.GetBlockDetails(wardenid);
                return Ok(new { status=true,data=res,message="data retrieved!"});
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/GetRoomedFloors")]
        [HttpGet]
        public IHttpActionResult GetRoomedFloors(int wardenid)
        {
            try
            {
                var res = _warden.GetFloorByWardenId(wardenid);
                return Ok(new { status=true,data=res,message="data retrieved!"});
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/GetRoomsByFloorNo")]
        [HttpGet]
        public IHttpActionResult GetRoomsByFloor(int floorno,int wardenid)
        {
            try
            {
                var res = _warden.GetRoomsByFloor(floorno,wardenid);
                return Ok(new { status=true,data=res,message="data retrieved!"});
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/GetStudentDetailOnReallocate")]
        [HttpGet]
        public IHttpActionResult GetDataToReallocate(int studentid)
        {
            try
            {
                var res = _warden.GetDetailForRoomAllocation(studentid);
                return Ok(new { status=true,data=res,message="data retrieved!"});
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/GetAllFeeRecords")]
        [HttpGet]
        public IHttpActionResult GetAllFeeRecords(int userid)
        {
            try
            {
                int adminid = _warden.GetAdminId(userid);
                List<FeeRecordsModel> res = _warden.GetLastFeeRecords(adminid);
                return Ok(new { status=true,data=res,message="data retrieved!"});
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/GetFeeRecordById")]
        [HttpGet]
        public IHttpActionResult GetFeeRecordById(int FeeRecordId)
        {
            try
            {
                FeeRecordsModel res = _warden.GetFeeRecordById(FeeRecordId);
                return Ok(new { status = true, data = res, message = "data retrieved!" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }


        [Route("api/UpdateFeeRecord")]
        [HttpPost]
        public IHttpActionResult UpdateFeeRecord()
        {
            try
            {
                string errorMessage = "";
                var httpRequest = HttpContext.Current.Request;
                int id = Convert.ToInt32(httpRequest.Form.Get("id"));
                var data = new FeeRecordsModel
                {
                 
                    FeeType = httpRequest.Form.Get("FeeType").ToString(),
                    FeesSubmitted = Convert.ToInt32(httpRequest.Form.Get("FeesSubmitted")),
                    hostelId = Convert.ToInt32(httpRequest.Form.Get("hostelId")),
                    PaymentDate = Convert.ToDateTime(httpRequest.Form.Get("PaymentDate")),
                    id=id,
                };
                HttpPostedFile FeeSlip = httpRequest.Files["FeeSlip"];
                if (FeeSlip != null)
                {
                    HttpPostedFileWrapper cfile = new HttpPostedFileWrapper(FeeSlip);
                    data.FeeSlip = cfile;
                }
                bool res = _warden.UpdateFeeRecord(data, out errorMessage);
                return Ok(new
                {

                    status = true,
                    StatusCode = res ? 200 : 400,
                    MessageProcessingHandler = res ? "Fee Records Updated Successfully!" : errorMessage,
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        
        }
        #endregion
        #region HolidaysForAll
        [Route("api/GetHolidaysForAll")]
        [HttpGet]
        public IHttpActionResult GetHolidaysForAll(int wardenId)
        {
            try
            {
                int adminId = _warden.GetAdminId(wardenId);
                List<HolidayModel> res = _warden.GetHolidaysForAll(adminId);
                return Ok(new { status = true, data = res, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region RoomAllocation 
        [Route("api/GetStudentDetailsByAdmissionNo")]
        [HttpGet]
        public IHttpActionResult GetStudentDetailsByAdmissionNo(string admissionNo,int wardenid)
        {
            try
            {
                int adminId = _warden.GetAdminId(wardenid);
                List<UserOrdersModel> res = _warden.SeletUserForRoomAllocation(admissionNo, adminId);
                return Ok(new { status = true, data = res, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/SelectUsersByRoomNo")]
        [HttpGet]
        public IHttpActionResult SelectUsersByRoomNo(int roomNo, int wardenId)
        {
            try
            {
                int adminId = _warden.GetAdminId(wardenId);
                List<UserOrdersModel> res = _warden.SelectUsersByRoomNo(roomNo, adminId);
                return Ok(new { status = true, data = res, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/GetRoomInfo")]
        [HttpGet]
        public IHttpActionResult GetRoomInfo(int roomId,int wardenId)
        {
            try
            {
                int adminId = _warden.GetAdminId(wardenId);
                College_ERP.Models.Warden.RoomInfo res = _warden.GetRoomInfo(roomId, adminId);
                return Ok(new { status = true, data = res, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false,message = ex.Message });
            }

        }

        [Route("api/GetRoomsByBlockId")]
        [HttpGet]
        public IHttpActionResult GetRoomsByBlockId(int blockId, int wardenid)
        {
            try
            {
                int adminId = _warden.GetAdminId(wardenid);
                List<RoomModel> rooms = _warden.GetRoomsByBlockId(blockId, adminId);
                return Ok(new { status = true, data = rooms, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }


        [Route("api/InsertReallocatedRoom")]
        [HttpPost]
        public IHttpActionResult InsertReallocatedRoom()
        {
            try
            {

                string errorMessage = "";
                var httpRequest = HttpContext.Current.Request;

                var data = new ReallocateRoomModel
                {
                    RoomId = Convert.ToInt32(httpRequest.Form.Get("RoomId")),
                    StudentId = Convert.ToInt32(httpRequest.Form.Get("StudentId")),
                    RemainingFees = Convert.ToInt32(httpRequest.Form.Get("RemainingFees")),
                };
                bool res = _warden.InsertReallocatedRoom(data, out errorMessage);
                if (res)
                {
                    return Ok(new
                    {

                        status = true,
                        StatusCode = res ? 200 : 400,
                        MessageProcessingHandler = res ? "Room Reallocated Successfully!" : "Server Error Occurred!",
                    });
                }
                else
                {
                    return Ok(new { status = false, message = "Failed to Reallocate Room!." });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }

        }
        #endregion
        #region WardenDashboard
        [Route("api/WardenDashboard")]
        [HttpGet]
        public IHttpActionResult WardenDashboard(int wardenId)
        {
            try
            {
                int adminId = _warden.GetAdminId(wardenId);
                WardenDashboardModel model = _warden.GetWardenDashboard(adminId, wardenId);
                return Ok(new { status = true, data = model, message = "data retrieved!" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/GetNoticeCount")]
        [HttpGet]
        public IHttpActionResult GetNoticeCount(int wardenId)
        {
            try
            {
                int adminId = _warden.GetAdminId(wardenId);
                WardenDashboardModel model = _warden.GetNoticeCount(adminId, wardenId);
                return Ok(new { status = true, data = model, message = "data retrieved!" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        #region HostelProblems
        [Route("api/GetHostelProblem")]
        [HttpGet]
        public IHttpActionResult GetHostelProblem(int wardenid)
        {
            try 
            {
                List<HostelProblemsModel> list = _warden.GetHostelProblem(wardenid);
                return Ok(new { status = true, data = list, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/CompleteRejectHostelProblem")]
        [HttpPost]
        public IHttpActionResult CompleteRejectHostelProblem(int id, int status, string reason)
        {
            try
            {
                bool res = _warden.CompleteRejectHostelProblem(id, status, reason);
                return Ok(new
                {
                    status=res,
                    StatusCode=res?200:400,
                    message=res?(status==1?"Completed Successfully!":"Rejected Successfully!"):"Failed to update!",
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        #endregion
        [Route("api/GetHolidaysTodayAndTomorrow")]
        [HttpGet]
        public IHttpActionResult GetHolidaysTodayAndTomorrow(int wardenId)
        {
            try
            {
                int adminId = _warden.GetAdminId(wardenId);
                List<HolidayModel> holidays = _warden.GetHolidaysTodayAndTomorrow(adminId);
                return Ok(new { status = true, data = holidays, message = "data retrieved!" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/GetMealDetailsByDayAndUser")]
        [HttpGet]
        public IHttpActionResult GetMealDetailsByDayAndUser(string day, int wardenId)
        {
            try
            {
               var mealDict = _warden.GetMealDetailsByDayAndUser(day, wardenId);
                return Ok(new { status = true, data = mealDict, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/GetFeeHistory")]
        [HttpGet]
        public IHttpActionResult GetFeeHistory(int studentidhostelid)
        {
            try
            {
               var mealDict = _warden.GetFeeHistoryOfStudent(studentidhostelid);
                return Ok(new { status = true, data = mealDict, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/GetMealScheduleById")]
        [HttpGet]
        public IHttpActionResult GetMealScheduleById(int mealId, int wardenId)
        {
            try
            {
                MealViewModel meal = _warden.GetMealScheduleById(mealId, wardenId);
                return Ok(new { status = true, data = meal, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [Route("api/GetWarden")]
        [HttpGet]

        public IHttpActionResult GetWarden(int wardenId)
        {
            try
            {
                int adminId = _warden.GetAdminId(wardenId);
                College_ERP.Models.Warden.warden list = _warden.GetWarden(adminId);
                return Ok(new { status = true, data = list, message = "data retrieved!" });
            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
        [Route("api/GetRoomDetailByWardenId")]
        [HttpGet]
        public IHttpActionResult GetRoomList(int wardenId)
        {
            try
            {
                int adminId = _warden.GetAdminId(wardenId);
                var data= _warden.GetRoomList(wardenId);
                return Ok(new { status=true,data=data,message="Data retrived!!"});


            }
            catch(Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
    }
}
