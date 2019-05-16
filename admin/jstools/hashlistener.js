
var hashListener = {

  ie:   /MSIE/.test(navigator.userAgent),
  hash: document.location.hash,

  check:  function () {
    var h = document.location.hash;
    if (h != this.hash) {
      this.hash = h;
      var title = this.onHashChanged();
    }
  },

  checkIE:  function () {
    var h = document.location.hash;
    if (h != this.hash) setHash(h);
  },

  init: function () {
    if (this.ie) {
      var frame = document.createElement("iframe");
      frame.id = "state-frame";
      frame.style.display = "none";
      document.body.appendChild(frame);
    }

    var self = this;
    if ("onpropertychange" in document && "attachEvent" in document) {
      document.attachEvent("onpropertychange", function () {
        if (event.propertyName == "location") {
          self.check();
        }
      });
      window.setInterval(function () { self.checkIE() }, 200);
    }
    else
      window.setInterval(function () { self.check() }, 200);
  },

  setHash: function (h) {
    if (this.ie) this.writeFrame(h);
    else document.location.href = "#" + h;
  },

  getHash: function () {
    return document.location.hash.substring(1);
  },

  writeFrame: function (h) {
    var f = document.getElementById("state-frame");
    var d = f.contentDocument || f.contentWindow.document;
    d.open();
    d.write("<head><title>" + this.getTitle() + "</title><script>window._hash = '" + h + "'; window.onload = parent.hashListener.syncHash;<\/script></head>");
    d.close();
  },

  syncHash: function () {
    var h = this._hash;
    document.location.hash = h;
  },

  onHashChanged:  function () {},

  getTitle: function() { return document.title; }

};


