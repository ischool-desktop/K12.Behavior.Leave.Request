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
    public partial class OfficeLeaveRequestViewer : BaseForm
    {
        // 存放LeaveRequestRecord用，以便傳入OfficeLeaveRequestDetail()
        List<LeaveRequestRecord> list_to_LeaveReqestDetail;

        public OfficeLeaveRequestViewer()
        {
            InitializeComponent();
        }
    
        // 搜尋
        private void buttonX1_Click_1(object sender, EventArgs e)
        {
            // 把舊的Rows刪光
            dgvResult.Rows.Clear();

            var studentHelper = new SmartSchool.Customization.Data.AccessHelper().StudentHelper;
            AccessHelper accessHelper = new AccessHelper();

            
            List<K12.Data.StudentRecord> StudentList = K12.Data.Student.SelectAll();

            //轉換學號、ID用的Dic
            Dictionary<String, String> StuNum_To_StuID = new Dictionary<string, string>();

            foreach (var StuRecord in StudentList)
            {
                if (!StuNum_To_StuID.ContainsKey(StuRecord.StudentNumber))
                {
                    StuNum_To_StuID.Add(StuRecord.StudentNumber, StuRecord.ID);
                }

            }

            if (dateTimeInput1.Value.Ticks == 0 || dateTimeInput2.Value.Ticks ==0)
            {
                MsgBox.Show("請選擇輸入時間區間");
                return;
            }

            //2016/8/9 穎驊註解， 原本的時間 * 10000 + 621355968000000000 為格林威治(+0)的時間，因應台灣是(+8)時區，所以必須
            //再補上 8*60*60*1000*10000 =288000000000 ticks(豪微秒?) 才是真正的時間
            long startDay = (dateTimeInput1.Value.Ticks - 621355968000000000 - 288000000000) / 10000;
            long endDay = (dateTimeInput2.Value.AddDays(1).Ticks - 621355968000000000 - 288000000000) / 10000;



            // 2016/8/8 父親節，穎驊改寫，選擇條件改為 ref_student_id 等於指定id 如此一來才不會選到公假單(公假單的ref_student_id 等於null)
            var list = accessHelper.Select<LeaveRequestRecord>("ref_teacher_id >0" + " AND uqid >=" + "'" + startDay + "'" + " AND uqid <= " + "'" + endDay + "'");

            //選擇所有一般假單，一般假單由學生填寫，不會有ref_teacher_id 
            //var list = accessHelper.Select<LeaveRequestRecord>("ref_student_id > 0");

            //選擇所有公假單，公假單只能由老師填寫，不會有ref_student_id
            //var list = accessHelper.Select<LeaveRequestRecord>("ref_teacher_id >0");

            list.Sort(delegate(LeaveRequestRecord lr1, LeaveRequestRecord lr2)
            {
                return lr2.key.CompareTo(lr1.key);
            });

            if (list.Count == 0)
            {
                MsgBox.Show("該時段內無公假單紀錄");
            }

            foreach (var item in list)
            {
               
                dgvResult.Rows.Add(                    
                    item.key,
                    item.Approved.HasValue && item.Approved.Value ? "已核准" : "",

                    //2016/8/9 穎驊註解， 原本系統的時間 * 10000 + 621355968000000000 為格林威治(+0)的時間，因應台灣是(+8)時區，所以必須
                    //再補上 8*60*60*1000*10000 =288000000000 ticks(豪微秒?) 才是真正的時間
                    //簡單來說，存在資料庫的都是格林威治標準時間
                    new DateTime(long.Parse(item.key) * 10000 + 621355968000000000 + 288000000000).ToString("yyyy/MM/dd HH:mm:ss")
                );
            }

            list_to_LeaveReqestDetail = list;
        }
    
        //  預先設定時間區間，從上禮拜一 到今天
        private void OfficeLeaveRequestViewer_Load(object sender, EventArgs e)
        {
            dateTimeInput1.Value = DateTime.Now.Date.AddDays(-7 - (int)DateTime.Now.DayOfWeek + 1);
            dateTimeInput2.Value = DateTime.Now.Date;
        }

        //公假單項目被點選，可看細節

        private void dgvResult_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Int32 selectedRowCount = dgvResult.CurrentCell.RowIndex;

            OfficeLeaveRequestDetail OLRD = new OfficeLeaveRequestDetail(list_to_LeaveReqestDetail[selectedRowCount]);

            OLRD.ShowDialog();
        }
    }


    }



