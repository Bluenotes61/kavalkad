/* $Date: 2010-11-26 13:47:31 +0100 (fr, 26 nov 2010) $    $Revision: 7114 $ */
using System;
using System.Web;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NFN {

  public class PageProperty {

    private int id;
    private String _controltype = "";
    private String _value;
    private String _prelimvalue;
    private bool _isvisible;
    private bool _ispublished;
    private bool _valchanged;
    private bool _pvalchanged;
    private bool _languagedependent = true;
    private int _orderno;
    protected CMS Cms;
    private SortedList childprops;
    private DateTime moddate;
    private int modby;
    private DateTime publisheddate = DateTime.Now;
    private int publishedby;
    private String _propertyname;
    private PageProperty _parent;
    private DateTime _startdate;
    private DateTime _enddate;
    private BasePage containingpage;


    /// <summary>Unique id of the page property</summary>
    public int Id {
      get { return id; }
    }


    /// <summary>Name of the page property</summary>
    public String PropertyName {
      get { return _propertyname; }
      set { _propertyname = value; }
    }


    /// <summary>Type of BWControl to which the PageProperty belongs</summary>
    public String ControlType {
      get { return _controltype; }
      set { _controltype = value; }
    }


    public bool LanguageDependent {
      get { return _languagedependent; }
      set { _languagedependent = value; }
    }

    /// <summary>True if the BWControl to which the PageProperty belongs should be visible</summary>
    public bool IsVisible {
      get { return _isvisible; }
      set { _isvisible = value; }
    }

    public bool IsInDateSpan {
      get { return ((StartDate == DateTime.MinValue || EndDate == DateTime.MinValue) || (DateTime.Now >= StartDate && DateTime.Now < EndDate.AddDays(1))); }
    }

    public bool Show {
      get { return IsVisible && IsInDateSpan; }
    }

    /// <summary>First date at which the BWControl to which the PageProperty belongs should be visible</summary>
    public DateTime StartDate {
      get { return _startdate; }
      set { _startdate = value; }
    }


    /// <summary>Last date at which the BWControl to which the PageProperty belongs should be visible</summary>
    public DateTime EndDate {
      get { return _enddate; }
      set { _enddate = value; }
    }


    /// <summary>True if changes to the PageProperty are published</summary>
    public bool IsPublished {
      get { return _ispublished; }
      set { _ispublished = value; }
    }

    public bool IsCommon {
      get { return Parent != null && Cms.CommonProperty.Id == Parent.Id; }
    }

    /// <summary>Parent PageProperty of the current PageProperty</summary>
    public PageProperty Parent {
      get { return _parent; }
      set {
        if (value != null && _parent.Id != ((PageProperty)value).Id) {
          _parent = value;
          DB.ExecSql("update pageproperty set parentproperty=" + _parent.Id + " where id=" + Id);
        }
      }
    }


    ///<summary>Id of the <see cref="LoggedInUser">LoggedInUser</see> who last modified the value of the PageProperty</summary>
    public int ModBy {
      get { return modby; }
    }


    ///<summary>Date for the last modification of the PageProperty</summary>
    public DateTime ModDate {
      get { return moddate; }
    }

    public DateTime LastModDate {
      get { return GetLastModDate(Id); }
    }

    public static DateTime GetLastModDate(int propid) {
      String sql = "select * from pageproperty where id={0} or parentproperty={0} order by moddate desc";
      DataSet ds = DB.GetDS(String.Format(sql, propid.ToString()));
      if (DB.GetRowCount(ds) > 0) return DB.GetDate(ds, 0, "lastdate");
      else return DateTime.MinValue;
    }
   
    ///<summary>Id of the <see cref="LoggedInUser">LoggedInUser</see> who last published the changes of the PageProperty</summary>
    public int PublishedBy {
      get { return publishedby; }
    }


    ///<summary>Date for the last publication of the PageProperty</summary>
    public DateTime PublishedDate {
      get { return publisheddate; }
    }


    /// <summary>Initializes a new instance of the PageProperty, class given a DataRow with information.</summary>
    /// <param name="BW"><see cref="CMS"/> class</param>
    /// <param name="row"><see cref="DataRow"/> containing property information</param>
    public PageProperty(CMS Cms, DataRow row) {
      this.Cms = Cms;
      id = Convert.ToInt32(row["id"]);

      ReadData(row);

      String parentid = row["parentproperty"] == System.DBNull.Value ? "0" : row["parentproperty"].ToString();
      if (parentid != "0") {
        DataSet ds = DB.GetDS("select * from pageproperty where deleted <> 1 and Id = " + parentid);
        if (DB.GetRowCount(ds) > 0)
          _parent = new PageProperty(Cms, DB.GetRow(ds, 0));
        else
          _parent = null;
      }
      else
        _parent = null;

      childprops = null;
    }


    /// <summary>Initializes a new instance of the PageProperty, class given a DataRow with information.</summary>
    /// <param name="BW">Parent <see cref="CMS"/> class</param>
    /// <param name="row"><see cref="DataRow"/> containing property information</param>
    /// <param name="aParent">Parent <see cref="PageProperty"/></param>
    public PageProperty(CMS Cms, DataRow row, PageProperty aParent) {
      this.Cms = Cms;
      id = Convert.ToInt32(row["id"]);
      _parent = aParent;

      ReadData(row);

      childprops = null;
    }


    /// <summary>Initializes a new instance of the PageProperty class and stores it to the database.</summary>
    /// <param name="BW">Parent <see cref="CMS"/> class</param>
    /// <param name="aParent">Parent <see cref="PageProperty"/></param>
    /// <param name="propName">Name of the property to create</param>
    /// <param name="propVal">Value of the property to create</param>
    public PageProperty(CMS Cms, PageProperty aParent, String propName, String propVal) {

      this.Cms = Cms;
      propVal = (propVal == null ? "" : propVal);
      _parent = aParent;
      _propertyname = propName;
      _value = propVal;
      _prelimvalue = propVal;

      String sqlStr = "select max(orderno) as maxno from pageproperty where deleted <> 1 and parentproperty{0}";
      sqlStr = String.Format(sqlStr, (aParent == null ? " is null" : "=" + aParent.Id.ToString()));
      int aOrder = DB.GetInt(sqlStr, "maxno") + 1;

      sqlStr = "insert into pageproperty (propertyname, propertyvalue, prelimpropertyvalue, visible, published, parentproperty, moddate, modby, publisheddate, publishedby, orderno) "
        + "values('{0}', '{1}', '{1}', 0, 1, {2}, '{3}', {4}, '{3}', {4}, {5})";
      sqlStr = String.Format(sqlStr, propName, propVal.Replace("'","''"), (aParent == null ? "null" : aParent.Id.ToString()), Util.GetNow(), Cms.User.Id, aOrder.ToString());
      DB.ExecSql(sqlStr);

      _isvisible = true;
      _ispublished = false;
      moddate = DateTime.Now;
      modby = Cms.User.Id;
      _startdate = DateTime.MinValue;
      _enddate = DateTime.MinValue;
      _orderno = aOrder;

      sqlStr = "select id from pageproperty where propertyname = '" + propName + "' order by id desc";
      DataSet ds2 = DB.GetDS(sqlStr);
      DataRow proprow = DB.GetRow(ds2, 0);
      id = Convert.ToInt32(proprow["id"]);

      childprops = null;
      _valchanged = false;
      _pvalchanged = false;
    }


    /// <summary>Refresh the PageProperty information from the database</summary>
    public void Refresh() {
      DataSet ds = DB.GetDS("select * from pageproperty where deleted <> 1 and Id = " + id);
      if (DB.GetRowCount(ds) > 0) {
        ReadData(DB.GetRow(ds, 0));
        childprops = null;
      }
    }


    /// <summary>Read property data from the database</summary>
    /// <param name="row"><see cref="DataRow"/> containing property information</param>
    private void ReadData(DataRow row) {
      _propertyname = row["propertyname"].ToString();
      _controltype = row["controltype"].ToString();
      _value = row["propertyvalue"] == System.DBNull.Value ? "" : row["propertyvalue"].ToString();
      PrelimValue = row["prelimpropertyvalue"] == System.DBNull.Value ? "" : row["prelimpropertyvalue"].ToString();
      _languagedependent = (row["languageindependent"] == System.DBNull.Value || !Convert.ToBoolean(row["languageindependent"]));
      _isvisible = (row["visible"] != System.DBNull.Value && Convert.ToBoolean(row["visible"]));
      _ispublished = (row["published"] != System.DBNull.Value && Convert.ToBoolean(row["published"]));
      _orderno = row["orderno"] == System.DBNull.Value ? 0 : Convert.ToInt32(row["orderno"]);
      try { moddate = DateTime.Parse(row["moddate"].ToString()); }
      catch { moddate = DateTime.MinValue; }
      try { modby = Convert.ToInt32(row["modby"]); }
      catch { modby = 0; }
      try { publisheddate = DateTime.Parse(row["publisheddate"].ToString()); }
      catch { publisheddate = DateTime.MinValue; }
      try { publishedby = Convert.ToInt32(row["publishedby"]); }
      catch { publishedby = 0; }
      try { _startdate = DateTime.Parse(row["startdate"].ToString()); }
      catch { _startdate = DateTime.MinValue; }
      try { _enddate = DateTime.Parse(row["enddate"].ToString()); }
      catch { _enddate = DateTime.MinValue; }
      _valchanged = false;
      _pvalchanged = false;
    }


    /// <summary>Forces th PageProperty to reread its child PageProperties from the database</summary>
    public void RefreshChildProperties() {
      childprops = null;
    }


    /// <summary>List of child PageProperties of the current PageProperty</summary>
    public SortedList ChildProperties {
      get {
        if (childprops == null) {
          childprops = new SortedList();
          ReadChildProps();
        }
        return childprops;
      }
      set { childprops = value; }
    }


    /// <summary>List of child PageProperties of the current PageProperty, sorted by order number</summary>
    public ArrayList ChildPropertiesSorted {
      get {
        ArrayList sorted = new ArrayList();
        foreach (PageProperty prop in ChildProperties.Values) {
          int i = 0;
          while (i < sorted.Count && prop.OrderNo > ((PageProperty)sorted[i]).OrderNo)
            i++;
          sorted.Insert(i, prop);
        }
        return sorted;
      }
    }


    /// <summary>The level of the PageProperty in the PageProperty hierarchy</summary>
    public int PropLevel {
      get {
        int level = 0;
        PageProperty aProp = this;
        while (aProp.Parent != null) {
          aProp = aProp.Parent;
          level++;
        }
        return level;
      }
    }


    /// <summary>The topmost PageProperty, with no parent, in the hierarchy</summary>
    public PageProperty RootProp {
      get {
        PageProperty aProp = this;
        while (aProp.Parent != null) aProp = aProp.Parent;
        return aProp;
      }
    }


    /// <summary>Locks the property for editing to the given session</summary>
    /// <param name="sessionid">Id of session to lock to</param>
    public void Lock(String sessionid) {
      DB.ExecSql("update pageproperty set locked='" + sessionid + "', locktime='" + DateTime.Now.ToString(CMS.SiteSetting("dateTimeFormat")) + "', lockuser=" + Cms.User.Id + " where id=" + Id);
    }


    /// <summary>Unlocks the property for editing</summary>
    /// <param name="sessionid">Id of session to unlock from</param>
    public void Unlock(String sessionid) {
      DB.ExecSql("update pageproperty set locked='', locktime=NULL where locked='" + sessionid + "' and id=" + Id);
    }


    /// <summary>Unlocks the property for editing</summary>
    public void Unlock() {
      DB.ExecSql("update pageproperty set locked='', locktime=NULL where id=" + Id);
    }


    /// <summary>Returns true if the property is locked to another session</summary>
    /// <param name="sessionid">Id of current session</param>
    /// <returns>True if locked to another session</returns>
    public bool IsLocked(String sessionid) {
      String aid = DB.GetString("select locked from pageproperty where deleted <> 1 and id=" + Id, "locked");
      return (aid.Length > 0 && aid != sessionid);
    }


    /// <summary>Returns true if the property is locked by any session</summary>
    public bool IsLocked() {
      String aid = DB.GetString("select locked from pageproperty where deleted <> 1 and id=" + Id, "locked");
      return (aid.Length > 0);
    }

    /// <summary>Returns username of the user that last locked the property</summary>
    public String LockUserName() {
      return DB.GetString("select u.username from pageproperty p, users u where u.id=p.lockuser and p.id=" + Id, "username");
    }

    public DateTime LockTime() {
      return DB.GetDate("select locktime from pageproperty where id=" + Id, "locktime", DateTime.MinValue);
    }

    /// <summary>Reads all child properties of the current property</summary>
    private void ReadChildProps() {
      if (childprops == null)
        childprops = new SortedList();
      childprops.Clear();
      DataSet ds = DB.GetDS("select * from pageproperty where deleted <> 1 and parentproperty = " + Id + " order by orderno, id");
      for (int i = 0; i < DB.GetRowCount(ds); i++) {
        DataRow proprow = DB.GetRow(ds, i);
        String propName = proprow["propertyname"].ToString();
        PageProperty aProp = new PageProperty(Cms, proprow, this);
        if (!childprops.Contains(propName))
          childprops.Add(propName, aProp);
      }
    }


    /// <summary>The priliminary (not published) value of the PageProperty</summary>
    public String PrelimValue {
      get {
        String aval = _prelimvalue;
        if (_ispublished) aval = _value;
        return aval == null ? "" : aval;
      }
      set {
        if (_prelimvalue != value) {
          _prelimvalue = value == null ? "" : value;
          _ispublished = false;
          _pvalchanged = true;
        }
      }
    }


    /// <summary>The published value of the page property</summary>
    public String Value {
      get { return _value == null ? "" : _value; }
      set { 
        if (_value != value) {
          _value = value;
          _valchanged = true;
        }
      }
    }


    public virtual String ShownValue {
      get {
        if (Cms.GetPermission(this.Id.ToString(), "PageProperty", "edit")) return PrelimValue;
        else return Value;
      }
    }


    /// <summary>Returns the current value of the property stripped from any html tags</summary>
    public String HtmlStrippedText {
      get { return Regex.Replace(Value, "<(.|\n)+?>", " ", RegexOptions.IgnoreCase); }
    }


    /// <summary>Sets the priliminary an published values directly</summary>
    public void SetValuesDirectly(String prelimval, String val) {
      if (_prelimvalue != prelimval) {
        _prelimvalue = prelimval;
        _pvalchanged = true;
      }
      if (_value != val) {
        _value = val;
        _valchanged = true;
      }
    }


    /// <summary>Value by which the PageProperty is ordered when relatively other PageProperties</summary>
    public int OrderNo {
      get { return _orderno; }
      set {
        if (_orderno != value) {
          _orderno = value;
          DB.ExecSql("update pageproperty set orderno = " + _orderno + " where id = " + Id);
        }
      }
    }


    /// <summary>Integer representation of the PageProperty value</summary>
    public int IntValue {
      get {
        try { return Convert.ToInt32(Value); }
        catch { return 0; }
      }
    }


    /// <summary>Boolean representation of the PageProperty value</summary>
    public Boolean BoolValue {
      get { return (Value.ToLower() == "true" || Value.ToLower() == "True"); }
    }


    /// <summary>Retrieves a child PageProperty of the current PageProperty. If the child PageProperty does not exist, it is created.</summary>
    public PageProperty GetProperty(String propName) {
      PageProperty aProp = (PageProperty)ChildProperties[propName];
      if (aProp == null) {
        childprops = null;
        aProp = (PageProperty)ChildProperties[propName];
      }
      if (aProp == null) {
        aProp = new PageProperty(Cms, this, propName, "");
        aProp.WriteToDB();
        ChildProperties.Add(propName, aProp);
      }
      return aProp;
    }

    /// <summary>Returns true if the current PageProperty has child PageProperties</summary>
    public bool HasProperty(String propName) {
      return DB.GetInt("select count(*) as nof from pageproperty where parentproperty=" + this.Id + " and propertyname='" + propName + "' and deleted=0", "nof") > 0;
    }


    /// <summary>Retrieves a child page property of the current PageProperty. If a PageProperty with the given id does not exist, null is returned</summary>
    public PageProperty GetPropertyById(String propid) {
      try {
        int iid = Convert.ToInt32(propid);
        return Cms.GetPropertyById(iid);
      }
      catch { return null; }
    }


    /// <summary>Deletes the current PageProperty from the database</summary>
    public void Delete() {
      DeletePriv();
      if (Parent != null)
        Parent.ChildProperties.Remove(this.PropertyName);
    }


    /// <summary>Deletes the current PageProperty from the database</summary>
    private void DeletePriv() {
      IDictionaryEnumerator enumer = ChildProperties.GetEnumerator();
      while (enumer.MoveNext()) {
        PageProperty aProp = (PageProperty)enumer.Value;
        aProp.DeletePriv();
      }
      DB.ExecSql("update pageproperty set deleted=1 where Id=" + Id);
      Cms.LogEvent("deleteprop", Id.ToString());
      ChildProperties.Clear();
    }


    /// <summary>Returns a clone of the current PageProperty</summary>
    public PageProperty Clone(PageProperty aParent) {

      Refresh();
      PageProperty aProp = new PageProperty(Cms, aParent, this.PropertyName, this.Value);
      aProp.PrelimValue = this.PrelimValue;
      aProp.ControlType = this.ControlType;
      aProp.LanguageDependent = this.LanguageDependent;
      aProp.IsVisible = this.IsVisible;
      aProp._ispublished = this._ispublished;
      aProp._orderno = this._orderno;
      aProp.StartDate = this.StartDate;
      aProp.EndDate = this.EndDate;
      aProp.moddate = this.moddate;
      aProp.modby = this.modby;
      aProp.publisheddate = this.publisheddate;
      aProp.publishedby = this.publishedby;

      for (int i = 0; i < this.ChildProperties.Count; i++) {
        PageProperty aChild = (PageProperty)this.ChildProperties.GetByIndex(i);
        aChild.Clone(aProp);
      }
      aProp.WriteToDB();
      return aProp;
    }


    /// <summary>Writes values of the current property and its child properties to the database</summary>
    public void WriteToDB() {
      IDictionaryEnumerator enumer = ChildProperties.GetEnumerator();
      while (enumer.MoveNext()) {
        PageProperty aProp = (PageProperty)enumer.Value;
        aProp.WriteToDB();
      }
      String pubdate = (PublishedDate == DateTime.MinValue ? "null" : "'" + PublishedDate.ToString(CMS.SiteSetting("dateTimeFormat")) + "'");
      String startdate = (StartDate == DateTime.MinValue ? "null" : "'" + StartDate.ToString(CMS.SiteSetting("dateTimeFormat")) + "'");
      String enddate = (EndDate == DateTime.MinValue ? "null" : "'" + EndDate.ToString(CMS.SiteSetting("dateTimeFormat")) + "'");
      String snip = "";
      if (_valchanged) snip += "propertyvalue='" + DB.FixApostrophe(Value) + "',";
      if (_pvalchanged) snip += "prelimpropertyvalue='" + DB.FixApostrophe(PrelimValue) + "',";
      String sql = "update pageproperty set " + snip + "propertyname='" + PropertyName + "', controltype='" + ControlType + "', visible=" + Convert.ToByte(IsVisible) + ", languageindependent=" + Convert.ToByte(!LanguageDependent) + ", published=" + Convert.ToByte(IsPublished) + ", orderno=" + OrderNo + ", moddate='" + DateTime.Now.ToString(CMS.SiteSetting("dateTimeFormat")) + "', modby=" + Cms.User.Id + ", publisheddate=" + pubdate + ", publishedby=" + PublishedBy + ", startdate=" + startdate + ", enddate=" + enddate + " where Id=" + Id;
      DB.ExecSql(sql);
      _valchanged = false;
      _pvalchanged = false;
    }


    public void Move(String dir) {
      int idx = -1;
      for (int i = 0; i < this.Parent.ChildPropertiesSorted.Count && idx == -1; i++)
        if (((PageProperty)this.Parent.ChildPropertiesSorted[i]).Id == this.Id)
          idx = i;
      PageProperty switchprop;
      if (dir == "up") {
        if (idx == 0) return;
        switchprop = (PageProperty)this.Parent.ChildPropertiesSorted[idx - 1];
      }
      else if (dir == "down") {
        if (idx == this.Parent.ChildPropertiesSorted.Count - 1) return;
        switchprop = (PageProperty)this.Parent.ChildPropertiesSorted[idx + 1];
      }
      else return;
      int aNo = switchprop.OrderNo;
      switchprop.OrderNo = this.OrderNo;
      switchprop.WriteToDB();
      this.OrderNo = aNo;
      this.WriteToDB();
    }

    /// <summary>Copies the published value of the PageProperty to the preliminaty value, thus undoing changes.</summary>
    public void RevertToPublished() {
      if (_prelimvalue != _value) {
        _prelimvalue = _value;
        _pvalchanged = true;
      }
      _ispublished = true;
      String snip = "";
      if (_valchanged) snip += "propertyvalue='" + Value.Replace("'", "''") + "',";
      if (_pvalchanged) snip += "prelimpropertyvalue='" + PrelimValue.Replace("'", "''") + "',";
      DB.ExecSql("update pageproperty set " + snip + "published=" + Convert.ToByte(IsPublished) + " where Id=" + Id);
      foreach (DictionaryEntry item in this.ChildProperties)
        ((PageProperty)item.Value).RevertToPublished();
    }

    /// <summary>Copies the preliminary value of the PageProperty to the published value.</summary>
    public void Publish(bool updatedb) {
      PageProperty[] cprops = new PageProperty[this.ChildProperties.Count];
      for (int i = 0; i < this.ChildProperties.Count; i++)
        cprops[i] = (PageProperty)this.ChildProperties.GetByIndex(i);
      for (int i = 0; i < cprops.Length; i++) 
        cprops[i].Publish(updatedb);
      if (_value != _prelimvalue) {
        _value = _prelimvalue;
        _valchanged = true;
      }
      publisheddate = DateTime.Now;
      publishedby = Cms.User.Id;
      _ispublished = true;
      if (updatedb) WriteToDB();
    }


    public BasePage ContainingPage {
      get { return containingpage; }
      set { containingpage = value; }
    }
    
    public bool EditPermission {
      get { return Cms.User.IsAdmin && Cms.GetPermission(ContainingPage.PageId, "WebPage", "Edit") && Cms.GetPermission(Id.ToString(), "PageProperty", "Edit", "General"); }
    }
  
    public String ShownControlValue {
      get {
        if (EditPermission) return ControlTypeProperty.PrelimValue;
        else return ControlTypeProperty.Value;
      }
    }
    
    public void SetControlPropertyValue(String value, bool updateDB) {
      if (ControlTypeProperty.PrelimValue != value) {
        ControlTypeProperty.PrelimValue = value;
        if (updateDB) ControlTypeProperty.WriteToDB();
      }
    }
    
    public String ControlTypePropertyName {
      get {
        if (ControlType == "tiny") return TinyPropertyName;
        else if (ControlType == "text") return TextPropertyName;
        else if (ControlType == "image") return ImagePropertyName;
        else if (ControlType == "images") return ImagesPropertyName;
        else if (ControlType == "flash") return FlashPropertyName;
        else if (ControlType == "movie") return MoviePropertyName;
        else return TinyPropertyName;
      }
    }

    public PageProperty ControlTypeProperty {
      get {
        if (ControlType == "tiny") return TinyProperty;
        else if (ControlType == "text") return TextProperty;
        else if (ControlType == "image") return ImageProperty;
        else if (ControlType == "images") return ImagesProperty;
        else if (ControlType == "flash") return FlashProperty;
        else if (ControlType == "movie") return MovieProperty;
        else return TinyProperty;
      }
    }
        
    private DateTime MaxDate(DateTime d1, DateTime d2) {
      if (d1 > d2) return d1;
      else return d2;
    }
    
    public DateTime ContentModDate {
      get {
        if (ControlTypeProperty.ModDate > ModDate) return ControlTypeProperty.ModDate;
        else return ModDate;
      }
    }

    private String LangSuffix {
      get {
        if (LanguageDependent && Cms.Language != "sv") return "_" + Cms.Language;
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
  

    public PageProperty TinyProperty {
      get { return GetProperty(TinyPropertyName); }
    }

    public PageProperty TextProperty {
      get { return GetProperty(TextPropertyName); }
    }

    public PageProperty ImageProperty {
      get { return GetProperty(ImagePropertyName); }
    }

    public PageProperty ImagesProperty {
      get { return GetProperty(ImagesPropertyName); }
    }

    public PageProperty FlashProperty {
      get { return GetProperty(FlashPropertyName); }
    }

    public PageProperty MovieProperty {
      get { return GetProperty(MoviePropertyName); }
    }

  }
  
  
}