/* $Date: 2010-09-08 11:43:22 +0200 (on, 08 sep 2010) $    $Revision: 6910 $ */
function N$(id) { return document.getElementById(id);}
function N$S(id) { return document.getElementById(id).style;}

/*** General ***/

function anim(img, action) {
  if (action == "over") img.src = "gfx/" + img.id + "_over.gif";
  else if (action == "out") img.src = "gfx/" + img.id + "_btn.gif";
  else if (action == "down") img.src = "gfx/" + img.id + "_sel.gif";
  else if (action == "up") img.src = "gfx/" + img.id + "_over.gif";
}

function showHide(elem, disp) {
  if (N$(elem)) N$(elem).style.display = disp;
}

function getIdx(elem, arr) {
  var res = -1;
  for (var i=0; i < arr.length && res == -1; i++)
    if (elem == arr[i]) res = i;
  return res;
}

function inArr(elem, arr) {
  var found = false;
  for (var i=0; i < arr.length && !found; i++)
    found = arr[i] == elem;
  return found;
}

function isInputElement(elem) {
  var inputs = document.getElementsByTagName("input");
  for (var i=0; i < inputs.length; i++) {
    if (inputs[i] == elem) return true;
  }
  var sels = document.getElementsByTagName("select");
  for (var i=0; i < sels.length; i++) {
    if (sels[i] == elem) return true;
  }
  return false;
}

function findPos(obj) {
  var curleft = curtop = 0;
  if (obj && obj.offsetParent) {
    curleft = obj.offsetLeft
    curtop = obj.offsetTop
    while (obj = obj.offsetParent) {
      curleft += obj.offsetLeft
      curtop += obj.offsetTop
    }
  }
  return [curleft,curtop];
}




/*** NodeTree ***/

function NodeTree(db) {

  this.permitEditFolder = false;

  this.selectPath = function(pathId) {
    db.setMode("browse");
    ob_t25(N$(pathId));
  }

  this.expandIfClosed = function(nodeid) {
    if (!ob_isExpanded(N$(nodeid))) ob_t23(N$(nodeid));
  }

  /* ob tree responses */

  this.checkAddNode = function(id) {
    return db.hasPermission(id, "addfolder");
  }

  this.checkRemoveNode = function(id) {
    if (ob_getFirstNodeOfTree().id == id) return false;
    return db.hasPermission(id, "delfolder");
  }

  this.checkNodeEdit = function(id) {
    if (ob_getFirstNodeOfTree().id == id) return false;
    return db.hasPermission(id, "editfolder");
  }

  this.checkNodeSelect = function(id) {
    return (db.hasPermission(id, "viewfolders") || db.hasPermission(id, "view"));
  }

  this.checkNodeDrop = function(src, dst, copy) {
    if (db.hasPermission(src, "dragndrop") && db.hasPermission(dst, "dragndrop")) {
      var mess = "";
      var response = DocumentBank.CheckDrop(dst, src, copy);
      if (response.error != null) mess = "Error in CheckDrop:" + response.error.Message;
      else if (response.value.length > 0) mess = response.value;
      if (mess.length > 0) alert(mess);
      else return true;
    }
    else return false;
  }

  this.checkNodeDrag = function(id) {
    return db.hasPermission(id, "dragndrop");
  }

  this.checkNodeExpand = function(id) {
    return db.hasPermission(id, "viewfolders");
  }


  this.nodeDropped = function(src, dst, copy) {
    if (copy) this.nodeCopied(src, dst, draggedNode);
    else this.nodeMoved(src, dst);
  }

  this.nodeMoved = function(src, dst) {
    db.checkLoggedIn();

    var response = DocumentBank.NodeMoved(src, dst);
    if (response.error != null)
      alert("Error in NodeMoved:" + response.error.Message);
    else
      db.reloadDocs(true);
  }

  this.nodeCopied = function(src, dest, oriId) {
    db.checkLoggedIn();
    var response = DocumentBank.NodeCopied(dest, oriId);
    if (response.error != null)
      alert("Error in NodeCopied:" + response.error.Message);
    else
      this.nodeSelected(oriId, true);
  }

  this.nodeSelected = function(filepathid, clearSel, callback) {
    db.nodeSelected(filepathid, clearSel, callback);
  }

  this.nodeExpanded = function(id, dynamic) {
    if (dynamic) {
      var child = ob_getFirstChildOfNode(N$(id));
      this.addMenuToNodes(child);
    }
  }

  this.addNode = function(nodeid) {
    db.checkLoggedIn();
    var obj = this;
    if (!nodeid) nodeid = tree_selected_id;
    DocumentBank.AddNode(nodeid, function(response) { obj.doneAdd(response); } );
  }

  this.doneAdd = function(response) {
    if (response.error != null) return;
    var parentnode = tree_selected_id;
    ob_t2_Add(parentnode, response.value[0], response.value[1], false, 'Folder.gif', null);
    SimpleContextMenu.attachTo(response.value[0], 'TreeMenu');
  }

  this.delNode = function(nodeid, force) {
    if (force == null) force = false;
    db.checkLoggedIn();
    var obj = this;
    if (!nodeid) nodeid = tree_selected_id;
    var ok = force || confirm(db.Translate("Är du säker på att du vill ta bort mappen, inklusive dess undermappar och dokument") + "?");
    if (ok) DocumentBank.DelNode(nodeid, force, function(response) { obj.doneDelNode(response, nodeid); } );
  }

  this.showError = function(err, time) {
    var html = "<div class='errclose'><a href='javascript:void(0)' onclick='N$S(\"ErrArea\").display=\"none\"' onfocus='this.blur()'><img src='gfx/closewind.gif' alt='" + translate("Stäng") + "' border=0 /></a></div><div class='errhead'>" + err + "</div>";
    N$('ErrArea').innerHTML = html;
    N$S('ErrArea').display = "block";
    if (time)
      setTimeout("N$S('ErrArea').display = 'none'", time);
  }

  this.doneDelNode = function(response, nodeid) {
    if (response.error != null) return;

    var res = response.value;
    if (res[1].length > 0)
      this.showError(translate("Ett eller flera dokument i mappen används på sajten. Mappen kan därför inte tas bort") + ".<p><a href='javascript:db.tree.delNode(\"" + nodeid + "\", true)' onfocus='this.blur()'>" + translate("Tvinga ta bort" + "</a></p>", ""));
    else
      N$S('ErrArea').display = "none";

    if (res[0].length > 0) {
      var nodeids = res[0].split(',');
      var anode = N$(nodeids[0]);
      var nextnode = ob_getNextSiblingOfNode(anode);
      if (!nextnode) nextnode = ob_getPrevSiblingOfNode(anode);
      if (!nextnode) nextnode = ob_getParentOfNode(anode);
      for (var i=0; i < nodeids.length; i++) {
        try {
          ob_t2_Remove(nodeids[i]);
        }
        catch(e) {
          alert(e);
        }
      }
      ob_t25(nextnode);
    }
  }

  this.nodeRename = function(id, newName, prevName) {
    db.checkLoggedIn();

    if (this.permitEditFolder) {
      var response = DocumentBank.NodeRename(id, newName);
      var mess = (response.error != null ? "Error in NodeRename:" + response.error.Message : (response.value.length > 0 ? response.value : ""));
      if (mess.length > 0) {
        alert(mess);
        if (N$(id).firstChild.tagName)
          N$(id).firstChild.innerHTML = prevName;
        else
          N$(id).innerHTML = prevName;
      }
    }
  }

  this.addMenuToNodes = function(anode) {
    if (anode) {
      SimpleContextMenu.attachTo(anode.id, 'TreeMenu');
      if (ob_hasChildren(anode)) {
        var child = ob_getFirstChildOfNode(anode);
        this.addMenuToNodes(child);
        child = ob_getNextSiblingOfNode(child);
        while (child) {
          this.addMenuToNodes(child);
          child = ob_getNextSiblingOfNode(child);
        }
      }
    }
  }

}


/*** Permissions ***/

function Permissions(db) {

  this.changed = function(cb) {
    var pathid = tree_selected_id;
    var help = cb.id.split(';');
    var response = DocumentBank.PermissionChanged(pathid, help[0], help[1], cb.checked);
    if (response.error != null) alert("Error in PermissionChanged:" + response.error.Message);
  }

  this.copyTo = function() {
    if (confirm(db.Translate("Är du säker på att du vill kopiera behörigheterna från den aktuella mappen till alla dess underliggande mappar") + "?")) {
      var thisnode = tree_selected_id;
      var response = DocumentBank.CopyPermToChildren(thisnode);
      if (response.error != null)
        alert("Error in CopyPermToChildren:" + response.error);
      if (N$("PermissionArea")) N$("PermissionArea").innerHTML = response.value;
    }
  }

  this.copyFrom = function() {
    if (confirm(db.Translate("Är du säker på att du vill kopiera behörigheterna från överliggande mapp till den aktuella mappen") + "?")) {
      var thisnode = tree_selected_id;
      var response = DocumentBank.CopyPermFromParent(thisnode);
      if (response.error != null)
        alert("Error in CopyPermFromParent:" + response.error);
      if (N$("PermissionArea") && response.value.length > 0) N$("PermissionArea").innerHTML = response.value;
    }
  }

}


/*** TinyMCE Popup ***/

var MediaBankPopup;
if (fromtiny) {
  MediaBankPopup = {
    init : function () {
      //initPage();
    },
    submit : function (docInfo) {
      var URL = docInfo.url;
      var win = tinyMCEPopup.getWindowArg("window");

      win.document.getElementById(tinyMCEPopup.getWindowArg("input")).value = URL;

      if (tinyMCEPopup.getWindowArg("allowedtype") == "image") {
        if (win.ImageDialog.getImageData) win.ImageDialog.getImageData();
        if (win.ImageDialog.showPreviewImage) win.ImageDialog.showPreviewImage(URL);
      }

      if (tinyMCEPopup.getWindowArg("allowedtype") == "media") {
        win.document.getElementById("width").value = docInfo.width;
        win.document.getElementById("height").value = docInfo.height;
        win.generatePreview();
      }

      tinyMCEPopup.close();
    }
  }
  tinyMCEPopup.onInit.add(MediaBankPopup.init, MediaBankPopup);
}


/*** DBDoc ***/

function DBDoc() {

  this.getInfo = function(fileid) {
    var response = DocumentBank.GetDocumentInfo(fileid);
    if (response.error != null) {
      alert("Error in GetDocumentInfo:" + response.error.Message);
      return;
    }
    var vals = response.value[0].split('|');
    this.id = fileid;
    this.type = vals[0];
    this.path = vals[1];
    this.filename = vals[2];
    this.url = "/DocumentBank/" + this.filename;
    this.size = parseInt(vals[3]);
    this.sizeStr = vals[4];
    this.width = parseInt(vals[5]);
    this.height = parseInt(vals[6]);
    this.proportions = parseFloat(vals[7]);

    if (response.value[1].length > 0) {
      this.extraProp = new Object();
      var evals = response.value[1].split(';');
      for (var i=0; i < evals.length; i++) {
        var hlp = evals[i].split('|');
        this.extraProp[hlp[0]] = hlp[1];
      }
    }
    else
      this.extraProp = null;
  }

  this.clear = function() {
    for (var prop in this) {
      if (typeof(this[prop]) != "function")
        this[prop] = null;
    }
  }
}


function translate(txt) {
  var response = DocumentBank.Translate(txt);
  return response.value;
}

/*** DB ***/

function DB() {

  var selectedDoc = "";
  var currMode = "browse";
  var currView = "thumbs";
  var currOrder = "filename";
  var currDir = "asc";
  var permitUseDoc = false;
  var viewIdPrefix = "vi_";
  var editIdPrefix = "ed_";
  var searchIdPrefix = "se_";
  var showeditbutt = false;

  var currDoc = new DBDoc();

  this.tree = new NodeTree(this);

  this.perm = new Permissions(this);

  this.setVisible = function(id, display) {
    if (N$(id)) N$S(id).display = display;
  }

  this.Translate = function(txt) {
    var response = DocumentBank.Translate(txt);
    return response.value;
  }

  this.overButton = function(img) {
    if (img.id == 'browsebtn' && currMode != 'browse')
      img.src = (img.src.indexOf('btn') > 0 ? 'gfx/browse_over.gif' : 'gfx/browse_btn.gif');
    else if (img.id == 'searchbtn' && currMode != 'search')
      img.src = (img.src.indexOf('btn') > 0 ? 'gfx/search_over.gif' : 'gfx/search_btn.gif');
    else if (img.id == 'permissionbtn' && currMode != 'permission')
      img.src = (img.src.indexOf('btn') > 0 ? 'gfx/permission_over.gif' : 'gfx/permission_btn.gif');
    else if (img.id == 'uploadbtn' && img.src.indexOf('_sel') < 0)
      img.src = (img.src.indexOf('btn') > 0 ? 'gfx/upload_over.gif' : 'gfx/upload_btn.gif');
    else if (img.id == 'thumbsbtn' && currView != 'thumbs')
      img.src = (img.src.indexOf('btn') > 0 ? 'gfx/thumbs_over.gif' : 'gfx/thumbs_btn.gif');
    else if (img.id == 'listbtn' && currView != 'list')
      img.src = (img.src.indexOf('btn') > 0 ? 'gfx/list_over.gif' : 'gfx/list_btn.gif');
  }

  this.showError = function(head, err, time) {
    var html = "<div class='errclose'><a href='javascript:void(0)' onclick='N$S(\"ErrArea\").display=\"none\"' onfocus='this.blur()'><img src='gfx/closewind.gif' alt='" + this.Translate("Stäng") + "' border=0 /></a></div><div class='errhead'>" + head + "</div><div class='errmess'>" + err + "</div>";
    N$('ErrArea').innerHTML = html;
    N$S('ErrArea').display = "block";
    if (time)
      setTimeout("N$S('ErrArea').display = 'none'", time);
  }

  this.checkLoggedIn = function() {
    var response = DocumentBank.IsLoggedIn();
    if (!response.value) {
      alert(this.Translate("Sessionen hav avslutats. Logga in som administratör igen!"));
      if (fromtiny) tinyMCEPopup.close();
      else window.close();
    }
  }

  this.hasPermission = function(id, type) {
    var response = DocumentBank.HasPermission(id, type);
    return response.value;
  }

  this.reloadDocs = function(clearSel, callback) {
    this.nodeSelected(tree_selected_id, clearSel, callback);
  }

  this.closePreview = function() {
    N$S("CenterArea").display = "block";
    N$S("PreviewArea").display = "none";
  }

  this.newWidthChanged = function() {
    if (N$('aspectlock').checked) {
      N$('newsize').checked = true;
      var width = N$('newwidth').value;
      if (parseFloat(currDoc.proportions) != 0) {
        var height = parseFloat(width)/parseFloat(currDoc.proportions);
        N$('newheight').value = Math.round(height);
      }
    }
  }

  this.newHeightChanged = function() {
    if (N$('aspectlock').checked) {
      N$('newsize').checked = true;
      var height = N$('newheight').value;
      var width = parseFloat(height)*parseFloat(currDoc.proportions);
      N$('newwidth').value = Math.round(width);
    }
  }


  this.setView = function(aView) {
    this.checkLoggedIn();
    currView = aView;
    N$("thumbsbtn").src = "gfx/thumbs_" + (currView == "thumbs" ? "sel" : "btn") + ".gif";
    N$("listbtn").src = "gfx/list_" + (currView == "list" ? "sel" : "btn") + ".gif";
    if (currMode == "browse")
      this.reloadDocs();
    else if (currMode == "search")
      this.searchDoc();
  }

  this.setMode = function(aMode) {
    this.checkLoggedIn();
    if (aMode != currMode) {
      if (aMode == "browse") {
        N$("browsebtn").src = "gfx/browse_sel.gif";
        N$("searchbtn").src = "gfx/search_btn.gif";
        if (N$("permissionbtn")) N$("permissionbtn").src = "gfx/permission_btn.gif";
        this.setVisible("BrowseFrame","block");
        this.setVisible("SearchBtn","none");
        this.setVisible("SearchFrame","none");
        this.setVisible(DocAreaId,"block");
        this.setVisible("SearchDocArea","none");
        this.setVisible("PermissionArea","none");
        this.setVisible("FolderButtons","block");
        this.setVisible("CopyPermButtons","none");
        this.setVisible("ViewButtons","block");
        this.setVisible("BrowsePathLab","inline");
        this.setVisible("SearchPathLab","none");
        this.setVisible("CenterHeaderLab","none");
        N$("LeftHeader").innerHTML = this.Translate("Mappar");
        N$("RightHeader").innerHTML = this.Translate("Dokumentinformation");
        this.reloadDocs();
      }
      else if (aMode == "search") {
        N$("browsebtn").src = "gfx/browse_btn.gif";
        N$("searchbtn").src = "gfx/search_sel.gif";
        if (N$("permissionbtn")) N$("permissionbtn").src = "gfx/permission_btn.gif";
        this.setVisible("BrowseFrame","none");
        this.setVisible("SearchBtn","block");
        this.setVisible("SearchFrame","block");
        this.setVisible(DocAreaId,"none");
        this.setVisible("SearchDocArea","block");
        this.setVisible("PermissionArea","none");
        this.setVisible("FolderButtons","none");
        this.setVisible("CopyPermButtons","none");
        this.setVisible("ViewButtons","block");
        this.setVisible("BrowsePathLab","none");
        this.setVisible("SearchPathLab","inline");
        this.setVisible("CenterHeaderLab","none");
        N$("LeftHeader").innerHTML = this.Translate("Sökkriterier");
        N$("RightHeader").innerHTML = this.Translate("Dokumentinformation");
      }
      else if (aMode == "permission") {
        N$("browsebtn").src = "gfx/browse_btn.gif";
        N$("searchbtn").src = "gfx/search_btn.gif";
        if (N$("permissionbtn")) N$("permissionbtn").src = "gfx/permission_sel.gif";
        this.setVisible("BrowseFrame","block");
        this.setVisible("SearchFrame","none");
        this.setVisible("SearchBtn","none");
        this.setVisible(DocAreaId,"none");
        this.setVisible("SearchDocArea","none");
        this.setVisible("PermissionArea","block");
        this.setVisible("FolderButtons","none");
        this.setVisible("CopyPermButtons","block");
        N$("LeftHeader").innerHTML = this.Translate("Mappar");
        this.setVisible("ViewButtons","block");
        this.setVisible("BrowsePathLab","none");
        this.setVisible("SearchPathLab","none");
        this.setVisible("CenterHeaderLab","inline");
        N$("CenterHeaderLab").innerHTML = this.Translate("Mappbehörigheter");
        N$("RightHeader").innerHTML = "";
      }
      currMode = aMode;
      this.setVisible("FileInfo","none");
      this.setVisible("InfoButtonDiv","none");
    }
  }

  this.reorder = function(newOrder) {
    if (newOrder == currOrder)
      currDir = (currDir == "asc" ? "desc" : "asc");
    else
      currDir = "asc";
    currOrder = newOrder;
    this.reloadDocs();
  }



  this.nodeSelected = function(filepathid, clearSel, callback) {
    var obj = this;
    this.checkLoggedIn();
    if (filepathid) {
      if (clearSel) selectedDoc = "";
      DocumentBank.NodeSelected(currMode, filepathid, currView, currOrder, currDir, function(response) { obj.doneNodeSelected(response, callback); });
    }
  }

  this.doneNodeSelected = function(response, callback) {
    if (response.error != null) return;
    var path = response.value[0];
    var dochtml = response.value[1];
    var docs = response.value[2];
    var permhtml = response.value[3];
    var permissions = response.value[4];

    // Use|Edit|Upload|Del|AddFolder|EditFolder|DelFolder
    var permArr = permissions.split(';');
    permitUseDoc = (permArr[0] == "Y");
    showHide("editbutt",(permArr[1] == "Y" && showeditbutt ? "inline" : "none"));
    showHide("uploadbtn",(permArr[2] == "Y" ? "inline" : "none"));
    showHide("deldoc",(permArr[3] == "Y" ? "inline" : "none"));
    showHide("addfolderbutt",(permArr[4] == "Y" ? "inline" : "none"));
    this.tree.permitEditFolder = (permArr[5] == "Y");
    showHide("delnode",(permArr[6] == "Y" ? "inline" : "none"));

    if (N$("BrowsePathLab")) N$("BrowsePathLab").innerHTML = path;

    if (N$(DocAreaId)) {
      N$(DocAreaId).innerHTML = dochtml;

      var docarr = docs.split(';');
      for (var i=0; i < docarr.length; i++) {
        SimpleContextMenu.attachTo(docarr[i], 'FileMenu');
        ob_attachDragAndDrop(N$(docarr[i]));
      }
    }

    var elem = N$("cd_delete");
    if (elem) elem.style.display = (permArr[3] == "Y" ? "inline" : "none");
    elem = N$("cd_use");
    if (elem) elem.style.display = (permitUseDoc ? "inline" : "none");

    var docIds = N$("docids");
    if (docIds) {
      var allArr = docIds.value.split(',');
      var selectedArr = selectedDoc.split(',');
      this.doSelect(allArr, selectedArr);
    }

    if (permhtml.length > 0) {
      var permarea = N$("PermissionArea");
      if (permarea) permarea.innerHTML = permhtml;
    }

    showHide("UseInfo",(permitUseDoc ? "block" : "none"));
    showHide("UseDocSpan",(permitUseDoc ? "inline" : "none"));
    showHide("FileInfo","none");
    showHide("InfoButtonDiv","none");
    showHide("InfoArea","none");

    if (callback) callback();
  }

  this.delDocuments = function(docs, force) {
    if (force == null) force = false;
    var obj = this;
    this.checkLoggedIn();
    if (docs == null) docs = selectedDoc;
    if (docs.length > 0) {
      var docsarr = docs.split(',');
      var ok = force || confirm(this.Translate("Är du säker på att du vill ta bort") + " "  + docsarr.length + " " + this.Translate("dokument") + "?");
      if (ok) DocumentBank.DelDocuments(docs, force, function(response) { obj.doneDelDoc(docs, response); });
    }
    else {
      this.showError("", this.Translate("Markera filer att ta bort") + ".", 2000);
    }
  }

  this.doneDelDoc = function(deldocs, response) {
    if (response.error != null) return;
    if (response.value.length > 0) {
      var head = this.Translate("Följande dokument togs inte bort eftersom de används på indikerade webbsidor") + ".";
      var err = "<table><tr><td><b>" + this.Translate("Dokument") + "</b></td><td><b>" + this.Translate("Webbsida") + "</b></td></tr>";
      var docs = response.value.split(';');
      for (var i=0; i < docs.length; i++) {
        var hlp = docs[i].split('|');
        err += "<tr><td>" + hlp[0] + "</td><td>";
        for (var j=1; j < hlp.length; j++) {
          if (j > 1) err += "<br />";
          err += "<a href='/" + hlp[j] + ".aspx' onfocus='this.blur()' target='_blank'>" + hlp[j] + "</a>";
        }
        err += "</td></tr>";
      }
      err += "</table><p><a href='javascript:db.delDocuments(\"" + deldocs + "\", true)' onfocus='this.blur()'>" + this.Translate("Tvinga ta bort") + "</a></p>";
      this.showError(head, err);
    }
    else N$S('ErrArea').display = "none";

    this.reloadDocs(true);
  }


  this.docMovedExternal = function(docid, destid) {
    if (selectedDoc.Length > 0) docid = selectedDoc;
    else docid = docid.split('_')[1];
    var response = DocumentBank.DocMovedExternal(docid, destid);
    if (response.error != null)
      return "Error in DocMovedExternal:" + response.error.Message;
    else if (response.value.length == 0)
      this.reloadDocs(true);
    return response.value;
  }

  this.docCopiedExternal = function(docid, destid) {
    if (selectedDoc.Length > 0) docid = selectedDoc;
    else docid = docid.split('_')[1];
    var response = DocumentBank.DocCopiedExternal(docid, destid);
    if (response.error != null)
      return "Error in DocCopiedExternal:" + response.error.Message;
    else if (response.value.length == 0)
      this.reloadDocs(true);
    return response.value;
  }



  this.doSelect = function(oldArr, newArr) {
    for (var i=0; i < oldArr.length; i++) {
      var elem = N$("thumb_" + oldArr[i]);
      if (elem) {
        if (currView == "thumbs") {
          elem.style.border = "2px solid #ffffff";
        }
        else if (currView == "list") {
          for (var j=0; j < elem.childNodes.length; j++)
            elem.childNodes[j].style.backgroundColor = "#ffffff";
        }
      }
    }
    for (var i=0; i < newArr.length; i++) {
      var elem = N$("thumb_" + newArr[i]);
      if (elem) {
        if (currView == "thumbs") {
          elem.style.border = "2px solid #316AC5";
        }
        else if (currView == "list") {
          for (var j=0; j < elem.childNodes.length; j++)
            elem.childNodes[j].style.backgroundColor = "#cccccc";
        }
      }
    }
  }


  this.markDoc = function(evt, docid) {
    this.checkLoggedIn();

    var shiftPressed = evt && evt != null && evt.shiftKey;
    var ctrlPressed = evt && evt != null && evt.ctrlKey;

    var oldSelectedArr = selectedDoc.split(',');
    var allArr = N$("docids").value.split(',');

    var selectedArr = selectedDoc.split(',');
    if (selectedDoc.length > 0) {
      selectedDoc = "";
      if (shiftPressed) {
        var idxLast = getIdx(selectedArr[selectedArr.length - 1], allArr);
        var idxCurr = getIdx(docid, allArr);

        if (!ctrlPressed) selectedArr = new Array(0);
        var from = (idxCurr >= idxLast ? idxLast : idxCurr);
        var to = (idxCurr >= idxLast ? idxCurr : idxLast);
        for (var i=from; i <= to; i++)
          if (getIdx(allArr[i], selectedArr) == -1) selectedDoc += "," + allArr[i];
        for (var i=0; i < selectedArr.length; i++)
          selectedDoc = "," + selectedArr[i] + selectedDoc;
        selectedDoc = selectedDoc.substring(1);
      }
      else if (ctrlPressed) {
        var idx = getIdx(docid, selectedArr);
        if (idx == -1) selectedArr.push(docid);
        else selectedArr.splice(idx, 1);
        for (var i=0; i < selectedArr.length; i++)
          selectedDoc += "," + selectedArr[i];
        selectedDoc = selectedDoc.substring(1);
      }
      else
        selectedDoc = docid;
    }
    else
      selectedDoc = docid;

    selectedArr = selectedDoc.split(',');
    this.doSelect(oldSelectedArr, selectedArr);

    if (selectedArr.length != 1) {
      showHide("FileInfo","none");
      showHide("SaveButtonDiv","none");
      showHide("InfoButtonDiv","none");
      showHide("InfoArea","none");
      if (currMode == "search") N$('SearchPathLab').innerHTML = "";
      currDoc.clear();
    }
    else {
      currDoc.getInfo(docid);
      N$('FileNameLab').innerHTML = currDoc.filename;
      N$('FileSizeLab').innerHTML = currDoc.sizeStr;
      N$('OriWidthLab').innerHTML = currDoc.width;
      N$('OriHeightLab').innerHTML = currDoc.height;
      N$('newwidth').value = currDoc.width;
      N$('newheight').value = currDoc.height;
      if (currMode == "search") N$('SearchPathLab').innerHTML = currDoc.path;
      if (currDoc.extraProp) {
        for (var prop in currDoc.extraProp) {
          N$(viewIdPrefix + prop).innerHTML = currDoc.extraProp[prop];
          N$(editIdPrefix + prop).value = currDoc.extraProp[prop];
        }
      }
      showHide("InfoButtonDiv","block");
      showHide("InfoArea","block");
      showHide("FileInfo","block");
      showHide("SaveButtonDiv","none");
      showHide("UseImageInfo",(currDoc.type == "image" ? "block" : "none"));
    }
  }


  this.saveDocInfo = function() {
    this.checkLoggedIn();
    var res = "";
    var div = N$("extrainfoedit");
    var elems = div.getElementsByTagName("input");
    for (var i=0; i < elems.length; i++) {
      if (res.length > 0) res += ";";
      res += elems[i].id.substring(editIdPrefix.length) + "|" + elems[i].value;
    }
    elems = div.getElementsByTagName("select");
    for (var i=0; i < elems.length; i++) {
      if (res.length > 0) res += ";";
      res += elems[i].id.substring(editIdPrefix.length) + "|" + elems[i].value;
    }
    if (res.length > 0) {
      var response = DocumentBank.SaveDocInfo(selectedDoc, res);
      if (response.error != null) {
        alert("Error in SaveDocInfo:" + response.error.Message);
        return;
      }
      this.markDoc(null, selectedDoc);
    }
    this.hideEdit();
    if (currView == "list")
      this.reloadDocs();
  }

  this.viewDoc = function(docid) {
    this.checkLoggedIn();
    if (docid && docid != currDoc.id)
      currDoc.getInfo(docid);

    var pwdiv = N$("PreviewArea");
    var pwadiv = N$("PreviewContent");
    pwadiv.innerHTML = "";
    var odiv = N$("rightSplit_LeftP_Content");
    var w = odiv.offsetWidth;
    var h = odiv.offsetHeight;
    if (currDoc.type == 'image') {
      pwdiv.style.width = w + "px";
      pwdiv.style.height = h + "px";
      pwadiv.innerHTML = "<img src='" + currDoc.url + "' alt='' />";
    }
    else if (currDoc.url.substring(currDoc.url.length-3) == 'swf') {
      pwadiv.innerHTML = "<div id='pwflash'></div>";
      var so = new SWFObject(currDoc.url, 'content', currDoc.width, currDoc.height, '8', '#ffffff', false);
      so.addParam('menu', 'false');
      so.addParam('scale', 'noscale');
      so.write('pwflash');
    }
    else {
      window.open(currDoc.url);
    }
    if (pwadiv.innerHTML.length > 0) {
      N$S("CenterArea").display = "none";
      pwdiv.style.display = "block";
    }
  }

  this.downloadDoc = function(docid) {
    this.checkLoggedIn();
    if (docid && docid != currDoc.id)
      currDoc.getInfo(docid);
    window.open("/admin/download.aspx?file=" + currDoc.url);
  }

  this.useDoc = function(docid) {
    this.checkLoggedIn();

    if (docid && docid != currDoc.id)
      currDoc.getInfo(docid);

    if (permitUseDoc) {
      if (currDoc.type == "image") {
        var fitImage = N$("maxwidth").checked && maxWidth != null && maxWidth.length > 0;
        var imgWidth;
        var imgHeight;
        var makeThumb = false;
        var keepAspect = N$("aspectlock").checked;
        var oriWidth = currDoc.width;
        if (N$("newsize").checked || (fitImage && parseInt(maxWidth) < oriWidth)) {
          var oriHeight = currDoc.height;
          var reqWidth = parseFloat(N$("newwidth").value);
          var reqHeight = parseFloat(N$("newheight").value);
          if (fitImage && parseInt(maxWidth) < reqWidth)
            reqWidth = parseInt(maxWidth);
          if (!keepAspect) {
            imgWidth = reqWidth;
            imgHeight = reqHeight;
          }
          else if (oriHeight >= oriWidth) {
            imgHeight = reqHeight;
            imgWidth = parseInt(Math.round((oriWidth/oriHeight)*reqHeight));
            if (imgWidth > reqWidth) {
              imgWidth = reqWidth;
              imgHeight = parseInt(Math.round((oriHeight/oriWidth)*reqWidth));
            }
          }
          else {
            imgWidth = reqWidth;
            imgHeight = parseInt(Math.round((oriHeight/oriWidth)*reqWidth));
            if (imgHeight > reqHeight) {
              imgHeight = reqHeight;
              imgWidth = parseInt(Math.round((oriWidth/oriHeight)*reqHeight));
            }
          }
          if (imgHeight != oriHeight || imgWidth != oriWidth)
            makeThumb = true;
        }
        else if (N$("percsize").checked) {
          imgWidth = currDoc.width;
          imgHeight = currDoc.height;
          var p = parseInt(N$("percent").value);
          imgWidth = imgWidth*p/100;
          imgHeight = imgHeight*p/100;
          if (p != 100)
            makeThumb = true;
        }
        else if (N$("orisize").checked) {
          imgWidth = currDoc.width;
          imgHeight = currDoc.height;
        }
        currDoc.width = imgWidth;
        currDoc.height = imgHeight;
        if (makeThumb) {
          var response = DocumentBank.GetThumbnail(currDoc.url, imgWidth, imgHeight, keepAspect, "/admin/DocumentBank/gfx/thumberror.gif");
          currDoc.url = response.value;
        }
      }

      var allowedtype = (fromtiny ? tinyMCEPopup.getWindowArg("allowedtype") : "file");

      if (allowedtype != "file" && allowedtype != currDoc.type) {
        alert(this.Translate("Denna dokumenttyp kan inte användas här") + ".");
        return;
      }

      if (fromtiny) {
        MediaBankPopup.submit(currDoc);
      }
      else {
        opener[callback](currDoc, cbparams);
        window.close();
        //window.blur();
        //opener.focus();
      }
    }
  }


  this.showEdit = function(){
    this.checkLoggedIn();
    showHide("extrainfoview","none");
    showHide("extrainfoedit","block");
    showHide("InfoButtonDiv","none");
    showHide("editbutt","none");
    showHide("SaveButtonDiv","block");
    showHide("UseInfo","none");
  }


  this.hideEdit = function(){
    this.checkLoggedIn();
    showHide("extrainfoview","block");
    showHide("extrainfoedit","none");
    showHide("InfoButtonDiv","block");
    showHide("editbutt","inline");
    showHide("SaveButtonDiv","none");
    showHide("UseInfo",(permitUseDoc ? "block" : "none"));
  }



  this.searchDoc = function() {
    this.checkLoggedIn();

    this.setVisible("FileInfo","none");
    this.setVisible("InfoButtonDiv","none");
    N$('SearchPathLab').innerHTML = "";

    var docType = N$("DocTypeSearch").value;
    var wmin = N$("MinWidthEdt").value;
    var wmax = N$("MaxWidthEdt").value;
    var hmin = N$("MinHeightEdt").value;
    var hmax = N$("MaxHeightEdt").value;
    var smin = N$("MinSizeEdt").value;
    var smax = N$("MaxSizeEdt").value;
    var fname = N$("FileNameSearch").value;
    var std = docType + "|" + wmin + "|" + wmax + "|" + hmin + "|" + hmax + "|" + smin + "|" + smax + "|" + fname;

    var extra = "";
    var div = N$("extrainfosearch");
    var elems = div.getElementsByTagName("input");
    for (var i=0; i < elems.length; i++) {
      if (extra.length > 0) extra += ";";
      extra += elems[i].id.substring(searchIdPrefix.length) + "|" + elems[i].value;
    }
    elems = div.getElementsByTagName("select");
    for (var i=0; i < elems.length; i++) {
      if (extra.length > 0) extra += ";";
      extra += elems[i].id.substring(searchIdPrefix.length) + "|" + elems[i].value;
    }
    var obj = this;
    DocumentBank.SearchDoc(std, extra, currView, function(response) { obj.doneSearch(response); } );
  }

  this.doneSearch = function(response) {
    if (response.error != null) {
      this.showError("Error at search", response.error.Message, 2000);
      return;
    }
    var docarea = response.value.split('^');
    N$("SearchDocArea").innerHTML = docarea[0];
  }

  this.setupExtraInfo = function() {
    var vhtml = "";
    var ehtml = "";
    var shtml = "";

    var response = DocumentBank.GetExtraFields();
    showeditbutt = response.value.length > 0;
    for (var i=0; i < response.value.length; i++) {
      var field = response.value[i].split('|');
      var headline = field[0];
      var fieldid = field[1];
      var fieldtype = field[2];

      vhtml += "<p><b>" + headline + ":</b><br/><span id='" + viewIdPrefix + fieldid + "'></p>";

      ehtml += "<p><b>" + headline + ":</b><br />";
      shtml += "<p><b>" + headline + "</b><br />";
      if (fieldtype.length == 0 || fieldtype == "text") {
        ehtml += "<input type='text' id='" + editIdPrefix + fieldid + "' size='30' /></p>";
        shtml += "<input type='text' id='" + searchIdPrefix + fieldid + "' style='width:165px' /></p>";
      }
      else if (fieldtype == "dropdown") {
        var fieldalt = DocumentBank.GetInfoAlt(fieldid);
        ehtml += "<select id='" + editIdPrefix + fieldid + "'><option></option>";
        shtml += "<select id='" + searchIdPrefix + fieldid + "'><option></option>";
        for (var j=0; j < fieldalt.length; j++) {
          ehtml += "<option>" + fieldalt[j] + "</option>";
          shtml += "<option>" + fieldalt[j] + "</option>";
        }
        ehtml += "</select></p>";
        shtml += "</select></p>";
      }
    }

    N$('extrainfoview').innerHTML = vhtml;
    N$('extrainfoedit').innerHTML = ehtml;
    N$('extrainfosearch').innerHTML = shtml;
  }

  this.uploadDone = function() {
    var response = DocumentBank.AfterUpload();
    this.reloadDocs(false, function() {
      db.markDoc(null, response.value);
    });
  }
}



/*** Init ***/

var db = new DB();

function initPage() {
  db.setupExtraInfo();
  db.nodeSelected(tree_selected_id, true);
  SimpleContextMenu.onShow = function(elem) {
    if (elem.id.indexOf("thumb") == 0)
      db.markDoc(null, elem.id.substring(6));
    else
      ob_t26(elem.id);
  };
  db.tree.addMenuToNodes(ob_getFirstNodeOfTree());
  initUpload();
}



if(typeof AjaxPro != "undefined" && AjaxPro !== null){
  AjaxPro.timeoutPeriod = 1000*20;
  AjaxPro.onTimeout = function(b,res) { ajaxTimeout(b,res); }
  AjaxPro.onLoading = function(b) { showBusy(b); }
  AjaxPro.onError = function(res){ ajaxError(res); }
}
function showBusy(busy) { document.getElementById("Busy").style.display = (busy ? "block" : "none"); }
function ajaxTimeout(b, res) { showBusy(false); alert("Timeout i funktionen " + res.method); }
function ajaxError(res) { showBusy(false); alert(res.Message); }
