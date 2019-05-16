<%-- $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ --%>
<%@ Page language="C#" MasterPageFile="admin.master" CodeFile="behind/adminlog.cs" Inherits="AdminLog" validateRequest=false%>
<%@ Register Tagprefix="obgrd" Namespace="Obout.Grid" Assembly="obout_Grid_NET" %>

<asp:Content ContentPlaceHolderId="Header" runat="server">
  <script type='text/javascript' src="script/adminlog.js"></script>
</asp:Content>

<asp:Content ContentPlaceHolderId="ButtonBar" runat="server">
  <div id="StdButtons">
    <img id="btn_excel" src='gfx/excel.gif' alt="<%=Translate("Exportera lista") %>" title="<%=Translate("Exportera lista") %>" />
  </div>
</asp:Content>

<asp:Content ContentPlaceHolderId="SingleFrame" runat="server">
  <obgrd:Grid id="ItemGrid" Serialize="false" runat="server">
    <Columns>
      <obgrd:Column DataField="id" Width="80px" runat="server" />
      <obgrd:Column DataField="time" Width="180px" runat="server" />
      <obgrd:Column DataField="user" Width="120px" runat="server" />
      <obgrd:Column DataField="event" Width="150px" runat="server" />
      <obgrd:Column DataField="webpage" ParseHtml="true" Width="150px" runat="server" />
      <obgrd:Column DataField="control" Width="150px" runat="server" />
    </Columns>
  </obgrd:Grid>
  <div id='recInfo'></div>
</asp:Content>
