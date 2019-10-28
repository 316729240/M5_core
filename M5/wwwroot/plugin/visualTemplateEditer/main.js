$M.visualTemplateEditer.labelEdit = function (S) {
    var win = new $M.dialog({
        title: "图文编辑",
        style: { width: 1000, height: 560 },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                var html = "";
                var _type = type.val();
                m5_body.removeClass("left1");
                m5_body.removeClass("left2");
                m5_body.removeClass("right1");
                m5_body.removeClass("right2");
                if (_type == "0") {
                    html = content.val();
                } else {
                    var newcss = "";
                    switch (_type) {
                        case "2":
                            newcss = "left1";
                            break;
                        case "3":
                            newcss = "left2";
                            break;
                        case "4":
                            newcss = "right1";
                            break;
                        case "5":
                            newcss = "right2";
                            break;
                    }
                    m5_body.addClass(newcss);
                    var imgHtml = "",conHtml="";
                    if (href.val()==""){
                        imgHtml = "<div class='m5_defImg'><img src='" + pic.val() + "' " + ((text.val() == "") ? "" : "title=\"" + text.val() + "\"") + " class='m5_img' ></div>";
                    } else {
                        imgHtml = "<div class='m5_defImg'><a href='" + href.val() + "' " + ((target.val() == 0) ? "" : "target=_blank") + "  ><img src='" + pic.val() + "' class='m5_img' " + ((text.val() == "") ? "" : "title=\"" + text.val() + "\"") + "  ></a></div>";
                    }
                    conHtml = "<div class='m5_defContent'>" + content.val() + "</div>";
                    if (_type == "6") {
                        html = conHtml + imgHtml;
                    } else {
                        html = imgHtml+conHtml;
                    }

                }
                S.obj.find(".m5_body").html(html);
                S.obj.find("h2").html(title.val());
                //alert(S.obj.find(".m5_body .m5_defImg img").attr("class"));
                //S.obj.find(".m5_body .m5_defImg img").css({width:img.width+"px",height:img.height+"px"});
                S.obj.find(".m5_body .m5_defImg img").width(size[0].val());
                S.obj.find(".m5_body .m5_defImg img").height(size[2].val());
            }
        }
    });
    var m5_body=S.obj.find(".m5_body");
    win.show();
    var tab = win.append("<div class=form-horizontal></div>");
    //var tab = win.addControl({ xtype: "Tab", items: [{ text: "常规", "class": "form-horizontal" }, { text: "高级" }] });
    var title = tab.addControl({ xtype: "TextBox", name: "title", labelText: "模块标题", labelWidth: 2, value: S.obj.find("h2").html() });
    var type = tab.addControl({
        xtype: "SelectBox", name: "title", labelText: "排版方式", labelWidth: 2, items: [
            { text: "无图", value: 0 },
            { text: "顶部居中", value: 1 },
            { text: "居左1", value: 2 },
            { text: "居左2", value: 3 },
            { text: "居右1", value: 4 },
            { text: "居右2", value: 5 },
            { text: "底部居中", value: 6 }
        ]
    });
    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, style: { height: 300 }, items: [{ size: 260 }, { size: "*" }] });
    var pic=frame.items[0].addControl({ labelText: "图片文件", labelWidth: 4, xtype: "UploadFileBox", name: "picUrl" });
    var size=frame.items[0].addControl({ labelText: "图片尺寸", labelWidth: 4, items: [{ xtype: "Spinner", style: { width: "60px", float: "left" }, name: "width", value: 0 }, { xtype: "Label", text: " * ", style: { float: "left" } }, { xtype: "Spinner", name: "height", value: 0, style: { width: "60px", float: "left" } }] });

    var href = frame.items[0].addControl({ labelText: "图片链接", labelWidth: 4, xtype: "TextBox", name: "href" });
    var target = frame.items[0].addControl({ labelText: "打开方式", labelWidth: 4, xtype: "SelectBox", name: "target", items: [{text:"当前窗口",value:0},{text:"新建窗口",value:1}] });
    var text=frame.items[0].addControl({ labelText: "图片描述", labelWidth: 4, xtype: "TextBox", name: "text" });
    var content = frame.items[1].addControl({ xtype: "Editor", name: "html", codemirror: true, sourceMode: true, dock: 2 });
    win.append("<div style='clear:both;'></div>");
    frame.resize();
    if (m5_body.find(".m5_defContent").length > 0) {
        content.val(m5_body.find(".m5_defContent").html());
        var m5_defImg_class = m5_body.attr("class");
        href.val(m5_body.find(".m5_defImg a").attr("href"));
        var img = m5_body.find(".m5_defImg img");
        pic.val(img.attr("src"));
        size[0].val(img.width());
        size[2].val(img.height());
        target.val(m5_body.find(".m5_defImg a").attr("target")=="_blank"?1:0);
        text.val(img.attr("title"));
        type.val(1);
        if (m5_defImg_class.indexOf("left1") > -1) type.val(2);
        else if (m5_defImg_class.indexOf("left2") > -1) type.val(3);
        else if (m5_defImg_class.indexOf("right1") > -1) type.val(4);
        else if (m5_defImg_class.indexOf("right2") > -1) type.val(5);
        if ($(m5_body.children()[0]).attr("class").indexOf("m5_defContent") > -1) type.val(6);
    } else {
        content.val(S.obj.find(".m5_body").html());
    }
};
$M.visualTemplateEditer.columnEdit = function (S) {
    var win = new $M.dialog({
        title: "分栏编辑",
        style: { width: 500, height: 360 },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                S.obj.find(".m5_title > h2").html(title.val());
                var _count = count.val();
                if (_count != S.value) {
                    var m5_body = S.obj.find("> .m5_body");
                    var m5_container = m5_body.find("> .m5_container");
                    m5_body.html("");
                    var appendButton = '<div class="insertButton text-center"><button type="button" class="btn btn-primary">添加模块</button></div>';
                    for (var i = 0; i < _count-1; i++) {
                        if (m5_container[i]){
                            $(m5_container[i]).appendTo(m5_body);
                            $(m5_container[i]).attr("class", "col-md-" + (12 / _count) + " m5_container");
                        } else {
                            var container = $("<div class=\"col-md-" + (12 / _count) + " m5_container\"></div>").appendTo(m5_body);
                            $(appendButton).appendTo(container);
                        }
                    }
                    var _html = "";
                    var container = $("<div class=\"col-md-" + (12 / _count) + " m5_container\"></div>").appendTo(m5_body);
                    for (var i = _count - 2; i < m5_container.length; i++) {
                        if ($(m5_container[i]).find(".insertButton").length == 0) {
                            _html += $(m5_container[i]).html();
                        }
                    }
                    if (_html == "") _html = appendButton;
                    container.html(_html);
                }
            }
        }
    });
    win.show();
    var tab = win.addControl({ xtype: "Tab", items: [{ text: "常规", "class": "form-horizontal" }] });
    var title = tab.items[0].addControl({ xtype: "TextBox", name: "title", labelText: "模块标题", labelWidth: 3, value: S.obj.find("h2").html() });
    var count = tab.items[0].addControl({ xtype: "SelectBox", name: "title", labelText: "选择分栏", labelWidth: 3, items: [2,3,4,6],value:S.value });
};
$M.visualTemplateEditer.listEdit = function (S) {
    var n1 = S.viewName.indexOf("("), n2 = S.viewName.indexOf(")");
    var p = S.viewName.substr(n1 + 1, n2 - n1 - 1).split(",");
    var win = new $M.dialog({
        title: "列表编辑",
        style: { width: 600, height: 500 },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                var viewName = S.viewName.substr(0, n1);
                viewName += "(\"" + title.val() + "\"," + style.val() + "," + (classId.data ? classId.data.id : 0) + ")";
                $M.comm("visualTemplateEditer.renderView", {
                    viewName: viewName
                }, function (json) {
                    S.obj.html(json);
                    S.obj.attr("viewValue", viewName);
                    S.obj.find(".m5_title h2").html(title.val());
                });
            }
        }
    });
    win.show();
    var tab = win.addControl({ xtype: "Tab", items: [{ text: "常规", "class": "form-horizontal" }, { text: "高级" }] });
    var title = tab.items[0].addControl({ xtype: "TextBox", name: "title", labelText: "模块标题", labelWidth: 2, value: S.obj.find(".m5_title h2").html() });
    var style = tab.items[0].addControl({ xtype: "RadioBox", name: "style", labelText: "列表样式", labelWidth: 2, items: [{ text: "列表一", value: 0 }, { text: "列表二", value: 1 }, { text: "列表三", value: 2 }], value: p[1] });
    var classId = tab.items[0].addControl({
        labelText: "栏目", labelWidth: 2, xtype: "DialogInput", name: "classId", inputReadOnly: true, style: { width: 120 },
        onButtonClick: function (sender, e) {
            var back = function (json) {
                sender.val(json.text);
                sender.data = json;
            };
            $M.dialog.selectColumn("", back);
        }
    });
};