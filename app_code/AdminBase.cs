/* $Date: 2010-08-19 13:18:45 +0200 (to, 19 aug 2010) $    $Revision: 6800 $ */
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Collections;
using System.Configuration;
using System.Globalization;
using AjaxPro;
using OboutInc.Splitter2;
using Obout.Grid;
using NFN;

public partial class AdminBase : Page {

  protected CMS Cms {
    get { return (CMS)Page.Session["CMS"]; }
  }

  protected virtual Grid ObItemGrid {
    get { return null; }
  }

  protected virtual DataView ItemGridData {
    get { return null; }
  }

  public virtual String BWPageName {
    get { return ""; }
  }

  public virtual String BWPageUrl {
    get { return DB.GetString("select pageurl from backweb where name='" + BWPageName + "'", "pageurl"); }
  }

  public virtual int NofRows {
    get { 
      int nof = DB.GetInt("select nofrows from backweb where name='" + BWPageName + "'", "nofrows"); 
      return (nof == 0 ? 20 : nof);
    }
  }

  public virtual int LeftWidth {
    get { return 0; }
  }

  protected virtual bool UseObjectDataSource {
    get { return false; }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String Translate(String txt) {
    return Cms.Translate(txt);
  }

  protected virtual void SetGridProps() {
    ObItemGrid.FolderStyle = "/admin/obout/grid/bwstyle";
    ObItemGrid.AllowFiltering = true;
    ObItemGrid.AllowAddingRecords = true;
    ObItemGrid.AllowColumnResizing = true;
    ObItemGrid.AutoGenerateColumns = false;
    ObItemGrid.EnableTypeValidation = true;
    ObItemGrid.Serialize = false;
    ObItemGrid.FolderLocalization = "/admin/obout/grid/localization";
    ObItemGrid.Language = Cms.Language;
    ObItemGrid.CallbackMode = true;
    ObItemGrid.AllowGrouping = false;
    ObItemGrid.FolderExports = "/admin/exports";
    ObItemGrid.PageSize = NofRows;
    ObItemGrid.PageSizeOptions = "5,10,20,50,100,500";
  }

  protected override void OnLoad(EventArgs e) {
    AjaxPro.Utility.RegisterTypeForAjax(typeof(AdminBase), this);

    Session["CurrentBW"] = BWPageUrl;
    Splitter splitter = (Splitter)Master.FindControl("mainSplit");
    if (LeftWidth > 0)
      splitter.LeftPanel.WidthDefault = LeftWidth;
    else
      splitter.Visible = false;

    if (ObItemGrid != null) {
      if (!IsPostBack)
        SetGridProps();
      if (!UseObjectDataSource) {
        ObItemGrid.DataSource = ItemGridData;
        ObItemGrid.DataBind();
      }
    }
    base.OnLoad(e);
  }

  protected String GetUserName(int userid) {
    DataSet ds = DB.GetDS("select username from Users where id = " + userid);
    if (DB.GetRowCount(ds) > 0) return DB.GetString(ds, 0, "username");
    else return userid.ToString();
  }

  protected String GetDateString(DateTime aDate) {
    return (aDate == DateTime.MinValue ? "" : aDate.ToString());
  }

}



public partial class NewsBase : AdminBase {

  protected virtual String GroupId {
    get { return "news"; }
  }

  public override String BWPageName {
    get { return "News"; }
  }

  public override int LeftWidth {
    get { return 0; }
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
  }

  protected override void SetGridProps() {
    base.SetGridProps();
  }
  
  private String SubProp {
    get {
      return (Cms.Language == "sv" ? "text" : "text_" + Cms.Language);
    }
  }


  protected void DeleteRecord(object sender, GridRecordEventArgs e) {
    int propid = Convert.ToInt32(e.Record["id"]);
    PageProperty mainProp = Cms.GetPropertyById(propid);
    mainProp.Delete();
    DB.ExecSql("update news set deleted=1 where mainpropid='" + propid + "'");
  }

  protected void UpdateRecord(object sender, GridRecordEventArgs e) {
    int propid = Convert.ToInt32(e.Record["id"]);
    int sumpropid = Convert.ToInt32(e.Record["sumid"]);
    String date = e.Record["newsdate"].ToString().Length == 0 ? DateTime.Now.ToString(CMS.SiteSetting("dateFormat")) : DateTime.Parse(e.Record["newsdate"].ToString()).ToString(CMS.SiteSetting("dateTimeFormat"));
    String date2 = (e.Record["newsdate2"] == null || e.Record["newsdate2"].ToString().Length == 0 ? "null" : "'" + DateTime.Parse(e.Record["newsdate2"].ToString()).ToString(CMS.SiteSetting("dateTimeFormat")) + "'");
    String show = (Convert.ToBoolean(e.Record["showinlist"]) ? "1" : "0");
    String active = (Convert.ToBoolean(e.Record["active"]) ? "1" : "0");
    String preview = (Convert.ToBoolean(e.Record["preview"]) ? "1" : "0");
    String rss = (Convert.ToBoolean(e.Record["showinrss"]) ? "1" : "0");
    DB.ExecSql("update news set newsdate='" + date + "', newsdate2=" + date2 + ", headline='" + e.Record["headline"] + "', showinlist=" + show + ", active=" + active + ", preview=" + preview + ", showinrss=" + rss + ", image='" + e.Record["image"] + "' where mainpropid=" + propid);

    PageProperty sumProp = Cms.GetPropertyById(sumpropid);
    if (sumProp != null) {
      sumProp.GetProperty(SubProp).PrelimValue = e.Record["summary"].ToString();
      sumProp.Publish(false);
      sumProp.WriteToDB();
    }

    PageProperty mainProp = Cms.GetPropertyById(propid);
    mainProp.GetProperty(SubProp).PrelimValue = e.Record["content"].ToString();
    mainProp.Publish(false);
    mainProp.WriteToDB();
  }

  protected void InsertRecord(object sender, GridRecordEventArgs e) {
    DB.ExecSql("insert into pageproperty (propertyname, controltype, visible) values ('NewsProp', 'tiny', 1)");
    int mainPropId = DB.GetInt("select top 1 id from pageproperty order by id desc", "id");
    DB.ExecSql("insert into pageproperty (propertyname, controltype, visible) values ('NewsPropSum', 'tiny', 1)");
    int sumPropId = DB.GetInt("select top 1 id from pageproperty order by id desc", "id");

    String date = e.Record["newsdate"].ToString().Length == 0 ? DateTime.Now.ToString(CMS.SiteSetting("dateFormat")) : DateTime.Parse(e.Record["newsdate"].ToString()).ToString(CMS.SiteSetting("dateTimeFormat"));
    String date2 = (e.Record["newsdate2"] == null || e.Record["newsdate2"].ToString().Length == 0 ? "null" : "'" + DateTime.Parse(e.Record["newsdate2"].ToString()).ToString(CMS.SiteSetting("dateTimeFormat")) + "'");
    String show = (Convert.ToBoolean(e.Record["showinlist"]) ? "1" : "0");
    String active = (Convert.ToBoolean(e.Record["active"]) ? "1" : "0");
    String preview = (Convert.ToBoolean(e.Record["preview"]) ? "1" : "0");
    String rss = (Convert.ToBoolean(e.Record["showinrss"]) ? "1" : "0");
    DB.ExecSql("insert into news (groupid, mainpropid, summarypropid, headline, showinlist, active, preview, showinrss, newsdate, newsdate2, newslanguage, image) values ('" + GroupId + "', " + mainPropId + ", " + sumPropId + ", '" + e.Record["headline"] + "', " + show + ", " + active + ", " + preview + ", " + rss + ", '" + date + "'," + date2 + ", '" + Cms.Language + "', '" + e.Record["image"] + "')");


    PageProperty mainProp = Cms.GetPropertyById(mainPropId);
    mainProp.GetProperty(SubProp).PrelimValue = e.Record["content"].ToString();
    mainProp.Publish(false);
    mainProp.WriteToDB();

    PageProperty sumProp = Cms.GetPropertyById(sumPropId);
    sumProp.GetProperty(SubProp).PrelimValue = e.Record["summary"].ToString();
    sumProp.Publish(false);
    sumProp.WriteToDB();
  }


  protected override DataView ItemGridData {
    get {
      DataTable dt = new DataTable();
      DataRow dr;

      dt.Columns.Add(new DataColumn("id", typeof(int)));
      dt.Columns.Add(new DataColumn("sumid", typeof(int)));
      dt.Columns.Add(new DataColumn("headline", typeof(string)));
      dt.Columns.Add(new DataColumn("summary", typeof(string)));
      dt.Columns.Add(new DataColumn("content", typeof(string)));
      dt.Columns.Add(new DataColumn("newsdate", typeof(string)));
      dt.Columns.Add(new DataColumn("newsdate2", typeof(string)));
      dt.Columns.Add(new DataColumn("active", typeof(bool)));
      dt.Columns.Add(new DataColumn("showinlist", typeof(bool)));
      dt.Columns.Add(new DataColumn("showinrss", typeof(bool)));
      dt.Columns.Add(new DataColumn("preview", typeof(bool)));
      dt.Columns.Add(new DataColumn("image", typeof(string)));

      DataSet ds = DB.GetDS("select * from news where deleted=0 and groupid='" + GroupId + "' and newslanguage='" + Cms.Language + "' order by newsdate desc");
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        dr = dt.NewRow();
        int mainPropId = DB.GetInt(ds, i, "mainpropid");
        PageProperty mainProp = Cms.GetPropertyById(mainPropId);
        int sumPropId = DB.GetInt(ds, i, "summarypropid");
        PageProperty sumProp = Cms.GetPropertyById(sumPropId);

        dr[0] = mainPropId;
        dr[1] = sumPropId;
        dr[2] = DB.GetString(ds, i, "headline");
        dr[3] = sumProp.GetProperty(SubProp).Value;
        dr[4] = mainProp.GetProperty(SubProp).Value;
        dr[5] = DB.GetDate(ds, i, "newsdate").ToString("yyyy-MM-dd HH:mm");
        dr[6] = DB.GetDate(ds, i, "newsdate2").ToString("yyyy-MM-dd HH:mm");
        dr[7] = DB.GetBoolean(ds, i, "active");
        dr[8] = DB.GetBoolean(ds, i, "showinlist");
        dr[9] = DB.GetBoolean(ds, i, "showinrss");
        dr[10] = DB.GetBoolean(ds, i, "preview");
        dr[11] = DB.GetString(ds, i, "image");
        dt.Rows.Add(dr);
      }
      return dt.DefaultView;
    }
  }

}

