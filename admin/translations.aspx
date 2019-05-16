<%-- $Date: 2011-03-17 14:09:04 +0100 (to, 17 mar 2011) $    $Revision: 7496 $ --%>
<%@ Page language="C#" MasterPageFile="admin.master" CodeFile="behind/translations.cs" Inherits="Translations" validateRequest=false EnableEventValidation="false" %>
<%@ Register Tagprefix="obgrd" Namespace="Obout.Grid" Assembly="obout_Grid_NET" %>

<asp:Content ContentPlaceHolderId="Header" runat="server">
  <script type="text/javascript">var currLang = '<%= Cms.Language %>';</script>
  <script type='text/javascript' src="script/translations.js"></script>
</asp:Content>

<asp:Content ContentPlaceHolderId="ButtonBar" runat="server">
  <div id="StdButtons">
    <img id="btn_excel" src='gfx/excel.gif' alt="<%=Translate("Exportera lista") %>" title="<%=Translate("Exportera lista") %>" />&nbsp;&nbsp;
    <asp:DropDownList id='SelLangDD' AutoPostBack="True" OnSelectedIndexChanged="LangChanged" runat="server" />
  </div>
</asp:Content>

<asp:Content ContentPlaceHolderId="SingleFrame" runat="server">

  <obgrd:Grid id="ItemGrid"
    OnDeleteCommand = "DeleteRecord"
    OnUpdateCommand = "UpdateRecord"
    OnInsertCommand = "InsertRecord"
    runat="server"
  >
    <ClientSideEvents
      OnClientDblClick = "onDoubleClick"
      OnClientCallbackError = "onCallbackError"
      OnBeforeClientDelete = "onDelete"
    />
    <Columns>
      <obgrd:Column Width="0" runat="server" />
      <obgrd:Column AllowEdit="true" AllowDelete="true" HeaderText="" Width="80px" runat="server" >
        <TemplateSettings TemplateId="editBtnTemplate" EditTemplateId="updateBtnTemplate" />
      </obgrd:Column>
      <obgrd:Column DataField="sv" Width="400px" runat="server" />
    </Columns>
    <Templates>

      <obgrd:GridTemplate runat="server" ID="editBtnTemplate">
        <Template>
          <input type='image' id='btnEdit' value="Edit" alt='<%= Translate("Redigera") %>' title='<%= Translate("Redigera") %>' src='gfx/edit.gif' onclick='ItemGrid.edit_record(this);this.blur();return false;' />&nbsp;&nbsp;
          <input type='image' id='btnDelete' value="Delete" alt='<%= Translate("Ta bort") %>' title='<%= Translate("Ta bort") %>' src='gfx/delete.gif' onclick='ItemGrid.delete_record(this);this.blur();return false;' />
        </Template>
      </obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="updateBtnTemplate">
        <Template>
          <input type="image" id="btnUpdate" value="Update" alt='<%= Translate("Spara") %>' title='<%= Translate("Spara") %>' src='gfx/save.gif' onclick="ItemGrid.update_record(this);this.blur();return false;" />&nbsp;&nbsp;
          <input type="image" id="btnCancel" value="Cancel" alt='<%= Translate("Avbryt") %>' title='<%= Translate("Avbryt") %>' src='gfx/cancel.gif' onclick="ItemGrid.cancel_edit(this);this.blur();return false;" />
        </Template>
      </obgrd:GridTemplate>

    </Templates>

  </obgrd:Grid>

</asp:Content>
