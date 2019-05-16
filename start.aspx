<%@ Page language="C#" MasterPageFile="Site.master" CodeFile="/behind/start.cs" Inherits="StartPage" EnableEventValidation="true" %>
<%@ Register TagPrefix="NFN" Namespace="NFN.Controls" %>

<asp:Content ContentPlaceHolderId="HeaderContent" runat="server">
  <link rel="stylesheet" media="screen,print" type="text/css" href="css/start.css" />
  <script type='text/javascript' src='js/start.js'></script>
</asp:Content>

<asp:Content ContentPlaceHolderId="MainContent" runat="server">

  <div id='startcontent'>
    <h3>Och nu blir det...</h3>
    <h1>KAVALKAD!</h1>
    <div id="logindiv">
      <div id='loginerr'>Ånä, det funkade inte!</div>
      <div id='un'>Användarnamn<br /><input type="text" id='username' /></div>
      <div id='pw'>Lösenord<br /><input type="password" id='password' onkeydown='checkEnter(event)' /></div>
      <div class='clearfloat'></div>
      <div id='li'><a href='javascript:void(0)' onclick='login()'>Logga in</a></div>
    </div>
  </div>

</asp:Content>
