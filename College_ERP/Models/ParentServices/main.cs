using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace College_ERP.Models.ParentServices
{
    public class main
    {
        public class ParentProfileModel
        {
            public string FatherName { get; set; }
            public string FatherOfficeAddress { get; set; }
            public string FatherQualification { get; set; }
            public string FatherOccupation { get; set; }
            public string FatherNo { get; set; }
            public string FatherPhoto { get; set; }

            public string MotherName { get; set; }
            public string MotherOfficeAddress { get; set; }
            public string MotherQualification { get; set; }
            public string MotherOccupation { get; set; }
            public string MotherNo { get; set; }
            public string MotherPhoto { get; set; }

            public string ParentEmail { get; set; }
        }
        public class StudentModel
        {
            public string admissionstage { get; set; }
            public int studentid { get; set; }
            public string studentname { get; set; }
            public string className { get; set; }
            public string studentImage { get; set; }
            public int classId { get; set; }
            public int sectionId { get; set; }
        }
    }
}