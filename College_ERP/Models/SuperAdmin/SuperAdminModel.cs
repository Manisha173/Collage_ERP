using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace College_ERP.Models.SuperAdmin
{
	public class SuperAdminModel
	{
		public int Id { get; set; }
		public string SchoolName { get; set; }

		public string AuthorizedPersonName { get; set; }
		public long? MobileNo { get; set; }
		public long? LandLineNo { get; set; }
		public string EmailId { get; set; }
		public int State { get; set; }
		public string stateName { get; set; }
		public int City { get; set; }
		public string City_Name { get; set; }
		public string Website { get; set; }

		public string School_Logo { get; set; }
		public HttpPostedFileBase School_Logo1 { get; set; }

		public string Duration { get; set; }
		public string Authorized_Sign { get; set; }
		public HttpPostedFileBase Authorized_Sign1 { get; set; }

		public string School_Address { get; set; }

		public bool status { get; set; }
	}

	public class CreateAdmin
	{
		public string UserName { get; set; }
		public int Id { get; set; }

		public int CompanyId { get; set; }
		public string UserId { get; set; }
		public string Name { get; set; }
		public string CompanyName { get; set; }
		public string schoolAddress { get; set; }

		public long MobileNo { get; set; }
		public string EmailId { get; set; }
		public string AuthorizedPersonName { get; set; }
		public string AuthorizedPersonEmail { get; set; }
		public string AuthorizedPersonState { get; set; }
		public string AuthorizedPersonCity { get; set; }
		public string SchoolLogo { get; set; }
		public string AuthorizedSign { get; set; }
		public long AuthorizedPersonMobileNo { get; set; }
		public long AuthorizedPersonLandlineNo { get; set; }

		public HttpPostedFileBase Image { get; set; }
		public string Images { get; set; }
		public string Register_By { get; set; }
		public bool status { get; set; }
		public string Password { get; set; }
	}

	public class masterCity
	{
		public int city_Id { get; set; }
		public string City_Name { get; set; }
		public int state { get; set; }
		public int status { get; set; }
	}

	public class masterState
	{
		public int st_Id { get; set; }
		public string stateName { get; set; }
		public int status { get; set; }
	}

	public class commanData
	{
		public int Id { get; set; }
		public string Name { get; set; }
	
	}
    public class SuperAdminDashboardModel
    {
        public int totalcompanys { get; set; }
        public int totaladmins { get; set; }
    }
}