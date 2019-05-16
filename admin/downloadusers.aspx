<%-- $Date: 2009-04-20 11:16:14 +0200 (må, 20 apr 2009) $    $Revision: 4823 $ --%>
<%@ Page language="c#" explicit="true" strict="true" %>
<%@ Import Namespace="NFN" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.IO" %>


<script language="c#" runat="server">

  protected CMS Cms {
    get { return (CMS)Page.Session["CMS"]; }
  }

  private void AddCell(HtmlTableRow row, String value) {
    HtmlTableCell cell = new HtmlTableCell();
    cell.VAlign = "Top";
    cell.Controls.Add(new LiteralControl(value));
    row.Cells.Add(cell);
  }

  protected HtmlTable GetTable(String ids) {
    HtmlTable ht = new HtmlTable();
    ht.Border = 1;

    HtmlTableRow headrow = new HtmlTableRow();
    AddCell(headrow, "<b>Id</b>");
    AddCell(headrow, "<b>Usertype</b>");
    AddCell(headrow, "<b>Username</b>");
    AddCell(headrow, "<b>Password</b>");
    AddCell(headrow, "<b>E-mail address</b>");
    AddCell(headrow, "<b>Description</b>");
    AddCell(headrow, "<b>Active</b>");
    AddCell(headrow, "<b>Roles</b>");
    DataSet dsfields = DB.GetDS("select * from userattribfields order by orderno");
    for (int i=0; i < DB.GetRowCount(dsfields); i++)
      AddCell(headrow, "<b>" + DB.GetString(dsfields, i, "DisplayName") + "</b>");
    ht.Rows.Add(headrow);

    String[] idarr = ids.Split(',');
    for (int i=0; i < idarr.Length; i++) {
      DataSet ds = DB.GetDS("select * from users where id=" + idarr[i]);
      String userid = DB.GetString(ds, 0, "id");
      HtmlTableRow row = new HtmlTableRow();
      AddCell(row, userid);
      AddCell(row, DB.GetString(ds, 0, "usertype"));
      AddCell(row, DB.GetString(ds, 0, "username"));
      AddCell(row, DB.GetString(ds, 0, "password"));
      AddCell(row, DB.GetString(ds, 0, "email"));
      AddCell(row, DB.GetString(ds, 0, "description"));
      AddCell(row, DB.GetString(ds, 0, "approved"));

      String roles = "";
      DataSet ds2 = DB.GetDS("select role from viewpermissions where itemtype='User' and id='" + userid + "'");
      for (int j=0; j < DB.GetRowCount(ds2); j++) {
        if (roles.Length > 0) roles += "\n";
        roles += DB.GetString(ds2, j, "role");
      }
      AddCell(row, roles);

      for (int j=0; j < DB.GetRowCount(dsfields); j++) {
        String val = DB.GetString("select svalue from userattrib where userid=" + userid + " and fieldid=" + DB.GetInt(dsfields, j, "id"), "svalue");
        val = val.Replace("<br />","\n");
        AddCell(row, val);
      }
      ht.Rows.Add(row);

    }
    return ht;
  }

  protected void Page_Load(object src, EventArgs e) {
    if (!Cms.User.IsAdmin) Response.Redirect("/");

    this.ResponseEncoding = "iso-8859-1";

    HtmlTable ht = GetTable(Request["ids"]);

    Response.Clear();
    Response.AddHeader("content-disposition", "attachment;filename=users.xls");
    Response.Charset = "";
    //Response.Cache.SetCacheability(HttpCacheability.NoCache);
    Response.ContentType = "application/vnd.xls";

    StringWriter sw = new StringWriter();
    HtmlTextWriter hw = new HtmlTextWriter(sw);
    ht.RenderControl(hw);

    Response.Write(sw.ToString());
    Response.End();
  }

</script>
