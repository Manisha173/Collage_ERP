using College_ERP.Models.customeFilter;
using System.Web;
using System.Web.Mvc;

namespace College_ERP
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            
        }
    }
}
