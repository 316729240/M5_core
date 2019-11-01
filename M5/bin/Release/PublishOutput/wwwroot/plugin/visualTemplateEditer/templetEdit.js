var mainTab = {
    addItem: function (S) {
        var win = $(document.body).addControl({
            xtype: "Window",
            text: S.text,
            isModal: true,
            class: "form-horizontal",
            ico: S.ico,
            style: { width: 1000, height: 500 }
        });
        win.show();
        win.focus = function () {
            win.resize();
        };
        return win;
    }
};
var resizable = {
    focusObj: null,
    start: false,
    offset: null,
    dropObj: $("<span  class='m5_pix_drop' style='cursor:se-resize;position: absolute;float:left;background-color: #FF00FF;width:10px;height:10px;overflow:hidden;z-index:100000' ></span>")
};
$(function () {
    var css = ["/static/css/codemirror.css", "/static/skin/css/tree.css", "/static/skin/css/font-awesome.min.css", "/static/skin/css/other.css", "/manage/app/visualTemplateEditer/templetEdit.css"];
    for (var i = 0; i < css.length; i++) {
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
var findViewDiv = function (dom, k) {
    var name = k.split(",");
    var parent = dom;//.parent();
    if (parent.length == 0 || parent[0] == document.body) return;
    var className = parent.attr("class");
    //alert(parent.html());
    //alert(className);
    if (className != null) {
        for (var i = 0; i < name.length; i++) {
            if (className.indexOf(name[i]) > -1) return parent;
        }
    }
    return findViewDiv(parent.parent(), k);
};
var blankBox = $("<div class='blankBox text-center'>插入</div>");
var appendButton = '<div class="insertButton text-center"><button type="button" class="btn btn-primary">添加模块</button></div>';
var viewList = $(".m5_view");
var move = function (_box, x, y) {
    var target = findViewDiv($(event.target), "m5_template,insertButton");
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
var init = function () {
    //var obj = findViewDiv($(""), "m5_template");
    var lastMenuButton = $("<div class='' style='position: absolute;width:25px;z-index:10000'><button type=\"button\" class=\"btn btn-default m5_menu_button fa fa-cog\"></button></div>");//.appendTo($(document.body));
    var lastDataButton = $("<div class='btn-group btn-group-xs' style='position: absolute;width:100px;z-index:10000;float:none'><button type=\"button\" class=\"btn btn-default fa fa-pencil m5_editData\"></button><button type=\"button\" class=\"btn btn-default fa fa-trash-o m5_delData\"></button></div>");
    $(document.body).on("selectstart", function () { return false; });
    $(document.body).addClass("disableSelect");
    $(document.body).on("mouseup", function (e) {
        //var obj = $(e.target);
        //var obj = findViewDiv(obj, "m5_template");
        //alert(obj);
        //return;
        resizable.start = false;
        resizable.dropObj.remove();
    });
    $(document.body).on("mousemove", function (e) {
        var obj = $(e.target);
        var className = obj.attr("class");
        if (resizable.start) {
            var _position = resizable.offset;
            $("#saveTempletButton").html(_position.left + "_" + _position.top);
            var newW = e.pageX - _position.left, newH = e.pageY - _position.top;
            resizable.focusObj.css({ width: newW, height: newH });
            resizable.dropObj.css({ left: _position.left + newW - 5, top: _position.top + newH - 5 });
            //return;
        }
        if (className && (className.indexOf("m5_img") > -1 || className.indexOf("m5_pix_drop") > -1)) {
            if (className.indexOf("m5_pix_drop") > -1) return;
            $(document.body).append(resizable.dropObj);
            var _position = obj.offset();
            resizable.dropObj.css({ left: _position.left + obj.width() - 5, top: _position.top + obj.height() - 5 });
            resizable.focusObj = obj;
            //return;
        } else {
            if (resizable.focusObj && !resizable.start) {
                resizable.dropObj.remove();
                resizable.focusObj = null;
            }
        }
        var dataTitle = findViewDiv(obj, "m5_dataTitle");
        if (dataTitle == null) {
            lastDataButton.remove();
        } else {
            dataTitle.append(lastDataButton);
            var xy = dataTitle.position();
            lastDataButton.css({ top: xy.top, left: dataTitle.width() });
        }

        obj = findViewDiv(obj, "m5_template");

        if (obj == null) {
            lastMenuButton.remove(); return;
        }
        var xy = obj.position();
        obj.append(lastMenuButton);
        lastMenuButton.css({ top: xy.top, left: obj.width() });

    });
    $(document.body).on("mousedown", function (e) {
        var obj = $(e.target);
        var className = obj.attr("class");
        if (className && className.indexOf("m5_pix_drop") > -1) {
            resizable.start = true;
            resizable.offset = resizable.focusObj.offset();
            return;
        }
        if (className != null) {
            if (className.indexOf("m5_editData") > -1) {
                var href = $(e.target).parent().parent().find(".title").attr("href");
                $M.comm("visualTemplateEditer.getContentData", { url: href }, function (json) {
                    $M.app.interface("edit", json.dataTypeId, function (f) { f({ dataId: json.id, classId: json.classId, back: function () { location.reload(); } }) });
                });
            }
            else if (className.indexOf("m5_delData") > -1) {
                var href = $(e.target).parent().parent().find(".title").attr("href");
                $M.comm("visualTemplateEditer.getContentData", { url: href }, function (json) {
                    $M.confirm("您确定要删除所选数据吗？", function () {
                        $M.app.interface("del", json.dataTypeId, function (f) {
                            if (f) {
                                f({
                                    moduleId: json.moduleId, classId: json.classId, ids: json.id, type: 0, back: function () {
                                        location.reload();
                                    }
                                });
                            } else {
                                $M.comm("delData", { moduleId: json.moduleId, classId: json.classId, ids: json.id, tag: 0 }, function () {
                                    location.reload();
                                });
                            }
                        });
                    });
                });
            }
            else if (className.indexOf("m5_menu_button") > -1) {
                var templateObj = $(e.target).parent().parent();
                var m5_view = templateObj.find("> .m5_view");
                var m5_container = templateObj.find("> .m5_body > .m5_container");
                var className = templateObj.attr("class");
                if (m5_view.length > 0) {
                    var viewValue = m5_view.attr("viewvalue");
                    var l1 = viewValue.indexOf("("), l2 = viewValue.indexOf(")");
                    var p = "";
                    if(l1>-1 && l2>-1)p=viewValue.substr(l1 + 1, l2 - l1 - 1);
                    if (p!="") {
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
                        $M.app.call("$M.templateManage.viewEdit", {
                            viewName: viewValue, back: function (value) {
                                $M.comm("visualTemplateEditer.renderView", {
                                    viewName: viewValue
                                }, function (json) {
                                    templateObj.html(json);
                                });
                            }
                        });
                        //$M.app.call("$M.visualTemplateEditer.viewEdit", { viewName: viewValue, obj: m5_view });
                    }
                } else if (m5_container.length > 0) {
                    $M.app.call("$M.visualTemplateEditer.columnEdit", { obj: templateObj, value: m5_container.length });
                } else {
                    var type = templateObj.attr("type");
                    $M.app.call("$M.visualTemplateEditer.labelEdit", { obj: templateObj });
                }
            }
            return;
        }
        var obj = findViewDiv($(e.target), "m5_template");
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
                if (parentBox.children().length == 0) $(appendButton).appendTo(parentBox);
                blankBox.parent().find(">.insertButton").remove();
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
            var viewvalue = $(value).attr("viewvalue");
            $(value).parent().html("${" + viewvalue + "}");
        });
        html = temp.html();
        $M.comm("visualTemplateEditer.save", { url: location.href, html: html }, function () {
            $M.alert("保存成功！");
        });
        return false;
    });
    //$M.app.call("$M.article.edit", {});
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
    "/static/js/xml.js",
    "/manage/app/includeJS.ashx"
], init);