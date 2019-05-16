/* $Date: 2011-04-22 04:16:13 +0200 (fr, 22 apr 2011) $    $Revision: 7618 $ */
using System;
using System.Text;
using System.Data;
using System.IO;
using System.Collections;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Web.Configuration;
using NFN;
using AjaxPro;
using obout_ASPTreeView_2_NET;

public partial class DocumentBank : System.Web.UI.Page {

  private const int thumbWidth = 90;
  private const int thumbHeight = 90;
  private const int icoWidth = 25;
  private const int icoHeight = 25;

  private const int dummyPathId = -1;

  public string docPath = "DocumentBank/";
  public string codePath = "";
  private string folderIcons = "/admin/obout/treeview/icons";
  private string folderStyle = "/admin/obout/treeview/style/MediaBank";
  private string folderScript = "/admin/obout/treeview/script/MediaBank";

  protected CMS Cms {
    get { return (CMS)Page.Session["CMS"]; }
  }

  public String DocPath {
    get { return NFN.Util.HtmlRoot + "/" + docPath; }
  }
  
  public String MaxUploadSize {
    get {
      int result = 4096;
      HttpRuntimeSection section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
      if (section != null) result = section.MaxRequestLength;
      return result.ToString();
    }
  }
  
  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String Translate(String txt) {
    return Cms.Translate(txt);
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetThumbnail(String url, int width, int height) {
    return Util.GetThumbnail(url, width, height, true, null);
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetThumbnail(String url, int width, int height, bool keepaspect, String errthumb) {
    return Util.GetThumbnail(url, width, height, keepaspect, errthumb);
  }

  public String CurrFilePathId {
    get {
      if (Session["filepathid"] == null) Session["filepathid"] = "1";
      return Session["filepathid"].ToString();
    }
    set {
      if (Session["filepathid"] == null || value != Session["filepathid"].ToString()) {
        Session["filepathid"] = value;
      }
    }
  }

  public String DocAreaId {
    get { return DocArea.ClientID; }
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    AjaxPro.Utility.RegisterTypeForAjax(typeof(DocumentBank));
    TinyScript.Visible = (Request["fromtiny"] != null && Request["fromtiny"]=="y");

    if (!IsPostBack) {
      Page.Title = Translate("Mediabank");
      
      TreeViewBrowse.Text = GenerateTree();
      PermissionButton.Visible = Cms.User.IsSysAdmin;
    }
  }

  private void ReadViewPermissions() {
    int pt = DB.GetInt("select id from permissiontypes where itemtype='DocumentBank' and actiontype='View'", "id");

    String allRoles = "";
    foreach (String aRole in Cms.User.Roles) {
      if (allRoles.Length > 0) allRoles += ",";
      allRoles += "'" + aRole + "'";
    }
    bool defPerm = DB.GetInt("select count(*) as nof from permissions where id='DEFAULT' and permission='Y' and role in (" + allRoles + ") and typeid=" + pt, "nof") > 0;
    Session["DefPerm"] = defPerm;

    if (defPerm) {
      DataSet ds = DB.GetDS("select id from permissions where id<>'DEFAULT' and permission='N' and role in (" + allRoles + ") and typeid=" + pt);
      String perm = "";
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        if (perm.Length > 0) perm += ",";
        perm += DB.GetString(ds, i, "id");
      }
      Session["NoPermPath"] = perm;
    }
    else {
      DataSet ds = DB.GetDS("select id from permissions where id<>'DEFAULT' and permission='Y' and role in (" + allRoles + ") and typeid=" + pt);
      String perm = "";
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        if (perm.Length > 0) perm += ",";
        perm += DB.GetString(ds, i, "id");
      }
      Session["PermPath"] = perm;
    }
  }

  private bool DefViewPerm {
    get {
      if (Session["DefPerm"] == null)
        ReadViewPermissions();
      return Convert.ToBoolean(Session["DefPerm"]);
    }
  }

  private String PermViewPath {
    get {
      if (Session["PermPath"] == null)
        ReadViewPermissions();
      return Session["PermPath"].ToString();
    }
  }

  private String NoPermViewPath {
    get {
      if (Session["NoPermPath"] == null)
        ReadViewPermissions();
      return Session["NoPermPath"].ToString();
    }
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public bool HasPermission(String id, String type) {
    if (id == "1" && type == "viewfolders") return true;
    else return Cms.GetPermission(id, "DocumentBank", type);
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public bool IsLoggedIn() {
    return Cms.GetPermission("DEFAULT", "DocumentBank", "DEFAULT");
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String AfterUpload() {
    DataSet ds = DB.GetDS("select id from documents where filepath = -1");
    String ids = "";
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      if (ids.Length > 0) ids += ",";
      ids += DB.GetString(ds, i, "id");
    }
    DB.ExecSql("update documents set filepath=" + CurrFilePathId + " where filepath = -1");
    return ids;
  }

  private bool IsInt(String aval) {
    try {
      int x = Convert.ToInt32(aval);
      return true;
    }
    catch { return false; }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String SearchDoc(String standard, String extra, String currView) {
    // docType|wmin|wmax|hmin|hmax|smin|smax|fname
    String[] stdarr = standard.Split('|');

    String sqlWhere = "where d.deleted=0 and d.filename like '%" + stdarr[7] + "%'";
    if (stdarr[0] != "all") sqlWhere += " and d.doctype='" + stdarr[0] + "'";
    if (IsInt(stdarr[1]) && stdarr[0] == "image") sqlWhere += " and d.imagewidth >= " + stdarr[1];
    if (IsInt(stdarr[2]) && stdarr[0] == "image") sqlWhere += " and d.imagewidth <= " + stdarr[2];
    if (IsInt(stdarr[3]) && stdarr[0] == "image") sqlWhere += " and d.imageheight >= " + stdarr[3];
    if (IsInt(stdarr[4]) && stdarr[0] == "image") sqlWhere += " and d.imageheight <= " + stdarr[4];
    if (IsInt(stdarr[5])) sqlWhere += " and d.filesize >= " + (Convert.ToInt32(stdarr[5])*1024).ToString();
    if (IsInt(stdarr[6])) sqlWhere += " and d.filesize <= " + (Convert.ToInt32(stdarr[6])*1024).ToString();

    String extraWhere = "";
    String[] extraarr = extra.Split(';');
    for (int i=0; i < extraarr.Length; i++) {
      String[] info = extraarr[i].Split('|');
      if (info.Length > 1 && info[1].Length > 0) {
        String extraId = info[0];
        DataSet ds0 = DB.GetDS("select infotype from docinfo where id = " + extraId);
        String infoType = DB.GetString(ds0, 0, "infotype");
        if (infoType.Length == 0 || infoType == "text")
          extraWhere += " and d.id=i.DocId and i.DocInfoId=" + extraId + " and i.InfoText like '%" + info[1] + "%'";
        else
          extraWhere += " and d.id=i.DocId and i.DocInfoId=" + extraId + " and i.InfoText='" + info[1] + "'";
      }
    }

    String permWhere = "";
    if (!Cms.User.IsSysAdmin) {
      if (DefViewPerm && NoPermViewPath.Length > 0) permWhere = " and not filepath in (" + NoPermViewPath + ")";
      else if (!DefViewPerm && PermViewPath.Length > 0) permWhere = " and filepath in (" + PermViewPath + ")";
    }

    String sqlSelect = (extraWhere.Length > 0 ? "select distinct d.* from Documents d, DocInfoItem i " : "select distinct d.* from Documents d ");
    String sqlStr = sqlSelect + sqlWhere + extraWhere + permWhere + " order by d.filename";

    return DocAreaHtml(sqlStr, true, currView);
  }


  private String GetSizeStr(int fileSize) {
    String fileSizeStr = "";
    if (fileSize > 1048576) fileSizeStr = String.Format("{0:0.00}", Convert.ToDouble(fileSize)/1048576) + " Mb";
    else if (fileSize > 1024) fileSizeStr = String.Format("{0:0.00}", Convert.ToDouble(fileSize)/1024) + " kb";
    else fileSizeStr = fileSize.ToString() + " bytes";
    return fileSizeStr;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String[] GetDocumentInfo(String fileId) {
    String[] res = new String[2];
    DataSet ds = DB.GetDS("select * from documents where deleted=0 and id = " + fileId);

    int fileSize = DB.GetInt(ds, 0, "filesize");
    String fileSizeStr = GetSizeStr(fileSize);

    String filePathId = DB.GetString(ds, 0, "filepath");
    String fname = Path.GetFileName(DB.GetString(ds, 0, "filename"));
    String path = GetPathString(filePathId);
    String docType = DB.GetString(ds, 0, "doctype");
    String imgWidth = DB.GetString(ds, 0, "imagewidth");
    String imgHeight = DB.GetString(ds, 0, "imageheight");
    double proportions = 1;
    try { proportions = DB.GetDouble(ds, 0, "imagewidth")/DB.GetDouble(ds, 0, "imageheight"); }
    catch {}
    fileSizeStr = (docType == "image" || fname.EndsWith("swf") ? imgWidth + "px x " + imgHeight + "px (" + fileSizeStr + ")" : fileSizeStr);

    res[0] = docType + "|" + path + "|" + fname + "|" + fileSize + "|" + fileSizeStr + "|" + imgWidth + "|" + imgHeight + "|" + proportions.ToString().Replace(",",".");
    res[1] = "";

    ds = DB.GetDS("select * from docinfo");
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      if (res[1].Length > 0) res[1] += ";";
      int itemId = DB.GetInt(ds, i, "id");
      String itemVal = DB.GetString("select infotext from docinfoitem where docid = " + fileId + " and docinfoid=" + itemId, "infotext");
      res[1] += itemId + "|" + itemVal;
    }

    return res;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String[] GetExtraFields() {
    DataSet ds = DB.GetDS("select * from docinfo");
    String[] res = new String[DB.GetRowCount(ds)];
    for (int i=0; i < DB.GetRowCount(ds); i++) 
      res[i] = DB.GetString(ds, i, "InfoHeadline") + "|" + DB.GetString(ds, i, "id") + "|" + DB.GetString(ds, i, "InfoType");
    return res;
  }
  

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
  public void PermissionChanged(String pathId, String aRole, String action, bool isChecked) {
    Cms.SetRolePermission(pathId, "DocumentBank", aRole, action, "", isChecked);
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String CheckDrop(String dst, String src, bool copy) {
    if (!copy) {
      String pname = DB.GetString("select pathname from filepath where deleted=0 and id=" + src, "pathname");
      int count = DB.GetInt("select count(*) as nof from filepath where deleted=0 and parentpath=" + dst + " and pathname='" + pname + "'", "nof");
      if (count > 0) return "Mappen innehåller redan mapp med detta namn.";
      return "";
    }
    else
      return "";
  }



  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String CopyPermFromParent(String node) {
    int parent = DB.GetInt("select parentpath from filepath where deleted=0 and id=" + node, "parentpath");
    if (parent > 0) {
      DB.ExecSql("delete from permissions where id='" + node + "' and typeid in (select id from permissiontypes where itemtype='DocumentBank' and actiontype<>'DEFAULT')");
      DataSet ds = DB.GetDS("select * from permissions where id='" + parent + "' and typeid in (select id from permissiontypes where itemtype='DocumentBank' and actiontype<>'DEFAULT')");
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        String sql = "insert into permissions (id, typeid, role, permission) values('" + node + "', " + DB.GetInt(ds, i, "typeid") + ", '" + DB.GetString(ds, i, "role") + "', '" + DB.GetString(ds, i, "permission") + "')";
        DB.ExecSql(sql);
      }
    }
    return PermissionAreaHtml(node);
  }

  private void DoCopyPerm(String basenode, String node) {
    DataSet dsch = DB.GetDS("select id from filepath where deleted=0 and parentpath=" + node);
    for (int i=0; i < DB.GetRowCount(dsch); i++) {
      int currNode = DB.GetInt(dsch, i, "id");
      DoCopyPerm(basenode, currNode.ToString());
    }
    DataSet ds = DB.GetDS("select * from permissions where id='" + basenode + "' and typeid in (select id from permissiontypes where itemtype='DocumentBank' and actiontype<>'DEFAULT')");
    DB.ExecSql("delete from permissions where id='" + node + "' and typeid in (select id from permissiontypes where itemtype='DocumentBank' and actiontype<>'DEFAULT')");
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String sql = "insert into permissions (id, typeid, role, permission) values('" + node + "', " + DB.GetInt(ds, i, "typeid") + ", '" + DB.GetString(ds, i, "role") + "', '" + DB.GetString(ds, i, "permission") + "')";
      DB.ExecSql(sql);
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void CopyPermToChildren(String node) {
    DoCopyPerm(node, node);
  }

  private String GetPermissionChar(String id, String itemtype, String actiontype) {
    if (!Cms.User.LoggedIn) return "N";
    if (Cms.User.IsSysAdmin) return "Y";
    int ptid = Cms.GetPermissionTypeId(itemtype, actiontype);

    String roles = "";
    foreach (String role in Cms.User.Roles)
      roles += ",'" + role + "'";
    DataSet ds = DB.GetDS("select permission from permissions where typeid=" + ptid + " and role in (" + roles.Substring(1) + ") and id='" + id + "'");
    String aPerm = "";
    for (int i=0; i < DB.GetRowCount(ds) && aPerm != "Y"; i++) {
      String thisPerm = DB.GetString(ds, i, "permission");
      if (thisPerm.Length > 0) aPerm = thisPerm;
    }
    return aPerm;
  }

  private String GetRecursivePermission(String pathId, String action) {
    String aPerm = GetPermissionChar(pathId, "DocumentBank", action);
    if (aPerm.Length > 0) return aPerm;
    String parentPath = DB.GetString("select parentpath from filepath where deleted=0 and id=" + pathId, "parentpath");
    if (parentPath == "0")
      return (Cms.GetPermission(parentPath, "DocumentBank", action) ? "Y" : "N");
    return GetRecursivePermission(parentPath, action);
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
  public String[] NodeSelected(String mode, String pathId, String currView, String currOrder, String currDir) {
    String pathString = GetPathString(pathId);
    CurrFilePathId = pathId;

    String docHtml = "";
    String docs = "";
    if (Cms.GetPermission(pathId, "DocumentBank", "view")) {
      String sql = "select * from Documents where deleted=0 and filepath=" + pathId + " order by " + currOrder + " " + currDir;
      docHtml = DocAreaHtml(sql, false, currView);
      DataSet ds = DB.GetDS(sql);
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        if (i > 0) docs += ";";
        docs += "thumb_" + DB.GetString(ds, i, "id");
      }
    }
    String permHtml = (Cms.User.IsSysAdmin ? PermissionAreaHtml(pathId) : "");

    String addPerm = (GetRecursivePermission(pathId, "AddFolder") == "Y" ? "Y" : "N");
    String delPerm = (GetRecursivePermission(pathId, "DelFolder") == "Y" ? "Y" : "N");
    String permissions = GetRecursivePermission(pathId, "Use") + ";" +
      GetRecursivePermission(pathId, "Edit") + ";" +
      GetRecursivePermission(pathId, "Upload") + ";" +
      GetRecursivePermission(pathId, "Del") + ";" +
      addPerm + ";" +
      GetRecursivePermission(pathId, "EditFolder") + ";" +
      delPerm;
    String[] res = new String[5];
    res[0] = pathString;
    res[1] = docHtml;
    res[2] = docs;
    res[3] = permHtml;
    res[4] = permissions;
    return res;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void NodeMoved(String src, String dest) {
    String pathname = DB.GetString("select pathname from filepath where deleted=0 and id=" + src, "pathname");
    if (DB.GetInt("select count(*) as nof from filepath where deleted=0 and pathname='" + pathname + "' and parentpath=" + dest, "nof") > 0) {
      int counter = 1;
      while (DB.GetInt("select count(*) as nof from filepath where deleted=0 and pathname='" + pathname + "(" + counter + ")' and parentpath=" + dest, "nof") > 0)
        counter++;
      pathname = pathname + "(" + counter + ")";
    }
    DB.ExecSql("update filepath set parentpath='" + dest + "', pathname='" + pathname + "' where id=" + src);
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void NodeCopied(String destId, String oriId) {
    CreatePathCopy(destId, oriId);
  }

  private void CreatePathCopy(String destpath, String oripath) {
    String pathname = DB.GetString("select pathname from filepath where deleted=0 and id=" + oripath, "pathname");
    if (DB.GetInt("select count(*) as nof from filepath where deleted=0 and pathname='" + pathname + "' and parentpath=" + destpath, "nof") > 0) {
      int counter = 1;
      while (DB.GetInt("select count(*) as nof from filepath where deleted=0 and pathname='" + pathname + "(" + counter + ")' and parentpath=" + destpath, "nof") > 0)
        counter++;
      pathname = pathname + "(" + counter + ")";
    }
    DB.ExecSql("insert into filepath (pathname, parentpath) values ('" + pathname + "', " + destpath + ")");

    String newpath;
    if (ConfigurationManager.AppSettings["connectionType"] == "ODBC")
      newpath = DB.GetString("select id from filepath where deleted=0 order by id desc", "id");
    else
      newpath = DB.GetString("select top 1 id from filepath where deleted=0 order by id desc", "id");

    DataSet ds = DB.GetDS("select * from filepath where deleted=0 and parentpath=" + oripath);
    for (int i=0; i < DB.GetRowCount(ds); i++)
      CreatePathCopy(newpath, DB.GetString(ds, i, "id"));

    ds = DB.GetDS("select * from documents where deleted=0 and filepath=" + oripath);
    for (int i=0; i < DB.GetRowCount(ds); i++)
      CreateDocCopy(newpath, DB.GetString(ds, i, "id"));
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String DocMovedExternal(String docIds, String destid) {
    String[] docArr = docIds.Split(',');
    String oripath = DB.GetString("select filepath from documents where deleted=0 and id=" + docArr[0], "filepath");
    if (oripath == destid)
      return "Kan inte flytta. Destinationen är samma som källan.";
    for (int i=0; i < docArr.Length; i++) {
      DB.ExecSql("update documents set filepath='" + destid + "' where id=" + docArr[i]);
    }
    return "";
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String DocCopiedExternal(String docIds, String destid) {
    String[] docArr = docIds.Split(',');
    String oripath = DB.GetString("select filepath from documents where deleted=0 and id=" + docIds, "filepath");
    if (oripath == destid)
      return Translate("Kan inte kopiera. Destinationen är samma som källan") + ".";
    for (int i=0; i < docArr.Length; i++)
      CreateDocCopy(destid, docArr[i]);
    return "";
  }

  private void CreateDocCopy(String filepath, String docid) {
    DataSet ds = DB.GetDS("select * from documents where deleted=0 and id=" + docid);

    String sql = "insert into documents (filepath, doctype, filesize, imagewidth, imageheight, filename, date_start, date_stop) " +
      "values (" + filepath + ",'" + DB.GetString(ds, 0, "doctype") + "'," + DB.GetInt(ds, 0, "filesize") + "," + DB.GetInt(ds, 0, "imagewidth") + "," + DB.GetString(ds, 0, "imageheight") + ",'" + DB.GetString(ds, 0, "filename") + "', '" + DB.GetString(ds, 0, "date_start") + "', '" + DB.GetString(ds, 0, "date_stop") + "')";
    DB.ExecSql(sql);
    int newid;
    if (ConfigurationManager.AppSettings["connectionType"] == "ODBC")
      newid = DB.GetInt("select id from documents where deleted=0 order by id desc", "id");
    else
      newid = DB.GetInt("select top 1 id from documents where deleted=0 order by id desc", "id");
    DataSet ds2 = DB.GetDS("select * from docinfoitem where DocId=" + newid);
    for (int i=0; i < DB.GetRowCount(ds2); i++)
      DB.ExecSql("insert into docinfoitem (DocId, DocInfoId, InfoText) values (" + newid + ", " + DB.GetInt(ds2, i, "DocInfoId") + ", '" + DB.GetString(ds2, i, "InfoText") + "')");
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String NodeRename(String id, String newname) {
    if (DB.GetInt("select count(*) as nof from filepath where deleted=0 and pathname='" + newname + "' and parentpath in (select parentpath from filepath where deleted=0 and id=" + id + ")", "nof") > 0)
      return Translate("En mapp med detta namn exiterar redan");;
    DB.ExecSql("update filepath set pathname='" + newname + "' where id=" + id);
    return "";
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String[] AddNode(String parentNode) {
    int counter = 0;
    String nodename = Translate("Ny mapp");
    DataSet ds = DB.GetDS("select * from filepath where deleted=0 and pathname='" + nodename + "' and parentpath=" + parentNode);
    while (DB.GetRowCount(ds) > 0) {
      counter++;
      nodename = Translate("Ny mapp") + "(" + counter + ")";
      ds = DB.GetDS("select * from filepath where deleted=0 and pathname='" + nodename + "' and parentpath=" + parentNode);
    }
    DB.ExecSql("insert into filepath (pathname, parentpath) values ('" + nodename + "', " + parentNode + ")");
    if (ConfigurationManager.AppSettings["connectionType"] == "ODBC")
      ds = DB.GetDS("select id from filepath where deleted=0 order by id desc");
    else
      ds = DB.GetDS("select top 1 id from filepath where deleted=0 order by id desc");
    String nodeId = DB.GetString(ds, 0, "id");
    CopyPermFromParent(nodeId);

    String[] res = new String[2];
    res[0] = nodeId;
    res[1] = nodename;
    return res;
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String[] DelNode(String nodeId, bool force) {
    String[] res = {"",""};
    return DoDelNode(nodeId, force, res);
  }

  private String[] DoDelNode(String nodeId, bool force, String[] res) {
    DataSet ds = DB.GetDS("select id from documents where deleted=0 and filepath=" + nodeId);
    String ids = "";
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      if (i > 0) ids += ",";
      ids += DB.GetString(ds, i, "id");
    }
    String noderes = DelDocuments(ids, force);

    if (noderes.Length == 0) {
      if (res[0].Length > 0) res[0] += ",";
      res[0] += nodeId;
      DB.ExecSql("update filepath set deleted=1 where id=" + nodeId);
      DB.ExecSql("delete from permissions where id='" + nodeId + "' and typeid in (select id from permissiontypes where itemtype='DocumentBank' and actiontype <> 'DEFAULT')");
    }
    else {
      if (res[1].Length > 0) res[1] += ";";
      res[1] += noderes;
    }

    ds = DB.GetDS("select id from filepath where deleted=0 and parentpath=" + nodeId);
    for (int i=0; i < DB.GetRowCount(ds); i++)
      res = DoDelNode(DB.GetString(ds, i, "id"), force, res);

    return res;
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String DelDocuments(String docIds, bool force) {
    String res = "";
    if (docIds.Length > 0) {
      String[] docArr = docIds.Split(',');
      for (int i=0; i < docArr.Length; i++) {
        String delres = DelOneDocument(docArr[i], force);
        if (delres.Length > 0) {
          if (res.Length > 0) res += ";";
          res += delres;
        }
      }
    }
    return res;
  }

  private String DelOneDocument(String docId, bool force) {
    String aFileName = DB.GetString("select filename from documents where deleted=0 and id = " + docId, "filename");
    String res = "";
    if (!force) {
      DataSet ds = DB.GetDS("select id, parentproperty from pageproperty where deleted <> 1 and (propertyvalue like '%" + aFileName + "%' or prelimpropertyvalue like '%" + aFileName + "%')");
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        int parentprop = DB.GetInt(ds, i, "parentproperty");
        int lastparent = parentprop;
        while (parentprop > 0) {
          parentprop = DB.GetInt("select parentproperty from pageproperty where id=" + parentprop, "parentproperty");
          if (parentprop > 0) lastparent = parentprop;
        }
        res += "|" + DB.GetString("select id from webpage where deleted=0 and mainprop=" + lastparent + " or sharedprop=" + lastparent, "id");
      }
      if (res.Length > 0)
        return Path.GetFileName(aFileName) + res;
    }

    DB.ExecSql("update documents set deleted=1 where id=" + docId);
//      DB.ExecSql("delete from docinfoitem where DocId=" + docId);
/*    try {
      File.Delete(Server.MapPath(NFN.Util.HtmlRoot + aFileName));

      String fname = Path.GetFileNameWithoutExtension(aFileName);
      DirectoryInfo di = new DirectoryInfo(Server.MapPath(NFN.Util.HtmlRoot + "/DocumentBank/thumbs"));
      FileInfo[] fi = di.GetFiles();
      foreach (FileInfo fiTemp in fi)
        if (fiTemp.Name.StartsWith(fname))
          fiTemp.Delete();
    }
    catch {}*/
    return "";
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void SaveDocInfo(String docId, String info){
    String[] values = info.Split(';');
    for (int i=0; i < values.Length; i++) {
      String[] help2 = values[i].Split('|');
      String infoId = help2[0];
      String currVal = help2[1];
      DataSet ds = DB.GetDS("select infotext from docinfoitem where docid=" + docId + " and docinfoid=" + infoId);
      if (DB.GetRowCount(ds) > 0) {
        String lastVal = DB.GetString(ds, 0, "infotext");
        if (lastVal != currVal) DB.ExecSql("update docinfoItem set infotext='" + currVal + "' where docid=" + docId + " and docinfoid=" + infoId);
      }
      else if (currVal.Length > 0)
        DB.ExecSql("insert into docinfoitem (docid, docinfoid, infotext) values (" + docId + ", " + infoId + ", '" + currVal + "')");
    }
  }

  protected void Page_PreRender(object sender, System.EventArgs e) {
    if (!Cms.User.LoggedIn && !IsClientScriptBlockRegistered("CloseWind")) {
      RegisterClientScriptBlock( "CloseWind", "<script type='text/javascript'>window.close();</script>");
    }
  }

  private void AddTableCell(TableRow aRow, String html) {
    TableCell cell = new TableCell();
    cell.Text = html;
    aRow.Cells.Add(cell);
  }

  private String GetPathString(String filePathId) {
    DataSet ds = DB.GetDS("select * from filepath where deleted=0 and id=" + filePathId);
    String apath = "<a href='#' onClick='db.tree.selectPath(" + filePathId + ")' onFocus='this.blur()'>" + DB.GetString(ds, 0, "pathname") + "</a>";
    filePathId = DB.GetString(ds, 0, "parentpath");
    while (filePathId != "0" ) {
      ds = DB.GetDS("select * from filepath where deleted=0 and id=" + filePathId);
      if (DB.GetRowCount(ds) > 0) {
        apath = "<a href='#' onClick='db.tree.selectPath(" + filePathId + ")' onFocus='this.blur()'>" + DB.GetString(ds, 0, "pathname") + "</a> / " + apath;
        filePathId = DB.GetString(ds, 0, "parentpath");
      }
      else
        filePathId = "0";
    }
    return apath;
  }


  private String CurrFullNodePath() {
    ArrayList nodes = new ArrayList();
    nodes.Add(CurrFilePathId);
    String parentNode = DB.GetInt("select parentpath from filepath where deleted=0 and id=" + CurrFilePathId, "parentpath").ToString();
    while (CurrFilePathId != "1" && parentNode != "1" && parentNode != "0") {
      nodes.Add(parentNode);
      parentNode = DB.GetInt("select parentpath from filepath where deleted=0 and id=" + parentNode, "parentpath").ToString();
    }
    String nodeStr = "";
    for (int i=nodes.Count-1; i >= 0; i--)
      nodeStr += "," + nodes[i].ToString();
    return nodeStr.Substring(1);
  }

  private void CreateNode(obout_ASPTreeView_2_NET.Tree oTree, String parent, String html, int id) {
    oTree.Add(parent, id.ToString(), html, false, null, null);
    if (Cms.GetPermission(id.ToString(), "DocumentBank", "viewfolders")) {
      DataSet ds = DB.GetDS("select * from filepath where deleted=0 and parentpath=" + id + " order by pathname");
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        int subid = DB.GetInt(ds, i, "id");
        if (Cms.GetPermission(subid.ToString(), "DocumentBank", "view")) {
          String html2 = DB.GetString(ds, i, "pathname");
          CreateNode(oTree, id.ToString(), html2, subid);
        }
      }
    }
  }

  private String GenerateTree() {

    obout_ASPTreeView_2_NET.Tree oTree = new obout_ASPTreeView_2_NET.Tree();

    DataSet ds = DB.GetDS("select * from filepath where deleted=0 and parentpath=0 order by pathname");
    String rootid = DB.GetString(ds, 0, "id");
    String html = "<span onClick=\"db.tree.expandIfClosed('" + rootid + "')\">" + DB.GetString(ds, 0, "pathname") + "</span>";
    oTree.Add("root", rootid, html, true, "xpMyComp.gif\" onClick=\"ob_t25(document.getElementById('" + rootid + "'))", null);

    ds = DB.GetDS("select * from filepath where deleted=0 and parentpath=1 order by pathname");
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      int currId = DB.GetInt(ds, i, "id");
      if (Cms.GetPermission(currId.ToString(), "DocumentBank", "view") || Cms.GetPermission(currId.ToString(), "DocumentBank", "viewfolders")) {
        String currName = DB.GetString(ds, i, "pathname");
        html = currName;
        CreateNode(oTree, rootid, html, currId);
      }
    }

    bool dragndrop = Cms.GetPermission("DummyId", "DocumentBank", "dragndrop");

    oTree.FolderIcons = folderIcons;
    oTree.FolderStyle = folderStyle;
    oTree.FolderScript = folderScript;
    oTree.EditNodeEnable = true;
    oTree.DragAndDropEnable = dragndrop;
    oTree.MultiSelectEnable = false;

    oTree.SelectedId = CurrFullNodePath();

    String tree = oTree.HTML();

    // Remove trial info
    int idx = tree.IndexOf("<div style=\"font:11px verdana; background-color:white; border:3px solid #cccccc; color:#666666;");
    if (idx >= 0)
      tree = tree.Substring(0,idx);

    return tree;
  }


  private String DocAreaHtml(String sql, bool fromSearch, String currView) {

    StringBuilder html = new StringBuilder();
    StringBuilder docs = new StringBuilder();

    DataSet ds = DB.GetDS(sql);

    if (DB.GetRowCount(ds) == 0 && fromSearch)
      return "<p style='margin:10px 0 0 10px; font-weight:bold'>Sökningen gav inget resultat</p>";

    DataSet dsdi = null;
    if (currView == "list") {
      html.Append("<div id='docTable'><table cellspacing=0 cellpadding=0><tr class='headRow'><td>&nbsp;</td><td><a href='javascript:db.reorder(\"filename\")' onfocus='this.blur()'>Namn</a></td><td><a href='javascript:db.reorder(\"filesize\")' onfocus='this.blur()'>Storlek</a></td><td><a href='javascript:db.reorder(\"imagewidth\")' onfocus='this.blur()'>Bredd</a></td><td><a href='javascript:db.reorder(\"imageheight\")' onfocus='this.blur()'>Höjd</a></td><td>Datum</td>");
      dsdi = DB.GetDS("select id, infoheadline from docinfo order by id");
      for (int i=0; i < DB.GetRowCount(dsdi); i++)
        html.Append("<td><b>" + DB.GetString(dsdi, i, "infoheadline") + "</b></td>");
      html.Append("</tr>");
    }

    for (int i=0; i < DB.GetRowCount(ds); i++) {
      String fileId = DB.GetString(ds, i, "id");
      String docType = DB.GetString(ds, i, "doctype");
      String fileName = DB.GetString(ds, i, "filename");

      FileInfo fileInfo = null;
      if (fileName.StartsWith("/"))
        fileInfo = new FileInfo(Server.MapPath("~" + fileName));
      else
        fileInfo = new FileInfo(Server.MapPath("~/" + fileName));

      if (fileInfo != null && fileInfo.Exists) {
        fileName = fileInfo.Name;
        if (docs.Length > 0) docs.Append(",");
        docs.Append(fileId);

        if (currView == "thumbs") {
          html.Append("<div id='thumb_" + fileId + "' class='DocumentBox' onClick='db.markDoc(event, \"" + fileId + "\")' onDblClick='db.useDoc(\"" + fileId + "\")' >");
          html.Append("<div class='Document'>");

          if (docType == "image") 
            html.Append("<img border='0' src='" + Util.GetThumbnail(docPath + fileName, thumbWidth, thumbHeight) + "' alt='" + fileName + "'/>");
          else if (fileInfo.Extension.ToLower() == ".txt")
            html.Append("<img border='0' src='" + codePath + "gfx/icoText.gif' alt='" + fileName + "' />");
          else if (fileInfo.Extension.ToLower() == ".doc")
            html.Append("<img border='0' src='" + codePath + "gfx/icoWord.gif' alt='" + fileName + "' />");
          else if (fileInfo.Extension.ToLower() == ".pdf")
            html.Append("<img border='0' src='" + codePath + "gfx/icoPdf.gif' alt='" + fileName + "' />");
          else if (fileInfo.Extension.ToLower() == ".swf")
            html.Append("<img border='0' src='" + codePath + "gfx/icoFlash.gif' alt='" + fileName + "' />");
          else if (fileInfo.Extension.ToLower() == ".mp3")
            html.Append("<img border='0' src='" + codePath + "gfx/icoMp3.gif' alt='" + fileName + "' />");
          else if (docType == "media")
            html.Append("<img border='0' src='" + codePath + "gfx/icoMedia.gif' alt='" + fileName + "' />");
          else
            html.Append("<img border='0' src='" + codePath + "gfx/icoGeneral.gif' alt='" + fileName + "' />");
          html.Append("</div><p>" + fileName + "</p></div>");
        }
        if (currView == "list") {
          int fileSize = DB.GetInt(ds, i, "filesize");
          int imgWidth = DB.GetInt(ds, i, "imagewidth");
          int imgHeight = DB.GetInt(ds, i, "imageheight");
          DateTime savedate = fileInfo.LastWriteTime;
          html.Append("<tr id='thumb_" + fileId + "' class='docRow' onClick='db.markDoc(event, \"" + fileId + "\")' onDblClick='db.useDoc(\"" + fileId + "\")'><td>");
          if (docType == "image")
            html.Append("<img border='0' src='" + Util.GetThumbnail(docPath + fileName, icoWidth, icoHeight) + "' alt='" + fileName + "'/>");
          else if (fileInfo.Extension.ToLower() == ".txt")
            html.Append("<img border='0' src='" + codePath + "gfx/icoTxt_small.gif' alt='" + fileName + "' />");
          else if (fileInfo.Extension.ToLower() == ".doc")
            html.Append("<img border='0' src='" + codePath + "gfx/icoWoed_small.gif' alt='" + fileName + "' />");
          else if (fileInfo.Extension.ToLower() == ".pdf")
            html.Append("<img border='0' src='" + codePath + "gfx/icoPdf_small.gif' alt='" + fileName + "' />");
          else if (fileInfo.Extension.ToLower() == ".swf")
            html.Append("<img border='0' src='" + codePath + "gfx/icoFlash_small.gif' alt='" + fileName + "' />");
          else if (fileInfo.Extension.ToLower() == ".mp3")
            html.Append("<img border='0' src='" + codePath + "gfx/icoMp3_small.gif' alt='" + fileName + "' />");
          else if (docType == "media")
            html.Append("<img border='0' src='" + codePath + "gfx/icoMedia_small.gif' alt='" + fileName + "' />");
          else
            html.Append("<img border='0' src='" + codePath + "gfx/icoGeneral_small.gif' alt='" + fileName + "' />");
          html.Append("</td><td>" + fileName + "</td>");
          html.Append("<td>" + GetSizeStr(fileSize) + "</td>");
          html.Append("<td>" + imgWidth.ToString() + "</td>");
          html.Append("<td>" + imgHeight.ToString() + "</td>");
          html.Append("<td> "+ savedate.ToString("yyyy-MM-dd HH:mm") + "</td>");

          for (int j=0; j < DB.GetRowCount(dsdi); j++) {
            String val = DB.GetString("select infotext from docinfoitem where docid=" + fileId + " and docinfoid=" + DB.GetInt(dsdi, j, "id"), "infotext");
            if (val.Length == 0) val = "&nbsp;";
            else if (val.Length > 28) val = val.Substring(0, 25) + "...";
            html.Append("<td>" + val + "</td>");
          }

          html.Append("</tr>");
        }
      }
    }
    if (currView == "list")
      html.Append("</table></div>");
    html.Append("<input type='hidden' id='docids' value='" + docs.ToString() + "' />");
    return html.ToString();
  }


  private String GetPermissionTypes() {
    DataRow[] ptids = Cms.PermissionTypes.Select("itemtype='DocumentBank'");
    StringBuilder res = new StringBuilder();
    for (int i=0; i < ptids.Length; i++)
      res.Append("," + ptids[i]["id"].ToString());
    return res.ToString().Substring(1);
  }

  private bool GetPathPermission(DataTable ptable, String aRole, String action, String pathId) {
    int typeid = Cms.GetPermissionTypeId("DocumentBank", action);
    DataRow[] rows = ptable.Select("typeid=" + typeid + " and role='" + aRole + "' and (id='" + pathId + "' or id='DEFAULT')", "id");
    if (rows.Length > 0) return rows[0]["permission"].ToString() == "Y";
    else return false;
  }

  private String PermissionAreaHtml(String pathId) {
    StringBuilder permHtml = new StringBuilder();
    if (Cms.User.IsSysAdmin) {

      DataTable ptable = DB.GetDS("select * from permissions where typeid in (" + GetPermissionTypes() + ")").Tables[0];

      permHtml.Append("<table cellspacing='0' cellpadding='5' width='100%' border='1'>");
      permHtml.Append("<tr><td><b>" + Translate("Roll") + "</b></td><td><b>" + Translate("Visa undermappar") + "</b></td><td><b>" + Translate("Visa dokument") + "</b></td><td><b>" + Translate("Använda dokument") + "</b></td><td><b>" + Translate("Redigera dokument") + "</b></td><td><b>" + Translate("Ladda upp dokument") + "</b></td><td><b>" + Translate("Radera dokument") + "</b></td><td><b>" + Translate("Skapa mappar") + "</b></td><td><b>" + Translate("Redigera mappar") + "</b></td><td><b>" + Translate("Radera mappar") + "</b></td></tr>");
      DataSet ds = DB.GetDS("select name from role where name <> 'Admin' order by name");
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        permHtml.Append("<tr>");
        String aRole = DB.GetString(ds, i, "name");
        permHtml.Append("<td>" + aRole + "</td>");
        permHtml.Append("<td><input type='checkbox' id='" + aRole + ";viewfolders' onClick='db.perm.changed(this)' " + (GetPathPermission(ptable, aRole, "ViewFolders", pathId) ? "checked" : "") + "/></td>");
        permHtml.Append("<td><input type='checkbox' id='" + aRole + ";view' onClick='db.perm.changed(this)' " + (GetPathPermission(ptable, aRole, "View", pathId) ? "checked" : "") + "/></td>");
        permHtml.Append("<td><input type='checkbox' id='" + aRole + ";use' onClick='db.perm.changed(this)' " + (GetPathPermission(ptable, aRole, "Use", pathId) ? "checked" : "") + "/></td>");
        permHtml.Append("<td><input type='checkbox' id='" + aRole + ";edit' onClick='db.perm.changed(this)' " + (GetPathPermission(ptable, aRole, "Edit", pathId) ? "checked" : "") + "/></td>");
        permHtml.Append("<td><input type='checkbox' id='" + aRole + ";upload' onClick='db.perm.changed(this)' " + (GetPathPermission(ptable, aRole, "Upload", pathId) ? "checked" : "") + "/></td>");
        permHtml.Append("<td><input type='checkbox' id='" + aRole + ";del' onClick='db.perm.changed(this)' " + (GetPathPermission(ptable, aRole, "Del", pathId) ? "checked" : "") + "/></td>");
        permHtml.Append("<td><input type='checkbox' id='" + aRole + ";addfolder' onClick='db.perm.changed(this)' " + (GetPathPermission(ptable, aRole, "AddFolder", pathId) ? "checked" : "") + "/></td>");
        permHtml.Append("<td><input type='checkbox' id='" + aRole + ";editfolder' onClick='db.perm.changed(this)' " + (GetPathPermission(ptable, aRole, "EditFolder", pathId) ? "checked" : "") + "/></td>");
        permHtml.Append("<td><input type='checkbox' id='" + aRole + ";delfolder' onClick='db.perm.changed(this)' " + (GetPathPermission(ptable, aRole, "DelFolder", pathId) ? "checked" : "") + "/></td>");
        permHtml.Append("</tr>");
      }
      permHtml.Append("</table>");
    }
    return permHtml.ToString();
  }
}
