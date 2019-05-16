<%-- $Date: 2010-11-22 14:32:20 +0100 (mÃ¥, 22 nov 2010) $    $Revision: 7099 $ --%>
<%@ Page language="C#" MasterPageFile="admin.master" CodeFile="behind/users.cs" Inherits="Users" validateRequest=false%>
<%@ Register Tagprefix="obgrd" Namespace="Obout.Grid" Assembly="obout_Grid_NET" %>

<asp:Content ContentPlaceHolderId="Header" runat="server">
  <script type='text/javascript' src="script/users.js"></script>
</asp:Content>

<asp:Content ContentPlaceHolderId="ButtonBar" runat="server">
  <div id="StdButtons">
    <img id="btn_excel" src='gfx/excel.gif' alt="<%=Translate("Exportera lista") %>" title="<%=Translate("Exportera lista") %>" />
    <span id='colbtns' style='display:<%= HasMoreAttribs ? "inline" : "none" %>'>
      <img id="btn_morecols" src='gfx/more.png' alt="<%=Translate("Fler kolumner") %>" title="<%=Translate("Fler kolumner") %>" />
      <img id="btn_lesscols" style='display:none' src='gfx/less.png' alt="<%=Translate("Färre kolumner") %>" title="<%=Translate("Färre kolumner") %>" />
    </span>
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
      <obgrd:Column DataField="usertype" EditTemplateId="tplUserType" Width="120px" runat="server" />
      <obgrd:Column DataField="username" Width="250px" runat="server" />
      <obgrd:Column DataField="password" Width="150px" runat="server" />
      <obgrd:Column DataField="email" Width="250px" runat="server" />
      <obgrd:Column DataField="description" Width="200px" runat="server" />
      <obgrd:Column DataField="approved" Width="80px" ShowFilterCriterias="false" FilterTemplateId="filterYN" TemplateId="tplYN" EditTemplateId="tplChb" runat="server" />
      <obgrd:Column DataField="roles" Width="120px" ParseHTML="true" EditTemplateId="tplRoles" runat="server" />
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

      <obgrd:GridTemplate runat="server" ID="tplUserType" ControlID="UT" ControlPropertyName="value"><Template>
        <select id="UT" class="ob_gEC">
          <option value="Normal"><%= Translate("Normal") %></option>
          <option value="Administrator"><%= Translate("Administratör") %></option>
        </select>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" id="tplYN"><Template>
        <%# Convert.ToBoolean(Container.Value) ? Translate("Ja") : Translate("Nej") %>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplChb" ControlID="chkApproved" ControlPropertyName="checked" UseQuotes="false"><Template>
        <input type='checkbox' id="chkApproved" value="" />
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplRoles" ControlID="hdroles" ControlPropertyName="value" UseQuotes="true"><Template>
        <input type='hidden' id='hdroles' />
        <div id='editroles'></div>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="filterYN" ControlID="ddYN" ControlPropertyName="value"><Template>
        <select id="ddYN" class="ob_gEC"><option value=""></option><option value="true"><%=Translate("Ja")%></option><option value="false"><%=Translate("Nej")%></option></select>
      </Template></obgrd:GridTemplate>
    </Templates>

  </obgrd:Grid>

  <div id="RoleChkList" runat="server" style='display:none' /></div>
  <script type="text/javascript">
    document.getElementById("editroles").innerHTML = document.getElementById("rolelist").innerHTML;
    document.getElementById("rolelist").innerHTML = "";
  </script>

</asp:Content>

