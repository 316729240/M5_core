$(function () {
    var css = ["/static/css/codemirror.css", "/static/skin/css/tree.css", "/static/skin/css/font-awesome.min.css", "/static/skin/css/other.css"];
    for (var i = 0; i < css.length;i++){
        var link = document.createElement('link');
        link.type = 'text/css';
        link.rel = 'stylesheet';
        link.href = css[i];
        document.getElementsByTagName("head")[0].appendChild(link);
    }
});
var loadJs = function (files, f) {
    var count = 0, max = files.length;
    var back = function () {
        count++;
        if (max == count) f();
        else {
            $.getScript(files[count], back);
        }
    };
    $.getScript(files[count], back);
};
var findViewDiv = function (dom) {
    var parent = dom.parent();
    if (parent.length == 0 || parent[0] == document.body) return;
    var className = parent.attr("class");
    if (className != null && className.indexOf("m5_template") > -1) {
        return parent;
    } else {
        return findViewDiv(parent);
    }
};
var blankBox = $("<div class='blankBox text-center'>插入</div>");
var appendButton = '<div class="m5_template insertButton text-center"><button type="button" class="btn btn-primary">添加模块</button></div>';
var viewList = $(".m5_view");
var move = function (_box, x, y) {
    var target = findViewDiv($(event.target));
    if (target == null) return;
    if (target) {
        var xy = target.offset();
        if (event.pageY < (xy.top + target.height() / 2)) {
            blankBox.insertBefore(target);
        } else {
            blankBox.insertAfter(target);
        }
    }
};
var mainTab = null;
var init = function () {
    var lastMenuButton = $("<span class='m5_menu_button fa fa-cog' style='position: absolute;width:25px;z-index:10000'></span>");//.appendTo($(document.body));
    $(document.body).on("selectstart", function () { return false; });
    $(document.body).addClass("disableSelect");

    $(document.body).on("mousemove", function (e) {
        var obj = findViewDiv($(e.target));
        if (obj == null) {
            lastMenuButton.remove(); return;
        }
        var xy = obj.position();
        obj.append(lastMenuButton);
        lastMenuButton.css({ top: xy.top, left: obj.width() });

    });
    $(document.body).on("mousedown", function (e) {
        var className = $(e.target).attr("class");
        if (className != null && className.indexOf("m5_menu_button") > -1) {
            var templateObj = $(e.target).parent();
            var className = templateObj.attr("class");
            if (className.indexOf("m5_label") > -1) {
                var type = templateObj.attr("type");
                $M.app.call("$M.visualTemplateEditer.labelEdit", { obj: templateObj });
            } else if (className.indexOf("m5_view") > -1) {
                var viewValue = templateObj.attr("viewvalue");
                if (viewValue.indexOf("文章列表") > -1) {
                    $M.app.call("$M.templateManage.insertView", {
                        viewName: viewValue, back: function (value) {
                            $M.comm("visualTemplateEditer.renderView", {
                                viewName: value
                            }, function (json) {
                                templateObj.html(json);
                                templateObj.attr("viewValue", value);
                            });
                        }
                    });
                } else {
                    $M.app.call("$M.visualTemplateEditer.viewEdit", { viewName: viewValue, obj: templateObj });
                }
            }
            return;
        }
        var obj = findViewDiv($(e.target));
        if (obj == null) return;
        var parentBox = obj.parent();
        obj.moveBox({
            html: "<div class='m5_movebox'>对象</div>",
            onMove: function (sender, e) {
                move($(sender), e.x, e.y);
            },
            onStart: function (sender, e) {
                blankBox.insertAfter(obj);
                obj.hide();
            },
            onEnd: function (sender, e) {
                obj.insertAfter(blankBox);
                obj.show();
                if (parentBox.children().length == 0) {
                    $(appendButton).appendTo(parentBox);
                } else {
                    blankBox.parent().find(".insertButton").remove();
                }
                blankBox.remove();
            }
        });
    });
    var oldHtml = $(document.body).html();
    $(document.body).html("");
    $('<div class="navbar navbar-inverse navbar-fixed-top"><button id=saveTempletButton>保存</button></div><div class=templetBox id=templetBox></div>').appendTo($(document.body));
    $("#templetBox").html(oldHtml);
    $.each($("#templetBox .m5_container"), function (n, value) {
        if ($(value).children().length == 0) $(value).append(appendButton);
    });
    var temp = $("<div/>");
    $("#saveTempletButton").click(function () {
        var html = $("#templetBox").html();
        temp.html(html);
        temp.find(".insertButton").remove();
        var viewList = temp.find(".m5_view");


        $.each(viewList, function (n, value) {
            $(value).html("");
        });
        html = temp.html();
        $M.comm("visualTemplateEditer.save", { url: location.href, html: html }, function () {
            $M.alert("保存成功！");

        });
        return false;
    });
};

loadJs([
    "/static/js/baseControl.js",
    "/static/js/frame.js",
    "/static/js/jquery.validate.min.js",
    "/static/js/validate-methods.js",
    "/static/js/messages_zh.js",
    "/static/js/config.js",
    "/static/js/dialog.js",
    "/static/js/jquery.minicolors.js",
    "/static/js/codemirror.js",
    "/static/js/editor.js",
    "/static/js/extend.js",
    "/manage/app/includeJS.ashx"
], init);