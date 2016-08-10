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
using K12.Data;

namespace K12.Behavior.Leave.Request
{
    public partial class LeaveRequestDetail : BaseForm
    {
        //專門來放 假別的比較表，幫助縮寫
        Dictionary<String, String> LeaveReference = new Dictionary<string, string>();

        //存放學校系統課程表
        List<PeriodMappingInfo> PeriodList = new List<PeriodMappingInfo>();

        internal LeaveRequestDetail(LeaveRequestRecord item)
        {
            InitializeComponent();

            LeaveReference.Add("曠課", "曠");
            LeaveReference.Add("事假", "事");
            LeaveReference.Add("病假", "病");
            LeaveReference.Add("喪假", "喪");
            LeaveReference.Add("公假", "公");
            LeaveReference.Add("婚假", "婚");
            LeaveReference.Add("產假", "產");
            LeaveReference.Add("遲到", "遲");
            LeaveReference.Add("生產假", "生");
            LeaveReference.Add("免修", "免");

            //取得學校系統課程表
            PeriodList = K12.Data.PeriodMapping.SelectAll();

            var studentHelper = new SmartSchool.Customization.Data.AccessHelper().StudentHelper;
            var stuRec = studentHelper.GetStudent("" + item.RefStudentID);

            //事由
            labelX5.Text = item.Content.Reason;

            labelX5.TextAlignment = (System.Drawing.StringAlignment)ContentAlignment.TopLeft;

            //假單編碼
            textBoxX2.Text = item.key;

            //學生
            labelX4.Text = "班級:" + stuRec.RefClass.ClassName + "座號:" + stuRec.SeatNo + "姓名:" + stuRec.StudentName;

            //核可狀態
            labelX7.Text = item.Approved.HasValue && item.Approved.Value ? "已核准" : "";

            //動態新增課程Col
            for (int ii = 0; ii < PeriodList.Count; ii++)
            {
                DataGridViewColumn col = new DataGridViewColumn();
                col.CellTemplate = new DataGridViewTextBoxCell();

                col.Name = PeriodList[ii].Name;
                col.Visible = true;

                //if (PeriodList[ii].Name == "早讀" || PeriodList[ii].Name == "升旗" || PeriodList[ii].Name == "午休")
                //{
                //    col.Width = 50;
                //}
                //else
                //{
                //    col.Width = 25;
                //}

                //自動符合欄寬
                col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
                
                //一個字寬25
                col.MinimumWidth = PeriodList[ii].Name.Length * 25;

                dataGridViewX1.Columns.Add(col);
            }
            // 填表
            foreach (var Date in item.Content.Dates)
            {
                dataGridViewX1.Rows.Add(
                Date.Date,
                Date.Periods[0].Absence == "" ? "" : LeaveReference[Date.Periods[0].Absence],
                Date.Periods[1].Absence == "" ? "" : LeaveReference[Date.Periods[1].Absence],
                Date.Periods[2].Absence == "" ? "" : LeaveReference[Date.Periods[2].Absence],
                Date.Periods[3].Absence == "" ? "" : LeaveReference[Date.Periods[3].Absence],
                Date.Periods[4].Absence == "" ? "" : LeaveReference[Date.Periods[4].Absence],
                Date.Periods[5].Absence == "" ? "" : LeaveReference[Date.Periods[5].Absence],
                Date.Periods[6].Absence == "" ? "" : LeaveReference[Date.Periods[6].Absence],
                Date.Periods[7].Absence == "" ? "" : LeaveReference[Date.Periods[7].Absence],
                Date.Periods[8].Absence == "" ? "" : LeaveReference[Date.Periods[8].Absence],
                Date.Periods[9].Absence == "" ? "" : LeaveReference[Date.Periods[9].Absence],
                Date.Periods[10].Absence == "" ? "" : LeaveReference[Date.Periods[10].Absence]
             );
            }
        }

        // 確認後關閉
        private void buttonX1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
