/* $Date: 2011-01-10 11:57:10 +0100 (mÃ¥, 10 jan 2011) $    $Revision: 7267 $ */
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using NFN;
using Obout.Grid;


public partial class NewsAdmin : NewsBase {

  protected override void OnLoad(EventArgs e) {
    ItemGrid.Columns[2].HeaderText = Translate("Id");
    ItemGrid.Columns[4].HeaderText = Translate("Rubrik");
    ItemGrid.Columns[5].HeaderText = Translate("Datum");
    ItemGrid.Columns[6].HeaderText = Translate("Aktiv");
    ItemGrid.Columns[7].HeaderText = Translate("Visa i lista");
    ItemGrid.Columns[8].HeaderText = Translate("Förhandsgranska");
    ItemGrid.Columns[9].HeaderText = Translate("RSS");
    ItemGrid.Columns[10].HeaderText = Translate("Sammanfattning");
    ItemGrid.Columns[11].HeaderText = Translate("Innehåll");
    ItemGrid.Columns[12].HeaderText = Translate("Bild");

    base.OnLoad(e);
    
    TinyHelper.Text = GetHtmlTemplates() + GetStyles();

    if (!IsPostBack) {    
      DataSet ds = DB.GetDS("select * from extra where extratype='NewsType' order by orderno");
      if (DB.GetRowCount(ds) > 0) {
        SelHolder.Visible = true;
        NewsSelDD.DataSource = ds.Tables[0].DefaultView;
        NewsSelDD.DataValueField = "name";
        NewsSelDD.DataTextField = "description";
        NewsSelDD.DataBind();
      }
    }
  }
  
  protected override Grid ObItemGrid {
    get { return ItemGrid; }
  }

  protected override String GroupId {
    get { 
      String groupid = Request["group"];
      if (groupid == null || groupid.Length == 0)
        groupid = "News";
      if (IsPostBack && NewsSelDD.SelectedValue.Length > 0)
        groupid = NewsSelDD.SelectedValue;
      return groupid;
    }
  }

  public override String BWPageName {
    get { return "News"; }
  }

  private String GetHtmlTemplates() {
    String html = "var htmlTemplates = [";
    DataSet ds = DB.GetDS("select * from extra where extratype='HtmlTemplate' order by orderno");
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      if (i > 0) html += ",";
      html += "{title:'" + DB.GetString(ds, i, "name") + "', value:'" + DB.GetString(ds, i, "val") + "', description:'" + DB.GetString(ds, i, "description") + "'}";
    }
    html += "];";
    return html;
  }

  private String GetStyles() {
    DataSet ds = DB.GetDS("select name, description from extra where extratype='Css' order by orderno");
    String styles = "";
    for (int i = 0; i < DB.GetRowCount(ds); i++) {
      if (i > 0) styles += ";";
      styles += DB.GetString(ds, i, "description") + "=" + DB.GetString(ds, i, "name");
    }
    return "var tinyStyles='" + styles + "';";
  }

}
