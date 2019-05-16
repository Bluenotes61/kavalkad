<%-- $Date: 2010-11-22 14:32:20 +0100 (mÃ¥, 22 nov 2010) $    $Revision: 7099 $ --%>
<%@ Page language="C#" MasterPageFile="admin.master" CodeFile="behind/roles.cs" Inherits="Roles" validateRequest="false"%>
<%@ Register Tagprefix="obgrd" Namespace="Obout.Grid" Assembly="obout_Grid_NET" %>

<asp:Content ContentPlaceHolderId="Header" runat="server">
  <link rel="stylesheet" href="css/roles.css" type="text/css">
  <script type='text/javascript' src="script/roles.js"></script>
</asp:Content>

<asp:Content ContentPlaceHolderId="ButtonBar" runat="server">
  <div id="StdButtons">
    <img id="btn_excel" src='gfx/excel.gif' alt="<%=Translate("Exportera lista") %>" title="<%=Translate("Exportera lista") %>" />
  </div>
</asp:Content>

<asp:Content ContentPlaceHolderId="SingleFrame" runat="server">

  <div id='popfield' onclick="$(this).slideUp()"></div>

  <obgrd:Grid id="ItemGrid"
    OnDeleteCommand = "DeleteRecord"
    OnUpdateCommand = "UpdateRecord"
    OnInsertCommand = "InsertRecord"
    runat="server"
  >
    <ClientSideEvents OnClientDblClick="onDoubleClick" OnClientEdit="onRowEdit" OnBeforeClientUpdate="onUpdate" OnBeforeClientInsert="onUpdate" OnClientCallbackError="onCallbackError" OnBeforeClientDelete="onDelete" />
    <Columns>
      <obgrd:Column Width="0" runat="server" />
      <obgrd:Column AllowEdit="true" AllowDelete="true" HeaderText="" Width="80px" runat="server" >
        <TemplateSettings TemplateId="editBtnTemplate" EditTemplateId="updateBtnTemplate" />
      </obgrd:Column>
      <obgrd:Column DataField="name" runat="server" />
      <obgrd:Column DataField="description" Wrap="true" runat="server" />
      <obgrd:Column DataField="users" ParseHTML="true" Width="300px" TemplateId="tplUsers" EditTemplateId="tplEUsers" runat="server" />
      <obgrd:Column DataField="permissions" ParseHTML="true" Width="550px" TemplateId="tplPermissions" EditTemplateId="tplEPermissions" runat="server" />
    </Columns>
    <Templates>

      <obgrd:GridTemplate runat="server" ID="editBtnTemplate"><Template>
        <input type='image' id='btnEdit' value="Edit" alt='<%= Translate("Redigera") %>' title='<%= Translate("Redigera") %>' src='gfx/edit.gif' onclick='ItemGrid.edit_record(this);this.blur();return false;' />&nbsp;&nbsp;
        <input type='image' id='btnDelete' value="Delete" alt='<%= Translate("Ta bort") %>' title='<%= Translate("Ta bort") %>' src='gfx/delete.gif' onclick='ItemGrid.delete_record(this);this.blur();return false;' />
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="updateBtnTemplate"><Template>
        <input type="image" id="btnUpdate" value="Update" alt='<%= Translate("Spara") %>' title='<%= Translate("Spara") %>' src='gfx/save.gif' onclick="ItemGrid.update_record(this);this.blur();return false;" />&nbsp;&nbsp;
        <input type="image" id="btnCancel" value="Cancel" alt='<%= Translate("Avbryt") %>' title='<%= Translate("Avbryt") %>' src='gfx/cancel.gif' onclick="ItemGrid.cancel_edit(this);this.blur();return false;" />
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplUsers"><Template>
        <%# GetFieldLink("Användare", Container.Value.ToString()) %>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplEUsers" ControlID="hdusers" ControlPropertyName="value" UseQuotes="true"><Template>
        <input type='hidden' id='hdusers' />
        <div id='editusers'></div>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplPermissions"><Template>
        <%# GetFieldLink("Behörigheter", Container.Value.ToString()) %>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplEPermissions" ControlID="hdperm" ControlPropertyName="value" UseQuotes="true"><Template>
        <input type='hidden' id='hdperm' />
        <div id='editperm'></div>
      </Template></obgrd:GridTemplate>

    </Templates>

  </obgrd:Grid>

  <div id="UserChkList" runat="server" style='display:none'></div>
  <div id="PermChkList" runat="server" style='display:none'></div>
  <script type="text/javascript">
    document.getElementById("editusers").innerHTML = document.getElementById("userlist").innerHTML;
    document.getElementById("userlist").innerHTML = "";
    document.getElementById("editperm").innerHTML = document.getElementById("permlist").innerHTML;
    document.getElementById("permlist").innerHTML = "";
  </script>

</asp:Content>
