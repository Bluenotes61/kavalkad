/* $Date: 2010-02-12 00:32:36 +0100 (fr, 12 feb 2010) $    $Revision: 5940 $ */
using System;
using System.Web.UI;
using AjaxPro;
using NFN;

public partial class LoginPage : Page {

  public CMS Cms {
    get { return (CMS)Session["CMS"]; }
  }
  
  public String Url {
    get {
      return Util.GetBaseUrl();
    }
  }

  private String LogoName {
    get {
      String fname = "";
      if (System.IO.File.Exists(Server.MapPath("~/gfx/logo.png"))) fname = "/gfx/logo.png";
      else if (System.IO.File.Exists(Server.MapPath("~/gfx/logo.gif"))) fname = "/gfx/logo.gif";
      else if (System.IO.File.Exists(Server.MapPath("~/gfx/logo.jpg"))) fname = "/gfx/logo.jpg";
      if (fname.Length > 0) return Util.GetThumbnail(fname, 250, 250);
      else return "";
    }
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    AjaxPro.Utility.RegisterTypeForAjax(typeof(LoginPage));

    if (!IsPostBack) {
      if (LogoName.Length > 0) Logo.Src = LogoName;
      else Logo.Visible = false;
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String Login(String username, String password) {
    return Cms.LogIn(username, password, "Administrator");
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String RequestLogin(String username, String email) {
    String res = Cms.RequestPassword(username, email);
    String mess = "";
    if (res == "NoInput") mess = Cms.Translate("Ange användarnamn eller e-postadress") + ".";
    else if (res == "NoUser") mess = Cms.Translate("Ingen användare existerar med användarnamnet") + " " + username;
    else if (res == "NoMailForUser") mess = Cms.Translate("Användare") + " " + username + " " + Cms.Translate("har ingen registrerad e-postadress");
    else if (res == "NoUserWithMail") mess = Cms.Translate("Ingen användare existerar med e-postadressen") + " " + email;
    else if (res.StartsWith("Error")) mess = res;
    else mess = "ok" + mess;
    return mess;
  }
}