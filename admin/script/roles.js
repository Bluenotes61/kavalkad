/* $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ */
function initPage() {
  new RollOverButton("btn_excel", { rolloverimageurl: "gfx/excel_over.gif", clickimageurl: "gfx/excel_sel.gif", onclick: function() { exportTable('roles') } });
}

function onRowEdit(record) {
  var chb = document.getElementsByName("users");
  var users = record.users.split('<br />');
  for (var i=0; i < chb.length; i++)
    chb[i].checked = false;
  for (var i=0; i < chb.length; i++)
    for (var j=0; j < users.length; j++)
      if (chb[i].id == 'user' + users[j]) chb[i].checked = true;

  var chb = document.getElementsByName("perm");
  var perm = record.permissions.split('<br />');
  for (var i=0; i < chb.length; i++)
    chb[i].checked = false;
  for (var i=0; i < chb.length; i++) {
    for (var j=0; j < perm.length; j++) {
      var ptid = perm[j].split('-')[0];
      if (chb[i].id == 'perm' + ptid) chb[i].checked = true;
    }
  }

  return true;
}

function onUpdate(record) {
  var users = "";
  var chb = document.getElementsByName("users");
  for (var i=0; i < chb.length; i++) {
    if (chb[i].checked) {
      if (users.length > 0) users += ",";
      users += chb[i].id.substring(4);
    }
  }
  N$("hdusers").value = users;

  var perm = "";
  chb = document.getElementsByName("perm");
  for (var i=0; i < chb.length; i++) {
    if (chb[i].checked) {
      if (perm.length > 0) perm += ",";
      perm += chb[i].id.substring(4);
    }
  }
  N$("hdperm").value = perm;
}

function onDelete(record) {
  return confirm(translate("Är du säker på att du vill ta bort rollen") + " " + record["name"] + "?");
}

function popField(a, val) {
  var pos = $(a).offset();
  $("#popfield").css({left:pos.left,top:pos.top+14}).html(val).slideDown();
}