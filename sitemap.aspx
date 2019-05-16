<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Xml" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="NFN" %>

<script runat="server" language="C#">

  private String GetBaseUrl() {
    return "http://" + Request.ServerVariables["HTTP_HOST"] + "/";
  }


  private void Page_Load(object sender, System.EventArgs e) {
    Response.ContentType = "text/xml";
    Response.ContentEncoding = Encoding.UTF8;

    if (Cache["Sitemap"] == null) {
      StringWriter sw = new StringWriter();
      XmlTextWriter writer = new XmlTextWriter(sw);

      writer.WriteStartElement("rss");
      writer.WriteAttributeString("version", "2.0");
      writer.WriteStartElement("channel");

      writer.WriteElementString("title", "Sitemap");

      writer.WriteElementString("link", GetBaseUrl());
      writer.WriteElementString("description", "");
      writer.WriteElementString("ttl", "60");

      DataSet ds = DB.GetDS("select * from webpage where protected <> 'y' and deleted=0 and pagestatus in ('active','hidden')");
      for (int i=0; i < DB.GetRowCount(ds); i++) {
        writer.WriteStartElement("item");

        writer.WriteElementString("title", DB.GetString(ds, i, "name"));
        writer.WriteElementString("link", String.Format("{0}{1}.aspx", GetBaseUrl(), DB.GetString(ds, i, "id")));
        writer.WriteElementString("description", DB.GetString(ds, i, "title"));
        writer.WriteElementString("pubDate", DB.GetDate(ds, i, "moddate").ToString("yyyy-MM-dd"));

        writer.WriteEndElement();
      }
      writer.WriteEndElement();
      writer.WriteEndElement();

      Cache.Insert("Sitemap", sw.ToString());

      writer.Close();
    }

    Response.Write(Cache["Sitemap"].ToString());
  }
</script>