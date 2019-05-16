<%-- $Date: 2010-02-12 00:32:36 +0100 (fr, 12 feb 2010) $    $Revision: 5940 $ --%>
<%@ Page language="C#" CodeFile="behind/default.cs" Inherits="LoginPage"%>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">
<html>
  <head>
    <title>040 Plus - <%= Cms.Translate("Inloggning")%></title>
    <link rel="stylesheet" href="css/base.css" type="text/css">
    <link rel="stylesheet" href="css/default.css" type="text/css">
    <script type="text/javascript" src="/admin/jstools/jquery/jquery-1.3.2.js"></script>
    <script type="text/javascript" src="script/default.js"></script>
  </head>
  <body>
    <form runat="server">
      <div id="maindiv"><div id="maindiv_inner">
        <div id="urldiv"><%= Url %></div>
        <div id='logodiv'><img id="Logo" runat="server"/></div>
        <div id="logindiv">
          <div id='liform'>
            <table><tr>
              <td><%= Cms.Translate("Användarnamn")%>:<br /><input type="text" id="username" /></td>
              <td><%= Cms.Translate("Lösenord")%>:<br><input type="password" id="password" /></td>
            </tr></table>
          </div>
          <div id='libtn'><input type='button' value='<%= Cms.Translate("Logga in") %>' onclick='login();return false;' /></div>
          <div id="forgotlink"><a href='javascript:openForgot()' onfocus='this.blur()'><%= Cms.Translate("Glömt lösenordet?") %></a></div>
        </div>
        <div id="forgotdiv">
          <div id='forgotform'><%= Cms.Translate("Ange användarnamn")%>:<br /><input type='text' id='fusername' /><br /><%= Cms.Translate("eller") %><br /><%= Cms.Translate("Ange e-postadress")%>:<br /><input type='text' id='femail' /></div>
          <div id='reqerr'></div>
          <div id='sendbtn'>
            <input type='button' value='<%= Cms.Translate("Skicka inloggningsinfo") %>' onclick='request();return false;' />&nbsp;&nbsp;&nbsp;&nbsp;
            <input type='button' value='<%= Cms.Translate("Avbryt") %>' onclick='closeForgot();return false;' />
          </div>
          <div class='clearfloat'></div>
        </div>
        <div id="reqresult">
          <%= Cms.Translate("Information har skickats till e-postadressen")%>: <span id='reqemail'></span>
          <div id="okreq"><input type='button' value='<%= Cms.Translate("Ok") %>' onclick='okreq();return false;' /></div>
        </div>
      </div></div>
    </form>
  </body>
</html>

