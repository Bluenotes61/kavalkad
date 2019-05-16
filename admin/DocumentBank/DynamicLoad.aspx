<!-- $Date: 2010-10-07 12:13:34 +0200 (to, 07 okt 2010) $    $Revision: 7023 $ -->
<%@ Page Language="C#" %>
<%@ Import Namespace="obout_ASPTreeView_2_NET" %>
<%@ Import Namespace="NFN" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.IO" %>

<script language="C#" runat="server">

  private CMS Cms {
    get { return (CMS)Page.Session["CMS"]; }
  }

  void Page_Load(object sender, EventArgs e) {
    obout_ASPTreeView_2_NET.Tree oTree = new obout_ASPTreeView_2_NET.Tree();

    // These 3 lines prevent from browser caching. They are optional.DS
    // Useful when your data changes frequently.
    Response.AddHeader("pragma","no-cache");
    Response.AddHeader("cache-control","private");
    Response.CacheControl = "no-cache";

    // For non-English characters. See MSDN for your language settings.
    //Response.CodePage = 1252;
    //Response.CharSet = "windows-1252";

    // IMPORTANT:  For loaded SubTree set to TRUE.
    oTree.SubTree = true;

    String id = Request["id"];

    DataSet ds = DB.GetDS("select * from filepath where deleted=0 and parentpath=" + id + " order by pathname");
    for ( int i=0; i < DB.GetRowCount(ds); i++) {
      String id2 = DB.GetInt(ds, i, "id").ToString();
      int count = DB.GetInt("select count(*) as nof from filepath where deleted=0 and parentpath=" + id2, "nof");
      bool hasChildren = count > 0;
      String html = (hasChildren ? "<span onClick='ob_t23(this)'>" + DB.GetString(ds, i, "pathname") + "</span>" : DB.GetString(ds, i, "pathname"));
      if (Cms.GetPermission(id2, "DocumentBank", "viewfolders", ""))
        oTree.Add("root", id2, html, false, null, "DynamicLoad.aspx?parent=" + id + "&id=" + id2);
    }

    oTree.FolderIcons = "/admin/TreeIcons/icons";
    oTree.FolderStyle = "/admin/TreeIcons/style/MediaBank";
    oTree.FolderScript = "TreeIcons/script/MediaBank";
    oTree.EditNodeEnable = true;
    oTree.DragAndDropEnable = true;
    oTree.MultiSelectEnable = false;

    String tree = oTree.HTML();

    // Remove trial info
    int idx = tree.IndexOf("<div style=\"font:11px verdana; background-color:white; border:3px solid #cccccc; color:#666666;");
    if (idx >= 0)
      tree = tree.Substring(0,idx);

    Response.Write(tree);
  }

</script>
