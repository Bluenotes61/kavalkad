function initMaster() {
  if (typeof AjaxPro != "undefined" && AjaxPro !== null) {
    AjaxPro.timeoutPeriod = 1000 * 10;
    AjaxPro.onTimeout = function(b, res) { ajaxTimeout(b, res); }
    AjaxPro.onLoading = function(b) { showBusy(b); }
    AjaxPro.onError = function(res) { ajaxError(res); }
  }
  try { initAdmin();}
  catch (e) {};
  try { initPage();}
  catch (e) { };
}

function ajaxTimeout(b, res) {
  showBusy(false);
  $('#ajaxerr').html("Timeout i funktionen " + res.method).show();
  setTimeout("$('#ajaxerr').hide()", 3000);
}
function ajaxError(res) {
  showBusy(false);
  $('#ajaxerr').html(res.Message).show();
  setTimeout("$('#ajaxerr').hide()", 3000);
}
function showBusy(busy) {
  $("#ajaxbusy").css("display", (busy ? "block" : "none"));
}

$(document).ready(function(){
   initMaster();
});
