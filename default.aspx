<%@ Page language="C#" MasterPageFile="Site.master" CodeFile="/behind/default.cs" Inherits="DefaultPage" EnableEventValidation="false" %>
<%@ Register TagPrefix="NFN" Namespace="NFN.Controls" %>

<asp:Content ContentPlaceHolderId="HeaderContent" runat="server">
  <link rel="stylesheet" media="screen,print" type="text/css" href="css/default.css" />
  <link rel="stylesheet" media="print" type="text/css" href="css/default_print.css" />
  <link rel="stylesheet" type="text/css" href='admin/jstools/jquery/plugins/ui/css/smoothness/jquery-ui-1.8.5.custom.css'>
  <link rel="stylesheet" type="text/css" href='admin/jstools/jquery/plugins/grid/css/ui.jqgrid.css'>
  <script type='text/javascript' src="admin/jstools/jquery/plugins/ui/jquery-ui-1.8.5.custom.min.js"></script>
  <script type='text/javascript' src="admin/jstools/jquery/plugins/grid/lang/grid.locale-sv.js"></script>
  <script type='text/javascript' src="admin/jstools/jquery/plugins/grid/jquery.jqGrid.min.js"></script>
  <script type='text/javascript'><%= ServerJS %></script>
  <script type='text/javascript' src='js/default.js'></script>
</asp:Content>

<asp:Content ContentPlaceHolderId="MainContent" runat="server">

  <div id='intro'>
    <h1>KAVALKAD!</h1>
    <ul>
      <li>Sök via sökrutorna</li>
      <li>Sortera via kolumnrubrikerna</li>
      <li>Visa fler kolumner via <img src='gfx/cols.gif' alt='Kolumner' /></li>
      <li>Visa all info via <img src='gfx/info.gif' alt='Info' /></li>
      <li>Visa text via <img src='gfx/text.gif' alt='Text' /></li>
      <li>Visa noter via <img src='gfx/note.gif' alt='Not' /></li>
    </ul>
  </div>

  <div id='infopop' class='pop songpop'>
    <div class='toppop'>
      <asp:PlaceHolder id="EditBtns" runat="server">
        <div class='editbtn'>
          <span class='static'>
            <a href='javascript:void(0)' onclick="editSong()" onfocus='this.blur()' title='Redigera'><img src='gfx/edit.gif' alt='Redigera' border='0' /></a>
            <a href='javascript:void(0)' onclick="deleteSong()" onfocus='this.blur()' title='Radera kuplett'><img src='gfx/delete.png' alt='Ta bort' border='0' /></a>
          </span>
          <span class='edit'>
            <a href='javascript:void(0)' onclick="saveEditSong()" onfocus='this.blur()' title='Spara ändringar'><img src='gfx/save.gif' alt='Spara' border='0' /></a>
            <a href='javascript:void(0)' onclick="cancelEditSong()" onfocus='this.blur()' title='Avbryt'><img src='gfx/cancel.gif' alt='Avbryt' border='0' /></a>
          </span>
        </div>
      </asp:PlaceHolder>
      <div class='closebtn'><a href='javascript:void(0)' onclick="closeInfo()" onfocus='this.blur()' title='Stäng'><img src='gfx/closepop.gif' alt='Stäng' border='0' /></a></div>
      <div class='clearfloat'></div>
      <div id='songinfo'></div>
    </div>
  </div>

  <div id='newpop' class='pop songpop'>
    <div class='toppop'>
      <div class='editbtn'>
        <a href='javascript:void(0)' onclick="saveNewSong()" onfocus='this.blur()' title='Spara kuplett'><img src='gfx/save.gif' alt='Spara' border='0' /></a>
        <a href='javascript:void(0)' onclick="cancelNewSong()" onfocus='this.blur()' title='Avbryt'><img src='gfx/cancel.gif' alt='Avbryt' border='0' /></a>
      </div>
      <div class='closebtn'><a href='javascript:void(0)' onclick="closeNew()" onfocus='this.blur()' title='Stäng'><img src='gfx/closepop.gif' alt='Stäng' border='0' /></a></div>
      <div class='clearfloat'></div>
      <div id='newsonginfo'></div>
    </div>
  </div>

  <div id='poptext' class='pop'>
    <div class='toppop'>
      <div id='printbtn'><a href='javascript:void(0)' onclick="window.print()" onfocus='this.blur()' title='Skriv ut'><img src='gfx/print.gif' alt='Skriv ut' border='0' /></a></div>
      <div class='closebtn'><a href='javascript:void(0)' onclick="closeText()" onfocus='this.blur()' title='Stäng'><img src='gfx/closepop.gif' alt='Stäng' border='0' /></a></div>
      <div class='clearfloat'></div>
    </div>
    <div id='info'>
      <h2 id='i_title'></h2>
      <div id='i_spex'></div>
      <div class='clearfloat'></div>
      <div id='i_melody'></div>
    </div>
    <div id='textdiv'><NFN:AjaxControl Id="SongText" runat="server" /></div>
  </div>

  <asp:PlaceHolder id="AddLink" runat="server">
    <div id='addlinks'>
      <div id='addlinkdiv'><a href='javascript:addSong()' onfocus='this.blur()' title='Lägg till kuplett'><img src='gfx/add.gif' border='0' alt='' /></a></div>
      <div id='mediabank'><a href="javascript:void(0)" onfocus="this.blur()" onclick="window.open('/admin/DocumentBank/DocumentBank.aspx','_documentBank','left=0,top=0,width=1000,height=700,resizable=yes')"><img title="Öppna mediabank" alt="Öppna mediabank" src="admin/gfx/documentbank.gif"></a></div>
      <div class='clearfloat'></div>
    </div>
  </asp:PlaceHolder>
  <div id='textraddiv'>Textinnehåll: <input type='text' id='textrow' onkeydown='searchtext()' /></div>
  <div id='songgriddiv'>
    <table id="songgrid"></table>
    <div id="songctrl"></div>
  </div>
  <div id='cbupps'><label><input type='checkbox' onchange='toggleUpps()' />&nbsp;Inkludera uppsättningar</label></div>

  <asp:PlaceHolder id="SpexGrids" runat="server">
    <div id='spexgridsdiv'>
      <div id='addspexdiv'><a href='javascript:addSpex()' onfocus='this.blur()' title='Lägg till spex'><img src='gfx/add.gif' border='0' alt='' /></a></div>
      <div id='spexgriddiv'>
        <table id="spexgrid"></table>
        <div id="spexctrl"></div>
      </div>
    </div>
  </asp:PlaceHolder>

  <div id='logoutdiv'><a href='javascript:logout()' onfocus='this.blur()'>Logga ut</a></div>

</asp:Content>
