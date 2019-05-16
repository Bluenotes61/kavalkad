/* $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ */
function initPage() {
  new RollOverButton("btn_excel", { rolloverimageurl: "gfx/excel_over.gif", clickimageurl: "gfx/excel_sel.gif", onclick: function() { exportTable('backweb') } });
  new RollOverButton("btn_up", { rolloverimageurl: "gfx/up_over.gif", clickimageurl: "gfx/up_sel.gif", onclick: function() { moveRow('up'); } });
  new RollOverButton("btn_down", { rolloverimageurl: "gfx/down_over.gif", clickimageurl: "gfx/down_sel.gif", onclick: function() { moveRow('down'); } });
}

function onRowEdit(record) {
  var dd = document.getElementsByName("roles");
  var roles = record.permissions.split('\r\n');
  for (var i=0; i < dd.length; i++)
    dd[i].selectedIndex = 0;
  if (record.permissions.length > 0) {
    for (var i=0; i < dd.length; i++) {
      for (var j=0; j < roles.length; j++) {
        var hlp = roles[j].split('-');
        if (dd[i].id == 'role' + hlp[0])
          dd[i].selectedIndex = (hlp[1] == 'Y' ? 1 : 2);
      }
    }
  }
  return true;
}

function onUpdate(record) {
  var roles = "";
  var dd = document.getElementsByName("roles");
  for (var i=0; i < dd.length; i++) {
    if (dd[i].selectedIndex > 0) {
      if (roles.length > 0) roles += ",";
      roles += dd[i].id.substring(4) + "-" + dd[i].value;
    }
  }
  N$("hdperm").value = roles;
  return true;
}


function moveRow(dir) {
  if ( ItemGrid.SelectedRecords[0] == null ) {
    alert(translate("Markera en post att flytta") + ".");
    return;
  }

  var currId = ItemGrid.SelectedRecords[0].id;
  Backweb.MoveRow(dir, currId);
  ItemGrid.refresh();
}


function onDelete(record) {
  return confirm(translate("Är du säker på att du vill ta bort backwebsidan") + " " + record["name"] + "?");
}

