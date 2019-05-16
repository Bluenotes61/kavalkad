/* $Date: 2010-11-17 15:25:18 +0100 (on, 17 nov 2010) $    $Revision: 7088 $ */
using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using NFN;

namespace NFN.Controls {

  /********************************************************************************
  *
  * Class PageMenu
  *
  *********************************************************************************/

  /// <summary></summary>
  public class PageMenu : Control, INamingContainer {

    public enum LinkedItems { Div, Anchor };
    public enum Layouts { List, Div, JS };
    public enum JSActions { OnClick, OnOver };
    public enum ItemContents { Name, Title, Id, Template };
    public enum PageTypes { Protected, Unprotected, Both };

    private HtmlGenericControl container = null;
    private String parentpageid = "";
    private String itemtemplate = "";
    private String selitemtemplate = "";
    private String lastitemtemplate = "";
    private String firstitemtemplate = "";
    private String separatortemplate = "";
    private String menuclass = "";
    private String itemclass = "";
    private String selitemclass = "";
    private String linkeditemclass = "";
    private String linkedselitemclass = "";
    private bool activeselection = false;
    private PageTypes pageType = PageTypes.Both;
    private string orderby = "orderno";
    private String onitemclick = "";
    private String onitemmouseover = "";
    private String onitemmouseout = "";
    private String onmenumouseover = "";
    private String onmenumouseout = "";
    private String onmenumousemove = "";
    private String ontogglesub = "";
    private String onshowsub = "";
    private String onhidesub = "";
    private String css = "";
    private String itemheight = "50px";
    private String itemwidth = "100px";
    protected ArrayList menuitems = null;
    private int menulevel = 0;
    private int maxmenulevel = -1;
    private Layouts layout = Layouts.List;
    private JSActions jsaction = JSActions.OnClick; 
    private ItemContents itemcontent = ItemContents.Name;
    private LinkedItems linkeditem = LinkedItems.Anchor;
    private String precontent = "";
    private String postcontent = "";
    private PageMenu subMenu = null;
    private bool alwaysopen = false;
    private bool keepparentselected = true;
    private int nofSubs = 0;

    /// <summary></summary>
    protected CMS Cms {
      get { return (CMS)Page.Session["CMS"]; }
    }

    protected HtmlGenericControl Container {
      get { 
        if (container == null) container = new HtmlGenericControl("div");
        return container; 
      }
    }

    public PageMenu CreateSubMenu() {
      subMenu = new PageMenu();
      subMenu.MenuLevel = this.MenuLevel + 1;
      nofSubs++;
      subMenu.ID = this.ID + "_" + nofSubs.ToString();
      return subMenu;
    }

    public PageMenu GetAdditionalSubMenu(String parentPageId) {
      if (subMenu == null) return null;
      PageMenu aMenu = SubMenu.Clone();
      aMenu.ParentPageId = parentPageId;
      nofSubs++;
      aMenu.ID = this.ID + "_" + nofSubs.ToString();
      return aMenu;
    }

    public PageMenu SubMenu {
      get { 
        if (subMenu == null) CreateSubMenu();
        return subMenu; 
      }
    }

    public PageMenu Clone() {
      PageMenu aMenu = new PageMenu();
      aMenu.Layout = this.Layout;
      aMenu.PreContent = this.PreContent;
      aMenu.PostContent = this.PostContent;
      aMenu.ItemContent = this.ItemContent;
      aMenu.ItemTemplate = this.ItemTemplate;
      aMenu.SelItemTemplate = this.SelItemTemplate;
      aMenu.FirstItemTemplate = this.FirstItemTemplate;
      aMenu.LastItemTemplate = this.LastItemTemplate;
      aMenu.ActiveSelection = this.ActiveSelection;
      aMenu.OnItemClick = this.OnItemClick;
      aMenu.OnItemMouseOut = this.OnItemMouseOut;
      aMenu.OnItemMouseOver = this.OnItemMouseOver;
      aMenu.OnMenuMouseOver = this.OnMenuMouseOver;
      aMenu.OnMenuMouseOut = this.OnMenuMouseOut;
      aMenu.OnMenuMouseMove = this.OnMenuMouseMove;
      aMenu.OnToggleSub = this.OnToggleSub;
      aMenu.OnShowSub = this.OnShowSub;
      aMenu.OnHideSub = this.OnHideSub;
      aMenu.MenuLevel = this.MenuLevel;
      aMenu.ParentPageId = this.ParentPageId;
      aMenu.MenuClass = this.MenuClass;
      aMenu.ItemClass = this.ItemClass;
      aMenu.SelItemClass = this.SelItemClass;
      aMenu.LinkedItemClass = this.LinkedItemClass;
      aMenu.LinkedSelItemClass = this.LinkedSelItemClass;
      aMenu.LinkedItem = this.LinkedItem;
      aMenu.Css = this.Css;
      aMenu.ItemWidth = this.ItemWidth;
      aMenu.AlwaysOpen = this.AlwaysOpen;
      if (this.subMenu != null) aMenu.subMenu = this.subMenu.Clone();
      return aMenu;
    }

    public Layouts Layout {
      get { return layout; }
      set { layout = value; }
    }

    public String PreContent {
      get { return precontent; }
      set { precontent = value; }
    }

    public String PostContent {
      get { return postcontent; }
      set { postcontent = value; }
    }

    public ItemContents ItemContent {
      get { return itemcontent; }
      set { itemcontent = value; }
    }

    /// <summary></summary>
    public String ItemTemplate {
      get { return itemtemplate; }
      set { itemtemplate = value; }
    }

    public String SelItemTemplate {
      get { return selitemtemplate; }
      set { selitemtemplate = value; }
    }

    public String FirstItemTemplate {
      get { return firstitemtemplate; }
      set { firstitemtemplate = value; }
    }

    public String SeparatorTemplate {
      get { return separatortemplate; }
      set { separatortemplate = value; }
    }

    /// <summary></summary>
    public String LastItemTemplate {
      get { return lastitemtemplate; }
      set { lastitemtemplate = value; }
    }

    public bool ActiveSelection {
      get { return activeselection; }
      set { activeselection = value; }
    }

    public PageTypes PageType {
      get { return pageType; }
      set { pageType = value; }
    }

    public String OrderBy {
      get { return orderby; }
      set { orderby = value; }
    }

    public String OnItemClick {
      get { return onitemclick; }
      set { onitemclick = value; }
    }

    public String OnItemMouseOver {
      get { return onitemmouseover; }
      set { onitemmouseover = value; }
    }

    public String OnItemMouseOut {
      get { return onitemmouseout; }
      set { onitemmouseout = value; }
    }

    public String OnMenuMouseOut {
      get { return onmenumouseout; }
      set { onmenumouseout = value; }
    }

    public String OnMenuMouseOver {
      get { return onmenumouseover; }
      set { onmenumouseover = value; }
    }

    public String OnMenuMouseMove {
      get { return onmenumousemove; }
      set { onmenumousemove = value; }
    }

    public JSActions JSAction {
      get { return jsaction; }
      set { jsaction = value; }
    }

    public LinkedItems LinkedItem {
      get { return linkeditem; }
      set { linkeditem = value; }
    }

    public String OnShowSub {
      get {
        if (onshowsub.Length == 0) onshowsub = "openSubMenu(|item|)";
        return onshowsub; 
      }
      set { onshowsub = value; }
    }

    public String OnHideSub {
      get {
        if (onhidesub.Length == 0) onhidesub = "closeSubMenu(|item|)";
        return onhidesub;
      }
      set { onhidesub = value; }
    }

    public String OnToggleSub {
      get {
        if (ontogglesub.Length == 0) ontogglesub = "toggleSubMenu(|item|)";
        return ontogglesub;
      }
      set { ontogglesub = value; }
    }

    protected int ThisLevel {
      get {
        int level = 0;
        WebPage aPage = ThisPage;
        while (aPage.Parent != null) {
          level++;
          aPage = aPage.Parent;
        }
        return level;
      }
    }

    public int MenuLevel {
      get { return menulevel; }
      set { menulevel = value; }
    }

    public int MaxMenuLevel {
      get { return maxmenulevel; }
      set { maxmenulevel = value; }
    }

    protected WebPage ThisPage {
      get { return ((BasePage)Page).ThisPage; }
    }

    /// <summary></summary>
    public String ParentPageId {
      get { return parentpageid; }
      set { parentpageid = value; }
    }

    public String MenuClass {
      get { return menuclass; }
      set { menuclass = value; }
    }

    public String ItemClass {
      get { return itemclass; }
      set { itemclass = value; }
    }

    public String SelItemClass {
      get { return selitemclass; }
      set { selitemclass = value; }
    }

    public String LinkedItemClass {
      get { return linkeditemclass; }
      set { linkeditemclass = value; }
    }

    public String LinkedSelItemClass {
      get { return linkedselitemclass; }
      set { linkedselitemclass = value; }
    }

    public String Css {
      get { return css; }
      set { css = value; }
    }

    public String ItemHeight {
      get { return itemheight; }
      set { itemheight = value; }
    }

    public String ItemWidth {
      get { return itemwidth; }
      set { itemwidth = value; }
    }

    public bool AlwaysOpen {
      get { return alwaysopen; }
      set { alwaysopen = value; }
    }

    public bool KeepParentSelected {
      get { return keepparentselected; }
      set { keepparentselected = value; }
    }

    public int ItemCount {
      get { return MenuItems.Count; }
    }

    protected virtual ArrayList MenuItems {
      get {
        if (menuitems == null) {
          if (ParentPageId.Length == 0) {
            WebPage aParent = ThisPage;
            int aLevel = ThisLevel;
            while (aLevel >= menulevel && aParent != null) {
              aParent = aParent.Parent;
              aLevel--;
            }
            if (aParent == null) ParentPageId = "";
            else ParentPageId = aParent.Id;
          }

          String sql = "select * from webpage where deleted=0 and languages like '%" + Cms.Language + "%' and parentpage " + (ParentPageId.Length == 0 ? "is null" : "='" + ParentPageId + "'") + " and pagestatus='active' and (timecontrolled <> 1 or starttime <= GETDATE() and endtime >= GETDATE()) ";
          if (PageType == PageTypes.Protected) sql += "and protected='Y' ";
          else if (PageType == PageTypes.Unprotected) sql += "and protected<>'Y' ";
          sql += "order by " + OrderBy;
          DataSet ds = DB.GetDS(sql);
          menuitems = new ArrayList();
          for (int i = 0; i < DB.GetRowCount(ds); i++) {
            String currId = DB.GetString(ds, i, "id");
            bool isProt = DB.GetString(ds, i, "protected") == "Y";
            if (!isProt || Cms.GetPermission(currId, "WebPage", "View", ""))
              menuitems.Add(currId + "|" + DB.GetString(ds, i, "name") + "|" + DB.GetString(ds, i, "title") + "|" + DB.GetString(ds, i, "mainprop"));
          }
        }
        return menuitems;
      }
    }

    private bool HasChildPages(String itemId) {
      return DB.GetInt("select count(*) as nof from webpage where deleted=0 and parentpage='" + itemId + "' and pagestatus='active' and (timecontrolled <> 1 or starttime <= GETDATE() and endtime >= GETDATE())", "nof") > 0;
    }

    private bool IsSelected(String id) {
      if (id == ThisPage.Id) return true;
      String parentId = DB.GetString("select parentpage from webpage where deleted=0 and id='" + ThisPage.Id + "'", "parentpage");
      while (parentId.Length > 0) {
        if (parentId == id) return true;
        parentId = DB.GetString("select parentpage from webpage where deleted=0 and id='" + parentId + "'", "parentpage");
      }
      return false;
    }

    private bool RedirectToChild(String id) {
      return DB.GetBoolean("select redirecttochild from webpage where id='" + id + "'", "redirecttochild", true);
    }

    private bool IsLinkedItem(String currId) {
      if (ActiveSelection) return true;
      return (!IsSelected(currId) && !(Layout == Layouts.JS && JSAction == JSActions.OnClick && HasChildPages(currId)));
    }

    private HtmlGenericControl GetItemDivAction(String itemId, HtmlGenericControl item) {
      if (LinkedItem == LinkedItems.Div) {
        String stopEvents = "var e=(arguments[0]?arguments[0]:window.event);e.cancelBubble=true;if(e.stopPropagation)e.stopPropagation();";
        if (Layout == Layouts.JS && HasChildPages(itemId)) {
          if (JSAction == JSActions.OnOver) {
            item.Attributes["onmouseover"] = OnShowSub.Replace("|item|", "this.childNodes[1]");
            item.Attributes["onmouseout"] = OnHideSub.Replace("|item|", "this.childNodes[1]");
          }
          else {
            if (IsLinkedItem(itemId)) item.Attributes["onclick"] = "document.location.href = '/" + itemId + ".aspx';";
            else item.Attributes["onclick"] = OnToggleSub.Replace("|item|", "this.childNodes[1]");
          }
          item.Style["cursor"] = "pointer";
        }
        else if (IsLinkedItem(itemId)) {
          String js = "";
          if (OnItemClick.Length > 0) js = OnItemClick + ";";
          js += "document.location.href = '/" + itemId + ".aspx';" + stopEvents;
          item.Attributes["onclick"] = js;
          item.Style["cursor"] = "pointer";
        }
        else {
          item.Attributes["onclick"] = stopEvents;
          item.Style["cursor"] = "default";
        }
      }
      return item;
    }

    private Control GetItemAction(String itemId, String itemName, Control item) {
      Control res = item;
      if (LinkedItem == LinkedItems.Anchor && Layout == Layouts.JS && HasChildPages(itemId)) {
        HtmlAnchor alink = new HtmlAnchor();
        alink.Attributes["onfocus"] = "this.blur()";
        if (JSAction == JSActions.OnOver) {
          alink.Attributes["onmouseover"] = OnShowSub.Replace("|item|", "this.nextSibling");
          alink.Attributes["onmouseout"] = OnHideSub.Replace("|item|", "this.nextSibling");
        }
        else
          alink.Attributes["onclick"] = OnToggleSub.Replace("|item|", "this.nextSibling");
        alink.HRef = (IsLinkedItem(itemId) ? "/" + itemId + ".aspx" : alink.HRef = "javascript:void(0)");
        alink.Controls.Add(item);
        res = alink;
      }
      else if (LinkedItem == LinkedItems.Anchor && IsLinkedItem(itemId)) {
        HtmlAnchor alink = new HtmlAnchor();
        alink.Attributes["onfocus"] = "this.blur()";
        if (OnItemClick.Length > 0)
          alink.Attributes["onclick"] = OnItemClick;
        alink.Controls.Add(item);
        alink.Title = itemName;
        alink.HRef = "/" + itemId + ".aspx";
        res = alink;
      }
      else {
        HtmlGenericControl aspan = new HtmlGenericControl("span");
        aspan.Controls.Add(item);
        res = aspan;
      }
      return res;
    }

    private String GetTemplate(String currId, int i) {
      if (SelItemTemplate.Length > 0 && (currId == ThisPage.Id || (KeepParentSelected && IsSelected(currId))))
        return SelItemTemplate;
      else if (i == 0 && FirstItemTemplate.Length > 0)
        return FirstItemTemplate;
      else if (i == MenuItems.Count - 1 && LastItemTemplate.Length > 0)
        return LastItemTemplate;
      else if (ItemTemplate.Length > 0)
        return ItemTemplate;
      else
        return "";
    }

    protected override void OnPreRender(EventArgs e) {
      base.OnPreRender(e);
    }

    protected override void CreateChildControls() {
      base.CreateChildControls();

      if (MenuItems.Count > 0) {
        if (MenuClass.Length > 0) Container.Attributes["class"] = MenuClass;
        Container.ID = this.ID + "_list";

        if (OnMenuMouseOver.Length > 0) Container.Attributes["onmouseover"] = OnMenuMouseOver;
        if (OnMenuMouseOut.Length > 0) Container.Attributes["onmouseout"] = OnMenuMouseOut;
        if (OnMenuMouseMove.Length > 0) Container.Attributes["onmousemove"] = OnMenuMouseMove;
        this.Controls.Add(Container);

        if (PreContent.Length > 0)
          Container.Controls.Add(new LiteralControl(PreContent));

        HtmlGenericControl parenttag = Container;
        if (Layout == Layouts.List) {
          HtmlGenericControl ul = new HtmlGenericControl("ul");
          Container.Controls.Add(ul);
          parenttag = ul;
        }

        for (int i = 0; i < MenuItems.Count; i++) {
          if (SeparatorTemplate.Length > 0 && i > 0) {
            HtmlGenericControl sep = (Layout == Layouts.List ? new HtmlGenericControl("li") : new HtmlGenericControl("div"));
            sep.InnerHtml = SeparatorTemplate;
            parenttag.Controls.Add(sep);
          }

          String[] help = MenuItems[i].ToString().Split('|');
          String currId = help[0];
          String currName = Cms.Translate(help[1]);
          String currTitle = Cms.Translate(help[2]);
          String currMainPropId = help[3];

          String template = GetTemplate(currId, i);

          Control itemElement;
          if (ItemContent == ItemContents.Name) itemElement = new LiteralControl(NFN.Util.ReplaceHtmlChars(currName));
          else if (ItemContent == ItemContents.Id) itemElement = new LiteralControl("<span>" + NFN.Util.ReplaceHtmlChars(currId) + "</span>");
          else if (ItemContent == ItemContents.Title) itemElement = new LiteralControl(NFN.Util.ReplaceHtmlChars(currTitle));
          else if (ItemContent == ItemContents.Template) itemElement = new LiteralControl(template.Replace("|id|", currId).Replace("|url|", Util.HtmlRoot + "/" + currId + ".aspx").Replace("|name|", currName).Replace("|title|", currTitle));
          else itemElement = new LiteralControl(NFN.Util.ReplaceHtmlChars(currName));

          itemElement = GetItemAction(currId, currName, itemElement);

          ArrayList itemElements = new ArrayList();
          itemElements.Add(itemElement);

          if (ItemContent != ItemContents.Template && template.Length > 0) {
            Control aItem = (Control)itemElements[0];
            itemElements.Clear();
            template = template.Replace("¤text¤", "§").Replace("|id|", currId).Replace("|url|", Util.HtmlRoot + "/" + currId + ".aspx").Replace("|name|", currName).Replace("|title|", currTitle);
            String[] tmpl = template.Split('§');
            for (int j = 0; j < tmpl.Length - 1; j++) {
              itemElements.Add(new LiteralControl(tmpl[j]));
              itemElements.Add(aItem);
            }
            itemElements.Add(new LiteralControl(tmpl[tmpl.Length - 1]));
          }

          HtmlGenericControl currChild = (Layout == Layouts.List ? new HtmlGenericControl("li") : new HtmlGenericControl("div"));
          currChild = GetItemDivAction(currId, currChild);
          parenttag.Controls.Add(currChild);

          if (IsSelected(currId)) {
            if (SelItemClass.Length > 0) currChild.Attributes["class"] = SelItemClass;
            if (LinkedSelItemClass.Length > 0 && ThisPage.Id == currId) currChild.Attributes["class"] = LinkedSelItemClass;
            for (int j = 0; j < itemElements.Count; j++)
              currChild.Controls.Add((Control)itemElements[j]);

            if ((MaxMenuLevel == -1 || ThisLevel < MaxMenuLevel) && subMenu != null && HasChildPages(currId)) {
              if (Layout == Layouts.JS) {
                SubMenu.Container.Style["display"] = "block";
                if (JSAction == JSActions.OnOver) {
                  SubMenu.OnMenuMouseOver = OnShowSub.Replace("|item|", "this");
                  SubMenu.OnMenuMouseOut = OnHideSub.Replace("|item|", "this");
                }
              }
              currChild.Controls.Add(SubMenu);
            }
          }
          else {
            if (ItemClass.Length > 0) currChild.Attributes["class"] = ItemClass;
            if (LinkedItemClass.Length > 0 && IsLinkedItem(currId)) currChild.Attributes["class"] = LinkedItemClass;
            for (int j = 0; j < itemElements.Count; j++)
              currChild.Controls.Add((Control)itemElements[j]);

            if ((Layout == Layouts.JS || AlwaysOpen) && subMenu != null && HasChildPages(currId)) {
              PageMenu sub = GetAdditionalSubMenu(currId);
              if (Layout == Layouts.JS) {
                sub.Container.Style["display"] = "none";
                if (JSAction == JSActions.OnOver) {
                  sub.OnMenuMouseOver = OnShowSub.Replace("|item|", "this");
                  sub.OnMenuMouseOut = OnHideSub.Replace("|item|", "this");
                }
              }
              currChild.Controls.Add(sub);
            }
          }
          if (OnItemMouseOver.Length > 0) currChild.Attributes["onmouseover"] = OnItemMouseOver;
          if (OnItemMouseOut.Length > 0) currChild.Attributes["onmouseout"] = OnItemMouseOut;
        }
        if (PostContent.Length > 0) {
          HtmlGenericControl postChild = (Layout == Layouts.List ? new HtmlGenericControl("li") : new HtmlGenericControl("div"));
          postChild.Controls.Add(new LiteralControl(PostContent));
          parenttag.Controls.Add(postChild);
        }
      }
    }
  }




  /********************************************************************************
  *
  * Class DropDownMenu
  *
  *********************************************************************************/

  public class DropDownMenu : Control, INamingContainer {

    private bool clickableMain = false;
    private bool clickableSelected = false;
    
    protected CMS Cms {
      get { return (CMS)Page.Session["CMS"]; }
    }

    private WebPage ThisPage {
      get { return ((BasePage)Page).ThisPage; }
    }
    
    public bool ClickableMain {
      get { return clickableMain; }
      set { clickableMain = value; }
    }

    public bool ClickableSelected {
      get { return clickableSelected; }
      set { clickableSelected = value; }
    }

    private bool IsSelected(String id) {
      if (id == ThisPage.Id) return true;
      String parentId = DB.GetString("select parentpage from webpage where deleted=0 and id='" + ThisPage.Id + "'", "parentpage");
      while (parentId.Length > 0) {
        if (parentId == id) return true;
        parentId = DB.GetString("select parentpage from webpage where deleted=0 and id='" + parentId + "'", "parentpage");
      }
      return false;
    }
    
    private String[][] GetMenuItems(String parentPage) {
      String snip = (parentPage.Length == 0 ? "is null" : "='" + parentPage + "'");
      String sql = "select w.id, w.protected, w.name, w.redirecttochild, (select count(*) as nof from webpage w2 where w2.deleted=0 and w2.parentpage=w.id and w2.languages like '%" + Cms.Language + "%' and w2.pagestatus='active' and (w2.timecontrolled <> 1 or w2.starttime <= GETDATE() and w2.endtime >= GETDATE())) as nofchildren from webpage w where w.deleted=0 and w.languages like '%" + Cms.Language + "%' and w.parentpage " + snip + " and w.pagestatus='active' and (w.timecontrolled <> 1 or w.starttime <= GETDATE() and w.endtime >= GETDATE()) order by orderno";
      DataSet ds = DB.GetDS(sql);
      String[][] items = new String[DB.GetRowCount(ds)][];
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        String currId = DB.GetString(ds, i, "id");
        bool isProt = DB.GetString(ds, i, "protected") == "Y";
        if (!isProt || Cms.GetPermission(currId, "WebPage", "View", "")) {
          items[i] = new String[4];
          items[i][0] = currId;
          items[i][1] = Cms.Translate(DB.GetString(ds, i, "name"));
          items[i][2] = (DB.GetInt(ds, i, "nofchildren") > 0 ? "y" : "n");
          items[i][3] = (DB.GetBoolean(ds, i, "redirecttochild") ? "y" : "n");
        }
      }
      return items;
    }
    
    private String GetSubMenu(String pageId) {
      StringBuilder res = new StringBuilder();
      res.Append("<ul class='nfn_sm_ul'>");
      String[][] menuItems = GetMenuItems(pageId);
      for (int i=0; i < menuItems.Length; i++) {
        if (menuItems[i] != null) {
          String currId = menuItems[i][0];
          String currName = menuItems[i][1];
          bool isselected = IsSelected(currId);

          String classes = (isselected ? "nfn_sm nfn_sm_selected " : "nfn_sm ");
          String item = "<li class='" + classes + "'>";
          if (isselected && !ClickableSelected) item += currName;
          else item += "<a href='" + currId + ".aspx' onfocus='this.blur()'>" + currName + "</a>";
          item += "</li>";
          res.Append(item);
        }
      }
      res.Append("</ul>");
      return res.ToString();
    }

    protected override void CreateChildControls() {
      base.CreateChildControls();

      StringBuilder menuHtml = new StringBuilder();
      menuHtml.Append("<ul id='" + ID + "'>");
      String[][] menuItems = GetMenuItems("");
      for (int i=0; i < menuItems.Length; i++) {
        if (menuItems[i] != null) {
          String currId = menuItems[i][0];
          String currName = menuItems[i][1];
          bool hasChildren = menuItems[i][2] == "y";
          bool redirect = menuItems[i][3] == "y";
          bool isselected = IsSelected(currId);

          String classes = (isselected ? "nfn_mm nfn_mm_selected " : "nfn_mm ");
          if (hasChildren) classes += "nfn_mm_haschildren";
          String item = "<li class='" + classes + "'>";
          String href = (redirect && hasChildren && !ClickableMain ? "javascript:void(0)" : currId + ".aspx");
          item += "<a href='" + href + "' onfocus='this.blur()'>" + currName + "</a>";
          if (hasChildren) item += GetSubMenu(currId);
          item += "</li>";
          menuHtml.Append(item);
        }
      }
      menuHtml.Append("</ul>");
      this.Controls.Add(new LiteralControl(menuHtml.ToString()));
    }
  }




  /********************************************************************************
  *
  * Class BreadCrumb
  *
  *********************************************************************************/

  /// <summary></summary>
  public class BreadCrumb : Control, INamingContainer {
    private bool includeDefaultPage = false;
    private String separator = "&nbsp;/&nbsp;";
    private String textClass = "";
    private String linkClass = "";

    protected override void CreateChildControls() {
      base.CreateChildControls();
      String bcStr = "";
      ArrayList data = GetBreadCrumbData();
      for (int i = 0; i < data.Count; i++) {
        if (i > 0) bcStr += Separator;
        bcStr += data[i].ToString();
      }
      this.Controls.Add(new LiteralControl(bcStr));
    }

    public string TextClass {
      get { return textClass; }
      set { textClass = value; }
    }

    public string LinkClass {
      get { return linkClass; }
      set { linkClass = value; }
    }

    public bool IncludeDefaultPage {
      get { return includeDefaultPage; }
      set { includeDefaultPage = value; }
    }

    public String Separator {
      get { return separator; }
      set { separator = value; }
    }

    protected CMS Cms {
      get { return (CMS)Page.Session["CMS"]; }
    }
    protected BasePage NFNPage {
      get { return (BasePage)Page; }
    }

    protected ArrayList GetBreadCrumbData() {
      int thisLevel = NFNPage.ThisPage.PageLevel;
      ArrayList res = new ArrayList();
      res.Add("<span" + (TextClass.Length > 0 ? " class='" + TextClass + "'" : "") + ">" + NFNPage.ThisPage.Name + "</span>");
      WebPage aPage = NFNPage.ThisPage;
      while (aPage.Parent != null) {
        aPage = aPage.Parent;
        res.Add("<a href='" + Util.HtmlRoot + "/" + aPage.Id + ".aspx' " + (LinkClass.Length > 0 ? " class='" + LinkClass + "'" : "") + "onfocus='this.blur()' title='" + aPage.Name + "'>" + aPage.Name + "</a>");
      }
      if (aPage.Id != NFNPage.ThisMaster.DefaultPageId && IncludeDefaultPage) {
        aPage = Cms.GetPageById(NFNPage.ThisMaster.DefaultPageId);
        res.Add("<a href='" + Util.HtmlRoot + "/" + aPage.Id + ".aspx' " + (LinkClass.Length > 0 ? " class='" + LinkClass + "'" : "") + "onfocus='this.blur()' title='" + aPage.Name + "'>" + aPage.Name + "</a>");
      }
      res.Reverse();
      return res;
    }

  }



  /********************************************************************************
  *
  * Class NewsList
  *
  *********************************************************************************/


  /// <summary></summary>
  public class NewsList : Control, INamingContainer {

    private String groupId = "news";
    private int maxitems = 0;
    private bool selectable = true;
    private bool withindates = false;
    private String displayPage = "news";
    private int currMainPropId = 0;
    private String dateformat = "yyyy-MM-dd";
    private bool showDate = true;
    private bool showHeadline = true;
    private bool showSummary = false;
    private bool clickableDate = true;
    private bool clickableHeadline = true;
    private bool clickableSummary = false;
    private int yearToShow = 0;
    private String itemTemplate = "";
    private String selItemTemplate = "";
    private String linkText = "";
    private String ulClass = "";
    private bool showcontinue = false;
    private int firstitem = -1;
    private int newsid = -1;

    public String GroupId {
      get { return groupId; }
      set { groupId = value; }
    }

    public String UlClass {
      get { return ulClass; }
      set { ulClass = value; }
    }

    public int MaxItems {
      get { return maxitems; }
      set { maxitems = value; }
    }

    public bool ShowContinue {
      get { return showcontinue; }
      set { showcontinue = value; }
    }

    public String ItemTemplate {
      get { return itemTemplate; }
      set { itemTemplate = value; }
    }

    public String SelectedItemTemplate {
      get { return selItemTemplate; }
      set { selItemTemplate = value; }
    }

    public String LinkText {
      get { return linkText; }
      set { linkText = value; }
    }

    public bool ShowDate {
      get { return showDate; }
      set { showDate = value; }
    }

    public bool ShowHeadline {
      get { return showHeadline; }
      set { showHeadline = value; }
    }

    public bool ShowSummary {
      get { return showSummary; }
      set { showSummary = value; }
    }

    public bool ClickableDate {
      get { return clickableDate; }
      set { clickableDate = value; }
    }

    public bool ClickableHeadline {
      get { return clickableHeadline; }
      set { clickableHeadline = value; }
    }

    public bool ClickableSummary {
      get { return clickableSummary; }
      set { clickableSummary = value; }
    }

    public bool Selectable {
      get { return selectable; }
      set { selectable = value; }
    }

    public String DateFormat {
      get { return dateformat; }
      set { dateformat = value; }
    }

    public bool WithinDates {
      get { return withindates; }
      set { withindates = value; }
    }

    public int YearToShow {
      get { return yearToShow; }
      set { yearToShow = value; }
    }

    private String DateSnip {
      get {
        String snip = "";
        if (WithinDates)
          snip = " and newsdate <= GETDATE() and newsdate2 >= GETDATE() ";
        if (YearToShow != 0) {
          String t1 = DateTime.Parse(YearToShow + "-01-01").ToString(CMS.SiteSetting("dateFormat"));
          String t2 = DateTime.Parse(YearToShow + "-12-31").ToString(CMS.SiteSetting("dateFormat"));
          snip += " and newsdate >= '" + t1 + "' and newsdate <= '" + t2 + "' ";
        }
        return snip;
      }
    }

    public int CurrMainPropId {
      get {
        if (((BasePage)Page).PageId != DisplayPage) return 0;
        if (currMainPropId == 0) {
          if (!Page.IsPostBack) {
            try { currMainPropId = NewsId; }
            catch { currMainPropId = 0; }
          }
        }
        if (currMainPropId == 0)
          currMainPropId = DB.GetInt("select top 1 mainpropid from news where showinlist=1 and groupid='" + GroupId + "' " + DateSnip + " order by newsdate desc", "mainpropid");
        return currMainPropId;
      }
      set {
        bool exists = (DB.GetInt("select count(*) as nof from news where showinlist=1 and groupid='" + GroupId + "' " + DateSnip + " and mainpropid=" + value, "nof") > 0);
        if (exists) currMainPropId = value;
      }
    }

    public String DisplayPage {
      get { return displayPage; }
      set { displayPage = value; }
    }

    protected CMS Cms {
      get { return (CMS)Page.Session["CMS"]; }
    }

    private int FirstItem {
      get {
        if (firstitem == -1) {
          try { firstitem = Convert.ToInt32(Page.Request["f"]); }
          catch { firstitem = 0; }
        }
        return firstitem;
      }
      set { firstitem = value; }
    }

    /*    private int NewsId {
          get {
            if (newsid == -1) {
              try { newsid = Convert.ToInt32(Page.Request["newsid"]); }
              catch { newsid = 0; }
            }
            return newsid;
          }
          set { newsid = value; }
        }*/

    private int NewsId {
      get {
        return Convert.ToInt32(Page.Request["newsid"]);
      }
      set { newsid = value; }
    }

    protected override void CreateChildControls() {
      base.CreateChildControls();

      DataSet ds = DB.GetDS("select * from news where newslanguage='" + Cms.Language + "' and groupid='" + GroupId + "' and showinlist=1 " + DateSnip + " order by newsdate desc");
      int nofnews = DB.GetRowCount(ds);
      StringBuilder html = new StringBuilder();
      html.Append((UlClass.Length > 0 ? "<ul id='" + this.ID + "' class='" + UlClass + "'>" : "<ul>"));
      int amax = (MaxItems == 0 ? nofnews : FirstItem + MaxItems);
      for (int i = FirstItem; i < amax && i < nofnews; i++) {
        int mainPropId = DB.GetInt(ds, i, "mainpropid");
        PageProperty aProp = Cms.GetPropertyById(mainPropId);
        String content = aProp.ControlTypeProperty.Value;
        String newsDate = DB.GetDate(ds, i, "newsdate").ToString(DateFormat);
        String headline = DB.GetString(ds, i, "headline");
        int sumPropId = DB.GetInt(ds, i, "summarypropid");
        PageProperty sProp = Cms.GetPropertyById(sumPropId);
        String summary = sProp.ControlTypeProperty.Value;
        String linkAnchor = "<a href='" + Util.HtmlRoot + "/" + DisplayPage + ".aspx?newsid=" + mainPropId + (firstitem > 0 ? "&f=" + firstitem : "") + "' title='{0}' onfocus='this.blur()'>{0}</a>";
        html.Append("<li>");
        if (Selectable && mainPropId == CurrMainPropId) {
          if (SelectedItemTemplate.Length > 0)
            html.Append(SelectedItemTemplate.Replace("¤id¤", mainPropId.ToString()).Replace("¤date¤", newsDate).Replace("¤headline¤", headline).Replace("¤summary¤", summary).Replace("¤linktext¤", "").Replace("¤content¤", content));
          else if (ItemTemplate.Length > 0)
            html.Append(ItemTemplate.Replace("¤id¤", mainPropId.ToString()).Replace("¤date¤", newsDate).Replace("¤headline¤", headline).Replace("¤summary¤", summary).Replace("¤linktext¤", "").Replace("¤content¤", content));
          else {
            if (ShowDate) html.Append("<div class='newsdatesel'>" + newsDate + "</div>");
            if (ShowHeadline) html.Append("<div class='newsheadlinesel'>" + headline + "</div>");
            if (ShowSummary) html.Append("<div class='newssummarysel'>" + summary + "</div>");
          }
        }
        else {
          String dateTxt = (ClickableDate ? String.Format(linkAnchor, newsDate) : newsDate);
          String hlTxt = (ClickableHeadline ? String.Format(linkAnchor, headline) : headline);
          String sumTxt = (ClickableSummary ? String.Format(linkAnchor, summary) : summary);
          String linkTxt = String.Format(linkAnchor, LinkText);
          if (ItemTemplate.Length > 0) {
            html.Append(ItemTemplate.Replace("¤id¤", mainPropId.ToString()).Replace("¤date¤", dateTxt).Replace("¤headline¤", hlTxt).Replace("¤summary¤", sumTxt).Replace("¤linktext¤", linkTxt).Replace("¤content¤", content));
          }
          else {
            if (ShowDate) html.Append("<div class='newsdate'>" + dateTxt + "</div>");
            if (ShowHeadline) html.Append("<div class='newsheadline'>" + hlTxt + "</div>");
            if (ShowSummary) html.Append("<div class='newssummary'>" + sumTxt + "</div>");
          }
        }
        html.Append("</li>");
      }
      html.Append("</ul>");
      this.Controls.Add(new LiteralControl(html.ToString()));

      if (ShowContinue && nofnews > MaxItems) {
        HtmlGenericControl adiv = new HtmlGenericControl("div");
        adiv.Attributes["class"] = "newscontinue";
        int acount = 0;
        int lab = 1;
        while (acount < nofnews) {
          HtmlGenericControl aspan = new HtmlGenericControl("span");
          if (acount >= FirstItem && acount < FirstItem + MaxItems) {
            aspan.InnerHtml = lab.ToString();
          }
          else {
            LinkButton btn = new LinkButton();
            btn.Text = lab.ToString();
            btn.CommandName = "BrowseNews";
            btn.CommandArgument = acount.ToString();
            btn.Command += new CommandEventHandler(this.NewsContinue);
            aspan.Controls.Add(btn);
          }
          adiv.Controls.Add(aspan);
          acount += MaxItems;
          lab++;
        }
        this.Controls.Add(adiv);
      }
    }

    public void NewsContinue(object src, CommandEventArgs e) {
      int first = 0;
      try { first = Convert.ToInt32(e.CommandArgument); }
      catch { first = 0; }
      FirstItem = first;
      DataSet ds = DB.GetDS("select mainpropid from news where newslanguage='" + Cms.Language + "' and groupid='" + GroupId + "' and showinlist=1 " + DateSnip + " order by newsdate desc");
      NewsId = DB.GetInt(ds, first, "mainpropid");
      CurrMainPropId = NewsId;
      this.Controls.Clear();
      CreateChildControls();
    }

    protected override void OnPreRender(EventArgs e) {
      base.OnPreRender(e);
      if (Selectable) {
        Control cont = NFN.Util.FindInPage(Page, "NewsContent");
        if (cont != null && cont.GetType() == typeof(AjaxControl))
          ((AjaxControl)cont).PropId = CurrMainPropId;
      }
    }

  }



  /********************************************************************************
  *
  * Class PermissionControl
  *
  *********************************************************************************/

  /// <summary>Control for setting permissions</summary>
  public class PermissionControl : Control, INamingContainer {

    private INFNPermission _parent;
    private Panel[] _ppanels;

    /// <summary></summary>
    public PermissionControl(INFNPermission parent)
      : base() {
      this._parent = parent;
    }

    /// <summary>DropDown containing roles</summary>
    protected DropDownList _rolelist;

    /// <summary>Main interface to the web pages of the site and their properties</summary>
    protected CMS Cms {
      get { return (CMS)Page.Session["CMS"]; }
    }

    /// <summary></summary>
    public void Save() {
      foreach (String aRole in Cms.Roles) {
        foreach (String aAction in _parent.Actions) {
          String[] help = aAction.Split(';');
          RadioButtonList plist = (RadioButtonList)this.FindControl(aRole + "-" + help[0]);
          if (plist != null)
            _parent.SetRolePermission(aRole, help[0], plist.SelectedValue == "Y");
        }
      }
    }

    /// <summary></summary>
    protected override void CreateChildControls() {

      _rolelist = new DropDownList();
      _rolelist.Attributes.Add("style", "font: xx-small Verdana;");

      _ppanels = new Panel[Cms.Roles.Count];
      int count = 0;
      foreach (String aRole in Cms.Roles) {
        _rolelist.Items.Add(aRole);

        _ppanels[count] = new Panel();
        _ppanels[count].ID = "P" + count.ToString();
        if (count == 0) _ppanels[count].Attributes.Add("style", "display:block");
        else _ppanels[count].Attributes.Add("style", "display:none");

        _ppanels[count].Controls.Add(new LiteralControl("<table cellpadding='0' cellspacing='3'><tr><td><h3>" + Cms.Translate("Egenskap") + "</h3></td><td><h3>" + Cms.Translate("Behörighet") + "</h3></td></tr>"));
        foreach (String aAction in _parent.Actions) {
          String[] help = aAction.Split(';');
          String aPermission = (_parent.GetRolePermission(aRole, help[0]) ? "Y" : "N");
          _ppanels[count].Controls.Add(new LiteralControl("<tr><td>" + help[1] + ": </td><td>"));
          RadioButtonList plist = new RadioButtonList();
          plist.ID = aRole + "-" + help[0];
          plist.RepeatDirection = RepeatDirection.Horizontal;
          plist.Items.Add(new ListItem(Cms.Translate("Ja"), "Y"));
          plist.Items.Add(new ListItem(Cms.Translate("Nej"), "N"));
          plist.SelectedValue = aPermission;
          _ppanels[count].Controls.Add(plist);
          _ppanels[count].Controls.Add(new LiteralControl("</td></tr>"));
        }
        _ppanels[count].Controls.Add(new LiteralControl("</table>"));
        this.Controls.Add(_ppanels[count]);
        count++;
      }
      this.Controls.Add(_rolelist);
    }

    /// <summary></summary>
    /// <param name="e"></param>
    protected override void OnPreRender(EventArgs e) {

      String ppanelids = "";
      for (int i = 0; i < _ppanels.Length; i++) {
        if (i > 0) ppanelids += ",";
        ppanelids += _ppanels[i].ClientID;
      }
      _rolelist.Attributes.Add("onChange", "__showPermissions(this.selectedIndex, '" + ppanelids + "')");

      base.OnPreRender(e);
    }


    /// <summary></summary>
    /// <param name="output"></param>
    protected override void Render(HtmlTextWriter output) {
      output.Write("<h3>Roll</h3><div>");
      _rolelist.RenderControl(output);
      output.Write("</div><div style='margin-top:20px;overflow:auto'>");
      for (int i = 0; i < _ppanels.Length; i++)
        _ppanels[i].RenderControl(output);
      output.Write("</div>");
    }

  }


  /********************************************************************************
  *
  * Class BWLabel
  *
  *********************************************************************************/

  /// <summary></summary>
  public class BWLabel : Label {

    protected CMS Cms {
      get { return (CMS)Page.Session["CMS"]; }
    }

    protected override void Render(HtmlTextWriter output) {
      this.Text = Cms.Translate(this.Text);
      base.Render(output);
    }
  }



  /********************************************************************************
  *
  * Class AjaxBase
  *
  *********************************************************************************/

  public abstract class AjaxBase : Control, INamingContainer {

    private String propname = "";
    private int propid = 0;
    protected PageProperty mainprop = null;

    protected CMS Cms {
      get { return (CMS)Page.Session["CMS"]; }
    }

    protected BasePage NFNPage {
      get { return (BasePage)Page; }
    }

    public virtual String PropName {
      get { return propname; }
      set { propname = value; }
    }

    public int PropId {
      get { return propid; }
      set { propid = value; }
    }
    
    public virtual PageProperty MainProp {
      get {
        if (mainprop == null) {
          if (PropId != 0) 
            mainprop = Cms.GetPropertyById(PropId);
          else {
            String propName = (PropName.Length > 0 ? PropName : (ID.Length > 0 ? ID : "empty"));
            mainprop = NFNPage.ThisPage.GetControlProperty(propName);
          }
        }
        mainprop.ContainingPage = NFNPage;
        return mainprop;
      }
    }

  }

  /********************************************************************************
  *
  * Class AjaxControl
  *
  *********************************************************************************/

  public class AjaxControl : AjaxBase {
  
    private String controltype = "";
    
    public String ControlType {
      get { return controltype; }
      set { controltype = value; }
    }

    public String Value {
      get { return MainProp.ControlTypeProperty.Value; }
      set { MainProp.ControlTypeProperty.Value = value; }
    }
    
    public String PrelimValue {
      get { return MainProp.ControlTypeProperty.PrelimValue; }
      set { MainProp.ControlTypeProperty.PrelimValue = value; }
    }
    
    protected override void CreateChildControls() {
      base.CreateChildControls();
      
      if (MainProp.ControlType.Length > 0)
        ControlType = MainProp.ControlType;
      else {
        MainProp.ControlType = ControlType;
        DB.ExecSql("update pageproperty set controltype='" + ControlType + "' where id=" + MainProp.Id);
      }

      String divid = (ID.Length > 0 ? ID : MainProp.PropertyName);
      String spropid = (PropId != 0 ? " propid='" + PropId + "' " : "");
      String height = (MainProp.ControlTypeProperty.Show || MainProp.EditPermission ? GetDimensions() : "");
      String html = "<div id='" + divid + "' class='" + ClassName + "'" + spropid + height + ">";
      if (MainProp.ControlTypeProperty.Show || MainProp.EditPermission) html += GetHtml();
      html += "</div>";

      this.Controls.Add(new LiteralControl(html));
    }
    
    private String ClassName {
      get {
        if (ControlType == "tiny") return "ajaxControl ajaxTiny";
        else if (ControlType == "text") return "ajaxControl ajaxText";
        else if (ControlType == "image") return "ajaxControl ajaxImage";
        else if (ControlType == "images") return "ajaxControl ajaxImages";
        else if (ControlType == "flash") return "ajaxControl ajaxFlash";
        else if (ControlType == "movie") return "ajaxControl ajaxMovie";
        else return "ajaxControl ajaxTiny";
      }
    }
    
    private String GetDimensions() {
      int maxh = 0;
      int maxw = 0;
      if (ControlType == "images") {
        String ctrlval = MainProp.ShownControlValue;
        if (ctrlval.Length > 0) {
          String[] images = ctrlval.Split(';');
          for (int i=0; i < images.Length; i++) {
            String[] vals = images[i].Split('|');
            int awidth = 0;
            try { awidth = (vals.Length > 7 ? Convert.ToInt32(vals[7]) : 0); }
            catch { awidth = 0; }
            if (awidth > maxw) maxw = awidth;
            int aheight = 0;
            try { aheight = (vals.Length > 8 ? Convert.ToInt32(vals[8]) : 0); }
            catch { aheight = 0; }
            if (aheight > maxh) maxh = aheight;
          }
        }
      }
      if (maxh > 0) return " style='width:" + maxw + "px;height:" + maxh + "px;' ";
      else return "";
    }

    private String GetHtml() {
      String html = "";
      if (ControlType == "tiny" || ControlType.Length == 0) {
        html = MainProp.ShownControlValue;
      }
      else if (ControlType == "images") {
        String ctrlval = MainProp.ShownControlValue;
        if (ctrlval.Length > 0) {
          html += "<div class='ajaximagesimages'>";
          String dhtml = "<div style='display:none' class='ajaximagesdata'>";
          String[] images = ctrlval.Split(';');
          for (int i=0; i < images.Length; i++) {
            String[] vals = images[i].Split('|');
            String src = vals[0];
            String alt = (vals.Length > 1 ? vals[1] : "");
            String href = (vals.Length > 2 ? vals[2] : "");
            String target = (vals.Length > 3 ? vals[3] : "_self");
            String effect = (vals.Length > 4 ? vals[4] : "fade");
            String timeout = (vals.Length > 5 ? vals[5] : "4000");
            String type = (vals.Length > 6 ? vals[6] : "href");
            String width = (vals.Length > 7 ? vals[7] : "");
            String height = (vals.Length > 8 ? vals[8] : "");

            bool linked = href.Length > 0;
            String hide = (i > 0 ? " style='display:none'" : "");
            if (linked) {
              if (type == "js")
                html += "<a href='javascript:void(0)' onclick=\"" + href + "\" onfocus='this.blur()'" + hide + ">";
              else
                html += "<a href='" + href + "' target='" + target + "' onfocus='this.blur()'" + hide + ">";
            }
            html += "<img class='ajaxImagesImage' src='" + src + "' alt='" + alt + "' " + (width.Length > 0 ? "width='" + width : "") + "' " + (height.Length > 0 ? "height='" + height : "") + "' border='0'" + (!linked ? hide : "") + " />";
            if (href.Length > 0) html += "</a>";
            
            dhtml += "<span>" + effect + "|" + timeout + "</span>"; 
          }
          html += "</div>" + dhtml + "</div>";;
        }
      }
      else if (ControlType == "text") {
        String[] vals = MainProp.ShownControlValue.Split('|');
        String txt = vals[0];
        String href = (vals.Length > 1 ? vals[1] : "");
        String target = (vals.Length > 2 ? vals[2] : "_self");
        String type = (vals.Length > 3 ? vals[3] : "href");

        if (href.Length > 0) {
          if (type == "js")
            html += "<a href='javascript:void(0)' onclick=\"" + href + "\" onfocus='this.blur()'>";
          else
            html += "<a href='" + href + "' target='" + target + "' onfocus='this.blur()'>";
        }
        html += txt;
        if (href.Length > 0) html += "</a>";
      }
      else if (ControlType == "flash") {
        html = "<div style='display:none' class='nfnflashparams'>" + MainProp.ShownControlValue + "</div>";
      }
      else if (ControlType == "movie") {
        html = "<div style='display:none' class='nfnmovieparams'>" + MainProp.ShownControlValue + "</div>";
      }
      else {
        html = MainProp.ShownControlValue;
      }
      return html;
    }

  }




  /********************************************************************************
  *
  * Class MoveableControl
  *
  *********************************************************************************/

  public class MoveableControl : AjaxBase {
  
    bool isrelative = false;
    
    protected virtual int Left {
      get { 
        int left = 0;
        try { left = Convert.ToInt32(MainProp.GetProperty("left").Value); }
        catch { left = 0; }
        return left;
      }
    }

    protected virtual int Top {
      get { 
        int top = 0;
        try { top = Convert.ToInt32(MainProp.GetProperty("top").Value); }
        catch { top = 0; }
        return top;
      }
    }

    public bool IsRelative {
      get { return isrelative; }
      set { isrelative = value; }
    }

    protected override void CreateChildControls() {
      base.CreateChildControls();
      
      HtmlGenericControl maindiv = new HtmlGenericControl("div");
      maindiv.ID = "move_" + MainProp.Id;
      maindiv.Style["position"] = "absolute";
      maindiv.Style["left"] = Left + "px";
      maindiv.Style["top"] = Top + "px";
      maindiv.Attributes.Add("class", "moveableControl");
      for (int i=0; i < this.Controls.Count; i++) {
        Control ctrl = this.Controls[i];
        this.Controls.Remove(ctrl);
        maindiv.Controls.Add(ctrl);
      }
      this.Controls.Add(maindiv);
      if (Cms.User.IsAdmin) {
        String script = "<script type='text/javascript'>_initMoveable('{0}',{1}, {2});</script>";     
        LiteralControl js = new LiteralControl(String.Format(script, maindiv.ClientID, MainProp.Id.ToString(),(IsRelative ? "true" : "false")).Replace("]","}").Replace("[","{"));
        this.Controls.Add(js);
      }
    }
  }


  /********************************************************************************
  *
  * Class MainControls
  *
  *********************************************************************************/

  public class MainControls : MoveableControl {

    private WebPage ThisPage {
      get { return ((BasePage)Page).ThisPage; }
    }

    public override String PropName {
      get { return "MainControls" + Cms.User.Id; }
    }
    
    private String T(String txt) {
      return Cms.Translate(txt);
    }

    protected override int Left {
      get { 
        int left = base.Left;
        if (left < 0) left = 0;
        return left;
      }
    }

    protected override int Top {
      get { 
        int top = base.Top;
        if (top < 0) top = 0;
        return top;
      }
    }

    protected override void CreateChildControls() {
      if (Cms.User.IsAdmin && Cms.GetPermission("DummyId", "MainControl", "DEFAULT")) { 
        String chtml = "<div id='maincontrols' class='maincontrols_dim' onmouseover='$(this).addClass(\"maincontrols_highlight\")' onmouseout='$(this).removeClass(\"maincontrols_highlight\")'>" + 
          "<table cellspacing='0' cellpadding='0'><tr><td>{0}&nbsp;{1}</td><td width='10'></td><td>{7}</td></tr>" + 
          "<tr><td colspan='3'><table cellspacing='0' cellpadding='0'><tr>";
        if (Cms.GetPermission(ThisPage.Id, "WebPage", "Edit")) 
          chtml += "<td><a onfocus='this.blur()' href='javascript:_showEditPage()'><img title='{11}' alt='{11}' src='admin/gfx/edit.gif' /></a></td>";

        bool templatesavail = DB.RowExists("select * from webpagetemplate where category='" + ThisPage.TemplateCategory + "'");
        if (ThisPage.Parent != null && ThisPage.Parent.AllowQuickChildren && Cms.GetPermission(ThisPage.Id, "WebPage", "Create") && templatesavail) {
            chtml += "<td><a onfocus='this.blur()' href='javascript:_showCreatePage()'><img title='{9}' alt='{9}' src='admin/gfx/new.gif' /></a></td>" +
            "<td><a onfocus='this.blur()' href='javascript:_deletePage()'><img title='{10}' alt='{10}' src='admin/gfx/delete.gif' /></a></td>";
        }
        if (Cms.GetBackWebPages().Count > 0)
          chtml += "<td><a onfocus='this.blur()' href='{8}' title='{2}'><img alt='{2}' src='admin/gfx/backweb.gif' title='{2}' /></a></td>";
        if (Cms.GetPermission(ThisPage.Id, "WebPage", "Edit")) 
          chtml += "<td><a onfocus='this.blur()' href='javascript:showEditControls()'><img title='{3}' alt='{3}' src='admin/gfx/showedit.gif' /></a></td>";
        if (Cms.GetPermission("DummyId", "DocumentBank", "DEFAULT", ""))
          chtml += "<td><a onfocus='this.blur()' onclick=\"window.open('/admin/DocumentBank/DocumentBank.aspx','_documentBank','left=0,top=0,width=1000,height=700,resizable=yes')\" href='javascript:void(0)'><img title='{4}' alt='{4}' src='admin/gfx/documentbank.gif'/></a></td>";
        chtml += "<td><a href='javascript:_showUserInfo()' onfocus='this.blur()'><img title='{5}' alt='{5}' src='admin/gfx/key.gif'/></a></td>" +
          "<td><a href='javascript:void(0)' onfocus='this.blur()' onclick='NFN.BaseMaster.Logout();window.location.reload();' title='{6}'><img src='admin/gfx/logout.gif' alt='{6}' /></a></td></tr></table></tr></table></div>";

        String bwurl = "";
        if (Page.Session["CurrentBW"] != null && Page.Session["CurrentBW"].ToString().Length > 0)
          bwurl = Page.Session["CurrentBW"].ToString();
        else if (Cms.GetBackWebPages().Count > 0) {
          Hashtable page = (Hashtable)Cms.GetBackWebPages()[0];
          bwurl = page["URL"].ToString();
        }
        DB.ExecSql("delete from sessions where lastactiontime is null or lastactiontime < '" + DateTime.Now.AddMinutes(-10).ToString(CMS.SiteSetting("dateTimeFormat")) + "'");
        int visitors = DB.GetInt("select count(*) as nof from sessions", "nof");
        
        String visstr = (Cms.GetPermission("DummyId", "ShowUsers", "DEFAULT") ? visitors.ToString() + "&nbsp;" + T("besökare") : "");
        chtml = String.Format(chtml, T("Inloggad som").Replace(" ","&nbsp;"), Cms.User.UserName, T("Gå till administrationssidorna"), T("Visa redigeringskontroller"), T("Öppna mediabank"), T("Uppdatera användaruppgifter"), T("Logga ut som administratör"), visstr, bwurl, T("Skapa ny webbsida"), T("Ta bort webbsida"), T("Redigera aktuell webbsida"));

        if (Cms.GetPermission(ThisPage.Id, "WebPage", "Edit")) 
          chtml += GetEditPageHtml();

        if (ThisPage.Parent != null && ThisPage.Parent.AllowQuickChildren && Cms.GetPermission(ThisPage.Id, "WebPage", "Create")) 
          chtml += GetCreatePageHtml();
          
        chtml += GetUserInfoHtml();

        this.Controls.Add(new LiteralControl(chtml));
      }

      base.CreateChildControls();
    }
    
    private String GetUserInfoHtml() {
      Cms.User.Refresh();
      String html = "<div id='userinfoform' class='pageform'><h2>{0}</h2><div id='pu_err' class='p_err'></div><table>" +
        "<tr><td>{1}</td><td><span id='span_UserName' style='font-weight:bold'>{2}</span></td></tr>" + 
        "<tr><td>{3}</td><td><input id='inp_PWOld' style='width:200px' type='password' /></td></tr>" + 
        "<tr><td>{4}</td><td><input id='inp_PWNew1' style='width:200px' type='password' /></td></tr>" + 
        "<tr><td>{5}</td><td><input id='inp_PWNew2' style='width:200px' type='password' /></td></tr>" + 
        "<tr><td>{6}</td><td><input id='inp_UEmail' style='width:200px' type='text' value='{7}' /></td></tr>";
        
      for (int i=0; i < Cms.User.AttribFields.Length; i++) {
        String aname = Cms.User.AttribFields[i][1];
        String adisplayname = T(Cms.User.AttribFields[i][2]);
        String mandatory = Cms.User.AttribFields[i][3];
        String regexp = Cms.User.AttribFields[i][4];
        String value = Cms.User.GetAttribValue(aname);

        html += "<tr><td>" + adisplayname + "</td><td><input id='ua_" + aname + "' style='width:200px' type='text' value='" + value + "' name='ua_field' /><span style='display:none'>" + mandatory + "</span><span style='display:none'>" + regexp + "</span></td></tr>";
      }
      html += "</table><div class='pfbtn'><a href='javascript:void(0)' onclick='_saveUserInfo()' onfocus='this.blur()'><img src='/admin/gfx/save.gif' alt='{8}' border='0' /></a>&nbsp;&nbsp;&nbsp;&nbsp;<a href='javascript:void(0)' onclick='_cancelUserInfo()' onfocus='this.blur()'><img src='/admin/gfx/cancel.gif' alt='{9}' border='0' /></a></div></div>";
      html = String.Format(html, T("Användaruppgifter"), T("Användarnamn"), Cms.User.UserName, T("Gammalt lösenord"), T("Nytt lösenord"), T("Repetera nytt lösenord"), T("E-postadress"), Cms.User.Email, T("Spara"), T("Avbryt"));
      return html;
    }

    private String GetCreatePageHtml() {
      String thtml = "";
      DataSet dstmpl = DB.GetDS("select * from webpagetemplate where category='" + ThisPage.TemplateCategory + "' order by orderno");
      if (DB.GetRowCount(dstmpl) > 1) {
        thtml = "<select id='sel_pageTemplate'>";
        for (int i=0; i < DB.GetRowCount(dstmpl); i++)
          thtml += "<option value='" + DB.GetString(dstmpl, i, "filename") + "' " + (DB.GetString(dstmpl, i, "filename") == ThisPage.Filename ? "selected" : "") + ">" + DB.GetString(dstmpl, i, "name") + "</option>";
        thtml += "</select>";
      }
      else
        thtml = "<input id='sel_pageTemplate' type='hidden' value='" + ThisPage.Filename + "' />";
        
      String mhtml = "<select id='sel_pageOrder'>";
      int count = DB.GetInt("select count(id) as nof from webpage where deleted=0 and languages like '%" + Cms.Language + "%' and parentpage='" + ThisPage.Parent.Id + "'", "nof") + 1;
      for (int i=1; i <= count; i++)
        mhtml += "<option value='" + i + "'>" + i + "</option>";
      mhtml += "</select>";

      String html = "<div id='createpageform' class='pageform'><h2>" + T("Skapa ny sida") + "</h2><div id='pc_err' class='p_err'></div><table>" +
        "<tr><td>" + T("Identifierare") + "</td><td><input id='inp_PageId' style='width:160px' type='text' disabled value='" + ThisPage.Parent.GetNextSubpageId() + "'/><input id='hd_PageId' type='hidden' value='" + ThisPage.Parent.GetNextSubpageId() + "'/><input type='checkbox' id='chk_PageId' checked title='" + T("Autogenerera") + "' onclick='chkAutoIdClicked(this)' /></td></tr>" + 
        "<tr><td>" + T("Menytext") + "</td><td><input id='inp_PageName' style='width:200px' type='text' /></td></tr>" + 
        "<tr><td>" + T("Titel") + "</td><td><input id='inp_PageTitle' style='width:200px' type='text' /></td></tr>" + 
        "<tr><td>" + T("Aktiv") + "</td><td><input id='inp_PageActive' type='checkbox' /></td></tr>" + 
        "<tr><td>" + T("Tidsstyrd") + "</td><td><input id='inp_PageTimeControlled' type='checkbox' onclick='togglePageTimecontrol(this)' /></td></tr>" +
        "<tr style='display:none'><td>" + T("Startdatum") + "</td><td><input id='inp_PageStartDate' type='hidden' value='" + DateTime.Now.ToString("yyyy-MM-dd") + "' /><a href='javascript:void(0)' onclick='selPageDate(this)'>" + DateTime.Now.ToString("yyyy-MM-dd") + "</a></td></tr>" +
        "<tr style='display:none'><td>" + T("Slutdatum") + "</td><td><input id='inp_PageEndDate' type='hidden' value='" + DateTime.Now.ToString("yyyy-MM-dd") + "' /><a href='javascript:void(0)' onclick='selPageDate(this)'>" + DateTime.Now.ToString("yyyy-MM-dd") + "</a></td></tr>" +
        "<tr><td>" + (DB.GetRowCount(dstmpl) > 1 ? T("Sidmall") : "") + "</td><td>" + thtml + "</td></tr>" + 
        "<tr><td>" + T("Menyplats") + "</td><td>" + mhtml + "</td></tr>";

        if (DB.GetRowCount(dstmpl) == 1) {
          String[][] pageAttributes = ThisPage.GetPageAttributes();
          for (int i = 0; i < pageAttributes.Length; i++) {
            String lang = pageAttributes[i][4];
            if (lang.Length == 0 || lang.Contains(Cms.Language)) {
              String id = pageAttributes[i][0];
              String name = Cms.Translate(pageAttributes[i][1]);
              String type = pageAttributes[i][2];
              String alt = pageAttributes[i][3];
              String inp = "";
              if (type == "bool")
                inp = "<input type='checkbox' id='attrib_" + id + "' />";
              else if (type == "datetime") {
                String now = DateTime.Now.ToString("yyyy-MM-dd");
                inp = "<input type='hidden' id='attrib_" + id + "' name='" + type + "' value='" + now + "' /><a href='javascript:void(0)' onclick='selPageDate(this)'>" + now + "</a>";
              }
              else if (alt.Length > 0) {
                String[] altarr = alt.Split(';');
                inp = "<select id='attrib_" + id + "'>";
                for (int j = 0; j < altarr.Length; j++)
                  inp += "<option value='" + altarr[j] + "'>" + altarr[j] + "</option>";
                inp += "</select>";
              }
              else
                inp = "<input type='text' id='attrib_" + id + "' name='" + type + "' />";
              html += "<tr><td>" + name + ":</td><td>" + inp + "</td></tr>";
            }
          }
        }

        html += "</table><div class='pfbtn'><a href='javascript:void(0)' onclick='_saveCreatePage()' onfocus='this.blur()'><img src='/admin/gfx/save.gif' alt='" + T("Spara") + "' border='0' /></a>&nbsp;&nbsp;&nbsp;&nbsp;<a href='javascript:void(0)' onclick='_cancelCreatePage()' onfocus='this.blur()'><img src='/admin/gfx/cancel.gif' alt='" + T("Avbryt") + "' border='0' /></a></div></div>";
        
      return html;
    }

    private String GetEditPageHtml() {
      String html = "<div id='editpageform' class='pageform'><h2>" + T("Redigera sidattribut") + "</h2><div id='pa_err' class='p_err'></div><table class='pftable'>" +
        "<tr><td>" + T("Menytext") + "</td><td><input id='inpPageName' style='width:200px' type='text' value='" + Cms.Translate(ThisPage.Name) + "' /></td></tr>" + 
        "<tr><td>" + T("Titel") + "</td><td><input id='inpPageTitle' style='width:200px' type='text' value='" + Cms.Translate(ThisPage.Title) + "' /></td></tr>" + 
        "<tr><td>" + T("Aktiv") + "</td><td><input id='inpPageActive' type='checkbox' " + (ThisPage.Status == "active" ? "checked" : "") + " /></td></tr>" +
        "<tr><td>" + T("Tidsstyrd") + "</td><td><input id='inpPageTimeControlled' type='checkbox' " + (ThisPage.TimeControlled ? "checked" : "") + " onclick='togglePageTimecontrol(this)' /></td></tr>" +
        "<tr style='display:" + (ThisPage.TimeControlled ? "" : "none") + "'><td>" + T("Startdatum") + "</td><td><input id='inpPageStartDate' type='hidden' value='" + ThisPage.StartTime.ToString("yyyy-MM-dd") + "' /><a href='javascript:void(0)' onclick='selPageDate(this)'>" + ThisPage.StartTime.ToString("yyyy-MM-dd") + "</a></td></tr>" +
        "<tr style='display:" + (ThisPage.TimeControlled ? "" : "none") + "'><td>" + T("Slutdatum") + "</td><td><input id='inpPageEndDate' type='hidden' value='" + ThisPage.EndTime.ToString("yyyy-MM-dd") + "' /><a href='javascript:void(0)' onclick='selPageDate(this)'>" + ThisPage.EndTime.ToString("yyyy-MM-dd") + "</a></td></tr>";

      DataSet ds = DB.GetDS("select w1.filename, w1.name from webpagetemplate w1, webpagetemplate w2 where w1.category=w2.category and w2.filename='" + ThisPage.Filename + "' order by w1.orderno");
      if (DB.GetRowCount(ds) > 1) {
        String thtml = "<select id='selPageTemplate'>";
        for (int i=0; i < DB.GetRowCount(ds); i++)
          thtml += "<option " + (DB.GetString(ds, i, "filename") == ThisPage.Filename ? "selected " : "") + "value='" + DB.GetString(ds, i, "filename") + "'>" + DB.GetString(ds, i, "name") + "</option>";
        thtml += "</select>";
        if (ThisPage.PrelimFilename.Length > 0 && ThisPage.PublishedFilename != ThisPage.PrelimFilename)
          thtml += "<a href='javascript:void(0)' id='_publishBtn' onclick='publishTemplate(this)' onfocus='this.blur()'><img src='admin/gfx/publish.gif' alt='" + T("Publicera sidmall") + "' title='" + T("Publicera sidmall") + "' border='0' /></a>";
        thtml += "<input type='hidden' id='_publishTemplate' value='n' />";
        html += "<tr><td>" + T("Sidmall") + "</td><td>" + thtml + "</td></tr>";
      }

      if (ThisPage.Parent != null) {
        String mhtml = "<select id='selpageOrder'>";
        String snip = (ThisPage.Parent == null ? "is null" : "='" + ThisPage.Parent.Id + "'");
        int count = DB.GetInt("select count(id) as nof from webpage where deleted=0 and pagestatus='active' and languages like '%" + Cms.Language + "%' and parentpage " + snip, "nof");
        int thisorder = ThisPage.GetChildOrder();
        if (count == 0)
          mhtml += "<option value='1' selected>1</option>";
        else {
          for (int i=1; i <= count; i++)
            mhtml += "<option value='" + i + "' " + (i == thisorder ? "selected" : "") + ">" + i + "</option>";
        }
        mhtml += "</select>";
        html += "<tr><td>" + T("Menyplats") + "</td><td>" + mhtml + "</td></tr>";
      }
        
      String[][] pageAttributes = ThisPage.GetPageAttributes();
      for (int i = 0; i < pageAttributes.Length; i++) {
        String lang = pageAttributes[i][4];
        if (lang.Length == 0 || lang.Contains(Cms.Language)) {
          String id = pageAttributes[i][0];
          String name = Cms.Translate(pageAttributes[i][1]);
          String type = pageAttributes[i][2];
          String alt = pageAttributes[i][3];
          String val = ThisPage.PageAttributeValues[id].ToString();
          if (type == "bool") val = (val == "true" ? "checked" : "");
          String inp = "";
          if (type == "bool")
            inp = "<input type='checkbox' id='attrib_" + id + "' " + val + " />";
          else if (type == "datetime") {
            if (val.Length == 0) val = DateTime.Now.ToString("yyyy-MM-dd");
            inp = "<input type='hidden' id='attrib_" + id + "' name='" + type + "' value='" + val + "' /><a href='javascript:void(0)' onclick='selPageDate(this)'>" + val + "</a>";
          }
          else if (alt.Length > 0) {
            String[] altarr = alt.Split(';');
            inp = "<select id='attrib_" + id + "'>";
            for (int j = 0; j < altarr.Length; j++)
              inp += "<option " + (altarr[j] == val ? "selected " : "") + "value='" + altarr[j] + "'>" + altarr[j] + "</option>";
            inp += "</select>";
          }
          else
            inp = "<input type='text' id='attrib_" + id + "' name='" + type + "' value='" + val + "' />";
          html += "<tr><td>" + name + ":</td><td>" + inp + "</td></tr>";
        }
      }
      html += "</table>";
      html += "<div><a href='javascript:void(0)' onclick='toggleRelatedPages()' onfocus='this.blur()'>" + Cms.Translate("Relaterade sidor") + "</a></div>";
      html += "<div id='relatedpagesdiv' style='display:none'>" + ThisPage.GetRelatedPagesTree() + "</div>";
      html += "<div><a href='javascript:void(0)' onclick='toggleAllPages()' onfocus='this.blur()'>" + Cms.Translate("Sajtkarta") + "</a></div>";
      html += "<div id='allpagesdiv' style='display:none'>" + Cms.GetPagesTree() + "</div>";

      html += "<div class='pfbtn'><a href='javascript:void(0)' onclick='_saveEditPage()' onfocus='this.blur()'><img src='/admin/gfx/save.gif' alt='" + T("Spara") + "' border='0' /></a>&nbsp;&nbsp;&nbsp;&nbsp;<a href='javascript:void(0)' onclick='_cancelEditPage()' onfocus='this.blur()'><img src='/admin/gfx/cancel.gif' alt='" + T("Avbryt") + "' border='0' /></a></div></div>";
      return html;
    }
    

    protected void LogOutClick(Object sender, ImageClickEventArgs e) {
      Cms.LogOut(Page.Session.SessionID);
      Page.Response.Redirect("~/");
    }


    protected void RefreshClick(Object sender, ImageClickEventArgs e) {
      Cms.Refresh();
    }
  }



}
