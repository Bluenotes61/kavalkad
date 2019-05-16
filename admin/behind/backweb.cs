/* $Date: 2010-11-22 14:32:20 +0100 (m√•, 22 nov 2010) $    $Revision: 7099 $ */
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using System.Collections;
using System.Text;
using obout_ASPTreeView_2_NET;
using NFN;
using AjaxPro;
using Obout.Grid;


public partial class Backweb : AdminBase {

  public override int LeftWidth {
    get { return 0; }
  }

  public override String BWPageName {
    get { return "Backweb"; }
  }

  protected override Grid ObItemGrid {
    get { return ItemGrid; }
  }

  protected override void OnLoad(EventArgs e) {
    ItemGrid.Columns[2].HeaderText = Translate("Id");
    ItemGrid.Columns[3].HeaderText = Translate("Namn");
    ItemGrid.Columns[4].HeaderText = Translate("Text");
    ItemGrid.Columns[5].HeaderText = Translate("Beskrivning");
    ItemGrid.Columns[6].HeaderText = Translate("Rader");
    ItemGrid.Columns[7].HeaderText = Translate("Url");
    ItemGrid.Columns[8].HeaderText = Translate("R‰ttigheter");

    base.OnLoad(e);
    AjaxPro.Utility.RegisterTypeForAjax(typeof(Backweb), this);
    RenderPermTmpl();
  }


  public void RenderPermTmpl  () {
    DataSet ds = DB.GetDS("select name from role order by name");
    String html = "<div id='editpermtmpl'><table>";
    for (int i=0; i < DB.GetRowCount(ds); i++)
      html += "<tr><td>" + DB.GetString(ds, i, "name") + "</td><td><select id='role" + DB.GetString(ds, i, "name") + "' class='ob_gEC' name='roles' /><option value=''>" + Translate("Standard") + "</option><option value='Y'>" + Translate("Ja") + "</option><option value='N'>" + Translate("Nej") + "</option></select></td></tr>";
    html += "</table></div>";
    PermEditTmpl.InnerHtml = html;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void MoveRow(String dir, String id) {
    int currorder = DB.GetInt("select orderno from backweb where id=" + id, "orderno");
    String sql;
    if (dir == "up") sql = "select id, orderno from backweb where orderno < " + currorder + " order by orderno desc";
    else sql = "select id, orderno from backweb where orderno > " + currorder + " order by orderno asc";
    DataSet ds = DB.GetDS(sql);
    if (DB.GetRowCount(ds) > 0) {
      int prevorder = DB.GetInt(ds, 0, "orderno");
      int previd = DB.GetInt(ds, 0, "id");
      DB.ExecSql("update backweb set orderno=" + prevorder + " where id=" + id);
      DB.ExecSql("update backweb set orderno=" + currorder + " where id=" + previd);
    }
  }


  protected void DeleteRecord(object sender, GridRecordEventArgs e) {
    int bwid = Convert.ToInt32(e.Record["id"]);
    DB.ExecSql("delete from backweb where id=" + bwid);
  }

  protected void UpdateRecord(object sender, GridRecordEventArgs e) {
    String id = e.Record["id"].ToString();
    String sql = "update backweb set name='" + e.Record["name"] + "', text='" + e.Record["text"] + "', description='" + e.Record["description"] + "', nofrows=" + e.Record["nofrows"] + ", pageurl='" + e.Record["pageurl"] + "' where id=" + id;
    DB.ExecSql(sql);

    UpdatePerm(id, e.Record["permissions"].ToString());
  }

  protected void InsertRecord(object sender, GridRecordEventArgs e) {
    int maxorder = DB.GetInt("select max(orderno) as maxno from backweb", "maxno") + 1;
    String sql = "insert into backweb (name, text, description, nofrows, pageurl, orderno) values('" + e.Record["name"] + "', '" + e.Record["text"] + "', '" + e.Record["description"] + "', " + e.Record["nofrows"] + ", '" + e.Record["pageurl"] + "', " + maxorder + ")";
    DB.ExecSql(sql);

    String id = DB.GetString("select max(id) as currid from backweb", "currid");
    UpdatePerm(id, e.Record["permissions"].ToString());
  }


  private void UpdatePerm(String id, String perm) {
    int ptid = DB.GetInt("select id from permissiontypes where itemtype='backweb'", "id");
    String sql = "delete from permissions where id='" + id + "' and typeid=" + ptid;
    DB.ExecSql(sql);

    if (perm.Length > 0) {
      String[] permarr = perm.Split(',');
      for (int i=0; i < permarr.Length; i++) {
        String[] hlp = permarr[i].Split('-');
        sql = "insert into permissions (id, typeid, role, permission) values('" + id + "', " + ptid + ", '" + hlp[0] + "', '" + hlp[1] + "')";
        DB.ExecSql(sql);
      }
    }
  }


  protected override DataView ItemGridData {
    get {
      DataTable dt = new DataTable();
      DataRow dr;

      dt.Columns.Add(new DataColumn("id", typeof(int)));
      dt.Columns.Add(new DataColumn("name", typeof(string)));
      dt.Columns.Add(new DataColumn("text", typeof(string)));
      dt.Columns.Add(new DataColumn("description", typeof(string)));
      dt.Columns.Add(new DataColumn("nofrows", typeof(int)));
      dt.Columns.Add(new DataColumn("pageurl", typeof(string)));
      dt.Columns.Add(new DataColumn("permissions", typeof(string)));

      DataSet ds = DB.GetDS("select * from backweb order by orderno");
      int ptid = DB.GetInt("select id from permissiontypes where itemtype='backweb'", "id");
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        dr = dt.NewRow();
        int id = DB.GetInt(ds, i, "id");
        dr[0] = id;
        dr[1] = DB.GetString(ds, i, "name");
        dr[2] = Translate(DB.GetString(ds, i, "text"));
        dr[3] = Translate(DB.GetString(ds, i, "description"));
        dr[4] = DB.GetInt(ds, i, "nofrows");
        dr[5] = DB.GetString(ds, i, "pageurl");

        String perm = "";
        DataSet ds2 = DB.GetDS("select * from permissions where typeid=" + ptid + " and id='" + id + "' order by role");
        for (int j=0; j < DB.GetRowCount(ds2); j++) {
          if (perm.Length > 0) perm += "<br>";
          perm += DB.GetString(ds2, j, "role") + "-" + DB.GetString(ds2, j, "permission");
        }
        dr[6] = perm;
        dt.Rows.Add(dr);
      }
      return dt.DefaultView;
    }
  }

}
