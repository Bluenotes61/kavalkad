/* $Date: 2009-11-09 01:03:45 +0100 (må, 09 nov 2009) $    $Revision: 5604 $ */
var SimpleContextMenu = {
  _menus : new Array,
  _attachedElement : null,
  _menuElement : null,
  _preventDefault : true,
  _preventForms : true,
  _attachedArr : new Array(),

  target : null,
  onShow : null,

  setup : function (conf) {
    if ( document.all && document.getElementById && !window.opera ) SimpleContextMenu.IE = true;
    if ( !document.all && document.getElementById && !window.opera ) SimpleContextMenu.FF = true;
    if ( document.all && document.getElementById && window.opera ) SimpleContextMenu.OP = true;

    if ( SimpleContextMenu.IE || SimpleContextMenu.FF ) {

      if (document.attachEvent) document.attachEvent("onclick", SimpleContextMenu._hide);
      else document.addEventListener("click", SimpleContextMenu._hide, false);

      if (conf && typeof(conf.preventDefault) != "undefined") SimpleContextMenu._preventDefault = conf.preventDefault;

      if (conf && typeof(conf.preventForms) != "undefined") SimpleContextMenu._preventForms = conf.preventForms;
    }

  },


  attachTo : function(itemId, menuId) {
    if (N$(itemId)) {
      N$(itemId).oncontextmenu = SimpleContextMenu._show;
      SimpleContextMenu._attachedArr.push([itemId, menuId]);
    }
  },


  detach : function() {
    SimpleContextMenu._attachedArr.clear();
  },


  _getMenuElementId : function (e) {

    SimpleContextMenu._attachedElement = (SimpleContextMenu.IE ? event.srcElement : e.target);

    while (SimpleContextMenu._attachedElement != null) {
      for (var i=0; i < SimpleContextMenu._attachedArr.length; i++) {
        var elem = SimpleContextMenu._attachedArr[i];
        if (elem[0] == SimpleContextMenu._attachedElement.id) return elem[1];
      }

      SimpleContextMenu._attachedElement = (SimpleContextMenu.IE ? SimpleContextMenu._attachedElement.parentElement : SimpleContextMenu._attachedElement.parentNode);
    }

    return null;

  },


  // private method. Shows context menu
  _getReturnValue : function (e) {

    var returnValue = true;
    var evt = SimpleContextMenu.IE ? window.event : e;

    if (evt.button != 1) {
      if (evt.target) var el = evt.target;
      else if (evt.srcElement) var el = evt.srcElement;

      var tname = el.tagName.toLowerCase();

      if ((tname == "input" || tname == "textarea")) {
        if (!SimpleContextMenu._preventForms) returnValue = true;
        else returnValue = false;
      }
      else {
        if (!SimpleContextMenu._preventDefault) returnValue = true;
        else returnValue = false;
      }
    }

    return returnValue;

  },


  // private method. Shows context menu
  _show : function (e) {

    SimpleContextMenu._hide();
    var menuElementId = SimpleContextMenu._getMenuElementId(e);

    if (menuElementId) {
      var m = SimpleContextMenu._getMousePosition(e);
      var s = SimpleContextMenu._getScrollPosition(e);

      SimpleContextMenu._menuElement = document.getElementById(menuElementId);
      SimpleContextMenu._menuElement.style.left = m.x + s.x + 'px';
      SimpleContextMenu._menuElement.style.top = m.y + s.y + 'px';
      SimpleContextMenu._menuElement.style.display = 'block';
      SimpleContextMenu.target = SimpleContextMenu._attachedElement;
      if (SimpleContextMenu.onShow && typeof(SimpleContextMenu.onShow) == "function")
        SimpleContextMenu.onShow(SimpleContextMenu.target);

      return false;
    }

    return SimpleContextMenu._getReturnValue(e);

  },


  // private method. Hides context menu
  _hide : function () {

    if (SimpleContextMenu._menuElement) {
      SimpleContextMenu._menuElement.style.display = 'none';
    }

  },


  // private method. Returns mouse position
  _getMousePosition : function (e) {

    e = e ? e : window.event;
    var position = {
      'x' : e.clientX,
      'y' : e.clientY
    }

    return position;

  },


  // private method. Get document scroll position
  _getScrollPosition : function () {

    var x = 0;
    var y = 0;

    if( typeof( window.pageYOffset ) == 'number' ) {
      x = window.pageXOffset;
      y = window.pageYOffset;
    } else if( document.documentElement && ( document.documentElement.scrollLeft || document.documentElement.scrollTop ) ) {
      x = document.documentElement.scrollLeft;
      y = document.documentElement.scrollTop;
    } else if( document.body && ( document.body.scrollLeft || document.body.scrollTop ) ) {
      x = document.body.scrollLeft;
      y = document.body.scrollTop;
    }

    var position = {
      'x' : x,
      'y' : y
    }

    return position;

  }

}

SimpleContextMenu.setup({'preventDefault':true, 'preventForms':false});
