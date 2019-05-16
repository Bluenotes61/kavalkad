/* $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ */
function initPage() {
  new RollOverButton("btn_excel", { rolloverimageurl: "gfx/excel_over.gif", clickimageurl: "gfx/excel_sel.gif", onclick: openDL });
  new RollOverButton("btn_morecols", { rolloverimageurl: "gfx/more_over.png", clickimageurl: "gfx/more_sel.png", onclick: moreCols });
  new RollOverButton("btn_lesscols", { rolloverimageurl: "gfx/less_over.png", clickimageurl: "gfx/less_sel.png", onclick: lessCols });
}

function moreCols() {
  for (var i=10; i < ItemGrid.Rows[0].Cells.length; i++)
    ItemGrid.showColumn(i);
  $("#btn_morecols").hide();
  $("#btn_lesscols").show();
}

function lessCols() {
  for (var i=10; i < ItemGrid.Rows[0].Cells.length; i++)
    ItemGrid.hideColumn(i);
  $("#btn_lesscols").hide();
  $("#btn_morecols").show();
}

function openDL() {
  var ids = "";
  for(var i = 0; i < ItemGrid.Rows.length; i++) {
    if(i > 0) ids += ",";
    ids += ItemGrid.Rows[i].Cells[2].Value;
  }
  window.open("downloadusers.aspx?ids=" + ids);
  return false;
}

function onRowEdit(record) {
  var chb = document.getElementsByName("roles");
  var roles = record.roles.split('<br />');
  for (var i=0; i < chb.length; i++)
    chb[i].checked = false;
  for (var i=0; i < chb.length; i++)
    for (var j=0; j < roles.length; j++)
      if (chb[i].id == 'role' + roles[j]) chb[i].checked = true;
  return true;
}

function onUpdate(record) {
  var roles = "";
  var chb = document.getElementsByName("roles");
  for (var i=0; i < chb.length; i++) {
    if (chb[i].checked) {
      if (roles.length > 0) roles += ",";
      roles += chb[i].id.substring(4);
    }
  }
  N$("hdroles").value = roles;
}

function onDelete(record) {
  return confirm(translate("Är du säker på att du vill ta bort användare") + " " + record["username"] + "?");
}

