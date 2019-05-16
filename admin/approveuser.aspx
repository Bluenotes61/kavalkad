<%-- $Date: 2009-04-20 11:16:14 +0200 (mÃ¥, 20 apr 2009) $    $Revision: 4823 $ --%>
<%@ Page language="C#" CodeFile="behind/approveuser.cs" Inherits="ApproveUser"%>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">

<html>
  <head>
    <title>Bevilja användare</title>
    <link rel="stylesheet" media="screen,print" type="text/css" href="/css/base.css">

    <script language="javascript" type="text/javascript" src="tiny_mce/tiny_mce.js"></script>
    <script language="javascript" type="text/javascript">

      function send() {
        var html = tinyMCE.getContent("mailbody");
        var email = document.getElementById("email").innerHTML;
        var subject = document.getElementById("subject").value;
        var response = ApproveUser.SendMail(subject, html, email);
        document.getElementById("sendmail").style.display = "none";
        if (response.error != null) {
          document.getElementById("errmail").style.display = "block";
          document.getElementById("errmess").innerHTML = response.error.Message;
        }
        else
          document.getElementById("okmail").style.display = "block";
      }

      tinyMCE.init({
        mode:'textareas',
        editor_selector : "mceEditor",
        theme:'advanced',
        language:'sv_utf8',
        content_css:'/css/base.css,/css/base_news.css',
        width:'500px',
        height:'400px',
        plugins:'inlinepopups,contextmenu,docbank,advlink,advimage,paste',
        relative_urls : false,
        convert_urls : false,
        theme_advanced_toolbar_location:'top',
        theme_advanced_containers_default_align:'left',
        theme_advanced_layout_manager:'RowLayout',
        theme_advanced_containers:'cont1,mceEditor',
        theme_advanced_container_cont1:'formatselect,separator,bold,italic,separator,undo,redo,separator,link,unlink,separator,code'
      });
    </script>
  </head>
  <body style="margin:20px;">
    <form runat="server">
      <asp:PlaceHolder id="okholder" runat="server">
        <h2 id="yes" runat="server">Användaren kan nu logga in på sajten.</h2>
        <h2 id="no" runat="server">Användaren gavs inte tillgång till sajten och är borttagen.</h2>
        <p>Användarnamn: <asp:Label id="username" runat="server" /></p>
        <p>E-postadress: <asp:Label id="email" runat="server" /></p>
        <div id="sendmail" style="margin-top:20px;" runat="server">
          <p><b>Mejlets ämne</b><br /><input type="text" id="subject" size=60 value="Registrering på www.minc.se" /></p>
          <p><b>Mejlets innehåll</b><br />
          <textarea id="mailbody" class='mceEditor'>
            <asp:Literal id="initialbody" runat="server" />
          </textarea></p>
          <p><a href="javascript:send()" onfocus="this.blur()">SKICKA MEJL</a></p>
        </div>
        <div id="okmail" style="margin:10px 0 10px 0;display:none"><h2>Mejlet är skickat</h2></div>
        <div id="errmail" style="margin:10px 0 10px 0;display:none"><h2>Ett fel uppstod då mejlet skickades: <span id="errmess"></span></h2></div>
      </asp:PlaceHolder>
      <asp:PlaceHolder id="errorholder" runat="server">
        <h2>Sidan anropad på felaktigt sätt.</h2>
      </asp:PlaceHolder>
      <p><a href="/" onfocus="this.blur()">TILL SAJTEN</a></p>
    </form>
  </body>
</html>
