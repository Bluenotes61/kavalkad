/* $Date: 2009-10-19 04:06:44 +0200 (mÃ¥, 19 okt 2009) $    $Revision: 5516 $ */
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using System.Collections;
using System.Text;
using Obout.Grid;
using NFN;


public partial class Sessions : AdminBase {

  public override int LeftWidth {
    get { return 0; }
  }

  public override String BWPageName {
    get { return "Sessions"; }
  }

  protected override Grid ObItemGrid {
    get { return ItemGrid; }
  }

  protected override void OnLoad(EventArgs e) {
    ItemGrid.Columns[1].HeaderText = Translate("Besökstid");
    ItemGrid.Columns[2].HeaderText = Translate("Senast aktiv");
    ItemGrid.Columns[3].HeaderText = Translate("IP-nummer");
    ItemGrid.Columns[4].HeaderText = Translate("DNS");
    ItemGrid.Columns[5].HeaderText = Translate("Referens");
    ItemGrid.Columns[6].HeaderText = Translate("Webbläsare");

    base.OnLoad(e);
  }

  protected override DataView ItemGridData {
    get {
      DataTable dt = new DataTable();
      DataRow dr;

      dt.Columns.Add(new DataColumn("sessionid", typeof(string)));
      dt.Columns.Add(new DataColumn("connecttime", typeof(string)));
      dt.Columns.Add(new DataColumn("lastactiontime", typeof(string)));
      dt.Columns.Add(new DataColumn("ip", typeof(string)));
      dt.Columns.Add(new DataColumn("dns", typeof(string)));
      dt.Columns.Add(new DataColumn("referrer", typeof(string)));
      dt.Columns.Add(new DataColumn("useragent", typeof(string)));

      DataSet ds = DB.GetDS("select * from sessions order by connecttime desc");
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        dr = dt.NewRow();
        dr[0] = DB.GetString(ds, i, "sessionid");
        dr[1] = DB.GetDate(ds, i, "connecttime").ToString("d MMM HH:mm:ss");
        dr[2] = DB.GetDate(ds, i, "lastactiontime").ToString("d MMM HH:mm:ss");
        dr[3] = DB.GetString(ds, i, "ip");
        dr[4] = DB.GetString(ds, i, "dns");
        dr[5] = DB.GetString(ds, i, "referrer");
        dr[6] = DB.GetString(ds, i, "useragent");

        dt.Rows.Add(dr);
      }
      return dt.DefaultView;
    }
  }

}
