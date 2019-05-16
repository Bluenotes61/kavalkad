(function($){

  $.fn.checkboxes = function(data, values, options) {
    if (!data) data = [];
    if (!values) values = [];

    return this.each(function() {

      var obj = this;
      obj.settings = jQuery.extend({}, jQuery.fn.checkboxes.defaults, options);

      obj.init = function() {
        var listyle = (obj.settings.orientation == "horiz" ? "style='display:inline'" : "");
        var ul = $("<ul />").css({"list-style-type":"none","margin":"0","padding":"0"});
        for (var i=0; i < data.length; i++) {
          var selected = jQuery.inArray(data[i].value, values) >= 0;
          ul.append(
            $("<li " + listyle + " />").append(
              $("<input />").attr("type","hidden").val(data[i].value)
            ).append(
              $("<img />").css("vertical-align","middle").attr({"src":(selected ? obj.settings.checkedimg : obj.settings.uncheckedimg),"alt":(selected ? "on" : "off")}).click(function(){obj.toggle(this.parentNode);}).mouseover(function(){obj.over(this.parentNode);}).mouseout(function(){obj.out(this.parentNode);})
            ).append(
              $("<label />").attr("href","javascript:void(0)").click(function(){obj.toggle(this.parentNode);}).html(data[i].text).mouseover(function(){obj.over(this.parentNode);}).mouseout(function(){obj.out(this.parentNode);})
            )
          );
        }
        $(obj).append(ul);
      },

      obj.toggle = function(li) {
        if (obj.settings.asradiobuttons) obj.setValues([]);
        var img = $(li).children("img");
        var checked = img.attr("alt") == "off";
        img.attr({
          "src":(checked ? obj.settings.checkedimg : obj.settings.uncheckedimg),
          "alt":(checked ? "on" : "off")
        });
        if (obj.settings.onclick) {
          var val = $(li).children("input").val();
          obj.settings.onclick(checked, val, obj);
        }
      }

      obj.over = function(li) {
        if (obj.settings.overimg.length > 0) {
          $(li).children("img").attr("src",obj.settings.overimg);
        }
      }

      obj.out = function(li) {
        if (obj.settings.overimg.length > 0) {
          var img = $(li).children("img");
          var checked = img.attr("alt") == "off";
          img.attr("src", (checked ? obj.settings.uncheckedimg : obj.settings.checkedimg));
        }
      }

      obj.getValues = function() {
        var res = [];
        $(this).find("img[alt=on]").each(function(){
          res.push($(this).prev().val());
        });
        return res;
      }

      obj.setValues = function(vals) {
        $(this).find("img").attr({"src":obj.settings.uncheckedimg, "alt":"off"});
        for (var i=0; i < vals.length; i++)
          $(this).find("input[value=" + vals[i] + "]").next().attr({"src":obj.settings.checkedimg, "alt":"on"});
      }

      obj.init();

    });

    return this;
  }


  $.fn.checkboxes_getValues = function() {
    var res = [];

    this.each(function() {
      $(this).find("img[alt=on]").each(function(){
        res.push($(this).prev().val());
      });
    });
    return res;
  }


  $.fn.checkboxes_setValues = function(vals) {
    return this.each(function() {
      $(this).find("img").attr({"src":this.settings.uncheckedimg, "alt":"off"});
      for (var i=0; i < vals.length; i++)
        $(this).find("input[value=" + vals[i] + "]").next().attr({"src":this.settings.checkedimg, "alt":"on"});
    });
  }

  $.fn.checkboxes.defaults = {
    checkedimg : "admin/jstools/jquery/plugins/checkboxes/on.gif",
    uncheckedimg : "admin/jstools/jquery/plugins/checkboxes/off.gif",
    overimg : "",
    orientation : "vert",
    asradiobuttons : false,
    onclick : null
  };

})(jQuery);

