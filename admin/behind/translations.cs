/* $Date: 2011-03-10 01:28:36 +0100 (to, 10 mar 2011) $    $Revision: 7465 $ */
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


public partial class Translations : AdminBase {

  public override int LeftWidth {
    get { return 0; }
  }

  public override String BWPageName {
    get { return "Translations"; }
  }

  protected override Grid ObItemGrid {
    get { return ItemGrid; }
  }

  public String LangName(String lang) {
    if (lang == "sv") return Translate("Svenska");
    else if (lang == "en") return Translate("Engelska");
    else if (lang == "da") return Translate("Danska");
    else if (lang == "no") return Translate("Norska");
    else if (lang == "fi") return Translate("Finska");
    else if (lang == "de") return Translate("Tyska");
    else return Translate("Okänt");
  }
  
  protected void LangChanged(Object sender, EventArgs e) {
    Cms.Language = SelLangDD.SelectedItem.Value;
    SelLangDD.SelectedValue = Cms.Language;

    for (int i=0; i < ItemGrid.Columns.Count; i++) 
      if (ItemGrid.Columns[i].DataField.Length > 0)
        ItemGrid.Columns[i].HeaderText = LangName(ItemGrid.Columns[i].DataField);

    ItemGrid.DataSource = ItemGridData;
    ItemGrid.DataBind();
  }

  protected override void OnLoad(EventArgs e) {
    if (!IsPostBack) {
      ItemGrid.Columns[2].HeaderText = Translate("Svenska");
      SelLangDD.Items.Add(new ListItem(Translate("Svenska"), "sv"));

      for (int i=0; i < Cms.Languages.Length; i++) {
        if (Cms.Languages[i] != "sv") {
          Column col = new Column();
          col.HeaderText = LangName(Cms.Languages[i]);
          col.Width = "400px";
          col.DataField = Cms.Languages[i];
          ItemGrid.Columns.Add(col);

          SelLangDD.Items.Add(new ListItem(LangName(Cms.Languages[i]), Cms.Languages[i]));
        }
      }
      SelLangDD.SelectedValue = Cms.Language;
      AjaxPro.Utility.RegisterTypeForAjax(typeof(Translations), this);
    }
    base.OnLoad(e);
    
  }

  protected void DeleteRecord(object sender, GridRecordEventArgs e) {
    String sv = e.Record["sv"].ToString();
    DB.ExecSql("delete from translation where sv='" + sv + "'");
    Cms.RefreshTranslations();
  }

  protected void UpdateRecord(object sender, GridRecordEventArgs e) {
    String sv = e.Record["sv"].ToString().Replace("'","''");
    String snip = "";
    for (int i=0; i < Cms.Languages.Length; i++) {
      if (snip.Length > 0) snip += ",";
      snip += Cms.Languages[i] + "='" + e.Record[Cms.Languages[i]].ToString().Replace("'","''") + "'";
    }
    String sql = "update translation set " + snip + " where sv='" + sv + "'";
    DB.ExecSql(sql);
    Cms.RefreshTranslations();
  }


  protected void InsertRecord(object sender, GridRecordEventArgs e) {
    String snip1 = "";
    String snip2 = "";
    for (int i=0; i < Cms.Languages.Length; i++) {
      if (snip1.Length > 0) snip1 += ",";
      snip1 += Cms.Languages[i];
      if (snip2.Length > 0) snip2 += ",";
      snip2 += "'" + e.Record[Cms.Languages[i]].ToString().Replace("'","''") + "'";
    }
    String sql = "insert into translation (" + snip1 + ") values(" + snip2 + ")";
    DB.ExecSql(sql);
    Cms.RefreshTranslations();
  }


  protected override DataView ItemGridData {
    get {
      DataSet ds = DB.GetDS("select * from translation order by sv");
      return ds.Tables[0].DefaultView;
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void SetLanguage(String lang) {
    Cms.Language = lang;
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void RefreshLanguage() {
    Cms.RefreshTranslations();
  }

}
