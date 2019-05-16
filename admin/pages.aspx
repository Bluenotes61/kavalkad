<%-- $Date: 2010-11-22 14:32:20 +0100 (mÃ¥, 22 nov 2010) $    $Revision: 7099 $ --%>
<%@ Page language="C#" MasterPageFile="admin.master" CodeFile="behind/pages.cs" Inherits="Pages" validateRequest=false%>

<asp:Content ContentPlaceHolderId="Header" runat="server">
  <script type="text/javascript">
    var currid = '<%=ThisId%>';
  </script>
  <link rel="stylesheet" type="text/css" href='/admin/jstools/jquery/plugins/treeview/jquery.treeviewpages.css'>
  <link rel="stylesheet" type="text/css" href='/admin/jstools/datetimepicker/skins/tiger/theme.css'>
  <script type="text/javascript" src="/admin/jstools/datetimepicker/calendar.js"></script>
  <script type="text/javascript" src="/admin/jstools/datetimepicker/lang/calendar-sv.js"></script>
  <script type="text/javascript" src="/admin/jstools/jquery/plugins/treeview/jquery.treeview.js"></script>
  <script src="script/pages.js" type="text/javascript"></script>
</asp:Content>

<asp:Content ContentPlaceHolderId="ButtonBar" runat="server">

  <div id="StdButtons">
    <ul>
      <li><img id="btn_gotopage" src='gfx/page_btn.gif' alt="<%=Translate("Gå till aktuell webbsida") %>" title="<%=Translate("Gå till aktuell webbsida") %>" /></li>
      <li><img id="btn_newpage" src='gfx/new_btn.gif' alt="<%=Translate("Skapa ny webbsida") %>" title="<%=Translate("Skapa ny webbsida") %>" /></li>
      <li><img id="btn_editpage" src='gfx/edit_btn.gif' alt="<%=Translate("Redigera aktuell webbsida") %>" title="<%=Translate("Redigera aktuell webbsida") %>" /></li>
      <li><img id="btn_delpage" src='gfx/delete_btn.gif' alt="<%=Translate("Ta bort aktuell webbsida") %>" title="<%=Translate("Ta bort aktuell webbsida") %>" /></li>
      <li><img id="btn_uppage" src='gfx/up_btn.gif' alt="<%=Translate("Flytta sidan uppåt") %>" title="<%=Translate("Flytta sidan uppåt") %>" /></li>
      <li><img id="btn_downpage" src='gfx/down_btn.gif' alt="<%=Translate("Flytta sidan nedåt") %>" title="<%=Translate("Flytta sidan nedåt") %>" /></li>
      <asp:PlaceHolder ID="BtnRestorePage" runat="server"></asp:PlaceHolder><li><img id="btn_restorepage" src='gfx/restore_btn.gif' alt="<%=Translate("Återställ sidan") %>" title="<%=Translate("Återställ sidan") %>" /></li></asp:PlaceHolder>
      <asp:PlaceHolder ID="BtnUndeletePage" runat="server"><li><img id="btn_undelpage" src='gfx/recycle_btn.gif' alt="<%=Translate("Återställ borttagna sidor") %>" title="<%=Translate("Återställ borttagna sidor") %>" /></li></asp:PlaceHolder>
      <li><span id="UnlockBtn" style="display:none"><img id="btn_unlockpage" src='gfx/unlock_btn.gif' alt="<%=Translate("Ta bort lås från underliggande informationsposter") %>" title="<%=Translate("Ta bort lås från underliggande informationsposter") %>" /></span></li>
    </ul>
  </div>
  <div id="SaveNewButtons" style="display:none">
    <ul>
      <li><img id="btn_savenew" src='gfx/save_btn.gif' alt="<%=Translate("Spara ny webbsida") %>" title="<%=Translate("Spara ny webbsida") %>" /></li>
      <li><img id="btn_cancelnew" src='gfx/cancel_btn.gif' alt="<%=Translate("Ångra ny webbsida") %>" title="<%=Translate("Ångra ny webbsida") %>" /></li>
    </ul>
  </div>
  <div id="SaveEditButtons" style="display:none">
    <ul>
      <li><img id="btn_saveedit" src='gfx/save_btn.gif' alt="<%=Translate("Spara ändringar") %>" title="<%=Translate("Spara ändringar") %>" /></li>
      <li><img id="btn_canceledit" src='gfx/cancel_btn.gif' alt="<%=Translate("Ångra ändringar") %>" title="<%=Translate("Ångra ändringar") %>" /></li>
    </ul>
  </div>

  <div id="restoreDiv" onmouseout="this.style.display='none'"></div>
  <div id="undelDiv" onmouseout="this.style.display='none'"></div>
</asp:Content>

<asp:Content ContentPlaceHolderId="LeftFrame" runat="server">
  <div id="TreeDiv"><asp:Literal id="PageTree" runat="server" /></div>
</asp:Content>


<asp:Content ContentPlaceHolderId="RightFrame" runat="server">

  <div id="PageInfo" style="display:block">
    <div id="PageInfoDiv">
      <table cellspacing='0' cellpadding='0' class='itemtable'>
        <tr><td class='hl' title='<%= Translate("Sidans identifierare") %>'>Id:</td><td class='value'><span id="PageIdV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Sidans namn (visas i menyer)") %>'><%= Translate("Namn") %>:</td><td class='value'><span id="PageNameV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Mall som definierar sidans layout") %>'><%= Translate("Sidmall") %>:</td><td class='value'><span id="PageTemplateV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Om sidans åtkomst skyddas av behörigheter") %>'><%= Translate("Skyddad") %>:</td><td class='value'><span id="PageProtectV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Sidans visningsstatus") %>'><%= Translate("Status") %>:</td><td class='value'><span id="PageStatusV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Tillgängliga språk") %>'><%= Translate("Språk") %>:</td><td class='value'><span id="PageLangV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Sidans titel") %>'><%= Translate("Titel") %>:</td><td class='value'><span id="PageTitleV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Sidans nyckelord (meta-tag)") %>'><%= Translate("Nyckelord") %>:</td><td  class='value'><span id="PageKeysV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Sidans beskrivning (meta-tag)") %>'><%= Translate("Beskrivning") %>:</td><td  class='value'><span id="PageDescriptV"></span>&nbsp;</td></tr>
        <tr><td class='sep' colspan='2'><hr /></td></tr>
        <tr><td class='hl' title='<%= Translate("Tillåt snabbskapande av undersidor") %>'><%= Translate("Snabbskapa undersidor") %>:</td><td class='value'><span id="QuickCreateV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Tidsstyrning") %>'><%= Translate("Tidsstyrning") %>:</td><td  class='value'><span id="PageTimeControlV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Sidor med vilka denna sida delar information") %>'><%= Translate("Delade sidor") %>:</td><td class='value'><span id="PageSharedV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Sidor relaterade till denna sida") %>'><%= Translate("Relaterade sidor") %>:</td><td class='value'><span id="PageRelatedV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Omdirigera till första undersidan") %>'><%= Translate("Omdirigera till undersida") %>:</td><td class='value'><span id="RedirToChildV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Sida att omdirigera till") %>'><%= Translate("Omdirigera till sida") %>:</td><td class='value'><span id="PageRedirectV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Specifika behörigheter för sidan") %>'><%= Translate("Behörigheter") %>:</td><td  class='value'><span id="PagePermissionsV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Informationsinnehåll unikt för denna sida") %>'><%= Translate("Informationsinnehåll") %>:</td><td class='value'><span id="PagePropertyV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Informationsinnehåll som kan delas med andra sidor") %>'><%= Translate("Delat informationsinnehåll") %>:</td><td class='value'><span id="PageSharedPropertyV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Datum för senaste ändring") %>'><%= Translate("Ändringsdatum") %>:</td><td class='value'><span id="PageModDateV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Administratör som senast ändrade sidan") %>'><%= Translate("Ändrad av") %>:</td><td class='value'><span id="PageModByV"></span>&nbsp;</td></tr>
      </table>
    </div>
  </div>

  <div id="PageNew" style="display:none">
    <div id="NewPageError" class="error" style="display:none"></div>

    <table cellspacing="0" cellpadding="0" class='itemtable'>
      <tr><td class='hl' title='<%= Translate("Sidans identifierare") %>'>Id:</td><td class='editvalue'><input type="text" id="PageIdN" size="60" /></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidans namn (visas i menyer)") %>'><%= Translate("Namn") %>:</td><td class="editvalue"><input type="text" id="PageNameN" size="60" /></td></tr>
      <tr><td class='hl' title='<%= Translate("Mall som definierar sidans layout") %>'><%= Translate("Sidmall") %>:</td><td class="editvalue"><asp:Literal id="PageTemplateN" runat="server"/><input type="hidden" id="FileNameN" /></td></tr>
      <tr><td class='hl' title='<%= Translate("Om sidans åtkomst skyddas av behörigheter") %>'><%= Translate("Skyddad") %>:</td><td class="editvalue"><select id="PageProtectN"><option value="Y"><%= Translate("Ja") %></option><option value="N" selected><%= Translate("Nej") %></option><option value="I"><%= Translate("Ärvd") %></option></select></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidans visningsstatus") %>'><%= Translate("Status") %>:</td><td class="editvalue"><select id="PageStatusN"><option value="active"><%= Translate("Aktiv") %></option><option value="hidden" selected><%= Translate("Dold") %></option><option value="inactive"><%= Translate("Inaktiv") %></option></select></td></tr>
      <tr><td class='hl' title='<%= Translate("Tillgängliga språk") %>'><%= Translate("Språk") %>:</td><td class="editvalue"><asp:Literal id="ChkLangN" runat="server" /></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidans titel") %>'><%= Translate("Titel") %>:</td><td class="editvalue"><input type="text" id="PageTitleN" size="60" /></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidans nyckelord (meta-tag)") %>'><%= Translate("Nyckelord") %>:</td><td class="editvalue"><input type="text" id="PageKeysN" size="60" /></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidans beskrivning (meta-tag)") %>'><%= Translate("Beskrivning") %>:</td><td class="editvalue"><textarea id="PageDescriptN" rows='5' cols="60" /></textarea></td></tr>
      <tr><td class='sep' colspan='2'><hr /></td></tr>
      <tr><td class='hl' title='<%= Translate("Tillåt snabbskapande av undersidor") %>'><%= Translate("Snabbskapa undersidor") %>:</td><td class="editvalue"><input type="checkbox" checked id="QuickCreateN"/></td></tr>
      <tr><td class='hl' title='<%= Translate("Tidsstyrning") %>'><%= Translate("Tidsstyrning") %>:</td><td class="editvalue"><input type="checkbox" id="PageTimeControlN" /><br /><%=Translate("Starttid")%>:&nbsp;<input id='PageTimeControlStartN' type='hidden' /><a id='EPageTimeControlStartN' class="ob_gAL" href='javascript:void(0)' onClick='show_cal(this)' onfocus='this.blur()'><%=Translate("Välj datum")%></a><br /><%=Translate("Sluttid")%>:&nbsp;<input id='PageTimeControlEndN' type='hidden' /><a id='EPageTimeControlEndN' class="ob_gAL" href='javascript:void(0)' onClick='show_cal(this)' onfocus='this.blur()'><%=Translate("Välj datum")%></a></td></tr>
      <tr><td class='hl' title='<%= Translate("Omdirigera till första undersidan") %>'><%= Translate("Omdirigera till undersida") %>:</td><td class="editvalue"><input type="checkbox" checked id="RedirToChildN"/></td></tr>
      <tr><td class='hl' title='<%= Translate("Sida att omdirigera till") %>'><%= Translate("Omdirigera till sida") %>:</td><td class="editvalue"><input type="text" id="PageRedirectN" size="60"/></td></tr>
    </table>
  </div>

  <div id="PageEdit" style="display:none">
    <div id="EditPageError" class="error" style="display:none"></div>

    <table cellspacing="0" cellpadding="3" class='itemtable'>
      <tr><td class='hl' title='<%= Translate("Sidans identifierare") %>'>Id:</td><td class="value"><span id="PageIdE"></span></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidans namn (visas i menyer)") %>'><%= Translate("Namn") %>:</td><td class="editvalue"><input type="text" id="PageNameE" size='60'/></td></tr>
      <tr><td class='hl' title='<%= Translate("Mall som definierar sidans layout") %>'><%= Translate("Sidmall") %>:</td><td class="editvalue"><div id="PageTmplDDE"></div><input type="hidden" id="PageTemplateE" size="60"/></td></tr>
      <tr><td class='hl' title='<%= Translate("Om sidans åtkomst skyddas av behörigheter") %>'><%= Translate("Skyddad") %>:</td><td class="editvalue"><select id="PageProtectE"><option value="Y"><%= Translate("Ja") %></option><option value="N"><%= Translate("Nej") %></option><option value="I"><%= Translate("Ärvd") %></option></select></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidans visningsstatus") %>'><%= Translate("Status") %>:</td><td class="editvalue"><select id="PageStatusE"><option value="active"><%= Translate("Aktiv") %></option><option value="hidden"><%= Translate("Dold") %></option><option value="inactive"><%= Translate("Inaktiv") %></option></select></td></tr>
      <tr><td class='hl' title='<%= Translate("Tillgängliga språk") %>'><%= Translate("Språk") %>:</td><td class="editvalue"><asp:Literal id="ChkLangE" runat="server" /></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidans titel") %>'><%= Translate("Titel") %>:</td><td class="editvalue"><input type="text" id="PageTitleE" size='60'/></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidans nyckelord (meta-tag)") %>'><%= Translate("Nyckelord") %>:</td><td class="editvalue"><input type="text" id="PageKeysE" size='60'/></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidans beskrivning (meta-tag)") %>'><%= Translate("Beskrivning") %>:</td><td class="editvalue"><textarea id="PageDescriptE" rows='5' cols="60" /></textarea></td></tr>
      <tr><td class='sep' colspan='2'><hr /></td></tr>
      <tr><td class='hl' title='<%= Translate("Tillåt snabbskapande av undersidor") %>'><%= Translate("Snabbskapa undersidor") %>:</td><td class="editvalue"><input type="checkbox" id="QuickCreateE"/></td></tr>
      <tr><td class='hl' title='<%= Translate("Tidsstyrning") %>'><%= Translate("Tidsstyrning") %>:</td><td class="editvalue"><input type="checkbox" id="PageTimeControlE" /><br /><%=Translate("Starttid")%>:&nbsp;<input id='PageTimeControlStartE' type='hidden' /><a id='EPageTimeControlStartE' class="ob_gAL" href='javascript:void(0)' onClick='show_cal(this)' onfocus='this.blur()'></a><br /><%=Translate("Sluttid")%>:&nbsp;<input id='PageTimeControlEndE' type='hidden' /><a id='EPageTimeControlEndE' class="ob_gAL" href='javascript:void(0)' onClick='show_cal(this)' onfocus='this.blur()'></a></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidor med vilka denna sida delar information") %>'><%= Translate("Dela info från") %>:</td><td class="value"><div style="overflow:auto"><div id="PageSharedE"></div></td></tr>
      <tr><td class='hl' title='<%= Translate("Sidor relaterade till denna sida") %>'><%= Translate("Relatera till") %>:</td><td class="value"><div style="overflow:auto"><div id="PageRelatedE"></div></td></tr>
      <tr><td class='hl' title='<%= Translate("Omdirigera till första undersidan") %>'><%= Translate("Omdirigera till undersida") %>:</td><td class="editvalue"><input type="checkbox" id="RedirToChildE"/></td></tr>
      <tr><td class='hl' title='<%= Translate("Sida att omdirigera till") %>'><%= Translate("Omdirigera till sida") %>:</td><td class="editvalue"><input type="text" id="PageRedirectE" size="60"/></td></tr>
      <tr><td class='hl' title='<%= Translate("Specifika behörigheter för sidan") %>'><%= Translate("Behörigheter") %>:</td><td class="value"><h3><%= Translate("Roll") %></h3><div><asp:DropDownList id="PermissionRoles" onChange="permissionRoleChanged(this);" runat="server"/><div id="PermissionDivE" style="margin-top:10px" runat="server"></div></td></tr>
      <tr><td class='hl' title='<%= Translate("Informationsinnehåll unikt för denna sida") %>'><%= Translate("Informationsinnehåll") %>:</td><td class="value"><span id="PagePropertyE"></span></td></tr>
      <tr><td class='hl' title='<%= Translate("Informationsinnehåll som kan delas med andra sidor") %>'><%= Translate("Delat informationsinnehåll") %>:</td><td class="value"><span id="PageSharedPropertyE" ></span></td></tr>
      <tr><td class='hl' title='<%= Translate("Datum för senaste ändring") %>'><%= Translate("Ändringsdatum") %>:</td><td class="value"><span id="PageModDateE"></span></td></tr>
      <tr><td class='hl' title='<%= Translate("Administratör som senast ändrade sidan") %>'><%= Translate("Ändrad av") %>:</td><td class="value"><span id="PageModByE"></span></td></tr>
    </table>
  </div>

  <!--  <script language='javascript'>ob_SelectedId('hem');</script>-->

</asp:Content>
