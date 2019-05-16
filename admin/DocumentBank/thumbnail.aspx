<!-- $Date: 2010-10-07 12:13:34 +0200 (to, 07 okt 2010) $    $Revision: 7023 $ -->
<%@ Page Language="C#" ContentType="text/html" ResponseEncoding="iso-8859-1" %>
<%@ Import namespace="System.IO" %>

<script language="C#" runat="server">


  void Page_Load(Object sender, EventArgs e) {

    String reqPath = Request.QueryString["image"];
    String absPath;
    if (reqPath.StartsWith("/")) {
      if (reqPath.StartsWith(NFN.Util.HtmlRoot)) absPath = reqPath;
      else absPath = NFN.Util.HtmlRoot + reqPath;
    }
    else
      absPath = NFN.Util.HtmlRoot + "/" + reqPath;
    String fpath = absPath.Substring(0, absPath.Length - System.IO.Path.GetFileName(absPath).Length);

    String fname = System.IO.Path.GetFileNameWithoutExtension(absPath);
    String thumbName = fpath + "thumbs/" + fname + "_" + Request.QueryString["width"] + "x" + Request.QueryString["height"] + System.IO.Path.GetExtension(absPath);

    if (System.IO.File.Exists(Server.MapPath(thumbName))) {
      Response.Redirect(thumbName);
    }
    else {
      try{
        System.Drawing.Bitmap origBitmap = new System.Drawing.Bitmap(Server.MapPath(absPath));
        int origHeight = origBitmap.Height;
        int origWidth = origBitmap.Width;
        int reqHeight = origHeight;
        int reqWidth = origWidth;
        int newHeight = origHeight;
        int newWidth = origWidth;

        if (Request.QueryString["Height"] != null)
          reqHeight = Convert.ToInt32(Request.QueryString["height"]);

        if(Request.QueryString["Width"] != null)
          reqWidth = Convert.ToInt32(Request.QueryString["width"]);

        bool keepaspect = true;
        if (Request.QueryString["KeepAspect"] == "false")
          keepaspect = false;

        if (keepaspect) {

          double aspect = (double)origHeight / (double)origWidth;
          if (reqWidth <= newWidth) {
            newWidth = reqWidth;
            newHeight = Convert.ToInt32(Math.Round(newWidth*aspect));
          }
          if (reqHeight < newHeight) {
            newHeight = reqHeight;
            newWidth = Convert.ToInt32(Math.Round(1/aspect*newHeight));
          }
        }
        else {
          newHeight = reqHeight;
          newWidth = reqWidth;
        }
        if (newHeight <= 0) newHeight = 1;
        if (newWidth <= 0) newWidth = 1;

        System.Drawing.Bitmap outputImage = new System.Drawing.Bitmap(newWidth, newHeight);
        try {
          System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(outputImage);
          g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
          g.FillRectangle(System.Drawing.Brushes.White, 0, 0, newWidth, newHeight);
          g.DrawImage(origBitmap, 0, 0, newWidth, newHeight);
          //System.Drawing.Image outputImage = origBitmap.GetThumbnailImage(newWidth, newHeight, null, new IntPtr());
        }
        catch {
          System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(outputImage);
          g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
          g.FillRectangle(System.Drawing.Brushes.Gray, 0, 0, newWidth, newHeight);
        }

        Response.Cache.VaryByParams["Image;Width;Height"] = true;
        Response.ContentType = "image/gif";
        System.Collections.Hashtable imageOutputFormatsTable = new System.Collections.Hashtable();
        imageOutputFormatsTable.Add(System.Drawing.Imaging.ImageFormat.Gif.Guid, System.Drawing.Imaging.ImageFormat.Gif);
        imageOutputFormatsTable.Add(System.Drawing.Imaging.ImageFormat.Jpeg.Guid, System.Drawing.Imaging.ImageFormat.Jpeg);
        imageOutputFormatsTable.Add(System.Drawing.Imaging.ImageFormat.Bmp.Guid, System.Drawing.Imaging.ImageFormat.Gif);
        imageOutputFormatsTable.Add(System.Drawing.Imaging.ImageFormat.Tiff.Guid, System.Drawing.Imaging.ImageFormat.Jpeg);
        imageOutputFormatsTable.Add(System.Drawing.Imaging.ImageFormat.Png.Guid, System.Drawing.Imaging.ImageFormat.Png);

        //outputImage.InterpolationMode = InterpolationMode.HighQualityBicubic;
        System.Drawing.Imaging.ImageFormat outputFormat = (System.Drawing.Imaging.ImageFormat)imageOutputFormatsTable[origBitmap.RawFormat.Guid];

        if (!Directory.Exists(Server.MapPath(fpath + "thumbs")))
          Directory.CreateDirectory(Server.MapPath(fpath + "thumbs"));

        outputImage.Save(Server.MapPath(thumbName), outputFormat);
        outputImage.Save(Response.OutputStream, outputFormat);
        outputImage.Dispose();
        origBitmap.Dispose();
      }
      catch (Exception ex) {
        //Response.Write(ex.Message);
        Response.Redirect(HttpRuntime.AppDomainAppVirtualPath + "/admin/DocumentBank/gfx/thumberror.gif");
      }
    }
  }

</script>