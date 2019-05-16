<%-- $Date: 2009-11-26 12:24:59 +0100 (to, 26 nov 2009) $    $Revision: 5668 $ --%>
<%@ Page Language="C#"%>
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

    DataSet ds = DB.GetDS("select * from webpage where deleted=0 and parentpage='" + id + "' order by orderno");
    for ( int i=0; i < DB.GetRowCount(ds); i++) {
      String newid = "_s_" + DB.GetString(ds, i, "id");
      oTree.Add("root", newid, DB.GetString(ds, i, "name"), false, "ie_link.gif", "DynamicLoad.aspx?id=" + newid);
    }

    oTree.FolderIcons = "obout/treeview/icons";
    oTree.FolderStyle = "obout/treeview/style/Pages";
    oTree.FolderScript = "obout/treeview/script/Pages";
    oTree.EditNodeEnable = true;
    oTree.DragAndDropEnable = true;
    oTree.MultiSelectEnable = false;

    String tree = oTree.HTML();
    int idx = tree.IndexOf("<div style=\"font:11px verdana; background-color:white; border:3px solid #cccccc; color:#666666;");
    if (idx >= 0)
      tree = tree.Substring(0,idx);

    Response.Write(tree);
  }

</script>
