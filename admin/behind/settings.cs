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
using AjaxPro;
using Obout.Grid;


public partial class Settings : AdminBase {
  private String rightHeader = "";

  public override int LeftWidth {
    get { return 0; }
  }

  public override String BWPageName {
    get { return "Settings"; }
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    AjaxPro.Utility.RegisterTypeForAjax(typeof(Settings), this);
    
    TemplatesGrid.Columns[2].HeaderText = Translate("Filnamn");
    TemplatesGrid.Columns[3].HeaderText = Translate("Namn");
    TemplatesGrid.Columns[4].HeaderText = Translate("Kategori");
    TemplatesGrid.Columns[5].HeaderText = Translate("Ikon");
    
    AttribsGrid.Columns[3].HeaderText = Translate("Id");
    AttribsGrid.Columns[4].HeaderText = Translate("Namn");
    AttribsGrid.Columns[5].HeaderText = Translate("Typ");
    AttribsGrid.Columns[6].HeaderText = Translate("Språk");
    AttribsGrid.Columns[7].HeaderText = Translate("Alternativ");

    CssGrid.Columns[2].HeaderText = Translate("Klassnamn");
    CssGrid.Columns[3].HeaderText = Translate("Beskrivning");

    if (!IsPostBack) {
      SetGridProps(TemplatesGrid);
      SetGridProps(AttribsGrid);
      SetGridProps(CssGrid);
    }

    GenSettings.Text = GetSettingsTable();
    TemplatesGrid.DataSource = TemplatesData;
    TemplatesGrid.DataBind();
    AttribsGrid.DataSource = AttribData();
    AttribsGrid.DataBind();
    CssGrid.DataSource = ExtraData("Css");
    CssGrid.DataBind();
  }

  private String GetSettingsTable() {
    String html = "<table cellspacing='0' cellpadding='0' class='itemtable' style='width:auto'>";
    DataSet ds = DB.GetDS("select * from extra where extratype='settings' order by orderno");
    String btns = "<a href='javascript:void(0)' onclick='editSetting(this)' onfocus='this.blur()'><img src='gfx/edit.gif' border='0' /></a><a href='javascript:void(0)' onclick='saveSetting(this)' style='display:none' onfocus='this.blur()'><img src='gfx/save.gif' border='0' /></a><a href='javascript:void(0)' onclick='cancelSetting(this)' style='display:none' onfocus='this.blur()'><img src='gfx/cancel.gif' border='0' /></a>";
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      html += "<tr>" +
        "<td class='hl'>" + Translate(DB.GetString(ds, i, "description")) + ":</td>" +
        "<td class='value'><span id='" + DB.GetString(ds, i, "name") + "'>" + DB.GetString(ds, i, "val") + "</span><input type='text' class='editvalue' value='" + DB.GetString(ds, i, "val") + "' style='display:none' /></td><td class='btns'>" + btns + "</td>" +
        "</tr>";
    }
    html += "</table>";
    return html;
  }

  public void DeleteTemplate(object sender, GridRecordEventArgs e) {
    DB.ExecSql("delete from pagetemplateattrib where filename='" + e.Record["filename"] + "'");
    DB.ExecSql("delete from webpagetemplate where filename='" + e.Record["filename"] + "'");
  }

  public void UpdateTemplate(object sender, GridRecordEventArgs e) {
    DB.ExecSql("update webpagetemplate set name='" + e.Record["name"] + "', category='" + e.Record["category"] + "', icon='" + e.Record["icon"] + "' where filename='" + e.Record["filename"] + "'");
  }

  public void InsertTemplate(object sender, GridRecordEventArgs e) {
    int maxorder = DB.GetInt("select max(orderno) as maxno from webpagetemplate", "maxno") + 1;
    DB.ExecSql("insert into webpagetemplate (filename, name, category, icon, orderno) values('" + e.Record["filename"] + "', '" + e.Record["name"] + "', '" + e.Record["category"] + "', '" + e.Record["icon"] + "', " + maxorder + ")");
  }

  public void DeleteAttrib(object sender, GridRecordEventArgs e) {
    DB.ExecSql("delete from pagetemplateattrib where filename='" + e.Record["filename"] + "' and id='" + e.Record["id"] + "'");
  }

  public void UpdateAttrib(object sender, GridRecordEventArgs e) {
    DB.ExecSql("update pagetemplateattrib set name='" + e.Record["name"] + "', type='" + e.Record["type"] + "', lang='" + e.Record["lang"] + "', alternatives='" + e.Record["alternatives"] + "' where filename='" + e.Record["filename"] + "' and id='" + e.Record["id"] + "'");
  }

  public void InsertAttrib(object sender, GridRecordEventArgs e) {
    int maxorder = DB.GetInt("select max(orderno) as maxno from pagetemplateattrib where filename='" + e.Record["filename"] + "'", "maxno") + 1;
    DB.ExecSql("insert into pagetemplateattrib (filename, id, name, type, lang, alternatives, orderno) values('" + e.Record["filename"] + "', '" + e.Record["id"] + "', '" + e.Record["name"] + "', '" + e.Record["type"] + "', '" + e.Record["lang"] + "', '" + e.Record["alternatives"] + "', " + maxorder + ")");
  }

  public void DeleteCss(object sender, GridRecordEventArgs e) {
    DB.ExecSql("delete from extra where extratype='css' and name='" + e.Record["name"] + "'");
  }

  public void UpdateCss(object sender, GridRecordEventArgs e) {
    DB.ExecSql("update extra set description='" + e.Record["description"] + "' where extratype='css' and name='" + e.Record["name"] + "'");
  }

  public void InsertCss(object sender, GridRecordEventArgs e) {
    int maxorder = DB.GetInt("select max(orderno) as maxno from extra where extratype='css'", "maxno") + 1;
    DB.ExecSql("insert into extra (extratype, name, description, orderno) values('css', '" + e.Record["name"] + "', '" + e.Record["description"] + "', " + maxorder + ")");
  }

  public void DeleteScript(object sender, GridRecordEventArgs e) {
    DB.ExecSql("delete from extra where extratype='scriptfile' and name='" + e.Record["name"] + "'");
  }

  public void UpdateScript(object sender, GridRecordEventArgs e) {
    DB.ExecSql("update extra set val='" + e.Record["val"] + "' where extratype='scriptfile' and name='" + e.Record["name"] + "'");
  }

  public void InsertScript(object sender, GridRecordEventArgs e) {
    int maxorder = DB.GetInt("select max(orderno) as maxno from extra where extratype='scriptfile'", "maxno") + 1;
    DB.ExecSql("insert into extra (extratype, name, val, orderno) values('ScriptFile', '" + e.Record["name"] + "', '" + e.Record["val"] + "', " + maxorder + ")");
  }

  private DataView TemplatesData {
    get {
      DataTable dt = new DataTable();
      DataRow dr;

      dt.Columns.Add(new DataColumn("filename", typeof(string)));
      dt.Columns.Add(new DataColumn("name", typeof(string)));
      dt.Columns.Add(new DataColumn("category", typeof(string)));
      dt.Columns.Add(new DataColumn("icon", typeof(string)));

      DataSet ds = DB.GetDS("select * from webpagetemplate order by orderno");
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        dr = dt.NewRow();
        dr[0] = DB.GetString(ds, i, "filename");
        dr[1] = DB.GetString(ds, i, "name");
        dr[2] = DB.GetString(ds, i, "category");
        dr[3] = DB.GetString(ds, i, "icon");
        dt.Rows.Add(dr);
      }
      return dt.DefaultView;
    }
  }

  private DataView ExtraData(String extratype) {
    DataTable dt = new DataTable();
    DataRow dr;

    dt.Columns.Add(new DataColumn("name", typeof(string)));
    dt.Columns.Add(new DataColumn("description", typeof(string)));
    dt.Columns.Add(new DataColumn("val", typeof(string)));

    DataSet ds = DB.GetDS("select * from extra where extratype='" + extratype + "' order by orderno");
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      dr = dt.NewRow();
      dr[0] = DB.GetString(ds, i, "name");
      dr[1] = DB.GetString(ds, i, "description");
      dr[2] = DB.GetString(ds, i, "val");
      dt.Rows.Add(dr);
    }
    return dt.DefaultView;
  }

  private DataView AttribData() {
    DataTable dt = new DataTable();
    DataRow dr;

    dt.Columns.Add(new DataColumn("filename", typeof(string)));
    dt.Columns.Add(new DataColumn("id", typeof(string)));
    dt.Columns.Add(new DataColumn("name", typeof(string)));
    dt.Columns.Add(new DataColumn("type", typeof(string)));
    dt.Columns.Add(new DataColumn("lang", typeof(string)));
    dt.Columns.Add(new DataColumn("alternatives", typeof(string)));

    DataSet ds = DB.GetDS("select * from pagetemplateattrib order by orderno");
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      dr = dt.NewRow();
      dr[0] = DB.GetString(ds, i, "filename");
      dr[1] = DB.GetString(ds, i, "id");
      dr[2] = DB.GetString(ds, i, "name");
      dr[3] = DB.GetString(ds, i, "type");
      dr[4] = DB.GetString(ds, i, "lang");
      dr[5] = DB.GetString(ds, i, "alternatives");
      dt.Rows.Add(dr);
    }
    return dt.DefaultView;
  }

  protected virtual void SetGridProps(Grid agrid) {
    agrid.FolderStyle = "/admin/obout/grid/bwstyle";
    agrid.AllowFiltering = true;
    agrid.AllowAddingRecords = true;
    agrid.AllowColumnResizing = true;
    agrid.AutoGenerateColumns = false;
    agrid.EnableTypeValidation = true;
    agrid.Serialize = false;
    agrid.FolderLocalization = "/admin/obout/grid/localization";
    agrid.Language = Cms.Language;
    agrid.CallbackMode = true;
    agrid.AllowGrouping = false;
    agrid.FolderExports = "/admin/exports";
    agrid.PageSize = NofRows;
    agrid.PageSizeOptions = "5,10,20,50,100,500";
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void MoveTemplateRow(String dir, String filename) {
    int currorder = DB.GetInt("select orderno from webpagetemplate where filename='" + filename + "'", "orderno");
    String sql;
    if (dir == "up") sql = "select filename, orderno from webpagetemplate where orderno < " + currorder + " order by orderno desc";
    else sql = "select filename, orderno from webpagetemplate where orderno > " + currorder + " order by orderno asc";
    DataSet ds = DB.GetDS(sql);
    if (DB.GetRowCount(ds) > 0) {
      int prevorder = DB.GetInt(ds, 0, "orderno");
      String prevname = DB.GetString(ds, 0, "filename");
      DB.ExecSql("update webpagetemplate set orderno=" + prevorder + " where filename='" + filename + "'");
      DB.ExecSql("update webpagetemplate set orderno=" + currorder + " where filename='" + prevname + "'");
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void MoveAttribRow(String dir, String filename, String id) {
    int currorder = DB.GetInt("select orderno from pagetemplateattrib where filename='" + filename + "' and id='" + id + "'", "orderno");
    String sql;
    if (dir == "up") sql = "select filename, id, orderno from pagetemplateattrib where orderno < " + currorder + " order by orderno desc";
    else sql = "select filename, id, orderno from pagetemplateattrib where orderno > " + currorder + " order by orderno asc";
    DataSet ds = DB.GetDS(sql);
    if (DB.GetRowCount(ds) > 0) {
      int prevorder = DB.GetInt(ds, 0, "orderno");
      String prevname = DB.GetString(ds, 0, "filename");
      String previd = DB.GetString(ds, 0, "id");
      DB.ExecSql("update pagetemplateattrib set orderno=" + prevorder + " where filename='" + filename + "' and id='" + id + "'");
      DB.ExecSql("update pagetemplateattrib set orderno=" + currorder + " where filename='" + prevname + "' and id='" + previd + "'");
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void MoveExtraRow(String dir, String extratype, String name) {
    int currorder = DB.GetInt("select orderno from extra where extratype='" + extratype + "' and name='" + name + "'", "orderno");
    String sql;
    if (dir == "up") sql = "select name, orderno from extra where extratype='" + extratype + "' and orderno < " + currorder + " order by orderno desc";
    else sql = "select name, orderno from extra where extratype='" + extratype + "' and orderno > " + currorder + " order by orderno asc";
    DataSet ds = DB.GetDS(sql);
    if (DB.GetRowCount(ds) > 0) {
      int prevorder = DB.GetInt(ds, 0, "orderno");
      String prevname = DB.GetString(ds, 0, "name");
      DB.ExecSql("update extra set orderno=" + prevorder + " where extratype='" + extratype + "' and name='" + name + "'");
      DB.ExecSql("update extra set orderno=" + currorder + " where extratype='" + extratype + "' and name='" + prevname + "'");
    }
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void SaveSetting(String key, String value) {
    DB.ExecSql("update extra set val='" + value + "' where name='" + key + "' and extratype='Settings'");
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void Recover(String adate) {
  
    DataSet ds = DB.GetDS("select * from a_documents where changedate >= '" + adate + "' order by changedate asc");
    ArrayList ids = new ArrayList();
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String currId = DB.GetString(ds, i, "docid");
      if (!ids.Contains(currId)) {
        ids.Add(currId);
        RestoreDoc(ds, i);
      }
    }

    ds = DB.GetDS("select * from a_filepath where changedate >= '" + adate + "' order by changedate asc");
    ids.Clear();
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String currId = DB.GetString(ds, i, "filepathid");
      if (!ids.Contains(currId)) {
        ids.Add(currId);
        RestorePath(ds, i);
      }
    }

    ds = DB.GetDS("select * from a_news where changedate >= '" + adate + "' order by changedate asc");
    ids.Clear();
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String currId = DB.GetString(ds, i, "mainpropid");
      if (!ids.Contains(currId)) {
        ids.Add(currId);
        RestoreNews(ds, i);
      }
    }

    ds = DB.GetDS("select * from a_pageproperty where changedate >= '" + adate + "' order by changedate asc");
    ids.Clear();
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String currId = DB.GetString(ds, i, "propid");
      if (!ids.Contains(currId)) {
        ids.Add(currId);
        Cms.RestoreProp(ds, i);
      }
    }

    ds = DB.GetDS("select * from users where deleted=0 and deletedate >= '" + adate + "'");
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      int currId = DB.GetInt(ds, i, "id");
      Cms.RestoreUser(i);
    }
  }
  
  private void RestoreDoc(DataSet ds, int idx) {
    String id = DB.GetString(ds, idx, "docid");
    String sql = "update documents set filepath={0}, doctype={1}, filesize={2}, imagewidth={3}, imageheight={4}, type={5}, category={6}, filename={7}, description={8}, date_start={9}, date_stop={10}, deleted={11} where id={12}";
    sql = String.Format(sql,DB.GetValAsString(ds,idx,"filepath"),DB.GetValAsString(ds,idx,"doctype"),DB.GetValAsString(ds,idx,"filesize"),DB.GetValAsString(ds,idx,"imagewidth"),DB.GetValAsString(ds,idx,"imageheight"),DB.GetValAsString(ds,idx,"type"),DB.GetValAsString(ds,idx,"category"),DB.GetValAsString(ds,idx,"filename"),DB.GetValAsString(ds,idx,"description"),DB.GetValAsString(ds,idx,"date_start"),DB.GetValAsString(ds,idx,"date_stop"),DB.GetValAsString(ds,idx,"deleted"),DB.GetValAsString(ds,idx,"docid"));
    DB.ExecSql(sql);
  }

  private void RestorePath(DataSet ds, int idx) {
    String id = DB.GetString(ds, idx, "filepathid");
    String sql = "update filepath set pathname={0}, parentpath={1}, deleted={2} where id={3}";
    sql = String.Format(sql,DB.GetValAsString(ds,idx,"pathname"),DB.GetValAsString(ds,idx,"parentpath"),DB.GetValAsString(ds,idx,"deleted"),DB.GetValAsString(ds,idx,"filepathid"));
    DB.ExecSql(sql);
  }

  private void RestoreNews(DataSet ds, int idx) {
    String id = DB.GetString(ds, idx, "mainpropid");
    String sql = "update news set groupid={0}, headline={1}, image={2}, active={3}, showinlist={4}, preview={5}, showinrss={6}, newsdate={7}, newsdate2={8}, newslanguage={9}, deleted={10} where mainpropid={11}";
    sql = String.Format(sql,DB.GetValAsString(ds,idx,"groupid"),DB.GetValAsString(ds,idx,"headline"),DB.GetValAsString(ds,idx,"image"),DB.GetValAsString(ds,idx,"active"),DB.GetValAsString(ds,idx,"showinlist"),DB.GetValAsString(ds,idx,"preview"),DB.GetValAsString(ds,idx,"showinrss"),DB.GetValAsString(ds,idx,"newsdate"),DB.GetValAsString(ds,idx,"newsdate2"),DB.GetValAsString(ds,idx,"newslanguage"),DB.GetValAsString(ds,idx,"deleted"),DB.GetValAsString(ds,idx,"mainpropid"));
    DB.ExecSql(sql);
  }


}
