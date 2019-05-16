/* $Date: 2010-08-19 12:01:42 +0200 (to, 19 aug 2010) $    $Revision: 6796 $ */
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Xml;
using System.Collections;
using AjaxPro;

namespace NFN {
  
  public class BaseMaster : MasterPage {

    /// <summary></summary>
    public HtmlGenericControl DetectScreenRes;

    private WebPage _thispage = null;
    private System.Globalization.DateTimeFormatInfo swedate = null;
    private String urchinId = "";
    private String pageId = "";

    /// <summary>Information about the browser making the request</summary>
    public System.Web.HttpBrowserCapabilities Browser {
      get { return this.Request.Browser; }
    }

    private bool XmlRequest {
      get { return Request["getxml"] == "yes"; }
    }


    /// <summary>The default pageid of the application</summary>
    public String DefaultPageId {
      get { return CMS.SiteSetting("defaultPage"); }
    }


    /// <summary>The id of the current page</summary>
    public virtual String PageId {
      get {
        if (pageId.Length == 0)
          pageId = (Request["pageid"] != null ? Request["pageid"] : DefaultPageId);
        return pageId;
      }
      set { pageId = value; }
    }


    /// <summary>The id of the parent page of the current page</summary>
    public String ParentPageId {
      get { return ThisPage.Parent.Id; }
    }


    /// <summary>Id used by Google Analytics </summary>
    public String UrchinId {
      get { return urchinId; }
      set { urchinId = value; }
    }


    public CMS Cms {
      get { return (CMS)HttpContext.Current.Session["CMS"]; }
    }

    private BasePage BWPage {
      get {
        if (Page.GetType() == typeof(BasePage))
          return (BasePage)Page;
        else
          return null;
      }
    }

    /// <summary>The <see cref="WebPage"/> object of the current page</summary>
    public WebPage ThisPage {
      get {
        if (_thispage == null)
          _thispage = Cms.GetPageById(PageId).DisplayPage;
        if ((_thispage.Status == "inactive" || !_thispage.InTimeSpan) && !Cms.User.IsAdmin)
          _thispage = Cms.GetPageById(DefaultPageId).DisplayPage;
        return _thispage;
      }
      set {
        _thispage = value;
      }
    }


    /// <summary>Swedish DateTime format</summary>
    public System.Globalization.DateTimeFormatInfo SweDate {
      get {
        if (swedate == null) swedate = new System.Globalization.CultureInfo("sv-SE", true).DateTimeFormat;
        return swedate;
      }
    }


    /// <summary>Overrides the <see cref="MasterPage"/> OnLoad method</summary>
    /// <param name="e"><see cref="EventArgs"/></param>
    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);
      AjaxPro.Utility.RegisterTypeForAjax(typeof(BaseMaster));

      if (Cms.User.IsAdmin || !IsPostBack) ThisPage.Refresh();

      if (!IsPostBack) {
        BindMenus();

        ThisPage.MainProp.Refresh();
        ThisPage.SharedProp.Refresh();
        Cms.CommonProperty.Refresh();

        WebPage displayPage = ThisPage.DisplayPage;
        if (PageId != displayPage.Id)
          Response.Redirect("~/" + displayPage.Id + ".aspx");

        if (ThisPage.Redirect != null && ThisPage.Redirect.Length > 0)
          Response.Redirect(ThisPage.Redirect);
      }
      NotifyDatabase();
    }


    public void NotifyDatabase() {
      String sid = HttpContext.Current.Session.SessionID;
      DB.ExecSql("update sessions set lastactiontime='" + DateTime.Now.ToString(CMS.SiteSetting("dateTimeFormat")) + "' where sessionid='" + sid + "'");
    }


    /// <summary>Recursive method to get the publication date of the latest published child property</summary>
    private DateTime GetPropertyPublishedDate(int propId, DateTime currDate) {
      DataSet ds = DB.GetDS("select id, publisheddate from pageproperty where parentproperty=" + propId);
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        int aId = DB.GetInt(ds, i, "id");
        DateTime aDate = DB.GetDate(ds, i, "publisheddate");
        if (aDate > currDate) currDate = aDate;
        currDate = GetPropertyPublishedDate(aId, currDate);
      }
      return currDate;
    }


    /// <summary>Returns the publication date for the latest published property of the page</summary>
    public DateTime LastContentPublished {
      get {
        DateTime aDate = DB.GetDate("select publisheddate from pageproperty where id=" + ThisPage.MainProp.Id, "publisheddate", DateTime.Parse("2000-01-01"));
        return GetPropertyPublishedDate(ThisPage.MainProp.Id, aDate);
      }
    }

    /// <summary>Recursive method to get the modification date of the latest modified child property</summary>
    private DateTime GetPropertyModifiedDate(int propId, DateTime currDate) {
      DataSet ds = DB.GetDS("select id, modifieddate from pageproperty where parentproperty=" + propId);
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        int aId = DB.GetInt(ds, i, "id");
        DateTime aDate = DB.GetDate(ds, i, "modifieddate");
        if (aDate > currDate) currDate = aDate;
        currDate = GetPropertyModifiedDate(aId, currDate);
      }
      return currDate;
    }


    /// <summary>Returns the modification date for the latest modified property of the page</summary>
    public DateTime LastContentModified {
      get {
        DateTime aDate = DB.GetDate("select modifieddate from pageproperty where id=" + ThisPage.MainProp.Id, "modifieddate", DateTime.Parse("2000-01-01"));
        return GetPropertyModifiedDate(ThisPage.MainProp.Id, aDate);
      }
    }


    /// <summary>Returns a <see cref="DataTable"/> with data for the menu of the given level</summary>
    /// <param name="menuLevel">Level for the menu (>=0)</param>
    /// <param name="includeDefaultPage">If the default page should be included in the menu</param>
    protected virtual DataTable GetMenuData(int menuLevel, bool includeDefaultPage) {
      WebPage parentPage = ThisPage;
      String selPage = "";
      String sqlSnip1 = " is null ";
      String sqlSnip2 = (includeDefaultPage ? "" : " and id <> '" + DefaultPageId + "' ");

      int counter = ThisPage.PageLevel;
      while (counter >= menuLevel) {
        if (counter == menuLevel)
          selPage = parentPage.Id;
        parentPage = parentPage.Parent;
        if (counter == menuLevel && parentPage != null)
          sqlSnip1 = " ='" + parentPage.Id + "' ";
        counter--;
      }

      DataTable dt = DB.GetDS("select id, name, protected, 1 as enabled from webpage where deleted=0 and parentpage" + sqlSnip1 + " and languages like '%" + Cms.Language + "%' and pagestatus='active'" + sqlSnip2 + " order by orderno").Tables[0];
      dt.Columns.Add(new DataColumn("url", typeof(string)));
      dt.Columns.Add(new DataColumn("gif", typeof(string)));
      dt.Columns.Add(new DataColumn("selgif", typeof(string)));
      dt.Columns.Add(new DataColumn("first", typeof(bool)));
      dt.Columns.Add(new DataColumn("last", typeof(bool)));

      DataRow dr;
      ArrayList delrows = new ArrayList();
      for (int i = 0; i < dt.Rows.Count; i++) {
        dr = dt.Rows[i];
        String aId = dr["id"].ToString().ToLower();
        object prot = dr["protected"];
        if (prot != null && prot.ToString() == "Y" && !Cms.GetPermission(aId, "WebPage", "View", ""))
          delrows.Add(i);
        if (aId == selPage)
          dr["enabled"] = 0;
        dr["url"] = "/" + aId + ".aspx";
        dr["gif"] = (aId == selPage ? "/gfx/nav/" + aId + "_a.gif" : "/gfx/nav/" + aId + ".gif");
        dr["selgif"] = "/gfx/nav/" + aId + "_a.gif";
        dr["first"] = i == 0;
        dr["last"] = i == dt.Rows.Count - 1;
      }
      for (int i = delrows.Count - 1; i >= 0; i--)
        dt.Rows.RemoveAt(Convert.ToInt32(delrows[i]));
      return dt;
    }


    /// <summary>Binds data to the menus of the page. The menus must be of class <see cref="Repeater"/> and have ids of the form Menu[manuLevel]</summary>
    public virtual void BindMenus() {
      for (int i = 0; i <= ThisPage.PageLevel; i++) {
        Repeater aMenu = (Repeater)this.FindControl("Menu" + i);
        if (aMenu != null) {
          aMenu.DataSource = GetMenuData(i, false).DefaultView;
          aMenu.DataBind();
        }
      }
    }

    /// <summary>Used for flash page</summary>
    private XmlDocument XmlDoc {
      get {
        XmlDocument doc = new XmlDocument();
        XmlElement page = doc.CreateElement("page");
        doc.AppendChild(page);
        XmlElement tmpl = doc.CreateElement("template");
        tmpl.InnerText = System.IO.Path.GetFileNameWithoutExtension(BWPage.ThisPage.Filename);
        page.AppendChild(tmpl);
        XmlElement items = doc.CreateElement("items");
        for (int i = 0; i < ThisPage.MainProp.ChildPropertiesSorted.Count; i++) {
          PageProperty prop = (PageProperty)ThisPage.MainProp.ChildPropertiesSorted[i];
          if (prop.HasProperty("text")) {
            XmlElement item = doc.CreateElement("item");
            item.SetAttribute("name", prop.PropertyName);
            XmlCDataSection data = doc.CreateCDataSection(prop.GetProperty("text").Value);
            item.AppendChild(data);
            items.AppendChild(item);
          }
        }
        page.AppendChild(items);

        XmlDeclaration xmldecl;
        xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
        xmldecl.Encoding = "UTF-8";
        XmlElement root = doc.DocumentElement;
        doc.InsertBefore(xmldecl, root);
        return doc;
      }
    }

    /// <summary>Renders the BrandInternet logo at the bottom of the page</summary>
    protected override void Render(HtmlTextWriter writer) {
      if (XmlRequest) {
        XmlTextWriter xmlwriter = new XmlTextWriter(writer);
        xmlwriter.Formatting = Formatting.Indented;
        XmlDoc.WriteTo(xmlwriter);
        xmlwriter.Flush();
      }
      else {
        if (BWPage != null && BWPage.ThisPage.FlashPage) {
          Control flashholder = FindControlRecursive(this, "FlashHolder");
          Control htmlholder = FindControlRecursive(this, "HtmlHolder");
          if (flashholder != null) flashholder.Visible = ShowFlash;
          if (htmlholder != null) htmlholder.Visible = !ShowFlash;
        }
        base.Render(writer);
        String logo = @"
  <!--
  Created with
   __     __   _        _
  |  ||_||  | |_||  | ||_
  |__|  ||__| |  |_ |_| _|
  www.040.se
  -->";
        writer.Write("\r\n" + logo);
      }
    }

    private bool ShowFlash {
      get { return (BWPage.ThisPage.FlashPage && (!Cms.User.IsAdmin || !Cms.GetPermission(ThisPage.Id, "WebPage", "edit"))); }
    }

    public Control FindControlRecursive(Control root, String Id) {
      if (root.ID == Id) return root;
      foreach (Control ctrl in root.Controls) {
        Control foundCtl = FindControlRecursive(ctrl, Id);
        if (foundCtl != null) return foundCtl;
      }
      return null;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String Translate(String txt) {
      return Cms.Translate(txt);
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String[] TranslateArr(String[] txt) {
      String[] res = new String[txt.Length];
      for (int i=0; i < res.Length; i++)
        res[i] = Cms.Translate(txt[i]);
      return res;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public virtual bool Login(String uname, String pwd) {
      String res = Cms.LogIn(uname, pwd);
      return res.Length == 0;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public virtual void Logout() {
      Cms.LogOut();
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String RequestPassword(String uname, String email) {
      String res = Cms.RequestPassword(uname, email);
      if (res == "NoInput") return Translate("Ange användarnamn eller e-postadress.");
      else if (res == "NoUser") return Translate("Ingen användare med det angivna användarnamnet existerar.");
      else if (res == "NoUserWithMail") return Translate("Ingen användare med den angivna e-postadressen existerar.");
      else if (res == "NoMailForUser") return Translate("Ingen e-postadress finns angiven för användare") + " " + uname + ".";
      else if (res.StartsWith("Error")) return res;
      else return "OK" + Translate("Ett mejl med inloggningsuppgifter har skickats till adressen") + " " + res + ".";
    }

  }

}
