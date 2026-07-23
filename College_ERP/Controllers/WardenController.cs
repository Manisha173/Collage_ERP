using College_ERP.Models.Admin;
using College_ERP.Models.AdminServices;
using College_ERP.Models.HomeServices;
using College_ERP.Models.MailService;
using College_ERP.Models.Teacher;
using College_ERP.Models.Warden;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace College_ERP.Controllers
{
    [Authorize(Roles = "warden")]
    public class WardenController : Controller
    {
        // GET: Warden
        WardenService wardenService = new WardenService();
        private readonly HomeService homeService;
        public WardenController()
        {
            homeService = new HomeService();
        }

        public ActionResult WardenProfile()
        {
            int userId = wardenService.GetUserId(User.Identity.Name).Id;
            var teacherDetails = wardenService.GetWardenById(userId);

            if (teacherDetails == null)
            {
                return View("Error");
            }
            return View(teacherDetails);
        }
        public ActionResult Dashboard()
        {
            int userId = wardenService.GetUserId(User.Identity.Name).userId;
            int wardenId = homeService.GetUserId(User.Identity.Name);

            var result = wardenService.GetWardenDashboard(userId, wardenId);
            return View(result);
        }
        [HttpGet]
        public JsonResult GetNoticeCount()
        {

            int AdminId = wardenService.GetUserId(User.Identity.Name).userId;
            int UserId = homeService.GetUserId(User.Identity.Name);
            var result = wardenService.GetNoticeCount(AdminId, UserId);
            return Json(new { notice = result.notice }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetWardenDetails()
        {
            int userId = wardenService.GetUserId(User.Identity.Name).userId;
            int wardenId = homeService.GetUserId(User.Identity.Name);
            var warden = wardenService.GetWardenById(wardenId);

            if (warden != null)
            {
                var result = new
                {
                    WardenId = warden.Id,
                    WardenName = warden.Name,
                    BlockName = warden.BlockName,
                    ProfileImage = warden.Document

                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult StudentsInBlock()
        {

            int userId = homeService.GetUserId(User.Identity.Name);
            var model = wardenService.GetStudentsInBlock(userId);
            ViewData["floors"] = wardenService.GetFloorByWardenId(userId);
            return View(model);
        }
        [HttpGet]
        public JsonResult GetRoomDetails(int roomId)
        {
            int userId = wardenService.GetUserId(User.Identity.Name).userId;
            var result = wardenService.GetRoomInfo(roomId, userId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetRoomInfo(int? chroom)
        {
            int wardenid = homeService.GetUserId(User.Identity.Name);
            int userId = wardenService.GetAdminId(wardenid);
            var result = wardenService.GetRoomInfoByRoomNo(Convert.ToInt32(chroom), userId, wardenid);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ReallocateRoom(ReallocateRoomModel model)
        {
            string errorMessage;
            model.userid = wardenService.GetUserId(User.Identity.Name).userId;
            try
            {
                bool isSuccess = wardenService.InsertReallocatedRoom(model, out errorMessage);

                if (isSuccess)
                {
                    return Json(new { success = true, message = "Room reallocated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public ActionResult MealSchedule()
        {
            int userId = homeService.GetUserId(User.Identity.Name);

            var meals = wardenService.GetAllMeals(userId);

            foreach (var meal in meals)
            {
                meal.Menus = meal.Menus ?? new List<MenuViewModel>();
            }

            return View(meals);
        }
        [HttpPost]
        public JsonResult MealSchedule(List<MealViewModel> meals)
        {
            try
            {
                int userId = homeService.GetUserId(User.Identity.Name);
                int lastMealId = wardenService.InsertMeal(meals, userId);

                return Json(new { success = true, message = "Meals and menus inserted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        public JsonResult GetMealDetailsByDay(string day)
        {
            try
            {
                int userId = homeService.GetUserId(User.Identity.Name);
                //var meals = wardenService.GetMealsByDay(day, userId); 
                var meals = wardenService.GetMenusByDay(day, userId);

                return Json(new { success = true, data = meals }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult DeleteMealSchedule(int mealId)
        {
            try
            {
                bool isDeleted = wardenService.DeleteMealSchedule(mealId);

                if (isDeleted)
                {
                    return Json(new { success = true, message = "MealSchedule deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete MealSchedule." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetMealScheduleById(int mealId)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var meal = wardenService.GetMealScheduleById(mealId, userId);
            if (meal != null)
            {
                return Json(new { success = true, data = meal }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "MealSchedule not found" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMealDetails(string day)
        {
            try
            {
                int userId = homeService.GetUserId(User.Identity.Name);
                var mealDetails = wardenService.GetMealDetailsByDayAndUser(day, userId);

                if (mealDetails != null && mealDetails.Any())
                {
                    return Json(new { success = true, data = mealDetails }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { success = false, message = "No meal details found for the selected day." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while fetching meal details." }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult UpdateMealDetails(MealUpdateRequest request)
        {
            try
            {
                string username = User.Identity.Name;
                int userid = homeService.GetUserId(username);
                request.userid = userid;
                bool res = wardenService.UpdateMeal(request);
                if (res)
                {

                    return Json(new { success = true, message = "Meal updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Some error occured." });

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        public ActionResult Communication()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var communicationList = wardenService.GetAllCommunication(userId);
            return View(communicationList);
        }

        [HttpPost]
        public JsonResult Communication(CommunicationModel model)
        {
            string errorMessage = string.Empty;
            int userId = homeService.GetUserId(User.Identity.Name);
            bool isSuccess = wardenService.InsertCommunication(model, out errorMessage, userId);

            return Json(new { success = isSuccess, message = isSuccess ? "Inserted Successfully!" : errorMessage });
        }

        [HttpPost]
        public JsonResult DeleteCommunication(int communicationId)
        {
            bool success = false;
            string message = "";

            try
            {
                success = wardenService.DeleteCommunicationById(communicationId);
                message = success ? "Communication deleted successfully." : "Delete failed.";
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
            }

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCommunicationById(int id)
        {
            CommunicationModel data = null;
            string message = "";

            try
            {
                data = wardenService.GetCommunicationById(id);
                if (data != null)
                {
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    message = "Communication not found.";
                }
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
            }

            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateCommunication(CommunicationModel model)
        {
            try
            {
                string errorMessage;
                bool isUpdated = wardenService.UpdateCommunication(model, out errorMessage);

                if (isUpdated)
                {
                    return Json(new { success = true, message = "Communication updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult GetUserByAdmissionNo(string userNo, string userType)
        {
            int userId = wardenService.GetUserId(User.Identity.Name).userId;
            var data = wardenService.SeletUserForRoomAllocation(userNo, userId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetStudentsByRoomNo(int roomNo)
        {
            int userId = wardenService.GetUserId(User.Identity.Name).userId;
            var data = wardenService.SelectUsersByRoomNo(roomNo, userId);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetRoomsByBlockId(int blockId)
        {
            int userId = wardenService.GetUserId(User.Identity.Name).userId;
            var data = wardenService.GetRoomsByBlockId(blockId, userId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetRoomsByFloor(int floorno)
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            var data = wardenService.GetRoomsByFloor(floorno, userId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Notice()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            int adminid = wardenService.GetUserId(User.Identity.Name).userId;
            var notices = wardenService.GetWardenNotices("Warden", userId, adminid);
            return View(notices);
        }

        [HttpGet]

        public ActionResult FeeRecords()
        {
            int adminid = wardenService.GetUserId(User.Identity.Name).userId;
            var data = wardenService.GetLastFeeRecords(adminid);
            return View(data);
        }

        [HttpPost]
        public ActionResult FeeRecords(FeeRecordsModel model)
        {
            string errorMessage;
            int currentUserId = wardenService.GetUserId(User.Identity.Name).userId;
            try
            {
                bool isSuccess = wardenService.InsertFeesRecord(model, out errorMessage);

                if (isSuccess)
                {
                    return Json(new { success = true, message = "Fees record added successfully." });
                }
                else
                {
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        public JsonResult GetFeeRecordById(int id)
        {
            try
            {
                var model = wardenService.GetFeeRecordById(id);
                if (model != null)
                {
                    return Json(new { success = true, data = model }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Record not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult UpdateFeeRecord(FeeRecordsModel model)
        {
            string errorMessage;
            try
            {
                bool isSuccess = wardenService.UpdateFeeRecord(model, out errorMessage);

                if (isSuccess)
                {
                    return Json(new { success = true, message = "Fees record added successfully." });
                }
                else
                {
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        public ActionResult Holidays()
        {
            int userId = wardenService.GetUserId(User.Identity.Name).userId;
            var holidays = wardenService.GetHolidaysForAll(userId);
            return View(holidays);
        }
        public ActionResult HostelProblems()
        {
            int userId = homeService.GetUserId(User.Identity.Name);
            ViewData["problemlist"] = wardenService.GetHostelProblem(userId);
            return View();
        }
        [HttpPost]
        public ActionResult CompleteOrRejectHostelProblem(int id, int status, string reason)
        {
            bool res = wardenService.CompleteRejectHostelProblem(id, status, reason);
            return Json(new
            {
                status = res,
                message = res && status == 1 ? "Completed Problem Successfully" : res && status == 2 ? "Rejected Problem Successfully" : "Some error occured"
            });
        }
        public ActionResult BlockDetails()
        {
            int wardenid = homeService.GetUserId(User.Identity.Name);
            ViewData["blockdetails"] = wardenService.GetBlockDetails(wardenid);
            return View();
        }
        public ActionResult RoomList()
        {
            int wardenid = homeService.GetUserId(User.Identity.Name);
            ViewData["roomlist"] = wardenService.GetRoomList(wardenid);
            return View();
        }
        public ActionResult GetDataForReallocte(int studentid)
        {
            var data = wardenService.GetDetailForRoomAllocation(studentid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult StudentFeeHistory(int studentidhostelid)
        {
            if (User.IsInRole("warden") || User.IsInRole("admin"))
            {
                var data = wardenService.GetFeeHistoryOfStudent(studentidhostelid);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            return new HttpUnauthorizedResult();
        }
    }
}