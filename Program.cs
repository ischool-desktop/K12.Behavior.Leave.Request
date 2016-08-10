using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Behavior.Leave.Request
{
    public static class Program
    {
        [FISCA.MainMethod]
        public static void Main()
        {
            {
                var btn = FISCA.Presentation.MotherForm.RibbonBarItems["學務作業", "線上請假"]["假單核准"];
                btn.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
                btn.Image = Properties.Resources.請假;
                btn.Click += delegate
                {
                    new LeaveRequestApprove().ShowDialog();
                };
            }
            {
                var btn = FISCA.Presentation.MotherForm.RibbonBarItems["學務作業", "線上請假"]["學生假單查詢"];
                btn.Click += delegate
                {
                    new LeaveRequestViewer().ShowDialog();
                };
            }
            {
                var btn = FISCA.Presentation.MotherForm.RibbonBarItems["學務作業", "線上請假"]["公假假單查詢"];
                btn.Click += delegate
                {
                    new OfficeLeaveRequestViewer().ShowDialog();
                    
                };
            }
        }
    }
}
