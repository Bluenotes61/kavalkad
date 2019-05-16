/* $Date: 2010-10-07 12:13:34 +0200 (to, 07 okt 2010) $    $Revision: 7023 $ */
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Collections.Generic;
using NFN;
using NFN.Flash;


public partial class Upload : Page {

  private string[] imageExtensions = {"gif","jpg","bmp", "jpeg", "png"};
  private string[] mediaExtensions = {"swf","flv","dcr","mov","qt","mpg","mp3","mp4","mpeg","dcr","avi","wmv","wm","asf","asx","wmx","wvx","rm","ra","ram"};
  private string[] documentExtensions = {"doc","docx","pdf","xls"};
  private string docPath = "DocumentBank/";
  private const int dummyPathId = -1;

  private CMS Cms {
    get { return (CMS)Page.Session["CMS"]; }
  }

  private System.Drawing.Imaging.ImageFormat GetImageFormat(String filename) {
    String ext = Path.GetExtension(filename).TrimStart('.').ToLower();
    if (ext == "gif") return System.Drawing.Imaging.ImageFormat.Gif;
    else if (ext == "jpg") return System.Drawing.Imaging.ImageFormat.Jpeg;
    else if (ext == "jpeg") return System.Drawing.Imaging.ImageFormat.Jpeg;
    else if (ext == "bmp") return System.Drawing.Imaging.ImageFormat.Bmp;
    else if (ext == "png") return System.Drawing.Imaging.ImageFormat.Png;
    else return System.Drawing.Imaging.ImageFormat.Jpeg;
  }

  protected void Page_Load(object sender, EventArgs e) {

    try {
      HttpPostedFile file = Request.Files["Filedata"];
      String fileName = Path.GetFileName(file.FileName);

      String docType = "file";
      foreach (string extension in imageExtensions){
        if (extension == Path.GetExtension(fileName).TrimStart('.').ToLower()){
          docType = "image";
          break;
        }
      }
      foreach (string extension in mediaExtensions){
        if (extension == Path.GetExtension(fileName).TrimStart('.').ToLower()){
          docType = "media";
          break;
        }
      }
      foreach (string extension in documentExtensions){
        if (extension == Path.GetExtension(fileName).TrimStart('.').ToLower()){
          docType = "document";
          break;
        }
      }

      String aPath = docPath + fileName;
      int count = 1;
      while (File.Exists(Server.MapPath("~/" + aPath))) {
        aPath = docPath + Path.GetFileNameWithoutExtension(fileName) + "(" + count.ToString() + ")" + Path.GetExtension(fileName);
        count++;
      }

      int fileSize = file.ContentLength;
      if (fileSize == 0)
        throw new Exception("Fel: Filen " + fileName + " är tom.");

      try {
        String now = DateTime.Now.ToString(CMS.SiteSetting("dateTimeFormat"));
        String sqlStr = "insert into documents (filepath, doctype, filesize, imagewidth, imageheight, filename, date_start, upload_date, upload_user) " +
          "values({0}, '{1}', {2}, {3}, {4}, '{5}', '" + now + "', '" + now + "', " + Cms.User.Id + ")";
        if (docType == "image") {
          System.Drawing.Bitmap origBitmap;
          try {
            origBitmap = new System.Drawing.Bitmap(file.InputStream);
          }
          catch (Exception ex) {
            throw new Exception("Error:" + ex.Message);
          }

          int newHeight;
          int newWidth;

          newHeight = origBitmap.Height;
          newWidth = origBitmap.Width;
          origBitmap.Save(Server.MapPath("~/" + aPath),GetImageFormat(fileName));

          sqlStr = String.Format(sqlStr, dummyPathId, "image", fileSize.ToString(), newWidth.ToString(), newHeight.ToString(), "/" + aPath);
          DB.ExecSql(sqlStr);

        }
        else if (docType == "media" && Path.GetExtension(fileName).TrimStart('.').ToLower() == "swf") {
          file.SaveAs(Server.MapPath("~/" + aPath));

          SWFHeaderInfo swf = new SWFHeaderInfo(Server.MapPath("~/" + aPath));
          int w = Convert.ToInt32(swf.xMax - swf.xMin);
          int h = Convert.ToInt32(swf.yMax - swf.yMin);

          sqlStr = String.Format(sqlStr, dummyPathId, "media", fileSize.ToString(), w.ToString(), h.ToString(), "/" + aPath);
          DB.ExecSql(sqlStr);
        }
        else {
          file.SaveAs(Server.MapPath("~/" + aPath));

          sqlStr = String.Format(sqlStr, dummyPathId, docType, fileSize.ToString(), "null", "null", "/" + aPath);
          DB.ExecSql(sqlStr);
        }

        DataSet ds;
        if (ConfigurationManager.AppSettings["connectionType"] == "ODBC")
          ds = DB.GetDS("select id from documents order by id desc");
        else
          ds = DB.GetDS("select top 1 id from documents order by id desc");

        Cms.LogEvent("docupload", DB.GetString(ds, 0, "id"));        

      }
      catch (Exception ex) {
        Util.Debug("Fel:" + ex.Message);
        throw new Exception("Fel:" + ex.Message);
      }
    }
    catch (Exception ex) {
      Util.Debug("Fel:" + ex.Message);
      throw new Exception("Fel:" + ex.Message);
    }
  }
}

