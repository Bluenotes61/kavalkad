/* $Date: 2009-04-17 23:12:31 +0200 (fr, 17 apr 2009) $    $Revision: 4820 $ */
using System;
using System.Web.UI;
using System.Configuration;
using System.Data;
using NFN;
using AjaxPro;
using System.Net;
using System.Net.Mail;

public partial class ApproveUser : Page {

  bool defaultCredentials = false;

  private CMS Cms {
    get { return (CMS)Session["CMS"]; }
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    AjaxPro.Utility.RegisterTypeForAjax(typeof(ApproveUser), this);

    String guid = Request["guid"];
    try {
      DataSet ds = DB.GetDS("select * from users where deleted=0 and guid='" + guid + "'");
      if (DB.GetRowCount(ds) > 0) {
        okholder.Visible = true;
        errorholder.Visible = false;
        String un = DB.GetString(ds, 0, "username");
        username.Text = un;
        email.Text = DB.GetString(ds, 0, "email");
        int id = DB.GetInt(ds, 0, "id");
        bool approve = Request["ok"] == "Y";
        yes.Visible = approve;
        no.Visible = !approve;
        sendmail.Visible = approve;
        if (approve) {
          DB.ExecSql("update users set approved='Y' where id=" + id);
          String atxt = @"
            <img src='http://{0}/gfx/logo.gif' />
            <p>&nbsp;</p>
            <h1>Välkommen till Mincs interna sidor</h1>
            <p>Du kan nu logga in på <a href='http://{0}'>{0}</a> med användarnamnet <b>{1}</b></p>
            <p>Med vänliga hälsningar<br>Minc</p>";
          initialbody.Text = String.Format(atxt, Request.ServerVariables["HTTP_HOST"], un).Replace("[","{").Replace("]","}");
        }
        else {
          Cms.DeleteUser(id);
        }
      }
      else {
        okholder.Visible = false;
        errorholder.Visible = true;
      }
    }
    catch {
      okholder.Visible = false;
      errorholder.Visible = true;
    }
  }

  private SmtpClient client = null;
  private SmtpClient Client {
    get {
      if ( client == null) {
        client = new SmtpClient();
        if (defaultCredentials)
          client.Credentials = CredentialCache.DefaultNetworkCredentials;
      }
      return client;
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void SendMail(String subject, String body, String email) {

    MailMessage message = new MailMessage(new MailAddress("web@minc.se"), new MailAddress(email));
    message.Subject = subject;
    message.SubjectEncoding = System.Text.Encoding.UTF8;
    message.Body = "<body style='margin:20px'><link rel='stylesheet' type='text/css' href='http://" + Request.ServerVariables["HTTP_HOST"] + "/css/base.css'>" + body + "</body>";
    message.BodyEncoding =  System.Text.Encoding.UTF8;
    message.IsBodyHtml = true;

    Client.Send(message);

  }
}