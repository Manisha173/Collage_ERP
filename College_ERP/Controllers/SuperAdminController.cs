using College_ERP.Models.Admin;
using College_ERP.Models.AdminServices;
using College_ERP.Models.Security;
using College_ERP.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace College_ERP.Controllers
{
	[Authorize(Roles= "superadmin")]
    public class SuperAdminController : Controller

    {
		SuperAdminDataService superAdminDataService = new SuperAdminDataService();
        // GET: SuperAdmin
        public ActionResult Dashboard()
        {
            var model = superAdminDataService.GetSuperadminDashboard();
            return View(model);
        }
		public ActionResult Company()
		{
			return View();
		}
		public ActionResult CompanyRegistration(int? Id)
		{
			if (Id.HasValue)
			{
				ViewBag.updata = superAdminDataService.GetCompanyById((int)Id);
			}
			List<masterState> state = superAdminDataService.GetAllState();
            ViewBag.state = state;
			return View();
			 
		}

		public ActionResult GetCityByState(int id)
		{
			var data = superAdminDataService.GetCityByState(id);
			return Json(new { status = true, data = data },JsonRequestBehavior.AllowGet);

        }

		[HttpPost]
		public JsonResult CompanyRegistration( SuperAdminModel model)
		{

			try
			{
				if (model.School_Logo1 != null)	
				{
					model.School_Logo = superAdminDataService.UploadImageToServer(model.School_Logo1);
				}
				if (model.Authorized_Sign1 != null)
				{
					model.Authorized_Sign = superAdminDataService.UploadImageToServer(model.Authorized_Sign1);
				}
				
				
					bool res=superAdminDataService.InsertCompanyRegistration(model);
					if (res)
					{
					return Json(new { success = true, message =  model.Id>0? "Update Successfully":"Company registered successfully!" });
					}
					else
					{
                        return Json(new { success = false, message = "Something went wrong or maybe email or mobile number already exists." });
                    }
				
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Error: " + ex.Message });
			}
		}

		public ActionResult CompanyList()
		{
			var company = superAdminDataService.GetAllCompanyReg();
			return View(company);
		}

		[HttpPost]
		public JsonResult DeleteCompanyReg(int Id)
		{
			string resultMessage = superAdminDataService.deleteCompanyReg(Id: Id);

			if (resultMessage == "Success")
			{
				return Json(new { success = true, message = "Data deleted successfully!" });
			}
			else
			{
				return Json(new { success = false, message = resultMessage });
			}
		}
		public ActionResult Admin()
		{
			return View();
		}
		public ActionResult CreateAdmin(int? id)
		{
			var data = superAdminDataService.GetAllAdmin();
			ViewBag.company = superAdminDataService.GetAllCompanyReg();

            return View(data);
		}

		[HttpPost]
		public JsonResult CreateAdmin(CreateAdmin model)
		{

			try
			{
				
				
				if (ModelState.IsValid)
				{

					bool res=superAdminDataService.InsertAdmin(model);
					if (res)
					{

					return Json(new { success = true, message = "Admin registered successfully!" });
					}
                    return Json(new { success = false, message = "Invalid input data maybe emailId or password are already exists." });
                }
				else
				{
					return Json(new { success = false, message = "Invalid input data." });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Error: " + ex.Message });
			}
		}

		[HttpPost]
		public JsonResult DeleteAdmin(int Id)
		{
			string resultMessage = superAdminDataService.deleteAdmin(Id: Id);

			if (resultMessage == "Success")
			{
				return Json(new { success = true, message = "Data deleted successfully!" });
			}
			else
			{
				return Json(new { success = false, message = resultMessage });
			}
		}

		[HttpPost]
		public JsonResult UpdateAdmin(CreateAdmin data)
		{
			if (!string.IsNullOrWhiteSpace(data.Name))
			{
				bool res = superAdminDataService.UpdateAdmin(data);
				if (res)
				{
					return Json(new { success = true, message = "  updated successfully." });
				}
				else
				{
					return Json(new { success = false, message = "Something went wrong." });

				}
			}
			else
			{
				return Json(new { success = false, message = "Please fill out all required fields." });
			}

		}

		public ActionResult GetAdminById(int id)
		{
			var data = superAdminDataService.GetAdminById(id);
			return Json(new { status = true, data = data }, JsonRequestBehavior.AllowGet);
		}


	}
}