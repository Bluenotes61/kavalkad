/* $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ */
function initPage() {
  new RollOverButton("btn_excel", { rolloverimageurl: "gfx/excel_over.gif", clickimageurl: "gfx/excel_sel.gif", onclick: function() { exportTable('translations') } });
}

function onDelete(record) {
  return confirm(translate("Är du säker på att du vill ta bort frasen") + "?");
}
