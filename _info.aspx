<%@ Page language="C#"%>

<script language="c#" runat="server">

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    AppendRequestInfo();
  }

  protected HtmlGenericControl BodyTag {
    get {
      Control bodyTag = FindControl("bodyTag");
      if (bodyTag == null) return null;
      else return (HtmlGenericControl)bodyTag;
    }
  }

  private void AppendRequestInfo() {
    HttpBrowserCapabilities bc = Request.Browser;
    ip.Text = Request.UserHostAddress;
    dns.Text = Request.UserHostName;
    os.Text = bc.Platform;
    browser.Text = bc.Type + " - " + bc.Browser + " - " + bc.Version;
    javascript.Text = (bc.JavaScript ? "Supported" : "Not supported");
    lang.Text = Request.UserLanguages[0];
    encoding.Text = Request.ContentEncoding.EncodingName;

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
    cookielab.Text = cookies;

    String variables = "<table border=1><tr><td><b>Keys</b></td><td><b>Values</b></td></tr>";
    NameValueCollection coll = Request.ServerVariables;
    String[] arr1 = coll.AllKeys;
    for (int i=0; i < arr1.Length; i++) {
      variables += "<tr><td>" + arr1[i] + "</td><td>";
      String[] arr2 = coll.GetValues(arr1[i]);
      for (int j=0; j < arr2.Length; j++) {
        if (j > 0) variables += "<br />";
        variables += Server.HtmlEncode(arr2[j]);
      }
      variables += "</td></tr>";
    }
    variables += "</table>";
    variableslab.Text = variables;
  }

</script>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">

<html>
  <head runat="server">
    <link rel="stylesheet" media="screen,print" type="text/css" href="css/base.css" />
    <script type="text/javascript" src="/admin/jstools/swfobject.js"></script>

    <script type='text/javascript'>
      function showFlash() {
        if (typeof(swfobject) != 'undefined') {
          var ver = swfobject.getFlashPlayerVersion();
          var txt = 'Major version: ' + ver.major + '<br />';
          txt += 'Minor version: ' + ver.minor + '<br />';
          txt += 'Release: ' + ver.release + '<br />';
          document.getElementById("rinfo_flash").innerHTML = txt;
        }
        else
          document.getElementById("rinfo_flash").innerHTML = 'No Flash on page';
      }
    </script>
  </head>
  <body id="bodyTag" runat="server">

    <h2>Request information</h2>
    <table border=1 width="800px">
      <tr><td valign='top'>IP-address</td><td valign='top'><asp:Label id="ip" runat="server" /></td></tr>
      <tr><td valign='top'>DNS</td><td valign='top'><asp:Label id="dns" runat="server" /></td></tr>
      <tr><td valign='top'>OS</td><td valign='top'><asp:Label id="os" runat="server" /></td></tr>
      <tr><td valign='top'>Browser</td><td valign='top'><asp:Label id="browser" runat="server" /></td></tr>
      <tr><td valign='top'>Javascript</td><td valign='top'><asp:Label id="javascript" runat="server" /></td></tr>
      <tr><td valign='top'>Flash</td><td valign='top'><div id='rinfo_flash'></div></td></tr>
      <tr><td valign='top'>Languages</td><td valign='top'><asp:Label id="lang" runat="server" /></td></tr>
      <tr><td valign='top'>Encoding</td><td valign='top'><asp:Label id="encoding" runat="server" /></td></tr>
      <tr><td valign='top'>Cookies</td><td valign='top'><asp:Label id="cookielab" runat="server" /></td></tr>
      <tr><td valign='top'>Server Variables</td><td valign='top'><asp:Label id="variableslab" runat="server" /></td></tr>
    </table>

    <script type="text/javascript">showFlash();</script>
  </body>
</html>

