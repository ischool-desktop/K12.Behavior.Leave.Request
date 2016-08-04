using FISCA.Presentation.Controls;
using FISCA.UDT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace K12.Behavior.Leave.Request
{
    public partial class LeaveRequestViewer : BaseForm
    {
        public LeaveRequestViewer()
        {
            InitializeComponent();
        }

        private void LeaveRegistrationViewer_Load(object sender, EventArgs e)
        {
            var studentHelper = new SmartSchool.Customization.Data.AccessHelper().StudentHelper;
            AccessHelper accessHelper = new AccessHelper();
            var list = accessHelper.Select<LeaveRequestRecord>("ref_student_id > 0");
            list.Sort(delegate(LeaveRequestRecord lr1, LeaveRequestRecord lr2)
            {
                return lr2.key.CompareTo(lr1.key);
            });

            foreach (var item in list)
            {
                var stuRec = studentHelper.GetStudent("" + item.RefStudentID);
                dgvResult.Rows.Add(
                    stuRec.RefClass == null ? "" : stuRec.RefClass.ClassName,
                    stuRec.SeatNo,
                    stuRec.StudentNumber,
                    stuRec.StudentName,
                    item.key,
                    item.Approved.HasValue && item.Approved.Value ? "已核准" : "",
                    new DateTime(long.Parse(item.key) * 10000 + 621355968000000000).ToString("yyyy/MM/dd HH:mm:ss")
                );
            }
        }
    }
}
