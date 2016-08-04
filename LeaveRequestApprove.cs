using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using FISCA.UDT;
using FISCA.DSAUtil;
using System.Xml;

namespace K12.Behavior.Leave.Request
{
    public partial class LeaveRequestApprove : BaseForm
    {
        private int _ApproveCount = 0;
        private int _SuccessCount = 0;
        private int _FailedCount = 0;

        public LeaveRequestApprove()
        {
            InitializeComponent();
        }

        private void buttonX1_Click(object sender_, EventArgs e_)
        {
            var uqid = textBoxX1.Text.Trim();

            if (uqid == "") return;

            this.dgvResult.Rows.Insert(0, uqid, "處理中...");
            var targetRow = dgvResult.Rows[0];

            var result = "";
            BackgroundWorker bkw = new BackgroundWorker();

            bkw.RunWorkerCompleted += delegate
            {
                targetRow.Cells[1].Value = result;
                if (result.EndsWith("假單已核可。"))
                    _SuccessCount++;
                else
                {
                    _FailedCount++;
                    targetRow.Cells[0].ErrorText = "!";
                }
                labelX2.Text = "執行紀錄："
                    + (_ApproveCount > _SuccessCount + _FailedCount ? "處理中：" + (_ApproveCount - _SuccessCount - _FailedCount) : "")
                    + (_SuccessCount > 0 ? " 已核可：" + _SuccessCount : "")
                    + (_FailedCount > 0 ? " 發生錯誤：" + _FailedCount : "");
            };
            bkw.DoWork += delegate
            {
                try
                {
                    AccessHelper accessHelper = new AccessHelper();
                    var list = accessHelper.Select<LeaveRequestRecord>("uqid='" + uqid + "'");
                    if (list.Count == 0)
                    {
                        result = "查無此假單。";
                        return;
                    }
                    foreach (var lrRec in list)
                    {
                        var lrHint = (lrRec.RefTeacherID == null ? ("學生：" + K12.Data.Student.SelectByID("" + lrRec.RefStudentID).Name+"，") : "公假單，");

                        Dictionary<string, XmlElement> dicCurrentStudentAttendance = new Dictionary<string, XmlElement>();
                        #region 取得缺曠紀錄
                        #region 整理學生ID
                        List<string> idList = new List<string>();
                        foreach (var lrcStudent in lrRec.Content.Students)
                        {
                            idList.Add(lrcStudent.StudentID);
                        }
                        #endregion
                        DSXmlHelper helper = new DSXmlHelper("Request");
                        helper.AddElement("Field");
                        helper.AddElement("Field", "All");
                        helper.AddElement("Condition");
                        foreach (string id in idList)
                        {
                            helper.AddElement("Condition", "RefStudentID", id);
                        }
                        //if ( schoolYear > 0 )
                        //    helper.AddElement("Condition", "SchoolYear", schoolYear.ToString());
                        //if ( semester > 0 )
                        //    helper.AddElement("Condition", "Semester", semester.ToString());
                        helper.AddElement("Order");
                        helper.AddElement("Order", "OccurDate", "desc");

                        foreach (XmlElement attendanceElement in FISCA.Authentication.DSAServices.CallService("SmartSchool.Student.Attendance.GetAttendance", new DSRequest(helper)).GetContent().GetElements("Attendance"))
                        {
                            XmlElement var = attendanceElement;
                            DateTime occurdate;
                            DateTime.TryParse(var.SelectSingleNode("OccurDate").InnerText, out occurdate);
                            string studentID = var.SelectSingleNode("RefStudentID").InnerText;

                            var key = studentID + "^^" + occurdate.ToString("yyyy/MM/dd");
                            dicCurrentStudentAttendance.Add(key, attendanceElement);
                            //int schoolyear = 0;
                            //int.TryParse(var.SelectSingleNode("SchoolYear").InnerText, out schoolyear);
                            //int semester2 = 0;
                            //int.TryParse(var.SelectSingleNode("Semester").InnerText, out semester2);

                            //foreach (XmlElement element in var.SelectNodes("Detail/Attendance/Period"))
                            //{
                            //    string period = element.InnerText;
                            //    string periodtype = element.GetAttribute("AttendanceType");
                            //    string attendance = element.GetAttribute("AbsenceType");

                            //    //if ( !periodList.Contains(period) || !absenceList.Contains(attendance) )
                            //    //    continue;

                            //    //Attendance_islo attendanceInfo = new Attendance_islo(schoolyear, semester2, occurdate, period, periodtype, attendance, var);

                            //    //if (!studentAttendanceInfo.ContainsKey(studentID))
                            //    //    studentAttendanceInfo.Add(studentID, new List<SmartSchool.Customization.Data.StudentExtension.AttendanceInfo>());
                            //    //studentAttendanceInfo[studentID].Add(attendanceInfo);
                            //}
                        }
                        #endregion
                        Dictionary<StringBuilder, string> studentLogMsg = new Dictionary<StringBuilder, string>();
                        var hasInsert = false;
                        DSXmlHelper InsertHelper = new DSXmlHelper("InsertRequest");
                        var hasUpdate = false;
                        DSXmlHelper updateHelper = new DSXmlHelper("UpdateRequest");
                        #region 彙整寫入資料
                        XmlDocument doc = new XmlDocument();
                        foreach (var lrcStudent in lrRec.Content.Students)
                        {
                            StringBuilder appLogMsg = new StringBuilder();
                            appLogMsg.Append("假單編號：");
                            appLogMsg.AppendLine(lrRec.key);
                            appLogMsg.Append("事由：");
                            appLogMsg.AppendLine(lrRec.Content.Reason);
                            foreach (var lrcDate in lrRec.Content.Dates)
                            {
                                appLogMsg.Append("日期：");
                                appLogMsg.AppendLine(DateTime.Parse(lrcDate.Date).ToString("yyyy/MM/dd"));

                                var key = lrcStudent.StudentID + "^^" + DateTime.Parse(lrcDate.Date).ToString("yyyy/MM/dd");

                                if (dicCurrentStudentAttendance.ContainsKey(key))
                                {
                                    var currentAttendanceElement = dicCurrentStudentAttendance[key];
                                    #region 更新
                                    foreach (var lrcPeriod in lrcDate.Periods)
                                    {
                                        if (lrcPeriod.Absence != "")
                                        {
                                            bool hasOverride = false;
                                            var attendanceEle = currentAttendanceElement.SelectSingleNode("Detail/Attendance") as XmlElement;
                                            if (attendanceEle != null)
                                            {
                                                foreach (XmlElement attendItem in currentAttendanceElement.SelectNodes("Detail/Attendance/Period"))
                                                {
                                                    string period = attendItem.InnerText;
                                                    string attendance = attendItem.GetAttribute("AbsenceType");
                                                    if (period == lrcPeriod.Period)
                                                    {
                                                        appLogMsg.AppendLine(string.Format("\t節次 {0}：改為{1}(原為{2})", lrcPeriod.Period, lrcPeriod.Absence, attendItem.GetAttribute("AbsenceType")));

                                                        hasOverride = true;
                                                        attendItem.SetAttribute("AbsenceType", lrcPeriod.Absence);
                                                    }
                                                }
                                                if (!hasOverride)
                                                {
                                                    appLogMsg.AppendLine(string.Format("\t節次 {0}：設為{1}", lrcPeriod.Period, lrcPeriod.Absence));
                                                    XmlElement attendItem = currentAttendanceElement.OwnerDocument.CreateElement("Period");
                                                    attendItem.InnerText = lrcPeriod.Period;
                                                    attendItem.SetAttribute("AbsenceType", lrcPeriod.Absence);
                                                    attendanceEle.AppendChild(attendItem);
                                                }
                                                updateHelper.AddElement("Attendance");
                                                updateHelper.AddElement("Attendance", "Field");
                                                updateHelper.AddElement("Attendance/Field", "Detail", attendanceEle.OuterXml, true);
                                                updateHelper.AddElement("Attendance", "Condition");
                                                updateHelper.AddElement("Attendance/Condition", "ID", currentAttendanceElement.GetAttribute("ID"));
                                                hasUpdate = true;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region 新增
                                    XmlElement attendanceEle = doc.CreateElement("Attendance");
                                    foreach (var lrcPeriod in lrcDate.Periods)
                                    {
                                        if (lrcPeriod.Absence != "")
                                        {
                                            appLogMsg.AppendLine(string.Format("\t節次 {0}：設為{1}", lrcPeriod.Period, lrcPeriod.Absence));
                                            XmlElement attendItem = doc.CreateElement("Period");
                                            attendItem.InnerText = lrcPeriod.Period;
                                            attendItem.SetAttribute("AbsenceType", lrcPeriod.Absence);
                                            attendanceEle.AppendChild(attendItem);
                                        }
                                    }

                                    #region 自動判斷學年度
                                    string schoolYear = K12.Data.School.DefaultSchoolYear;
                                    string semester = K12.Data.School.DefaultSemester;

                                    var occurDate = DateTime.Parse(lrcDate.Date);
                                    int syBase = occurDate.Year - 1911;
                                    if (occurDate.Month < 8 && occurDate.Month >= 2)
                                    {
                                        schoolYear = (syBase - 1).ToString();
                                        semester = "2";
                                    }
                                    else
                                    {
                                        if (occurDate.Month < 2)
                                        {
                                            schoolYear = (syBase - 1).ToString();
                                        }
                                        else
                                        {
                                            schoolYear = (syBase).ToString();
                                        }
                                        semester = "1";
                                    }
                                    #endregion
                                    InsertHelper.AddElement("Attendance");
                                    InsertHelper.AddElement("Attendance", "Field");
                                    InsertHelper.AddElement("Attendance/Field", "RefStudentID", lrcStudent.StudentID);
                                    InsertHelper.AddElement("Attendance/Field", "SchoolYear", "" + schoolYear);
                                    InsertHelper.AddElement("Attendance/Field", "Semester", "" + semester);
                                    InsertHelper.AddElement("Attendance/Field", "OccurDate", DateTime.Parse(lrcDate.Date).ToString("yyyy/MM/dd"));
                                    InsertHelper.AddElement("Attendance/Field", "Detail", attendanceEle.OuterXml, true);
                                    hasInsert = true;
                                    #endregion
                                }
                            }
                            studentLogMsg.Add(appLogMsg, lrcStudent.StudentID);
                        }
                        #endregion
                        #region 寫入資料
                        if (hasInsert)
                        {
                            //Log_sb.AppendLine(GetString(InsertHelper, "新增"));
                            FISCA.Authentication.DSAServices.CallService("SmartSchool.Student.Attendance.Insert", new DSRequest(InsertHelper));
                        }
                        if (hasUpdate)
                        {
                            //Log_sb.AppendLine(GetString(updateHelper, "更新"));
                            FISCA.Authentication.DSAServices.CallService("SmartSchool.Student.Attendance.Update", new DSRequest(updateHelper));
                        }
                        if (hasUpdate || hasInsert)
                        {
                            foreach (var log in studentLogMsg.Keys)
                            {
                                FISCA.LogAgent.ApplicationLog.Log("線上請假系統", "假單核可", "student", studentLogMsg[log], log.ToString());
                            }
                        }
                        result = lrHint+ "假單已核可。";
                        lrRec.Approved = true;
                        lrRec.Save();
                        #endregion
                    }
                }
                catch (Exception exc)
                {
                    result = "執行發生錯誤：" + exc.Message;
                }
            };
            _ApproveCount++;
            labelX2.Text = "執行紀錄："
                + (_ApproveCount > _SuccessCount + _FailedCount ? "處理中：" + (_ApproveCount - _SuccessCount - _FailedCount) : "")
                + (_SuccessCount > 0 ? " 已核可：" + _SuccessCount : "")
                + (_FailedCount > 0 ? " 發生錯誤：" + _FailedCount : "");
            bkw.RunWorkerAsync();
            this.textBoxX1.Text = "";
            this.textBoxX1.SelectAll();
        }


        private void textBoxX1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonX1_Click(null, null);
            }
        }
    }
}
