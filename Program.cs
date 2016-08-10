using FISCA.Permission;
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
                Catalog detail1 = RoleAclSource.Instance["學務作業"]["線上請假"];
                detail1.Add(new RibbonFeature("B74882A9-7C7C-411A-A7FB-5CF258CD52EF", "假單核准"));

                var btn = FISCA.Presentation.MotherForm.RibbonBarItems["學務作業", "線上請假"]["假單核准"];
                btn.Enable = FISCA.Permission.UserAcl.Current["B74882A9-7C7C-411A-A7FB-5CF258CD52EF"].Executable;
                btn.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
                btn.Image = Properties.Resources.請假;
                btn.Click += delegate
                {
                    new LeaveRequestApprove().ShowDialog();
                };
            }
            {
                Catalog detail1 = RoleAclSource.Instance["學務作業"]["線上請假"];
                detail1.Add(new RibbonFeature("5D915DBD-4B19-49E2-AE1D-A263FB056531", "學生假單查詢"));

                var btn = FISCA.Presentation.MotherForm.RibbonBarItems["學務作業", "線上請假"]["學生假單查詢"];
                btn.Enable = FISCA.Permission.UserAcl.Current["5D915DBD-4B19-49E2-AE1D-A263FB056531"].Executable;
                btn.Click += delegate
                {
                    new LeaveRequestViewer().ShowDialog();
                };
            }
            {
                Catalog detail1 = RoleAclSource.Instance["學務作業"]["線上請假"];
                detail1.Add(new RibbonFeature("0C3B1555-CFB7-492F-A5D9-074B384F9DAA", "公假假單查詢"));

                var btn = FISCA.Presentation.MotherForm.RibbonBarItems["學務作業", "線上請假"]["公假假單查詢"];
                btn.Enable = FISCA.Permission.UserAcl.Current["0C3B1555-CFB7-492F-A5D9-074B384F9DAA"].Executable;
                btn.Click += delegate
                {
                    new OfficeLeaveRequestViewer().ShowDialog();

                };
            }
        }
    }
}
