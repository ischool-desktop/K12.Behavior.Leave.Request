using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using K12.Data;
using K12.Data.Configuration;
using Campus.Configuration;
using System.Xml;
using System.Xml.Linq;
using FISCA.DSAUtil;
using Framework.Feature;
using Framework;

namespace K12.Behavior.Leave.Request
{
    public partial class LeaveConfigSetting : BaseForm
    {

        //設定
        Campus.Configuration.ConfigData cd;

        private Dictionary<string, bool> AbsenceWasAllow = new Dictionary<string, bool>();

        private List<string> RequestSignTitle_List = new List<string>();

        private List<string> AnnualSignTitle_List = new List<string>();




        public LeaveConfigSetting()
        {
            InitializeComponent();

        }

        private void LeaveConfigSetting_Load(object sender, EventArgs e)
        {


            Campus.Configuration.ConfigData absence1 = Campus.Configuration.Config.App["ischool.leave.config"];
            XElement xmlabs1 = XElement.Parse(absence1.PreviousData.OuterXml);
            
            foreach (XElement abs in xmlabs1.Elements("AllowAbsence"))
            {
                AbsenceWasAllow.Add(abs.Value, true);
            }
                 
            //顯示「假別對照表」於 ComboBox。
            Campus.Configuration.ConfigData absence2 = Campus.Configuration.Config.App["假別對照表"];
            XElement xmlabs2 = XElement.Parse(absence2.PreviousData.OuterXml);
            foreach (XElement abs in xmlabs2.Elements("Absence"))
            {
               
                var item = listViewEx1.Items.Add(abs.Attribute("Name").Value);

                if (AbsenceWasAllow.ContainsKey(abs.Attribute("Name").Value))
                {
                    item.Checked = true;
                }
                else 
                {
                    item.Checked = false;
                }

                comboBoxEx1.Items.Add(abs.Attribute("Name").Value);
                         
            }

            foreach (XElement abs in xmlabs1.Elements("AnnualAbsence"))
            {
                comboBoxEx1.SelectedItem = abs.Value;
            }


            foreach (XElement abs in xmlabs1.Elements("RequestSignTitle"))
            {
                RequestSignTitle_List.Add(abs.Value);
            }

            if (RequestSignTitle_List.Count != 0) 
            {
                textBoxX1.Text = RequestSignTitle_List[0] != null ? RequestSignTitle_List[0] : "";
                textBoxX2.Text = RequestSignTitle_List[1] != null ? RequestSignTitle_List[1] : "";
                textBoxX3.Text = RequestSignTitle_List[2] != null ? RequestSignTitle_List[2] : "";
                textBoxX4.Text = RequestSignTitle_List[3] != null ? RequestSignTitle_List[3] : "";
                textBoxX5.Text = RequestSignTitle_List[4] != null ? RequestSignTitle_List[4] : "";
                textBoxX6.Text = RequestSignTitle_List[5] != null ? RequestSignTitle_List[5] : "";
            
            }

            foreach (XElement abs in xmlabs1.Elements("AnnualSignTitle"))
            {
                AnnualSignTitle_List.Add(abs.Value);
            }

            if (AnnualSignTitle_List.Count != 0)
            {
                textBoxX7.Text = AnnualSignTitle_List[0] != null ? AnnualSignTitle_List[0] : "";
                textBoxX8.Text = AnnualSignTitle_List[1] != null ? AnnualSignTitle_List[1] : "";
                textBoxX9.Text = AnnualSignTitle_List[2] != null ? AnnualSignTitle_List[2] : "";
                textBoxX10.Text = AnnualSignTitle_List[3] != null ? AnnualSignTitle_List[3] : "";
                textBoxX11.Text = AnnualSignTitle_List[4] != null ? AnnualSignTitle_List[4] : "";
                

            }

        }


        //儲存
        private void buttonX1_Click(object sender, EventArgs e)
        {
            RequestSignTitle_List.Clear();

            RequestSignTitle_List.Add(textBoxX1.Text);
            RequestSignTitle_List.Add(textBoxX2.Text);
            RequestSignTitle_List.Add(textBoxX3.Text);
            RequestSignTitle_List.Add(textBoxX4.Text);
            RequestSignTitle_List.Add(textBoxX5.Text);
            RequestSignTitle_List.Add(textBoxX6.Text);


            AnnualSignTitle_List.Clear();

            AnnualSignTitle_List.Add(textBoxX7.Text);
            AnnualSignTitle_List.Add(textBoxX8.Text);
            AnnualSignTitle_List.Add(textBoxX9.Text);
            AnnualSignTitle_List.Add(textBoxX10.Text);
            AnnualSignTitle_List.Add(textBoxX11.Text);


            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("AbsenceList");
            doc.AppendChild(root);

            foreach (ListViewItem item in listViewEx1.Items)
            {
                if (item.Checked)
                {
                    XmlElement AllowAbsence = doc.CreateElement("AllowAbsence");

                    AllowAbsence.InnerText = item.Text;

                    root.AppendChild(AllowAbsence);


                }
     
            }

            if(comboBoxEx1.SelectedItem!="")
            {
            
            XmlElement AnnualAbsence = doc.CreateElement("AnnualAbsence");

            AnnualAbsence.InnerText = comboBoxEx1.SelectedItem+"";

            root.AppendChild(AnnualAbsence);
            
            }

            foreach (string title in RequestSignTitle_List) 
            {
           
                    XmlElement RequestSignTitle = doc.CreateElement("RequestSignTitle");

                    RequestSignTitle.InnerText = title;

                    root.AppendChild(RequestSignTitle);                                
                            
            }

            foreach (string title in AnnualSignTitle_List)
            {

                XmlElement AnnualSignTitle = doc.CreateElement("AnnualSignTitle");

                AnnualSignTitle.InnerText = title;

                root.AppendChild(AnnualSignTitle);

            }


            DSXmlHelper helper = new DSXmlHelper("Lists");
            helper.AddElement("List");
            helper.AddElement("List", "Content", root.OuterXml, true);
            helper.AddElement("List", "Condition");
            helper.AddElement("List/Condition", "Name", "ischool.leave.config");



            //儲存
            try
            {
                Framework.Feature.Config.Update(new DSRequest(helper));
            }
            catch (Exception exception)
            {
                FISCA.Presentation.Controls.MsgBox.Show("更新失敗 :" + exception.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            //更新
            try
            {      
                Campus.Configuration.Config.App.Sync("ischool.leave.config");
            }
            catch
            {
                FISCA.Presentation.Controls.MsgBox.Show("資料重設失敗，新設定值將於下次啟動系統後生效!", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            FISCA.Presentation.Controls.MsgBox.Show("儲存成功!", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
            
   
        }

        //取消
        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
