/* $Date: 2010-11-22 14:32:20 +0100 (mÃ¥, 22 nov 2010) $    $Revision: 7099 $ */
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using System.Collections;
using System.Text;
using obout_ASPTreeView_2_NET;
using NFN;
using Obout.Grid;
using AjaxPro;


public partial class Roles : AdminBase {

  public override int LeftWidth {
    get { return 0; }
  }

  public override String BWPageName {
    get { return "Roles"; }
  }

  protected override Grid ObItemGrid {
    get { return ItemGrid; }
  }



  protected override void OnLoad(EventArgs e) {
    ItemGrid.Columns[2].HeaderText = Translate("Rollnamn");
    ItemGrid.Columns[3].HeaderText = Translate("Beskrivning");
    ItemGrid.Columns[4].HeaderText = Translate("Användare");
    ItemGrid.Columns[5].HeaderText = Translate("Behörigheter");

    base.OnLoad(e);
    AjaxPro.Utility.RegisterTypeForAjax(typeof(Roles), this);
    RenderCheckboxes();
  }

  public void RenderCheckboxes() {
    DataSet ds = DB.GetDS("select id, username from users where deleted=0 order by username");
    String html = "<div id='userlist'>";
    for (int i=0; i < DB.GetRowCount(ds); i++)
      html += "<div><input type='checkbox' id='user" + DB.GetString(ds, i, "username") + "' name='users' /><label for='user" + DB.GetString(ds, i, "username") + "'>" + DB.GetString(ds, i, "username") + "</label></div>";
    html += "</div>";
    UserChkList.InnerHtml = html;

    ds = DB.GetDS("select * from permissiontypes order by itemtype, controltype, orderno");
    html = "<div id='permlist'>";
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String lab = DB.GetString(ds, i, "itemtype") + "-" + DB.GetString(ds, i, "controltype") + "-" + DB.GetString(ds, i, "actiontype") + "-" + DB.GetString(ds, i, "description");
      html += "<div><input type='checkbox' id='perm" + DB.GetString(ds, i, "id") + "' name='perm' /><label for='perm" + DB.GetString(ds, i, "id") + "'>" + lab + "</label></div>";
    }
    html += "</div>";
    PermChkList.InnerHtml = html;
  }

  protected String GetFieldLink(String text, String value) {
    if (value.Length == 0) return "";
    String[] arr = value.Replace("<br>","|").Replace("<br />","|").Split('|');
    if (arr.Length == 1) return arr[0];
    return "<a href='javascript:void(0)' onclick=\"popField(this, '" + value + "')\">" + arr.Length + " " + Translate(text) + "</a>";
  }

  protected void DeleteRecord(object sender, GridRecordEventArgs e) {
    String role = e.Record["name"].ToString();
    DB.ExecSql("delete from permissions where role='" + role + "'");
    DB.ExecSql("delete from role where name='" + role + "'");
    Cms.RefreshRoles();
  }

  private void UpdateRolePerm(String role, String users, String perm) {
    DataSet ds = DB.GetDS("select id from permissiontypes where itemtype='User'");
    int userdefid = DB.GetInt(ds, 0, "id");
    String sql = "delete from permissions where typeid=" + userdefid + " and role='" + role + "'";
    DB.ExecSql(sql);

    if (users.Length > 0) {
      String[] userarr = users.Split(',');
      for (int i=0; i < userarr.Length; i++) {
        int id = DB.GetInt("select id from users where deleted=0 and username='" + userarr[i] + "'", "id");
        sql = "insert into permissions (id, typeid, role, permission) values('" + id + "', " + userdefid + ", '" + role + "', 'Y')";
        DB.ExecSql(sql);
      }
    }

    sql = "delete from permissions where id='DEFAULT' and role='" + role + "'";
    DB.ExecSql(sql);

    if (perm.Length > 0) {
      String[] permarr = perm.Split(',');
      for (int i=0; i < permarr.Length; i++) {
        sql = "insert into permissions (id, typeid, role, permission) values('DEFAULT', " + permarr[i] + ", '" + role + "', 'Y')";
        DB.ExecSql(sql);
      }
    }
  }

  protected void UpdateRecord(object sender, GridRecordEventArgs e) {

    String role = e.Record["name"].ToString();

    String sql = "update role set description='" + e.Record["description"] + "' where name='" + role + "'";
    DB.ExecSql(sql);

    UpdateRolePerm(role, e.Record["users"].ToString(), e.Record["permissions"].ToString());
  }


  protected void InsertRecord(object sender, GridRecordEventArgs e) {
    String role = e.Record["name"].ToString();

    String sql = "insert into role (name, description) values('" + role + "', '" + e.Record["description"] + "')";
    DB.ExecSql(sql);

    UpdateRolePerm(role, e.Record["users"].ToString(), e.Record["permissions"].ToString());
    Cms.RefreshRoles();
  }


  protected override DataView ItemGridData {
    get {
      DataTable dt = new DataTable();
      DataRow dr;

      dt.Columns.Add(new DataColumn("name", typeof(string)));
      dt.Columns.Add(new DataColumn("description", typeof(string)));
      dt.Columns.Add(new DataColumn("users", typeof(string)));
      dt.Columns.Add(new DataColumn("permissions", typeof(string)));

      DataSet ds0 = DB.GetDS("select * from role where name <> 'Admin'");
      int utypeid = DB.GetInt("select id from permissiontypes where itemtype='user'", "id");

      for (int i=0; i < DB.GetRowCount(ds0); i++) {
        dr = dt.NewRow();
        String arole = DB.GetString(ds0, i, "name");
        dr[0] = arole;
        dr[1] = DB.GetString(ds0, i, "description");
        String users = "";
        String sql = "select u.username from users u, permissions p where u.deleted=0 and p.typeid=" + utypeid + " and p.role='" + arole + "' and CAST(p.id as char)=CAST(u.id AS char)";
        DataSet ds2 = DB.GetDS(sql);
        for (int j=0; j < DB.GetRowCount(ds2); j++) {
          if (users.Length > 0) users += "<br />";
          users += DB.GetString(ds2, j, "username");
        }
        dr[2] = users;
        String perm = "";
        sql = "select distinct pt.* from permissions p, permissiontypes pt where p.role='" + arole + "' and pt.itemtype <> 'User' and p.permission='Y' and p.id='DEFAULT' and p.typeid=pt.id order by pt.itemtype, controltype, orderno";
        ds2 = DB.GetDS(sql);
        for (int j=0; j < DB.GetRowCount(ds2); j++) {
          if (perm.Length > 0) perm += "<br />";
          perm += DB.GetString(ds2, j, "id") + "-" + DB.GetString(ds2, j, "itemtype") + "-" + DB.GetString(ds2, j, "controltype") + "-" + DB.GetString(ds2, j, "actiontype") + "-" + Translate(DB.GetString(ds2, j, "description"));
        }
        dr[3] = perm;

        dt.Rows.Add(dr);
      }
      return dt.DefaultView;
    }
  }


}
