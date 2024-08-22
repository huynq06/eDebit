using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Utils
{
    public static class ControlClass
    {
        public static void SetPostBackUrlLinkControl(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                string abc = c.ID;
                if (c.Controls.Count > 0)
                {
                    SetPostBackUrlLinkControl(c);
                }
                else
                {
                    switch (c.GetType().ToString())
                    {
                        case "System.Web.UI.WebControls.LinkButton":
                            ((LinkButton)c).PostBackUrl = c.ID.Replace("lbt", "/Admin/") + ".aspx";
                            break;
                    }
                }
            }
        }
        //reset giá trị các control
        public static void ResetControlValues(Control parent)
        {
            //duyệt qua tất cả Control trên trang
            foreach (Control c in parent.Controls)
            {
                string abc = c.ID;
                //tại sao?
                if (c.Controls.Count > 0)
                {
                    //kiểm tra loại Control
                    if (c.GetType().ToString() == "CKEditor.NET")
                    {
                        //((CKEditorControl)c).Text = "";
                        break;
                    }
                    else
                    {
                        // sử dụng đệ quy
                        ResetControlValues(c);
                    }
                }
                else // < 0
                {
                    // kiểm tra loại control
                    switch (c.GetType().ToString())
                    {
                        case "System.Web.UI.WebControls.TextBox":
                            ((TextBox)c).Text = "";
                            break;
                        case "System.Web.UI.WebControls.CheckBox":
                            ((CheckBox)c).Checked = false;
                            break;
                        case "System.Web.UI.WebControls.RadioButton":
                            ((RadioButton)c).Checked = false;
                            break;
                        case "System.Web.UI.WebControls.Image":
                            ((Image)c).ImageUrl = null;
                            ((Image)c).Width = 0;
                            break;
                        case "CKEditor.NET":
                            //((CKEditorControl)c).Text = "";
                            break;
                        case "System.Web.UI.WebControls.DropDownList":
                            ((DropDownList)c).SelectedIndex = 0;
                            break;
                        case "System.Web.UI.WebControls.HiddenField":
                            ((HiddenField)c).Value = "";
                            break;
                    }
                }
            }
        }

        public static void SetSelectedValue(this DropDownList ddl, string value)
        {
            if (ddl.Items.FindByValue(value) != null)
                ddl.SelectedValue = value;
            else
                ddl.SelectedIndex = 0;
        }

        public static string GetSelectedText(this DropDownList ddl)
        {
            ListItem item = ddl.SelectedItem;
            if (item != null)
                return item.Text;
            return string.Empty;
        }
    }
}
