<%-- $Date: 2010-11-22 14:32:20 +0100 (mÃ¥, 22 nov 2010) $    $Revision: 7099 $ --%>
<%@ Page language="C#" MasterPageFile="admin.master" CodeFile="behind/settings.cs" Inherits="Settings" validateRequest=false EnableEventValidation="false" %>
<%@ Register Tagprefix="obgrd" Namespace="Obout.Grid" Assembly="obout_Grid_NET" %>

<asp:Content ContentPlaceHolderId="Header" runat="server">
  <script type='text/javascript' src="script/settings.js"></script>
  <link rel="stylesheet" href="css/settings.css" type="text/css">
  <link rel="stylesheet" type="text/css" href='jstools/datetimepicker/skins/tiger/theme.css'>
  <script type="text/javascript" src="jstools/datetimepicker/calendar.js"></script>
  <script type="text/javascript" src="jstools/datetimepicker/lang/calendar-sv.js"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderId="ButtonBar" runat="server">
  <div id="StdButtons"></div>
</asp:Content>

<asp:Content ContentPlaceHolderId="SingleFrame" runat="server">
  <div id="maincontent">
    <div id="leftcol">
      <h1><%= Translate("Generella inställningar") %></h1>
      <asp:Literal id="GenSettings" runat="server" />

      <div id='recoverdiv'>
        <h1><%= Translate("Återställ sajten") %></h1>
        <b>Återställningsdatum: </b><input id='e_recover' type='hidden' value='<%= DateTime.Now.ToString("yyyy-MM-dd HH:mm") %>' /><a id='rDate' class="ob_gAL" href='javascript:void(0)' onClick='show_cal(this)' onfocus='this.blur()'><%= DateTime.Now.ToString("yyyy-MM-dd HH:mm") %></a>
        <div><a href='javascript:recover()' onfocus='this.blur()'><img src='/admin/gfx/restore_btn.gif' border='0' alt='<%= Translate("Återställ") %>' /></a></div>
        <div id='recoverok'><h1><%= Translate("Sajten har återställts") %></h1></div>
      </div>
    </div>
    <div id='rightcol'>

      <div id="attribPop">
        <input type='hidden' id='attribfor' />
        <div id="closediv"><a href='javascript:void(0)' onclick="$('#attribPop').hide('normal')" oncocus='this.blur()'><img src='gfx/closewind.gif' alt='<%= Translate("Stäng") %>' border='0' /></a></div>
        <h1 style='float:left' id='attribhl'></h1>
        <div style='float:right'>
          <a href="javascript:void(0)" onclick="moveAttrib('up')"><img border="0" title='<%= Translate("Flytta upp ett steg") %>' alt='<%= Translate("Flytta upp ett steg") %>' src="/admin/gfx/up_btn.gif" id="btn_up"></a>
          <a href="javascript:void(0)" onclick="moveAttrib('down')"><img border="0" title='<%= Translate("Flytta ner ett steg") %>' alt='<%= Translate("Flytta ner ett steg") %>' src="/admin/gfx/down_btn.gif" id="btn_up"></a>
        </div>
        <div class='clearfloat'></div>
        <obgrd:Grid id="AttribsGrid"
          OnDeleteCommand = "DeleteAttrib"
          OnUpdateCommand = "UpdateAttrib"
          OnInsertCommand = "InsertAttrib"
          FilterType="ProgrammaticOnly"
          runat="server"
        >
          <ClientSideEvents
            OnBeforeClientDelete="onDeleteAttrib"
            OnClientAdd="onAddAttrib"
            OnClientEdit="onEditAttrib"
            onClientPopulateControls="onPopulateControls"
          />

          <Columns>
            <obgrd:Column Width="0" runat="server" />
            <obgrd:Column AllowEdit="true" AllowDelete="true" HeaderText="" Width="80px" runat="server" ><TemplateSettings TemplateId="editTmplAttribs" EditTemplateId="updateTmplAttribs" /></obgrd:Column>
            <obgrd:Column DataField="filename" Visible="false" runat="server" />
            <obgrd:Column DataField="id" Width="150" runat="server"><TemplateSettings EditTemplateId="idTmplAttrib" /></obgrd:Column>
            <obgrd:Column DataField="name" Width="250" runat="server" />
            <obgrd:Column DataField="type" Width="150" runat="server" />
            <obgrd:Column DataField="lang" Width="150" runat="server" />
            <obgrd:Column DataField="alternatives" Width="250" runat="server" />
          </Columns>

          <Templates>
            <obgrd:GridTemplate runat="server" ID="editTmplAttribs"><Template>
              <input type='image' id='btnEdit' value="Edit" alt='<%= Translate("Redigera") %>' title='<%= Translate("Redigera") %>' src='gfx/edit.gif' onclick='AttribsGrid.edit_record(this);this.blur();return false;' />&nbsp;&nbsp;
              <input type='image' id='btnDelete' value="Delete" alt='<%= Translate("Ta bort") %>' title='<%= Translate("Ta bort") %>' src='gfx/delete.gif' onclick='AttribsGrid.delete_record(this);this.blur();return false;' />
            </Template></obgrd:GridTemplate>

            <obgrd:GridTemplate runat="server" ID="updateTmplAttribs"><Template>
              <input type="image" id="btnUpdate" value="Update" alt='<%= Translate("Spara") %>' title='<%= Translate("Spara") %>' src='gfx/save.gif' onclick="AttribsGrid.update_record(this);this.blur();return false;" />&nbsp;&nbsp;
              <input type="image" id="btnCancel" value="Cancel" alt='<%= Translate("Avbryt") %>' title='<%= Translate("Avbryt") %>' src='gfx/cancel.gif' onclick="AttribsGrid.cancel_edit(this);this.blur();return false;" />
            </Template></obgrd:GridTemplate>

            <obgrd:GridTemplate runat="server" ID="idTmplAttrib" ControlID="idattrib" ControlPropertyName="value"><Template>
              <span id='idattribspan'></span><input type='text' id='idattrib' />
            </Template></obgrd:GridTemplate>

          </Templates>
        </obgrd:Grid>
      </div>

      <div class='stablediv'>
        <h1 style='float:left'><%= Translate("Sidmallar") %></h1>
        <div style='float:right'>
          <a href="javascript:void(0)" onclick="moveTemplate('up')"><img border="0" title='<%= Translate("Flytta upp ett steg") %>' alt='<%= Translate("Flytta upp ett steg") %>' src="/admin/gfx/up_btn.gif" id="btn_up"></a>
          <a href="javascript:void(0)" onclick="moveTemplate('down')"><img border="0" title='<%= Translate("Flytta ner ett steg") %>' alt='<%= Translate("Flytta ner ett steg") %>' src="/admin/gfx/down_btn.gif" id="btn_up"></a>
        </div>
        <div class='clearfloat'></div>
        <obgrd:Grid id="TemplatesGrid"
          OnDeleteCommand = "DeleteTemplate"
          OnUpdateCommand = "UpdateTemplate"
          OnInsertCommand = "InsertTemplate"
          runat="server"
        >
          <ClientSideEvents
            OnBeforeClientDelete="onDeleteTemplate"
            OnClientAdd="onAddTemplate"
            OnClientEdit="onEditTemplate"
          />

          <Columns>
            <obgrd:Column Width="0" runat="server" />
            <obgrd:Column AllowEdit="true" AllowDelete="true" HeaderText="" Width="80px" runat="server" ><TemplateSettings TemplateId="editTmplTemplates" EditTemplateId="updateTmplTemplates" /></obgrd:Column>
            <obgrd:Column DataField="filename" Width="150" runat="server"><TemplateSettings EditTemplateId="idTmplTemplates" /></obgrd:Column>
            <obgrd:Column DataField="name" Width="200" runat="server" />
            <obgrd:Column DataField="category" Width="150" runat="server" />
            <obgrd:Column DataField="icon" Width="250" runat="server" />
            <obgrd:Column Width="100px" ReadOnly="true" runat="server"><TemplateSettings TemplateId="attribTmpl" /></obgrd:Column>
          </Columns>

          <Templates>
            <obgrd:GridTemplate runat="server" ID="editTmplTemplates"><Template>
              <input type='image' id='btnEdit' value="Edit" alt='<%= Translate("Redigera") %>' title='<%= Translate("Redigera") %>' src='gfx/edit.gif' onclick='TemplatesGrid.edit_record(this);this.blur();return false;' />&nbsp;&nbsp;
              <input type='image' id='btnDelete' value="Delete" alt='<%= Translate("Ta bort") %>' title='<%= Translate("Ta bort") %>' src='gfx/delete.gif' onclick='TemplatesGrid.delete_record(this);this.blur();return false;' />
            </Template></obgrd:GridTemplate>

            <obgrd:GridTemplate runat="server" ID="updateTmplTemplates"><Template>
              <input type="image" id="btnUpdate" value="Update" alt='<%= Translate("Spara") %>' title='<%= Translate("Spara") %>' src='gfx/save.gif' onclick="TemplatesGrid.update_record(this);this.blur();return false;" />&nbsp;&nbsp;
              <input type="image" id="btnCancel" value="Cancel" alt='<%= Translate("Avbryt") %>' title='<%= Translate("Avbryt") %>' src='gfx/cancel.gif' onclick="TemplatesGrid.cancel_edit(this);this.blur();return false;" />
            </Template></obgrd:GridTemplate>

            <obgrd:GridTemplate runat="server" ID="idTmplTemplates" ControlID="idtmpl" ControlPropertyName="value"><Template>
              <span id='idtmplspan'></span><input type='text' id='idtmpl' />
            </Template></obgrd:GridTemplate>

            <obgrd:GridTemplate runat="server" ID="attribTmpl"><Template>
              <a href="javascript:void(0)" onclick="openAttributes(this)"><%= Translate("Attribut") %></a>
            </Template></obgrd:GridTemplate>

          </Templates>
        </obgrd:Grid>
      </div>
      <div class='clearfloat'></div>


      <div class='stablediv'>
        <h1 style='float:left'><%= Translate("Stilmallar") %></h1>
        <div style='float:right'>
          <a href="javascript:void(0)" onclick="moveCss('up')"><img border="0" title='<%= Translate("Flytta upp ett steg") %>' alt='<%= Translate("Flytta upp ett steg") %>' src="/admin/gfx/up_btn.gif" id="btn_up"></a>
          <a href="javascript:void(0)" onclick="moveCss('down')"><img border="0" title='<%= Translate("Flytta ner ett steg") %>' alt='<%= Translate("Flytta ner ett steg") %>' src="/admin/gfx/down_btn.gif" id="btn_up"></a>
        </div>
        <div class='clearfloat'></div>
        <obgrd:Grid id="CssGrid"
          OnDeleteCommand = "DeleteCss"
          OnUpdateCommand = "UpdateCss"
          OnInsertCommand = "InsertCss"
          runat="server"
        >
          <ClientSideEvents
            OnBeforeClientDelete="onDeleteCss"
            OnClientAdd="onAddCss"
            OnClientEdit="onEditCss"
          />

          <Columns>
            <obgrd:Column Width="0" runat="server" />
            <obgrd:Column AllowEdit="true" AllowDelete="true" HeaderText="" Width="80px" runat="server" ><TemplateSettings TemplateId="editTmplCss" EditTemplateId="updateTmplCss" /></obgrd:Column>
            <obgrd:Column DataField="name" Width="200" runat="server"><TemplateSettings EditTemplateId="idTmplCss" /></obgrd:Column>
            <obgrd:Column DataField="description" Width="300" runat="server" />
          </Columns>
          <Templates>
            <obgrd:GridTemplate runat="server" ID="editTmplCss"><Template>
                <input type='image' id='btnEdit' value="Edit" alt='<%= Translate("Redigera") %>' title='<%= Translate("Redigera") %>' src='gfx/edit.gif' onclick='CssGrid.edit_record(this);this.blur();return false;' />&nbsp;&nbsp;
                <input type='image' id='btnDelete' value="Delete" alt='<%= Translate("Ta bort") %>' title='<%= Translate("Ta bort") %>' src='gfx/delete.gif' onclick='CssGrid.delete_record(this);this.blur();return false;' />
            </Template></obgrd:GridTemplate>

            <obgrd:GridTemplate runat="server" ID="updateTmplCss"><Template>
                <input type="image" id="btnUpdate" value="Update" alt='<%= Translate("Spara") %>' title='<%= Translate("Spara") %>' src='gfx/save.gif' onclick="CssGrid.update_record(this);this.blur();return false;" />&nbsp;&nbsp;
                <input type="image" id="btnCancel" value="Cancel" alt='<%= Translate("Avbryt") %>' title='<%= Translate("Avbryt") %>' src='gfx/cancel.gif' onclick="CssGrid.cancel_edit(this);this.blur();return false;" />
            </Template></obgrd:GridTemplate>
            <obgrd:GridTemplate runat="server" ID="idTmplCss" ControlID="idcss" ControlPropertyName="value"><Template>
              <span id='idcssspan'></span><input type='text' id='idcss' />
            </Template></obgrd:GridTemplate>
          </Templates>
        </obgrd:Grid>
      </div>
      <div class='clearfloat'></div>


    </div>
    <div class='clearfloat'></div>
  </div>
</asp:Content>
