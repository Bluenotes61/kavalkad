/* $Date: 2010-03-17 13:53:26 +0100 (on, 17 mar 2010) $    $Revision: 6107 $ */

(function($){

  $.fn.combo = function(opt) {

    return this.each(function() {

      var obj = this;
      var settings = jQuery.extend({}, jQuery.fn.combo.defaults, opt);

      var orisel = obj.selectedIndex;
      obj.seloptions = [];
      obj.selectedIndex = 0;

      $(this).children().each(function(i) {
        var anopt = {
          value: $(this).val(),
          text:$(this).html()
        };
        obj.seloptions.push(anopt);
        if (anopt.value == orisel) obj.selectedIndex = i;
      });

      obj.selectedValue = obj.seloptions[obj.selectedIndex].text;

      obj.seldiv = $("<div />").addClass("cbo_seldivinner").html(obj.selectedValue).click(function(){dddiv.slideToggle('normal');});
      var outerseldiv = $("<div />").addClass("cbo_seldiv").append(obj.seldiv);
      var selarrow = $("<div />").addClass("cbo_selarrow").html("&nbsp;").click(function(){dddiv.slideToggle('normal');});
      var dddiv = $("<div />").addClass("cbo_options");
      for (var i=0; i < obj.seloptions.length; i++) {
        var anopt = $("<div />").attr("id","cbo_" + i).addClass("cbo_option").html(obj.seloptions[i].text).click(function(){obj.selVal(this);});
        if (i == obj.selectedIndex) anopt.addClass("cbo_seloption");
        dddiv.append(anopt);
      }
      var maindiv = $("<div />").append(outerseldiv).append(selarrow).append($("<div />").addClass("clearfloat")).append(dddiv);
      $(this).after(maindiv);
      $(this).css("display","none");

      obj.selVal = function(opt) {
        $(".cbo_seloption").removeClass("cbo_seloption");
        $(opt).addClass("cbo_seloption");
        var idx = parseInt($(opt).attr("id").substring(4));
        obj.selectedIndex = idx;
        obj.selectedValue = obj.seloptions[idx].value;
        obj.selectedText = obj.seloptions[idx].text;
        obj.seldiv.html(obj.selectedText);
        dddiv.slideToggle('normal');
        if (obj.onchange) obj.onchange();
      }
    });

  }

  $.fn.combo.defaults = {
    width:0
  };


})(jQuery);

