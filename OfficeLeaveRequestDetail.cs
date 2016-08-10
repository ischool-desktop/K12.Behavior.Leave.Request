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
using FISCA.Presentation.Controls;

namespace K12.Behavior.Leave.Request
{
    public partial class OfficeLeaveRequestDetail : BaseForm
    {

        //專門來放 假別的比較表，幫助縮寫
        Dictionary<String, String> LeaveReference = new Dictionary<string, string>();

        //存放學校系統課程表
        List<PeriodMappingInfo> PeriodList = new List<PeriodMappingInfo>();

        List<String> StuIDList = new List<string>(); 


       internal OfficeLeaveRequestDetail(LeaveRequestRecord item)
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

           // 取得課表設定
           PeriodList = K12.Data.PeriodMapping.SelectAll();
           
           foreach(var element in item.Content.Students)
           {
           StuIDList.Add(element.StudentID);           
           }

           List<StudentRecord> StuRecList = K12.Data.Student.SelectByIDs(StuIDList);


            //事由
            labelX5.Text = item.Content.Reason;

            labelX5.TextAlignment = (System.Drawing.StringAlignment)ContentAlignment.TopLeft;

            //假單編碼
            textBoxX2.Text = item.key;

           //核可狀態
            labelX7.Text = item.Approved.HasValue && item.Approved.Value ? "已核准" : "";
                       
           // 填寫學生清單
            foreach (var Rec in StuRecList) 
            {                              
                dataGridViewX2.Rows.Add(Rec.StudentNumber,Rec.Class.GradeYear,Rec.Class.Name,Rec.SeatNo,Rec.Name);           
            }
           //動態新增課表Col
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

                col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
                col.MinimumWidth = PeriodList[ii].Name.Length * 25;

                dataGridViewX1.Columns.Add(col);
            }

           //填寫請假節次
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
    }
}
