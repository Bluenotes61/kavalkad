function initPage() {
  $("#username").focus();

  $("#liform input").keypress(function (e) {
    if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13)) login();
  });
  $("#forgotform input").keypress(function (e) {
    if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13)) request();
  });
}

function login() {
  LoginPage.Login($("#username").val(), $("#password").val(), login2);
}
function login2(response) {
  if (response.value.length == 0)
    document.location.href = "/";
  else
    $('#forgotlink').slideDown('normal');
}

function openForgot() {
  $("#logindiv").slideUp('normal');
  $("#forgotdiv").slideDown('normal');
}

function closeForgot() {
  $("#forgotdiv").slideUp('normal');
  $("#logindiv").slideDown('normal');
}

function request() {
  LoginPage.RequestLogin($("#fusername").val(), $("#femail").val(), request2)
}
function request2(response) {
  if (response.value.indexOf("ok") == 0) {
    $("#reqemail").html(response.value.substring(2));
    $("#forgotdiv").slideUp('normal');
    $("#reqresult").slideDown('normal');
  }
  else
    $("#reqerr").html(response.value).slideDown('normal');
}

function okreq() {
  $("#okreq").slideUp('normal');
  $("#reqresult").slideUp('normal');
  $("#logindiv").slideDown('normal');
}

$(document).ready(function(){
   initPage();
});
