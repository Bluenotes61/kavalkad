/* $Date: 2011-04-24 20:51:47 +0200 (sÃ¶, 24 apr 2011) $    $Revision: 7627 $ */
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using System.Collections;
using System.Text;
using System.Configuration;
using obout_ASPTreeView_2_NET;
using NFN;
using Obout.Grid;
using AjaxPro;


public partial class AdminLog : AdminBase {

  public override int LeftWidth {
    get { return 0; }
  }

  public override String BWPageName {
    get { return "AdminLog"; }
  }

  protected override Grid ObItemGrid {
    get { return ItemGrid; }
  }

  protected override void OnLoad(EventArgs e) {
    ItemGrid.Columns[0].HeaderText = Translate("Id");
    ItemGrid.Columns[1].HeaderText = Translate("Tid");
    ItemGrid.Columns[2].HeaderText = Translate("Administratör");
    ItemGrid.Columns[3].HeaderText = Translate("Händelse");
    ItemGrid.Columns[4].HeaderText = Translate("Sida");
    ItemGrid.Columns[5].HeaderText = Translate("Kontroll");

    base.OnLoad(e);
  }

  protected override DataView ItemGridData {
    get {
      DataTable dt = new DataTable();
      DataRow dr;

      dt.Columns.Add(new DataColumn("id", typeof(int)));
      dt.Columns.Add(new DataColumn("time", typeof(string)));
      dt.Columns.Add(new DataColumn("user", typeof(string)));
      dt.Columns.Add(new DataColumn("event", typeof(string)));
      dt.Columns.Add(new DataColumn("webpage", typeof(string)));
      dt.Columns.Add(new DataColumn("control", typeof(string)));

      DataSet ds = DB.GetDS("select a.*, u.username from adminlog a, users u where u.id=a.userid order by eventtime desc");
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        dr = dt.NewRow();

        String atime = DB.GetDate(ds, i, "eventtime").ToString("yyyy-MM-dd HH:mm:ss");

        dr[0] = DB.GetInt(ds, i, "id");
        dr[1] = atime;
        dr[2] = DB.GetString(ds, i, "username");
        String eventaction = DB.GetString(ds, i, "eventaction");
        dr[3] = GetEventString(Translate(eventaction));
        String aPage = "";
        String actrl = "";
        String itemid = DB.GetString(ds, i, "itemid");
        if (itemid.Length > 0) {
          if (eventaction == "saveprop" || eventaction == "publish" || eventaction == "unpublish") {
            aPage = GetPageForCtrl(itemid);
            actrl = GetCtrlName(itemid);
          }
          else if (eventaction == "savepage")
            aPage = itemid;
          aPage = "<a href='/" + aPage + ".aspx'>" + aPage + ".aspx</a>";
        }
        dr[4] = aPage;
        dr[5] = actrl;


        dt.Rows.Add(dr);
      }
      return dt.DefaultView;
    }
  }

  private String GetEventString(String eventStr) {
    if (eventStr == "login") return "Inloggning";
    else if (eventStr == "logout") return "Utloggning";
    else if (eventStr == "logout") return "Utloggning";
    else if (eventStr == "saveprop") return "Spara ändring";
    else if (eventStr == "publish") return "Publicera ändring";
    else if (eventStr == "unpublish") return "Ångra ändring";
    else if (eventStr == "savepage") return "Spara webbsida";
    else return eventStr;
  }
  
  private String GetPageForCtrl(String strid) {
    int id = 0;
    try { id = Convert.ToInt32(strid); }
    catch { return ""; }
    int parent = DB.GetInt("select parentproperty from pageproperty where id=" + id, "parentproperty");
    while (parent != 0) {
      id = parent;
      parent = DB.GetInt("select parentproperty from pageproperty where id=" + id, "parentproperty");
    }
    return DB.GetString("select id from webpage where mainprop=" + id, "id");
  }

  private String GetCtrlName(String id) {
    DataSet ds = DB.GetDS("select propertyname, parentproperty from pageproperty where id=" + id);
    String aname = "";
    if (DB.GetRowCount(ds) > 0) {
      aname = DB.GetString(ds, 0, "propertyname");
      if (aname == "text")
        aname = DB.GetString("select propertyname from pageproperty where id=" + DB.GetString(ds, 0, "parentproperty"), "propertyname");
    }
    return aname;
  }

}
