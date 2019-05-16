/* $Date: 2010-09-25 22:29:44 +0200 (lÃ¶, 25 sep 2010) $    $Revision: 6989 $ */
using System;
using System.Web;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Collections;
using System.Collections.Generic;

namespace NFN {

  public class CMS  {

    private NFNUser user;
    private int mainCount;
    private ArrayList roles;
    private HttpApplicationState application;
    private PageProperty commonProperty = null;
    private String language = null;
    private String[] languages = null;
    private SortedDictionary<string, string> translation = null;


    public CMS(HttpApplicationState app) {
      application = app;

      roles = new ArrayList();
      user = new NFNUser();

      Refresh();
    }

    public NFNUser User {
      get { return user; }
    }

    public ArrayList Roles {
      get { return roles; }
    }

    public String Language {
      get {
        if (language == null) language = Languages[0];
        return language;
      }
      set { 
        if (value != language) {
          language = value; 
          translation = null;
        }
      }
    }

    public String[] Languages {
      get {
        if (languages == null) {
          String lang = SiteSetting("Languages");
          if (lang.Length == 0) lang = "sv";
          languages = lang.Split(',');
        }
        return languages;
      }
    }

    private String LangSuffix {
      get {
        if (Language != "sv") return "_" + Language;
        else return "";
      }
    }
    
    public String TinyPropertyName {
      get { return "text" + LangSuffix; }
    }

    public String TextPropertyName {
      get { return "stext" + LangSuffix; }
    }

    public String ImagePropertyName {
      get { return "image" + LangSuffix; }
    }

    public String ImagesPropertyName {
      get { return "images" + LangSuffix; }
    }

    public String FlashPropertyName {
      get { return "flash" + LangSuffix; }
    }

    public String MoviePropertyName {
      get { return "movie" + LangSuffix; }
    }

    public String Translate(Object txt, String currLang) {
      String res = txt.ToString();
      if (currLang != "sv" && currLang != Language) {
        try {
          res = DB.GetString("select " + currLang + " from translation where sv='" + res + "'", currLang);
          if (res.Length == 0) res = txt.ToString();
        }
        catch { res = txt.ToString(); }
      }
      return res;
    }

    public String Translate(Object txt) {
      if (Language != "sv" && translation == null) {
        DataSet ds = DB.GetDS("select sv, " + language + " from translation");
        translation = new SortedDictionary<string, string>(StringComparer.CurrentCulture);
        for (int i = 0; i < DB.GetRowCount(ds); i++) {
          try { translation.Add(DB.GetString(ds, i, "sv"), DB.GetString(ds, i, language)); }
          catch { }
        }
      }
      String res = txt.ToString();
      if (Language != "sv") {
        try { res = translation[res]; }
        catch { res = txt.ToString(); }
        if (res.Length == 0) res = txt.ToString();
      }

      return res;
    }
    
    public void RefreshTranslations() {
      translation = null;
    }

    public static String SiteSetting(String key) {
      return DB.GetString("select val from extra where extratype='Settings' and name='" + key + "'", "val");
    }

    public PageProperty GetWebPageProperty(String pageId, String propName) {
      int mainpropid = DB.GetInt("select mainprop from webpage where id='" + pageId + "'", "mainprop");
      return this.GetPropertyById(mainpropid).GetProperty("PageProperties").GetProperty(propName);
    }

    public void SetWebPageProperty(String pageId, String propName, String val) {
      PageProperty prop = GetWebPageProperty(pageId, propName);
      prop.SetValuesDirectly(val, val);
      prop.WriteToDB();
    }

    public void Refresh() {
      mainCount = DB.GetInt("select count(*) as nof from webpage where deleted=0 and parentpage is null", "nof");
      RefreshRoles();
      User.Refresh();
    }

    public void RefreshRoles() {
      roles.Clear();
      DataSet ds = DB.GetDS("select * from role where name<>'Admin'");
      for (int i = 0; i < DB.GetRowCount(ds); i++)
        roles.Add(DB.GetString(ds, i, "name"));
    }

    public DataTable PermissionTypes {
      get {
        if (application["PermissionTypes"] == null)
          application["PermissionTypes"] = DB.GetDS("select * from permissiontypes").Tables[0];
        return (DataTable)application["PermissionTypes"];
      }
    }

    public int GetPermissionTypeId(String itemtype, String actiontype, String controltype) {
      String select = "itemtype='" + itemtype + "' and actiontype='" + actiontype + "'";
      if (controltype.Length > 0)
        select += " and controltype in ('" + controltype + "','General')";
      DataRow[] ptids = PermissionTypes.Select(select);
      if (ptids.Length > 0) return Convert.ToInt32(ptids[0]["id"]);
      else return -1;
    }

    public int GetPermissionTypeId(String itemtype, String actiontype) {
      return GetPermissionTypeId(itemtype, actiontype, "");
    }

    public bool GetRolePermission(String id, String role, String itemtype, String actiontype, String controltype) {
      int ptid = GetPermissionTypeId(itemtype, actiontype, controltype);
      if (ptid > 0) {
        String[] roles = role.Split('|');
        String aPerm = "";
        for (int i = 0; i < roles.Length && aPerm != "Y"; i++) {
          String sql = @"
              select permission from permissions 
              where id='{0}' and typeid='{1}' and role='{2}'";
          String thisPerm = DB.GetString(String.Format(sql, id, ptid, roles[i]), "permission");
          if (aPerm.Length == 0 || thisPerm == "Y")
            aPerm = thisPerm;
        }
        if (aPerm.Length == 0) {
          for (int i = 0; i < roles.Length && aPerm != "Y"; i++) {
            String sql = @"
                select permission from permissions 
                where id='DEFAULT' and typeid='{0}' and role='{1}'";
            String thisPerm = DB.GetString(String.Format(sql, ptid, roles[i]), "permission");
            if (aPerm.Length == 0 || thisPerm == "Y")
              aPerm = thisPerm;
          }
        }
        return aPerm == "Y";
      }
      return false;
    }

    /// <summary>Returns true if permission is granted for the logged in user to perform the given action on the given itemtype with the given id</summary>
    /// <param name="id">Id if the item to check</param>
    /// <param name="itemtype">Item type to check</param>
    /// <param name="actiontype">Action for which permission is checked</param>
    /// <param name="controltype">Type of BWControl for which permission is checked</param>
    public bool GetPermission(String id, String itemtype, String actiontype, String controltype) {
      if (User.IsSysAdmin) return true;
      if (User.Roles.Count == 0) return false;
      String roles = "";
      foreach (String aRole in User.Roles) {
        if (roles.Length > 0) roles += "|";
        roles += aRole;
      }
      return GetRolePermission(id, roles, itemtype, actiontype, controltype);
    }


    /// <summary>Returns true if permission is granted for the logged in user to perform the given action on the given itemtype with the given id</summary>
    /// <param name="id">Id if the item to check</param>
    /// <param name="itemtype">Item type to check</param>
    /// <param name="actiontype">Action for which permission is checked</param>
    public bool GetPermission(String id, String itemtype, String actiontype) {
      return GetPermission(id, itemtype, actiontype, "");
    }


    /// <summary>Sets permission for the given role to perform the given action on the given itemtype with given id</summary>
    /// <param name="id">Id of the item to set permission to</param>
    /// <param name="itemtype">Type of item to set permission to</param>
    /// <param name="role">Role to set permission to</param>
    /// <param name="actiontype">Action to ser permission to</param>
    /// <param name="controltype">Type of BWControl to ser permission to</param>
    /// <param name="permission">True if permission is granted</param>
    public void SetRolePermission(String id, String itemtype, String role, String actiontype, String controltype, bool permission) {
      int typeid = GetPermissionTypeId(itemtype, actiontype, controltype);
      if (typeid > 0) {
        if (GetRolePermission("DEFAULT", role, itemtype, actiontype, controltype) == permission)
          DB.ExecSql("delete from permissions where id='" + id + "' and typeid=" + typeid + " and role='" + role + "'");
        else {
          String permChar = (permission ? "Y" : "N");
          try {
            DB.ExecSql("insert into permissions (id, typeid, role, permission) values('" + id + "', " + typeid + ", '" + role + "', '" + permChar + "')");
          }
          catch {
            DB.ExecSql("update permissions set permission='" + permChar + "' where id='" + id + "' and typeid=" + typeid + " and role='" + role + "'");
          }
        }
      }
    }


    /// <summary>The number of main web pages in the site</summary>
    public int MainPageCount {
      get { return mainCount; }
    }


    /// <summary>Returns the <see cref="WebPage"/> with the given id</summary>
    /// <param name="pageid">Unique id of a <see cref="WebPage"/></param>
    /// <returns><see cref="WebPage"/> with given page id</returns>
    public WebPage GetPageById(String pageid) {
      WebPage aPage = null;
      DataSet ds = DB.GetDS("select * from webpage where deleted=0 and id='" + pageid + "'");
      if (DB.GetRowCount(ds) == 1) {
        DataRow pagerow = DB.GetRow(ds, 0);
        aPage = new WebPage(this, pagerow);
      }
      return aPage;
    }


    /// <summary>Returns the page property with the given id</summary>
    public PageProperty GetPropertyById(int propid) {
      PageProperty aProp = null;
      DataSet ds = DB.GetDS("select * from pageproperty where Id = " + propid);
      if (DB.GetRowCount(ds) > 0) 
        aProp = new PageProperty(this, DB.GetRow(ds, 0));
      return aProp;
    }

    /// <summary>Returns the common page property for all pages</summary>
    public PageProperty CommonProperty {
      get {
        if (commonProperty == null) {
          String cid = DB.GetString("select id from pageproperty where parentproperty is null and propertyname='_CommonProp_'", "id");
          if (cid.Length == 0) {
            DB.ExecSql("insert into pageproperty (propertyname) values ('_CommonProp_')");
            cid = DB.GetString("select id from pageproperty where parentproperty is null and propertyname='_CommonProp_'", "id");
          }
          commonProperty = GetPropertyById(Convert.ToInt32(cid));
        }
        return commonProperty;
      }
    }


    /// <summary>Reads a <see cref="WebPage"/> from the given <see cref="DataRow"/> and adds it to the <see cref="BWInterface"/> </summary>
    /// <param name="pagerow"><see cref="DataRow"/> containing web page info</param>
    private void AddPage(DataRow pagerow) {
      WebPage newpage = new WebPage(this, pagerow);
    }


    /// <summary>Creates a <see cref="WebPage"/> in the database and adds it to the <see cref="BWInterface"/></summary>
    /// <param name="pageId">Id of the web page</param>
    /// <param name="parentId">Page id of the parent web page</param>
    /// <param name="pageName">Name of the web page</param>
    /// <param name="pageTitle">Title of the web page</param>
    /// <param name="fileName">Filename (aspx) of the web page</param>
    /// <param name="pageStatus">Initial status (active|hidden|inactive) of the web page</param>
    /// <param name="languages">Languages for which the page should be available</param>
    /// <param name="pageKeys">Keywords of the web page (as rendered in the meta tag)</param>
    /// <param name="pageDescription">Description of the web page (as rendered in the meta tag)</param>
    /// <param name="pageOrder">Order number for the web page</param>
    /// <param name="pageProtected">True ig the page should be protected by permissions</param>
    /// <param name="redirtochild">True if redirection should be automatically performed to the first child page (if it exists).</param>
    /// <param name="redirect">Id of a web page to redirect to</param>
    /// <param name="sharedPropId">Id of the shared property of the web page</param>
    /// <returns>Empty string if successful. Otherwise error message.</returns>
    public String CreatePage(String pageId, String parentId, String pageName, String pageTitle, String fileName, String pageStatus, String languages, bool flash, String pageKeys, String pageDescription, String pageOrder, String pageProtection, bool redirtochild, String redirect, String sharedPropId) {

      PageProperty mainProp = new PageProperty(this, null, pageId, "");

      String sqlParentId = "null";
      if (parentId != null && parentId.Length > 1)
        sqlParentId = "'" + parentId + "'";
      if (sqlParentId == "'NoParent'" || sqlParentId == "'_root_'") sqlParentId = "null";
      int aorder;
      try { aorder = Convert.ToInt32(pageOrder); }
      catch { aorder = 0; }

      /*      String sqlSharedPropId = "null";
            if (sharedPropId != null && sharedPropId.Length > 1) 
              sqlSharedPropId = DB.GetString("select sharedprop from webpage where deleted=0 and id='" + sharedPropId + "'", "sharedprop");
      */
      String sqlSharedPropId = (sharedPropId != null && sharedPropId.Length > 1 ? sharedPropId : "null");

      String sql = "insert into webpage (id, name, title, filename, pagestatus, keywords, description, orderno, redirecttochild, redirectto, parentpage, flashpage, mainprop, sharedprop, protected, languages, moddate, modby, deleted) "
        + "values('" + pageId + "', '" + pageName + "', '" + pageTitle + "', '" + fileName + "', '" + pageStatus + "', '" + pageKeys + "', '" + pageDescription + "', " + aorder + ", " + (redirtochild ? 1 : 0) + ", '" + redirect + "', " + sqlParentId + ", " + (flash ? 1 : 0) + ", " + mainProp.Id.ToString() + ", " + sqlSharedPropId + ",'" + pageProtection + "','" + languages + "','" + DateTime.Now.ToString(SiteSetting("dateTimeFormat")) + "', " + User.Id + ", 1)";
      try {
        DB.ExecSql(sql);
        DB.ExecSql("update webpage set deleted=0 where id='" + pageId + "'");
        DataSet ds = DB.GetDS("select * from webpage where deleted=0 and id='" + pageId + "'");
        DataRow pagerow = DB.GetRow(ds, 0);

        AddPage(pagerow);

        LogEvent("createpage", pageId);
      }
      catch (Exception ex) {
        return ex.Message;
      }
      return "";
    }


    /// <summary>Deletes a <see cref="WebPage"/> from the database and from the BWInterface</summary>
    /// <param name="delpageid">Id of web page to delete</param>
    public void DeletePage(String delpageid) {
      WebPage delpage = GetPageById(delpageid);
      delpage.Delete();
    }


    public void RestoreProp(DataSet ds, int idx) {
      int id = DB.GetInt(ds, idx, "propid");
      PageProperty aProp = this.GetPropertyById(id);
      String sql = "update pageproperty set propertyvalue={0}, prelimpropertyvalue={1}, visible={2}, published={3}, parentproperty={4}, startdate={5}, enddate={6}, orderno={7}, deleted={8} where id={9}";
      sql = String.Format(sql, DB.GetValAsString(ds, idx, "propertyvalue"), DB.GetValAsString(ds, idx, "prelimpropertyvalue"), DB.GetValAsString(ds, idx, "visible"), DB.GetValAsString(ds, idx, "published"), DB.GetValAsString(ds, idx, "parentproperty"), DB.GetValAsString(ds, idx, "startdate"), DB.GetValAsString(ds, idx, "enddate"), DB.GetValAsString(ds, idx, "orderno"), DB.GetValAsString(ds, idx, "deleted"), DB.GetValAsString(ds, idx, "propid"));
      DB.ExecSql(sql);
      aProp.Refresh();
    }

    public void RestorePage(DataSet ds, int idx) {
      String id = DB.GetString(ds, idx, "pageid");
      String sql = "update webpage set name={0}, filename={1}, title={2}, keywords={3}, description={4}, orderno={5}, redirecttochild={6}, redirectto={7}, parentpage={8}, mainprop={9}, sharedprop={10}, protected={11}, flashpage={12}, pagestatus={13}, languages={14}, moddate={15}, modby={16}, deleted={17} where id={18}";
      sql = String.Format(sql, DB.GetValAsString(ds, idx, "name"), DB.GetValAsString(ds, idx, "filename"), DB.GetValAsString(ds, idx, "title"), DB.GetValAsString(ds, idx, "keywords"), DB.GetValAsString(ds, idx, "description"), DB.GetValAsString(ds, idx, "orderno"), DB.GetValAsString(ds, idx, "redirecttochild"), DB.GetValAsString(ds, idx, "redirectto"), DB.GetValAsString(ds, idx, "parentpage"), DB.GetValAsString(ds, idx, "mainprop"), DB.GetValAsString(ds, idx, "sharedprop"), DB.GetValAsString(ds, idx, "protected"), DB.GetValAsString(ds, idx, "flashpage"), DB.GetValAsString(ds, idx, "pagestatus"), DB.GetValAsString(ds, idx, "languages"), DB.GetValAsString(ds, idx, "moddate"), DB.GetValAsString(ds, idx, "modby"), DB.GetValAsString(ds, idx, "deleted"), DB.GetValAsString(ds, idx, "pageid"));
      DB.ExecSql(sql);
      WebPage aPage = this.GetPageById(id);
      aPage.Refresh();
    }


    /// <summary>Checks if a given username/password combination exists for the given usertype</summary>
    /// <param name="username">Username to check</param>
    /// <param name="password">Password to check</param>
    /// <param name="usertype">Type of user (Normal|Administrator)</param>
    /// <returns>Empty string if successful validation. Otherwise error message.</returns>
    public String ValidateLogIn(String username, String password, String usertype) {
      if (username.Length == 0) return Translate("Ange ett anvndarnamn");
      if (password.Length == 0) return Translate("Ange ett lsenord");
      if (username.Contains("'") || password.Contains("'")) return Translate("Anvndaruppgifter inte accepterade") + ".";
      String sql;
      if (usertype.Length > 0)
        sql = "select * from users where deleted=0 and usertype='" + usertype + "' and username='" + username + "' and password='" + password + "' and approved='Y'";
      else
        sql = "select * from users where deleted=0 and username='" + username + "' and password='" + password + "' and approved='Y'";
      DataSet ds = DB.GetDS(sql);
      if (DB.GetRowCount(ds) == 1) return "";
      else return Translate("Anvndaruppgifter inte accepterade") + ".";
    }


    /// <summary>Attempts to log in a user</summary>
    /// <param name="username">User name</param>
    /// <param name="password">Password</param>
    /// <returns>Error message if login failed. Empty string if success.</returns>
    public String LogIn(String username, String password) {
      return LogIn(username, password, "");
    }


    public String LogIn(String username, String password, String usertype) {
      String mess = ValidateLogIn(username, password, usertype);
      if (mess.Length == 0) {
        int aId = DB.GetInt("select id from users where deleted=0 and username='" + username + "' and password='" + password + "'", "id");
        User.LogIn(aId);
        try {
          String timeout = DateTime.Now.AddMinutes(-Convert.ToDouble(SiteSetting("lockTimeout"))).ToString(SiteSetting("dateTimeFormat"));
          DB.ExecSql("update pageproperty set locked='', locktime=NULL where locked <> '' and (locktime is null or locktime < '" + timeout + "')");
        }
        catch {}
      }
      return mess;
    }

    /// <summary>Logs out the current user</summary>
    /// <param name="sessionid">Id string of the current session</param>
    public void LogOut(String sessionid) {
      User.LogOut(sessionid);
    }

    /// <summary>Logs out the current user</summary>
    public void LogOut() {
      LogOut("");
    }

    /// <summary>Checks whether the given username or email address is found. 
    /// In that case an email to the email address of the found user is sent containing password information for login</summary>
    /// <param name="sitename">Name of the site (e.g. www.brandinternet.se)</param>
    /// <param name="username">Username to search for</param>
    /// <param name="email">Email address to search for</param>
    public String RequestPassword(String username, String email) {
      String sitename = Util.GetBaseUrl();

      String result = "";
      String mailres = "";
      ArrayList unameres = new ArrayList();
      ArrayList pwres = new ArrayList();
      bool unameFound = false;
      DataSet ds = DB.GetDS("select * from users where deleted=0 and username='" + username + "' and approved='Y'");
      if (DB.GetRowCount(ds) > 0) {
        unameFound = true;
        mailres = DB.GetString(ds, 0, "email");
        unameres.Add(DB.GetString(ds, 0, "username"));
        pwres.Add(DB.GetString(ds, 0, "password"));
      }
      else {
        DataSet ds2 = DB.GetDS("select * from users where deleted=0 and email='" + email + "' and approved='Y'");
        if (DB.GetRowCount(ds2) > 0) {
          mailres = email;
          for (int i = 0; i < DB.GetRowCount(ds2); i++) {
            unameres.Add(DB.GetString(ds2, i, "username"));
            pwres.Add(DB.GetString(ds2, i, "password"));
          }
        }
      }
      if (mailres.Length > 0) {
        String body;
        if (unameFound)
          body = Translate("Följande information finns registrerad för användare") + " " + username + " " + Translate("på") + sitename + ":";
        else
          body = Translate("Följande information finns registrerad för e-postadress") + " " + email + " " + Translate("på") + sitename + ":";
        for (int i = 0; i < unameres.Count; i++) {
          body += "\r\n\r\n" + Translate("Användarnamn") + ": " + unameres[i].ToString();
          body += "\r\n" + Translate("Lösenord") + ": " + pwres[i].ToString();
        }
        body += "\r\n\r\n" + Translate("Detta är ett automatiskt genererat mejl") + ".";
        
        result = Util.SendMail(null, mailres, Translate("Information för inloggning på") + " " + sitename, body, false);      
        if (result.Length == 0) result = mailres;
      }
      else if (username.Length == 0 && email.Length == 0)
        result = "NoInput";
      else if (username.Length > 0 && !unameFound)
        result = "NoUser";
      else if (email.Length > 0 && !unameFound)
        result = "NoUserWithMail";
      else
        result = "NoMailForUser";
      return result;
    }


    public String RegisterUser(String usertype, String username, String password, String email, String description, String roles, bool approved, Hashtable attrib) {
      String sql = @"
          insert into users (usertype, username, password, email, description, approved, regdate)
          values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')";
      try {
        DB.ExecSql(String.Format(sql, usertype, username, password, email, description, (approved ? "Y" : "N"), DateTime.Now.ToString(CMS.SiteSetting("dateFormat"))));
        int uid = DB.GetInt("select id from users where deleted=0 and username='" + username + "'", "id");
        if (roles.Length > 0) {
          String[] rolesarr = roles.Split(',');
          int typeid = DB.GetInt("select id from permissiontypes where itemtype='User'", "id");
          for (int i = 0; i < rolesarr.Length; i++)
            DB.ExecSql("insert into Permissions(id, typeid, role, permission) values(" + uid + ", " + typeid + ", '" + rolesarr[i] + "', 'Y')");
        }
        foreach (DictionaryEntry d in attrib) {
          int attribid = DB.GetInt("select id from userattribfields where name='" + d.Key + "'", "id");
          if (attribid != 0)
            DB.ExecSql("insert into userattrib (userid, fieldid, svalue) values(" + uid + ", " + attribid + ", '" + d.Value + "')");
        }
      }
      catch (Exception ex) { return "Ett fel uppstod vid registreringen: " + ex.Message; }
      return "";
    }

    public void ApproveUser(String username, bool approve) {
      if (approve)
        DB.ExecSql("update users set approved='Y' where deleted=0 and username='" + username + "'");
      else {
        int uid = DB.GetInt("select id from users where deleted=0 and username='" + username + "'", "id");
        DeleteUser(uid);
      }
    }

    public void DeleteUser(int userid) {
      DB.ExecSql("update users set deleted=1, deletedate='" + DateTime.Now.ToString(SiteSetting("dateTimeFormat")) + "' where id=" + userid);
    }

    public void RestoreUser(int userid) {
      DB.ExecSql("update users set deleted=0, deletedate=null where id=" + userid);
    }

    /// <summary>Returns information about the backweb administration pages in the system.</summary> 
    public ArrayList GetBackWebPages() {
      DataSet ds = DB.GetDS("select * from backweb order by orderno");
      ArrayList res = new ArrayList();
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        int id = DB.GetInt(ds, i, "id");
        if (GetPermission(id.ToString(), "BackWeb", "DEFAULT", "")) {
          Hashtable page = new Hashtable();
          page.Add("Name", DB.GetString(ds, i, "Name"));
          page.Add("Text", DB.GetString(ds, i, "Text"));
          page.Add("Description", DB.GetString(ds, i, "Description"));
          page.Add("URL", DB.GetString(ds, i, "PageUrl"));
          res.Add(page);
        }
      }
      return res;
    }

    public void LogEvent(String action, String itemid) {
      String sql = "insert into adminlog (userid, eventtime, eventaction, itemid) values (" + User.Id + ", '" + DateTime.Now.ToString(SiteSetting("dateTimeFormat")) + "', '" + action + "', '" + itemid + "' )";
      DB.ExecSql(sql);
    }

    public String GetPagesTree() {
      return GetPagesTree("");
    }

    private bool VisiblePage(String status, String timeControlled, String start, String end) {
      bool visible = (status == "active");
      if (timeControlled.ToLower() == "true") {
        try {
          DateTime t1 = DateTime.Parse(start);
          DateTime t2 = DateTime.Parse(end);
          visible = t1 <= DateTime.Now && t2 > DateTime.Now;
        }
        catch (Exception ex) {Util.Debug(ex.Message);}
      }
      return visible;
    }


    public String GetPagesTree(String condition) {
      if (condition.Length == 0) condition = "1=1";
      String html = "<ul id='allpagestree' class='filetree'>";
      DataSet ds = DB.GetDS("select id, name, pagestatus, parentpage, timecontrolled, starttime, endtime from webpage where deleted=0 and pagestatus <> 'inactive' and languages like '%" + Language + "%' and " + condition + " order by orderno");
      DataRow[] rows = ds.Tables[0].Select("(parentpage is null or parentpage='')");
      for (int i=0; i < rows.Length; i++) {
        bool visible = VisiblePage(rows[i][2].ToString(), rows[i][4].ToString(), rows[i][5].ToString(), rows[i][6].ToString());  
        html += GetPageNode(rows[i][0].ToString(), rows[i][1].ToString(), visible, ds);
      }
      html += "</ul>";
      return html;
    }

    private String GetPageNode(String pageid, String pagename, bool visible, DataSet ds) {
      String html = "<li>";
      DataRow[] rows = ds.Tables[0].Select("parentpage = '" + pageid + "'");
      String anchor = "<a href='" + pageid + ".aspx' class='" + (visible ? "" : "inactivepage") + "' onfocus='this.blur()'>" + Translate(pagename) + "</a>";
      if (rows.Length == 0)
        html += anchor;
      else {
        html += "<div class='hitarea collapsable-hitarea'></div>" + anchor + "<ul>";
        for (int i=0; i < rows.Length; i++) {
          bool vis2 = VisiblePage(rows[i][2].ToString(), rows[i][4].ToString(), rows[i][5].ToString(), rows[i][6].ToString());  
          html += GetPageNode(rows[i][0].ToString(), rows[i][1].ToString(), vis2, ds);
        }
        html += "</ul>";
      }
      html += "</li>";
      return html;
    }
    
  }
}