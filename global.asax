<%@ Import Namespace="System.Data.SqlClient"%>
<%@ Import Namespace="NFN"%>
<%@ Import Namespace="System.Data" %>
<script language="C#" runat="server">

  void Session_OnStart() {
    Session.Timeout = 20;

    Session["CMS"] = new CMS(Application);
    
    try {
      String ip = Request.UserHostAddress;
      String dns = NFN.Util.GetReverseDNS(ip, 10000);
      if (!DB.RowExists("select * from sessions where sessionid='" + Session.SessionID + "'")) {
        String atime = DateTime.Now.ToString(CMS.SiteSetting("dateTimeFormat"));
        DB.ExecSql("insert into sessions (ip, dns, referrer, useragent, connecttime, lastactiontime, sessionid) values('" + ip + "', '" + dns + "', '" + Request.UrlReferrer + "', '" + Request.UserAgent + "', '" + atime + "', '" + atime + "', '" + Session.SessionID + "')");
      }
//      DB.ExecSql("delete from sessions where DATEDIFF(minute, lastactiontime, GETDATE()) > 60");
    }
    catch {}
  }

  void Session_OnEnd() {
    try { DB.ExecSql("delete from sessions where sessionid='" + Session.SessionID + "'"); }
    catch {}

    CMS Cms = (CMS)Session["CMS"];
    Cms.LogOut(Session.SessionID);
  }

</script>
