<%-- $Date: 2009-04-20 11:16:14 +0200 (må, 20 apr 2009) $    $Revision: 4823 $ --%>
<%@ Page language="c#" explicit="true" strict="true" %>
<script language="c#" runat="server">
  void Page_Load(object src, EventArgs e)   {
    String strRequest = Request.QueryString["file"];
    if (strRequest.Length > 0) {
      String path = Server.MapPath("~/" + strRequest);
      System.IO.FileInfo file = new System.IO.FileInfo(path);
      if (file.Exists) {
        Response.Clear();
        Response.AddHeader("Content-Disposition", "attachment; filename=" + Server.UrlEncode(file.Name));
        Response.AddHeader("Content-Length", file.Length.ToString());
        Response.ContentType = "application/octet-stream";
        Response.WriteFile(file.FullName);
        Response.End();
      }
      else
        Response.Write("This file does not exist.");
    }
    else
      Response.Write("Please provide a file to download.");
  }
</script>

