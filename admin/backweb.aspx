<%-- $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ --%>
<%@ Page language="C#" MasterPageFile="admin.master" CodeFile="behind/backweb.cs" Inherits="Backweb" validateRequest=false%>
<%@ Register Tagprefix="obgrd" Namespace="Obout.Grid" Assembly="obout_Grid_NET" %>

<asp:Content ContentPlaceHolderId="Header" runat="server">
  <script type='text/javascript' src="script/backweb.js"></script>
</asp:Content>

<asp:Content ContentPlaceHolderId="ButtonBar" runat="server">
  <div id="StdButtons">
    <img id="btn_excel" src='gfx/excel.gif' alt="<%=Translate("Exportera lista") %>" title="<%=Translate("Exportera lista") %>" />
    <img id="btn_up" src='gfx/up_btn.gif' alt="<%=Translate("Flytta upp ett steg") %>" title="<%=Translate("Flytta upp ett steg") %>" />
    <img id="btn_down" src='gfx/down_btn.gif' alt="<%=Translate("Flytta ner ett steg") %>" title="<%=Translate("Flytta ner ett steg") %>" />
  </div>
</asp:Content>

<asp:Content ContentPlaceHolderId="SingleFrame" runat="server">

  <obgrd:Grid id="ItemGrid"
    OnDeleteCommand = "DeleteRecord"
    OnUpdateCommand = "UpdateRecord"
    OnInsertCommand = "InsertRecord"
    GenerateRecordIds = "true"
    runat="server"
  >
    <ClientSideEvents
      OnClientDblClick="onDoubleClick"
      OnClientEdit="onRowEdit"
      OnBeforeClientUpdate="onUpdate"
      OnBeforeClientInsert="onUpdate"
      OnClientCallbackError="onCallbackError"
      OnBeforeClientDelete="onDelete"
    />

    <Columns>
      <obgrd:Column Width="0" runat="server" />
      <obgrd:Column AllowEdit="true" AllowDelete="true" HeaderText="" Width="80px" runat="server" >
        <TemplateSettings TemplateId="editBtnTemplate" EditTemplateId="updateBtnTemplate" />
      </obgrd:Column>
      <obgrd:Column DataField="id" ReadOnly="true" Width="50px" runat="server" />
      <obgrd:Column DataField="name" Width="100px" runat="server" />
      <obgrd:Column DataField="text" Width="100px" runat="server" />
      <obgrd:Column DataField="description" Width="300px" runat="server" />
      <obgrd:Column DataField="nofrows" Width="80px" runat="server" />
      <obgrd:Column DataField="pageurl" Width="200px" runat="server" />
      <obgrd:Column DataField="permissions" ParseHTML="true" Width="300px" EditTemplateId="tplPerm" runat="server" />
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

      <obgrd:GridTemplate runat="server" ID="tplPerm" ControlID="hdperm" ControlPropertyName="value" UseQuotes="true">
        <Template>
          <input type='hidden' id='hdperm' />
          <div id='editperm'></div>
        </Template>
      </obgrd:GridTemplate>

    </Templates>

  </obgrd:Grid>

  <div id="PermEditTmpl" runat="server" style='display:none' /></div>
  <script type="text/javascript">
    document.getElementById("editperm").innerHTML = document.getElementById("editpermtmpl").innerHTML;
    document.getElementById("editpermtmpl").innerHTML = "";
  </script>

</asp:Content>

