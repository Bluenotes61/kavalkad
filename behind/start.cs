using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Net.Mail;
using System.Text;
using NFN;
using NFN.Controls;
using AjaxPro;


public partial class StartPage : BasePage {

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    AjaxPro.Utility.RegisterTypeForAjax(typeof(StartPage));
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String Login(String uname, String pwd) {
    return Cms.LogIn(uname, pwd);
  }

}

