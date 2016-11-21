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
        private Dictionary<string, bool> _AbsenceWasAllow = new Dictionary<string, bool>();

        private List<string> _RequestSignTitleList = new List<string>();

        private List<string> _AnnualSignTitleList = new List<string>();




        public LeaveConfigSetting()
        {
            InitializeComponent();

        }

        private void LeaveConfigSetting_Load(object sender, EventArgs e)
        {
            Campus.Configuration.ConfigData absence1 = Campus.Configuration.Config.App["ischool.leave.config"];
            XElement xmlabs1;
            if (absence1.PreviousData != null)
                xmlabs1 = XElement.Parse(absence1.PreviousData.OuterXml);
            else
                xmlabs1 = XElement.Parse("<Config />");

            foreach (XElement abs in xmlabs1.Elements("AllowAbsence"))
            {
                _AbsenceWasAllow.Add(abs.Value, true);
            }

            //顯示「假別對照表」於 ComboBox。
            Campus.Configuration.ConfigData absence2 = Campus.Configuration.Config.App["假別對照表"];
            XElement xmlabs2 = XElement.Parse(absence2.PreviousData.OuterXml);
            foreach (XElement abs in xmlabs2.Elements("Absence"))
            {

                var item = listViewEx1.Items.Add(abs.Attribute("Name").Value);

                if (_AbsenceWasAllow.ContainsKey(abs.Attribute("Name").Value))
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
                comboBoxEx1.Text = abs.Value;
            }


            foreach (XElement abs in xmlabs1.Elements("RequestSignTitle"))
            {
                _RequestSignTitleList.Add(abs.Value);
            }

            foreach (XElement abs in xmlabs1.Elements("AnnualSignTitle"))
            {
                _AnnualSignTitleList.Add(abs.Value);
            }
            {

                int index = 0;
                foreach (var textBox in new DevComponents.DotNetBar.Controls.TextBoxX[] { textBoxX1, textBoxX2, textBoxX3, textBoxX4, textBoxX5, textBoxX6 })
                {
                    if (_RequestSignTitleList.Count > index)
                    {
                        textBox.Text = _RequestSignTitleList[index];
                    }
                    index++;
                }
            }
            {
                int index = 0;
                foreach (var textBox in new DevComponents.DotNetBar.Controls.TextBoxX[] { textBoxX7, textBoxX8, textBoxX9, textBoxX10, textBoxX11 })
                {
                    if (_AnnualSignTitleList.Count > index)
                    {
                        textBox.Text = _AnnualSignTitleList[index];
                    }
                    index++;
                }
            }
        }


        //儲存
        private void buttonX1_Click(object sender, EventArgs e)
        {
            _RequestSignTitleList.Clear();
            foreach (var textBox in new DevComponents.DotNetBar.Controls.TextBoxX[] { textBoxX1, textBoxX2, textBoxX3, textBoxX4, textBoxX5, textBoxX6 })
            {
                _RequestSignTitleList.Add(textBox.Text);
            }


            _AnnualSignTitleList.Clear();
            foreach (var textBox in new DevComponents.DotNetBar.Controls.TextBoxX[] { textBoxX7, textBoxX8, textBoxX9, textBoxX10, textBoxX11 })
            {
                _AnnualSignTitleList.Add(textBox.Text);
            }


            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Config");
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

            if (comboBoxEx1.Text != "")
            {
                XmlElement AnnualAbsence = doc.CreateElement("AnnualAbsence");
                AnnualAbsence.InnerText = comboBoxEx1.Text;
                root.AppendChild(AnnualAbsence);
            }

            foreach (string title in _RequestSignTitleList)
            {
                XmlElement RequestSignTitle = doc.CreateElement("RequestSignTitle");
                RequestSignTitle.InnerText = title;
                root.AppendChild(RequestSignTitle);
            }

            foreach (string title in _AnnualSignTitleList)
            {

                XmlElement AnnualSignTitle = doc.CreateElement("AnnualSignTitle");

                AnnualSignTitle.InnerText = title;

                root.AppendChild(AnnualSignTitle);

            }


            Campus.Configuration.ConfigData absence1 = Campus.Configuration.Config.App["ischool.leave.config"];
            if (absence1.PreviousData == null)
            {
                absence1.Save();
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

            FISCA.Presentation.Controls.MsgBox.Show("儲存成功!", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

            this.Close();
        }

        //取消
        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
