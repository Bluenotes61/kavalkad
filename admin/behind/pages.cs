/* $Date: 2011-04-17 14:23:15 +0200 (sÃ¶, 17 apr 2011) $    $Revision: 7601 $ */
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using System.Collections;
using System.Text;
using System.Configuration;
using obout_ASPTreeView_2_NET;
using NFN;
using AjaxPro;


public partial class Pages : AdminBase {

  private String thisId = null;
  private WebPage thisPage = null;

  private string folderIcons = "obout/treeview/icons";
  private string folderStyle = "obout/treeview/style/Pages";
  private string folderScript = "obout/treeview/script/Pages";

  public override int LeftWidth {
    get { return 250; }
  }

  private WebPage ThisPage {
    get {
      if (thisPage == null) {
        if (ThisId.Length > 0) {
          DataSet ds = DB.GetDS("select * from webpage where deleted=0 and id = '" + ThisId + "'");
          if (DB.GetRowCount(ds) > 0)
            thisPage = new WebPage(Cms, ds.Tables[0].Rows[0]);
        }
      }
      return thisPage;
    }
  }

  public String ThisId {
    get {
      if (thisId == null || thisId.Length == 0)
        thisId = Request.QueryString["pageid"];
      if ((thisId == null || thisId.Length == 0) && Session["CurrentPage"] != null)
        thisId = Session["CurrentPage"].ToString();
      if (thisId == null || thisId.Length == 0)
        thisId = CMS.SiteSetting("defaultPage");
      return thisId;
    }
    set {
      Session["CurrentPage"] = value;
      thisId = value;
    }
  }

  public override String BWPageName {
    get { return "Pages"; }
  }


  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    AjaxPro.Utility.RegisterTypeForAjax(typeof(Pages), this);
    
    BtnRestorePage.Visible = Cms.GetPermission("DEFAULT", "WebPage", "Restore");
    BtnUndeletePage.Visible = Cms.GetPermission("DEFAULT", "WebPage", "Restore");

    BindPageTree();
    BindPageTemplates();
    BindPermissions();
    BindLanguages();
  }

  private void BindLanguages() {
    String html_new = "<ul>";
    String html_edit = "<ul>";
    for (int i = 0; i < Cms.Languages.Length; i++) {
      html_new += "<li style='display:inline;margin-right:20px'><input type='checkbox' " + (i == 0 ? "checked" : "") + " name='chklang_new' id='" + Cms.Languages[i] + "_new' value='" + Cms.Languages[i] + "' /><label for='" + Cms.Languages[i] + "_new'>" + Cms.Languages[i] + "</label></li>";
      html_edit += "<li style='display:inline;margin-right:20px'><input type='checkbox' name='chklang_edit' id='" + Cms.Languages[i] + "_edit' value='" + Cms.Languages[i] + "' /><label for='" + Cms.Languages[i] + "_edit'>" + Cms.Languages[i] + "</label></li>";
    }
    ChkLangN.Text = html_new + "</ul>";
    ChkLangE.Text = html_edit + "</ul>";
  }

  private void BindPermissions() {

    StringBuilder html = new StringBuilder();
    foreach (String aRole in Cms.Roles) {
      PermissionRoles.Items.Add(aRole);

      String disp = (html.Length == 0 ? "block" : "none");
      html.Append("<div id='p_" + aRole + "' style='display:" + disp + "'>");
      html.Append("<table cellpadding='0' cellspacing='3'><tr><td><h3>" + Translate("Egenskap") + "</h3></td><td><h3>" + Translate("Behörighet") + "</h3></td></tr>");

      foreach (String aAction in ThisPage.Actions) {
        String[] help = aAction.Split(';');
        String aPermission = (ThisPage.GetRolePermission(aRole, help[0]) ? "Y" : "N");

        html.Append("<tr><td>" + Translate(help[1]) + ": </td><td><ul><li style='display:inline'><input id='p_" + help[0] + "_" + aRole + "_Y' type='radio' name='p_" + help[0] + "_" + aRole + "_V' value='Y'/><label for='p_" + aRole + "_Y'>" + Translate("Ja") + "</label></li>");
        html.Append("<li style='display:inline'><input id='p_" + help[0] + "_" + aRole + "_N' type='radio' name='p_" + help[0] + "_" + aRole + "_V' value='N' /><label for='p_" + aRole + "_N'>" + Translate("Nej") + "</label></li></ul></td></tr>");
      }
      html.Append("</table></div>");
    }
    PermissionDivE.InnerHtml = html.ToString();
  }


  private void BindPageTemplates() {
    String tmplEditTr1 = "";
    String tmplEditTr2 = "";
    DataSet ds = DB.GetDS("select * from webpagetemplate order by orderno");
    for (int j=0; j < DB.GetRowCount(ds); j++) {
      String fname = DB.GetString(ds, j, "filename");
      String descr = DB.GetString(ds, j, "name");
      String icon = DB.GetString(ds, j, "icon");
      if (icon.Length == 0) icon = "gfx/onearea.gif";

      tmplEditTr1 += "<td align='center'><label for='tr_" + j + "'><img src='" + icon + "' alt='" + descr + " - " + fname + "' title='" + descr + " - " + fname + "' /><br />" + descr + "</label></td>";
      tmplEditTr2 += "<td align='center'><input name='tnmplrn' onclick='pageTmplNChanged(this)' type='radio' id='trn_" + j + "' value='" + fname + "'></td>";
    }
    tmplEditTr1 += "<td align='center'><input type='text' id='trn_xi' size='20' onfocus='document.getElementById(\"trn_x\").checked=\"true\";pageTmplNChanged(this)' onchange='pageTmplNChanged(this)' /></td>";
    tmplEditTr2 += "<td align='center'><input id='trn_x' name='tnmplrn' onclick='pageTmplNChanged(document.getElementById(\"trn_xi\"))' type='radio' value=''></td>";
    PageTemplateN.Text = "<table cellspacing='10'><tr>" + tmplEditTr1 + "</tr><tr>" + tmplEditTr2 + "</tr></table>";
  }

  private void CreateNode(obout_ASPTreeView_2_NET.Tree oTree, String parent, String name, String id) {
    oTree.Add(parent, id.ToString(), name, false, "ie_link.gif", null);
    DataSet ds = DB.GetDS("select * from webpage where deleted=0 and parentpage='" + id + "' order by orderno");
    for (int i=0; i < DB.GetRowCount(ds); i++) 
      CreateNode(oTree, id, DB.GetString(ds, i, "name"), DB.GetString(ds, i, "id"));
  }

  private void BindPageTree() {

    obout_ASPTreeView_2_NET.Tree oTree = new obout_ASPTreeView_2_NET.Tree();
    oTree.id = "PageTree";
    oTree.Add("root", "_root_", Translate("Webbsidor"), true, "xpMyComp.gif", null);

    DataSet ds = DB.GetDS("select * from webpage where (parentpage is null or parentpage='') and deleted=0 order by orderno");
    for (int i=0; i < DB.GetRowCount(ds); i++) 
//      CreateNode(oTree, "_root_", DB.GetString(ds, i, "name"), DB.GetString(ds, i, "id"));
      oTree.Add("_root_", DB.GetString(ds, i, "id"), DB.GetString(ds, i, "name"), false, "ie_link.gif", "DynamicLoad.aspx?id=" + DB.GetString(ds, i, "id"));


    oTree.FolderIcons = folderIcons;
    oTree.FolderStyle = folderStyle;
    oTree.FolderScript = folderScript;
    oTree.EditNodeEnable = false;
    oTree.DragAndDropEnable = true;
    oTree.MultiSelectEnable = false;

    oTree.SelectedId = GetFullNodePath(ThisId, "");

    String tree = oTree.HTML();
    int idx = tree.IndexOf("<div style=\"font:11px verdana; background-color:white; border:3px solid #cccccc; color:#666666;");
    if (idx >= 0)
      tree = tree.Substring(0,idx);

    PageTree.Text = tree;
  }



  public String GetFullNodePath(String pid, String prefix) {
    ArrayList nodes = new ArrayList();
    nodes.Add(pid);

    String parentPage = DB.GetString("select parentpage from webpage where deleted=0 and id='" + pid + "'", "parentpage");
    while (pid != "_root_" && parentPage.Length > 0) {
      nodes.Add(parentPage);
      parentPage = DB.GetString("select parentpage from webpage where deleted=0 and id='" + parentPage + "'", "parentpage");
    }

    String nodeStr = "";
    for (int i=nodes.Count-1; i >= 0; i--)
      nodeStr += "," + prefix + nodes[i].ToString();
    return nodeStr.Substring(1);
  }

  private String GetPageAttributeHtml(String pageId) {
    WebPage aPage = Cms.GetPageById(pageId);
    String htmlv = "";
    String htmle = "";
    String[][] pageAttributes = aPage.GetPageAttributes();
    for (int i = 0; i < pageAttributes.Length; i++) {
      String id = pageAttributes[i][0];
      String name = pageAttributes[i][1];
      String type = pageAttributes[i][2];
      String alt = pageAttributes[i][3];
      String value = aPage.GetPageAttributeValue(id);
      String tvalue = value;
      String inp = "";
      if (type == "bool") {
        inp = "<input type='checkbox' id='{0}' name='{1}' " + (value == "true" ? "checked" : "") + " />";
        tvalue = (tvalue == "true" ? Translate("Ja") : Translate("Nej"));
      }
      else if (alt.Length > 0) {
        String[] altarr = alt.Split(';');
        inp = "<select id='{0}' name='{1}'>";
        for (int j = 0; j < altarr.Length; j++)
          inp += "<option value='" + altarr[j] + "' " + (value == altarr[j] ? "selected" : "") + ">" + altarr[j] + "</option>";
        inp += "</select>";
      }
      else
        inp = "<input type='text' id='{0}' name='{1}' value='" + value + "' />";
      htmlv += "<tr class='attrib_row'><td class='hl'>" + Translate(name) + ":</td><td  class='value'><span id='attribv_" + id + "'></span>" + tvalue + "</td></tr>";
      htmle += "<tr class='attrib_row'><td class='hl'>" + Translate(name) + ":</td><td class='editvalue'>" + String.Format(inp, "attribe_" + id, "pattrib_e") + "</td></tr>";
    }
    return htmlv + "|" + htmle;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String[] GetPageInfo(String pageid){

    ThisId = pageid;

    if (ThisPage == null) return null;

    String propText = ThisPage.MainProp.PropertyName + " (" + ThisPage.MainProp.Id.ToString() + ")";
    String propLink = "<a href='properties.aspx?propertyid=" + ThisPage.MainProp.Id.ToString() + "'>" + propText + "</a>";
    String sPropText = "";
    String sPropLink = "";
    String sharedPages = "";
    String firstShared = "";
    String relPages = "";
    if (ThisPage.SharedProp != null) {
      sPropText = ThisPage.SharedProp.PropertyName + " (" + ThisPage.SharedProp.Id.ToString() + ")";
      sPropLink = "<a href='properties.aspx?propertyid=" + ThisPage.SharedProp.Id.ToString() + "'>" + sPropText + "</a>";

      DataSet ds0 = DB.GetDS("select id, name from webpage where deleted=0 and id <> '" + ThisPage.Id + "' and sharedprop=" + ThisPage.SharedProp.Id);
      for (int i=0; i < DB.GetRowCount(ds0); i++) {
        if (sharedPages.Length > 0) sharedPages += "<br />";
        sharedPages += "<a href='/" + DB.GetString(ds0, i, "id") + ".aspx'>" + DB.GetString(ds0, i, "name") + " (" + DB.GetString(ds0, i, "id") + ")</a>";
        if (firstShared.Length == 0) firstShared = GetFullNodePath(DB.GetString(ds0, i, "id"), "_s_");
      }
    }
    
    for (int i=0; i < ThisPage.RelatedPages.Length; i++) {
      if (relPages.Length > 0) relPages += "<br />";
      relPages += "<a href='/" + ThisPage.RelatedPages[i].Id + ".aspx'>" + ThisPage.RelatedPages[i].Name + " (" + ThisPage.RelatedPages[i].Id + ")</a>";
    }

    String permissions = "";
    DataSet ds = DB.GetDS("select * from viewpermissions where itemtype='WebPage' and id='" + ThisPage.Id + "' order by role");
    for (int j=0; j < DB.GetRowCount(ds); j++) {
      if (permissions.Length > 0) permissions += "<br>";
      permissions += DB.GetString(ds, j, "role") + " - " + DB.GetString(ds, j, "actiontype") + " - " + DB.GetString(ds, j, "permission");
    }

    String permArr = "";
    ds = DB.GetDS("select * from permissiontypes where itemtype='WebPage'");
    for (int j=0; j < DB.GetRowCount(ds); j++) {
      foreach (String role in Cms.Roles) {
        if (permArr.Length > 0) permArr += ";";
        String action = DB.GetString(ds, j, "actiontype");
        permArr += role + "!" + action + "!" + (ThisPage.GetRolePermission(role,action) ? "Y" : "N");
      }
    }

    String tmplEditTr1 = "";
    String tmplEditTr2 = "";
    String tmplText = "";
    String tmplImg = "";
    ds = DB.GetDS("select * from webpagetemplate order by orderno");
    for (int j=0; j < DB.GetRowCount(ds); j++) {
      String fname = DB.GetString(ds, j, "filename");
      String descr = DB.GetString(ds, j, "name");
      String icon = DB.GetString(ds, j, "icon");
      if (icon.Length == 0) icon = "gfx/onearea.gif";
      bool ischecked = (fname == ThisPage.Filename);
      if (ischecked) {
        tmplText = descr;
        tmplImg = icon;
      }
      tmplEditTr1 += "<td align='center'><label for='tr_" + j + "'><img src='" + icon + "' alt='" + descr + " - " + fname + "' title='" + descr + " - " + fname + "' /><br />" + descr + "</label></td>";
      tmplEditTr2 += "<td align='center'><input name='tnmplr' " + (ischecked ? "checked" : "") + " onclick='pageTmplEChanged(this)' type='radio' id='tr_" + j + "' value='" + fname + "'></td>";
    }
    tmplEditTr1 += "<td align='center'><input type='text' id='tr_xi' size='20' value='" + (tmplText.Length == 0 ? ThisPage.Filename : "") + "' onfocus='document.getElementById(\"tr_x\").checked=\"true\";pageTmplEChanged(this)' onchange='pageTmplEChanged(this)' /></td>";
    tmplEditTr2 += "<td align='center'><input id='tr_x' name='tnmplr' onclick='pageTmplEChanged(document.getElementById(\"tr_xi\"))' type='radio' " + (tmplText.Length == 0 ? "checked" : "") + " value=''></td>";
    if (tmplText.Length == 0) tmplText = ThisPage.Filename;
    if (tmplImg.Length == 0) tmplImg = "gfx/onearea.gif";
    String tmplEdit = "<table cellspacing='10'><tr>" + tmplEditTr1 + "</tr><tr>" + tmplEditTr2 + "</tr></table>";

    bool locked = PropIsLocked(ThisPage.MainProp.Id) || PropIsLocked(ThisPage.SharedProp.Id);

    String protection = (ThisPage.Protection == "Y" ? Translate("Ja") : (ThisPage.Protection == "N" ? Translate("Nej") : Translate("Ärvd")));

    String[] res = new String[33];
    res[0] = ThisPage.Id;
    res[1] = ThisPage.Name;
    res[2] = GetDateString(ThisPage.ModDate);
    res[3] = GetUserName(ThisPage.ModBy);
    res[4] = ThisPage.Filename;
    res[5] = tmplText;
    res[6] = tmplImg;
    res[7] = protection;
    res[8] = ThisPage.OrderNo.ToString();
    res[9] = GetStatusString(ThisPage.Status);
    res[10] = ThisPage.Languages;
    res[11] = propLink;
    res[12] = sPropLink;
    res[13] = sharedPages;
    res[14] = relPages;
    res[15] = (ThisPage.RedirToChild ? Translate("Ja") : Translate("Nej"));
    res[16] = (ThisPage.AllowQuickChildren ? Translate("Ja") : Translate("Nej"));
    res[17] = ThisPage.Redirect;
    res[18] = ThisPage.Title;
    res[19] = ThisPage.KeyWords;
    res[20] = ThisPage.Description;
    res[21] = (ThisPage.TimeControlled ? Translate("Ja") : Translate("Nej"));
    res[22] = ThisPage.StartTime.ToString("yyyy-MM-dd HH:mm");
    res[23] = ThisPage.EndTime.ToString("yyyy-MM-dd HH:mm");
    res[24] = permissions;
    res[25] = tmplEdit;
    res[26] = ThisPage.Status;
    res[27] = propText;
    res[28] = sPropText;
    res[29] = firstShared;
    res[30] = permArr;
    res[31] = locked.ToString().ToLower();
    res[32] = GetPageAttributeHtml(ThisPage.Id);

    return res;
  }

  private bool PropIsLocked(int propid) {
    bool isLocked = DB.GetString("select locked from pageproperty where id=" + propid, "locked").Length > 0;
    if (!isLocked) {
      DataSet ds = DB.GetDS("select id from pageproperty where parentproperty=" + propid);
      for (int i=0; i < DB.GetRowCount(ds) && !isLocked; i++)
        isLocked = PropIsLocked(DB.GetInt(ds, i, "id"));
    }
    return isLocked;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void UnlockProp(String pageid) {
    int propid = DB.GetInt("select mainprop from webpage where deleted=0 and id='" + pageid + "'", "mainprop");
    DoUnlock(propid);
    propid = DB.GetInt("select sharedprop from webpage where deleted=0 and id='" + pageid + "'", "sharedprop");
    DoUnlock(propid);
  }

  private void DoUnlock(int propid) {
    DB.ExecSql("update pageproperty set locked='' where id=" + propid);
    DataSet ds = DB.GetDS("select id from pageproperty where parentproperty=" + propid);
    for (int i=0; i < DB.GetRowCount(ds); i++)
      DoUnlock(DB.GetInt(ds, i, "id"));
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void NodeMoved(String src, String dst){
    String parent = (dst == "_root_" ? "null" : "'" + dst + "'");
    int order = DB.GetInt("select max(orderno) as maxno from webpage where deleted=0 and parentpage=" + parent, "maxno") + 1;
    Cms.LogEvent("movepage", src);
    DB.ExecSql("update webpage set parentpage=" + parent + ", orderno=" + order + " where id='" + src + "'");
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetSharedTree(String pageid) {
    WebPage thispage = Cms.GetPageById(pageid);
    ArrayList sharedPages = new ArrayList();
    String firstShared = "";
    DataSet ds0 = DB.GetDS("select id from webpage where deleted=0 and id <> '" + thispage.Id + "' and sharedprop=" + thispage.SharedProp.Id);
    for (int i=0; i < DB.GetRowCount(ds0); i++) {
      sharedPages.Add(DB.GetString(ds0, i, "id"));
      if (firstShared.Length == 0) firstShared = DB.GetString(ds0, i, "id");
    }

    String html = "<a href='javascript:clearShared()' onfocus='this.blur()'>" + Translate("Rensa") + "</a><ul id='sharedpages' class='filetree'>";
    DataSet ds = DB.GetDS("select * from webpage where deleted=0 and (parentpage is null or parentpage='') order by orderno");
    for (int i=0; i < DB.GetRowCount(ds); i++) 
      html += GetSharedNode(DB.GetString(ds, i, "id"), DB.GetString(ds, i, "name"), sharedPages, firstShared, pageid);
    html += "</ul>";
    return html;
  }
  
  private String GetSharedNode(String pageid, String pagename, ArrayList sharedPages, String firstShared, String currpage) {
    bool isshared = false;
    for (int i=0; i < sharedPages.Count && !isshared; i++)
      isshared = (pageid == sharedPages[i].ToString());
    bool isfirstshared = (pageid == firstShared);
  
    String html = (isshared ? "<li class='selected'>" : "<li>");
    DataSet ds = DB.GetDS("select * from webpage where deleted=0 and parentpage='" + pageid + "' order by orderno");
    String input = (pageid == currpage ? "<span class='file'>" + pagename + "</span>" : "<span class='file'><label for='sp_" + pageid + "'>" + pagename + "</label><input name='sp_rb' type='radio' id='sp_" + pageid + "' onclick='sharedClicked(this)' " + (isfirstshared ? "checked" : "") +  "/></span>");
    if (DB.GetRowCount(ds) == 0)
      html += input;
    else {
      html += "<div class='hitarea collapsable-hitarea'></div>" + input + "<ul>";
      for (int i=0; i < DB.GetRowCount(ds); i++) 
        html += GetSharedNode(DB.GetString(ds, i, "id"), DB.GetString(ds, i, "name"), sharedPages, firstShared, currpage);
      html += "</ul>";
    }
    html += "</li>";
    return html;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetRelatedTree(String pageid) {
    WebPage apage = Cms.GetPageById(pageid);
    return apage.GetRelatedPagesTree();
  }


  private String GetStatusString(String astatus) {
    if (astatus == "active") return Translate("Aktiv");
    else if (astatus == "inactive") return Translate("Inaktiv");
    else if (astatus == "hidden") return Translate("Dold");
    else return "";
  }


  private String GetSubIds(String ids, String propid) {
    DataSet ds = DB.GetDS("select id from pageproperty where parentproperty=" + propid);
    for (int i=0; i < DB.GetRowCount(ds); i++)
      ids = GetSubIds(ids, DB.GetString(ds, i, "id"));
    ids += "," + propid;
    return ids;
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetRestoreHtml(String id) {
    String mainProp = DB.GetString("select mainprop from webpage where id='" + id + "'", "mainprop");
    String ids = "";
    ids = GetSubIds(ids, mainProp).Substring(1);
    String sql = @"
      select id, changedate, 'Innehåll' as type from a_pageproperty where propid in ({0}) union
      select id, changedate, 'Sida' as type from a_webpage where pageid='{1}'
      order by changedate desc";
    DataSet ds = DB.GetDS(String.Format(sql, ids, id));
    String html = "";
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String datestr = DB.GetDate(ds, i, "changedate").ToString("yyyy-MM-dd HH:mm:ss");
      html += "<div onMouseOver='this.parentNode.style.display=\"block\"' ><a href='javascript:void(0)' onClick='doRestore(\"" + datestr + "\")' onfocus='this.blur()'>" + datestr + "&nbsp;&nbsp;" + DB.GetString(ds, i, "type") + "</a></div>";
    }

    return html;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetUndeleteHtml() {
    DataSet ds = DB.GetDS("select id, name from webpage where deleted=1 order by name");
    String html = "";
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      html += "<div onMouseOver='this.parentNode.style.display=\"block\"' ><a href='javascript:void(0)' onClick='doUndelete(\"" + DB.GetString(ds, i, "id") + "\")' onfocus='this.blur()'>" + DB.GetString(ds, i, "name") + "</a></div>";
    }
    return html;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String DoRestore(String id, String date) {
    String mainProp = DB.GetString("select mainprop from webpage where id='" + id + "'", "mainprop");
    String rids = "";
    rids = GetSubIds(rids, mainProp).Substring(1);

    DataSet ds = DB.GetDS("select * from a_pageproperty where propid in (" + rids + ") and changedate >= '" + date + "' order by changedate asc");
    ArrayList ids = new ArrayList();
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String currId = DB.GetString(ds, i, "propid");
      if (!ids.Contains(currId)) {
        ids.Add(currId);
        Cms.RestoreProp(ds, i);
      }
    }

    ds = DB.GetDS("select * from a_webpage where pageid='" + id + "' and changedate >= '" + date + "' order by changedate asc");
    ids.Clear();
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String currId = DB.GetString(ds, i, "pageid");
      if (!ids.Contains(currId)) {
        ids.Add(currId);
        Cms.RestorePage(ds, i);
      }
    }
    return date;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void DoUndelete(String id) {
    String mainProp = DB.GetString("select mainprop from webpage where id='" + id + "'", "mainprop");
    String rids = "";
    rids = GetSubIds(rids, mainProp).Substring(1);

    DataSet ds = DB.GetDS("select * from a_pageproperty where propid in (" + rids + ") order by changedate asc");
    ArrayList ids = new ArrayList();
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String currId = DB.GetString(ds, i, "propid");
      if (!ids.Contains(currId)) {
        ids.Add(currId);
        Cms.RestoreProp(ds, i);
      }
    }
    String sharedProp = DB.GetString("select sharedprop from webpage where id='" + id + "'", "sharedprop");
    rids = GetSubIds(rids, sharedProp);
    DB.ExecSql("update pageproperty set deleted=0 where id in (" + rids + ")");

    ds = DB.GetDS("select * from a_webpage where pageid='" + id + "' order by changedate asc");
    ids.Clear();
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String currId = DB.GetString(ds, i, "pageid");
      if (!ids.Contains(currId)) {
        ids.Add(currId);
        Cms.RestorePage(ds, i);
      }
    }
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String SaveEdit(String[] valsArr) {

    String id = valsArr[0];
    String name = valsArr[1];
    String filename = valsArr[2];
    String protect = valsArr[3];
    String status = valsArr[4];
    String langs = valsArr[5];
    String shared = valsArr[6];
    String related = valsArr[7];
    String redir = valsArr[8];
    String quickcreate = valsArr[9];
    String redirto = valsArr[10];
    String title = valsArr[11];
    String keys = valsArr[12];
    String descript = valsArr[13];
    String timecontrol = valsArr[14];
    String timestart = valsArr[15];
    String timeend = valsArr[16];
    String pattrib = valsArr[17];
    String perm = valsArr[18];

    String fname = filename.Split('?')[0];
    if (!File.Exists(Server.MapPath("~/" + fname)))
      return Translate("Fel") + ": " + Translate("Mallsida med filnamn") + " " + fname + " " + Translate("existerar inte") + ".";

    Cms.LogEvent("savepage", id);

    ThisId = id;
    ThisPage.Name = name;
    ThisPage.Filename = filename;
    ThisPage.PrelimFilename = filename;
    ThisPage.Protection = protect;
    ThisPage.Status = status;
    ThisPage.Languages = langs;
    ThisPage.Redirect = redirto;
    ThisPage.RedirToChild = (redir.ToLower() == "true");
    ThisPage.Title = title;
    ThisPage.AllowQuickChildren = (quickcreate.ToLower() == "true");
    ThisPage.KeyWords = keys;
    ThisPage.Description = descript;
    ThisPage.TimeControlled = (timecontrol.ToLower() == "true");
    try {
      ThisPage.StartTime = DateTime.Parse(timestart);
      ThisPage.EndTime = DateTime.Parse(timeend);
    }
    catch {
      ThisPage.TimeControlled = false;
    }

    if (pattrib.Length > 0) {
      String[] parr = pattrib.Split(';');
      for (int i = 0; i < parr.Length; i++) {
        String[] pa = parr[i].Split('|');
        ThisPage.SavePageAttributeValue(pa[0], pa[1]);
      }
    }

    if (perm.Length > 0) {
      String[] parr = perm.Split('|');
      for (int i = 0; i < parr.Length; i++) {
        String[] aperm = parr[i].Split('_');
        String action = aperm[1];
        String role = aperm[2];
        for (int j = 3; j < aperm.Length - 1; j++)
          role += "_" + aperm[j];
        String currPerm = aperm[aperm.Length - 1];
        ThisPage.SetRolePermission(role, action, currPerm == "Y");
      }
    }

    int nofShared = DB.GetInt("select count(*) as nof from webpage where deleted=0 and id <> '" + id + "' and sharedprop=" + ThisPage.SharedProp.Id, "nof");
    if (shared.Length > 0) {
      WebPage sharedPage = Cms.GetPageById(shared);
      if (ThisPage.SharedProp.Id != sharedPage.SharedProp.Id) {
        if (ThisPage.SharedProp != null && nofShared == 0)
          ThisPage.SharedProp.Delete();
        ThisPage.SharedProp = sharedPage.SharedProp;
      }
    }
    else if (nofShared > 0) {
      PageProperty aProp = ThisPage.SharedProp.Clone(ThisPage.SharedProp.Parent);
      aProp.PropertyName = ThisPage.Name;
      ThisPage.SharedProp = ThisPage.SharedProp.Clone(ThisPage.SharedProp.Parent);
    }
    
    DB.ExecSql("delete from relatedpages where page1='" + ThisPage.Id + "'");
    if (related.Length > 0) {
      String[] relarr = related.Split('|');
      for (int i=0; i < relarr.Length; i++)
        DB.ExecSql("insert into relatedpages (page1, page2) values('" + ThisPage.Id + "', '" + relarr[i] + "')");
    }

    ThisPage.WriteToDB();

    return "";
  }



  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String SaveNewPage(String[] valsArr) {
    
    String id = valsArr[0];
    String parent = valsArr[1];
    String name = valsArr[2];
    String filename = valsArr[3];
    String protect = valsArr[4];
    String status = valsArr[5];
    String langs = valsArr[6];
    String redir = valsArr[7];
    String quickcreate = valsArr[8];
    String redirto = valsArr[9];
    String title = valsArr[10];
    String keys = valsArr[11];
    String descript = valsArr[12];
    String timecontrol = valsArr[13];
    String timestart = valsArr[14];
    String timeend = valsArr[15];
    String pattrib = valsArr[16];

    if (id.ToLower().IndexOfAny(new char[]{'å','ä','ö','/','&','%','?','#','\\',';'}) != -1 )
      return "Fel: Sidans identifierare kan inte innehålla tecken såsom å,ä,ö,/,&,%,?,#.";
    String fname = filename.Split('?')[0];
    if (!File.Exists(Server.MapPath("~/" + fname)))
      return Translate("Fel") + ": " + Translate("Mallsida med filnamn") + " " + fname + " " + Translate("existerar inte") + ".";
    DataSet ds = DB.GetDS("select deleted from webpage where id='" + id + "'");
    if (DB.GetRowCount(ds) > 0) {
      if (DB.GetBoolean(ds, 0, "deleted")) {
        DB.ExecSql("delete from webpage where id='" + id + "'");
      }
      else
        return Translate("Fel") + ": " + Translate("En sida med identifieraren") + " " + id + " " + Translate("existerar redan. Välj annan identifierare för sidan") + ".";
    }

    String sql = "select max(orderno) as maxno from webpage where deleted=0 and parentpage" + (parent == "_root_" ? " is null" : "='" + parent + "'");
    int nextorder = DB.GetInt(sql, "maxno") + 1;

    Cms.LogEvent("createpage", id);

    String mess = Cms.CreatePage(id, parent, name, title, filename, status, langs, false, keys, descript, nextorder.ToString(), protect, (redir.ToLower() == "true"), redirto, null);
    if (mess.Length == 0) {
      WebPage aPage = Cms.GetPageById(id);
      aPage.AllowQuickChildren = (quickcreate.ToLower() == "true");
      
      if (timecontrol.ToLower() == "true") {
        try {
          DateTime s = DateTime.Parse(timestart);
          DateTime e = DateTime.Parse(timeend);
          aPage.TimeControlled = true;
          aPage.StartTime = s;
          aPage.EndTime = e;
        }
        catch {
          aPage.TimeControlled = false;
        }
      }
   
      if (pattrib.Length > 0) {
        String[] parr = pattrib.Split(';');
        for (int i = 0; i < parr.Length; i++) {
          String[] pa = parr[i].Split('|');
          aPage.SavePageAttributeValue(pa[0], pa[1]);
        }
      }

      return "";
    }
    else return mess;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void DelPage(String id) {
    ThisId = id;
    ThisPage.DeleteRecursive();
    ThisId = "";
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public bool MoveNode(String id, String dir) {
    if (dir == "up") {
      DataSet ds = DB.GetDS("select parentpage, orderno from webpage where deleted=0 and id='" + id + "'");
      String parent = DB.GetString(ds, 0, "parentpage");
      int oldorder = DB.GetInt(ds, 0, "orderno");
      String where = (parent.Length > 0 ? "parentpage='" + parent + "'" : "parentpage is null");
      ds = DB.GetDS("select id, orderno from webpage where " + where + " and  deleted=0 and orderno < " + oldorder + " order by orderno desc");
      if (DB.GetRowCount(ds) > 0) {
        int neworder = DB.GetInt(ds, 0, "orderno");
        String switchid = DB.GetString(ds, 0, "id");
        Cms.LogEvent("pageorder", id);
        Cms.LogEvent("pageorder", switchid);
        DB.ExecSql("update webpage set orderno = " + oldorder + " where id='" + switchid + "'");
        DB.ExecSql("update webpage set orderno = " + neworder + " where id='" + id + "'");
        return true;
      }
      else return false;

    }
    else {
      DataSet ds = DB.GetDS("select parentpage, orderno from webpage where deleted=0 and id='" + id + "'");
      String parent = DB.GetString(ds, 0, "parentpage");
      int oldorder = DB.GetInt(ds, 0, "orderno");
      String where = (parent.Length > 0 ? "parentpage='" + parent + "'" : "parentpage is null");
      ds = DB.GetDS("select id, orderno from webpage where " + where + " and deleted=0 and orderno > " + oldorder + " order by orderno asc");
      if (DB.GetRowCount(ds) > 0) {
        int neworder = DB.GetInt(ds, 0, "orderno");
        String switchid = DB.GetString(ds, 0, "id");
        Cms.LogEvent("pageorder", id);
        Cms.LogEvent("pageorder", switchid);
        DB.ExecSql("update webpage set orderno = " + oldorder + " where id='" + switchid + "'");
        DB.ExecSql("update webpage set orderno = " + neworder + " where id='" + id + "'");
        return true;
      }
      else return false;
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void NewPageProperty(String pageid, String name, String val) {
    ThisId = pageid;
    PageProperty propProp = ThisPage.MainProp.GetProperty("PageProperties");
    propProp.GetProperty(name).PrelimValue = val;
    propProp.Publish(true);
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void DelPageProperty(int propId) {
    PageProperty propProp = Cms.GetPropertyById(propId);
    propProp.Delete();
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void SavePageProperty(int propId, String val) {
    PageProperty propProp = Cms.GetPropertyById(propId);
    propProp.Value = val;
    propProp.WriteToDB();
  }
}
