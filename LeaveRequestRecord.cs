using FISCA.UDT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace K12.Behavior.Leave.Request
{
    [TableName("ischool.leave")]
    class LeaveRequestRecord : FISCA.UDT.ActiveRecord
    {
        [Field(Field = "approved")]
        public bool? Approved { get; set; }
        [Field(Field = "ref_student_id")]
        public int? RefStudentID { get; set; }
        [Field(Field = "ref_teacher_id")]
        public int? RefTeacherID { get; set; }
        [Field(Field = "school_year")]
        public int? SchoolYear { get; set; }
        [Field(Field = "semester")]
        public int? Semester { get; set; }
        [Field(Field = "uqid")]
        public string key { get; set; }

        public LRContent Content { get; private set; }

        private string _JSON = "";

        [Field(Field = "content")]
        public string JSON
        {
            get { return _JSON; }
            set
            {
                _JSON = value;
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(_JSON)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LRContent));
                    Content = (LRContent)serializer.ReadObject(ms);
                }
                if (Content.Students == null)
                {
                    Content.Students = new LRStudent[] { new LRStudent() { StudentID = "" + RefStudentID } };
                }
                if (Content.Dates == null)
                {
                    Content.Dates = new LRDate[0];
                }
            }
        }
    }

    [DataContract]
    class LRPeriod
    {
        [DataMember]
        public string Period { get; set; }
        [DataMember]
        public string Absence { get; set; }
    }

    [DataContract]
    class LRDate
    {
        [DataMember]
        public string Date { get; set; }
        [DataMember]
        public LRPeriod[] Periods { get; set; }
    }

    [DataContract]
    class LRStudent
    {
        [DataMember(Name = "id")]
        public string StudentID { get; set; }
    }

    [DataContract]
    class LRContent
    {
        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public LRStudent[] Students { get; set; }

        [DataMember]
        public LRDate[] Dates { get; set; }

        public LRContent()
        {
            Reason = "";
            Students = new LRStudent[0];
            Dates = new LRDate[0];
        }
    }
}
