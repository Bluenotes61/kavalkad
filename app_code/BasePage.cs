/* $Date: 2010-11-22 12:39:57 +0100 (m√•, 22 nov 2010) $    $Revision: 7092 $ */
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Mail;
using System.Threading;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Collections;
using System.IO;
using System.Web.SessionState;
using System.Configuration;
using NFN.Controls;
using AjaxPro;

namespace NFN {

  public class BasePage : System.Web.UI.Page {
  
    private WebPage _thispage;
  
    protected override void OnInit(EventArgs e) {
      base.OnInit(e);
//      this.EnableViewState = false;
    }
    
    public override HttpSessionState Session {
      get { return HttpContext.Current.Session; }
    }

    public BaseMaster ThisMaster {
      get { return (Master != null ? (BaseMaster)Master : null); }
    }

    /// <summary>The default pageid of the application</summary>
    public String DefaultPageId {
      get { return ThisMaster.DefaultPageId; }
    }


    /// <summary>The id of the current page</summary>
    public virtual String PageId {
      get {
        if (ThisMaster != null) return ThisMaster.PageId;
        else return (Request["pageid"] != null ? Request["pageid"] : DefaultPageId);
      }
      set { ThisMaster.PageId = value; }
    }


    /// <summary>The id of the parent page of the current page</summary>
    public String ParentPageId {
      get { return ThisMaster.ParentPageId; }
    }


    /// <summary><see cref="BWInterface"/> containing database methods. Stored in the <see cref="HttpApplicationState"/> object</summary>
    public CMS Cms {
      get { return (CMS)Session["CMS"]; }
    }


    /// <summary>The <see cref="WebPage"/> object of the current page</summary>
    public WebPage ThisPage {
      get {
        if (ThisMaster != null)
          return ThisMaster.ThisPage;
        else {
          if (_thispage == null)
            _thispage = Cms.GetPageById(PageId).DisplayPage;
          if ((_thispage.Status == "inactive" || !_thispage.InTimeSpan) && !Cms.User.IsAdmin)
            _thispage = Cms.GetPageById(DefaultPageId).DisplayPage;
          return _thispage;
        }
      }
      set {
        if (ThisMaster != null)
          ThisMaster.ThisPage = value;
        else 
          _thispage = value;
      }
    }


    /// <summary>Swedish DateTime format</summary>
    public System.Globalization.DateTimeFormatInfo SweDate {
      get { return ThisMaster.SweDate; }
    }


    /// <summary>Method to use for validating e-mail addresses</summary>
    public virtual void ValidateEmail(object source, ServerValidateEventArgs args) {
      args.IsValid = Util.IsValidEmail(args.Value, false);
    }


    /// <summary>True if the user has permission to view this page</summary>
    public bool ViewPermission {
      get { return ThisPage.ViewPermission; }
    }


    /// <summary>True if the user has permission to edit this page</summary>
    public bool EditPermission {
      get { return ThisPage.EditPermission; }
    }


    /// <summary>Id used by Google Analytics </summary>
    public String UrchinId {
      get { return ThisMaster.UrchinId; }
      set { ThisMaster.UrchinId = value; }
    }

    protected override void OnLoadComplete(EventArgs e) {
      base.OnLoadComplete(e);
      String url = Request.RawUrl;
      if (url.Contains("?")) url += "&";
      else url += "?";
      Context.RewritePath(url);
    }

    public HtmlGenericControl BodyTag {
      get {
        Control bodyTag = Master.FindControl("bodyTag");
        MasterPage amaster = Master.Master;
        while (bodyTag == null && amaster != null) {
          bodyTag = amaster.FindControl("bodyTag");
          amaster = amaster.Master;
        }
        if (bodyTag == null) return null;
        else return (HtmlGenericControl)bodyTag;
      }
    }
    
    public virtual bool UseAjax {
      get { return true; }
    }

    /// <summary>Overrides the inherited <see cref="Page"/> OnLoad method</summary>
    /// <param name="e"><see cref="EventArgs"/></param>
    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);
      if (UseAjax) AjaxPro.Utility.RegisterTypeForAjax(typeof(BasePage), this);

      if (Cms.User.IsAdmin) CheckPrelimTemplate();

      if ((!ViewPermission || !ThisPage.Languages.Contains(Cms.Language)) && PageId != DefaultPageId)
        Response.Redirect("~/" + DefaultPageId + ".aspx");
        
      if (Request["lang"] != null && Request["lang"].Length > 0)
        Cms.Language = Request["lang"];

      if (Cms.User.IsAdmin)
        Session["CurrentPage"] = ThisPage.Id;
    }
    
    private void CheckPrelimTemplate() {
      if (ThisPage.PrelimFilename.Length > 0 && ThisPage.PrelimFilename != ThisPage.PublishedFilename) {
        String currFilename = Request.CurrentExecutionFilePath;
        if (currFilename.StartsWith("/")) currFilename = currFilename.Substring(1);
        if (currFilename != ThisPage.PrelimFilename) {
          String newurl = "/" + ThisPage.PrelimFilename + "?" + Request.QueryString.ToString();;
          Server.Transfer(newurl);
        }
      }
    
    }
    
    protected override void CreateChildControls() {
      base.CreateChildControls();

      String aTitle = Translate(ThisPage.Title);
      if (aTitle.Length == 0) {
        WebPage aPage = ThisPage;
        aTitle = Translate(aPage.Name);
        while (aPage.Parent != null) {
          aPage = aPage.Parent;
          aTitle = Translate(aPage.Name) + " - " + aTitle;
        }
      }
      Header.Title = aTitle;

      String html = "";
      if (ThisPage.KeyWords.Length > 0) 
        html += "<meta content='" + Util.ReplaceHtmlChars(ThisPage.KeyWords) + "' name='keywords'>";

      if (ThisPage.Description.Length > 0) 
        html += "<meta content='" + Util.ReplaceHtmlChars(ThisPage.Description) + "' name='description'>";

      html += "<meta http-equiv='content-type' content='text/html; charset=utf-8;' />";
      html += "<script type='text/javascript'>var _pageId='" + PageId + "';var _url='" + Util.GetBaseUrl(Request) + "';var _loggedin=" + Cms.User.LoggedIn.ToString().ToLower() + ";</script>";

      if (ConfigurationManager.AppSettings["CombineResources"] != null && ConfigurationManager.AppSettings["CombineResources"].ToLower().StartsWith("y")) {
        html += "<link rel='stylesheet' media='screen,print' type='text/css' href='resources.aspx?type=css' />" +
          "<script type='text/javascript' src='resources.aspx?type=js'></script>";
      }
      else {
        String[] cssfiles = Util.GetCSSFiles();
        for (int i=0; i < cssfiles.Length; i++)
          html += "<link rel='stylesheet' media='screen,print' type='text/css' href='" + cssfiles[i].Replace("{language}", Cms.Language) + "' />";

        String[] jsfiles = Util.GetJSFiles();
        for (int i=0; i < jsfiles.Length; i++)
          html += "<script type='text/javascript' src='" + jsfiles[i].Replace("{language}", Cms.Language) + "'></script>";
      }
      if (Cms.User.IsAdmin) 
        html += "<script type='text/javascript' src='admin/tinymce/tiny_mce.js'></script><script type='text/javascript' src='admin/tinymce/jquery.tinymce.js'></script>";
      LiteralControl scripts = new LiteralControl(html);
      Header.Controls.AddAt(0, scripts);

      if (ShowRequestInfo) AppendRequestInfo();
    }
    
    
    private bool ShowRequestInfo {
      get {
        if (Request["requestinfo"] != null)
          Session["requestinfo"] = Request["requestinfo"];
        return (Session["requestinfo"] == null ? false : Session["requestinfo"].ToString() == "y");
      }
    }

    private void AppendRequestInfo() {
      String html = @"
        <div id='_requestinfo' style='padding:20px;margin-top:20px;border-top:solid 2px #000;'>
          <h2>Request information</h2>
          <table border=1>
            <tr><td valign='top'>IP-address</td><td valign='top'>{0}</td></tr>
            <tr><td valign='top'>DNS</td><td valign='top'>{1}</td></tr>
            <tr><td valign='top'>OS</td><td valign='top'>{2}</td></tr>
            <tr><td valign='top'>Browser</td><td valign='top'>{3}</td></tr>
            <tr><td valign='top'>Javascript</td><td valign='top'>{4}</td></tr>
            <tr><td valign='top'>Flash</td><td valign='top'><div id='rinfo_flash'></div></td></tr>
            <tr><td valign='top'>Languages</td><td valign='top'>{5}</td></tr>
            <tr><td valign='top'>Encoding</td><td valign='top'>{6}</td></tr>
            <tr><td valign='top'>Referrer</td><td valign='top'>{7}</td></tr>
            <tr><td valign='top'>Cookies</td><td valign='top'>{8}</td></tr>
          </table>
        </div>
        <script type='text/javascript'>
          if (typeof(swfobject) != 'undefined') [
            var ver = swfobject.getFlashPlayerVersion();
            var txt = 'Major version: ' + ver.major + '<br />';
            txt += 'Minor version: ' + ver.minor + '<br />';
            txt += 'Release: ' + ver.release + '<br />';
            N$('rinfo_flash').innerHTML = txt;
          ]
          else
            N$('rinfo_flash').innerHTML = 'No Flash on page';
        </script>";

      HttpBrowserCapabilities bc = Request.Browser;
      String ip = Request.UserHostAddress;
      String dns = Request.UserHostName;
      String os = bc.Platform;
      String browser = bc.Type + " - " + bc.Browser + " - " + bc.Version;
      String javascript = (bc.JavaScript ? "Supported" : "Not supported");
      String lang = Request.UserLanguages[0];
      String encoding = Request.ContentEncoding.EncodingName;
      String referrer = (Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "&nbsp;");

      String cookies = "";
      HttpCookieCollection cc = Request.Cookies;
      String[] carr = cc.AllKeys;
      for (int i=0; i < carr.Length; i++) {
        if (i > 0) cookies += "<br />";
        HttpCookie c = cc[carr[i]];
        cookies  += "Cookie: " + c.Name + "<br />" + "Expires: " + c.Expires + "<br />" + "Secure:" + c.Secure + "<br />";
        String[] carr2 = c.Values.AllKeys;
        for (int j=0; j < carr2.Length; j++) 
          cookies += "Value" + (j+1).ToString() + ": " + Server.HtmlEncode(carr2[j]) + "<br />";
      }

      html = String.Format(html, ip, dns, os, browser, javascript, lang, encoding, referrer, cookies).Replace("[","{").Replace("]","}");
      BodyTag.Controls.Add(new LiteralControl(html));
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void SetLanguage(String lang) {
      Cms.Language = lang;
    }


    /// <summary>Returns a <see cref="WebPage"/> that is the root page for the current page in the page hierarchy</summary>
    protected WebPage MainPage {
      get {
        WebPage aPage = ThisPage;
        while (aPage.Parent != null)
          aPage = aPage.Parent;
        return aPage;
      }
    }
    

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void SaveControlProp(int propId, String propName, String propVal) {
      PageProperty aProp = Cms.GetPropertyById(propId);
      aProp.GetProperty(propName).PrelimValue = propVal;
      aProp.GetProperty(propName).Publish(false);
      aProp.WriteToDB();
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void SaveAjaxControl(int mainPropId, String propVal, bool publish) {
      int idx = propVal.IndexOf((char)65279);
      while (idx >= 0) {
        propVal = propVal.Remove(idx, 1);
        idx = propVal.IndexOf((char)65279);
      }
      PageProperty aProp = Cms.GetPropertyById(mainPropId);
      aProp.Refresh();
      aProp.SetControlPropertyValue(propVal, false);
      Cms.LogEvent("saveprop", mainPropId.ToString());
      if (publish && Cms.GetPermission(aProp.Id.ToString(), "PageProperty", "Publish", "")) {
        aProp.ControlTypeProperty.Publish(false);
        Cms.LogEvent("publish", mainPropId.ToString());
      }
      aProp.WriteToDB();
    }
    

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void SaveControlAttributes(String pageId, int propId, String controltype, String langdep, String show, String share, String common, String start, String end) {
      PageProperty aProp = Cms.GetPropertyById(propId);
      if (controltype.Length > 0) aProp.ControlType = controltype;
      if (langdep.Length > 0 ) aProp.LanguageDependent = (langdep == "1");
      if (show.Length > 0 ) aProp.ControlTypeProperty.IsVisible = (show == "1");
      if (share.Length > 0) SetSharing(pageId, propId, share == "1");
      if (common.Length > 0) SetCommon(pageId, propId, common == "1");
      try {
        if (start.Length > 0) aProp.ControlTypeProperty.StartDate = DateTime.Parse(start);
        if (end.Length > 0) aProp.ControlTypeProperty.EndDate = DateTime.Parse(end);
      }
      catch {}
      aProp.WriteToDB();
    }
    
    private void SetSharing(String pageId, int propId, bool share) {
      WebPage aPage = Cms.GetPageById(pageId);
      PageProperty aProp = Cms.GetPropertyById(propId);
      PageProperty newProp = null;
      String newhtml = "$nochange$";
      bool isShared = aPage.SharedProp.Id == aProp.Parent.Id;
      if (aProp.Parent != null && (share != (isShared))) {
        if (share) {
          String aPropName = aProp.PropertyName;
          if (aPage.SharedProp.HasProperty(aPropName)) {
            aProp.Delete();
            newProp = aPage.SharedProp.GetProperty(aPropName);
            newhtml = newProp.ControlTypeProperty.PrelimValue;
          }
          else {
            aProp.Parent = aPage.SharedProp;
            newProp = aProp;
          }
        }
        else {
          DataSet ds = DB.GetDS("select count(*) as nof from webpage where deleted=0 and sharedprop=" + aPage.SharedProp.Id);
          if (DB.GetInt(ds, 0, "nof") > 1) {
            newProp = aProp.Clone(aPage.MainProp);
          }
          else {
            aProp.Parent = aPage.MainProp;
            newProp = aProp;
          }
        }
      }
    }

    private void SetCommon(String pageId, int propId, bool share) {
      PageProperty aProp = Cms.GetPropertyById(propId);

      if (aProp.Parent != null && (share != aProp.IsCommon)) {
        WebPage aPage = Cms.GetPageById(pageId);
        if (share) {
          String aName = aProp.PropertyName;
          if (Cms.CommonProperty.HasProperty(aName)) {
            aProp.Delete();
            aProp = Cms.CommonProperty.GetProperty(aName);
          }
          else {
            aProp.Parent = Cms.CommonProperty;
          }
        }
        else {
          if (aPage.MainProp.HasProperty(aProp.PropertyName)) {
            PageProperty newProp = aPage.MainProp.GetProperty(aProp.PropertyName);
            aProp.Delete();
            aProp = newProp;
          }
          else
            aProp.Parent = aPage.MainProp;
        }
        Cms.CommonProperty.Refresh();

      }
    }


    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String AddTiny(String pageId, String listName) {
      WebPage aPage = Cms.GetPageById(pageId);

      PageProperty mainprop = aPage.MainProp.GetProperty(listName);
      int no = 1;
      while (mainprop.HasProperty(listName + "_" + no)) no++;
      String aPropName = listName + "_" + no;
      PageProperty aProp = mainprop.GetProperty(aPropName);
      PageProperty ajaxprop = aPage.MainProp.GetProperty(aPropName);
      mainprop.Value = mainprop.ChildProperties.Count.ToString();
      return aPropName;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void DeleteTiny(String pageId, String listName, String tinyName) {
      WebPage aPage = Cms.GetPageById(pageId);
      PageProperty listProp = aPage.MainProp.GetProperty(listName);
      listProp.GetProperty(tinyName).Delete();
      aPage.MainProp.GetProperty(tinyName).Delete();
      listProp.Value = listProp.ChildProperties.Count.ToString();
    }


    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void UpdatePropertyCol(String propId, String col, String val) {
      try {
        String sql = "update pageproperty set " + col + "=" + val + " where id=" + propId;
        DB.ExecSql(sql);
      }
      catch { }
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String[] GetHtmlTemplates() {
      try {
        DirectoryInfo di = new DirectoryInfo(Server.MapPath("/templates"));
        FileInfo[] fi = di.GetFiles();
        String[] res = new String[fi.Length];
        int i = 0;
        foreach (FileInfo fiTemp in fi) {
          String desc = fiTemp.Name;
          using (StreamReader sr = new StreamReader(Server.MapPath("/templates/" + fiTemp.Name))) {
            String line = sr.ReadLine();
            if (line != null && line.StartsWith("<!--")) {
              line = line.Substring(4);
              line = line.Substring(0, line.Length -3);
              desc = line.Trim();
            }
          }
          res[i] = desc + "|" + "templates/" + fiTemp.Name + "|" + desc;
          i++;
        }
        return res;
      }
      catch {
        return new String[0];
      }
    }


    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String[] GetListControlInfo(String pageId, String propName) {
      WebPage aPage = Cms.GetPageById(pageId);

      PageProperty mainprop = aPage.MainProp.GetProperty(propName);
      mainprop.Refresh();
      
      String propId = mainprop.Id.ToString();
      bool addPerm = (Cms.GetPermission(pageId, "WebPage", "Edit") && Cms.GetPermission(propId, "PageProperty", "Add", "General"));
      bool delPerm = (Cms.GetPermission(pageId, "WebPage", "Edit") && Cms.GetPermission(propId, "PageProperty", "Delete", "General"));
      
      String[] res = new String[8];
      res[0] = mainprop.Id.ToString();
      res[1] = mainprop.PropertyName;
      res[2] = mainprop.ModDate.ToString("yyyy-MM-dd");
      res[3] = DB.GetString("select username from users where deleted=0 and id=" + mainprop.ModBy, "username");
      res[4] = Cms.User.IsSysAdmin ? "y" : "n";
      res[5] = addPerm ? "y" : "n";
      res[6] = delPerm ? "y" : "n";
      res[7] = Cms.Language;

      return res;
    }
    

    private PageProperty GetMainProp(String pageId, int propId, String propName) {
      WebPage aPage = Cms.GetPageById(pageId);

      PageProperty mainprop = null;
      if (propId > 0) 
        mainprop = Cms.GetPropertyById(propId);
      else 
        return aPage.GetControlProperty(propName);
      if (mainprop == null)
        mainprop = aPage.MainProp.GetProperty("empty");
        
      return mainprop;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public bool HasEditPermission(String pageId, String propName, String propId) {
      if (propId == null || propId.Length == 0) {
        PageProperty mainprop = GetMainProp(pageId, 0, propName);
        mainprop.Refresh();
        propId = mainprop.Id.ToString();
      }
      bool perm = (Cms.GetPermission(pageId, "WebPage", "Edit") && Cms.GetPermission(propId, "PageProperty", "Edit", "General"));
      return perm;
    }


    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String[][] GetControlInfo(String pageId, int ipropId, String propName, bool returnPropVal) {
      WebPage aPage = Cms.GetPageById(pageId);
      PageProperty mainprop = GetMainProp(pageId, ipropId, propName);
      mainprop.Refresh();
      
      String propId = mainprop.Id.ToString();
      bool editPerm = (Cms.GetPermission(pageId, "WebPage", "Edit") || Cms.GetPermission(propId, "PageProperty", "Edit", "General"));
      
      String[][] res = new String[3][];
      res[0] = new String[17];
      
      res[0][0] = mainprop.Id.ToString();
      res[0][1] = mainprop.PropertyName;
      res[0][2] = mainprop.ControlTypeProperty.ModDate.ToString("yyyy-MM-dd");
      res[0][3] = DB.GetString("select username from users where deleted=0 and id=" + mainprop.ControlTypeProperty.ModBy, "username");
      res[0][4] = mainprop.ControlTypeProperty.PublishedDate.ToString("yyyy-MM-dd");
      res[0][5] = DB.GetString("select username from users where deleted=0 and id=" + mainprop.ControlTypeProperty.PublishedBy, "username");
      res[0][6] = (mainprop.ControlTypeProperty.IsPublished ? "y" : "n");
      res[0][7] = (mainprop.LanguageDependent ? "y" : "n");
      res[0][8] = (mainprop.ControlTypeProperty.IsVisible ? "y" : "n");
      res[0][9] = (mainprop.Parent != null && aPage.SharedProp.Id == mainprop.Parent.Id ? "y" : "n");
      res[0][10] = (!mainprop.IsCommon ? "y" : "n");
      res[0][11] = (mainprop.IsCommon ? "y" : "n");
      res[0][12] = (aPage.SharedProp == null || !aPage.SharedProp.HasProperty(propName) ? "y" : "n");
      res[0][13] = (mainprop.ControlTypeProperty.StartDate == DateTime.MinValue ? DateTime.Now.ToString("yyyy-MM-dd") : mainprop.ControlTypeProperty.StartDate.ToString("yyyy-MM-dd"));
      res[0][14] = (mainprop.ControlTypeProperty.EndDate == DateTime.MinValue ? DateTime.Now.AddYears(10).ToString("yyyy-MM-dd") : mainprop.ControlTypeProperty.EndDate.ToString("yyyy-MM-dd"));
      res[0][15] = mainprop.ControlType;

      if (returnPropVal) 
        res[0][16] = (Cms.User.IsAdmin && editPerm ? mainprop.ControlTypeProperty.PrelimValue : mainprop.ControlTypeProperty.Value);

      res[1] = new String[35];
      res[1][0] = Cms.User.IsSysAdmin ? "y" : "n";
      res[1][1] = editPerm ? "y" : "n";
      res[1][2] = Cms.GetPermission(propId, "PageProperty", "Publish", "General") ? "y" : "n";
      res[1][3] = Cms.GetPermission(propId, "PageProperty", "Show", "General") ? "y" : "n";
      res[1][4] = Cms.GetPermission(propId, "PageProperty", "Share", "General") ? "y" : "n";
      res[1][5] = Cms.GetPermission(propId, "PageProperty", "Restore", "General") ? "y" : "n";
      res[1][6] = Cms.GetPermission(propId, "PageProperty", "DateControl", "General") ? "y" : "n";
      res[1][7] = Cms.GetPermission(propId, "PageProperty", "ControlType", "General") ? "y" : "n";
      res[1][8] = Cms.GetPermission(propId, "PageProperty", "LanguageDependent", "General") ? "y" : "n";

      res[1][9] = Cms.GetPermission(propId, "PageProperty", "Paragraph", "AjaxRichText") ? "y" : "n";
      res[1][10] = Cms.GetPermission(propId, "PageProperty", "Font", "AjaxRichText") ? "y" : "n";
      res[1][11] = Cms.GetPermission(propId, "PageProperty", "Css", "AjaxRichText") ? "y" : "n";
      res[1][12] = Cms.GetPermission(propId, "PageProperty", "Character", "AjaxRichText") ? "y" : "n";
      res[1][13] = Cms.GetPermission(propId, "PageProperty", "CharacterExtended", "AjaxRichText") ? "y" : "n";
      res[1][14] = Cms.GetPermission(propId, "PageProperty", "Color", "AjaxRichText") ? "y" : "n";
      res[1][15] = Cms.GetPermission(propId, "PageProperty", "Justify", "AjaxRichText") ? "y" : "n";
      res[1][16] = Cms.GetPermission(propId, "PageProperty", "Paste", "AjaxRichText") ? "y" : "n";
      res[1][17] = Cms.GetPermission(propId, "PageProperty", "PasteExtended", "AjaxRichText") ? "y" : "n";
      res[1][18] = Cms.GetPermission(propId, "PageProperty", "Undo", "AjaxRichText") ? "y" : "n";
      res[1][19] = Cms.GetPermission(propId, "PageProperty", "Search", "AjaxRichText") ? "y" : "n";
      res[1][20] = Cms.GetPermission(propId, "PageProperty", "Link", "AjaxRichText") ? "y" : "n";
      res[1][21] = Cms.GetPermission(propId, "PageProperty", "DocumentBank", "AjaxRichText") ? "y" : "n";
      res[1][22] = Cms.GetPermission(propId, "PageProperty", "Media", "AjaxRichText") ? "y" : "n";
      res[1][23] = Cms.GetPermission(propId, "PageProperty", "HtmlTemplates", "AjaxRichText") ? "y" : "n";
      res[1][24] = Cms.GetPermission(propId, "PageProperty", "List", "AjaxRichText") ? "y" : "n";
      res[1][25] = Cms.GetPermission(propId, "PageProperty", "Indent", "AjaxRichText") ? "y" : "n";
      res[1][26] = Cms.GetPermission(propId, "PageProperty", "Table", "AjaxRichText") ? "y" : "n";
      res[1][27] = Cms.GetPermission(propId, "PageProperty", "TableExtended", "AjaxRichText") ? "y" : "n";
      res[1][28] = Cms.GetPermission(propId, "PageProperty", "Style", "AjaxRichText") ? "y" : "n";
      res[1][29] = Cms.GetPermission(propId, "PageProperty", "Layer", "AjaxRichText") ? "y" : "n";
      res[1][30] = Cms.GetPermission(propId, "PageProperty", "Zoom", "AjaxRichText") ? "y" : "n";
      res[1][31] = Cms.GetPermission(propId, "PageProperty", "Special", "AjaxRichText") ? "y" : "n";
      res[1][32] = Cms.GetPermission(propId, "PageProperty", "SpecialExtended", "AjaxRichText") ? "y" : "n";
      res[1][33] = Cms.GetPermission(propId, "PageProperty", "Advanced", "AjaxRichText") ? "y" : "n";
      res[1][34] = Cms.GetPermission(propId, "PageProperty", "InsertHtml", "AjaxRichText") ? "y" : "n";

      res[2] = new String[1];
      DataSet ds = DB.GetDS("select name, description from extra where extratype='Css' order by orderno");
      String styles = "";
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        if (i > 0) styles += ";";
        styles += DB.GetString(ds, i, "description") + "=" + DB.GetString(ds, i, "name");
      }
      res[2][0] = styles;
/*      res[2][1] = "n";
      try {
        DirectoryInfo di = new DirectoryInfo(Server.MapPath("/templates"));
        FileInfo[] fi = di.GetFiles();
        res[2][1] = (fi.Length > 0 ? "y" : "n");
      }
      catch {};
      res[2][2] = Cms.Language;*/

      return res;
    }


    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String GetPropVal(int propId, String propName) {
      PageProperty aProp = Cms.GetPropertyById(propId);
      return aProp.GetProperty(propName).Value;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String GetPropValue(String mainProp, String subProp) {
      PageProperty aProp = this.GetProperty(mainProp);
      return aProp.GetProperty(subProp).Value;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public bool ShareWillLooseData(String pageId, String propName) {
      WebPage aPage = Cms.GetPageById(pageId);
      return aPage.SharedProp.HasProperty(propName);
    }


    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public int PublishProperty(int propId) {
      PageProperty aProp = Cms.GetPropertyById(propId);
      aProp.Refresh();
      aProp.Publish(true);
      Cms.LogEvent("publish", propId.ToString());
      return propId;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void PublishControlValue(int propId) {
      PageProperty aProp = Cms.GetPropertyById(propId);
      aProp.ControlTypeProperty.Refresh();
      aProp.ControlTypeProperty.Publish(true);
      Cms.LogEvent("publish", propId.ToString());
    }


    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public int UnpublishProperty(int propId) {
      PageProperty aProp = Cms.GetPropertyById(propId);
      aProp.RevertToPublished();
      Cms.LogEvent("unpublish", propId.ToString());
      return propId;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String UnpublishControlValue(int propId) {
      PageProperty aProp = Cms.GetPropertyById(propId);
      aProp.ControlTypeProperty.RevertToPublished();
      Cms.LogEvent("unpublish", propId.ToString());
      return aProp.ControlTypeProperty.Value;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void LockProperty(int propId) {
      PageProperty aProp = Cms.GetPropertyById(propId);
      aProp.Lock(this.Session.SessionID);
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void UnlockProperty(int propId) {
      PageProperty aProp = Cms.GetPropertyById(propId);
      aProp.Unlock(this.Session.SessionID);
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public bool IsPagePropPublished(String pageId, String propName) {
      WebPage aPage = Cms.GetPageById(pageId);
      return aPage.MainProp.GetProperty(propName).IsPublished;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public bool IsLockedProperty(int propId) {
      PageProperty aProp = Cms.GetPropertyById(propId);
      if (aProp.IsLocked()) {
        int min = DateTime.Now.Subtract(aProp.LockTime()).Minutes;
        int timeout = Convert.ToInt32(CMS.SiteSetting("lockTimeout"));
        if (min >= timeout) {
          aProp.Unlock();
          return false;
        }
        else
          return true;
      }
      else
        return false;
    }

    

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String[] GetRestoreTimes(int mainPropId, int idx) {
      if (idx < 0) idx = 0;
      int nofrows = 10;
      PageProperty aProp = Cms.GetPropertyById(mainPropId);
      int propId = DB.GetInt("select id from pageproperty where parentproperty=" + mainPropId + " and propertyname='" + aProp.ControlTypePropertyName + "' and deleted=0", "id");
      DataSet ds = DB.GetDS("select id, changedate from a_pageproperty where propid=" + propId + " order by changedate desc");
      bool up = idx > 0;
      bool down = DB.GetRowCount(ds) > idx + nofrows;
      int count = DB.GetRowCount(ds) - idx;
      if (count > nofrows) count = nofrows;
      String[] res = new String[count + 1];
      res[0] = (up ? "1" : "0") + "|" + (down ? "1" : "0");
      for (int i=0; i < count; i++)
        res[i+1] = DB.GetString(ds, i, "id") + "|" + DB.GetDate(ds, i, "changedate").ToString("yyyy-MM-dd HH:mm:ss");
      return res;
    }

    private void RestoreOneProp(int propId, String adate) {
      DataSet ds = DB.GetDS("select top 1 * from a_pageproperty where propid=" + propId + " and changedate <= '" + adate + "' order by changedate desc");
      if (DB.GetRowCount(ds) == 0)
        ds = DB.GetDS("select top 1 * from a_pageproperty where propid=" + propId + " and changedate >= '" + adate + "' order by changedate asc");
      if (DB.GetRowCount(ds) > 0)
        Cms.RestoreProp(ds, 0);
    }
     
    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String RestoreProperty(int propId, int restoreId) {
      DataSet ds = DB.GetDS("select top 1 * from a_pageproperty where id=" + restoreId);
      Cms.RestoreProp(ds, 0);
      return Cms.GetPropertyById(propId).ControlTypeProperty.Value;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String[] GetLanguages() {
      String langs = "";
      for (int i=0; i < Cms.Languages.Length; i++) {
        if (i > 0) langs += ",";
        langs += Cms.Languages[i];
      }
      String[] res = new String[2];
      res[0] = Cms.Language;
      res[1] = langs;
      return res;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String Translate(String txt) {
      return Cms.Translate(txt);
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String[] TranslateArr(String[] txt) {
      String[] res = new String[txt.Length];
      for (int i = 0; i < res.Length; i++)
        res[i] = Cms.Translate(txt[i]);
      return res;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public bool CheckLoggedIn() {
      return Cms.User.IsAdmin;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String GetRoles(String propname) {
      String roles = "";
      for (int i = 0; i < Cms.Roles.Count; i++) {
        if (i > 0) roles += "|";
        roles += Cms.Roles[i];
      }
      return propname + ";" + roles;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public bool GetPermission(String id, String itemtype, String actiontype) {
      return Cms.GetPermission(id, itemtype, actiontype);
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String GetPermissionHtml(String aRole, String propName, String propId) {
      StringBuilder html = new StringBuilder();
      html.Append("<p class='ajaxHeadline'>" + Cms.Translate("Behrigheter") + "</p><table cellspacing=0 cellpadding=0>");

      DataSet ds = DB.GetDS("select * from permissiontypes where controltype='General' order by orderno");
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        html.Append("<tr><td>" + Cms.Translate(DB.GetString(ds, i, "description")) + "</td>");
        bool perm = Cms.GetRolePermission(propId, aRole, "pageproperty", DB.GetString(ds, i, "actiontype"), "General");
        html.Append("<td><input id='" + propId + "_" + aRole + "_" + DB.GetString(ds, i, "actiontype") + "' type='checkbox' " + (perm ? "checked" : "") + " onclick='bwPermClicked(this)' /></td></tr>");
      }

      ds = DB.GetDS("select * from permissiontypes where controltype='AjaxRichText' order by orderno");
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        html.Append("<tr><td>" + Cms.Translate(DB.GetString(ds, i, "description")) + "</td>");
        bool perm = Cms.GetRolePermission(propId, aRole, "PageProperty", DB.GetString(ds, i, "actiontype"), "AjaxRichText");
        html.Append("<td><input id='" + propId + "_" + aRole + "_" + DB.GetString(ds, i, "actiontype") + "' type='checkbox' " + (perm ? "checked" : "") + " onclick='bwPermClicked(this)' /></td></tr>");
      }
      html.Append("</table>");
      return propName + "|" + html.ToString();
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String[] GetPermissionInfo(String aRole, int propId) {
      DataSet ds = DB.GetDS("select * from permissiontypes where controltype='General' or controltype='AjaxRichText' order by controltype desc, orderno");
      String[] res = new String[DB.GetRowCount(ds)];
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        res[i] = Cms.Translate(DB.GetString(ds, i, "description")) + "|" + Cms.Translate(DB.GetString(ds, i, "actiontype"));
        bool perm = Cms.GetRolePermission(propId.ToString(), aRole, "pageproperty", DB.GetString(ds, i, "actiontype"), DB.GetString(ds, i, "controltype"));
        res[i] += "|" + (perm ? "y" : "n");
      }
      return res;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void SetControlPermission(int id, String role, String actiontype, bool permission) {
      Cms.SetRolePermission(id.ToString(), "PageProperty", role, actiontype, "AjaxRichText", permission);
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void SetDefaultPermissions(int propId, String role) {
      DB.ExecSql("delete from permissions where id='" + propId + "' and role='" + role + "'");
    }



    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String ReplaceBR(String value) {
      return value.Replace("<br>", "\r\n").Replace("<BR>", "\r\n");
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String GetThumbnail(String url, int width, int height) {
      return Util.GetThumbnail(url, width, height, true, null);
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String GetThumbnail(String url, int width, int height, bool keepaspect, String errthumb) {
      return Util.GetThumbnail(url, width, height, keepaspect, errthumb);
    }

    public PageProperty GetProperty(String propName) {
      return (propName.Length > 0 ? ThisPage.GetControlProperty(propName) : null);
    }

    public PageProperty GetProperty(int propId) {
      PageProperty res = ThisPage.MainProp.GetPropertyById(propId.ToString());
      if (res == null) res = ThisPage.MainProp.GetProperty("empty");
      return res;
    }
    
    private bool IsIdUnique(String id) {
      return !DB.RowExists("select id from webpage where id='" + id + "'");
    }
    
    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String QuickNewPage(String currPageId, String newid, String title, String name, bool active, bool istimecontrolled, String startdate, String enddate, String filename, int place, String attribs) {
      if (DB.RowExists("select id from webpage where id='" + newid + "'"))
        throw new Exception(Translate("En sida med identifieraren") + " " + newid + " " + Translate("existerar redan. V‰lj annan identifierare fˆr sidan"));
      if (newid.ToLower().IndexOfAny(new char[]{' ','Â','‰','ˆ','/','&','%','?','#','\\',';'}) != -1 )
        throw new Exception(Translate("Fel: Sidans identifierare kan inte innehÂlla tecken sÂsom mellanslag eller Â,‰,ˆ,/,&,%,?,#."));
    
      WebPage currPage = Cms.GetPageById(currPageId);
      WebPage parent = currPage.Parent;

      parent.UpdateChildPages();
      int order = 0;
      if (parent.ChildPages.Count > place)
        order = parent.GetChildPage(place).OrderNo;
      else
        order = parent.GetChildPage(parent.ChildPages.Count).OrderNo + 1;
      for (int i=place-1; i < parent.ChildPages.Count; i++) {
        WebPage apage = (WebPage)parent.ChildPages.GetByIndex(i);
        apage.OrderNo = apage.OrderNo + 1;
        apage.WriteToDB();
      }
      Cms.CreatePage(newid, parent.Id, name, title, filename, (active ? "active" : "hidden"), Cms.Language, false, "", "", order.ToString(), "N", false, "", "");
      
      WebPage aPage = Cms.GetPageById(newid);
      if (istimecontrolled) {
        aPage.TimeControlled = true;
        aPage.StartTime = DateTime.Parse(startdate);  
        aPage.EndTime = DateTime.Parse(enddate);  
        aPage.WriteToDB();
      }
      
      if (attribs.Length > 0) {
        String[] attrarr = attribs.Split(';');
        for (int i=0; i < attrarr.Length; i++) {
          String[] vals = attrarr[i].Split('|');
          aPage.SavePageAttributeValue(vals[0], vals[1]);
        }
      }
      
      return newid;
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String DeletePage(String pageId) {
      WebPage aPage = Cms.GetPageById(pageId);
      String nextPage = (aPage.Parent != null ? aPage.Parent.Id + ".aspx" : "/");
      aPage.Delete();
      return nextPage;
    }


    private void UpdateTranslation(String sv, String trans) {
      String sql = "insert into translation (sv, " + Cms.Language + ") values ('" + sv + "','" + trans + "')";
      if (DB.RowExists("select * from translation where sv='" + sv + "'"))
        sql = "update translation set " + Cms.Language + "='" + trans + "' where sv='" + sv + "'";
      DB.ExecSql(sql);
      Cms.RefreshTranslations();
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void SavePageAttribs(String pageId, String title, String name, bool isactive, bool istimecontrolled, String startdate, String enddate, String template, bool publish, int place, String attribs, String related) {
      WebPage apage = Cms.GetPageById(pageId);
      if (Cms.Language == "sv") {
        apage.Title = title;
        apage.Name = name;
      }
      else {
        UpdateTranslation(apage.Title, title);
        UpdateTranslation(apage.Name, name);
      }
      apage.Status = (isactive ? "active" : "hidden");
      apage.TimeControlled = istimecontrolled;
      if (istimecontrolled) {
        apage.StartTime = DateTime.Parse(startdate);  
        apage.EndTime = DateTime.Parse(enddate);  
      }
      if (template != null && template.Length > 0)
        apage.PrelimFilename = template;
      if (publish)
        apage.Filename = apage.PrelimFilename;
        
      if (attribs.Length > 0) {
        String[] attrarr = attribs.Split(';');
        for (int i=0; i < attrarr.Length; i++) {
          String[] vals = attrarr[i].Split('|');
          apage.SavePageAttributeValue(vals[0], vals[1]);
        }
      }
      DB.ExecSql("delete from relatedpages where page1='" + pageId + "' and lang='" + Cms.Language + "'");
      if (related.Length > 0) {
        String[] relarr = related.Split('|');
        for (int i=0; i < relarr.Length; i++)
          DB.ExecSql("insert into relatedpages (page1, page2, lang) values('" + pageId + "', '" + relarr[i] + "', '" + Cms.Language + "')");
      }
      
      if (apage.Parent != null) {
        apage.Parent.UpdateChildPages();
        WebPage switchpage = apage.Parent.GetChildPage(place);
        int switchorder = switchpage.OrderNo;
        switchpage.OrderNo = apage.OrderNo;
        apage.OrderNo = switchorder;
        switchpage.WriteToDB();
      }
      
      apage.WriteToDB();
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void SaveUserInfo(String username, String oldpass, String newpass, String email, String attribs) {
      NFNUser user = new NFNUser(username);
      if (newpass.Length > 0) {
        if (oldpass != user.Password)
          throw new Exception(Translate("Nuvarande lˆsenord ‰r ogiltigt"));
        user.Password = newpass;
      }

      user.Email = email;
      if (attribs.Length > 0) {
        String[] arr = attribs.Split(';');
        for (int i=0; i < arr.Length; i++) {
          String[] vals = arr[i].Split('|');
          user.SetAttrib(vals[0], vals[1]);
        }
      }
      user.WriteToDB();
    }

  }
}


