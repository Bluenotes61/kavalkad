tinyMCEPopup.requireLangPack();

var HtmlDialog = {
  init : function() {},
  insert : function() {
    var val = codeScripts(document.forms[0].htmlSnip.value);
    tinyMCEPopup.editor.execCommand('mceInsertContent', false, val);
    tinyMCEPopup.close();
  }
};

function codeScripts(val) {
  return val.
    replace(/<script/g, "<div class='nfnscript'>Javascript<div class='nfnscript_inner'><nfnscript").
    replace(/<\/script>/g, "</nfnscript></div></div>");
}

tinyMCEPopup.onInit.add(HtmlDialog.init, HtmlDialog);
