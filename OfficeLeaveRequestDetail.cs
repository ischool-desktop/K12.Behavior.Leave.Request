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
        Dictionary<String, String> _LeaveReference = new Dictionary<string, string>();

        //存放學校系統課程表
        List<PeriodMappingInfo> _PeriodList = new List<PeriodMappingInfo>();

        List<String> StuIDList = new List<string>();


        internal OfficeLeaveRequestDetail(LeaveRequestRecord item)
        {
            InitializeComponent();

            // 取得假別清單
            foreach (var absenceRec in K12.Data.AbsenceMapping.SelectAll())
            {
                _LeaveReference.Add(absenceRec.Name, absenceRec.Abbreviation);
            }

            // 取得課表設定
            _PeriodList = K12.Data.PeriodMapping.SelectAll();

            foreach (var element in item.Content.Students)
            {
                StuIDList.Add(element.StudentID);
            }

            List<StudentRecord> StuRecList = K12.Data.Student.SelectByIDs(StuIDList);


            //事由
            labelX5.Text = item.Content.Reason;

            //假單編碼
            textBoxX2.Text = item.key;

            //核可狀態
            labelX7.Text = item.Approved.HasValue && item.Approved.Value ? "已進入系統" : "";

            // 填寫學生清單
            foreach (var stuRec in StuRecList)
            {
                dataGridViewX2.Rows.Add(stuRec.StudentNumber, stuRec.Class != null ? stuRec.Class.GradeYear : null, stuRec.Class != null ? stuRec.Class.Name : "", stuRec.Class != null ? stuRec.SeatNo : null, stuRec.Name);
            }
            //動態新增課表Col
            for (int ii = 0; ii < _PeriodList.Count; ii++)
            {
                DataGridViewColumn col = new DataGridViewColumn();
                col.CellTemplate = new DataGridViewTextBoxCell();

                col.Name = _PeriodList[ii].Name;
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
                col.MinimumWidth = _PeriodList[ii].Name.Length * 25;

                dataGridViewX1.Columns.Add(col);
            }

            //填寫請假節次
            foreach (var date in item.Content.Dates)
            {
                object[] values = new object[_PeriodList.Count + 1];
                values[0] = date.Date;

                Dictionary<string, string> dicAbsence = new Dictionary<string, string>();

                foreach (var p in date.Periods)
                {
                    if (p.Absence != "")
                        dicAbsence.Add(p.Period, p.Absence);
                }

                for (int i = 0; i < _PeriodList.Count; i++)
                {
                    values[i + 1] = dicAbsence.ContainsKey(_PeriodList[i].Name) ? (_LeaveReference.ContainsKey(dicAbsence[_PeriodList[i].Name]) ? _LeaveReference[dicAbsence[_PeriodList[i].Name]] : "!?") : "";
                }

                dataGridViewX1.Rows.Add(values);
            }
        }
    }
}
