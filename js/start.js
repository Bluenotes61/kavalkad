function initPage() {
}

function checkEnter(e) {
  var keynum = 0;
  if(window.event) keynum = e.keyCode;
  else if(e.which) keynum = e.which;
  if (keynum == 13) login();
}


function checkEnter(e) {
  var keynum = 0;
  if(window.event) keynum = e.keyCode;
  else if(e.which) keynum = e.which;
  if (keynum == 13) login();
}

function login() {
  StartPage.Login($("#username").val(), $("#password").val(), login2);
}

function login2(response) {
  if (response.value.length == 0)  document.location.reload();
  else {
    $("#loginerr").show();
    setTimeout("$('#loginerr').hide()", 2000);
  }
}
