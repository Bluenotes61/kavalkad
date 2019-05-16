/* $Date: 2010-07-01 03:50:51 +0200 (to, 01 jul 2010) $    $Revision: 6649 $ */
function N$(id) {
  return document.getElementById(id);
}
function N$S(id){
  return document.getElementById(id).style;
}
function N$C(tag, attribs, styles){
  var tag = document.createElement(tag);
  if (attribs) tag = N$A(tag, attribs);
  if (styles) tag = N$AS(tag, styles);
  return tag;
}
function N$CA(tag, parent, attribs, styles){
  var e=N$C(tag, attribs, styles);
  parent.appendChild(e);
  return e;
}
function N$A(elem, attribs) {
  for(prop in attribs)
    elem[prop] = attribs[prop];
  return elem;
}
function N$AS(elem, styles) {
  for(prop in styles)
    elem.style[prop] = styles[prop];
  return elem;
}

function translate(txt) {
  var response = NFN.BaseMaster.Translate(txt);
  if (response.error) return txt;
  else return response.value;
}

function translateArr(txt) {
  var response = NFN.BaseMaster.TranslateArr(txt);
  if (response.error) return txt;
  else return response.value;
}

function nfndebug(mess) {
  try { console.log(mess); }
  catch (e) {}
}
