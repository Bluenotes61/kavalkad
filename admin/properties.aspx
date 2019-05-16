<%-- $Date: 2010-11-22 14:32:20 +0100 (mÃ¥, 22 nov 2010) $    $Revision: 7099 $ --%>
<%@ Page language="C#" MasterPageFile="admin.master" CodeFile="behind/properties.cs" Inherits="Properties" validateRequest=false%>

<asp:Content ContentPlaceHolderId="Header" runat="server">
  <script language="javascript">
    var currid = '<%=ThisId%>';
  </script>
  <script src="script/properties.js" type="text/javascript"></script>
</asp:Content>

<asp:Content ContentPlaceHolderId="ButtonBar" runat="server">
  <div id="StdButtons">
    <ul>
      <li><img id="btn_edit" src='gfx/edit_btn.gif' alt="<%=Translate("Redigera aktuell informationspost") %>" title="<%=Translate("Redigera aktuell informationspost") %>" /></li>
      <li><img id="btn_delete" src='gfx/delete_btn.gif' alt="<%=Translate("Ta bort aktuell informationspost") %>" title="<%=Translate("Ta bort aktuell informationspost") %>" /></li>
      <li><img id="btn_up" src='gfx/up_btn.gif' alt="<%=Translate("Flytta upp ett steg") %>" title="<%=Translate("Flytta upp ett steg") %>" /></li>
      <li><img id="btn_down" src='gfx/down_btn.gif' alt="<%=Translate("Flytta ner ett steg") %>" title="<%=Translate("Flytta ner ett steg") %>" /></li>
      <li><span id="UnlockBtn" style="display:none"><img id="btn_unlock" src='gfx/unlock_btn.gif' alt="<%=Translate("Ta bort låset från informationsposten") %>" title="<%=Translate("Ta bort låset från informationsposten") %>" /></span></li>
    </ul>
  </div>
  <div id="SaveEditButtons" style="display:none">
    <ul>
      <li><img id="btn_save" src='gfx/save_btn.gif' alt="<%=Translate("Spara ändringar") %>" title="<%=Translate("Spara ändringar") %>" /></li>
      <li><img id="btn_cancel" src='gfx/cancel_btn.gif' alt="<%=Translate("Ångra ändringar") %>" title="<%=Translate("Ångra ändringar") %>" /></li>
    </ul>
  </div>
</asp:Content>

<asp:Content ContentPlaceHolderId="LeftFrame" runat="server">
  <div id="TreeDiv"><asp:Literal id="PropTree" runat="server" /></div>
</asp:Content>


<asp:Content ContentPlaceHolderId="RightFrame" runat="server">

  <div id="PropInfo" style="display:block">
    <div id="PropInfoDiv">
      <table cellspacing='0' cellpadding='0' class='itemtable'>
        <tr><td class='hl' title='<%= Translate("Informationspostens identifierare") %>'><%= Translate("Id") %>:</td><td class="value"><span id="PropIdV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Informationspostens namn") %>'><%= Translate("Namn") %>:</td><td class="value"><span id="PropNameV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Informationspostens förälder") %>'><%= Translate("Förälder") %>:</td><td class="value"><span id="PropParentV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Typ av kontroll som posten är kopplad till") %>'><%= Translate("Kontrolltyp") %>:</td><td class="value"><span id="PropCtrlTypeV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Posten sortetingsnummer") %>'><%= Translate("Ordningsnr") %>:</td><td class="value"><span id="PropOrderV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Om innehållet i posten visas på sidan") %>'><%= Translate("Visas") %>:</td><td class="value"><span id="PropVisibleV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Om innehållet i posten är publicerat") %>'><%= Translate("Publicerad") %>:</td><td class="value"><span id="PropPublishedV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Datum för senaste ändring") %>'><%= Translate("Ändringsdatum") %>:</td><td class="value"><span id="PropModDateV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Administratör som senast ändrade posten") %>'><%= Translate("Ändrad av") %>:</td><td class="value"><span id="PropModByV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Datum för senaste publiceringen") %>'><%= Translate("Publiceringsdatum") %>:</td><td class="value"><span id="PropPublishedDateV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Administratör som senast publicerade posten") %>'><%= Translate("Publicerad av") %>:</td><td class="value"><span id="PropPublishedByV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Startdatum för visning av postens innehåll") %>'><%= Translate("Start") %>:</td><td class="value"><span id="PropStartV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Slutdatum för visning av postens innehåll") %>'><%= Translate("Slut") %>:</td><td class="value"><span id="PropEndV"></span>&nbsp;</td></tr>
        <tr><td class='hl' title='<%= Translate("Postens opublicerade informationsinnehåll") %>'><%= Translate("Opublicerat värde") %>:</td><td class="value"><div style="width=400px;height:100px;overflow:auto"><span id="PropPrelimValueV"></span>&nbsp;</div></td></tr>
        <tr><td class='hl' title='<%= Translate("Postens publicerade informationsinnehåll") %>'><%= Translate("Publicerat värde") %>:</td><td class="value"><div style="width=400px;height:100px;overflow:auto"><span id="PropValueV"></span>&nbsp;</div></td></tr>
        <tr><td class='hl' title='<%= Translate("Webbsidor som innehåller informationsposten") %>'><%= Translate("Webbsidor") %>:</td><td class="value">
          <asp:Repeater id="PageList" runat="server">
            <ItemTemplate>
              <div><a href='<%# "pages.aspx?pageid=" + DataBinder.Eval(Container.DataItem, "Id") %>' title='<%# Translate("Administrera webbsida") %>'><%# DataBinder.Eval(Container.DataItem, "Id") %></a>&nbsp;-&nbsp;<%# DataBinder.Eval(Container.DataItem, "name") %></div>
            </ItemTemplate>
          </asp:Repeater>
        </td></tr>
      </table>
    </div>
  </div>


  <div id="PropEdit" style="display:none">
    <div id="EditPropError" class="error" style="display:none"></div>

    <table cellspacing="0" cellpadding="3" class='itemtable'>
      <tr><td class='hl' title='<%= Translate("Informationspostens identifierare") %>'><%= Translate("Id") %>:</td><td class="value"><span id="PropIdE"></span>&nbsp;</td></tr>
      <tr><td class='hl' title='<%= Translate("Informationspostens namn") %>'><%= Translate("Namn") %>:</td><td class="value"><input type="text" id="PropNameE"/></td></tr>
      <tr><td class='hl' title='<%= Translate("Typ av kontroll som posten är kopplad till") %>'><%= Translate("Kontrolltyp") %>:</td><td class="value"><span id="PropCtrlTypeE"></span>&nbsp;</td></tr>
      <tr><td class='hl' title='<%= Translate("Posten sortetingsnummer") %>'><%= Translate("Ordningsnr") %>:</td><td class="value"><input type="text" id="PropOrderE" size="3"/></td></tr>
      <tr><td class='hl' title='<%= Translate("Om innehållet i posten visas på sidan") %>'><%= Translate("Visas") %>:</td><td class="value"><input type="checkbox" id="PropVisibleE" /></td></tr>
      <tr><td class='hl' title='<%= Translate("Om innehållet i posten är publicerat") %>'><%= Translate("Publicerad") %>:</td><td class="value"><input type="checkbox" id="PropPublishedE"/></td></tr>
      <tr><td class='hl' title='<%= Translate("Datum för senaste ändring") %>'><%= Translate("Ändrad") %>:</td><td class="value"><span id="PropModDateE"></span>&nbsp;</td></tr>
      <tr><td class='hl' title='<%= Translate("Administratör som senast ändrade posten") %>'><%= Translate("Ändrad av") %>:</td><td class="value"><span id="PropModByE"></span>&nbsp;</td></tr>
      <tr><td class='hl' title='<%= Translate("Datum för senaste publiceringen") %>'><%= Translate("Publicerad") %>:</td><td class="value"><span id="PropPublishedDateE"></span>&nbsp;</td></tr>
      <tr><td class='hl' title='<%= Translate("Administratör som senast publicerade posten") %>'><%= Translate("Publicerad av") %>:</td><td class="value"><span id="PropPublishedByE"></span>&nbsp;</td></tr>
      <tr><td class='hl' title='<%= Translate("Startdatum för visning av postens innehåll") %>'><%= Translate("Start") %>:</td><td class="value"><input type="text" id="PropStartE"></td></tr>
      <tr><td class='hl' title='<%= Translate("Slutdatum för visning av postens innehåll") %>'><%= Translate("Slut") %>:</td><td class="value"><input type="text" id="PropEndE"/></td></tr>
      <tr><td class='hl' title='<%= Translate("Postens opublicerade informationsinnehåll") %>'><%= Translate("Opublicerat värde") %>:</td><td class="value"><textarea id="PropPrelimValueE" rows='5' cols="120"></textarea></td></tr>
      <tr><td class='hl' title='<%= Translate("Postens publicerade informationsinnehåll") %>'><%= Translate("Publicerat värde") %>:</td><td class="value"><textarea id="PropValueE" rows='5' cols="120"></textarea></td></tr>
    </table>
  </div>

  <div id="PropNew" style="display:none">
    <div id="NewPropError" class="error" style="display:none"></div>
  </div>

  <script language='javascript'>ob_SelectedId('<%=ThisId%>')</script>

</asp:Content>
