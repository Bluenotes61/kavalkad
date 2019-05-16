/* $Date: 2010-08-10 10:31:34 +0200 (ti, 10 aug 2010) $    $Revision: 6733 $ */

(function($){

  $.fn.inputwithhelp = function(helptext, options) {

    var settings = $.extend({}, $.fn.inputwithhelp.defaults, options);

    return this.each(function() {

      var obj = this;

      obj.init = function() {
        if ($(this).val().length == 0)
          $(this).val(helptext).css("color",settings.helpfontcolor);

        $(obj).focus(function(){
          if ($(this).val() == helptext)
            $(this).val("");
          $(this).css("color","");
        });

        $(obj).blur(function(){
          if ($(this).val().length == 0)
            $(this).css("color",settings.helpfontcolor).val(helptext);
          else
            $(this).css("color","");
        });
        initDone = true;
      }

      obj.val = function(aval) {
        if (aval != null)
          $(obj).val(aval).blur();
        else
          return ($(obj).val() == helptext ? "" : $(obj).val());
      }

      obj.init();
    });

  }

  $.fn.inputwithhelp_val = function(aval) {
    if (aval != null) {
      this.each(function() {
        this.val(aval);
      });
    }
    else {
      var val = "";
      this.each(function() {
        val += this.val();
      });
      return val;
    }
  };

  $.fn.inputwithhelp.defaults = {
    helpfontcolor: "#ccc"
  };

})(jQuery);

