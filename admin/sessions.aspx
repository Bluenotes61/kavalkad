<%-- $Date: 2009-10-19 04:06:44 +0200 (må, 19 okt 2009) $    $Revision: 5516 $ --%>
<%@ Page language="C#" MasterPageFile="admin.master" CodeFile="behind/sessions.cs" Inherits="Sessions" validateRequest=false%>
<%@ Register Tagprefix="obgrd" Namespace="Obout.Grid" Assembly="obout_Grid_NET" %>

<asp:Content ContentPlaceHolderId="Header" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderId="ButtonBar" runat="server">
  <div id="StdButtons">
    <img id="btn_excel" src='gfx/excel.gif' alt="<%=Translate("Exportera lista") %>" title="<%=Translate("Exportera lista") %>" />
  </div>
</asp:Content>

<asp:Content ContentPlaceHolderId="SingleFrame" runat="server">

  <obgrd:Grid id="ItemGrid" runat="server" >
    <Columns>
      <obgrd:Column DataField="sessionid" Visible="false" runat="server" />
      <obgrd:Column DataField="connecttime" Width="140px" runat="server" />
      <obgrd:Column DataField="lastactiontime" Width="140px" runat="server" />
      <obgrd:Column DataField="ip" Width="100px" runat="server" />
      <obgrd:Column DataField="dns" Width="200px" runat="server" />
      <obgrd:Column DataField="referrer" Width="300px" runat="server" />
      <obgrd:Column DataField="useragent" Width="300px" Wrap="true" runat="server" />
    </Columns>
  </obgrd:Grid>

</asp:Content>

