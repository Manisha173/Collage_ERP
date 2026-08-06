using College_ERP.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static College_ERP.Models.StudentServices.main;

namespace College_ERP.ViewModels
{
    public class ParentTimeTableViewModel
    {
        public List<TodaySchedulesModel> TodaySchedule { get; set; }

        public List<timetableshowModel> WeeklyTimeTable { get; set; }
    }
}