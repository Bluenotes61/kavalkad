<!-- $Date: 2011-04-22 12:17:32 +0200 (fr, 22 apr 2011) $    $Revision: 7619 $ -->
<%@ Page language="C#" CodeFile="DocumentBank.cs" Inherits="DocumentBank"%>
<%@ Register TagPrefix="NFN" Namespace="NFN.Controls" %>
<%@ Register Tagprefix="obspl" Namespace="OboutInc.Splitter2" Assembly="obout_Splitter2_Net" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">
<html>
  <head runat="server">
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link rel="stylesheet" href="styles.css" type="text/css">
    <link rel="stylesheet" href="contextmenu.css" type="text/css">
    <script type="text/javascript">
      var maxWidth = '<%= Request["maxwidth"] %>';
      var initialPath = '<%= CurrFilePathId%>';
      var docPath = '<%= DocPath %>';
      var DocAreaId = '<%= DocAreaId%>';
      var allafiler = '<%= Translate("Alla filer") %>...';
      var fromtiny = <%= Request["fromtiny"] == "y" ? "true" : "false" %>;
      var callback = '<%= Request["callback"] %>';
      var cbparams = '<%= Request["params"] %>';
      var maxUploadSize = <%= MaxUploadSize %>;
    </script>
    <script src="../jstools/contextmenu.js" type="text/javascript"></script>
    <script type="text/javascript" src="upload/swfupload.js"></script>
    <asp:PlaceHolder id="TinyScript" runat="server"><script type="text/javascript" src="../tinymce/tiny_mce_popup.js"></script></asp:PlaceHolder>
    <script type="text/javascript" src="upload/upload.js"></script>
    <script type="text/javascript" src="/admin/obout/treeview/script/MediaBank/dndToTree.js"></script>
    <script type="text/javascript" src="scripts.js"></script>
  </head>

  <body onLoad="initPage()">
    <form runat="server">
      <ul id="FileMenu" class="SimpleContextMenu">
        <li><a href="javascript:db.viewDoc(SimpleContextMenu.target.id.substring(6))"><NFN:BWLabel Text="Förhandagranska" runat="server" /></a></li>
        <li><a href="javascript:db.useDoc(SimpleContextMenu.target.id.substring(6))"><NFN:BWLabel Text="Använd" runat="server" /></a></li>
        <li><a href="javascript:db.delDocuments(SimpleContextMenu.target.id.substring(6))"><NFN:BWLabel Text="Ta bort" runat="server" /></a></li>
      </ul>

      <ul id="TreeMenu" class="SimpleContextMenu">
        <li><a href="javascript:db.tree.addNode(SimpleContextMenu.target.id)"><NFN:BWLabel Text="Ny mapp" runat="server" /></a></li>
        <li><a href="javascript:db.tree.delNode(SimpleContextMenu.target.id)"><NFN:BWLabel Text="Ta bort" runat="server" /></a></li>
      </ul>

      <div id="Busy"><img src='../gfx/wait.gif' /></div>
      <div id="uploadProgress"><h3><%= Translate("Laddar upp") %></h3><div id="uploadProgressInner"></div></div>
      <div id="TopRow">
        <a href="javascript:db.setMode('browse')" onFocus="this.blur()"><img id="browsebtn" src="gfx/browse_sel.gif" onMouseOver="db.overButton(this)" onMouseOut="db.overButton(this)" alt='<%= Translate("Bläddra bland dokumenten") %>' title='<%= Translate("Bläddra bland dokumenten") %>' border='0'/></a>
        <a href="javascript:db.setMode('search')" onFocus="this.blur()"><img id="searchbtn" src="gfx/search_btn.gif" onMouseOver="db.overButton(this)" onMouseOut="db.overButton(this)" alt='<%= Translate("Sök dokument") %>' title='<%= Translate("Sök dokument") %>' border='0'/></a>
        <a href="javascript:db.setMode('permission')" onFocus="this.blur()" id="PermissionButton" runat="server"><img id="permissionbtn" src="gfx/permission_btn.gif" onMouseOver="db.overButton(this)" onMouseOut="db.overButton(this)" alt='<%= Translate("Administrera behörigheter") %>' title='<%= Translate("Administrera behörigheter") %>' border='0'/></a>
      </div>

      <obspl:Splitter StyleFolder="../obout/splitter/styles/documentbank" id="mainSplit" CookieDays="0" PanelResizable="right" runat="server">

        <LeftPanel WidthMin="100" WidthMax="400">
          <content>

            <div id="LeftHeader" class="headerdiv textheader"><NFN:BWLabel Text="Mappar" runat="server" /></div>
            <div class='headerdiv buttonheader' id='LeftButtons'>
              <div id="FolderButtons">
                <ul style='padding:4px;'>
                  <li id="addfolderbutt" style='display:none'><a href='javascript:db.tree.addNode()' onFocus="this.blur()"><img id="addfolder" src='gfx/addfolder_btn.gif' onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" alt='<%= Translate("Lägg till ny mapp") %>' title='<%= Translate("Lägg till ny mapp") %>' border='0'/></a></li>
                  <li id="delnode" style='display:none;'><a href='javascript:db.tree.delNode()' onFocus="this.blur()"><img id="recycle" src='gfx/recycle_btn.gif' onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" alt='<%= Translate("Ta bort mapp") %>' title='<%= Translate("Ta bort mapp") %>' border='0'/></a></li>
                </ul>
              </div>
              <div id="CopyPermButtons" style="display:none">
                <ul style='padding:4px;'>
                  <li id="copyfrombtn"><a href='javascript:db.perm.copyFrom()' onFocus="this.blur()"><img id="copyfrom" src='gfx/copyfrom_btn.gif' onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" alt='<%= Translate("Kopiera behörigheter från överliggande mapp") %>' title='<%= Translate("Kopiera behörigheter från överliggande mapp") %>' border='0'/></a></li>
                  <li id="copytobtn"><a href='javascript:db.perm.copyTo()' onFocus="this.blur()"><img id="copyto" src='gfx/copyto_btn.gif' onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" alt='<%= Translate("Kopiera behörigheter till underliggande mappar") %>' title='<%= Translate("Kopiera behörigheter till underliggande mappar") %>' border='0'/></a></li>
                </ul>
              </div>
              <div id="SearchBtn" style="display:none">
                <a href="javascript:db.searchDoc()" onFocus="this.blur()"><img id="search" src="gfx/search_btn.gif" onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" alt='<%= Translate("S?k dokument") %>' title='<%= Translate("S?k dokument") %>' border=0 style="margin:4px"/></a>
              </div>
            </div>

            <div id="BrowseFrame" style="display:block">
              <div id="TreeArea"><asp:Literal id="TreeViewBrowse" EnableViewState="false" runat="server" /></div>
            </div>
            <div id="SearchFrame" class="LeftFrame" style="display:none">
              <div id='infosearch'>
                <p><b><NFN:BWLabel Text="Dokumenttyp" runat="server" /></b><br /><select id='DocTypeSearch'><option value="image"><%= Translate("Bilder") %></option><option value="media"><%= Translate("Mediafiler") %></option><option value="document"><%= Translate("Dokument") %></option><option value="all"><%= Translate("Alla") %></option></select></p>
                <p><b><%= Translate("Bredd") %></b><br />Min:&nbsp;<input type="text" id='MinWidthEdt' style='width:50px'/>&nbsp;&nbsp;&nbsp;Max:&nbsp;<input type="text" id='MaxWidthEdt' style='width:50px'/></p>
                <p><b><%= Translate("Höjd") %></b><br />Min:&nbsp;<input type="text" id='MinHeightEdt' style='width:50px'/>&nbsp;&nbsp;&nbsp;Max:&nbsp;<input type="text" id='MaxHeightEdt' style='width:50px'/></p>
                <p><b><%= Translate("Storlek") %> (kb)</b><br />Min:&nbsp;<input type="text" id='MinSizeEdt' style='width:50px'/>&nbsp;&nbsp;&nbsp;Max:&nbsp;<input type="text" id='MaxSizeEdt' style='width:50px'/></p>
                <p><b><%= Translate("Filnamn") %></b><br /><input type="text" id='FileNameSearch' style='width:165px'/></p>
                <div id='extrainfosearch'></div>
              </div>
            </div>
          </content>
        </LeftPanel>

        <RightPanel>
          <content>
            <obspl:Splitter StyleFolder="../obout/splitter/styles/documentbank" id="rightSplit" CookieDays="0" PanelResizable="left" runat="server">

              <LeftPanel WidthMin="100">
                <content>

                  <div id="CenterHeader" class="headerdiv textheader"><span id="CenterHeaderLab"></span><span id="BrowsePathLab"></span><span id="SearchPathLab"></span></div>
                  <div id="CenterButtonDiv" class="headerdiv buttonheader">
                    <div id="ViewButtons">
                      <ul style='padding:4px;'>
                        <li><a href="javascript:db.setView('thumbs')" onFocus="this.blur()"><img id="thumbsbtn" alt='<%= Translate("Visa som tumnaglar") %>' title='<%= Translate("Visa som tumnaglar") %>' src="gfx/thumbs_sel.gif" onMouseOver="db.overButton(this)" onMouseOut="db.overButton(this)" border='0'/></a></li>
                        <li><a href="javascript:db.setView('list')" onFocus="this.blur()"><img id="listbtn" alt='<%= Translate("Visa detaljerad lista") %>' title='<%= Translate("Visa detaljerad lista") %>' src="gfx/list_btn.gif" onMouseOver="db.overButton(this)" onMouseOut="db.overButton(this)" border='0'/></a></li>
                        <li id="deldoc" style='display:inline;'><a href="javascript:db.delDocuments()" onFocus="this.blur()"><img id="recycle" src='gfx/recycle_btn.gif' onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" alt='<%= Translate("Ta bort dokument") %>' title='<%= Translate("Ta bort dokument") %>' border='0'/></a></li>
                        <li><span id="uploadbtn"><span id="uploadbtn_inner"></span></span></li>
                      </ul>
                    </div>
                  </div>


                  <div id="CenterArea">
                    <div id="DocArea" class="DocArea" runat="server"></div>
                    <div id="SearchDocArea" class="DocArea" style="display:none"></div>
                    <div id="PermissionArea" style="display:none"></div>
                    <div id="ErrArea"></div>
                  </div>
                  <div id="PreviewArea">
                    <div id="PreviewAreaInner">
                      <div class='headerdiv textheader' style='height:20px;text-align:right'><a href='javascript:db.closePreview()' onfocus='this.blur()'><img src='gfx/closewind.gif' border='0' alt='<%= Translate("Stäng") %>' title='<%= Translate("Stäng") %>' /></a></div>
                      <div id='PreviewContent' onClick="db.closePreview()"></div>
                    </div>
                    <div class='clearfloat'></div>
                  </div>
                </content>
              </LeftPanel>
              <RightPanel WidthDefault="220">
                <content>

                  <div id="RightHeader" class="headerdiv textheader"><%= Translate("Dokumentinformation") %></div>
                  <div id="RightButtons" class="headerdiv buttonheader">
                    <div id="InfoButtonDiv" style='display:none'>
                      <ul style='padding:4px;'>
                        <li><a href='javascript:db.viewDoc()' onFocus="this.blur()"><img id="preview" alt='<%= Translate("Förhandsgranska dokumentet") %>' title='<%= Translate("Förhandsgranska dokumentet") %>' src="gfx/preview_btn.gif" onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" border='0'/></a></li>
                        <li><a href='javascript:db.downloadDoc()' onFocus="this.blur()"><img id="download" alt='<%= Translate("Ladda ner dokumentet") %>' title='<%= Translate("Ladda ner dokumentet") %>' src="gfx/download_btn.gif" onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" border='0'/></a></li>
                        <li><span id="editbutt" style="display:none"><a href='javascript:db.showEdit()' onFocus="this.blur()"><img id="editbtn" alt='<%= Translate("Redigera dokumentinformation") %>' title='<%= Translate("Redigera dokumentinformation") %>' src="gfx/editbtn_btn.gif" onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" border='0'/></a></span></li>
                        <li><span id="UseDocSpan"><a href='javascript:db.useDoc()' onFocus="this.blur()"><img id="usedoc" src="gfx/usedoc_btn.gif" onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" alt='<%= Translate("Använd dokumentet på webbsidan") %>' title='<%= Translate("Använd dokumentet på webbsidan") %>' border=0 /></a></span></li>
                      </ul>
                    </div>
                    <div id="SaveButtonDiv" style='display:none'>
                      <ul style='padding:4px;'>
                        <li><a href='javascript:db.saveDocInfo()' onFocus="this.blur()"><img id="saveinfo" alt='<%= Translate("Spara ändringar") %>' title='<%= Translate("Spara ändringar") %>' src="gfx/save_btn.gif" onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" border='0'/></a></li>
                        <li><a href='javascript:db.hideEdit()' onFocus="this.blur()"><img id="cancelinfo" alt='<%= Translate("Avbryt") %>' title='<%= Translate("Avbryt") %>' src="gfx/cancel_btn.gif" onMouseOver="anim(this,'over')" onMouseOut="anim(this,'out');" onMouseDown="anim(this,'down');" onMouseUp="anim(this,'up');" border='0'/></a></li>
                      </ul>
                    </div>
                  </div>


                  <div id="InfoArea">
                    <div id="FileInfo">
                      <div id="StaticInfo" class="SetDiv">
                        <fieldset class="FieldSet"><legend><%= Translate("Dokumentinformation") %></legend>
                          <span id="FileType" style="display:none"></span>
                          <p><b><%= Translate("Namn") %>:</b><br/><span id="FileNameLab"></span></p>
                          <p><b><%= Translate("Storlek") %>:</b></br><span id="FileSizeLab"></span></p>
                          <div id="extrainfoview" style="display:block"></div>
                          <div id="extrainfoedit" style="display:none"></div>
                        </fieldset>
                      </div>

                      <div id="UseInfo">
                        <div id="UseImageInfo" style="display:none">
                          <div class="SetDiv">
                            <fieldset class="FieldSet"><legend><%= Translate("Dimensioner") %></legend>
                              <p><input id="orisize" type="radio" value="original" name="sizeopt" checked><label for="orisize"><%= Translate("Ursprunglig storlek") %>:</label><br/><span id="OriWidthLab" style="margin-left:20px"></span>px&nbsp;x&nbsp;<span id="OriHeightLab"></span>px</p>
                              <p><input id="newsize" type="radio" value="new" name="sizeopt"><label for="newsize"><%= Translate("Ny storlek") %>:</label><br/><input type="text" id="newwidth" size='3' style="margin-left:20px" onKeyUp="db.newWidthChanged()"/>&nbsp;px&nbsp;&nbsp;x&nbsp;&nbsp;<input type="text" id="newheight" size='3' onKeyUp="db.newHeightChanged();"/>&nbsp;px<input type='hidden' id='proportions' /></p>
                              <p><input id="percsize" type="radio" value="percent" name="sizeopt"><label for="percsize"><%= Translate("Procent") %>:</label><input type="text" id="percent" value='100' size='3' style="margin-left:5px" onKeyPress="document.getElementById('percsize').checked=true;"/>&nbsp;%</p>
                              <p><input id="aspectlock" type="checkbox" checked><label for="aspectlock"><%= Translate("Lås höjd/bredd") %></label></p>
                              <p><input id="maxwidth" type="checkbox" checked><label for="maxwidth"><%= Translate("Skala till maxbredd") %></label></p>
                            </fieldset>
                          </div>
                        </div>
                        <input type="hidden" id="DocId" />
                        <input type="hidden" id="DocType" />
                      </div>
                    </div>
                  </div>
                </content>
              </RightPanel>
            </obspl:Splitter>
          </content>
        </RightPanel>
      </obspl:Splitter>
    </form>
  </body>
</html>