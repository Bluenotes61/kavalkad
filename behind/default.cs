using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Net.Mail;
using System.Text;
using AjaxPro;
using NFN;
using NFN.Controls;


public partial class DefaultPage : BasePage {

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    AjaxPro.Utility.RegisterTypeForAjax(typeof(DefaultPage));
    
    EditBtns.Visible = Cms.User.IsAdmin;
    AddLink.Visible = Cms.User.IsAdmin;
    SpexGrids.Visible = Cms.User.IsAdmin;
  }
  
  protected override void Render(HtmlTextWriter writer) {
    String res = "";
    if (Request["oper"] != null && Request["oper"] == "del") {
      if (Request["grid"] == "spex")
        res = DelSpex();
      else if (Request["grid"] == "upps")
        res = DelUppsattning();
      Response.AddHeader("Content-type","text/js;charset=utf-8");
      writer.Write(res);
    }
    else if (Request["oper"] != null && Request["oper"] == "add") {
      if (Request["grid"] == "spex")
        res = SaveNewSpex();
      else if (Request["grid"] == "upps")
        res = SaveNewUppsattning();
      Response.AddHeader("Content-type","text/js;charset=utf-8");
      writer.Write(res);
    }
    else if (Request["oper"] != null && Request["oper"] == "edit") {
      if (Request["grid"] == "spex")
        res = SaveEditSpex();
      else if (Request["grid"] == "upps")
        res = SaveEditUppsattning();
      Response.AddHeader("Content-type","text/js;charset=utf-8");
      writer.Write(res);
    }
    else if (Request["sql"] != null) {
      Response.AddHeader("Content-type","text/js;charset=utf-8");
      writer.Write(GridUtils.GetGridData(Request, ""));
    }
    else 
      base.Render(writer);
  }
  
  public String ServerJS {
    get {
      String html = "var isAdmin = " + Cms.User.IsAdmin.ToString().ToLower() + ";";
      DataSet ds = DB.GetDS("select title from spex order by syear desc");
      html += "var spexes = \":";
      for (int i=0; i < DB.GetRowCount(ds); i++) 
        html += ";" + DB.GetString(ds, i, "title") + ":" + DB.GetString(ds, i, "title");
      html += "\";";
      return html;
    }
  }


  public String IsAdmin {
    get { return (Cms.User.IsAdmin ? "true" : "false"); }
  }
  
  private void GenerateSiU() {
    DataSet ds = DB.GetDS("select * from uppsattningar");
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      int spexid = DB.GetInt("select id from spex where title='" + DB.GetString(ds, i, "spex") + "'", "id");
      int uid = DB.GetInt("select id from uppsattningar0 where spexid=" + spexid + " and uyear=" + DB.GetInt(ds, i, "uppsattning"), "id");
      for (int col=1; col <=57; col++) {
        int songid = DB.GetInt(ds, i, col.ToString());
        if (songid != 0)
          DB.ExecSql("insert into sim (songid, uppsattningid, orderno) values(" + songid + "," + uid + "," + col + ")");
      }
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String[] GetTextInfo(int songid) {
    DataSet ds = DB.GetDS("select * from view_song where songid=" + songid);
    String[] res = new String[3];
    res[0] = DB.GetString(ds, 0, "title");
    res[1] = DB.GetString(ds, 0, "spex");
    res[2] = DB.GetString(ds, 0, "melody");
    DB.ExecSql("update songs set textviews = textviews+1 where id=" + songid);
    return res;
  }

  private String GetSpexOptions(int spexid) {
    String options = "<option></option>";
    DataSet dssp = DB.GetDS("select * from spex order by syear desc");
    for (int i=0; i < DB.GetRowCount(dssp); i++)
      options += "<option value='" + DB.GetInt(dssp, i, "id") + "' " + (DB.GetInt(dssp, i, "id") == spexid ? "selected" : "") + ">" + DB.GetString(dssp, i, "title") + " (" + DB.GetString(dssp, i, "syear") + ")</option>";
    return options;
  }
  
  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetUppsattningOptions(int spexid) {
    String options = "<option value=''>Välj uppsättning</option>";
    DataSet dssp = DB.GetDS("select id, uyear, info from uppsattningar where spexid=" + spexid + " order by uyear desc");
    for (int i=0; i < DB.GetRowCount(dssp); i++) 
      options += "<option value='" + DB.GetInt(dssp, i, "id") + "|" + DB.GetString(dssp, i, "uyear") + "|" + DB.GetString(dssp, i, "info") + "'>" + DB.GetString(dssp, i, "uyear") + (DB.GetString(dssp, i, "info").Length > 0 ? " - " + DB.GetString(dssp, i, "info") : "") + "</option>";
    return options;
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetCategoryOptions(String selcat) {
    String cat = "<option></option>";
    DataSet dsc = DB.GetDS("select * from categories");
    for (int i=0; i < DB.GetRowCount(dsc); i++) 
      cat += "<option value='" + DB.GetString(dsc, i, "category") + "' " + (DB.GetString(dsc, i, "category") == selcat ? "selected" : "") + ">" + DB.GetString(dsc, i, "category") + "</option>";
    return cat;
  }

  private String GetInfoTableHtml(int songid, String title, String alt_title, String spex, String spexyear, String spexOptions, String singer, String firstrow, String[] upps, String[] mel, int textlength, String[] notes) {
    
    String htmlu = "<div id='uppsdiv'>";
    for (int i=0; i < upps.Length; i++) {
      String[] vals = upps[i].Split('|');  // id, uyear, order, info
      String onehtml = @"
        <div class='oneupps'>
          <span class='edit'><a href='javascript:void(0)' onclick='deleteUppsattningFromSong(this)' title='Ta bort uppsättnig'><img src='gfx/delete.png' alt='' /></a></span>
          {1}{3}, Kuplettnummer <span class='static'>{2}</span>
          <span class='edit'>
            <input type='hidden' class='uppsid' value='{0}' />
            <input type='text' class='uppsorder' value='{2}' />
          </span>
         </div>";
       String info = (vals[3].Length > 0 ? ", " + vals[3] : "");
       htmlu += String.Format(onehtml, vals[0], vals[1], vals[2], info);
    }
    htmlu += "</div>";
    
    String html = @"
      <input type='hidden' id='songid' value='{0}' />
      <table id='infotable' cellpadding='2'>
        <tr><td></td><td valign='top' width='120'><b>Titel:</b></td><td><span class='static'>{1}</span><span class='edit'><input type='text' id='edtitle' value='{1}'/></span></td></tr>";

    if (alt_title.Length > 0 || songid == 0) 
      html += @"<tr><td></td><td valign='top' width='120'><b>Alt. titel:</b></td><td><span class='static'>{2}</span><span class='edit'><input type='text' id='edalttitle' value='{2}'/></span></td></tr>";

    html += @"
      <tr><td></td><td valign='top' width='120'><b>Spex:</b></td><td><span class='static'>{3} ({4})</span><span class='edit'><select id='edspex' onchange='songSpexSelected()'>{5}</select></span></td></tr>
      <tr><td valign='top'>
        <span class='edit'>
          <a href='javascript:addUppsattningToSong()' onfocus='this.blur()' title='Lägg till uppsättning'><img src='gfx/add.gif' alt='Lägg till uppsättning' /></a>
          <div id='uppsdd'><select onchange='uppsSelected()'></select></div>
        </span>
      </td>
      <td valign='top'><b>Uppsättningar:</b></td>
      <td>{8}</td></tr>
      <tr><td></td><td valign='top' width='120'><b>Rollfigur:</b></td><td><span class='static'>{6}</span><span class='edit'><input type='text' id='edsinger' value='{6}'/></span></td></tr>
      <tr><td></td><td valign='top' width='120'><b>Förstarad:</b></td><td><span class='static'>{7}</span><span class='edit'><input type='text' id='edfirstline' value='{7}'/></span></td></tr>";

    html += @"
      <tr><td valign='top'><span class='edit'><a href='javascript:addMelody()' onfocus='this.blur()' title='Lägg till melodi'><img src='gfx/add.gif' alt='Lägg till melodi' /></a></span></td>
      <td colspan='2'>
        <table id='melodytable' cellspacing='0' cellpadding='0'>
          <tr><td class='edit'></td><td><b>Melodi</b></td><td><b>Alt. melodi</b></td><td><b>Kompositör</b></td><td><b>Kategori</b></td></tr>";
    for (int i=0; i < mel.Length; i++) {
      String[] vals = mel[i].Split('|');  // melody, alt_mel, composer, cat, catOptions
      String onehtml = @"
        <tr><td class='edit'><span class='edit'><a href='javascript:void(0)' onclick='deleteMelody(this)' title='Ta bort melodi'><img src='gfx/delete.png' alt='' /></a></span></td>
        <td><span class='static'>{0}</span><span class='edit'><input type='text' class='i_mel' value='{0}' /></span></td>
        <td><span class='static'>{1}</span><span class='edit'><input type='text' class='i_amel' value='{1}' /></span></td>
        <td><span class='static'>{2}</span><span class='edit'><input type='text' class='i_composer' value='{2}' /></span></td>
        <td><span class='static'>{3}</span><span class='edit'><select>{4}</select></span></td></tr>";
      html += String.Format(onehtml, vals[0].Replace("'", "¤"), vals[1].Replace("'", "¤"), vals[2].Replace("'", "¤"), vals[3], vals[4]);
    }
    html += "</table></td></tr>";

    if ((Cms.User.IsAdmin && songid > 0) || textlength  > 0)
      html += "<tr><td valign='top'></td><td><b>Text:</b></td><td><a href='javascript:void(0)' onclick='openText({0})' onfocus='this.blur()'><img src='admin/gfx/page.gif' alt='Text' border='0' /></a></td></tr>";

    if ((Cms.User.IsAdmin && songid > 0) || notes.Length > 0) {
      html += @"
        <tr>
          <td valign='top'><span class='edit'><a href='javascript:addNotes()' title='Lägg till not'><img src='gfx/add.gif' alt='' /></a></span></td>
          <td><b>Noter:</b></td>
          <td><div id='notesdiv'>";
      for (int i=0; i < notes.Length; i++) {
        String[] hlp = notes[i].Split('/');
        String notename = hlp[hlp.Length - 1];
        String onehtml = @"
          <div class='onenote'>
            <span class='edit'><a href='javascript:void(0)' onclick='delNotes(this)' title='Ta bort not'><img src='gfx/delete.png' alt='' /></a></span>
            <a href='{0}' class='notelink' target='_blank' onfocus='this.blur()'>{1}</a>
          </div>";
        html += String.Format(onehtml, notes[i], notename);
      }
      html += "</div></td></tr>";
    }

    html += "</table>";
    return String.Format(html, songid.ToString(), title.Replace("'", "¤"), alt_title.Replace("'", "¤"), spex, spexyear, spexOptions, singer.Replace("'", "¤"), firstrow.Replace("'", "¤"), htmlu);
  }


  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetSongInfo(int songid) {

    DataSet dss = DB.GetDS("select * from view_song where songid=" + songid);

    DataSet dsu = DB.GetDS("select u.id, u.uyear, u.spexid, u.info, siu.orderno from uppsattningar u, songinuppsattning siu where siu.songid=" + songid + " and u.id=siu.uppsattningid order by u.uyear");
    String[] upps = new String[DB.GetRowCount(dsu)];
    for (int i=0; i < DB.GetRowCount(dsu); i++) 
      upps[i] = DB.GetString(dsu, i, "id") + "|" + DB.GetString(dsu, i, "uyear") + "|" + DB.GetString(dsu, i, "orderno") + "|" + DB.GetString(dsu, i, "info");

    DataSet dsmel = DB.GetDS("select * from songmelody where songid=" + songid + " order by orderno");
    String[] mel = new String[DB.GetRowCount(dsmel)];
    for (int i=0; i < DB.GetRowCount(dsmel); i++) 
      mel[i] = DB.GetString(dsmel, i, "melody") +  "|" + DB.GetString(dsmel, i, "alt_melody") +  "|" + DB.GetString(dsmel, i, "composer") +  "|" + DB.GetString(dsmel, i, "category") +  "|" + GetCategoryOptions(DB.GetString(dsmel, i, "category"));

    String snotes = DB.GetString(dss, 0, "notes");
    String[] notes = (snotes.Length > 0 ? snotes.Split(';') : new String[0]);

    return GetInfoTableHtml(
      songid, 
      DB.GetString(dss, 0, "title"), 
      DB.GetString(dss, 0, "alt_title"), 
      DB.GetString(dss, 0, "spex"), 
      DB.GetString(dss, 0, "spexyear"), 
      GetSpexOptions(DB.GetInt(dss, 0, "spexid")), 
      DB.GetString(dss, 0, "singer"), 
      DB.GetString(dss, 0, "firstline"), 
      upps, 
      mel, 
      DB.GetInt(dss, 0, "textlength"),
      notes);
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public String GetNewForm() {
    String[] empty = new String[0];
    return GetInfoTableHtml(0, "", "", "", "", GetSpexOptions(0), "", "", empty, empty, 0, empty);
  }

  private bool HasText(int songid) {
    if (ThisPage.MainProp.HasProperty("Song_" + songid))
      return (ThisPage.MainProp.GetProperty("Song_" + songid).GetProperty("text").Value.Length > 0);
    else
      return false;
  }

  public String CatOptions {
    get {
      String html = "<option value=''></option>";
      DataSet ds = DB.GetDS("select * from categories order by category");
      for (int i=0; i < DB.GetRowCount(ds); i++)
        html += "<option value='" + DB.GetString(ds, i, "category") + "'>" + DB.GetString(ds, i, "category") + "</option>";
      return html;
    }
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void SaveSong(String songid, String title, String alttitle, String spex, String singer, String firstline, String[] upps, String[] melody, String[] notes) {
    String sql = "update songs set title='" + title.Replace("'", "''") + "', alt_title='" + alttitle.Replace("'", "''") + "', singer='" + singer.Replace("'", "''") + "', firstline='" + firstline.Replace("'", "''") + "', spexid=" + spex + " where id=" + songid;
    DB.ExecSql(sql);
    DB.ExecSql("delete from songinuppsattning where songid=" + songid);
    for (int i=0; i < upps.Length; i++) {
      String[] oneupps = upps[i].Split('|');
      int order = 0;
      try { order = Convert.ToInt32(oneupps[1]); }
      catch { order = 0; }
      DB.ExecSql("insert into songinuppsattning (songid, uppsattningid, orderno) values(" + songid + ", " + oneupps[0] + ", " + order + ")");
    }
    DB.ExecSql("delete from songmelody where songid=" + songid);
    for (int i=0; i < melody.Length; i++) {
      String[] onemel = melody[i].Split('|');
      DB.ExecSql("insert into songmelody (songid, melody, alt_melody, composer, category) values(" + songid + ", '" + onemel[0].Replace("'", "''") + "', '" + onemel[1].Replace("'", "''") + "', '" + onemel[2].Replace("'", "''") + "', '" + onemel[3] + "')");
    }
    DB.ExecSql("delete from songnotes where songid=" + songid);
    for (int i=0; i < notes.Length; i++) 
      DB.ExecSql("insert into songnotes (songid, notes) values(" + songid + ", '" + notes[i] + "')");
  }
  
  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void SaveNewSong(String title, String alttitle, String spex, String singer, String firstline, String[] upps, String[] melody, String[] notes) {
    String sql = "insert into songs (title, alt_title, singer, firstline, spexid) values('" + title.Replace("'", "''") + "', '" + alttitle.Replace("'", "''") + "', '" + singer.Replace("'", "''") + "', '" + firstline.Replace("'", "''") + "', " + spex + ")";
    DB.ExecSql(sql);
    int songid = DB.GetInt("select max(id) as maxid from songs", "maxid");
    
    for (int i=0; i < upps.Length; i++) {
      String[] oneupps = upps[i].Split('|');
      int order = 0;
      try { order = Convert.ToInt32(oneupps[1]); }
      catch { order = 0; }
      DB.ExecSql("insert into songinuppsattning (songid, uppsattningid, orderno) values(" + songid + ", " + oneupps[0] + ", " + order + ")");
    }
    for (int i=0; i < melody.Length; i++) {
      String[] onemel = melody[i].Split('|');
      DB.ExecSql("insert into songmelody (songid, melody, alt_melody, composer, category) values(" + songid + ", '" + onemel[0].Replace("'", "''") + "', '" + onemel[1].Replace("'", "''") + "', '" + onemel[2].Replace("'", "''") + "', '" + onemel[3] + "')");
    }
    for (int i=0; i < notes.Length; i++) 
      DB.ExecSql("insert into songnotes (songid, notes) values(" + songid + ", '" + notes[i] + "')");
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void DeleteSong(String songid) {
    DB.ExecSql("delete from songnotes where songid=" + songid);
    DB.ExecSql("delete from songmelody where songid=" + songid);
    DB.ExecSql("delete from songinuppsattning where songid=" + songid);
    DB.ExecSql("delete from songs where id=" + songid);
  }

  private String SaveEditSpex() {
    String id = Request["id"];
    String title = Request["title"];
    if (title.Length == 0)
      return "Titelfältet kan inte vara tomt";
    String syear = Request["syear"];
    try { int c = Convert.ToInt32(syear); }
    catch { return "Årtalet måste vara en siffra"; }
    DB.ExecSql("update spex set title='" + title.Replace("'", "''") + "', syear=" + syear + " where id=" + id);
    return "";
  }

  private String SaveNewSpex() {
    String title = Request["title"];
    if (title.Length == 0)
      return "Titelfältet kan inte vara tomt";
    String syear = Request["syear"];
    try { int c = Convert.ToInt32(syear); }
    catch { return "Årtalet måste vara en siffra"; }
    DB.ExecSql("insert into spex (title, syear) values('" + title.Replace("'", "''") + "', " + syear + ")");
    return "";
  }

  private String DelSpex() {
    String id = Request["id"];
    if (DB.GetInt("select count(*) as nof from songs where spexid=" + id, "nof") > 0)
      return "Spexet innehåller kupletter och kan inte tas bort.";
    DB.ExecSql("delete from uppsattningar where spexid=" + id);
    DB.ExecSql("delete from spex where id=" + id);
    return "";
  }

  private String SaveEditUppsattning() {
    String id = Request["id"];
    String info = Request["info"];
    String uyear = Request["uyear"];
    try { int c = Convert.ToInt32(uyear); }
    catch { return "Årtalet måste vara en siffra"; }
    DB.ExecSql("update uppsattningar set info='" + info.Replace("'", "''") + "', uyear=" + uyear + " where id=" + id);
    return "";
  }

  private String SaveNewUppsattning() {
    String spex = Request["spex"];
    String info = Request["info"];
    String uyear = Request["uyear"];
    try { int c = Convert.ToInt32(uyear); }
    catch { return "Årtalet måste vara en siffra"; }
    DB.ExecSql("insert into uppsattningar (spexid, uyear, info) values(" + spex + ", " + uyear + ", '" + info.Replace("'", "''") + "')");
    return "";
  }

  private String DelUppsattning() {
    String id = Request["id"];
    DB.ExecSql("delete from songinuppsattning where uppsattningid=" + id);
    DB.ExecSql("delete from uppsattningar where id=" + id);
    return "";
  }

  [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
  public void Logout() {
    Cms.LogOut();
  }
  
}

