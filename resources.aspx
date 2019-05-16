<%@ Page Language="C#" debug="false"%>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.IO.Compression" %>
<%@ Import Namespace="NFN" %>

<script language="C#" runat="server">

  CMS Cms {
    get { return (CMS)Page.Session["CMS"]; }
  }

  void Page_Load(object sender, EventArgs e) {
    String type = Request["type"];

    String[] files = (type == "css" ? Util.GetCSSFiles() : Util.GetJSFiles());

    StringBuilder data = new StringBuilder();
    if (files.Length > 0) {
      for (int i=0; i < files.Length; i++) {
        String fname = Server.MapPath(files[i].Replace("{language}", Cms.Language));
        if (File.Exists(fname))
          data.Append(File.ReadAllText(fname));
      }
    }

    Response.Write(data);

    Response.ContentType = (type == "css" ? "text/css" : "text/javascript");
    Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
    String acceptEncoding = Request.Headers["Accept-Encoding"];

    if (acceptEncoding != null) {
      if (acceptEncoding.Contains("gzip")) {
        Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
        Response.AppendHeader("Content-Encoding", "gzip");
      }
      else if (acceptEncoding.Contains("deflate")) {
        Response.Filter = new DeflateStream(Response.Filter, CompressionMode.Compress);
        Response.AppendHeader("Content-Encoding", "deflate");
      }
    }

    Response.Cache.SetExpires(DateTime.Now.AddDays(7));
    Response.Cache.SetCacheability(HttpCacheability.Public);
    Response.Cache.SetValidUntilExpires(true);
    Response.Cache.SetVaryByCustom("browser");
  }

</script>



