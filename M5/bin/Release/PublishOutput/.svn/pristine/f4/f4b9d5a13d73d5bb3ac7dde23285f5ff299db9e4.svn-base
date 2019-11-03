$M.Control["Thumbnail"] = function (BoxID, S) {
    var T = this;
    var A = $("<div style='overflow:auto'/>").appendTo(BoxID);
    T.items = [];
    var columnCount = 6;
    if (S.columnCount) columnCount = S.columnCount;
    var size = 12 / columnCount;
    var item = function (json) {
        var T2 = this;
        var picbox = $("<div class=\"col-xs-" + size + " col-md-" + size + "\" ><a href=\"#\" class=\"thumbnail\"  style='height:" + S.picHeight + "px'  ><img src='" + json.url + "' ></a></div>").appendTo(A);
        T2.attr = function (a, b) {
            if (b != null) json[a] = b;
            return json[a];
        };
        picbox.click(function () {
            if (S.onSelectionChanaged) S.onSelectionChanaged(T, { item: T2 });
        });
    };
    T.addItem = function (json) {
        var obj = new item(json);
        T.items[T.items.length] = obj;
        return obj;
    };
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    if (S.style) A.css(S.style);
};
$M.Control["Popover"] = function (BoxID, S) {
    var T = this;
    var jiantou = "";
    var A = $("<div class=\"popover fade in\" tabindex=\"-1\" style=\"z-index:" + ($M.zIndex + 1) + "\"  ></div>").appendTo($(document.body));
    var B = $("<div class=\"arrow\" ></div>").appendTo(A);
    var title = null;
    if (S.title) title = $("<h3 class=\"popover-title\" ></h3>").appendTo(A);
    var content = $("<div class=\"popover-content\"></div>").appendTo(A);
    A.css({ "max-width": "1000px" });
    T.container = content;
    $M.BaseClass.apply(T, [S]);
    T.show = function (obj) {
        var x1 = obj.offset().left, y1 = obj.offset().top, w1 = obj.width(), h1 = obj.height();
        var x2 = A.offset().left, y2 = A.offset().top, w2 = A.width(), h2 = A.height();

        var pageWidth = $(window).width(), pageHeight = $(window).height();
        if (jiantou != "") A.removeClass(jiantou);
        var x = 0, y = 0;
        if (S.location == null) {
            if (y1 > h2) { jiantou = "top"; }
            else if ((pageWidth - x1 - w1) > w2) { jiantou = "right"; }
            else if ((pageHeight - y1 - h1) > h2) { jiantou = "bottom"; }
            else if (x1 > w2) { jiantou = "left"; }
        } else {
            jiantou = S.location;
        }
        switch (jiantou) {
            case "top":
                x = (x1 + w2) > pageWidth ? (x1 + w1 - w2) : x1;
                y = y1 - h2; B.css({ left: x1 - x + w1 / 2 + "px", top: "" });
                break;
            case "right":
                x = x1 + w1; y = y1; B.css({ top: h1 / 2 + "px", left: "" });
                break;
            case "bottom":
                x = (x1 + w2) > pageWidth ? (x1 + w1 - w2) : x1;
                y = y1 + h1; B.css({ left: x1 - x + w1 / 2 + "px", top: "" });
                break;
            case "left":
                //alert([x1,w1,x1-w1]);
                x = x1 - w2; y = (y1 - (h2 -h1)/ 2>1)?y1 - (h2 -h1)/ 2:1 ; B.css({ top: y1+h1/ 2 + "px", left: "" });
                break;
        }
        if (jiantou != "") A.addClass(jiantou);
        //$M.lock(A, true, T, false);
        A.css({ left: x + "px", top: y + "px" });
        A.show();
        $M.focusElement = A[0];
    };

    T.close = function () {
        A.hide();
        if (S.onClose) S.onClose(T, null);
    };
    T.dispose = function () {
        if (T.controls) {
            var count = T.controls.length;
            for (var i = count - 1; i > -1; i--) {
                T.controls[i].dispose();
            }
        }
        $(document).unbind("keydown", keydown);
        $(document).unbind("mousedown", mousedown);
        A.remove();
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) $M.focusElement = null;
        A = null;
    };
    T.loseFocus = function () {
        T.remove();
    };
    var keydown = function (e) {
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) {
            if (e.which == 27) {
                T.remove();
            }
            if (S.onKeyDown) S.onKeyDown(T, e);
        }
    };
    var mousedown = function (e) {
        if (A.has(e.target).length == 0) T.close();
    };
    $(document).on("keydown", keydown);
    $(document).on("mousedown", mousedown);
    if (S.style) A.css(S.style);
};

$M.Control["ToolTip"] = function (BoxID, S) {
    var T = this;
    var jiantou = "";
    var A = $("<div class=\"tooltip\" tabindex=\"-1\" style=\"display:none;z-index:" + ($M.zIndex + 1) + "\"  ></div>").appendTo($(document.body));
    var B = $("<div class=\"tooltip-arrow\" ></div>").appendTo(A);
    var title = null;
    //if (S.title) title = $("<h3 class=\"popover-title\" ></h3>").appendTo(A);
    var content = $("<div class=\"tooltip-inner\"></div>").appendTo(A);
    A.css({ "max-width": "1000px" });
    T.container = content;
    $M.BaseClass.apply(T, [S]);
    T.show = function (obj) {
        obj = $(obj);
        var x1 = obj.offset().left, y1 = obj.offset().top, w1 = obj.width(), h1 = obj.height();
        var x2 = A.offset().left, y2 = A.offset().top, w2 = A.width(), h2 = A.height();

        var pageWidth = $(window).width(), pageHeight = $(window).height();
        if (jiantou != "") A.removeClass(jiantou);
        var x = 0, y = 0;
        if (S.location == null) {
            if (y1 > h2) { jiantou = "top"; }
            else if ((pageWidth - x1 - w1) > w2) { jiantou = "right"; }
            else if ((pageHeight - y1 - h1) > h2) { jiantou = "bottom"; }
            else if (x1 > w2) { jiantou = "left"; }
        } else {
            jiantou = S.location;
        }

        switch (jiantou) {
            case "top":
                x = (x1 + w2) > pageWidth ? (x1 + w1 - w2) : x1;
                y = y1 - h2;
                //B.css({ left: x1 - x + w1 / 2 + "px", top: "" });
                break;
            case "right":
                x = x1 + w1; y = y1; B.css({ top: h1 / 2 + "px", left: "" });
                break;
            case "bottom":
                x = (x1 + w2) > pageWidth ? (x1 + w1 - w2) : x1;
                y = y1 + h1;
                //B.css({ left: x1 - x + w1 / 2 + "px", top: "" });
                break;
            case "left":
                //alert([x1,w1,x1-w1]);
                x = x1 - w2; y = (y1 - (h2 - h1) / 2 > 1) ? y1 - (h2 - h1) / 2 : 1; B.css({ top: y1 + h1 / 2 + "px", left: "" });
                break;
        }
        if (jiantou != "") A.addClass(jiantou);
        //$M.lock(A, true, T, false);
        A.css({ left: x + "px", top: y + "px", opacity: 1, transition: '0.3s ease-out' });
        A.show();
        $M.focusElement = A[0];
    };
    T.close = function () {
        A.hide();
        A.css({ opacity: 0, transition: '0.3s ease-out' });
        if (S.onClose) S.onClose(T, null);
    };
    T.dispose = function () {
        if (T.controls) {
            var count = T.controls.length;
            for (var i = count - 1; i > -1; i--) {
                T.controls[i].dispose();
            }
        }
        $(document).unbind("keydown", keydown);
        $(document).unbind("mousedown", mousedown);
        A.remove();
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) $M.focusElement = null;
        A = null;
    };
    T.loseFocus = function () {
        T.remove();
    };
    var keydown = function (e) {
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) {
            if (e.which == 27) {
                T.remove();
            }
            if (S.onKeyDown) S.onKeyDown(T, e);
        }
    };
    var mousedown = function (e) {
        if (A.has(e.target).length == 0) T.close();
    };
    $(document).on("keydown", keydown);
    $(document).on("mousedown", mousedown);
    if (S.style) A.css(S.style);
};
$M.Control["ToolBar"] = function (BoxID, S) {
    var T = this;
    T.controls = [];
    var A = $("<div class=\"btn-toolbar\" ></div>").appendTo(BoxID);
    for (var i = 0; i < S.items.length; i++) {
        if (S.items[i].length != null) {
            var group = $("<div class=\"btn-group\"></div>").appendTo(A);
            for (var i1 = 0; i1 < S.items[i].length; i1++) {
                if (!S.items[i][i1].xtype) S.items[i][i1].xtype = "Button";
                if (S.size != null) S.items[i][i1].size = S["size"];
                T.controls[T.controls.length] = group.addControl(S.items[i][i1]);
            }
        } else {
            if (!S.items[i].xtype) S.items[i].xtype = "Button";
            if (S.size != null) S.items[i].size = S["size"];
            T.controls[T.controls.length] = A.addControl(S.items[i]);
        }
    }
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    if (S["class"]) A.addClass(S["class"]);
};
//-----------------------------------------------------------
//表单控件
//属性
//command 提交地址或命令名称
//templateUrl 表单模板可以是一个html网页
//事件
//onLoad 模板加载完成
//onSubmit 表单提交完成
//onProgress 进度条
//-----------------------------------------------------------
$M.Control["Form"] = function(BoxID, S, CID) {
    var T = this;
    T.items = new Array();
    var A = null; //
    if (BoxID == null) {
        A = CID;
    } else {
        A = $("<form></form>").appendTo(BoxID);
    }
    if (S.url == null) {
        S.url = S.action == null ? "" : S.action;
        if (S.url == null) S.url = "";
    }
    T.container = A;
    //A.addClass("form-horizontal");
    $M.BaseClass.apply(T, [S]);
    if (S["class"]) A.addClass(S["class"]);
    var data = {};
    var getData = function(obj) {
        if (obj.controls == null) return;
        for (var i = 0; i < obj.controls.length; i++) {
            if (obj.controls[i] != null) {
                if (obj.controls[i] && obj.controls[i].attr) {
                    var commit = obj.controls[i].attr("commit");
                    var name = obj.controls[i].attr("name");
                    if (name != null && commit != false) {
                        data[name] = obj.controls[i].val();
                    }
                }
                getData(obj.controls[i]);
            }
        }
    };
    var submitForm = function() {
        if (S.onBeginSubmit) {
            if (S.onBeginSubmit(T) == false) return;
        }
        data = {};
        var list = A.find("input");

        for (var i = 0; i < list.length; i++) {
            var name = $(list[i]).attr("name");
            if ($(list[i]).attr("type") == "file") {
                data[name] = $(list[i])[0].files;
            } else if ($(list[i]).attr("type") == "checkbox") {
                if ($(list[i]).is(':checked')) {
                    if (data[name] == null) {
                        data[name] = [$(list[i]).val()];
                    } else {
                        data[name][data[name].length] = $(list[i]).val();
                    }
                }
                //data[name] = $(list[i]).is(':checked') ? $(list[i]).val() : "";
            } else if ($(list[i]).attr("type") == "radio") {
                if ($(list[i]).is(':checked')) {
                    data[name] = $(list[i]).val();
                }
            } else {
                data[name] = $(list[i]).val();
            }
        }
        list = A.find("select");
        for (var i = 0; i < list.length; i++) {
            data[$(list[i]).attr("name")] = $(list[i]).val();
        }
        list = A.find("textarea");
        for (var i = 0; i < list.length; i++) {
            data[$(list[i]).attr("name")] = $(list[i]).val();
        }

        getData(T);
        if (S.command) {
            $M.comm(S.command, data, function(userData) { if (S.onSubmit) S.onSubmit(T, { "formData": data, "returnData": userData }); }, S.onSubmitErr);
        } else if (S.url) {
            /*if (A.attr("enctype") == "multipart/form-data") {
            var fd = new FormData();
            for (var o in data) {
            fd.append(o, data[o]);
            }
            $M.ajax(S.url, fd, function(userData) {
            if (S.onSubmit) S.onSubmit(T, { "formData": data, "returnData": userData });
            });
            } else {
            $M.ajax(S.url, data, function(userData) {
            if (S.onSubmit) S.onSubmit(T, { "formData": data, "returnData": userData });
            });
            }*/
            $M.ajax(S.url, data, function(userData) {
                if (S.onSubmit) S.onSubmit(T, { "formData": data, "returnData": userData });
            }, null, function(e) {
                var percent = Math.round(e.loaded / e.total * 100);
                if (S.onProgress != null) S.onProgress(T, { value: percent, loaded: e.loaded, total: e.total });
            });
        } else {
            S.onSubmit(T, { "formData": data });
            return false;
        }

    };
    var errorPlacement = null;
    var invalidHandler = null;
    if (S.errorshowtype == 1) {
        errorPlacement = function() {
            return false;
        };
        invalidHandler = function(form, validator) {
            $.each(validator.invalid, function(key, value) {
                tmpkey = key;
                tmpval = value;
                validator.invalid = {};
                validator.invalid[tmpkey] = value;
                alert(value);
                return false;
            });
        };
    }

    if (A.validate) {
        A.validate({
            errorClass: "label-error",
            errorElement: "span",
            highlight: function(element) {
                if (element._control) {
                    $(element._control).closest('div').addClass("has-error");
                } else {
                    $(element).closest('div').addClass("has-error");
                }
            },
            unhighlight: function(element) {
                if (element._control) {
                    $(element._control).closest('div').removeClass("has-error");
                } else {
                    $(element).closest('div').removeClass("has-error");
                }

            },
            errorPlacement: function(error, element) {
                if (S.errshowtype != "0") {
                    if (element[0]._control) {
                        error.insertAfter($(element[0]._control));
                    }
                    else {
                        error.insertAfter($(element[0]));
                    }
                }
            },
            invalidHandler: invalidHandler,
            submitHandler: submitForm
        });
    } else {
        alert("没有加载插件jquery.validate.js");
    }
    T.submit = function() {
        A.submit();
    };
    T.find = function(name) {
        if (T.controls) {
            for (var i = 0; i < T.controls.length; i++) {
                if (T.controls[i].attr("name") == name) {
                    return (T.controls[i]);
                } else {
                    if (T.controls[i].find) {
                        var c = T.controls[i].find(name);
                        if (c) return c;
                    }
                }
            }
        }
        var obj = A.find("[name='" + name + "']");
        if (obj.length > 0) return obj;
        return null;
    };
    T.append = function(str) {
        return ($(str).appendTo(A));
    };
    T.val = function(value) {
        var list = A.find("input");
        for (var i = 0; i < list.length; i++) {
            var obj = $(list[i]);
            var name = obj.attr("name");
            if (obj.attr("type") == "checkbox" || obj.attr("type") == "radio") {
                var v = value[name] + "";
                if (v != null) {
                    var vl = v.split(',');
                    obj.prop("checked", vl.indexOf(obj.val()) > -1);
                }
            } else if (obj.attr("type") == "file") {

            } else {
                if (value[name] != null) $(list[i]).val(value[name]);
            }
        }
        list = A.find("select");
        for (var i = 0; i < list.length; i++) {
            var name = $(list[i]).attr("name");
            if (name && value[name] != null) $(list[i]).val(value[name]);
        }
        list = A.find("textarea");
        for (var i = 0; i < list.length; i++) {
            $(list[i]).val(value[$(list[i]).attr("name")]);
        }
        if (T.controls) {
            for (var i = 0; i < T.controls.length; i++) {

                if (T.controls[i].val) {
                    var v = value[T.controls[i].attr("name")]
                    if (v != null) T.controls[i].val(v);
                }
            }
        }
    };
    T.read = function(comm, id, back) {
        if (id != null) {
            $M.comm(comm, { id: id }, function(json) {
                T.val(json);
                if (back) back(json);
            });
        }
    };
    //重置表单
    T.reset = function() {
        var list = A.find("input");
        for (var i = 0; i < list.length; i++) {
            var obj = $(list[i]);
            var name = obj.attr("name");
            if (name) {
                var defaultValue = $(list[i]).attr("defaultValue");
                $(list[i]).val(defaultValue == null ? "" : defaultValue);
            }
        }
        list = A.find("select");
        for (var i = 0; i < list.length; i++) {
            var name = $(list[i]).attr("name");
            if (name) {
                var defaultValue = $(list[i]).attr("defaultValue");
                $(list[i]).val(defaultValue == null ? "" : defaultValue);
            }
        }
        list = A.find("textarea");
        for (var i = 0; i < list.length; i++) {
            var defaultValue = $(list[i]).attr("defaultValue");
            $(list[i]).val(defaultValue == null ? "" : defaultValue);
        }
        if (T.controls) {
            for (var i = 0; i < T.controls.length; i++) {
                if (T.controls[i].val) {
                    var defaultValue = $(list[i]).attr("defaultValue");
                    T.controls[i].val(defaultValue == null ? "" : defaultValue);
                }
            }
        }
    };
    if (S.templateUrl) {
        A.load(S.templateUrl + "?" + $M.getId(), function() {
            T.render();
            if (S.onLoad) S.onLoad(T, null);
        });
    }
};
$M.Control["Frame"] = function (BoxID, S) {
    //console.log("框架改变");
    var T = this;
    T.items = new Array();
    var A = null;
    var lineSize = 10;
    if (BoxID[0] == $('body')[0]) {
        A = BoxID;

    }
    else {
        A = BoxID;
    }
    A.addClass("M5_Frame");
    var lines = new Array();
    var item = function (S2) {
        var T2 = this;
        T.items[T.items.length] = T2;
        //var subControl = [];
        var box = null, content = null, head = null, contentDiv = null, title = null;
        if (S2.text) {
            var html = "<div class='box w-box' ><div class=\"w-box-header\">";
            html += "";
            if (S2.ico) html += "<i class=\"fa " + S2.ico + "\"></i> ";
            html += "<span class=\"w-box-title\">" + S2.text + "</span>";
            html += "<span class=\"w-box-options\"></span>";
            html += "";
            html += "</div><div class=\"w-box-content\" style=\"overflow:auto\"></div></div>";
            //    box = $("<div class='box w-box' ><div class=\"w-box-header\">" + S2.text + "</div><div class=\"w-box-content\"></div></div>").appendTo(A);
            box = $(html).appendTo(A);
            head = box.find(".w-box-header");
            title = box.find(".w-box-title");
            if (S2.buttons) {
                var options = box.find(".w-box-options");
                options =$("<div class=' btn-group'></div>").appendTo(options);
                for (var i = 0; i < S2.buttons.length; i++) {
                    //var button=$("<a href='#' class='fa " + S2.buttons[i].ico + "'></a>").appendTo(options);
                    S2.buttons[i].xtype = "Button";
                    S2.buttons[i].size = 0;
                    options.addControl(S2.buttons[i]);
                    //button.click(S2.buttons[i].onClick);
                }
            }
            content = box.find(".w-box-content");
            contentDiv = content;
            //if(S2.items)
            //var t1 = $("<i class='fa fa-chevron-down'></i>").appendTo(head);
            //t1.addControl({ xtype: "Button", ico: "fa-mobile" });
        } else {
            box = $("<div class='box' />").appendTo(A);
            contentDiv = box;
        }
        if (S2.visible == false) box.hide();
        contentDiv = content ? content : box;
        T2.container = contentDiv;
        $M.BaseClass.apply(T2, [S2]);
        T2.attr = function (a, b) {
            if (a == "visible") {
                if (b != null) {
                    S2[a] = b;
                    if (b) { box.show(); } else { box.hide(); }
                    T.resize();
                }
            }
            if (b != null) {
                S2[a] = b;
                if (a == "text" && title) title.html(b);
            }
            return (S[a]);
        };
        T2.css = function (style) {
            box.css(style);
            if (content) {
                content.css({ height: box.height() - head.height() + "px" });
            }
            resize();
        };
        if (S2["class"]) box.addClass(S2["class"]);
        if (S2.style) contentDiv.css(S2.style);
        var resize = function () {
            //alert(T2.controls);
            var w = contentDiv.width(), h = contentDiv.height();
            if (T2.controls) {
                //console.log(T2.controls.length);
                var countHeight = 0;
                var dockC = [];
                for (var i = 0; i < T2.controls.length; i++) {
                    if (T2.controls[i].attr("dock") == 2) {
                        dockC[dockC.length] = T2.controls[i];
                    } else {
                        //try{
                        countHeight += T2.controls[i].height();
                        //}catch(x){
                        //   alert(T2.controls[i].attr("xtype")); 
                        //}
                    }
                }
                if (dockC.length > 0) {

                    dockC[0].css({ width: w + "px", height: (h - countHeight) + "px" });
                }
            }
        };
        T2.box = box;
    };
    for (var i = 0; i < S.items.length; i++) {
        if (i > 0) lines[i] = $("<div class='line' />").appendTo(A);
        new item(S.items[i]);
    };
    var getAutoSize = function (maxSize) {
        var autoCount = 0;
        var width = 0;
        for (var i = 0; i < S.items.length; i++) {
            if (typeof (S.items[i].size) == "number") {
                if (S.items[i].visible == false) {
                    width = 0;
                } else {
                    width = S.items[i].size;
                }
            } else {
                autoCount++;
            }
        }
        return (maxSize - width - (S.items.length - 1) * lineSize) / autoCount;
    };
    var rFlag = false;
    T.resize = function () {
        //console.log((new Date()) + "框架改变");
        rFlag = true;
        var xFlag = S.type == null || S.type == "x";
        var bodyWidth = A.width(), bodyHeight = A.height();
        var maxSize = (xFlag) ? bodyWidth : bodyHeight;
        var leftXY = 0;
        var autoSize = getAutoSize(maxSize);

        for (var i = 0; i < S.items.length; i++) {
            if (i > 0) {
                if (xFlag) {
                    lines[i].css({ height: bodyHeight + "px", width: lineSize + "px" });
                }
                else {
                    lines[i].css({ width: bodyWidth + "px", height: lineSize + "px" });
                }
                leftXY += lineSize;
            }
            var size = typeof (S.items[i].size) == "number" ? S.items[i].size : autoSize;
            if (xFlag) {
                //if (T.items[i].controls) console.log("T.items[i].controls:" + T.items[i].controls.length + T.items[i].css);
                T.items[i].css({ height: bodyHeight + "px", width: size + "px" });
            } else {
                T.items[i].css({ width: bodyWidth + "px", height: size + "px" });
            }
            leftXY += size;
        }
        rFlag = false;
    };
    A.resize(function () {
        if (rFlag) return; T.resize();
    });
    T.resize();
    $M.BaseClass.apply(T, [S]);
    T.css = function (style) {
        A.css(style);
        T.resize();
    };
    if (S.style) T.css(S.style);
};
$M.Control["Tab"] = function (BoxID, S, CID) {
    var ID = "Button_" + $M.Index + "_";
    var T = this;
    T.items = [];
    T.controls = [];
    T.selectedItem = null;
    var A = null;
    if (CID) A = CID;
    else { A = $("<div class=\"tabbable\" ><ul class=\"nav nav-tabs\"></ul><div class=\"tab-content\"></div></div>").appendTo(BoxID); }
    var child = A.children();
    var B = $(child[0]);
    var C = $(child[1]);
    if (S.alignment == 1) A.addClass("tabs-left");
    var item = function (S2, nav_tabs, tab_content) {
        var T2 = this;
        T.items[T.items.length] = T2;
        T.controls[T.controls.length] = T2;
        var title = nav_tabs, content = tab_content;
        if (title == null) {
            var titlehtml = "<li><a href='#'>";
            if (S2.ico) titlehtml += "<i class=\"fa " + S2.ico + "\"></i>";
            titlehtml += "<span class=caption >" + S2.text + "</span>";
            if (S2.closeButton) titlehtml += "<i class=\"close fa fa-times-circle\"></i>";
            titlehtml += "</a>";
            titlehtml += "</li>"
            title = $(titlehtml).appendTo(B);
            content = $("<div class=\"tab-pane\"></div>").appendTo(C);
        }

        var closeButton = title.find(".close");
        var caption = title.find(".caption");
        //var content = $("<div class=\"tab-pane\"></div>").appendTo(C);
        if (S2.html) content.html(S2.html);
        if (S2["class"]) content.addClass(S2["class"]);

        T2.focus = function () {
            if (T.selectedItem) T.selectedItem.blur();
            title.addClass("active");
            content.addClass("active");
            T.selectedItem = T2;
            if (S.onSelectedIndexChanged) S.onSelectedIndexChanged(T, {});
            T2.resize();
        };
        T2.blur = function () {
            title.removeClass("active");
            content.removeClass("active");
        };
        T2.remove = function () {
            title.remove();
            content.remove();
            T.items = T.items.del(T.items.indexOf(T2));
            T.items[T.items.length - 1].focus();
            T.controls = T.controls.del(T2);
            T2 = null;
        };
        T2.resize = function () {
            //content.height(C.height());
            var w = content.width(), h = C.height();
            if (T2.controls) {
                var countHeight = 0;
                var dockC = [];
                for (var i = 0; i < T2.controls.length; i++) {
                    //console.warn(T2.controls[i]);
                    //console.warn(T2.controls[i].attr);
                    if (T2.controls[i].attr) {
                        if (T2.controls[i].attr("dock") == 2) {
                            dockC[dockC.length] = T2.controls[i];
                            //T2.controls[i].css({ width: w + "px", height: h + "px" });
                        } else {
                            if (T2.controls[i].height) countHeight += T2.controls[i].height();
                        }
                    }
                }
                //console.warn((h - countHeight) + "px");
                if (dockC.length > 0) {
                    console.warn(dockC[0].name);
                    //if (dockC[0].marginHeight) {
                    //    alert(1);
                    //}
                    var marginHeight = dockC[0].attr("marginHeight") ? dockC[0].attr("marginHeight") : 0;
                    dockC[0].css({ width: w + "px", height: (h - countHeight - marginHeight) + "px" });
                }
            }
            content.height(h);
        };
        T2.container = content;
        $M.BaseClass.apply(T2, [S2]);
        title.click(T2.focus);
        closeButton.click(function () {
            if (S2.onClose) {
                S2.onClose();
            }
            T2.remove();
        });
        this.attr = function (a, b) {
            if (b != null) S2[a] = b;
            if (a == "text") caption.html(b);
            return (S2[a]);
        };
		if(S2.style)content.css(S2.style);
    };
    T.addItem = function (S2) {
        if ($.isArray(S2)) {
            for (var i = 0; i < S2.length; i++) T.addItem(S2[i]);
        }else{
            return (new item(S2));
        }
    };
    var nav_tabs = B.children(), tab_content = C.children();
    for (var i = 0; i < nav_tabs.length; i++) {
        new item({}, $(nav_tabs[i]), $(tab_content[i]));
        T.items[0].focus();
    }
    if (S.items) {
        T.addItem(S.items);
        T.items[0].focus();
    }
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    T.css = function (style) {
        if (style) {
            A.css(style);
            if (style && style.height) {
                //console.log("tab");
                if (S.alignment == 1) {
                    C.outerHeight(A.height());
                } else {
                    C.outerHeight(A.height() - B.height());
                }
                T.selectedItem.resize();
            }
        }
    };
    if (S.dock != null && S.dock == 2) {
        T.css({ width: BoxID.width(), height: BoxID.height() });
    }
    if (S.style) T.css(S.style);
};
$M.Control["Panel"] = function (BoxID, S) {
    var T = this;
    var html = "<div class=\"panel\" tabindex=1>";
    var phead = null, pbody = null;
    if (S.text) {
        html += "<div class=\"panel-heading\"><h3 class=\"panel-title\">";
        if (S.ico) html += "<i class=\"fa " + S.ico + "\"></i> ";
        html += S.text;
        html += "<span class=\"panel-options\"></span>";
        html += "</h3></div>";
        html += "<div class=\"panel-body\">";
        html += "</div>";

    }
    html += "</div>";
    var A = $(html).appendTo(BoxID);
    if (S.text == null) pbody = A;
    else {
        phead = A.find(".panel-heading");
        pbody = A.find(".panel-body");
    }
    if (S.color != null) A.addClass("panel-" + $M.Control.Constant.colorCss[S.color]);
    T.container = pbody;
    $M.BaseClass.apply(T, [S]);

    if (S["class"]) T.addClass(S["class"]);
    $(document).on("keydown", function (e) {
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) {

        }
    });
    if (S.url) pbody.load(S.url);
};
$M.Control["PanelGroup"] = function (BoxID, S) {
    var T = this;
    var A = $("<div class=\"row grid ui-sortable\"></div>").appendTo(BoxID);
    T.items = [];
    var boxItem = [];
    var blankBox = null;
    function exchangePos(elem1, elem2) {
        var parent = elem1.parent();
        var next = elem1.next(), next2 = elem2.next();
        if (next.length == 0) {
            parent.append(elem2);
        } else {
            next.before(elem2);
        }
        if (next2.length == 0) {
            parent.append(elem1);
        } else {
            next2.before(elem1);
        }
    }
    var move = function (_obj, _box, x, y) {
        var obj = null, box = null;
        var x1 = x + _box.width(), y1 = y + _box.height();
        var _blank_xy = blankBox.position();
        for (var i = 0; i < boxItem.length; i++) {
            if (i == _obj.index) continue;
            var xy = boxItem[i].position();
            w = boxItem[i].width(); h = boxItem[i].height();
            var f = w * 0.5, f2 = h * 0.5;
            //alert([f,f2])
            var pix1 = _blank_xy.left < xy.left && x1 > (xy.left + f) && y > (xy.top) && x1 < (xy.left + w) && y < (xy.top + h);
            var pix2 = _blank_xy.left > xy.left && x > xy.left && y > xy.top && x < (xy.left + w - f) && y < (xy.top + h);
            var pix3 = _blank_xy.top < xy.top && y1 > (xy.top + f2) && x > xy.left && y1 < (xy.top + h) && x < (xy.left + w)
            var pix4 = _blank_xy.top > xy.top && y > xy.top && x > xy.left && y < (xy.top + h - f2) && x < (xy.left + w)
            //var pix2 = x1 > (xy.left + f) && y1 > (xy.top + f2) && x1 < (xy.left + w - f) && y1 < (xy.top + h - f2);
            //var pix3 = x > (xy.left + f) && y1 > (xy.top + f2) && x < (xy.left + w - f) && y1 < (xy.top + h - f2);
            //var pix4 = x1 > (xy.left + f) && y > (xy.top + f2) && x1 < (xy.left + w - f) && y < (xy.top + h - f2);
            //if (boxItem[i] != _box && (pix1 || pix2)) {
            if (boxItem[i] != _box && (pix1 || pix2 || pix3 || pix4)) {
                box = boxItem[i];
                obj = T.items[i];
                i = boxItem.length;
            }
        }
        if (obj) {
            exchangePos(blankBox, box);
            swap(_obj, obj);
        }
    };
    var swap = function (a, b) {
        var _index = a.index;
        var temp = T.items[a.index];
        T.items[a.index] = T.items[b.index];
        T.items[b.index] = temp;
        temp = boxItem[a.index];
        boxItem[a.index] = boxItem[b.index];
        boxItem[b.index] = temp;
        a.index = b.index;
        b.index = _index;
    };
    T.addItem = function (S2) {
        if (S2.size == null) S2.size = 4;
        var box = $("<div class=\"col-md-" + S2.size + "\"  ></div>").appendTo(A);
        S2.xtype = "Panel";
        var obj = box.addControl(S2);
        obj.index = boxItem.length;
        boxItem[obj.index] = box;
        T.items[obj.index] = obj;
        box.moveBox({
            onStart: function () { blankBox = $("<div class=\"col-md-2\"></div>").insertAfter(box); blankBox.css({ width: box.outerWidth() + "px", height: box.outerHeight() + "px" }); },
            onEnd: function () {
                blankBox.before(box);
                box.attr("style", "");
                blankBox.remove();
                if (S.onChange) S.onChange(T, null);
            },
            onMove: function (sendder, e) {
                move(obj, box, e.x, e.y);
            }
        });
        return obj;
    };
    if (S.items) {
        for (var i = 0; i < S.items.length; i++) {
            T.addItem(S.items[i]);
        }
    }

    T.container = A;
    $M.BaseClass.apply(T, [S]);
};
$M.Control["SlidingBar"]=function(BoxID,S)
	{
		var B=BoxID.addDom("div");B.className="M5_SlidingBar";
		var T=this;
		T.items=new Array();
		T.openItem=null;
		this.add=function(S2){
			var T2=this;
			T2.buttons=new Array();
			T.items[T.items.length]=this;
			var Bar=B.addDom("div");Bar.className="Bar";Bar.unselectable="on";
			if(S.ico!=null){var Ico=Bar.addDom("div");Ico.className="Ico "+S.ico;}
			var Title=Bar.addDom("span");//Title.className="Title";
			Title.unselectable="on";
			var ButtonBox=Bar.addDom("div");
			ButtonBox.setFloat("right");
			if(S2.buttons!=null){
			    for(var i=0;i<S2.buttons.length;i++)
			    {
			        S2.buttons[i].xtype="Button"; S2.buttons[i].className="LabelButton";
			        S2.buttons[i].disabled="true";
			        T2.buttons[T2.buttons.length]=ButtonBox.addControl(S2.buttons[i]);
			    }
			}
			if(S2.titleMenu!=null)
		    {
			    Bar.addEvent("onmouseup",function(){
			        if($M.event.button()==2)S2.titleMenu.show($M.event.x()-2,$M.event.y()-2);
			    });
		    }
			//var Button=ButtonBox.addControl({xtype:"Button",ico:"addClass",onclick:function(){},className:"LabelButton"});
			var Box=B.addDom("div");Box.className="Box";Box.style.display="none";
			var h=1;
			if(S2.caption!=null)Title.innerHTML=S2.caption;
			T2.setCaption=function(text)
			{
				Title.innerHTML=text;
			};
			T2.buttonsDisabled=function(tag){
			    for(var i=0;i<T2.buttons.length;i++){
			        T2.buttons[i].disabled(tag);
			    }
			};
			T2.remove=function(){
			    var n=-1;
			    for(var i=0;i<T.items.length;i++){
			        if(T.items[i]==T2)n=i;
			    }
			    T.items=T.items.del(n);
			    Bar.remove();
			    Box.remove();
			    T.items[0].open();
			};
			T2.open=function()
			{
				if(T.openItem!=null){T.openItem.buttonsDisabled(true);T.openItem.close();}
				Box.style.display="";
				Box.style.height=B.offsetHeight-Bar.offsetHeight*T.items.length+"px";
				T.openItem=T2;
				T2.buttonsDisabled(false);
				if(S.onopen)S.onopen(T2);
			};
			T2.resize=function()
			{
			    var height=Bar.offsetHeight*T.items.length;
			    if(height>B.offsetHeight)height=0;
			    else{height=B.offsetHeight-height;}
				Box.style.height=height+"px";
			};
			T2.close=function()
			{
				Box.style.display="none";
				T.openItem=null;
			};
			T2.addControl=function(Set){return(Box.addControl(Set));};
			Bar.addEvent("onclick",T2.open);
			return(this);
		};
		this.resize=function(){
		    if(T.openItem!=null)T.openItem.resize();
		};
		this.setAttribute=function(a,b){S[a]=b;};
		this.setSize=function(w,h)
		{
			B.style.height=h+"px";
			if(T.openItem!=null)T.openItem.resize();
		};
		if(S.items!=null){
		for(var n=0;n<S.items.length;n++){
			var s=new this.add(S.items[n]);
		}
		}
};
$M.Control["Window"] = function (BoxID, S) {
    var objID = "Window_" + $.Index + "_";
    var T = this;
    var html = "<div class=\"modal-content M5_Window\" tabindex=\"-1\" style=\"display:none;z-index:" + ($M.zIndex + 1) + "\">";
    html += "<div class=\"modal-header\">";
    html += "<button type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-hidden=\"true\">×</button>";
    html += "<h4 class=\"modal-title\" >";
    if (S.ico) html += "<i class=\"fa " + S.ico + "\"></i> ";
    html += S.text;
    html += "</h4>";
    html += "</div>";
    html += "</div>";
    T.footer = [];
    var modal_body = null, modal_footer = null, closeButton = null;
    var A = $(html).appendTo(BoxID);
    if (S.style) A.css(S.style);
    var header = A.find(".modal-header");
    if (S.text == null) header.hide();
    closeButton = A.find(".modal-header .close");
    T.form = A.addControl({
        url: S.url,
        command: S.command,
        xtype: "Form",
        onBeginSubmit: S.onBeginSubmit,
        onSubmit: S.onSubmit,
        onSubmitErr: S.onSubmitErr
    });
    T.dialogResult = $M.dialogResult.cancel;
    var modal_body = T.form.append("<div class=\"modal-body\"></div>");

    if (S["class"]) modal_body.addClass(S["class"]);
    //var modal_body = $("<div class=\"modal-body\"></div>").appendTo(form);
    if (S.footer) {
        modal_footer = T.form.append("<div class=\"modal-footer\"></div>");
        //modal_footer = $("<div class=\"modal-footer\"></div>").appendTo(form);
        for (var i = 0; i < S.footer.length; i++) {
            T.footer[T.footer.length] = modal_footer.addControl(S.footer[i]);
        }
    }
    T.remove = function () {
        var r = true;
        if (S.onClose) r = S.onClose(T,null);
        if (r == null || r) {
            if (S.isModal) {
                $M.lock(A, false);
            }
            A.remove();
            if (A[0] == $M.focusElement || A.has($M.focusElement).length) $M.focusElement = null;
        }
    };
    if (closeButton) {
        closeButton.click(function () { T.remove(); });
    }
    var keydown = function (e) {
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) {
            if (e.which == 27 && S.isModal && S.text != null) {
                T.remove();
            }
            if (S.onKeyDown) S.onKeyDown(T, e);
        }
    };

    $(document).on("keydown", keydown);
    T.container = modal_body;
    $M.BaseClass.apply(T, [S]);
    T.css = function (style) {
        if(style){
            A.css(style);
        if (style.height) {
            var headHeight = header ? header.outerHeight():0;
            var footHeight = modal_footer ? modal_footer.outerHeight() : 0;
            modal_body.css({ height: style.height - headHeight - footHeight });
        }
        }
    };
    T.show = function () {
        if (S.isModal) $M.lock(A, true, T, true);
        A.show();
        $M.focusElement = A[0];
        if (S.style == null || (S.style != null && S.style.left == null && S.style.top == null)) {
            var height = $(document.body).height();
            if (height == 0) height = $(window).height();
            var body = document.documentElement ? document.documentElement : document.body;
            var scH = body.clientHeight;
            T.css({ left: ($(document.body).width() - A.width()) / 2 + "px", top: (scH - A.height()) / 2 + $(document.body).scrollTop() + "px" });
            //A.css({ left: ($(document.body).width() - A.width()) / 2 + "px", top: ( height- A.height()) / 2 + "px" });
        }
        T.css(S.style);
    };
    T.append = function (str) {
        return $(str).appendTo(modal_body);
    };
    T.addControl = function (S2) {
        var list = modal_body.addControl(S2);
        if (T.form != null) {
            if (T.form.controls == null) T.form.controls = [];
            if ($.isArray(list)) {
                for (var i = 0; i < list.length; i++) {
                    T.form.controls[T.form.controls.length] = list[i];
                }
            } else {
                T.form.controls[T.form.controls.length] = list;
            }
        }
        return list;
    };
    T.resize = function () {
        //content.height(C.height());
        var w = modal_body.width(), h = modal_body.height();
        if (T.form.controls) {
            var countHeight = 0;
            var dockC = [];
            for (var i = 0; i < T.form.controls.length; i++) {
                //console.warn(T2.controls[i]);
                //console.warn(T2.controls[i].attr);
                if (T.form.controls[i].attr) {
                    if (T.form.controls[i].attr("dock") == 2) {
                        dockC[dockC.length] = T.form.controls[i];
                    } else {
                        if (T.form.controls[i].height) countHeight += T.form.controls[i].height();
                    }
                }
            }
            
            if (dockC.length > 0) {
                console.warn(dockC[0].name);
                var marginHeight = dockC[0].attr("marginHeight") ? dockC[0].attr("marginHeight") : 0;
                dockC[0].css({ width: w + "px", height: (h - countHeight - marginHeight) + "px" });
            }
        }
    };
};
$M.Control["Windows"] = function () {
    var T = this;
    var zIndex = 1000;
    T.items = new Array();
    var addItem = function () {

    };
};