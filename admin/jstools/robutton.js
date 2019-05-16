/* $Date: 2010-01-02 20:07:02 +0100 (lö, 02 jan 2010) $    $Revision: 5731 $ */
function RollOverButton(targetimg, param) {

  this.param = param;

  this.setup = function() {
    if (typeof (targetimg) != "object")
      targetimg = document.getElementById(targetimg);
    if (!targetimg || targetimg.tagName != "IMG") return;

    if (this.param.imageurl == null) this.param.imageurl = targetimg.src;
    if (this.param.rolloverimageurl == null) this.param.rolloverimageurl = targetimg.src;
    if (this.param.clickimageurl == null) this.param.clickimageurl = targetimg.src;
    if (this.param.href == null) this.param.href = null;
    if (this.param.enabled == null) this.param.enabled = true;
    if (this.param.staydown == null) this.param.staydown = false;
    if (this.param.selected == null) this.param.selected = false;
    if (this.param.onclick == null) this.param.onclick = null;

    this.img = targetimg;
    if (this.param.staydown && this.param.selected) this.img.src = this.param.clickimageurl;
    this.img.border = "0";

    this.anchor = document.createElement("A");
    this.anchor.href = (this.param.href ? this.param.href : "javascript:void(0)");
    this.anchor.onclick = this.param.onclick;
    this.anchor.onfocus = "this.blur()";

    this.img.parentNode.replaceChild(this.anchor, this.img);
    this.anchor.appendChild(this.img);
    this.setEvents();
  }

  this.select = function() {
    this.param.selected = !this.param.selected;
    this.img.src = (this.param.selected ? this.param.clickimageurl : this.param.imageurl);
    this.setEvents();
  }
  
  this.setImages = function(imageurl, rolloverimageurl, clickimageurl) {
    this.param.imageurl = imageurl;
    this.param.rolloverimageurl = (rolloverimageurl ? rolloverimageurl : imageurl);
    this.param.clickimageurl = (clickimageurl ? clickimageurl : imageurl);
    this.img.src = imageurl;
  }

  this.setEvents = function() {
    var thisref = this;

    if (this.param.staydown) {
      this.img.onmouseover = (this.param.selected ? null : function() { if (thisref.img.src != thisref.param.clickimageurl) thisref.img.src = thisref.param.rolloverimageurl });
      this.img.onmouseout = (this.param.selected ? null : function() { if (thisref.img.src != thisref.param.clickimageurl) thisref.img.src = thisref.param.imageurl });
      this.img.onmousedown = (this.param.selected ? null : function() { thisref.img.src = thisref.param.clickimageurl });
      this.img.onmouseup = (this.param.selected ? null : function() { if (thisref.img.src != thisref.param.clickimageurl) thisref.img.src = thisref.param.rolloverimageurl });
    }
    else {
      this.img.onmouseover = function() { thisref.img.src = thisref.param.rolloverimageurl };
      this.img.onmouseout = function() { thisref.img.src = thisref.param.imageurl };
      this.img.onmousedown = function() { thisref.img.src = thisref.param.clickimageurl };
      this.img.onmouseup = function() { thisref.img.src = thisref.param.rolloverimageurl };
    }
  }
  this.setup();
}