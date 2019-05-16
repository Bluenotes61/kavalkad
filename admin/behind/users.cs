/* $Date: 2010-11-22 14:32:20 +0100 (mÃ¥, 22 nov 2010) $    $Revision: 7099 $ */
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.IO;
using System.Collections;
using System.Text;
using obout_ASPTreeView_2_NET;
using NFN;
using Obout.Grid;
using AjaxPro;

public partial class Users : AdminBase {
  
  private bool hasmoreattribs = false;

  public override int LeftWidth {
    get { return 0; }
  }

  public override String BWPageName {
    get { return "Users"; }
  }

  protected override Grid ObItemGrid {
    get { return ItemGrid; }
  }
  
  public void ExportGrid(object src, EventArgs e) {
    ItemGrid.ExportToExcel("Users", true, false, false, true);
  }

  public bool HasMoreAttribs {
    get { return hasmoreattribs; }
  }

  protected override void OnLoad(EventArgs e) {
    ItemGrid.Columns[2].HeaderText = Translate("Id");
    ItemGrid.Columns[3].HeaderText = Translate("Användartyp");
    ItemGrid.Columns[4].HeaderText = Translate("Användarnamn");
    ItemGrid.Columns[5].HeaderText = Translate("Lösenord");
    ItemGrid.Columns[6].HeaderText = Translate("E-postadress");
    ItemGrid.Columns[7].HeaderText = Translate("Beskrivning");
    ItemGrid.Columns[8].HeaderText = Translate("Aktiv");
    ItemGrid.Columns[9].HeaderText = Translate("Roller");
    if (!IsPostBack) {
      DataSet ds = DB.GetDS("select * from userattribfields order by orderno");
      hasmoreattribs = (DB.GetRowCount(ds) > 0);
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        Column col = new Column();
        col.DataField = "attrib_" + DB.GetString(ds, i, "id");
        col.HeaderText = Translate(DB.GetString(ds, i, "displayname"));
        if (DB.GetString(ds, i, "colwidth").Length > 0)
          col.Width = DB.GetString(ds, i, "colwidth");
        col.Wrap = true;
        col.ParseHTML = true;
        col.Visible = false;
        ItemGrid.Columns.Add(col);
      }
    }

    base.OnLoad(e);
    AjaxPro.Utility.RegisterTypeForAjax(typeof(Users), this);
    RenderRoleCheckboxes();
  }

  public void RenderRoleCheckboxes() {
    DataSet ds = DB.GetDS("select name from role order by name");
    String html = "<div id='rolelist'>";
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String arole = DB.GetString(ds, i, "name");
      if (arole != "Admin" || Cms.User.IsSysAdmin)
        html += "<div><input type='checkbox' id='role" + arole + "' name='roles' /><label for='role" + arole + "'>" + arole + "</label></div>";
    }
    RoleChkList.InnerHtml = html;
    html += "</div>";
  }


  protected void DeleteRecord(object sender, GridRecordEventArgs e) {
    int userid = Convert.ToInt32(e.Record["id"]);
    Cms.DeleteUser(userid);
  }

  protected void UpdateRecord(object sender, GridRecordEventArgs e) {
    int userid = Convert.ToInt32(e.Record["id"]);

    bool exists = DB.GetInt("select count(*) as nof from users where deleted=0 and id <> " + e.Record["id"] + " and username='" + e.Record["username"] + "'", "nof") > 0;
    if (exists)
      throw new Exception("Fel: Användare " + e.Record["username"] + " finns redan");

    NFNUser currUser = new NFNUser(userid);
    currUser.UserType = e.Record["usertype"].ToString();
    currUser.UserName = e.Record["username"].ToString();
    currUser.Password = e.Record["password"].ToString();
    currUser.Email = e.Record["email"].ToString();
    currUser.Description = e.Record["description"].ToString();
    currUser.Approved = Convert.ToBoolean(e.Record["approved"]);

    DataSet dsfields = DB.GetDS("select * from userattribfields order by orderno");
    for (int i=0; i < DB.GetRowCount(dsfields); i++) {
      String fname = DB.GetString(dsfields, i, "name");
      String val = e.Record["attrib_" + DB.GetString(dsfields, i, "id")].ToString();
      currUser.SetAttrib(fname, val);
    }

    String[] rolearr = e.Record["roles"].ToString().Split(',');
    ArrayList roles = new ArrayList(rolearr);
    currUser.Roles = roles;
    currUser.WriteToDB();
  }


  protected void InsertRecord(object sender, GridRecordEventArgs e) {
    bool exists = DB.GetInt("select count(*) as nof from users where deleted=0 and username='" + e.Record["username"] + "'", "nof") > 0;
    if (exists)
      throw new Exception("Fel: Användare " + e.Record["username"] + " finns redan");

    Hashtable attrib = new Hashtable();
    String res = Cms.RegisterUser(e.Record["usertype"].ToString(), e.Record["username"].ToString(), e.Record["password"].ToString(), e.Record["email"].ToString(), e.Record["description"].ToString(), e.Record["roles"].ToString(), Convert.ToBoolean(e.Record["approved"]), attrib);
    if (res.Length > 0)
      throw new Exception(res);

    int userid = DB.GetInt("select id from users where deleted=0 and username='" + e.Record["username"] + "'", "id");
    NFNUser currUser = new NFNUser(userid);
    DataSet dsfields = DB.GetDS("select * from userattribfields order by orderno");
    for (int i=0; i < DB.GetRowCount(dsfields); i++) {
      String fname = DB.GetString(dsfields, i, "name");
      String val = e.Record["attrib_" + DB.GetString(dsfields, i, "id")].ToString();
      currUser.SetAttrib(fname, val);
    }
    currUser.WriteToDB();
  }


  protected override DataView ItemGridData {
    get {
      DataTable dt = new DataTable();
      DataRow dr;

      dt.Columns.Add(new DataColumn("id", typeof(string)));
      dt.Columns.Add(new DataColumn("usertype", typeof(string)));
      dt.Columns.Add(new DataColumn("username", typeof(string)));
      dt.Columns.Add(new DataColumn("password", typeof(string)));
      dt.Columns.Add(new DataColumn("email", typeof(string)));
      dt.Columns.Add(new DataColumn("description", typeof(string)));
      dt.Columns.Add(new DataColumn("approved", typeof(bool)));
      dt.Columns.Add(new DataColumn("roles", typeof(string)));

      DataSet dsfields = DB.GetDS("select * from userattribfields order by orderno");
      for (int i=0; i < DB.GetRowCount(dsfields); i++)
        dt.Columns.Add(new DataColumn("attrib_" + DB.GetString(dsfields, i, "id"), typeof(string)));

      DataSet ds = DB.GetDS("select * from users where deleted=0");
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        dr = dt.NewRow();
        String userid = DB.GetString(ds, i, "id");
        String displayid = userid;
        while (displayid.Length < 4) displayid = "0" + displayid;
        dr[0] = displayid;
        dr[1] = DB.GetString(ds, i, "usertype");
        dr[2] = DB.GetString(ds, i, "username");
        dr[3] = DB.GetString(ds, i, "password");
        dr[4] = DB.GetString(ds, i, "email");
        dr[5] = DB.GetString(ds, i, "description");
        dr[6] = DB.GetString(ds, i, "approved") == "Y";
        String roles = "";
        DataSet ds2 = DB.GetDS("select role from viewpermissions where itemtype='User' and id='" + userid + "'");
        bool userIsAdmin = false;
        for (int j=0; j < DB.GetRowCount(ds2); j++) {
          if (roles.Length > 0) roles += "<br />";
          String arole = DB.GetString(ds2, j, "role");
          if (arole == "Admin") userIsAdmin = true;
          roles += arole;
        }
        dr[7] = roles;

        if (!userIsAdmin || Cms.User.IsSysAdmin) {
          for (int j=0; j < DB.GetRowCount(dsfields); j++) {
            String val = DB.GetString("select svalue from userattrib where userid=" + userid + " and fieldid=" + DB.GetInt(dsfields, j, "id"), "svalue");
            dr[8+j] = val;
          }

          dt.Rows.Add(dr);
        }
      }
      return dt.DefaultView;
    }
  }


}
