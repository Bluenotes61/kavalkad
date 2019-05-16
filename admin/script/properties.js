/* $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ */

var lastid = "";

function initPage() {
  new RollOverButton("btn_edit", { rolloverimageurl: "gfx/edit_over.gif", clickimageurl: "gfx/edit_sel.gif", onclick: editProp });
  new RollOverButton("btn_delete", { rolloverimageurl: "gfx/delete_over.gif", clickimageurl: "gfx/delete_sel.gif", onclick: delProp });
  new RollOverButton("btn_up", { rolloverimageurl: "gfx/up_over.gif", clickimageurl: "gfx/up_sel.gif", onclick: moveUp });
  new RollOverButton("btn_down", { rolloverimageurl: "gfx/down_over.gif", clickimageurl: "gfx/down_sel.gif", onclick: moveDown });
  new RollOverButton("btn_unlock", { rolloverimageurl: "gfx/unlock_over.gif", clickimageurl: "gfx/unlock_sel.gif", onclick: unlock });
  new RollOverButton("btn_save", { rolloverimageurl: "gfx/save_over.gif", clickimageurl: "gfx/save_sel.gif", onclick: saveEdit });
  new RollOverButton("btn_cancel", { rolloverimageurl: "gfx/cancel_over.gif", clickimageurl: "gfx/cancel_sel.gif", onclick: undoEdit });
}

function GetPropInfo(propid) {
  Properties.GetPropInfo(propid, doneGetInfo);
}

function doneGetInfo(response) {
  if (response.value.length > 0) {
    var vals = response.value.split('|');

    N$("PropIdV").innerHTML = vals[0];
    N$("PropNameV").innerHTML = vals[1];
    N$("PropParentV").innerHTML = vals[2];
    N$("PropCtrlTypeV").innerHTML = vals[3];
    N$("PropOrderV").innerHTML = vals[4];
    N$("PropVisibleV").innerHTML = vals[5];
    N$("PropPublishedV").innerHTML = vals[6];
    N$("PropModDateV").innerHTML = vals[7];
    N$("PropModByV").innerHTML = vals[8];
    N$("PropPublishedDateV").innerHTML = vals[9];
    N$("PropPublishedByV").innerHTML = vals[10];
    N$("PropStartV").innerHTML = vals[11];
    N$("PropEndV").innerHTML = vals[12];
    N$("PropPrelimValueV").innerHTML = vals[13];
    N$("PropValueV").innerHTML = vals[14];

    N$S("UnlockBtn").display = (vals[17] == "true" ? "inline" : "none");
  }
}

function NodeSelected(propid) {
  GetPropInfo(propid);
  currid = propid;
}


function editProp() {
  Properties.GetPropInfo(currid, openEdit);
}

function openEdit(response) {
  if (response.value.length > 0) {
    var vals = response.value.split('|');

    N$("PropIdE").innerHTML = vals[0];
    N$("PropNameE").value = vals[1];
    N$("PropCtrlTypeE").innerHTML = vals[3];
    N$("PropOrderE").value = vals[4];
    N$("PropVisibleE").checked = (vals[5] == "Ja");
    N$("PropPublishedE").value = (vals[6] == "Ja");
    N$("PropModDateE").innerHTML = vals[7];
    N$("PropModByE").innerHTML = vals[8];
    N$("PropPublishedDateE").innerHTML = vals[9];
    N$("PropPublishedByE").innerHTML = vals[10];
    N$("PropStartE").value = vals[11];
    N$("PropEndE").value = vals[12];
    N$("PropPrelimValueE").innerHTML = vals[13];
    N$("PropValueE").innerHTML = vals[14];

    N$S("PropInfo").display = "none";
    N$S("PropEdit").display = "block";
    N$S("StdButtons").display = "none";
    N$S("SaveEditButtons").display = "block";
  }
}


function checkSaveEdit() {
  if (N$("PropNameE").value.length == 0) return translate("Fel") + ": " + translate("Ange postens namn") + ".";
  return "";
}

function saveEdit() {
  var err = checkSaveEdit();
  if (err.length > 0) {
    N$("EditPropError").innerHTML = err;
    N$S("EditPropError").display = "block";
  }
  else {
    var vals = N$("PropIdE").innerHTML + "|";
    vals += N$("PropNameE").value + "|";
    vals += N$("PropOrderE").value + "|";
    vals += N$("PropVisibleE").checked + "|";
    vals += N$("PropPublishedE").checked + "|";
    vals += N$("PropStartE").value + "|";
    vals += N$("PropEndE").value + "|";
    vals += N$("PropPrelimValueE").value + "|";
    vals += N$("PropValueE").value;

    Properties.SaveEdit(vals, doneEdit);
  }
}

function doneEdit(response) {
  N$S("EditPropError").display = "none";
  N$S("PropInfo").display = "block";
  N$S("PropEdit").display = "none";
  N$S("StdButtons").display = "block";
  N$S("SaveEditButtons").display = "none";

  var parent = ob_getParentOfNode(N$(currid));
  lastid = currid;
  N$(currid).innerHTML = N$("PropIdE").innerHTML + " - " + N$("PropNameE").value;
  GetPropInfo(currid);
}

function nodeExpanded(id) {
  if (lastid.length > 0) {
    var help = lastid;
    lastid = "";
    ob_SelectedId(help);
  }
}

function undoEdit() {
  N$S("EditPropError").display = "none";
  N$S("PropInfo").display = "block";
  N$S("PropEdit").display = "none";
  N$S("StdButtons").display = "block";
  N$S("SaveEditButtons").display = "none";
}


function delProp() {
  if (confirm(translate("Är du säker på att du vill permanent ta bort informationsposten") + " " + currid + ", " + translate("inklusive dess underposter") + "?"))
    Properties.DelProp(currid, doneDelete);
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
  var response = Properties.MoveNode(currid, 'up');
  if (response.value)
    ob_t2_UpDown('up');
}

function moveDown() {
  var response = Properties.MoveNode(currid, 'down');
  if (response.value)
    ob_t2_UpDown('down');
}

function unlock() {
  Properties.UnlockProp(currid, doneUnlock);
}

function doneUnlock(response) {
  N$S("locked_" + currid).color = "#000000";
  N$S("UnlockBtn").display = "none";
}
