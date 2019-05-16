// Does not work in IE
jQuery.fn.addCssFile = function(css) {
  return this.each(function(){
    if (jQuery(this).html().indexOf(css) < 0)
      jQuery(this).append(
        jQuery("<link />").attr({"rel":"stylesheet", "type":"text/css", "href":css, "media":"screen,print"})
      );
  });
};

jQuery.fn.addJsFile = function(js) {
  return this.each(function(){
    if (jQuery(this).html().indexOf(js) < 0)
      jQuery(this).append(
        jQuery("<script />").attr({"type":"text/javascript", "src":js})
      );
  });
};


jQuery.fn.getCss = function(csselem, asNumber) {
  var res = "";
  var elem = jQuery(this).get()[0];
  if (elem.style[csselem] && elem.style[csselem].length > 0)
    res = elem.style[csselem];
  else {
    var classes = elem.className.split(" ");
    var cssRules = (document.all ? "rules" : "cssRules");
    for (var i=0; i < document.styleSheets.length && res.length == 0; i++){
      for (var j=0; j < document.styleSheets[i][cssRules].length && res.length == 0; j++) {
        var sel = document.styleSheets[i][cssRules][j].selectorText;
        if (sel == "#" + elem.id)
          res = document.styleSheets[i][cssRules][j].style[csselem];
        else {
          for (var k=0; k < classes.length; k++)
            if (sel == "." + classes[k]) res = document.styleSheets[i][cssRules][j].style[csselem];
        }
      }
    }
  }
  if (asNumber) {
    if (res.length == 0) return 0;
    return parseInt(res.substring(0, res.length-2));
  }
  else
    return res;
};

jQuery.validateEmail = function(email) {
  var emailRegExp = new RegExp("^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", "i");
  return emailRegExp.test(email);
}

jQuery.fn.screencenter = function(offset) {
  if (!offset) offset = {left:0, top:0};
  return this.each(function(){
    var top = jQuery(window).height()/2 - jQuery(this).height()/2 + jQuery(window).scrollTop() + offset.top;
    if (top < 0) top = 0;
    var left = jQuery(window).width()/2 - jQuery(this).width()/2 + jQuery(window).scrollLeft() + offset.left;
    jQuery(this).css({"left":left,"top":top});
  });
};

jQuery.fn.center = function (onlyvert, onlyhoriz) {
  this.css("position","absolute");
  if (!onlyhoriz) this.css("top", ( $(window).height() - this.height() ) / 2+$(window).scrollTop() + "px");
  if (!onlyvert) this.css("left", ( $(window).width() - this.width() ) / 2+$(window).scrollLeft() + "px");
  return this;
};
