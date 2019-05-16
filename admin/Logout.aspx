<%-- $Date: 2010-10-07 12:13:34 +0200 (to, 07 okt 2010) $    $Revision: 7023 $ --%>
<%@ Import Namespace="NFN"%>
<%@ Page Language="C#" %>
<script language="C#" runat="server">
  public void Page_Load(Object sender, EventArgs E) {
    CMS Cms = (CMS)Session["CMS"];
    Cms.LogOut(Page.Session.SessionID);
    Response.Redirect("~/");
  }
</script>