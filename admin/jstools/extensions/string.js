//-------- Extensions of String object ----------


String.prototype.reverse = function() {
  var res = "";
  for (var i = this.length; i > 0; --i) {
    res += this.charAt(i - 1);
  }
  return res;
}

String.prototype.trim = function(ch) {
  if (!ch) ch = ' ';
  return this.replace(new RegExp("^" + ch + "+|" + ch + "+$", "g"), "");
}

String.leftPad = function (val, size, ch) {
  var result = new String(val);
  if (ch == null) {
    ch = " ";
  }
  while (result.length < size) {
    result = ch + result;
  }
  return result;
}

String.escape = function(s) {
  return s.replace(/('|\\)/g, "\\$1");
}
