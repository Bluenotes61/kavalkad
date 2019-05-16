<%-- $Date: 2010-11-22 14:32:20 +0100 (mÃ¥, 22 nov 2010) $    $Revision: 7099 $ --%>
<%@ Page language="C#" MasterPageFile="admin.master" CodeFile="behind/news.cs" Inherits="NewsAdmin" validateRequest=false%>
<%@ Register Tagprefix="obgrd" Namespace="Obout.Grid" Assembly="obout_Grid_NET" %>

<asp:Content ContentPlaceHolderId="Header" runat="server">
  <link rel="stylesheet" media="screen, print" type="text/css" href="../css/base_bw.css">
  <link rel="stylesheet" type="text/css" href='jstools/datetimepicker/skins/tiger/theme.css'>
  <script type="text/javascript" src="jstools/datetimepicker/calendar.js"></script>
  <script type="text/javascript" src="jstools/datetimepicker/lang/calendar-sv.js"></script>
  <script type='text/javascript' src='tinymce/tiny_mce.js'></script>
  <script type='text/javascript' src='tinymce/jquery.tinymce.js'></script>
  <script type='text/javascript'><asp:Literal id="TinyHelper" runat="server" /></script>
  <script type='text/javascript' src="jstools/extensions/date.js"></script>
  <script type='text/javascript' src="script/news.js"></script>
</asp:Content>

<asp:Content ContentPlaceHolderId="ButtonBar" runat="server">
  <div id="StdButtons">
    <img id="btn_excel" src='gfx/excel.gif' alt="<%=Translate("Exportera lista") %>" title="<%=Translate("Exportera lista") %>" />
    <asp:PlaceHolder id="SelHolder" Visible="false" runat="server">&nbsp;&nbsp;<asp:DropDownList id="NewsSelDD" AutoPostBack="true" runat="server" /></asp:PlaceHolder>
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
      OnBeforeClientEdit="onBeforeRowEdit"
      OnClientEdit="onRowEdit"
      OnClientCancelEdit="hideTiny"
      OnBeforeClientUpdate="beforeUpdate"
      OnBeforeClientInsert="beforeUpdate"
      OnBeforeClientAdd="beforeAddRow"
      OnClientAdd="onAddRow"
      OnBeforeClientCancelAdd="hideTiny"
      OnClientCallbackError="onCallbackError"
      OnBeforeClientDelete="onDelete"
    />
    <Columns>
      <obgrd:Column Width="0" runat="server" />
      <obgrd:Column AllowEdit="true" AllowDelete="true" HeaderText="" Width="80px" runat="server" >
        <TemplateSettings TemplateId="editBtnTemplate" EditTemplateId="updateBtnTemplate" />
      </obgrd:Column>
      <obgrd:Column DataField="id" ReadOnly="true" Width="80px" runat="server"><FilterOptions><obgrd:FilterOption Type="NoFilter" /><obgrd:FilterOption Type="EqualTo" /><obgrd:FilterOption Type="SmallerThanOrEqualTo " /><obgrd:FilterOption Type="SmallerThan" /><obgrd:FilterOption Type="GreaterThan" /><obgrd:FilterOption Type="GreaterThanOrEqualTo" /></FilterOptions></obgrd:Column>
      <obgrd:Column DataField="sumid" ReadOnly="true" Visible="false" runat="server" />
      <obgrd:Column DataField="headline" Width="200px" Wrap="true" runat="server"><TemplateSettings RowEditTemplateControlId="e_headline" RowEditTemplateControlPropertyName="value"/></obgrd:Column>
      <obgrd:Column DataField="newsdate" Width="140px" runat="server"><TemplateSettings RowEditTemplateControlId="e_date" RowEditTemplateControlPropertyName="value"/><FilterOptions><obgrd:FilterOption Type="NoFilter" /><obgrd:FilterOption Type="EqualTo" /><obgrd:FilterOption Type="SmallerThanOrEqualTo " /><obgrd:FilterOption Type="SmallerThan" /><obgrd:FilterOption Type="GreaterThan" /><obgrd:FilterOption Type="GreaterThanOrEqualTo" /></FilterOptions></obgrd:Column>
      <obgrd:Column DataField="active" Width="80px" ShowFilterCriterias="false" runat="server"><TemplateSettings FilterTemplateId="filterYN1" TemplateId="tplActiveStat" RowEditTemplateControlId="e_active" RowEditTemplateControlPropertyName="checked"/></obgrd:Column>
      <obgrd:Column DataField="showinlist" Width="100px" ShowFilterCriterias="false" runat="server"><TemplateSettings FilterTemplateId="filterYN2" TemplateId="tplShowStat" RowEditTemplateControlId="e_show" RowEditTemplateControlPropertyName="checked"/></obgrd:Column>
      <obgrd:Column DataField="preview" Width="130px" ShowFilterCriterias="false" runat="server"><TemplateSettings FilterTemplateId="filterYN3" TemplateId="tplShowPrev" RowEditTemplateControlId="e_preview" RowEditTemplateControlPropertyName="checked"/></obgrd:Column>
      <obgrd:Column DataField="showinrss" Width="100px" ShowFilterCriterias="false" runat="server"><TemplateSettings FilterTemplateId="filterYN4" TemplateId="tplShowRss" RowEditTemplateControlId="e_rss" RowEditTemplateControlPropertyName="checked"/></obgrd:Column>
      <obgrd:Column DataField="summary" ParseHTML="true" Wrap="true" Width="300px" runat="server"><TemplateSettings TemplateId="tplSummary" RowEditTemplateControlId="e_summary" RowEditTemplateControlPropertyName="value"/></obgrd:Column>
      <obgrd:Column DataField="content" ParseHTML="true" Wrap="true" Width="400px" runat="server"><TemplateSettings TemplateId="tplContent" RowEditTemplateControlId="e_content" RowEditTemplateControlPropertyName="value"/></obgrd:Column>
      <obgrd:Column DataField="image" Width="140px" runat="server"><TemplateSettings TemplateId="tplImage" RowEditTemplateControlId="e_image" RowEditTemplateControlPropertyName="value"/></obgrd:Column>
    </Columns>

    <TemplateSettings
      RowEditTemplateId="tplRowEdit"
    />

    <Templates>

      <obgrd:GridTemplate runat="server" ID="editBtnTemplate"><Template>
        <input type='image' id='btnEdit' value="Edit" alt='<%= Translate("Redigera") %>' title='<%= Translate("Redigera") %>' src='gfx/edit.gif' onclick='ItemGrid.edit_record(this);this.blur();return false;' />&nbsp;&nbsp;
        <input type='image' id='btnDelete' value="Delete" alt='<%= Translate("Ta bort") %>' title='<%= Translate("Ta bort") %>' src='gfx/delete.gif' onclick='ItemGrid.delete_record(this);this.blur();return false;' />
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="updateBtnTemplate"><Template>
        <input type="image" id="btnUpdate" value="Update" alt='<%= Translate("Spara") %>' title='<%= Translate("Spara") %>' src='gfx/save.gif' onclick="ItemGrid.update_record(this);this.blur();return false;" />&nbsp;&nbsp;
        <input type="image" id="btnCancel" value="Cancel" alt='<%= Translate("Avbryt") %>' title='<%= Translate("Avbryt") %>' src='gfx/cancel.gif' onclick="ItemGrid.cancel_edit(this);this.blur();return false;" />
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplRowEdit"><Template>
        <div style='height:470px'>
          <table><tr>
            <td width="300"><b><%= Translate("Rubrik") %></b></td>
            <td width="150"><b><%= Translate("Datum") %>:</b></td>
            <td width="250"><b><%= Translate("Bild") %></b></td>
            <td rowspan="2" width="100"><label for='e_show'><b><%= Translate("Visa i lista") %></b></label><input id='e_show' type='checkbox' /></td>
            <td rowspan="2" width="100"><label for='e_active'><b><%= Translate("Aktiv") %></b></label><input id='e_active' type='checkbox' /></td>
            <td rowspan="2"><label for='e_preview'><b><%= Translate("Förhandsgranska") %></b></label><input id='e_preview' type='checkbox' /></td>
            <td rowspan="2"><label for='e_rss'><b><%= Translate("RSS") %></b></label><input id='e_rss' type='checkbox' /></td>
          </tr><tr>
            <td><input id="e_headline" type='text' class='ob_gEC' /></td>
            <td><input id='e_date' type='hidden' /><a id='aDate' class="ob_gAL" href='javascript:void(0)' onClick='show_cal(this)' onfocus='this.blur()'></a></td>
            <td><input id="e_image" type='text' class='ob_gEC' style='width:200px' /><a href='javascript:void(0)' onfocus='this.blur()' onclick="openMediabank('gotImage')"><img src='gfx/documentbank.gif' alt='Mediabank' border='0' /></a></td>
          </tr></table>
          <table><tr>
            <td width="570"><b><%= Translate("Sammanfattning") %></b><input type='hidden' id='e_summary' /><div id='summarydiv'></div></td>
            <td><b><%= Translate("Innehåll") %></b><input type='hidden' id='e_content' /><div id='contdiv'></div></td>
          </tr></table>
        </div>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplContent"><Template>
        <div class="news" style="width:380px;height:100px;overflow:auto"><%# Container.Value %></div>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplSummary"><Template>
        <div class="news" style="width:280px;height:100px;overflow:auto"><%# Container.Value %></div>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplShowStat"><Template>
        <%# Convert.ToBoolean(Container.Value) ? "Ja" : "Nej" %>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplShowPrev"><Template>
        <%# Convert.ToBoolean(Container.Value) ? "Ja" : "Nej" %>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplShowRss"><Template>
        <%# Convert.ToBoolean(Container.Value) ? "Ja" : "Nej" %>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplActiveStat"><Template>
        <%# Convert.ToBoolean(Container.Value) ? "Ja" : "Nej" %>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="tplImage"><Template>
        <img src='<%# NFN.Util.GetThumbnail(Container.Value.ToString(), 100, 100, true, "gfx/empty.gif") %>' alt='' />
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="filterYN1" ControlID="ddYN1" ControlPropertyName="value"><Template>
        <select id="ddYN1" class="ob_gEC"><option value=""></option><option value="true"><%=Translate("Ja")%></option><option value="false"><%=Translate("Nej")%></option></select>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="filterYN2" ControlID="ddYN2" ControlPropertyName="value"><Template>
        <select id="ddYN2" class="ob_gEC"><option value=""></option><option value="true"><%=Translate("Ja")%></option><option value="false"><%=Translate("Nej")%></option></select>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="filterYN3" ControlID="ddYN3" ControlPropertyName="value"><Template>
        <select id="ddYN3" class="ob_gEC"><option value=""></option><option value="true"><%=Translate("Ja")%></option><option value="false"><%=Translate("Nej")%></option></select>
      </Template></obgrd:GridTemplate>

      <obgrd:GridTemplate runat="server" ID="filterYN4" ControlID="ddYN4" ControlPropertyName="value"><Template>
        <select id="ddYN4" class="ob_gEC"><option value=""></option><option value="true"><%=Translate("Ja")%></option><option value="false"><%=Translate("Nej")%></option></select>
      </Template></obgrd:GridTemplate>
    </Templates>

  </obgrd:Grid>

   <div id='tplTinyC' style='left:-1000px;position:absolute'><div id="txtContent"></div></div>
   <div id='tplTinyS' style='left:-1000px;position:absolute'><div id="txtSummary"></div></div>

</asp:Content>
