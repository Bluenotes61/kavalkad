/* $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ */
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


public partial class Properties : AdminBase {

  private string folderIcons = "obout/treeview/icons";
  private string folderStyle = "obout/treeview/style/Pages";
  private string folderScript = "obout/treeview/script/Pages";

  private String thisId = null;
  private PageProperty thisProp = null;

  public override int LeftWidth {
    get { return 250; }
  }

  public String ThisId {
    get {
      if (thisId == null)
        thisId = Request.QueryString["propertyid"];
      return thisId;
    }
    set {
      thisId = value;
    }
  }

  private PageProperty ThisProp {
    get {
      if (thisProp == null) {
        DataSet ds = DB.GetDS("select * from pageproperty where deleted=0 and id = " + ThisId);
        thisProp = new PageProperty(Cms, ds.Tables[0].Rows[0]);
      }
      return thisProp;
    }
  }

  protected void BindData() {
    BindPages();
    BindPropTree();
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    AjaxPro.Utility.RegisterTypeForAjax(typeof(Properties), this);

    BindData();
  }


  private void BindPages() {
    DataSet ds = DB.GetDS("select * from webpage where deleted=0 and mainprop = " + ThisProp.RootProp.Id + " or sharedprop = " + ThisProp.RootProp.Id);
    PageList.DataSource = ds.Tables[0].DefaultView;
    PageList.DataBind();
  }

  private void CreatePropNode(obout_ASPTreeView_2_NET.Tree oTree, String parent, String text, String id, String icon, bool locked) {
    String html;
    if (locked)
      html = "<span id='locked_" + id + "' style='color:red'>" + text + "</span>";
    else
      html = text;
    oTree.Add(parent, id, html, false, icon, null);

    DataSet ds = DB.GetDS("select * from pageproperty where deleted=0 and parentproperty=" + id + " order by orderno");
    for (int i=0; i < DB.GetRowCount(ds); i++)
      CreatePropNode(oTree, id, DB.GetString(ds, i, "id") + " - " + DB.GetString(ds, i, "propertyname"), DB.GetString(ds, i, "id"), "page.gif", DB.GetString(ds, i, "locked").Length > 0);
  }

  private void BindPropTree() {
    obout_ASPTreeView_2_NET.Tree oTree = new obout_ASPTreeView_2_NET.Tree();

    CreatePropNode(oTree, "root", ThisId + " - " + ThisProp.PropertyName, ThisId,"page.gif", ThisProp.IsLocked());

    oTree.FolderIcons = folderIcons;
    oTree.FolderStyle = folderStyle;
    oTree.FolderScript = folderScript;
    oTree.EditNodeEnable = false;
    oTree.DragAndDropEnable = false;
    oTree.MultiSelectEnable = false;

    oTree.SelectedId = ThisId;

    String tree = oTree.HTML();
    int idx = tree.IndexOf("<div style=\"font:11px verdana; background-color:white; border:3px solid #cccccc; color:#666666;");
    if (idx >= 0)
      tree = tree.Substring(0,idx);

    PropTree.Text = tree;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetPropInfo(String propid){

    try {
      ThisId = propid;

      if (ThisProp == null) return "";

      String prelimPropVal = "";
      String prelimPropValPre = "";
      if (!ThisProp.IsPublished) {
        StringBuilder aval = new StringBuilder(ThisProp.PrelimValue);
        aval = aval.Replace("<", "&lt;");
        aval = aval.Replace(">", "&gt;");
        prelimPropValPre = aval.ToString();
        prelimPropVal = ThisProp.PrelimValue;
      }
      StringBuilder aval2 = new StringBuilder(ThisProp.Value);
      aval2 = aval2.Replace("<", "&lt;");
      aval2 = aval2.Replace(">", "&gt;");
      String propValPre = aval2.ToString();
      String propVal = ThisProp.Value;
      bool locked = DB.GetString("select locked from pageproperty where id=" + ThisId, "locked").Length > 0;
      String parentlink = (ThisProp.Parent == null ? "" : "<a href='/admin/properties.aspx?propertyid=" + ThisProp.Parent.Id + "'>" + ThisProp.Parent.Id + "</a>");

      return ThisProp.Id.ToString() + "|" + ThisProp.PropertyName + "|" + parentlink + "|" + ThisProp.ControlType + "|" + ThisProp.OrderNo.ToString() + "|" + (ThisProp.IsVisible ? Translate("Ja") : Translate("Nej")) + "|" +
        (ThisProp.IsPublished ? "Ja" : "Nej") + "|" + GetDateString(ThisProp.ModDate) + "|" + GetUserName(ThisProp.ModBy) + "|" + GetDateString(ThisProp.PublishedDate) + "|" +
        GetUserName(ThisProp.PublishedBy) + "|" + GetDateString(ThisProp.StartDate) + "|" + GetDateString(ThisProp.EndDate) + "|" + prelimPropValPre + "|" + propValPre + "|" +
        prelimPropVal + "|" + propVal + "|" + locked.ToString().ToLower();
    }
    catch {
      return "";
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void SaveEdit(String vals) {

    String[] valsArr = vals.Split('|');
    String id = valsArr[0];
    String name = valsArr[1];
    String order = valsArr[2];
    String visible = valsArr[3];
    String publ = valsArr[4];
    String pstart = valsArr[5];
    String pend = valsArr[6];
    String prelimvalue = valsArr[7];
    String value = valsArr[8];

    ThisId = id;
    ThisProp.PropertyName = name;
    try { ThisProp.OrderNo = Convert.ToInt32(order); }
    catch { ThisProp.OrderNo = 0; }
    ThisProp.IsVisible = (visible.ToLower() == "true");
    ThisProp.IsPublished = (publ.ToLower() == "true");
    try { ThisProp.StartDate = DateTime.Parse(pstart); }
    catch {}
    try { ThisProp.EndDate = DateTime.Parse(pend); }
    catch {}
    ThisProp.PrelimValue = prelimvalue;
    ThisProp.Value = value;

    ThisProp.WriteToDB();
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public bool MoveNode(String id, String dir) {
    if (dir == "up") {
      DataSet ds = DB.GetDS("select parentproperty, orderno from pageproperty where id=" + id);
      int parent = DB.GetInt(ds, 0, "parentproperty");
      int oldorder = DB.GetInt(ds, 0, "orderno");
      String where = (parent > 0 ? "parentproperty=" + parent : "parentproperty is null");
      ds = DB.GetDS("select id, orderno from pageproperty where " + where + " and  orderno < " + oldorder + " order by orderno desc");
      if (DB.GetRowCount(ds) > 0) {
        int neworder = DB.GetInt(ds, 0, "orderno");
        int switchid = DB.GetInt(ds, 0, "id");
        DB.ExecSql("update pageproperty set orderno = " + oldorder + " where id=" + switchid);
        DB.ExecSql("update pageproperty set orderno = " + neworder + " where id=" + id);
        return true;
      }
      else return false;

    }
    else {
      DataSet ds = DB.GetDS("select parentproperty, orderno from pageproperty where id=" + id);
      int parent = DB.GetInt(ds, 0, "parentproperty");
      int oldorder = DB.GetInt(ds, 0, "orderno");
      String where = (parent > 0 ? "parentproperty=" + parent : "parentproperty is null");
      ds = DB.GetDS("select id, orderno from pageproperty where " + where + " and  orderno > " + oldorder + " order by orderno asc");
      if (DB.GetRowCount(ds) > 0) {
        int neworder = DB.GetInt(ds, 0, "orderno");
        int switchid = DB.GetInt(ds, 0, "id");
        DB.ExecSql("update pageproperty set orderno = " + oldorder + " where id=" + switchid);
        DB.ExecSql("update pageproperty set orderno = " + neworder + " where id=" + id);
        return true;
      }
      else return false;
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void DelProp(String id) {
    ThisId = id;
    ThisProp.Delete();
    ThisId = "";
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void UnlockProp(String id) {
    ThisId = id;
    ThisProp.Unlock();
    ThisId = "";
  }

}
