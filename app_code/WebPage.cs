/* $Date: 2010-11-17 15:25:18 +0100 (on, 17 nov 2010) $    $Revision: 7088 $ */
using System;
using System.Web;
using System.Data;
using System.Collections;
using System.Collections.Generic;

namespace NFN {

  public class WebPage : INFNPermission {

    private WebPage parent;
    private String id;
    private String pagename;
    private String pagestatus;
    private String languages;
    private bool flashpage;
    private String pagetitle;
    private String filename;
    private String prelimfilename;
    private String redirect;
    private String keywords;
    private String description;
    private bool timecontrolled;
    private bool allowquickchildren;
    private DateTime starttime;
    private DateTime endtime;
    private CMS Cms;
    private SortedList childpages = null;
    private DateTime moddate;
    private int modby;
    private int _orderno;
    private String _protection = null;
    private bool redirtochild;
    private PageProperty _mainprop;
    private PageProperty _sharedprop;
    private ArrayList _actions = null;
    private WebPage[] related = null;
    private Hashtable pageAttributeValues = null;


    ///<summary>Unique id of web page</summary>
    public String Id {
      get { return id; }
    }


    ///<summary>Name of web page</summary>
    public String Name {
      get { return pagename; }
      set { pagename = value; }
    }


    ///<summary>Status of web page [active|hidden|inactive]. If hidden, the page will not appear in menus. If inactive, the page will be inaccessible.</summary>
    public String Status {
      get { return pagestatus; }
      set { pagestatus = value; }
    }


    public String Languages {
      get { return languages; }
      set { languages = value; }
    }


    /// <summary>If not an empty string, contains the url to redirect to when the page loads. Default: empty string.</summary>
    public String Redirect {
      get { return redirect; }
      set { redirect = value; }
    }


    /// <summary>If true, redirection will be done to the first active child page. Default: true.</summary>
    public bool RedirToChild {
      get { return redirtochild; }
      set { redirtochild = value; }
    }


    ///<summary>Title of the web page</summary>
    public String Title {
      get { return pagetitle; }
      set { pagetitle = value; }
    }


    ///<summary>Flash movie to show on page</summary>
    public bool FlashPage {
      get { return flashpage; }
      set { flashpage = value; }
    }

    /// <summary>Keywords of the web page that will render in a meta tag</summary>
    public String KeyWords {
      get { return keywords; }
      set { keywords = value; }
    }


    /// <summary>Description of the web page that will render in a meta tag</summary>
    public String Description {
      get { return description; }
      set { description = value; }
    }


    ///<summary>Order number of web page</summary>
    public int OrderNo {
      get { return _orderno; }
      set { _orderno = value; }
    }

    public bool TimeControlled {
      get { return timecontrolled; }
      set { timecontrolled = value; }
    }

    public bool AllowQuickChildren {
      get { return allowquickchildren; }
      set { allowquickchildren = value; }
    }

    public DateTime StartTime {
      get { return starttime; }
      set { starttime = value; }
    }

    public DateTime EndTime {
      get { return endtime; }
      set { endtime = value; }
    }

    /// <summary>If true the page can only be viewed by a logged in user with the right permissions</summary>
    public bool IsProtected {
      get {
        if (Protection == "Y")
          return true;
        else if (Protection == "N")
          return false;
        else if (Parent != null)
          return Parent.IsProtected;
        else
          return false;
      }
    }

    public String Protection {
      get {
        if (_protection == null) return "";
        else return _protection;
      }
      set { _protection = value; }
    }
    
    // Array of id, name, type, alternatives
    public String[][] GetPageAttributes() {
      DataSet ds = DB.GetDS("select * from pagetemplateattrib where filename='" + Filename + "' order by orderno");
      String[][] pageAttributes = new String[DB.GetRowCount(ds)][];
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        pageAttributes[i] = new String[5];
        pageAttributes[i][0] = DB.GetString(ds, i, "id");
        pageAttributes[i][1] = DB.GetString(ds, i, "name");
        pageAttributes[i][2] = DB.GetString(ds, i, "type");
        pageAttributes[i][3] = DB.GetString(ds, i, "alternatives");
        pageAttributes[i][4] = DB.GetString(ds, i, "lang");
      }
      return pageAttributes;
    }

    public PageProperty GetControlProperty(String propName) {
      if (MainProp.HasProperty(propName))
        return MainProp.GetProperty(propName);
      else if (SharedProp != null && SharedProp.HasProperty(propName))
        return SharedProp.GetProperty(propName);
      else if (Cms.CommonProperty.HasProperty(propName))
        return Cms.CommonProperty.GetProperty(propName);
      else
        return MainProp.GetProperty(propName);
    }

    public String GetPropertyValue(String propName) {
      return GetControlProperty(propName).ControlTypeProperty.Value;
    }

    public Hashtable PageAttributeValues {
      get {
        if (pageAttributeValues == null) {
          pageAttributeValues = new Hashtable();
          String[][] pageAttrib = GetPageAttributes();
          PageProperty propProp = GetControlProperty("PageProperties");
          for (int i=0; i < pageAttrib.Length; i++) {
            String id = pageAttrib[i][0];
            pageAttributeValues.Add(id, propProp.GetProperty(id).Value);
          }
        }
        return pageAttributeValues;
      }
    }
    
    public String GetPageAttributeValue(String id) {
      Object o = PageAttributeValues[id];
      return (o == null ? "" : o.ToString());
    }
    
    public static String GetPageAttributeValue(String pageId, String attribId) {
      return DB.GetString("select p2.propertyvalue from WebPage w, PageProperty p1, PageProperty p2 where w.id='" + pageId + "' and p1.parentproperty=w.mainprop and p1.propertyname='PageProperties' and p2.parentproperty=p1.id and p2.propertyname='" + attribId + "'", "propertyvalue");
    }
    
    public void SavePageAttributeValue(String id, String value) {
      PageProperty propProp = GetControlProperty("PageProperties");
      PageProperty subProp = propProp.GetProperty(id);
      subProp.PrelimValue = value;
      subProp.WriteToDB();
      propProp.Publish(true);
    }

    public DataSet GetSubPagesWithPageAttribute(String propname, String propval) {
      String sql = "select w.* from webpage w, pageproperty p1, pageproperty p2 where w.deleted=0 and w.parentpage='" + Id + "' and p1.parentproperty=w.mainprop and p1.propertyname='PageProperties' and p2.parentproperty=p1.id and p2.propertyname='" + propname + "' and p2.propertyvalue like '" + propval + "' order by w.moddate desc";
      return DB.GetDS(sql);
    }

    public static DataSet GetPagesWithPageAttribute(String propname, String propval) {
      String sql = "select w.* from webpage w, pageproperty p1, pageproperty p2 where w.deleted=0 and p1.parentproperty=w.mainprop and p1.propertyname='PageProperties' and p2.parentproperty=p1.id and p2.propertyname='" + propname + "' and p2.propertyvalue like '" + propval + "' order by w.moddate desc";
      return DB.GetDS(sql);
    }

    public WebPage[] RelatedPages {
      get {
        if (related == null) {
          DataSet dss = DB.GetDS("select r.page2 from relatedpages r, WebPage p where r.page1='" + Id + "' and p.id=r.page2 and r.lang='" + Cms.Language + "' and p.languages like '%" + Cms.Language + "%'");
          related = new WebPage[DB.GetRowCount(dss)];
          for (int i=0; i < DB.GetRowCount(dss); i++) 
            related[i] = Cms.GetPageById(DB.GetString(dss, i, "page2"));
        }
        return related;
      }
    }

    public String GetRelatedPagesTree() {
      String html = "<ul id='relatedpages' class='filetree'>";
      DataSet ds = DB.GetDS("select * from webpage where pagestatus<>'inactive' and deleted=0 and languages like '%" + Cms.Language + "%' and (parentpage is null or parentpage='') order by orderno");
      for (int i=0; i < DB.GetRowCount(ds); i++) 
        html += GetRelatedNode(DB.GetString(ds, i, "id"), DB.GetString(ds, i, "name"), DB.GetString(ds, i, "pagestatus"), RelatedPages, Id);
      html += "</ul>";
      return html;
    }

    private String GetRelatedNode(String pageid, String pagename, String status, WebPage[] relPages, String currpage) {
      bool isrel = false;
      for (int i=0; i < relPages.Length && !isrel; i++)
        isrel = (relPages[i] != null && pageid == relPages[i].Id);

      String html = (isrel ? "<li class='selected'>" : "<li>");
      DataSet ds = DB.GetDS("select * from webpage where deleted=0 and parentpage='" + pageid + "' and languages like '%" + Cms.Language + "%' order by orderno");
      String input = (pageid == currpage ? "<span class='file " + (status != "active" ? "inactivepage" : "") + "'>" + pagename + "</span>" : "<span class='file " + (status != "active" ? "inactivepage" : "") + "'><label for='rp_" + pageid + "'>" + pagename + "</label><input type='checkbox' id='rp_" + pageid + "' " + (isrel ? "checked" : "") + " /></span>");
      if (DB.GetRowCount(ds) == 0)
        html += input;
      else {
        html += "<div class='hitarea collapsable-hitarea'></div>" + input + "<ul>";
        for (int i=0; i < DB.GetRowCount(ds); i++) 
          html += GetRelatedNode(DB.GetString(ds, i, "id"), DB.GetString(ds, i, "name"), DB.GetString(ds, i, "pagestatus"), relPages, currpage);
        html += "</ul>";
      }
      html += "</li>";
      return html;
    }


    public DateTime GetLastModDate() {
      return MainProp.LastModDate;
    }

    public static DateTime GetLastModDate(String pageId) {
      int propid = DB.GetInt("select mainprop from webpage where id='" + pageId + "'", "mainprop");
      return PageProperty.GetLastModDate(propid);
    }

    /// <summary>The main property of the web page</summary>
    public PageProperty MainProp {
      get { return _mainprop; }
    }


    /// <summary>The topmost page (no parent) in the page hirerarchy and in the branch of the current page.</summary>
    public WebPage MainPage {
      get {
        WebPage aPage = this;
        while (aPage.Parent != null)
          aPage = aPage.Parent;
        return aPage;
      }
    }


    /// <summary>Id of the main property of the page. If set to a non existing property, a new property is created.</summary>
    public int MainPropId {
      get { return MainProp.Id; }
      set {
        DataSet ds = DB.GetDS("select * from pageproperty where deleted <> 1 and Id = " + value);
        _mainprop = null;
        if (DB.GetRowCount(ds) > 0)
          _mainprop = new PageProperty(Cms, DB.GetRow(ds, 0), null);
        else
          throw (new Exception("Web page main property missing"));
      }
    }


    /// <summary>Main shared property of the web page</summary>
    public PageProperty SharedProp {
      get { return _sharedprop; }
      set { _sharedprop = value; }
    }


    /// <summary>Id of the main shared property of the page. If set to a non existing property, a new property is created.</summary>
    public int SharedPropId {
      get { return (SharedProp == null ? 0 : SharedProp.Id); }
      set {
        int aid = (_sharedprop == null ? -1 : _sharedprop.Id);
        if (aid != value) {
          _sharedprop = null;
          DataSet ds = DB.GetDS("select * from pageproperty where deleted <> 1 and Id = " + value);
          if (DB.GetRowCount(ds) > 0)
            _sharedprop = new PageProperty(Cms, DB.GetRow(ds, 0), null);
        }
      }
    }


    /// <summary>File name (aspx) of current web page</summary>
    public String Filename {
      get { return (GetPermission("edit") && filename != prelimfilename && prelimfilename.Length > 0 ? prelimfilename : filename); }
      set { filename = value; }
    }

    public String PublishedFilename {
      get { return filename; }
      set { filename = value; }
    }

    public String PrelimFilename {
      get { return prelimfilename; }
      set { prelimfilename = value; }
    }

    public String TemplateCategory {
      get { return DB.GetString("select category from webpagetemplate where filename='" + Filename + "'", "category");  }
    }

    ///<summary>Parent web page of the current web page</summary>
    public WebPage Parent {
      get { return parent; }
      set {
        if (parent != null) parent.RemovePage(this);
        parent = value;
        if (parent != null) parent.AddPage(this);
      }
    }


    ///<summary>Id of the user who did the latest modification of the page.</summary>
    public int ModBy {
      get { return modby; }
    }


    ///<summary>Date for the latest modification</summary>
    public DateTime ModDate {
      get { return moddate; }
    }


    public bool InTimeSpan {
      get { return (!TimeControlled || DateTime.Parse(StartTime.ToString("yyyy-MM-dd")) <= DateTime.Now && DateTime.Parse(EndTime.AddDays(1).ToString("yyyy-MM-dd")) >= DateTime.Now); }
    }
    
    public static bool IsInTimeSpan(String pageId) {
      String now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
      int nof = DB.GetInt("select count(*) as nof from webpage where id='" + pageId + "' and timecontrolled=0 or (starttime < '" + now + "' and endtime > '" + now + "')", "nof");
      return nof > 0;
    }

    public String GetNextSubpageId() {
      int count = 1;
      while (DB.RowExists("select id from webpage where id='" + Id + "_" + count.ToString() + "'"))
        count++;
      return Id + "_" + count.ToString();
    }

    /// <summary>A list of all actions for a WebPage for which permissions can be set (read from table PermissionTypes).</summary>
    public ArrayList Actions {
      get {
        if (_actions == null) {
          _actions = new ArrayList();
          String sql = "select * from permissiontypes where itemtype='WebPage' order by orderno";
          DataSet ds = DB.GetDS(sql);
          for (int i = 0; i < DB.GetRowCount(ds); i++)
            _actions.Add(DB.GetString(ds, i, "actiontype") + ";" + DB.GetString(ds, i, "description"));
        }
        return _actions;
      }
    }

    /// <summary>Returns true if permission is granted for the given role to perform the given action on the current WebPage</summary>
    /// <param name="role">Role for which permission is checked</param>
    /// <param name="action">Action for which permission is checked</param>
    public bool GetRolePermission(String role, String actiontype) {
      return Cms.GetRolePermission(this.Id, role, "WebPage", actiontype, "");
    }

    /// <summary>Returns true if permission is granted for the logged in user to perform the given action on the current WebPage</summary>
    /// <param name="role">Role for which permission is checked</param>
    /// <param name="action">Action for which permission is checked</param>
    public bool GetPermission(String actiontype) {
      return Cms.GetPermission(this.Id, "WebPage", actiontype, "");
    }


    /// <summary>Sets permission for the given role to perform the given action on the current WebPage</summary>
    /// <param name="role">Role to ser permission to</param>
    /// <param name="actiontype">Action to ser permission to</param>
    /// <param name="permission">True if permission is granted</param>
    public void SetRolePermission(String role, String actiontype, bool permission) {
      Cms.SetRolePermission(this.Id, "WebPage", role, actiontype, "", permission);
    }


    /// <summary>Returns true if the current user has permission to view the current WebPage</summary>
    public bool ViewPermission {
      get {
        if (IsProtected && !Cms.User.LoggedIn) return false;
        if (!IsProtected || Cms.User.IsSysAdmin) return true;
        return GetPermission("View");
      }
    }


    /// <summary>Returns true if the current user has permission to edit the current WebPage</summary>
    public bool EditPermission {
      get {
        if (!Cms.User.IsAdmin) return false;
        if (Cms.User.IsSysAdmin) return true;
        return GetPermission("Edit");
      }
    }

    ///<summary>Writes the info of the current WebPage to the database</summary>
    public void WriteToDB() {
      String sql = "update webpage set name='" + DB.FixApostrophe(Name) + "', title='" + DB.FixApostrophe(Title) + "', flashpage=" + (FlashPage ? 1 : 0) + ", pagestatus='" + Status + "', languages='" + Languages + "', filename='" + filename + "', prelimfilename='" + prelimfilename + "', orderno='" + OrderNo.ToString() + "', " +
        "keywords='" + KeyWords + "', redirecttochild=" + (RedirToChild ? 1 : 0) + ", redirectto='" + Redirect + "', description='" + DB.FixApostrophe(Description) + "', allowquickchildren=" + (AllowQuickChildren ? "1" : "0") + ", timecontrolled=" + (TimeControlled ? "1" : "0") + ", starttime='" + StartTime.ToString(CMS.SiteSetting("dateTimeFormat")) + "', endtime='" + EndTime.ToString(CMS.SiteSetting("dateTimeFormat")) + "'";
      if (Parent != null)
        sql += ", parentpage='" + Parent.Id.ToString() + "'";
      if (SharedProp != null)
        sql += ", sharedprop=" + SharedProp.Id.ToString();
      sql += ", mainprop=" + MainProp.Id.ToString() + ", protected='" + Protection + "', moddate='" + DateTime.Now.ToString(CMS.SiteSetting("dateTimeFormat")) + "', modby=" + Cms.User.Id + " where Id='" + Id + "'";
      Cms.LogEvent("savepage", this.Id);

      DB.ExecSql(sql);
    }


    /// <summary>Reads the info of the current WebPage from the database</summary>
    public void Refresh() {
      DataSet ds = DB.GetDS("select * from webpage where deleted=0 and id='" + Id + "'");
      DataRow aRow = ds.Tables[0].Rows[0];
      Init(Cms, aRow);
      if (aRow["parentpage"] == System.DBNull.Value) Parent = null;
      else Parent = Cms.GetPageById(aRow["parentpage"].ToString());
    }


    /// <summary>Initializes a new instance of the WebPage class given an explicit parent web page.</summary>
    /// <param name="Cms">Parent <see cref="CMS"/> class</param>
    /// <param name="row"><see cref="DataRow"/> containing information about the web page</param>
    /// <param name="aParent">Parent <see cref="WebPage"/> of the instantiated web page</param>
    public WebPage(CMS Cms, DataRow row, WebPage aParent) {
      Init(Cms, row);
      Parent = aParent;
    }


    /// <summary>Initializes a new instance of the WebPage class</summary>
    /// <param name="BW">Parent <see cref="CMS"/> class</param>
    /// <param name="row"><see cref="DataRow"/> containing information about the web page</param>
    public WebPage(CMS Cms, DataRow row) {
      Init(Cms, row);
      if (row["parentpage"] == System.DBNull.Value) Parent = null;
      else Parent = Cms.GetPageById(row["parentpage"].ToString());
    }


    /// <summary>Initializes the WebPage by reading from the supplied <see cref="DataRow"/></summary>
    /// <param name="Cms">Parent <see cref="CMS"/> class</param>
    /// <param name="row"><see cref="DataRow"/> containing information about the web page</param>
    private void Init(CMS Cms, DataRow row) {
      this.Cms = Cms;
      id = row["id"].ToString();
      Name = row["name"] == null ? "" : row["name"].ToString();
      Title = row["title"] == null ? "" : row["title"].ToString();
      FlashPage = (Convert.ToInt32(row["flashpage"]) == 1);
      Status = row["pagestatus"] == null ? "inactive" : row["pagestatus"].ToString();
      Languages = row["languages"] == null ? "sv" : row["languages"].ToString();
      KeyWords = row["keywords"] == null ? "" : row["keywords"].ToString();
      Redirect = row["redirectto"] == null ? "" : row["redirectto"].ToString();
      Description = row["description"] == null ? "" : row["description"].ToString();
      RedirToChild = (Convert.ToInt32(row["redirecttochild"]) == 1);
      TimeControlled = (Convert.ToInt32(row["timecontrolled"]) == 1);
      AllowQuickChildren = (Convert.ToInt32(row["allowquickchildren"]) == 1);
      try { StartTime = DateTime.Parse(row["starttime"].ToString()); }
      catch { StartTime = DateTime.Now; }
      try { EndTime = DateTime.Parse(row["endtime"].ToString()); }
      catch { EndTime = DateTime.Now; }
      filename = row["filename"] == null ? null : row["filename"].ToString();
      prelimfilename = row["prelimfilename"] == null ? "" : row["prelimfilename"].ToString();
      //      String pId = row["parentpage"] == null ? "" : row["parentpage"].ToString();
      //      if (pId.Length > 0) Parent = Cms.GetPageById(pId);
      try { moddate = DateTime.Parse(row["moddate"].ToString()); }
      catch { moddate = DateTime.MinValue; }
      try { modby = Convert.ToInt32(row["modby"]); }
      catch { modby = 0; }
      try { _orderno = Convert.ToInt32(row["orderno"]); }
      catch { _orderno = 0; }
      _protection = row["protected"].ToString();

      if (row["mainprop"] == System.DBNull.Value) {
        _mainprop = new PageProperty(Cms, null, Name, null);
        DB.ExecSql("update webpage set mainprop=" + MainProp.Id + " where Id='" + Id + "'");
      }
      else {
        MainPropId = Convert.ToInt32(row["mainprop"]);
      }

      if (row["sharedprop"] == System.DBNull.Value) {
        _sharedprop = new PageProperty(Cms, null, Name, null);
        DB.ExecSql("update webpage set sharedprop=" + SharedProp.Id + " where Id='" + Id + "'");
      }
      else {
        SharedPropId = Convert.ToInt32(row["sharedprop"]);
      }

      /*      DataSet ds2 = DB.GetDS("select * from webpage where deleted=0 and parentpage = '" + Id + "' order by orderno");
            for (int i=0; i < DB.GetRowCount(ds2); i++) {
              DataRow pagerow = DB.GetRow(ds2, i);
              WebPage aPage = new WebPage(this.Cms, pagerow, this);
              AddPage(aPage);
            }*/
    }


    /// <summary>The level of the web page in the web page hierarchy</summary>
    public int PageLevel {
      get {
        int level = 0;
        WebPage aPage = this;
        while (aPage.Parent != null) {
          aPage = aPage.Parent;
          level++;
        }
        return level;
      }
    }


    /// <summary>The <see cref="WebPage"/> to show when the current page is requested. This can be the current page or the first child page of the current page.</summary>
    public WebPage DisplayPage {
      get {
        WebPage aPage = this;
        if (RedirToChild) {
          aPage = FirstActiveChildPage;
          if (aPage == null) aPage = this;
        }
        return aPage;
      }
    }


    /// <summary>Adds a child web page to the current web page</summary>
    /// <param name="pagerow"><see cref="DataRow"/> to read child page information from</param>
    public void AddPage(DataRow pagerow) {
      AddPage(new WebPage(Cms, pagerow, this));
    }


    /// <summary>Adds a child web page to the current web page</summary>
    /// <param name="aPage">WebPage to add</param>
    public void AddPage(WebPage aPage) {
      bool found = false;
      for (int i = 0; i < ChildPages.Count && !found; i++)
        found = ((WebPage)ChildPages.GetByIndex(i)).id == aPage.id;

      if (!found) {
        int aOrder = aPage.OrderNo;
        while (ChildPages.ContainsKey(aOrder))
          aOrder++;
        if (aPage.OrderNo != aOrder) {
          aPage.OrderNo = aOrder;
          aPage.WriteToDB();
        }
        ChildPages.Add(aOrder, aPage);
      }
    }


    /// <summary>Removes a child web page from the current web page</summary>
    /// <param name="aPage">WebPage to remove</param>
    public void RemovePage(WebPage aPage) {
      ChildPages.Remove(aPage.OrderNo);
    }


    /// <summary>Deletes the current web page and its properties from the database</summary>
    public void Delete() {
      DataSet ds = DB.GetDS("select count(*) as nof from webpage where deleted=0 and id <> '" + Id + "' and mainprop=" + MainProp.Id);
      if (DB.GetInt(ds, 0, "nof") == 0)
        MainProp.Delete();
      if (SharedProp != null) {
        int aCount = DB.GetInt("select count(*) as nof from webpage where deleted=0 and id <> '" + Id + "' and sharedprop=" + SharedProp.Id, "nof");
        if (aCount == 0)
          SharedProp.Delete();
      }
      if (Parent != null) Parent.RemovePage(this);
      DB.ExecSql("delete from relatedpages where page1='" + Id + "' or page2='" + Id + "'");
      DB.ExecSql("update webpage set deleted=1 where id='" + Id + "'");
      for (int i = 0; i < ChildPages.Count; i++)
        ((WebPage)ChildPages.GetByIndex(i)).Delete();
      
      Cms.LogEvent("deletepage", Id);
      Cms.Refresh();
    }


    /// <summary>Deletes the current web page and its child pages from the database</summary>
    public void DeleteRecursive() {
      ArrayList ids = new ArrayList();
      foreach (DictionaryEntry child in ChildPages)
        ids.Add(((WebPage)child.Value).Id);
      foreach (String id in ids)
        GetChildPageById(id).DeleteRecursive();
      Delete();
    }


    /// <summary>The number of child pages of the current web page</summary>
    public int ChildPageCount {
      get { return ChildPages.Count; }
    }


    /// <summary>Returns the first active child page of the current page</summary>
    public WebPage FirstActiveChildPage {
      get {
        DataSet ds = DB.GetDS("select id, protected from webpage where deleted=0 and parentpage='" + this.Id + "' and languages like '%" + Cms.Language + "%' and pagestatus='active' and (timecontrolled <> 1 or starttime <= GETDATE() and endtime >= GETDATE()) order by orderno");
        WebPage resPage = null;
        for (int i = 0; i < DB.GetRowCount(ds) && resPage == null; i++) {
          String currId = DB.GetString(ds, i, "id");
          WebPage aPage = Cms.GetPageById(currId);
          if (!aPage.IsProtected || Cms.GetPermission(currId, "WebPage", "View"))
            resPage = aPage;
        }
        return resPage;
      }
    }

    /// <summary>Returns a list containing child pages of the current page</summary>
    public SortedList ChildPages {
      get {
        if (childpages == null) 
          childpages = new SortedList();
        return childpages;
      }
    }
    
    public void UpdateChildPages() {
      ChildPages.Clear();
      DataSet ds = DB.GetDS("select * from webpage where deleted=0 and parentpage='" + Id + "' order by orderno");
      int orderno = 0;
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        String id = DB.GetString(ds, i, "id");
        WebPage apage = Cms.GetPageById(id);
        apage.OrderNo = orderno;
        DB.ExecSql("update webpage set orderno=" + orderno + " where id='" + id + "'");
        ChildPages.Add(orderno, apage);
        orderno++;
      }
    }

    /// <summary>Returns a child web page of the current web page. If no child with the given id is found, null is returned.</summary>
    /// <param name="pageid">Id of the child web page</param>
    /// <returns>Child <see cref="WebPage"/></returns>
    public WebPage GetChildPageById(String pageid) {
      if (pageid == Id)
        return this;
      else {
        int sp = 0;
        WebPage aPage = null;
        while (sp < ChildPages.Count && aPage == null) {
          aPage = ((WebPage)ChildPages.GetByIndex(sp)).GetChildPageById(pageid);
          sp++;
        }
        return aPage;
      }
    }


    /// <summary>Returns child web page with tre order number pageno</summary>
    /// <param name="orderno">Order number of the child web page</param>
    /// <returns>Child <see cref="WebPage"/></returns>
    public WebPage GetChildPage(int orderno) {
      if (orderno <= ChildPages.Count) return (WebPage)ChildPages.GetByIndex(orderno - 1);
      else return null;
    }

    public WebPage GetChildPageLang(int orderno) {
      DataSet ds = DB.GetDS("select id from webpage where deleted=0 and languages like '%" + Cms.Language + "%' and pagestatus='active' and parentpage='" + Id + "' order by orderno");
      if (DB.GetRowCount(ds) >= orderno)
        return Cms.GetPageById(DB.GetString(ds, orderno-1, "id"));
      else
        return null;
    }
    
    
    public int GetChildOrder() {
      String snip = (Parent == null ? "is null" : "='" + Parent.Id + "'");
      DataSet ds = DB.GetDS("select id from webpage where parentpage " + snip + " and pagestatus='active' and languages like '%" + Cms.Language + "%' and deleted=0 order by orderno");
      int order = -1;
      for (int i=0; i < DB.GetRowCount(ds) && order == -1; i++)
        if (DB.GetString(ds, i, "id") == Id)
          order = i;
      return order + 1;
    }

  }

}
