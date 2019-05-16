/* $Date: 2011-04-17 14:23:15 +0200 (sö, 17 apr 2011) $    $Revision: 7601 $ */
var lastid = "";

function initPage() {
  new RollOverButton("btn_gotopage", { rolloverimageurl: "gfx/page_over.gif", clickimageurl: "gfx/page_sel.gif", onclick: gotoPage });
  new RollOverButton("btn_newpage", { rolloverimageurl: "gfx/new_over.gif", clickimageurl: "gfx/new_sel.gif", onclick: newPage });
  new RollOverButton("btn_editpage", { rolloverimageurl: "gfx/edit_over.gif", clickimageurl: "gfx/edit_sel.gif", onclick: editPage });
  new RollOverButton("btn_delpage", { rolloverimageurl: "gfx/delete_over.gif", clickimageurl: "gfx/delete_sel.gif", onclick: delPage });
  new RollOverButton("btn_uppage", { rolloverimageurl: "gfx/up_over.gif", clickimageurl: "gfx/up_sel.gif", onclick: moveUp });
  new RollOverButton("btn_downpage", { rolloverimageurl: "gfx/down_over.gif", clickimageurl: "gfx/down_sel.gif", onclick: moveDown });
  new RollOverButton("btn_restorepage", { rolloverimageurl: "gfx/restore_over.gif", clickimageurl: "gfx/restore_sel.gif", onclick: showRestore });
  new RollOverButton("btn_undelpage", { rolloverimageurl: "gfx/recycle_over.gif", clickimageurl: "gfx/recycle_sel.gif", onclick: showUndelete });
  new RollOverButton("btn_unlockpage", { rolloverimageurl: "gfx/unlock_over.gif", clickimageurl: "gfx/unlock_sel.gif", onclick: unlock });
  new RollOverButton("btn_savenew", { rolloverimageurl: "gfx/save_over.gif", clickimageurl: "gfx/save_sel.gif", onclick: saveNewPage });
  new RollOverButton("btn_cancelnew", { rolloverimageurl: "gfx/cancel_over.gif", clickimageurl: "gfx/cancel_sel.gif", onclick: undoNewPage });
  new RollOverButton("btn_saveedit", { rolloverimageurl: "gfx/save_over.gif", clickimageurl: "gfx/save_sel.gif", onclick: saveEdit });
  new RollOverButton("btn_canceledit", { rolloverimageurl: "gfx/cancel_over.gif", clickimageurl: "gfx/cancel_sel.gif", onclick: undoEdit });
}

function show_cal(a) {
  showCalendar(a.previousSibling, a, true, cal_onChange);
}
function cal_onChange(cal, adate) {
  cal.sel.value = adate;
  cal.shownAt.innerHTML = adate;
}

function getPageInfo(pageid) {
  Pages.GetPageInfo(pageid, doneGetInfo);
}

function doneGetInfo(response) {
  if (response.value != null && response.value.length > 0) {
    var vals = response.value;
    N$("PageIdV").innerHTML = vals[0];
    N$("PageNameV").innerHTML = vals[1];
    N$("PageModDateV").innerHTML = vals[2];
    N$("PageModByV").innerHTML = vals[3];
    N$("PageTemplateV").innerHTML = (vals[6].length > 0 ? "<img src='" + vals[6] + "' alt='" + vals[5] + " - " + vals[4] + "' title='" + vals[5] + " - " + vals[4] + "' /><br />" + vals[5] : vals[5] + " - " + vals[4]);
    N$("PageProtectV").innerHTML = vals[7];
    N$("PageStatusV").innerHTML = vals[9];
    N$("PageLangV").innerHTML = vals[10];
    N$("PagePropertyV").innerHTML = vals[11];
    N$("PageSharedPropertyV").innerHTML = vals[12];
    N$("PageSharedV").innerHTML = vals[13];
    N$("PageRelatedV").innerHTML = vals[14];
    N$("RedirToChildV").innerHTML = vals[15];
    N$("QuickCreateV").innerHTML = vals[16];
    N$("PageRedirectV").innerHTML = vals[17];
    N$("PageTitleV").innerHTML = vals[18];
    N$("PageKeysV").innerHTML = vals[19];
    N$("PageDescriptV").innerHTML = vals[20];
    if (vals[21] == translate("Ja"))
      N$("PageTimeControlV").innerHTML = translate("Starttid") + ": " + vals[22] + "<br />" + translate("Sluttid") + ": " + vals[23];
    else
      N$("PageTimeControlV").innerHTML = "";
    N$("PagePermissionsV").innerHTML = vals[24];
    N$S("UnlockBtn").display = (vals[31] == "true" ? "inline" : "none");
    $("#PageInfoDiv").find("tr.attrib_row").remove();
    if (vals[32].length > 1) {
      var html = vals[32].split('|');
      $("#PageInfoDiv").find("td.sep").parent().before(html[0]);
    }
  }
}

function unlock() {
  var id = N$("PageIdV").innerHTML;
  var response = Pages.UnlockProp(id);
  N$S("UnlockBtn").display = "none";
}

function showRestore() {
  Pages.GetRestoreHtml(currid, doneShowRestore);
}
function doneShowRestore(response) {
  if (response.error != null) {
    alert(response.error.Message);
    return;
  }
  var div = N$("restoreDiv");
  div.innerHTML = response.value;
  div.style.display = "block";
}

function showUndelete() {
  Pages.GetUndeleteHtml(doneShowUndelete);
}
function doneShowUndelete(response) {
  if (response.error != null) {
    alert(response.error.Message);
    return;
  }
  var div = N$("undelDiv");
  div.innerHTML = response.value;
  div.style.display = "block";
}

function doRestore(date) {
  var ok = confirm(translate("Är du säker på att du vill återställa sidan till tiden") + " " + date + "?");
  if (ok) Pages.DoRestore(currid, date, doneRestore);
}
function doneRestore(response) {
  if (response.error != null) {
    alert(response.error.Message);
    return;
  }
  getPageInfo(currid);
  alert(translate("Sidan återställd till tidpunkten") + " " + response.value);
}

function doUndelete(apage) {
  Pages.DoUndelete(apage, doneUndelete);
}
function doneUndelete(response) {
  if (response.error != null) {
    alert(response.error.Message);
    return;
  }
  alert(translate("Sidan återställd"));
}

function NodeSelected(pageid) {
  getPageInfo(pageid);
  currid = pageid;
}

function CheckDrop(src, dst, copy) {
  return true;
}

function NodeDropped(src, dst, copy) {
  var response = Pages.NodeMoved(src, dst);
  NodeSelected(currid);
}

function pageTmplEChanged(dd) {
  var ed = N$("PageTemplateE");
  ed.value = dd.value;
}
function pageTmplNChanged(dd) {
  var ed = N$("FileNameN");
  ed.value = dd.value;
}


function gotoPage() {
  document.location.href = "../" + currid + ".aspx";
}

function newPage() {
  N$S("PageInfo").display = "none";
  N$S("PageNew").display = "block";
  N$S("StdButtons").display = "none";
  N$S("SaveNewButtons").display = "block";
}

function checkSaveNew(id) {
  if (id.length == 0) return "Fel: Ange sidans identifierare.";
  if (id.indexOf(" ") != -1) return "Fel: Sidans identifierare kan inte innehålla mellanslag.";
  if (N$("PageNameN").value.length == 0) return "Fel: Ange sidans namn.";
  if (N$("PageTitleN").value.length == 0) return "Fel: Ange en titel för sidan.";
  if (N$("FileNameN").value.length == 0) return "Fel: Ange en sidmall för sidan.";
  return "";
}


function saveNewPage() {
  var id = N$("PageIdN").value;
  var err = checkSaveNew(id);
  if (err.length > 0) {
    N$("NewPageError").innerHTML = err;
    N$S("NewPageError").display = "block";
    return;
  }
  var protdd = N$("PageProtectN");
  var prot = protdd.options[protdd.selectedIndex].value;
  var langchk = document.getElementsByName('chklang_new');
  var lang = "";
  for (var i=0; i < langchk.length; i++) {
    if (langchk[i].checked) {
      if (lang.length > 0) lang += ",";
      lang += langchk[i].value;
    }
  }

  var vals = new Array(16);
  vals[0] = id;
  vals[1] = currid;
  vals[2] = N$("PageNameN").value;
  vals[3] = N$("FileNameN").value;
  vals[4] = prot;
  vals[5] = N$("PageStatusN").value;
  vals[6] = lang;
  vals[7] = String(N$("RedirToChildN").checked);
  vals[8] = String(N$("QuickCreateN").checked);
  vals[9] = N$("PageRedirectN").value;
  vals[10] = N$("PageTitleN").value;
  vals[11] = N$("PageKeysN").value;
  vals[12] = N$("PageDescriptN").value;
  vals[13] = String(N$("PageTimeControlN").checked);
  vals[14] = N$("PageTimeControlStartN").value;
  vals[15] = N$("PageTimeControlEndN").value;

  var pattrib = document.getElementsByName('pattrib_n');
  var spa = "";
  for (var i=0; i < pattrib.length; i++) {
    if (i > 0) spa += ";";
    var aval = ($(pattrib[i]).attr("type") == "checkbox" ? String($(pattrib[i]).attr("checked")) : $(pattrib[i]).val());
    spa += pattrib[i].id.substring(8) + "|" + aval;
  }
  vals[16] = spa;

  Pages.SaveNewPage(vals, doneNew);
}

function doneNew(response) {
  if (response.value.length > 0) {
    N$("NewPageError").innerHTML = response.value;
    N$S("NewPageError").display = "block";
    return;
  }

  var id = N$("PageIdN").value;
  try {
    ob_t2_Add(currid, id, N$("PageNameN").value, null, 'ie_link.gif', null);
  }
  catch(e) {}
  ob_SelectedId(id);

  N$S("PageInfo").display = "block";
  N$S("PageNew").display = "none";
  N$S("StdButtons").display = "block";
  N$S("SaveNewButtons").display = "none";
  N$S("NewPageError").display = "none";
}


function undoNewPage() {
  N$S("NewPageError").display = "none";
  N$S("PageInfo").display = "block";
  N$S("PageNew").display = "none";
  N$S("StdButtons").display = "block";
  N$S("SaveNewButtons").display = "none";
}



function editPage() {
  var pageid = N$("PageIdV").innerHTML;

  var response = Pages.GetSharedTree(pageid);
  var html = "<input id='sp__nopage' style='display:none' type='radio' name='sp_rb' checked='true' />" + response.value;
  N$("PageSharedE").innerHTML = html;
  $("#sharedpages li.selected").each(function(){
    var parent = $(this).parent().parent();
    if (parent.get(0).tagName == "LI")
      parent.addClass("open");
  });
  $("#sharedpages").treeview({collapsed:true});

  response = Pages.GetRelatedTree(pageid);
  N$("PageRelatedE").innerHTML = response.value;
  $("#relatedpages li.selected").each(function(){
    var parent = $(this).parent().parent();
    if (parent.get(0).tagName == "LI")
      parent.addClass("open");
  });
  $("#relatedpages").treeview({collapsed:true});

  Pages.GetPageInfo(currid, openEdit);
}

function clearShared() {
  $("#sp__nopage").attr("checked","true");

}

function sharedClicked(inp) {
  var ok = confirm(translate("Nuvarande innehåll i delade informationsposter på sidan går förlorat om du delar sidan med andra sidor. Ok att dela?"));
  if (!ok) inp.checked = false;
}

function openEdit(response) {
  if (response.value != null && response.value.length > 0) {
    var vals = response.value;

    var protidx = 2;
    if (vals[7] == "Ja") protidx = 0;
    else if (vals[7] == "Nej") protidx = 1;
    N$("PageIdE").innerHTML = vals[0];
    N$("PageNameE").value = vals[1];
    N$("PageModDateE").innerHTML = vals[2];
    N$("PageModByE").innerHTML = vals[3];
    N$("PageTemplateE").value = vals[4];
    N$("PageProtectE").selectedIndex = protidx;
    N$("RedirToChildE").checked = (vals[15] == translate("Ja"));
    N$("QuickCreateE").checked = (vals[16] == translate("Ja"));
    N$("PageRedirectE").value = vals[17];
    N$("PageTitleE").value = vals[18];
    N$("PageKeysE").value = vals[19];
    N$("PageDescriptE").innerHTML = vals[20];
    N$("PageTimeControlE").checked = (vals[21] == translate("Ja"));
    N$("PageTimeControlStartE").value = vals[22];
    N$("EPageTimeControlStartE").innerHTML = vals[22];
    N$("PageTimeControlEndE").value = vals[23];
    N$("EPageTimeControlEndE").innerHTML = vals[23];
    N$("PageTmplDDE").innerHTML = vals[25];
    N$("PageStatusE").value = vals[26];
    N$("PagePropertyE").innerHTML = vals[27];
    N$("PageSharedPropertyE").innerHTML = vals[28];

    var langchk = document.getElementsByName('chklang_edit');
    for (var i=0; i < langchk.length; i++)
      langchk[i].checked = false;

    if (vals[10].length > 0) {
      var langarr = vals[10].split(',');
      for (var i=0; i < langarr.length; i++)
        N$(langarr[i] + "_edit").checked = true;
    }

    if (vals[29].length > 0)
      ob_SelectedId("_s_" + vals[27]);

    if (vals[30].length > 0) {
      var permArr = vals[30].split(';');
      for (var i=0; i < permArr.length; i++) {
        var help = permArr[i].split('!');
        var role = help[0];
        var action = help[1];
        var perm = help[2];
        var aid = "p_" + action + "_" + role + "_" + perm;
        N$(aid).checked = true;
      }
    }

    $("#PageEdit").find("tr.attrib_row").remove();
    if (vals[32].length > 1) {
      var html = vals[32].split('|');
      $("#PageEdit").find("td.sep").parent().before(html[1]);
    }

    N$S("PageInfo").display = "none";
    N$S("PageEdit").display = "block";
    N$S("StdButtons").display = "none";
    N$S("SaveEditButtons").display = "block";
  }
}


function checkSaveEdit() {
  if (N$("PageNameE").value.length == 0) return "Fel: Ange sidans namn.";
  if (N$("PageTitleE").value.length == 0) return "Fel: Ange en titel för sidan.";
  if (N$("PageTemplateE").value.length == 0) return "Fel: Ange en sidmall för sidan.";
  return "";
}

function saveEdit() {
  var err = checkSaveEdit();
  if (err.length > 0) {
    N$("EditPageError").innerHTML = err;
    N$S("EditPageError").display = "block";
  }
  else {
    var protdd = N$("PageProtectE");
    var prot = protdd.options[protdd.selectedIndex].value;
    var langchk = document.getElementsByName('chklang_edit');
    var lang = "";
    for (var i=0; i < langchk.length; i++) {
      if (langchk[i].checked) {
        if (lang.length > 0) lang += ",";
        lang += langchk[i].value;
      }
    }

    var currshared = $("#sharedpages input:checked");
    currshared = (currshared.length > 0 ? currshared.attr("id").substring(3) : "");
    var related = "";
    $("#relatedpages input:checked").each(function(){
      if (related.length > 0) related += "|";
      related += this.id.substring(3);
    });

    var vals = new Array(17);
    vals[0] = N$("PageIdE").innerHTML;
    vals[1] = N$("PageNameE").value;
    vals[2] = N$("PageTemplateE").value;
    vals[3] = prot;
    vals[4] = N$("PageStatusE").value;
    vals[5] = lang;
    vals[6] = currshared;
    vals[7] = related;
    vals[8] = String(N$("RedirToChildE").checked);
    vals[9] = String(N$("QuickCreateE").checked);
    vals[10] = N$("PageRedirectE").value;
    vals[11] = N$("PageTitleE").value;
    vals[12] = N$("PageKeysE").value;
    vals[13] = N$("PageDescriptE").value;
    vals[14] = String(N$("PageTimeControlE").checked);
    vals[15] = N$("PageTimeControlStartE").value;
    vals[16] = N$("PageTimeControlEndE").value;

    var pattrib = document.getElementsByName('pattrib_e');
    var spa = "";
    for (var i=0; i < pattrib.length; i++) {
      if (i > 0) spa += ";";
      var aval = ($(pattrib[i]).attr("type") == "checkbox" ? String($(pattrib[i]).attr("checked")) : $(pattrib[i]).val());
      spa += pattrib[i].id.substring(8) + "|" + aval;
    }
    vals[17] = spa;

    var inp = document.getElementsByTagName("input");
    var perm = "";
    for (var i=0; i < inp.length; i++) {
      if (inp[i].id.indexOf("p_") == 0 && inp[i].checked) {
        if (perm.length > 0) perm += "|";
        perm += inp[i].id;
      }
    }
    vals[18] = perm;

    Pages.SaveEdit(vals, doneEdit);
  }
}

function doneEdit(response) {
  var err = "";
  if (response.value.length > 0)
    err = response.value;
  if (err.length > 0) {
    N$("EditPageError").innerHTML = err;
    N$S("EditPageError").display = "block";
  }
  else {
    N$S("EditPageError").display = "none";
    N$S("PageInfo").display = "block";
    N$S("PageEdit").display = "none";
    N$S("StdButtons").display = "block";
    N$S("SaveEditButtons").display = "none";
  }

  var parent = ob_getParentOfNode(N$(currid));
  lastid = currid;
  N$(currid).innerHTML = N$("PageNameE").value;
  getPageInfo(currid);
}

function nodeExpanded(id) {
  if (lastid.length > 0) {
    var help = lastid;
    lastid = "";
    ob_SelectedId(help);
  }
}

function undoEdit() {
  N$S("EditPageError").display = "none";
  N$S("PageInfo").display = "block";
  N$S("PageEdit").display = "none";
  N$S("StdButtons").display = "block";
  N$S("SaveEditButtons").display = "none";
}


function delPage() {
  if (confirm(translate("Är du säker på att du vill permanent ta bort sidan, inklusive dess undersidor och information?"))) {
    Pages.DelPage(currid, doneDelete);
  }
}

function doneDelete(response) {
  var newNode = ob_getPrevSiblingOfNode(N$(currid));
  if (newNode == null) newNode = ob_getNextSiblingOfNode(N$(currid));
  if (newNode == null) newNode = ob_getParentOfNode(N$(currid));
  if (newNode == null) newNode = ob_getFirstNodeOfTree();

  ob_t2_Remove (currid);
  ob_t25(newNode);
}

function moveUp() {
  var response = Pages.MoveNode(currid, 'up');
  if (response.value)
    ob_t2_UpDown('up');
}

function moveDown() {
  var response = Pages.MoveNode(currid, 'down');
  if (response.value)
    ob_t2_UpDown('down');
}

function permissionRoleChanged(dd) {
  var selRole = dd.value;
  for (var i=0; i < dd.length; i++) {
    var aRole = dd[i].value;
    N$S("p_" + aRole).display = (selRole == aRole ? "block" : "none");
  }
}
