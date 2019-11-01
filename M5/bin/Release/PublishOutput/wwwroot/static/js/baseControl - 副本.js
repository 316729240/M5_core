var userAgent = navigator.userAgent.toLowerCase();
// Figure out what browser is being used 
jQuery.browser = {
    version: (userAgent.match(/.+(?:rv|it|ra|ie)[\/: ]([\d.]+)/) || [])[1],
    safari: /webkit/.test(userAgent),
    opera: /opera/.test(userAgent),
    msie: /msie/.test(userAgent) && !/opera/.test(userAgent),
    mozilla: /mozilla/.test(userAgent) && !/(compatible|webkit)/.test(userAgent)
};
var $M = {};
$M.Index = 0;
$M.zIndex = 1000;
$M.focusElement = null;
$M.Control = {
    Constant: {
        algin: { "left": "left", "center": "center", "right": "right" },
        colorCss: ["default", "primary", "success", "info", "warning", "danger", "link"],
        color: {
            "default": 0,
            "primary": 1,//主要
            "success": 2,//正确
            "info": 3,//信息
            "warning": 4,//错误
            "danger": 5,//危险
            "link": 6
        },
        dockStyle: { "none": 0, "bottom": 1, "fill": 2, "left": 3, "right": 4, "top": 5 },
        sizeCss: ["xs", "sm", "", "lg"],
        size: { "xs": 0, "sm": 1, "default": 2, "lg": 3 }
    }
};
$M.getId = function () {
    var d = new Date();
    var id = Math.ceil(Math.random() * 999);
    return ("" + d.getFullYear() + d.getMonth() + d.getDay() + d.getHours() + d.getMinutes() + d.getSeconds() + id)
};
$M.getIco = function (ico) {
    if (ico.indexOf("fa-") > -1) return "fa " + ico;
};
$M.ajax = function (url, data, back, errback) {
    try {
        var formdata = new FormData();
        var flag = false;
        if (data) {
            for (var o in data) {
                if (data[o] != null && typeof (data[o]) == "object") {
                    flag = true;
                    formdata.append(o, data[o]);
                } else {
                    formdata.append(o, data[o]);
                    //formdata.append(o, encodeURIComponent(data[o]));
                }
            }
            if (flag) data = formdata;
        }
    } catch (x) { }
    var type = data ? "POST" : "GET";
    var json = {
        url: url,
        type: type,
        success: back,
        data: data,
        dataType: "json",
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            if (errback) {
                var messag = { errNo: -1000, errMsg: errorThrown, status: XMLHttpRequest.status };
                errback(messag);
            } else {
                alert("发生错误");
            }
        }
    };
    if (flag) {
        json.contentType = false;
        json.processData = false;
    }
    $.ajax(json);
};
$M.loadJs = function (files, f) {
    var count = 0, max = files.length;
    var back = function () { count++; if (max == count) f(); };
    for (var i = 0; i < max; i++) {
        $.getScript(files[i], back);
    }
};
$M.daysBetween = function (DateOne, DateTwo) {
    var OneMonth = DateOne.substring(5, DateOne.lastIndexOf('-'));
    var OneDay = DateOne.substring(DateOne.length, DateOne.lastIndexOf('-') + 1);
    var OneYear = DateOne.substring(0, DateOne.indexOf('-'));

    var TwoMonth = DateTwo.substring(5, DateTwo.lastIndexOf('-'));
    var TwoDay = DateTwo.substring(DateTwo.length, DateTwo.lastIndexOf('-') + 1);
    var TwoYear = DateTwo.substring(0, DateTwo.indexOf('-'));
    var cha = ((Date.parse(OneMonth + '/' + OneDay + '/' + OneYear) - Date.parse(TwoMonth + '/' + TwoDay + '/' + TwoYear)) / 86400000);
    return (cha);
};
$M.Method = {
    Array: function () {
        this.indexOf = function () {
            var C = this.length;
            for (var i = 0; i < C; i++) {
                if (this[i] == arguments[0]) return i;
            };
            return -1;
        };
        this.del = function (n) {
            if (typeof (n) != "number") {
                var index = this.indexOf(n);
                if (index > -1) return this.slice(0, index).concat(this.slice(index + 1, this.length));
                return this;
            } else {
                if (n < 0)
                    return this;
                else
                    return this.slice(0, n).concat(this.slice(n + 1, this.length));
            }
        };
    },
    String: function () {
        this.toDate = function () {
            if (this.indexOf("\/Date(") > -1) {
                var temp = this.replace("\/Date(", "");
                var ticks = parseInt(temp.replace(")\/", ""));
                var d = new Date(ticks);
                return d;
            }
            var arr = this.replace(/\d+(?=-[^-]+$)/,
       function (a) { return a; }).match(/\d+/g);
            if (arr == null || arr.length < 3) return null;
            var date = new Date(arr[0] + "/" + parseInt(arr[1]) + "/" + parseInt(arr[2]));
            return date;
        }
    },
    Date: function () {
        this.addDays = function (c) {
            this.setDate(this.getDate() + c);
            return this;
        };
        this.format = function (format) {
            /* 
            * eg:format="yyyy-MM-dd hh:mm:ss"; 
            */
            var o = {
                "M+": this.getMonth() + 1, // month  
                "d+": this.getDate(), // day  
                "h+": this.getHours(), // hour
                "H+": this.getHours(), // hour  
                "m+": this.getMinutes(), // minute  
                "s+": this.getSeconds(), // second  
                "q+": Math.floor((this.getMonth() + 3) / 3), // quarter  
                "S": this.getMilliseconds()
                // millisecond  
            }

            if (/(y+)/.test(format)) {
                format = format.replace(RegExp.$1, (this.getFullYear() + "").substr(4
                            - RegExp.$1.length));
            }

            for (var k in o) {
                if (new RegExp("(" + k + ")").test(format)) {
                    format = format.replace(RegExp.$1, RegExp.$1.length == 1
                                ? o[k]
                                : ("00" + o[k]).substr(("" + o[k]).length));
                }
            }
            return format;
        }
    }
};
$M.BaseClass = function (S) {
    var T = this;
    this.wait = function (flag) {
        if (flag) {
            T.waitBox = $("<div class='modal-backdrop fade in' style='position: absolute; top:0px;left:0px;background-color:#FFFFFF'></div>").appendTo(T.container);
            var img = $("<img src='../static/img/wait.gif' style='position: absolute;width:50px' />").appendTo(T.waitBox);
            var width = T.container.outerWidth(), height = T.container.outerHeight();
            img.css({ left: width / 2 - 25 + "px", top: height / 2 - 25 + "px" });
            T.waitBox.css({ width: width + "px", height: height + "px" });
        } else {
            if (T.waitBox) T.waitBox.remove();
            T.waitBox = null;
        }
    };
    this.comm = function (a, b, c, d) {
        T.wait(true);
        if (typeof (arguments[0]) == "string") {
            $M.comm(a, b, function (e) { T.wait(false); c(e); }, function (e) { T.wait(false); d(e); });
        } else {
            $M.comm(a, function (e) { T.wait(false); b(e); }, function (e) { T.wait(false); c(e); });
        }
    };
    this.attr = function (a, b) {
        if (b != null) S[a] = b;
        return (S[a]);
    };
    this.dispose = function () {
        if (T.controls) {
            var count = T.controls.length;
            for (var i = count - 1; i > -1; i--) {
                T.controls[i].dispose();
            }
        }
        T.container.remove();
        T.container = null;
    };
    this.addControl = function (S2) {
        var T = this;
        var listC = T.container.addControl(S2);
        if (T.controls == null) T.controls = [];
        if ($.isArray(listC)) {
            for (var i = 0; i < listC.length; i++) {
                if (listC[i]) T.controls[T.controls.length] = listC[i];
            }
        } else {
            if (listC) T.controls[T.controls.length] = listC;
        }
        return listC;
        var addItem = function (S3) {
            line = $("<div class=\"form-group\"></div>").appendTo(T.container);
            S3.width = (S3.width == null ? "12" : S3.width);
            var item = $("<label>" + S3.labelText + ((S3.vtype && S3.vtype.required) ? "<span class=\"f_req\">*</span>" : "") + "</label>").appendTo(line);
            var obj = null;
            if (S3.labelWidth) {
                item.addClass("col-sm-" + S3.labelWidth + " control-label");
                var line2 = $("<div class=\"col-sm-" + (12 - S3.labelWidth) + "\"></div>").appendTo(line);
                if (S3.xtype) {
                    obj = line2.addControl(S3);
                } else {
                    for (var i = 0; i < S3.items.length; i++) {
                        T.controls[T.controls.length] = line2.addControl(S3.items[i]);
                    }
                }
            } else {
                obj = line.addControl(S3);
            }
            if (T.controls == null) T.controls = [];
            T.controls[T.controls.length] = obj;
            return obj;
        };

        var obj = null;
        if (S2 == null) return;
        if (this.container) {
            if (S2 && S2.length) {
                var clist = [];
                for (var i = 0; i < S2.length; i++) {
                    var line = null;
                    if (S2[i].length) {
                        line = $("<div class=\"formSep\"></div>").appendTo(this.container);
                        line = $("<div class=\"row\"></div>").appendTo(line);
                        for (var i1 = 0; i1 < S2[i].length; i1++) {
                            var S3 = S2[i][i1];
                            S3.width = (S3.width == null ? "12" : S3.width);
                            var item = $("<div class=\"col-md-" + S3.width + "\"><label>" + S3.labelText + ((S3.vtype && S3.vtype.required) ? "<span class=\"f_req\">*</span>" : "") + "</label></div>").appendTo(line);
                            var obj = item.addControl(S3);

                            if (this.controls == null) this.controls = [];
                            this.controls[this.controls.length] = obj;
                            clist[clist.length] = obj;
                        }
                    } else {
                        clist[clist.length] = addItem(S2[i]);
                    }
                }
                return clist;
            } else {
                if (S2.labelText) {
                    obj = addItem(S2);
                    //                        line = $("<div class=\"form-group\"></div>").appendTo(this.container);
                    //                        var item = $("<label>" + S2.labelText + ((S2.vtype && S2.vtype.required) ? "<span class=\"f_req\">*</span>" : "") + "</label>").appendTo(line);
                    //                        if (S2.labelWidth) {
                    //                            item.addClass("col-sm-" + S2.labelWidth + " control-label");
                    //                            var line2 = $("<div class=\"col-sm-" + (12 - S2.labelWidth) + "\"></div>").appendTo(line);
                    //                            var obj = line2.addControl(S2);
                    //                        } else {
                    //                            obj = line.addControl(S2);
                    //                        }
                } else {
                    obj = this.container.addControl(S2);
                }
                //obj.parent = this;
                if (this.controls == null) this.controls = [];
                this.controls[this.controls.length] = obj;
                return obj;
            }
        }
    };
    this.height = function (h) { return (h == null ? this.container.outerHeight() : this.container.outerHeight(h)); };
    this.css = function (style) { return this.container.css(style); };
    this.addClass = function (css) { this.container.addClass(css); };
    this.removeClass = function (css) { this.container.removeClass(css); };
    this.append = function (str) {
        if (this.container) {
            return $(str).appendTo(this.container);
        };
        return null;
    };
    this.load = function (str, f) {
        this.container.load(str, f);
        return null;
    };
    this.$ = function (str) {
        return this.container.find(str);
    };
    this.find = function (name) {
        if (this.controls) {
            for (var i = 0; i < this.controls.length; i++) {
                if (this.controls[i].attr("name") == name) return this.controls[i];
                else {
                    if (this.controls[i].find) {
                        var c = this.controls[i].find(name);
                        if (c) return c;
                    }
                }
            }
        }
    };
    this.html = function (html) {
        this.container.html(html);
    };
    if (this.container != null) {
        this.container.mouseup(function (e) {
            if (S.onMouseUp) return S.onMouseUp(T, e);
            //return false;
        });
        this.container.dblclick(function (e) {
            if (S.onMouseDoubleClick) return S.onMouseDoubleClick(T, e);
            //return false;
        });
        this.container.mousedown(function (e) {
            if (S.onMouseDown) return S.onMouseDown(T, e);

        });
    }
    this.render = function () {
        T.controls = [];
        var item = this.container.find("input,select,div,button");
        for (var i = 0; i < item.length; i++) {
            var jqobj = $(item[i]);
            xtype = jqobj.attr("xtype");
            if (xtype) {
                attrs = item[i].attributes;
                S = []; C = attrs.length;
                for (var i1 = 0; i1 < C; i1++) S[attrs[i1].name] = attrs[i1].value;
                var obj = new $M.Control[xtype](null, S, jqobj);
                this.controls[this.controls.length] = obj;
            }
        }
    };
};
$(document).on("focusin.M4.public", function (e) { $M.focusElement = e.target; });
$M.Method.Array.apply(Array.prototype);
$M.Method.Date.apply(Date.prototype);
$M.Method.String.apply(String.prototype);
$.fn.extend({
render: function() {
    var xtype = this.attr("xtype");
    var attrs = this.get(0).attributes;
    var S = [];
    var C = attrs.length;
    for (var i = 0; i < C; i++) {
        S[attrs[i].name] = attrs[i].value;
    }
    var F = {};
    if (xtype) F = new $M.Control[xtype](null, S, this);
    var item = this.find("input,select,form,div");
    F.controls = [];
    for (var i = 0; i < item.length; i++) {
        var jqobj = $(item[i]);
        xtype = jqobj.attr("xtype");
        if (xtype) {
            attrs = item[i].attributes;
            S = []; C = attrs.length;
            for (var i1 = 0; i1 < C; i1++) S[attrs[i1].name] = attrs[i1].value;
            var obj = new $M.Control[xtype](null, S, jqobj);
            F.controls[F.controls.length] = obj;
        }
    }
    return F;
    },
    addControl: function (S2) {
        var box = this;
        var clist = new Array();
        //添加单个控件
        var add = function (dom, S) {
            if (!S) return;
            $M.Index++;
            if (!S.xtype) S.xtype = "TextBox";
            if ($M.Control[S.xtype] == null) {
                alert("找不到" + S.xtype + "扩展");
                return null;
            }
            var obj = new $M.Control[S.xtype](dom, S);
            try {
                clist[clist.length] = obj;
                return (obj);
            } finally {
                obj = null;
            }
        };
        var addItem = function (S3) {
            line = $("<div class=\"form-group\"></div>").appendTo(box);
            if (S3.width != null) line.addClass("col-sm-" + S3.width);
            S3.width = (S3.width == null ? "12" : S3.width);
            var item = $("<label>" + S3.labelText + ((S3.vtype && S3.vtype.required) ? "<span class=\"f_req\">*</span>" : "") + "</label>").appendTo(line);
            var obj = null;
            if (S3.labelWidth) {
                item.addClass("col-sm-" + S3.labelWidth + " control-label");
                var line2 = $("<div class=\"col-sm-" + (12 - S3.labelWidth) + "\"></div>").appendTo(line);
                if (S3.xtype) {
                    obj = add(line2, S3);
                } else {
                    obj = [];
                    for (var i = 0; i < S3.items.length; i++) {
                        obj[obj.length] = add(line2, S3.items[i]);
                    }
                }
            } else {
                obj = add(line, S3);
            }
            //if (T.controls == null) T.controls = [];
            //T.controls[T.controls.length] = obj;
            return obj;
        };
        var obj = null;
        if (S2 == null) return;
        if (S2 && S2.length) {
            var clist = [];
            for (var i = 0; i < S2.length; i++) {
                var line = null;
                if (S2[i].length) {
                    //line = $("<div class=\"formSep\"></div>").appendTo(box);
                    line = $("<div class=\"row\"></div>").appendTo(box);
                    for (var i1 = 0; i1 < S2[i].length; i1++) {
                        var S3 = S2[i][i1];
                        S3.width = (S3.width == null ? "12" : S3.width);
                        var item = null;
                        if (S3.labelText) {
                            item = $("<div class=\"col-md-" + S3.width + "\"><label>" + S3.labelText + ((S3.vtype && S3.vtype.required) ? "<span class=\"f_req\">*</span>" : "") + "</label></div>").appendTo(line);
                        } else {
                            item = $("<div class=\"col-md-" + S3.width + "\"></div>").appendTo(line);
                        }
                        var obj = add(item, S3);

                        //if (this.controls == null) this.controls = [];
                        //this.controls[this.controls.length] = obj;
                        //clist[clist.length] = obj;
                    }
                } else {
                    addItem(S2[i]);
                }
            }
            return clist;
        } else {
            if (S2.labelText) {
                obj = addItem(S2);
            } else {
                obj = add(this, S2);
            }
            //if (this.controls == null) this.controls = [];
            //this.controls[this.controls.length] = obj;
            return obj;
        }
        return null;
    },
    resizable: function (S) {
        var T = this;
        var resizeBox = [];
        var obj = $(this);
        var moveflag = false, isShow = false;
        var setPix = function () {
            var xy = obj.offset();
            resizeBox[0].css({ top: xy.top + obj.height() - 5, left: xy.left + obj.width() - 5 });
        };
        var resize = function (e) {
            var xy = obj.offset();
            obj.css({ width: e.pageX - xy.left, height: e.pageY - xy.top });
            setPix();
        };
        var mousedown = function () {
            moveflag = true;
            document.body.onselectstart = function () { return false; }
            $(document.body).on("mousemove", resize);
        };
        var mouseup = function () {
            moveflag = false;
            $(document.body).unbind("mousemove", resize);
            document.body.onselectstart = function () { return true; }
        };
        for (var i = 0; i < 1; i++) {
            resizeBox[i] = $("<span val=" + i + " class='m5_pix_drop' style='cursor:se-resize;position: absolute;float:left;background-color: #FF00FF;width:10px;height:10px;overflow:hidden;z-index:100000' ></span>");//.appendTo($(document.body));
        }
        $(document.body).on("mouseup", mouseup);
        $(document.body).on("mousemove", function (e) {
            var xy = obj.offset();
            if (xy.top < e.pageY && xy.left < e.pageX && xy.left + obj.width() + 15 > e.pageX && xy.left + obj.width() + 15 > e.pageX) {
                if (!isShow) {
                    for (var i = 0; i < 1; i++) {
                        $(document.body).append(resizeBox[i]);
                        resizeBox[i].on("mousedown", mousedown);
                    }
                    setPix();
                    isShow = true;
                }
            } else {
                if (isShow && !moveflag) {
                    resizeBox[0].remove();
                    isShow = false;
                }
            }
        });
        //obj.on("mousemove", function (e) {
        //    $(document.body).append(resizeBox[0]);
        //    setPix();
        //});
        //obj.on("mouseover", function (e) {
        //    resizeBox[0].remove();
        //});
    },
    moveBox: function (S) {
        //alert($(this).length);
        var T = this;
        var moveObj = null;
        var markObj = null;
        var moveflag = false;
        var $this = null;
        var init = function (e) {
            var _start = { x: e.pageX, y: e.pageY };
            var o = { iTop: 0, iLeft: 0, width: $(this).outerWidth(), height: $(this).outerHeight() };
            var start = function () {
                if (S.onStart) S.onStart(T, {});
                var _position = null;
                if (S.html == null) {
                    _position = moveObj.position();
                    //_position = moveObj.offset();
                    o.iTop = _position.top - e.pageY;
                    o.iLeft = _position.left - e.pageX;
                    //alert([o.iTop, o.iLeft]);
                    moveObj.css({ "z-index": 1000, position: "absolute", top: _position.top, left: _position.left, width: o.width + "px", height: o.height + "px" });

                } else {
                    moveObj = $(S.html).appendTo(document.body);
                    _position = moveObj.position();
                    //_position = moveObj.offset();
                    if (S.offsetY != null) o.iTop = S.offsetY;
                    if (S.offsetX != null) o.iLeft = S.offsetX;
                    moveObj.css({ "z-index": 1000, position: "absolute", top: e.pageY, left: e.pageX });
                    o.width = moveObj.width();
                    o.height = moveObj.height();
                }
                $this = moveObj;
                markObj = $("<div />").appendTo(moveObj.parent());
                markObj.css({ "z-index": 2000, position: "absolute", width: o.width + "px", height: o.height + "px" });
            };
            $(document).bind("mousemove", function (e) {
                if (!moveflag) {
                    var c_w = Math.abs(e.pageX - _start.x), c_h = Math.abs(e.pageY - _start.y);
                    if (c_w < 10 && c_h < 10) return;
                    start();
                    moveflag = true;
                }
                var iLeft = e.pageX + o.iLeft;
                var iTop = e.pageY + o.iTop;
                $this.css({
                    'left': iLeft + "px",
                    'top': iTop + "px"
                });
                markObj.css({
                    'left': iLeft + "px",
                    'top': iTop + "px"
                });
                if (S.onMove) S.onMove(T, { x: iLeft, y: iTop });
            });
            $(document).bind("mouseup", function () {
                setTimeout(function () {
                    if (moveflag) {
                        moveflag = false;
                        if (S.html) moveObj.remove();
                        if (S.onEnd) S.onEnd(moveObj, {});
                    }
                    $(document).unbind("mousemove");
                    $(document).unbind("mouseup");
                    if (markObj) markObj.remove();
                }, 100);
            });
            return false;
        };
        if (S.html) {
            init(S);
        } else {
            moveObj = $(this);
            $(this).mousedown(init);
        }
    }
});
$M.Control["inputBox"] = function (BoxID, S) {
    var BZ = false;
    var T = this;
    var tipColor = "#C0C0C0";
    var defaultColor = "";
    var A = BoxID.addDom("div");
    A.className = "M4_inputBox " + S.cssClass;
    if (S.width != null) A.style.width = S.width;
    var B = A.addDom("input");
    B.className = "TextBox";
    B.style.width = (A.offsetWidth - 21) + "px";
    B.name = S.name;
    if (S.value != null) B.value = S.value;
    var C = A.addDom("div");
    C.className = "Button";
    C.unselectable = "on";
    this.text = "";
    this.value = "";
    var tempValue = "";
    this.clearBoth = function () {
        A.style.clear = "both"
    };
    var D = null;
    B.addEvent("onfocus", function () {
        B.style.color = defaultColor;
        if (B.value == S.tip) {
            B.value = ""
        };
        B.className = "TextBox_focus";
        C.className = "Button_focus";
        BZ = true;
        if (S.onfocus != null) S.onfocus();
        return false;
    });
    B.addEvent("onblur", function () {
        if (S.tip != null && B.value == "") {
            B.value = S.tip;
            B.style.color = tipColor
        };
        B.className = "TextBox";
        C.className = "Button";
        BZ = false;
        if (S.onblur != null) S.onblur();
        return false;
    });
    B.addEvent("onkeyup", function () {
        if (tempValue != B.value) {
            tempValue = B.value;
            if (S.oninput != null) S.oninput(T)
        };
        if ($.event.keyCode() == 13 && S.onconfirm != null) S.onconfirm()
    });
    C.addEvent("onmousedown", function () {
        C.className = "Button_Down";
        if (S.onconfirm != null) S.onconfirm()
    });
    C.addEvent("onmouseup", function () {
        C.className = "Button_Up";
        B.className = "TextBox_focus";
        B.focus();
        BZ = true
    });
    this.remove = function () {
        B.remove();
        C.remove();
        A.remove();
        A = null;
        B = null;
        C = null
    };
    this.focus = function () {
        B.focus()
    };
    this.setValue = function (v) {
        B.style.color = defaultColor;
        B.value = v
    };
    this.val = function () {
        if (B.value == S.tip) return ("");
        else {
            return (B.value)
        }
    };
    this.setAttribute = function (a, b) {
        S[a] = b
    };
    this.setSize = function (w, h) {
        A.setSize(w, h);
        B.setSize(A.offsetWidth - 21, h)
    };
    C.className = "Button_Up";
    B.className = "TextBox_focus";
    B.focus()
};
$M.Control["DialogInput"] = function (BoxID, S) {
    var T = this;
    if (S.ico == null) S.ico = "fa-ellipsis-h";
    var html = '<div class="input-group">' +
                '<input class="inputbox form-control" ' + (S.inputReadOnly ? 'readonly' : '') + ' type="text">' +
				'<div class="input-group-btn"><button type="button" class="btn btn-default"><i class="fa ' + S.ico + '"></i></button></div>' +
			'</div>';
    var A = $(html).appendTo(BoxID);
    A.addClass("input-group-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size]);
    var button = A.find("button");
    var inputbox = A.find(".inputbox");
    if (S.onButtonClick) button.click(function () { S.onButtonClick(T, null); });
    T.val = function (value) {
        if (value != null) {
            inputbox.val(value);
            return (value);
        } else {
            return inputbox.val();
        }
    };

    A.keydown(function (e) {
        if (S.onKeyDown) return S.onKeyDown(T, e);
    });
    A.keyup(function (e) {
        if (e.keyCode == 13) {
            if (S.onButtonClick) S.onButtonClick(T, null);
        }
        if (S.onKeyUp) return S.onKeyUp(T, e);
    });
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    if (S.style) T.css(S.style);
};
$M.Control["UploadFileBox"] = function (BoxID, S, CID) {
    var T = this;
    if (S.ico == null) S.ico = "fa-ellipsis-h";
    var A = null;
    if (CID) {
        A = $("<div class=\"input-group\"></div>");
        A.insertBefore(CID);
        CID.attr("disabled", "true");
        CID.attr("class", "inputbox form-control");
        CID.disable = false;
        A.append(CID);
        A.append('<div class="input-group-btn"><button type="button" class="btn btn-default">选择文件</button></div>');
    }else{
        var html = '<div class="input-group">' +
                    '<input class="inputbox form-control" ' + (S.inputReadOnly ? 'readonly' : '') + ' type="text" name="' + S.name + '">' +
				    '<div class="input-group-btn"><button type="button" class="btn btn-default">选择文件</button></div>' +
			    '</div>';
        A=$(html).appendTo(BoxID);
    }
    A.addClass("input-group-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size]);
    var button = A.find("button");
    var inputbox = A.find(".inputbox");
    //var img = $("<input type=file name='" + S.name + "' style='display:none'/>").appendTo(A);
    button.click(function () {
        $M.app.call("$M.system.insertPic", {
            isMultiple: false,
            back: function (json) {
                T.val(json[0]);
                if (S.onChange) S.onChange(T,null);
            }
        });
        //        img.click();
    });
    T.val = function (value) {
        if (value != null) {
            inputbox.val(value);
        }
        return (inputbox.val());
    };

    A.keydown(function (e) {
        if (S.onKeyDown) return S.onKeyDown(T, e);
    });
    A.keyup(function (e) {
        if (S.onKeyUp) return S.onKeyUp(T, e);
    });
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    if (S.style) T.css(S.style);
};
$M.Control["PathBar"] = function (BoxID, S) {
    var T = this;
    T.items = new Array();
    var Box = BoxID.addDom("div");
    if (S.ico != null) {
        Box.className = "M4_PathBar " + S.ico
    } else {
        Box.className = "M4_PathBar"
    }
    var pathBox = Box.addDom("div");
    var textBox = Box.addDom("input");
    textBox.style.display = "none";
    Box.addEvent("onmousedown", function () {
        if ($.event.target().tagName != "DIV" || pathBox.style.display == "none") return;
        pathBox.style.display = "none";
        textBox.style.display = "";
        textBox.value = T.val();
        Box.className += " PathEdit";
        setTimeout(setf, 100);
    });
    var setf = function () {
        textBox.focused = true;
        textBox.select();
    };
    Box.addEvent("onkeydown", function () {
        if ($.event.keyCode() == 13) {
            if (S.ongopath != null) S.ongopath(textBox.value)
        }
    });
    textBox.addEvent("onblur", function () {
        pathBox.style.display = "";
        textBox.style.display = "none";
        Box.className = Box.className.replace(" PathEdit", "")
    });
    T.setSize = function (w, h) {
        Box.setSize(w, h);
        textBox.setSize(w - 10, null)
    };
    T.add = function (value) {
        T.clear();
        T.items[T.items.length] = value;
        T.reload()
    };
    T.clear = function () {
        var count = pathBox.childNodes.length;
        for (var i = 0; i < count; i++) pathBox.childNodes[0].remove()
    };
    T.val = function () {
        return (T.items[T.items.length - 1].value)
    };
    T.go = function (i) {
        T.items.length = i + 1;
        T.reload();
        if (S.onchange != null) S.onchange(T.items[i])
    };
    T.reload = function () {
        T.clear();
        for (var i = 0; i < T.items.length; i++) {
            if (i > 0) pathBox.addDom("span").innerHTML = "->";
            var text = pathBox.addDom("a");
            text.href = "#";
            eval('text.addEvent("onclick",function(){T.go(' + i + ');})');
            text.innerHTML = T.items[i].text
        }
    }
};

$M.Control["Panel"] = function (BoxID, S) {
    var T = this;
    var A = BoxID.addDom("div");
    A.className = "Panel";
    var Title_Div = A.addDom("div");
    Title_Div.className = "Title_L";
    Title_Div.addEvent("onmousedown", function () {
        if (S.onmovestart != null) S.onmovestart($.event.x(), $.event.y())
    });
    Title_Div.addEvent("onmousemove", function () {
        if (S.onmove != null) S.onmove($.event.x(), $.event.y())
    });
    Title_Div.addEvent("onmouseup", function () {
        if (S.onmoveend != null) S.onmoveend($.event.x(), $.event.y())
    });
    this.getXY = function () {
        return (A.getXY())
    };
    this.setCapture = function () {
        Title_Div.setCapture()
    };
    this.releaseCapture = function () {
        Title_Div.releaseCapture()
    };
    this.setXY = function (x, y) {
        if (x != null) A.style.left = x + "px";
        if (y != null) A.style.top = y + "px"
    };
    this.insertDom = function (o) {
        return (A.insertDom(o))
    };
    this.getSize = function () {
        return ({
            width: A.offsetWidth,
            height: A.offsetHeight
        })
    };
    this.replaceObj = function (obj) {
        if (obj != null) {
            var M = obj.parentNode.insertBefore(A, obj);
            T.setPosition("");
            obj.remove()
        }
    };
    this.remove = function () {
        A.remove();
        A = null
    };
    this.addDom = function (D) {
        return (Content.addDom(D))
    };
    this.addControl = function (D) {
        return (Content.addControl(D))
    };
    this.setPosition = function (p) {
        A.style.position = p
    };
    var B = Title_Div.addDom("div");
    B.className = "Title_R";
    B = B.addDom("div");
    B.className = "Title_C";
    var Caption = B.addDom("div");
    Caption.className = "Caption";
    var ButtonDiv = B.addDom("div");
    ButtonDiv.className = "Button";
    var maxButton, setButton, closeButton;
    if (S.setButton != null) {
        setButton = ButtonDiv.addDom("a");
        setButton.className = "Set";
        setButton.href = "#";
        setButton.addEvent("onmousedown", function () {
            if (S.onclose != null) S.onclose();
            T.remove();
            return (false)
        })
    }
    if (S.closeButton != null) {
        closeButton = ButtonDiv.addDom("a");
        closeButton.className = "Close";
        closeButton.href = "#";
        closeButton.addEvent("onmousedown", function () {
            T.remove();
            if (S.onclose != null) S.onclose();
            return (false)
        })
    }
    B = A.addDom("div");
    B.className = "Content_L";
    B = B.addDom("div");
    B.className = "Content_R";
    var Content = B.addDom("div");
    Content.className = "Content_C";
    var Foot_Div = A.addDom("div");
    Foot_Div.className = "Foot_L";
    B = Foot_Div.addDom("div");
    B.className = "Foot_R";
    B = B.addDom("div");
    B.className = "Foot_C";
    this.setCaption = function (text) {
        Caption.innerHTML = text
    };
    this.getCaption = function () {
        return (Caption.innerHTML)
    };
    if (S.caption != null) this.setCaption(S.caption);
    this.setSize = function (w, h) {
        if (w != null) A.style.width = w + "px";
        if (h != null) Content.style.height = (h - Title_Div.offsetHeight - Foot_Div.offsetHeight) + "px"
    };
    if (S.width != null) A.style.width = S.width;
    if (S.height != null) this.setSize(null, S.height);
    if (S.onload != null) S.onload(T)
};

$M.Control["Label"] = function (BoxID, S, CID) {
    var A = null, T = this;
    if (CID != null) A = CID;
    else {
        A = $("<p >" + S.text + "</p>").appendTo(BoxID);
        if (S.align) A.addClass("text-" + S.align);
    };
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    if (S.style) A.css(S.style);
    T.html = function (html) {
        A.html(html);

    };
    T.val = function (text) {
        A.html(text);
    };
};
$M.Control["ListBox"] = function (BoxID, S) {
    var OpenTag = 0;
    var BZ = false;
    var T = this;
    A = BoxID.addDom("div");
    A.className = "M4_ListBox";
    A.unselectable = "on";
    if (S.width) A.style.width = S.width + "px";
    if (S.height) A.style.height = S.height + "px";
    A.style.top = S.top + "px";
    A.style.left = S.left + "px";
    this.setZIndex = function (I) {
        A.style.zIndex = I
    };
    if (S.ZIndex != null) T.setZIndex(S.ZIndex);
    A.addEvent("onmouseover", function () {
        BZ = true;
        if (S.onMouseover != null) S.onMouseover()
    });
    A.addEvent("onmouseout", function () {
        BZ = false;
        if (S.onMouseout != null) S.onMouseout()
    });
    this.width = A.offsetWidth;
    this.height = A.offsetHeight;
    var Dclick = function () {
        if (!BZ) {
            if (OpenTag > 0) {
                if (S.onBlur != null) S.onBlur()
            }
            OpenTag++
        }
    };
    $D.addEvent("onclick", Dclick);
    this.setXY = function (X, Y) {
        if (X != null) A.style.left = X + "px";
        if (Y != null) A.style.top = Y + "px"
    };
    this.addItem = function (text, value) {
        var B = A.addDom("div");
        B.unselectable = "on";
        if (S.type == "color") {
            B.innerHTML = "<span style='float:left;width:12px;height:12px;background-color: " + value + ";overflow:hidden;margin-right:5px;'></span><span>" + text + "</span>"
        } else {
            B.innerHTML = text
        }
        B.value = value;
        B.addEvent("onmouseover", function () {
            B.className = "SelectItem"
        });
        B.addEvent("onmouseout", function () {
            B.className = ""
        });
        B.addEvent("onclick", function () {
            if (S.onSelectItem != null) S.onSelectItem(text, value)
        })
    };
    this.removeItem = function (n) {
        A.childNodes[n].remove()
    };
    this.removeAll = function () {
        var c = A.childNodes.length;
        for (var i = 0; i < c; i++) {
            A.childNodes[0].remove()
        }
    };
    if (S.items != null) {
        for (var i = 0; i < S.items.length; i++) {
            if (S.items[i].text != null) this.addItem(S.items[i].text, S.items[i].value);
            else {
                this.addItem(S.items[i].caption, S.items[i].value)
            }
        }
    }
    this.remove = function () {
        A.remove();
        $D.removeEvent('onclick', Dclick)
    }
};
$M.Control["RadioBox"] = function (BoxID, S, CID) {
    var T = this;
    var A = null;
    if (CID != null) BoxID = CID;
    else {
        var html = "";
        for (var i = 0; i < S.items.length; i++) {
            if (S.showType) {
                html += "<div class=\"radio\">";
                html += "<label >";
                html += "<input type=\"radio\" name=\"" + S.name + "\" value=\"" + S.items[i].value + "\" " + (S.value == S.items[i].value ? "checked" : "") + ">";
                html += S.items[i].text;
                html += "</label>";
                html += "</div>";
            } else {
                html += "<label class=\"radio-inline\">";
                html += "<input type=\"radio\" name=\"" + S.name + "\" value=\"" + S.items[i].value + "\" " + (S.value == S.items[i].value ? "checked" : "") + " >";
                html += S.items[i].text;
                html += "</label>";
            }
        }
        BoxID.append(html);
    }
    var c = BoxID.find("input");
    for (var i = 0; i < c.length; i++) {
        c[i]._control = BoxID;
    }
    //    BoxID._control;
    T.val = function (value) {
        var c = BoxID.find("input");
        if (value != null) {
            for (var i = 0; i < c.length; i++) {

                $(c[i]).prop("checked", $(c[i]).val() == value);

            }
            return value;
        }

        var v = "";

        for (var i = 0; i < c.length; i++) {
            if ($(c[i]).is(':checked')) {
                if (v != "") v += ",";
                v += $(c[i]).val();
            }
        }
        return v;
    };
    T.attr = function (name, value) {
        if (value != null) {
            S[name] = value;

            if (name == "items") init();
        }
        return (S[name]);
    };
};

$M.Control["Switch"] = function (BoxID, S, CID) {
    var T = this;
    var A = null;
    if (S.value == null) S.value = false;
    var html = "";
    if (S.text) {
        S.textOn = S.text;
        S.textOff = S.text;
    } else {
        if (S.textOn == null) S.textOn = "ON";
        if (S.textOff == null) S.textOff = "OFF";
    }
    var size = "switch-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size];
    var checkbox = null;
    var id = "switch_" + $M.Index;
    if (CID != null) {
        checkbox = CID;
        CID.attr("id", id);
        A = $('<label class="switch switch-danger switch-round ' + size + '"></label>');
        A.insertAfter(CID);
        A.append(CID);
        A.append($('<label for="' + id + '" data-on="' + S.textOn + '" data-off="' + S.textOff + '"></label>'));
    }
    else {
        //var color = "switch-" + $M.Control.Constant.colorCss[S.color == null ? 0 : S.color];
        html += '<label class="switch switch-danger switch-round ' + size + '">';
        html += '	<input type="checkbox" name="' + S.name + '" id="' + id + '" value=1 >';
        html += '   <label for="' + id + '" data-on="' + S.textOn + '" data-off="' + S.textOff + '"></label>';
        html += '</label>';
        A = $(html).appendTo(BoxID);
        checkbox = A.find("input");
    }
    checkbox.change(function () { if (S.onChange) S.onChange(T, null); });
    T.val = function (value) {
        if (value != null) {
            if (value) {
                checkbox.attr("checked", true);
                S.value = true;
            } else {
                S.value = false;
                checkbox.removeAttr("checked");
            }
        }
        return checkbox.is(':checked');
    }
    if (S.value) T.val(S.value);
    T.container = A;
    $M.BaseClass.apply(T, [S]);
};
$M.Control["CheckBox"] = function (BoxID, S, CID) {
    var T = this;
    var A = BoxID;
    var init = function () {
        var html = "";
        var value = [];
        if (S.value != null) {
            value = (S.value + "").split(',');
        }
        if (S.items == null) return;
        if (typeof (S.items) == "string") { eval("S.items=" + S.items); }
        for (var i = 0; i < S.items.length; i++) {
            if (S.showType) {
                html += "<div class=\"checkbox\">";
                html += "<label >";
                html += "<input type=\"checkbox\" name=\"" + S.name + "\" value=\"" + S.items[i].value + "\" " + (value.indexOf(S.items[i].value) > -1 ? "checked" : "") + " >";
                html += S.items[i].text;
                html += "</label>";
                html += "</div>";
            } else {
                html += "<label class=\"checkbox-inline\">";
                html += "<input type=\"checkbox\" name=\"" + S.name + "\" value=\"" + S.items[i].value + "\" " + (value.indexOf(S.items[i].value) > -1 ? "checked" : "") + " >";
                html += S.items[i].text;
                html += "</label>";
            }
        }
        A.html(html);
    };
    if (CID != null) A = CID;
    else {
        init();
    }
    T.val = function (value) {
        var c = BoxID.find("input[name='" + S.name + "']");
        if (value != null) {
            for (var i = 0; i < c.length; i++) {
                if (("," + value + ",").indexOf("," + $(c[i]).val() + ",") > -1) {
                    $(c[i]).prop("checked", true);
                } else {
                    $(c[i]).prop("checked", false);
                }
            }
            return value;
        }

        var v = "";

        for (var i = 0; i < c.length; i++) {
            if ($(c[i]).is(':checked')) {
                if (v != "") v += ",";
                v += $(c[i]).val();
            }
        }
        return v;
    };
    BoxID.find("input[name='" + S.name + "']").change(function () {
        if (S.onChange) S.onChange();
    });
    T.container = A;

    $M.BaseClass.apply(T, [S]);
    T.attr = function (name, value) {
        if (value != null) {
            S[name] = value;

            if (name == "items") init();
        }
        return (S[name]);
    };
};

$M.Control["SelectBox"] = function (BoxID, S, CID) {
    var T = this;
    var A = null;
    if (CID != null) A = CID;
    else {
        A = $("<select  class='form-control'></select>").appendTo(BoxID);
    }
    A.attr({ "name": S.name, "id": S.id, "disabled": (S.disabled || S.disabled == "") ? true : false });
    if (S.style) A.css(S.style);
    if (S.cssClass) A.addClass(S.cssClass);
    A.addClass("input-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size]);
    T.container = A;
    $M.BaseClass.apply(T, [S]);

    if (typeof (S.items) == "string") { eval("S.items=" + S.items); }
    S.items = S.items == null ? [] : S.items;
    var loadFlag = true;
    T.addItem = function (items) {
        if (typeof (items.length) != "undefined") {
            for (var i = 0; i < items.length; i++) {
                var value = "", text = "";
                if (typeof (items[i]) == "object") {
                    value = items[i].value == null ? items[i].text : items[i].value;
                    text = items[i].text;
                } else {
                    value = items[i]; text = items[i];
                }
                A.append("<option value=\"" + value + "\">" + text + "</option>");
                if (!loadFlag) S.items[S.items.length] = items[i];
            }
        } else {
            A.append("<option value=\"" + (items.value == null ? items.text : items.value) + "\" >" + items.text + "</option>");
            if (!loadFlag) S.items[S.items.length] = items;
        }
    };

    if (S.vtype) {
        if (typeof (S.vtype) == "string") { A.attr("vtype", S.vtype); }
        else { A.attr(S.vtype); }
    }
    T.val = function (value) {
        if (value) A.val(value);
        return (A.val());
    };
    T.text = function () {
        return (S.items[A[0].selectedIndex]["text"]);
    };
    T.selectedItem = {
        attr: function (name, value) {
            if (value != null) {
                S.items[A[0].selectedIndex][name] = value;
                if (name == "text") {
                    A[0].item(A[0].selectedIndex).innerHTML = value;
                }
            }
            return (S.items[A[0].selectedIndex][name]);
        }
    };
    T.attr = function (name, value) {
        if (value != null) {
            S[name] = value;
            if (name == "selectedIndex") A[0].selectedIndex = value;
        }
        if (name == "selectedIndex") return A[0].selectedIndex;
        return (S[name]);
    };
    T.remove = function (index) {
        A[0].remove(index);
        S.items = S.items.del(index);
    };
    T.clear = function () {
        A[0].length = 0;
        S.items = [];
    };
    T.focus = function () {
        A.focus();
    };
    A.blur(function () {
        if (S.onBlur != null) S.onBlur();
    });
    T.addItem(S.items);
    loadFlag = false;
    T.val(S.value);
    A.change(function () { if (S.onChange) S.onChange(T); });
    return;
    T.disabledTag = false;
    if (S.disabled) T.disabledTag = true;
    var OpenTag = false;
    var A = null;
    var inputTag = S.inputTag;
    if (inputTag == null) inputTag = false;
    if (S.width == null) S.width = 200;

    A.className = "M4_Select";
    if (S.width != null) A.style.width = S.width + "px";
    if (S.ZIndex != null) A.style.zIndex = 10000;
    T.name = S.name;
    if (S.position != null) A.style.position = S.position;
    if (S.top != null) A.style.top = S.top + "px";
    if (S.left != null) A.style.left = S.left + "px";
    var B = A.addDom("input");
    B.className = "TextBox";
    B.name = S.name;
    B.style.width = (S.width - 22) + "px";
    B.style.height = "18px";
    var B2 = A.addDom("div");
    B2.className = "TextBox";
    B2.style.width = (S.width - 22) + "px";
    B2.unselectable = "on";
    B2.style.cursor = "default";
    if (!T.disabledTag) B2.tabIndex = "0";
    if (inputTag) B2.style.display = "none";
    else {
        B.style.display = "none"
    }
    var B1 = A.addDom("input");
    B1.style.display = "none";
    var C = A.addDom("div");
    C.className = "Button";
    C.unselectable = "on";
    if (T.disabledTag) {
        B.className = "TextBox_Disabled";
        B2.className = "TextBox_Disabled";
        C.className = "Button_Disabled"
    }
    this.getIndex = function (v) {
        if (S.items != null) {
            for (var i = 0; i < S.items.length; i++) {
                if ((S.items[i].value != null && S.items[i].value == v) || (S.items[i].value == null && S.items[i].caption == v)) {
                    return i;
                }
            }
        }
        return null;
    };
    this.removeValue = function (v) {
        var i = this.getIndex(v);
        if (i != null) {
            S.items = S.items.del(i);
        }
        this.setIndex(0);
    };
    this.setValue = function (v) {
        var i = this.getIndex(v);
        if (i != null) {
            var text = "";
            if (S.items[i].text != null) text = S.items[i].text;
            else {
                text = S.items[i].caption
            }
            B.value = text;
            B2.innerHTML = text;
            B1.value = (S.items[i].value == null) ? text : S.items[i].value;
            if (S.onchange != null) S.onchange(T);
        }
        //        if (S.items != null) {
        //            for (var i = 0; i < S.items.length; i++) {
        //                if ((S.items[i].value != null && S.items[i].value == v) || (S.items[i].value == null && S.items[i].caption == v)) {
        //                    var text = "";
        //                    if (S.items[i].text != null) text = S.items[i].text;
        //                    else {
        //                        text = S.items[i].caption
        //                    }
        //                    B.value = text;
        //                    B2.innerHTML = text;
        //                    B1.value = (S.items[i].value == null) ? text : S.items[i].value;
        //                    if (S.onchange != null) S.onchange(T);
        //                    return
        //                }
        //            }
        //        }
    };
    this.setIndex = function (index) {
        T.setValue(S.items[index].value)
    };
    if (S.value != null) T.setValue(S.value);
    this.text = "";
    this.value = "";
    this.clearBoth = function () {
        A.style.clear = "both"
    };
    this.setSize = function (w, h) {
        A.setSize(w, h);
        B.setSize(w - 21, null);
        B2.setSize(w - 21, null)
    };
    B.addEvent("onfocus", function () {
        if (T.disabledTag) return;
        B.className = "TextBox_focus";
        B2.className = "TextBox_focus";
        C.className = "Button_focus";
        if (S.onfocus != null) S.onfocus()
    });
    B.addEvent("onblur", function () {
        B.className = "TextBox";
        B2.className = "TextBox";
        C.className = "Button";
        if (S.onblur != null && !OpenTag) S.onblur()
    });
    var openBox = function () {
        if (T.disabledTag) return;
        if (!OpenTag) {
            C.className = "Button_Up";
            B.className = "TextBox_focus";
            B2.className = "TextBox_focus";
            if (inputTag) B.focus();
            else {
                B2.focus()
            }
            var S1 = $B.addControl({
                xtype: 'Shadow',
                width: 0,
                height: 0,
                top: 0,
                left: 0
            });
            var WZ = A.getXY();
            var obj = B2;
            if (inputTag) obj = B;
            var L_height = null;
            var bottomY = $B.scrollTop + $B.offsetHeight,
                rightX = $B.scrollLeft + $B.offsetWidth;
            if (S.items && S.items.length > 10) L_height = 200;
            var L = $B.addControl({
                xtype: "ListBox",
                items: S.items,
                ZIndex: 200000,
                width: (obj.offsetWidth + C.offsetWidth - 2),
                top: (obj.offsetHeight + WZ.top),
                left: WZ.left,
                type: S.type,
                height: L_height,
                onBlur: function () {
                    OpenTag = false;
                    S1.remove();
                    L.remove();
                    if (S.onblur != null) S.onblur()
                },
                onSelectItem: function (text, value) {
                    B.value = text;
                    this.text = text;
                    this.value = value;
                    B1.value = (value == null) ? text : value;
                    B2.innerHTML = text;
                    OpenTag = false;
                    S1.remove();
                    L.remove();
                    if (inputTag) B.focus();
                    else {
                        B2.focus()
                    };
                    if (S.onchange != null) S.onchange(T)
                }
            });
            var L_Left = WZ.left,
                L_Top = (obj.offsetHeight + WZ.top);
            if ((WZ.top + L.height) > bottomY) L_Top = WZ.top - L.height;
            L.setXY(L_Left, L_Top);
            S1.setZIndex(2000);
            S1.setWidth(L.width);
            S1.setHeight(L.height);
            S1.setXY(L_Left, L_Top);
            OpenTag = true
        }
    };
    var inputfocus = function () {
        if (!T.disabledTag) {
            B.className = "TextBox_focus";
            B2.className = "TextBox_focus";
            C.className = "Button_focus";
            B2.focus();
            if (S.onfocus != null) S.onfocus()
        }
    };
    B2.addEvent("onfocus", inputfocus);
    B2.addEvent("onblur", function () {
        if (T.disabledTag) return;
        B.className = "TextBox";
        B2.className = "TextBox";
        C.className = "Button";
        if (S.onblur != null && !OpenTag) S.onblur()
    });
    B2.addEvent("onmouseup", openBox);
    C.addEvent("onmousedown", function () {
        if (T.disabledTag) return;
        C.className = "Button_Down"
    });
    C.addEvent("onmouseup", openBox);
    this.addItem = function (text, value) {
        if (S.items == null) S.items = new Array();
        S.items[S.items.length] = {
            text: text,
            value: value
        }
    };
    this.disabled = function (tag) {
        T.disabledTag = tag;
        if (tag) {
            B.className = "TextBox_Disabled";
            B2.className = "TextBox_Disabled";
            C.className = "Button_Disabled"
        } else {
            B.className = "TextBox";
            B2.className = "TextBox";
            C.className = "Button"
        }
    };
    this.clear = function () {
        if (S.items != null) S.items.length = 0
    };
    this.remove = function () {
        C.remove();
        B.remove();
        A.remove()
    };
    this.setfocus = function () {
        if (!T.disabledTag) {
            if (inputTag) B.focus();
            else {
                inputfocus()
            }
        }
    };
    this.focus = function () {
        if (!T.disabledTag) {
            if (inputTag) B.focus();
            else {
                inputfocus()
            }
        }
    };
    this.val = function () {
        return (B1.value)
    };
    this.setAttribute = function (a, b) {
        S[a] = b
    };
    this.focus()
};
$M.Control["InputGroup"] = function (BoxID, S) {
    var T = this;
    T.controls = [];
    var A = $("<div class=\"input-group\" ></div>").appendTo(BoxID);
    for (var i = 0; i < S.items.length; i++) {
        if (S.items[i].xtype == "Button") {
            var span = $("<span class='input-group-btn'></span>").appendTo(A);
            T.controls[T.controls.length] = span.addControl(S.items[i]);
        } else {
            T.controls[T.controls.length] = A.addControl(S.items[i]);
        }
    }
    if (S.style) A.css(S.style);
    T.container = A;
    $M.BaseClass.apply(T, [S]);
};
$M.Control["ButtonCheckGroup"] = function (BoxID, S) {
    var T = this;
    var A = $("<div class=\"btn-group\"></div>").appendTo(BoxID);
    for (var i = 0; i < S.items.length; i++) {


        var html = (S.items[i].ico != null) ? "<i class=\"" + $M.getIco(S.items[i].ico) + "\"></i> " : "";
        if (S.items[i].text != null) html += S.items[i].text;
        S.items[i].dom = $("<button type=\"button\" class=\"btn btn-default\" value=\"" + S.items[i].value + "\">" + html + "</button>").appendTo(A);
        if (S.items[i].tip) S.items[i].dom.attr("title", S.items[i].tip);
        S.items[i].dom.addClass("btn-" + $M.Control.Constant.colorCss[S.color == null ? 0 : S.items[i].color]);
        S.items[i].dom.addClass("btn-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.items[i].size]);
        S.items[i].dom.click(function () {
            A.find("button").removeClass("active");
            $(this).addClass("active");
            if (S.onClick) S.onClick(T, { value: $(this).val() });
        });
    }
    T.val = function (value) {
        for (var i = 0; i < S.items.length; i++) {
            if (S.items[i].dom.val() == S.value) {
                S.items[i].dom.addClass("active");
            }
        }
    };
    if (S.value != null) T.val(S.value);

    T.container = A;
    $M.BaseClass.apply(T, [S]);
};
$M.Control["Button"] = function(BoxID, S, CID) {
    var T = this;
    var A = null;
    var group = null, menuButton = null;
    T.val = function(text) {
        
        //if (text == null) {
        //} else {
        S.text = text;
        var html = (S.ico != null) ? "<i class=\"" + $M.getIco(S.ico) + "\"></i> " : "";
        if (S.text != null) html += S.text;
        if (group && S.onClick == null) html += "<span class=\"caret\"></span>";
        A.html(html);
        //}
    };
    if (CID != null) {
        A = CID;
    }
    else {

        if (S.menu) {
            if (S.menu instanceof Array) T.menu = $(document.body).addControl({ menuType: S.menuType, xtype: "Menu", items: S.menu, onItemClicked: S.onDropDownItemClicked });
            else { T.menu = S.menu; }
            group = $("<div class=\"btn-group\"/>").appendTo(BoxID); BoxID = group;
        }
        A = $("<button class=\"btn\" type=\"" + S.type + "\" />").appendTo(BoxID);
        if (S.tip) A.attr("title", S.tip);
        T.val(S.text);
        A.attr({ "name": S.name, "id": S.id, "disabled": S.enabled == false ? true : false });
        if (S.style) A.css(S.style);
        if (S.cssClass) A.addClass(S.cssClass);
        if (S.primary) {
            A.addClass("btn-primary");
        } else {
            A.addClass("btn-" + $M.Control.Constant.colorCss[S.color == null ? 0 : S.color]);
        }
        A.addClass("btn-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size]);
        if (group) {
            if (S.onClick != null) {
                menuButton = $("<button type=\"button\" class=\"btn\"><span class=\"caret\"></span></button>").appendTo(group);
                menuButton.addClass("btn-" + $M.Control.Constant.colorCss[S.color == null ? 0 : S.color]);
                menuButton.addClass("btn-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size]);
            } else {
                //$("<span class=\"caret\"></span>").appendTo(A);
                menuButton = A;
            }
            menuButton.addClass("dropdown-toggle");
        }
        //A.attr("type", S.type);
    }
    T.focus = function() {
        A.focus();
    };
    T.enabled = function(flag) {
        A.attr({ "disabled": !flag });
        if (menuButton) menuButton.attr({ "disabled": !flag });
    };
    var objID = "Button_" + $.Index + "_";
    A.attr("id", objID);
    A.click(function() {
        var f = true;
        if (S.onClick) { S.onClick(T, null); f = false; }
        return f;
    });
    if (T.menu) {
        menuButton.on("click", function() {
            if (T.menu) {
                group.addClass("open");
                var xy = group.offset();
                T.menu.open(null, null, group);
            }
            T.menu.attr("onClose", function() {
                group.removeClass("open");
                if (S.onMenuClose) S.onMenuClose(T, null);
            });
            return false;
        });
    }
    T.container = A;
    $M.BaseClass.apply(T, [S]);
};
$M.Control["Spinner"] = function (BoxID, S, CID) {
    T = this;
    S.minimum = S.minimum == null ? 0 : S.minimum;
    S.maximum = S.maximum == null ? 100 : S.maximum;
    S.step = S.step == null ? 1 : S.step;
    if (CID != null) A = CID;
    else {
        A = $("<span class=\"ui-spinner\"></span>").appendTo(BoxID);
    };
    if (S.style) A.css(S.style);
    var input = $("<input name=\"" + S.name + "\" class=\"form-control ui-spinner-input\" >").appendTo(A);
    var upButton = $("<a class=\"ui-spinner-button ui-spinner-up fa fa-caret-up\" tabindex=\"-1\" ></a>").appendTo(A);
    var downButton = $("<a class=\"ui-spinner-button ui-spinner-down  fa fa-caret-down\" tabindex=\"-1\" ></a></span>").appendTo(A);
    var _down = function () {
        var value = parseInt(input.val());
        if (isNaN(parseInt(value))) {
            value = S.maximum;
        } else {
            value = value - S.step;
        }
        if (value < S.minimum) value = S.maximum;
        input.val(value);
    };
    var _up = function () {
        var value = parseInt(input.val());
        if (isNaN(parseInt(value))) {
            value = S.minimum;
        } else {
            value = value + S.step;
        }
        if (value > S.maximum) value = S.minimum;
        input.val(value);
    };
    A.keydown(function (e) {
        if (e.which == 38) _up();
        if (e.which == 40) _down();
    });
    upButton.mousedown(_up);
    downButton.mousedown(_down);
    T.val = function (value) { if (value != null) { input.val(value); } return input.val(); };
    if (S.value != null) T.val(S.value);
    T.container = A;
    $M.BaseClass.apply(T, [S]);
};
$M.Control["TextBox"] = function (BoxID, S, CID) {
    var T = this;
    var B = null;
    if (CID) {
        B = CID;
        if (S._control != null) {
            B[0]._control = $("#" + S._control);
        }
    } else {
        if (S.password) {
            B = $("<input type=password class=form-control />").appendTo(BoxID);
        } else {
            if (S.multiLine) {
                B = $("<textarea class=form-control rows='" + S.rows + "' />").appendTo(BoxID);
            } else {
                B = $("<input class=form-control />").appendTo(BoxID);
            }
        }
    }
    if (S.mask) B.mask(S.mask);
    if (S.visible == false) B.hide();
    B.attr({ "name": S.name, "id": S.id, "disabled": S.disabled ? true : false, "placeholder": S.placeholder });
    B.addClass("input-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size]);
    if (S.style) B.css(S.style);
    if (S.cssClass) B.addClass(S.cssClass);
    if (S.vtype) {
        if (typeof (S.vtype) == "string") { B.attr("vtype", S.vtype); }
        else { B.attr(S.vtype); }
    }
    T.css = function (json) {
        B.css(json);
    };
    B.blur(function () { if (S.onBlur) S.onBlur(T); });
    B.change(function () { if (S.onChange) S.onChange(T); });
    B.keydown(function (e) {
        if (S.onKeyDown) S.onKeyDown(T, e);
    });
    B.keyup(function (e) {
        if (S.onKeyUp) S.onKeyUp(T, e);
    });
    T.val = function (value) {
        if (value == null) {
            if (B.val() == S.tip) {
                return ("")
            } else {
                return (B.val())
            }
        } else {
            B.val(value);
        }
    };
    T.focus = function () {
        B.focus();
    };

    T.container = B;
    $M.BaseClass.apply(T, [S]);
    if (S.value) T.val(S.value);
};
$M.Control["Shadow"] = function (BoxID, Setting) {
    var B = BoxID.addDom("div");
    if (Setting.width != null) B.style.width = Setting.width + "px";
    if (Setting.height != null) B.style.height = Setting.height + "px";
    if (Setting.left != null) B.style.left = Setting.left + "px";
    if (Setting.top != null) B.style.top = Setting.top + "px";
    B.className = "M4_Shadow";
    this.setWidth = function (W) {
        if ($.Browse.isIE()) {
            B.style.width = W + "px"
        } else {
            B.style.width = W - 5 + "px"
        }
    };
    this.setHeight = function (H) {
        if ($.Browse.isIE()) {
            B.style.height = H + "px"
        } else {
            B.style.height = H - 5 + "px"
        }
    };
    this.setZIndex = function (I) {
        B.style.zIndex = I
    };
    this.setLeft = function (L) {
        if ($.Browse.isIE()) {
            B.style.left = L + "px"
        } else {
            B.style.left = L + "px"
        }
    };
    this.setTop = function (T) {
        if ($.Browse.isIE()) {
            B.style.top = T + "px"
        } else {
            B.style.top = T + "px"
        }
    };
    this.setXY = function (l, t) {
        if ($.Browse.isIE()) {
            B.style.top = t - 5 + "px";
            B.style.left = l - 5 + "px"
        } else {
            B.style.top = t + "px";
            B.style.left = l + "px"
        }
    };
    this.hide = function () {
        B.hide()
    };
    this.show = function () {
        B.show()
    };
    this.remove = function () {
        B.remove();
        B = null
    }
};
$M.Control["Tip"] = function (BoxID, Setting) {
    var S = null;
    if ($.Browse.isIE()) {
        S = BoxID.addControl({
            xtype: 'Shadow',
            width: 0,
            height: 0,
            top: 0,
            left: 0
        })
    };
    var B = BoxID.addDom("div");
    B.style.zIndex = 10000;
    B.className = Setting.className;
    var A = B.addDom("div");
    A.className = "left";
    A = A.addDom("div");
    A.className = "right";
    A = A.addDom("div");
    A.className = "C";
    A = B.addDom("div");
    A.className = "left1";
    A = A.addDom("div");
    A.className = "right1";
    var M = A.addDom("div");
    M.className = "C1";
    A = B.addDom("div");
    A.className = "left2";
    A = A.addDom("div");
    A.className = "right2";
    A = A.addDom("div");
    A.className = "C2";
    B.style.width = "200px";
    if (Setting.width != null) B.style.width = Setting.width + "px";
    this.remove = function () {
        B.remove();
        if (S != null) {
            S.remove();
            S = null
        };
        A = null;
        B = null
    };
    this.show = function () {
        B.show();
        S.show()
    };
    this.setXY = function (X, Y) {
        B.style.left = X + "px";
        B.style.top = Y + "px";
        if ($.Browse.isIE()) {
            S.setWidth(B.offsetWidth - 5);
            S.setHeight(B.offsetHeight - 5);
            S.setTop(B.offsetTop);
            S.setLeft(B.offsetLeft)
        }
    };
    if (Setting.HTML != null) M.innerHTML = Setting.HTML;
};
$M.Control["Err"] = function (BoxID, Setting) {
    var B = BoxID.addDom("div");
    var T;
    B.className = "Err";
    var ShowMsg = function () {
        T = $B.addControl({
            xtype: 'Tip',
            HTML: Setting.text,
            className: 'M4_TipErrBox'
        });
        T.setXY(B.getXY().left + 30, B.getXY().top + 30)
    };
    var HideMsg = function () {
        setTimeout(T.remove, 500)
    };
    this.hide = function () {
        B.remove();
        B = null;
        T = null
    };
    B.addEvent("onmouseover", ShowMsg);
    B.addEvent("onmouseout", HideMsg)
};

$M.Control["DateRangePickerInput"] = function (BoxID, S, CID) {
    var A = $("<div class=\"input-group input-daterange input-group-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size] + "\" ></div>"), input = null;
    if (CID) {
        A.insertBefore(CID);
        input = CID; input.hide();
    } else {
        BoxID.append(A);
        input = $("<input name=" + S.name + " type=hidden >");
    }
    if (S.style) A.css(S.style);
    if (S.format == null) S.format = "yyyy-MM-dd";
    var v1 = new Date().format(S.format), v2 = new Date().format(S.format);
    if (S.value) {
        var item = S.value.split(",");
        v1 = item[0];
        v2 = item[1];
    }
    var value = new Date();
    var p1 = A.addControl({ xtype: "DatePickerInput", size: S.size, value: v1, toValue: v2 });
    $("<span class=\"input-group-btn add-on\">to</span>").appendTo(A);

    var p2 = A.addControl({ xtype: "DatePickerInput", size: S.size, value: v2, fromValue: v1 });
    var menu = A.addControl({
        xtype: "Menu", onItemClicked: function (sender, e) {
            var day = new Date();
            switch (e.attr("val")) {
                case 0:
                    p1.val(day);
                    p2.val(day);
                    break;
                case 1:
                    day.addDays(-1);
                    p1.val(day);
                    p2.val(day);
                    break;
                case 2:
                    p2.val(day);
                    day.addDays(-7);
                    p1.val(day);
                    /*
                    var week = "1234560";
                    var index = week.indexOf(day.getDay());
                    p2.val(day);
                    p1.val(day.addDays(-index));*/
                    break;
                case 3:
                    p2.val(day);
                    day.addMonths(-1);
                    p1.val(day);
                    /*
                    var index = day.getDate() - 1;
                    p2.val(day);
                    p1.val(day.addDays(-index));*/
                    break;
            }
        }, items: [{ text: "今天", val: 0 }, { text: "咋天", val: 1 }, { text: "最近七天", val: 2 }, { text: "最近一月", val: 3 }]
    });
    var button = $("<span class=\"input-group-btn open\"><button class='btn btn-default' ><span class=\"caret\"></span></button></span>").appendTo(A);
    button.find("button").click(function () {
        menu.open(null, null, button);
        return false;
    });
    p1.attr("onChange", function () {
        p2.attr("fromValue", p1.val());
        input.val(p1.val() + "," + p2.val());
    });

    p2.attr("onChange", function () {
        p1.attr("toValue", p2.val());
        input.val(p1.val() + "," + p2.val());
    });
    var T = this;
    T.dispose = function () {
        p1.dispose();
        p2.dispose();
        A.remove();
        A = null;
    };
    T.val = function (value) {
        if (value != null) {
            var v = value.split(',');
            p1.val(v[0]);
            p2.val(v[1]);
        }
        input.val(p1.val() + "," + p2.val());
        return input.val();
    };
    input.val(p1.val() + "," + p2.val());

    T.attr = function (a, b) {
        if (b != null) S[a] = b;
        return S[a];
    }
};
$M.Control["DatePickerInput"] = function (BoxID, S, CID) {
    var A = null;
    if (CID) {
        A = $("<div class=\"input-group input-daterange input-group-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size] + "\"></div>");
        A.insertBefore(CID);
    } else {
        if (S.fromValue || S.toValue) {
            A = BoxID;
        } else {
            A = $("<div class=\"input-group input-daterange input-group-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size] + "\"></div>").appendTo(BoxID);
        }
    }
    if (S.style) A.css(S.style);
    var input = null;//
    if (CID) {
        input = CID.appendTo(A);
    } else {
        input = $("<input class=\"form-control input-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size] + "\" name=\"" + S.name + "\">").appendTo(A);
    }
    input[0]._control = A[0];
    var button = $("<span class=\"input-group-btn\"><button class=\"dropdown-toggle btn btn-default btn-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size] + "\" ><i class=\"fa fa-calendar\"></i></button></span>").appendTo(A);
    var popover = $(document.body).addControl({ xtype: "Popover", location: "bottom", onClose: function () { button.removeClass("open"); } });
    var picker = popover.addControl({
        xtype: "DatePicker", value: S.value, format: S.format, minValue: S.minValue, maxValue: S.maxValue, toValue: S.toValue, fromValue: S.fromValue, onChange: function () {
            popover.close();
            T.val(picker.val());
        }
    });
    input.change(function () { S.value = input.val(); })
    button.click(function () {
        button.addClass("open");
        picker.val(S["value"]);
        popover.show(button);
        return false;
    });
    var T = this;
    T.dispose = function () {
        popover.dispose();
        A.remove();
        A = null;
    };
    T.val = function (a) {
        if (a != null) {
            if (!(a instanceof Date)) a = (a + "").toDate();
            S["value"] = a.format("yyyy-MM-dd");
            input.val(S["value"]);
            if (S.onChange) S.onChange(T, null);
        }
        return S["value"];
    };
    T.attr = function (a, b) {
        if (a == "minValue" || a == "maxValue" || a == "toValue" || a == "fromValue") picker.attr(a, b);
        if (b != null) S[a] = b;

        return S[a];
    };
    if (S.value) T.val(S.value);
};
$M.Control["DatePicker"] = function (BoxID, S) {
    var T = this;
    if (S.format == null) S.format = "yyyy-MM-dd";
    var date = new Date();
    var minDate = null, maxDate = null;
    var toDate = null, fromDate = null;
    if (S.value) date = (S.value + "").toDate()
    if (S.minValue) minDate = (S.minValue + "").toDate();
    if (S.maxValue) maxDate = (S.maxValue + "").toDate();
    if (S.toValue) toDate = (S.toValue + "").toDate();
    if (S.fromValue) fromDate = (S.fromValue + "").toDate();
    var year = date.getFullYear(), month = date.getMonth() + 1, day = date.getDate();

    var lastMonth = 1;
    var openMonthSwich = function () {
        A.hide();
        B.show();
        if (lastMonth != month) $(monthList[lastMonth - 1]).removeClass("active");
        $(monthList[month - 1]).addClass("active");
        lastMonth = month;
    };
    var menu = $("<div class=\"datepicker\" style=\"display: block; \"></div>").appendTo(BoxID);
    var A = $("<div class=\"datepicker-days\" style=\"display: block;\"></div>").appendTo(menu);

    var getMonthDay = function (year, month) {
        var tmp = new Date(year, month, 0);
        return tmp.getDate()
    };
    var getDayWeek = function (D) {
        var tmp = new Date(Date.parse(D));
        return tmp.getDay()
    };
    var showDay = function () {
        monthswitch.html(year + "年");
        var table = "<table class=\" table-condensed\">" +
        "<thead><tr>" +
        "<th class=\"prev\" style=\"visibility: visible;\">" +
	    "<i class=\"glyphicon glyphicon-arrow-left\"></i></th>" +
	    "<th colspan=\"5\" class=\"_switch\">" + year + "年 " + month + "月</th>" +
	    "<th class=\"next\" style=\"visibility: visible;\">" +
	    "<i class=\"glyphicon glyphicon-arrow-right\"></i></th>" +
        "</tr>" +
        "<tr>" +
	    "   <th class=\"dow\">日</th>" +
	    "   <th class=\"dow\">一</th>" +
	    "   <th class=\"dow\">二</th>" +
	    "   <th class=\"dow\">三</th>" +
	    "   <th class=\"dow\">四</th>" +
	    "   <th class=\"dow\">五</th>" +
	    "   <th class=\"dow\">六</th>" +
	    "</tr>" +
        "</thead>";
        var html = "";
        var LC = getMonthDay(year, month - 1);
        var SD = getDayWeek(year + "/" + month + "/1");
        var SC = getMonthDay(year, month);
        var nowDate = new Date(year + "/" + month + "/" + day);
        var i1 = 0;
        var Count = 7;
        var AddDay = function (Y, M, D, className) {
            var val = "";
            if (D != "") {
                var date = new Date(Y + "/" + M + "/" + D);
                if ((minDate && date < minDate) || (fromDate && date < fromDate)) className = "day disabled";
                if ((maxDate && date > maxDate) || (toDate && date > toDate)) className = "day disabled";
                if (toDate && date < toDate && date > nowDate) className = "day range";
                if (fromDate && date > fromDate && date < nowDate) className = "day range";
                if (toDate && date.getTime() == toDate.getTime()) className = "day selected";
                if (fromDate && date.getTime() == fromDate.getTime()) className = "day selected";
                if (D == day) className = "day active";
                if (className == "day" || className == "day range" || className.indexOf("selected") > -1) val = " val=" + D;
            }
            html += "<td class=\"" + className + "\" " + val + " >" + D + "</td>";
            if (i1 % 7 == 0) html += "</tr><tr>";
        };
        for (var i = 0; i < SD; i++) {
            i1++;
            AddDay(year, month - 1, "", "")
        }
        for (var i = 1; i <= SC; i++) {
            i1++
            AddDay(year, month, i, "day");
        }
        var i2 = 0;
        for (var i = i1; i < 42; i++) {
            i1++;
            AddDay(year, month + 1, "", "")
        }

        A.html(table + "<tr>" + html + "</tr></table>");
        A.find("._switch").click(openMonthSwich);
        A.find(".prev").click(function () {
            month--;
            if (month == 0) { year--; month = 12; }
            showDay();

        });
        A.find(".next").click(function () {

            month++;
            if (month == 13) { year++; month = 1; }
            showDay();
        });
        A.find(".day").click(function () {
            if ($(this).attr("val") == null) return;
            day = $(this).html();
            showDay();
            if (S.onChange) S.onChange(T, null);
        });
        T.val = function (a) {
            if (a) {
                date = a.toDate();
                if (date == null) date = new Date();
                year = date.getFullYear(); month = date.getMonth() + 1; day = date.getDate();
                showDay();
                A.show();
                B.hide();
                C.hide();
            }
            var date = new Date(year + "/" + month + "/" + day);
            return date.format(S.format);
        };
        T.container = A;
        $M.BaseClass.apply(T, [S]);
        T.attr = function (a, b) {
            if (b != null) {
                S[a] = b;
                switch (a) {
                    case "minValue":
                        minDate = (S.minValue + "").toDate();
                        break;
                    case "maxDate":
                        maxDate = (S.maxValue + "").toDate();
                        break;
                    case "toValue":
                        toDate = (S.toValue + "").toDate();
                        break;
                    case "fromValue":
                        fromDate = (S.fromValue + "").toDate();
                        break;
                }
                showDay();
            }
        };
    };
    var monthHtml = "<div  class='datepicker-months'>";
    monthHtml += "<table class=\"table-condensed\">";
    monthHtml += "<thead><tr>";
    monthHtml += "<th class=\"prev\"><i class=\"glyphicon glyphicon-arrow-left\"></i></th>";
    monthHtml += "<th colspan=\"5\" class=\"_switch\">" + year + "年</th>";
    monthHtml += "<th class=\"next\"><i class=\"glyphicon glyphicon-arrow-right\"></i></th>";
    monthHtml += "</tr></thead><tr>";
    monthHtml += "<td colspan=\"7\">";
    for (var i = 1; i < 13; i++) monthHtml += "<span class=\"month" + (i == month ? " active" : "") + "\" val=" + i + ">" + i + "月</span>";
    monthHtml += "</td>";
    monthHtml += "</tr></table></div>";
    var B = $(monthHtml).appendTo(menu);
    var C = $("<div  class='datepicker-years'></div>").appendTo(menu);
    var monthswitch = B.find("._switch");
    var monthList = B.find(".month");
    monthList.click(function () {
        B.hide();
        A.show();
        month = $(this).attr("val");
        showDay();
    });
    monthswitch.click(function () {
        B.hide();
        tempyear = parseInt(year / 10) * 10;
        showYearList();
        C.show();
    });
    B.find(".prev").click(function () {
        year--;
        monthswitch.html(year + "年");
    });
    B.find(".next").click(function () {
        year++;
        monthswitch.html(year + "年");
    });
    var tempyear = 0;
    var showYearList = function () {

        var monthHtml = "<table class=\"table-condensed\">";
        monthHtml += "<thead><tr>";
        monthHtml += "<th class=\"prev\"><i class=\"glyphicon glyphicon-arrow-left\"></i></th>";
        monthHtml += "<th colspan=\"5\" class=\"_switch\">" + tempyear + "年-" + (tempyear + 10) + "年</th>";
        monthHtml += "<th class=\"next\"><i class=\"glyphicon glyphicon-arrow-right\"></i></th>";
        monthHtml += "</tr></thead><tr>";
        monthHtml += "<td colspan=\"7\">";
        for (var i = 1; i < 17; i++) monthHtml += "<span class=\"year" + ((tempyear + i) == year ? " active" : "") + "\" val=" + (tempyear + i) + ">" + (tempyear + i) + "年</span>";
        monthHtml += "</td>";
        monthHtml += "</tr></table>";
        C.html(monthHtml);
        var yearswitch = C.find("._switch");
        C.find(".year").click(function () {
            year = $(this).attr("val");
            monthswitch.html(year + "年");
            C.hide();
            B.show();
            if (lastMonth != month) $(monthList[lastMonth - 1]).removeClass("active");
            $(monthList[month - 1]).addClass("active");
            lastMonth = month;
        });
        C.find(".prev").click(function () {
            tempyear = (parseInt(tempyear / 10) - 1) * 10;
            yearswitch.html(tempyear + "年-" + (tempyear + 10) + "年");
            showYearList();
        });
        C.find(".next").click(function () {
            tempyear = (parseInt(tempyear / 10) + 1) * 10;
            yearswitch.html(tempyear + "年-" + (tempyear + 10) + "年");
            showYearList();
        });
    };
    showDay();
};

$M.Control["ProgressBar"] = function (BoxID, S) {
    var T = this;
    this.value = 0;
    var A = $("<div class=\"progress\"></div>").appendTo(BoxID);
    var P = $(" <div class=\"progress-bar progress-bar-success progress-bar-striped\"  style=\"width: 0%\"></div>").appendTo(A);
    var span = $("<span class=\"sr-only\"></span>").appendTo(P);
    var refresh = function () {
        var value = ((S.value + 0.0) / (S.maximum - S.minimum)) * 100;
        P.css({ width: value + "%" });
    };
    if (S.minimum == null) S.minimum = 0;
    if (S.maximum == null) S.maximum = 100;
    T.attr = function (a, b) {
        if (b != null) S[a] = b;
        if (a == "maximum" || a == "minimum" || a == "value") {
            refresh();
        }

    };
    T.val = function (a) {
        if (a != null) T.attr("value", a);
        return S["value"];
    };
    refresh();
};
$M.Control["TrackBar"] = function (BoxID, S) {
    var T = this;
    var Tag = false;
    var xz = 0,
        oldValue = null,
        objID = "TrackBar_" + $.Index + "_";
    var Button = objID + "0";
    this.value = 0;
    var A = BoxID.addDom("div");
    A.className = "M4_TrackBar";
    if (S.width != null) A.style.width = S.width + "px";
    A.tabIndex = 1;
    var html = "<div class='left' unselectable='on'><div class='right' unselectable='on'><div class='C' unselectable='on'><div class='button' unselectable='on' id='" + Button + "'></div></div></div></div>";
    A.innerHTML = html;
    Button = $(Button);
    T.addEvent = function (E, F) {
        S[E] = F
    };
    T.setValue = function (value) {
        value = Math.floor(value);
        var xy = A.getXY();
        if (value == null) value = S.min;
        if (value > S.max) value = S.max;
        if (value < S.min) value = S.min;
        var bfb = (A.offsetWidth - 10) / 100;
        if (S.max == S.min) {
            Button.style.left = "0px"
        } else {
            Button.style.left = (bfb * ((value - S.min) / ((S.max - S.min) / 100)) - 5) + "px"
        }
        this.value = value
    };
    T.setAttribute = function (a, b) {
        S[a] = b
    };
    A.addEvent("onkeydown", function () {
        if ($.event.keyCode() == 37) T.setValue(T.value - 1);
        if ($.event.keyCode() == 39) T.setValue(T.value + 1)
    });
    A.addEvent("onclick", function () {
        var x = $.event.x();
        var xy = A.getXY();
        var bfb = (A.offsetWidth - 10) / 100;
        var value = ((x - xy.left - (Button.offsetWidth / 2)) / bfb) * ((S.max - S.min) / 100) + S.min;
        oldValue = T.value;
        T.setValue(value);
        if (S.onchange != null && oldValue != T.value) S.onchange(T.value);
        A.focus()
    });
    Button.addEvent("onmouseout", function () {
        Button.className = "button"
    });
    Button.addEvent("onmousedown", function () {
        oldValue = T.value;
        Button.className = "button_down";
        var x = $.event.x();
        xz = x - Button.offsetLeft;
        Button.setCapture();
        Tag = true
    });
    Button.addEvent("onmousemove", function () {
        Button.className = "button_move";
        if (Tag) {
            var x = $.event.x();
            var xy = A.getXY();
            var WZ = 0;
            if (x > A.offsetWidth + xy.left - Button.offsetWidth) {
                WZ = A.offsetWidth + xy.left - Button.offsetWidth
            } else if (x < xy.left) {
                WZ = xy.left
            } else {
                WZ = x - xz
            }
            var bfb = (A.offsetWidth - 10) / 100;
            var value = ((WZ - xy.left) / bfb) * ((S.max - S.min) / 100) + S.min;
            T.setValue(value)
        }
    });
    Button.addEvent("onmouseup", function () {
        Tag = false;
        Button.releaseCapture();
        Button.className = "button";
        if (S.onchange != null && oldValue != T.value) S.onchange(T.value)
    });
    if (S.value == null) this.setValue(S.min);
    else {
        this.setValue(S.value)
    }
};

$M.Control["ColorBox"] = function (BoxID, S) {
    var T = this;
    var A = null;
    var list = [["#000", "#444", "#666", "#999", "#ccc", "#eee", "#f3f3f3", "#fff"],
        ["#f00", "#f90", "#ff0", "#0f0", "#0ff", "#00f", "#90f", "#f0f"],
        ["#f4cccc", "#fce5cd", "#fff2cc", "#d9ead3", "#d0e0e3", "#cfe2f3", "#d9d2e9", "#ead1dc"],
        ["#ea9999", "#f9cb9c", "#ffe599", "#b6d7a8", "#a2c4c9", "#9fc5e8", "#b4a7d6", "#d5a6bd"],
        ["#e06666", "#f6b26b", "#ffd966", "#93c47d", "#76a5af", "#6fa8dc", "#8e7cc3", "#c27ba0"],
        ["#c00", "#e69138", "#f1c232", "#6aa84f", "#45818e", "#3d85c6", "#674ea7", "#a64d79"],
        ["#900", "#b45f06", "#bf9000", "#38761d", "#134f5c", "#0b5394", "#351c75", "#741b47"],
        ["#600", "#783f04", "#7f6000", "#274e13", "#0c343d", "#073763", "#20124d", "#4c1130"]];
    T.open = function (x, y, obj) {
        if (obj != null) BoxID = obj;
        var color = null;
        A = $("<div class=\"M5_droppicker dropdown-menu\" ></div>").appendTo(BoxID);
        var html = "<div class=\"M5_Color btn-group\" >";
        for (var i = 0; i < list.length; i++) {
            html += "<div class=\"color-row\">";
            for (var i1 = 0; i1 < list[i].length; i1++) {
                html += "<button type=\"button\" class=\"color-btn\" style=\"background-color:" + list[i][i1] + ";\"  val=\"" + list[i][i1] + "\"  />";
            }
            html += "</div>";
        }
        html += "</div>";
        A.html(html);
        A.find("button").click(function () { color = $(this).attr("val"); T.close(); });
        if (x != null && y != null) A.css({ left: x + "px", top: y + "px", display: "inline" });
        $(document).on("mousedown", function (e) {
            if (A.has(e.target).length == 0) T.close();
        });
    };
    T.val = function () { return color; };
    T.close = function () {
        if (S.onClose) S.onClose();
        //$(document).unbind("keydown", keydown);
        $(document).unbind("mousedown", T.close);
        if (A) A.remove();
    };
    $M.BaseClass.apply(T, [S]);
};
$M.Control["ColorBox2"] = function (BoxID, S) {
    var T = this;
    var A = null;
    T.open = function (x, y, obj) {
        if (obj != null) BoxID = obj;
        A = $("<div class=\"droppicker dropdown-menu\" ></div>").appendTo(BoxID);
        if (x != null && y != null) A.css({ left: x + "px", top: y + "px", display: "inline" });
        var box = $("<div></div>").appendTo(A);
        box.minicolors({
            control: false,
            inline: true,
            change: function (hex, opacity) {
                //T.close();
                //                if (!hex) return;
            }
        });
        if (x != null && y != null) A.css({ left: x + "px", top: y + "px", display: "inline" });

        //$(document).on("keydown", keydown);
        $(document).on("mousedown", function (e) {
            if (A.has(e.target).length == 0) T.close();
        });
    };
    T.close = function () {
        if (S.onClose) S.onClose();
        //$(document).unbind("keydown", keydown);
        $(document).unbind("mousedown", T.close);
        if (A) A.remove();
    };
    $M.BaseClass.apply(T, [S]);
};
$M.Control["Menu"] = function (BoxID, S) {
    var T = this;
    var A = null;
    if (!S.event) S.event = {};
    var keydown = function (e) {
        if (e.keyCode == 27) T.close();
        var $items = A.find("li a");
        if (!$items.length || (e.keyCode != 38 && e.keyCode != 40)) return
        var index = $items.index($items.filter(':focus'))
        if (e.keyCode == 38) {
            index = (index > 0) ? index - 1 : $items.length - 1;                        // up
        }
        if (e.keyCode == 40) {
            index = (index < $items.length - 1) ? index + 1 : 0;                        // down
        }
        if (! ~index) index = 0
        $items.eq(index).focus()
    };
    var menuItem = function (item) {
        var T2 = this;
        T2.items = [];
        T2.attr = function (a, b) {
            if (b != null) item[a] = b;
            return item[a];
        }
        if (item.items) {
            for (var i = 0; i < item.items.length; i++) {
                T2.items[i] = new menuItem(item.items[i]);
            }
        }
        T2.addDom = function (dom) {
            if (item.text == "-") {
                dom.append("<li class=\"divider\"></li>");
            } else {
                if (item.items) {
                    var li = $("<li  class=\"dropdown-submenu " + (item.enabled == null || item.enabled ? "" : "disabled") + "\"><a href=\"#\"><i class='ico fa " + (item.ico ? item.ico : "") + "' />" + item.text + "</a></li>").appendTo(dom);

                    var dom2 = $("<ul class=\"dropdown-menu\" ></ul>").appendTo(li);
                    for (var i = 0; i < T2.items.length; i++) {
                        T2.items[i].addDom(dom2);
                    }
                } else {
                    var id = item.id != null ? " id='" + item.id + "' " : "";
                    var li = $("<li " + id + " class='" + (item.enabled == null || item.enabled ? "" : "disabled") + "'><a href=\"#\"><i class='ico fa" + (item.checked ? " fa-check" : "") + " " + (item.ico ? item.ico : "") + "' />" + item.text + "</a></li>").appendTo(dom);

                    li.on("click", function () {
                        if (!(item.enabled == null || item.enabled)) return;
                        if (item.onClick) item.onClick(T, T2);
                        if (S.onItemClicked && (item.enabled == null || item.enabled)) S.onItemClicked(T, T2);
                        T.close();
                    });
                }
            }
        };
    };
    T.items = [];
    if (S.items) {
        for (var i = 0; i < S.items.length; i++) {
            T.items[i] = new menuItem(S.items[i]);
        }
    }
    var close = function (e) {
        if (A.has(e.target).length == 0) T.close();
    };
    T.close = function () {
        if (S.onClose) S.onClose();
        $(document).unbind("keydown", keydown);
        $(document).unbind("mousedown", close);
        if (A) A.remove();
    };
    T.open = function (x, y, obj) {
        if (S.onOpening) S.onOpening(T, null);
        if (obj != null) BoxID = obj;

        A = $("<ul class=\"dropdown-menu\"  id=\"" + S.id + "\"  ></ul>").appendTo(BoxID);
        for (var i = 0; i < T.items.length; i++) {
            T.items[i].addDom(A);
        }
        var height = A.outerHeight(), width = A.outerWidth();
        var bodyHeight = $(document.body).height(), bodyWidth = $(document.body).width();
        if (y + height > bodyHeight) {
            if (bodyHeight - height < 0) {
                var objheight = 0;
                if (obj != null) {
                    objheight = obj.height() + obj.offset().top;
                }
                A.outerHeight(bodyHeight - y - objheight);
                A.css({ overflow: "hidden" });
            } else {
                y = bodyHeight - height;
            }
        }
        if (x + width > bodyWidth) x = bodyWidth - width;
        if (x != null && y != null) A.css({ left: x + "px", top: y + "px", display: "inline" });
        $(document).on("keydown", keydown);
        $(document).on("mousedown", close);
        A.on("mousemove", function (e) {
            var xy = A.offset();
            var scroll_y = e.pageY - xy.top;
            var bl = (scroll_y + 0.0) / A.height();
            //            alert(bl)
            A.scrollTop((height - A.height()) * bl);
            //alert(scroll_y);
        })
        if (S.onOpened) S.onOpened(T, null);
    };
    $M.BaseClass.apply(T, [S]);
    T.attr = function (a, b) {
        if (b != null) S[a] = b;
        if (a == "items" && b != null) {
            T.items.length = 0;
            for (var i = 0; i < S.items.length; i++) {
                T.items[i] = new menuItem(S.items[i]);
            }
        }
        return (S[a]);
    };
};


$M.Control["TreeView"] = function (BoxID, S) {
    var T = this;
    if (S.allowDrop == null) S.allowDrop = false; //是否允许拖动节点
    T.selectedItem = null;
    var dragFlag = false; //start drag
    var insertLine = null;
    var dragData = {
        type: 0, //0 目标节点子节点  1 目标节点之前位置 2目标节点之后位置
        dragItem: null, //拖动节点
        toItem: null//目标节点
    };
    var A = $("<div class=\"tree\" tabindex=\"-1\" ></div>").appendTo(BoxID);
    if (!S.openIco) S.openIco = "fa fa-minus-square-o";
    if (!S.closeIco) S.closeIco = "fa fa-plus-square-o";
    var _setIco = function (item, _ico) {
        if (item.child.items.length > 0) {
            _ico.attr("class", item.child.isOpen ? S.openIco : S.closeIco);
        }
    };
    var _add = function (box, sItem) {
        if (!sItem) return;
        if (sItem.length == null) {
            var item = new box.item(sItem);
            item.parentItem = box.parentItem;
            return item;
        } else {
            for (var i = 0; i < sItem.length; i++) {
                var item = new box.item(sItem[i]);
                item.parentItem = box.parentItem;

            }
        }
    };
    var treeChild = function (BoxID, items) {
        var T2 = this;
        T2.parentItem = null;
        T2.isOpen = true;
        var A = $("<ul></ul>").appendTo(BoxID);
        T2.items = [];
        T2.item = function (S2) {
            var T3 = this;
            T2.items[T2.items.length] = T3;
            //T3.parentItem=
            T3.child = null;
            var line = "<li >";
            line += " <a tabindex=\"-1\"><i/>";
            if (S2.ico) line += "<b class='fa " + S2.ico + "' />";
            line += "<span>" + S2.text + "</span></a>";
            line += "</li>";
            T3.box = $(line).appendTo(A);
            var _ico = T3.box.find("i");
            var _title = T3.box.find("a");
            var _title2 = T3.box.find("a span");
            $M.BaseClass.apply(T3, [S2]);
            T3.addClass = function (S3) { T3.box.addClass(S3); };
            T3.css = function (S3) { T3.box.css(S3); };
            T3.removeClass = function (S3) { T3.box.removeClass(S3); };
            T3.setIco = function () {
                _setIco(T3, _ico);
            };
            T3.addItem = function (S3) {
                if (T3.child == null) {
                    T3.child = new treeChild(T3.box, null);
                    T3.child.parentItem = T3;
                }
                var item = _add(T3.child, S3);
                _setIco(T3, _ico);
                return item;
            };
            T3.clear = function () {
                T3.child.clear();
            };
            T3.addClass(S2["class"]);
            T3.addItem(S2.items);
            T3.after = function (obj) {
                T3.box.after(obj.box);
            };
            T3.before = function (obj) {
                T3.box.before(obj.box);
            };
            _ico.click(function () {
                if (T3.child) {
                    if (T3.child.isOpen) {
                        T3.child.close();
                    } else {
                        T3.child.open();
                    }
                    _setIco(T3, _ico);
                }
            });
            var downFlag = false, moveFlag = false;
            T3.focus = function () {
                if (T.selectedItem) T.selectedItem.removeClass("active");
                T.selectedItem = T3;
                T3.addClass("active");
                if (S.onAfterSelect) { S.onAfterSelect(T, { item: T3 }); };
            };
            _title.mousedown(function (e) {
                $(document.body).on("selectstart", function () { return false; });
                $(document.body).addClass("disableSelect");

                T3.focus();
                downFlag = true; moveFlag = false;
            });
            _title.mouseup(function (e) {
                downFlag = false; moveFlag = false;
                $(document.body).unbind("selectstart");
                $(document.body).removeClass("disableSelect");
            });
            var itemDrop = function (e) {
                if (downFlag && !moveFlag) {
                    insertLine = $("<div class='insertLine' style='display:none;'></div>").appendTo(document.body);
                    _title.moveBox({
                        html: "<span class='label label-info'>" + _title2.html() + "</span>",
                        pageX: e.pageX, pageY: e.pageY,
                        offsetX: 15, offsetY: 15,
                        onStart: function () {
                            dragData.dragItem = T.selectedItem;
                            dragFlag = true;
                        },
                        onEnd: function (sender, e) {
                            dragFlag = false;
                            //sender.remove();
                            insertLine.remove();
                            if (dragData.toItem) {
                                dragData.toItem.removeClass("dragActive");
                                insertLine.css({ "cursor": "auto" });
                                dragData.toItem.css({ "cursor": "auto" });
                            }
                            insertLine = null;
                            //                        console.warn(dragData);

                            if (T3 != dragData.toItem && dragData.toItem != null && T3.box.has(dragData.toItem.box[0]).length == 0) {
                                if (S.onDragDrop) {
                                    var flag = S.onDragDrop(T, dragData);
                                    if (!flag) return;
                                }
                                dragData.dragItem.moveTo(dragData);
                            }
                        }
                    });
                }
                moveFlag = true;
            };
            _title.mousemove(function (e) {
                if (S.allowDrop) itemDrop(e);
            });
            _title.mouseover(function () {
                if (dragFlag) {
                    dragData.type = 0;
                    insertLine.hide();
                    T3.addClass("dragActive");
                    dragData.toItem = T3;
                    if (dragData.toItem == dragData.dragItem || dragData.dragItem.box.has(dragData.toItem.box[0]).length > 0) {
                        insertLine.css({ "cursor": "no-drop" });
                        T3.css({ "cursor": "no-drop" });
                    } else {
                        insertLine.css({ "cursor": "move" });
                        T3.css({ "cursor": "move" });
                    }
                }
            });
            _title.mouseout(function (e) {
                if (dragFlag) {
                    T3.css({ "cursor": "auto" });
                    insertLine.css({ "cursor": "auto" });
                    T3.removeClass("dragActive");
                    var _position = _title.offset();

                    dragData.type = 1;
                    if (e.pageY > _position.top) {
                        dragData.type = 2;
                        _position.top = _position.top + _title.height();
                    }
                    //                    console.warn(e.pageY);
                    insertLine.show();
                    insertLine.css({ left: _position.left + "px", top: _position.top + "px" });
                }
            });
            T3.val = function (text) {
                _title2.html(text);
                S2["text"] = text;
            };
            T3.remove = function () {
                T3.child.remove();
                T3.box.remove();
                T2.items = T2.items.del(T3);
                if (T.selectedItem == T3) T.selectedItem = null;
                T3 = null;
            };
            T3.moveTo = function (data) {
                if (data.toItem == null) {
                    T.root.addItem(T3);
                } else if (T3 != data.toItem && T3.box.has(data.toItem.box[0]).length == 0) {
                    if (dragData.type == 0) {
                        if (T3.parentItem == null) {
                            T.root.items = T.root.items.del(T3);
                        } else {
                            T3.parentItem.child.items = T3.parentItem.child.items.del(T3);
                        }
                        if (data.toItem.child == null) data.toItem.child = new treeChild(box, null);
                        T3.parentItem = data.toItem;
                        data.toItem.child.addItem(T3);
                        data.toItem.setIco();
                    } else if (dragData.type == 1) {
                        data.toItem.before(T3);
                    } else if (dragData.type == 2) {
                        data.toItem.after(T3);
                    }
                }
            };
        };
        T2.clear = function () {
            for (var i = T2.items.length - 1; i > -1; i--) {
                T2.items[i].remove();
            }
        };
        T2.remove = function () {
            T2.clear();
            A.remove();
            T2 = null;
        };
        T2.addItem = function (S2) {
            if (S2 != null && typeof (S2.moveTo) != "undefined") {
                A.append(S2.box);
                T2.items[T2.items.length] = S2;
            } else {
                return _add(T2, S2);
            }
        };
        T2.close = function () {
            A.hide();
            T2.isOpen = false;
        };
        T2.open = function () {
            A.show();
            T2.isOpen = true;
        };
        T2.addItem(items);
        T2.find = function (f) {
            var item = T2.root;
            for (var i = 0; i < T2.items.length; i++) {
                var value = f(T2.items[i]);
                if (value) return T2.items[i];
                else {
                    value = T2.items[i].child.find(f);
                    if (value) return value;
                }
            }
            return null;
        };
    }
    T.root = new treeChild(A, S.items);
    T.find = T.root.find;
    T.container = A;
    $M.BaseClass.apply(T, [S]);

    var keydown = function (e) {
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) {
            if (S.onKeyDown) S.onKeyDown(T, e);
        }
    };
    $(document).on("keydown", keydown);

    A.mouseup(function (e) {
        if (e.which == 3 && S.contextMenuStrip) S.contextMenuStrip.open(e.pageX, e.pageY);
    });
    //    T.css = function (S2) {
    //    alert(1);
    //        A.css(S2);
    //        if (S2.height != "") {
    //            A.slimScroll({
    //                height: A.height(),
    //                alwaysVisible: true,
    //                });
    //        }
    //    };
    if (S.style) A.css(S.style);
};

$M.Control["GridView"] = function(BoxID, S, CID) {
    var T = this;
    var A = null;
    if (CID == null) {
        A = $("<div class=\"M5_GridView\"  tabindex=\"-1\"></div>").appendTo(BoxID);
    } else {
        CID.addClass("M5_GridView");
        CID.attr("tabindex", "-1");
        A = CID;
        if (S.allowmultiple == "true") S.allowMultiple = true;
    }
    var headDiv = $("<div class=\"headDiv\"></div>").appendTo(A);
    if (S.showHeader == 0) headDiv.hide();
    var borderCss = "table-bordered";
    var condensedCss = "";
    var stripedCss = "";
    if (S.striped == 1) stripedCss = "table-striped";
    if (S.border == 0) borderCss = "";
    if (S.condensed == 1) condensedCss = "table-condensed";
    var headTable = $("<table class=\"table  " + borderCss + " " + condensedCss + " " + stripedCss + " dTableR dataTable\"><thead><tr></tr></thead></table>").appendTo(headDiv);
    var bodyTable = $("<div class=\"scrollBody\"></div>").appendTo(A);
    var bodyTable2 = $("<table class=\"table  " + borderCss + " " + condensedCss + " " + stripedCss + " dTableR dataTable\"><tbody></tbody></table>").appendTo(bodyTable);
    var theadTr = headTable.find("thead tr");
    var tbody = A.find("tbody");
    var columnFocusIndex = null;
    var mainCheck = null;
    var tableWidth = 0;
    S.dragLine = (S.dragLine != null && S.dragLine == true) ? true : false;
    T.selectedRows = [];
    T.rows = [];
    S.allowSorting = S.allowSorting ? true : false;
    if (typeof (S.columns) == "string") eval("S.columns=" + S.columns);
    var columnClick = function() {
        var index = $(this).attr("index");
        if (S.allowSorting) {
            if (S.columns[index].name == null) return;
            if (columnFocusIndex != null && columnFocusIndex != index) {
                var index2 = $(columnFocus).attr("index");
                S.columns[columnFocusIndex].dom.attr("class", "sort");
                S.columns[columnFocusIndex].sortDirection = 0;
            }
            if (S.columns[index].sortDirection == null || S.columns[index].sortDirection == 0) S.columns[index].sortDirection = 1;
            else if (S.columns[index].sortDirection == 1) S.columns[index].sortDirection = 2;
            else if (S.columns[index].sortDirection == 2) S.columns[index].sortDirection = 1;
            if (S.columns[index].sortDirection == 1) $(this).attr("class", "sorting_desc");
            else if (S.columns[index].sortDirection == 2) $(this).attr("class", "sorting_asc");
            if (S.onSorting) S.onSorting(T, { name: S.columns[index].name, sortDirection: S.columns[index].sortDirection });
        }
        columnFocusIndex = index;
    };
    T.addColumn = function(columns) {
        for (var i = 0; i < columns.length; i++) {
            tableWidth += columns[i].width;
            var display = (columns[i].visible != false ? "" : "display:none;");
            var sortClass = "";
            if (columns[i].name && S.allowSorting) {
                sortClass = "sort";
                if (columns[i].sortDirection == 1) sortClass = "sorting_desc";
                else if (columns[i].sortDirection == 2) sortClass = "sorting_asc";
            }

            var th = $("<th style='width:" + columns[i].width + "px;" + display + "' class='" + sortClass + "' index=" + i + " >" + columns[i].text + "</th>").appendTo(theadTr);
            th.click(columnClick);
            columns[i].dom = th;
        }
        $("<th />").appendTo(theadTr);
    };
    T.tableRow = function(S2) {
        var T2 = this;
        T2.cells = [];
        T.selectedRow = null;
        var rowIndex = T.rows.length;
        T.rows[T.rows.length] = T2;
        var tableCell = function(S3) {
            var T3 = this;
            var td = S3.dom;
            var value = null;
            if (S.columns[S3.cellIndex].isLink) { S3.dom = $("<a href='#'></a>").appendTo(td); }
            if (S.columns[S3.cellIndex].xtype == "CheckBox") { S3.dom = $("<input type=checkbox >").appendTo(td); }

            T2.cells[T2.cells.length] = T3;
            T3.val = function(text) {
                if (text != null) value = text;
                if (S.columns[S3.cellIndex].xtype == "CheckBox") {
                    S3.dom.prop("checked", text);
                    return S3.dom.is(':checked');
                } else {
                    if (text != null) {
                        if (S.onCellFormatting) {
                            var v = S.onCellFormatting(T, { value: text, columnIndex: S3.cellIndex, rowIndex: S3.rowIndex });
                            if (v != null) text = v;
                        }
                        if (S.columns[S3.cellIndex].format) {
                            text = text.toDate().format(S.columns[S3.cellIndex].format);
                        }
                        S3.dom.html(text);
                    }
                    return (value);
                }
            };
            T3.html = function(html) {
                S3.dom.html(html);
            };
            T3.attr = function(name, value) {
                if (value != null) {
                    S3[name] = value;
                    if (name == "foreColor") S3.dom.css("color", value);
                }
                return (S3[name]);
            };
            T3.val(S3.value);
            T3.focus = function() {
                if (S.columns[S3.cellIndex].xtype && S.columns[S3.cellIndex].xtype != "CheckBox") {
                    var xy = td.offset();
                    S.columns[S3.cellIndex].style = { position: "fixed", zIndex: $M.zIndex++, left: xy.left + "px", top: xy.top + "px", width: td.outerWidth() + "px", height: td.outerHeight() + "px" };
                    S.columns[S3.cellIndex].onBlur = focusBlur;
                    foucsObj.control = A.addControl(S.columns[S3.cellIndex]);
                    foucsObj.cell = T3;
                    foucsObj.td = td;
                    foucsObj.control.val(T3.val());
                    foucsObj.control.focus();
                    if (S.onCellBeginEdit) S.onCellBeginEdit(T, { x: td.attr("_x"), y: td.parent().attr("_y") });

                } else if (S3.xtype && S3.xtype != "CheckBox") {
                    var xy = td.offset();
                    S3.style = { position: "fixed", zIndex: $M.zIndex++, left: xy.left + "px", top: xy.top + "px", width: td.outerWidth() + "px", height: td.outerHeight() + "px" };
                    S3.onBlur = focusBlur;
                    foucsObj.control = A.addControl(S3);
                    foucsObj.cell = T3;
                    foucsObj.td = td;
                    foucsObj.control.val(T3.val());
                    foucsObj.control.focus();
                    if (S.onCellBeginEdit) S.onCellBeginEdit(T, { x: td.attr("_x"), y: td.parent().attr("_y") });
                }
                if (S3.onMouseDown) S.onMouseDown(T, {});
                if (S.columns[S3.cellIndex].isLink) if (S.onRowCommand) S.onRowCommand(T, { commandName: "link", x: td.attr("_x"), y: td.parent().attr("_y") });

            };
            S3.dom.click(T3.focus);
            td.dblclick(function() {
                if (S.onCellMouseDoubleClick) S.onCellMouseDoubleClick(T, { x: td.attr("_x"), y: td.parent().attr("_y") });
            });
            td.click(function() {
                if (S.onCellMouseClick) S.onCellMouseClick(T, { x: td.attr("_x"), y: td.parent().attr("_y"), target: event.target });
            });
        };
        T2.find = function(name) {
            for (var i1 = 0; i1 < S.columns.length; i1++) {
                if (S.columns[i1].name == name) return T2.cells[i1];
            }
            return null;
        };
        var tr = $("<tr _y=" + rowIndex + " ></tr>").appendTo(tbody);

        T2.addClass = function(className) {
            tr.addClass(className);
        };
        if (S.allowMultiple) var th = $("<td  style='width:30px;'><input type=checkbox  class='rowCheck' index=" + T.rows.length + "></td>").appendTo(tr);
        for (var i1 = 0; i1 < S.columns.length; i1++) {
            var display = (S.columns[i1].visible != false ? "" : "display:none;");
            var td = $("<td  _x=\"" + i1 + "\"  _y=" + rowIndex + " style=\"width:" + S.columns[i1].width + "px;" + display + "\"></td>").appendTo(tr);
            var cellData = null;
            if (S2.value instanceof Array) {
                cellData = S2.value[i1];
            } else {
                if (S.columns[i1].name) cellData = S2.value[S.columns[i1].name];
            }
            var cell = new tableCell({ dom: td, value: cellData, cellIndex: i1, rowIndex: rowIndex });
            //T2.cells[T2.cells.length] = cell;
        }
        $("<td />").appendTo(tr);
        var rowCheck = tr.find(".rowCheck");

        T2.remove = function() {
            tr.remove();
            T.selectedRows = T.selectedRows.del(T2);
            T.rows = T.rows.del(T2);
            T2 = null;
        };
        T2.focus = function(obj) {
            //if (T.selectedRow == T2) return;
            //if (T.selectedRow) T.selectedRow.blur();
            T.selectedRows[T.selectedRows.length] = T2;
            tr.addClass("active");
            rowCheck.prop("checked", true);
            if (obj == null && S.onSelectionChanged) S.onSelectionChanged(T, { y: tr.attr("_y") });
        };
        T2.blur = function(obj) {
            tr.removeClass("active");
            rowCheck.prop("checked", false);
            T.selectedRows = T.selectedRows.del(T2);
            if (obj == null && S.onSelectionChanged) S.onSelectionChanged(T, { y: tr.attr("_y") });
        };

        tr.click(function(e) {
            var obj = $(e.target);
            if (obj.attr("class") == "rowCheck") {
                if (rowCheck.prop("checked")) {
                    T2.focus();
                } else {
                    T2.blur();
                }
            } else {
                for (var i = T.selectedRows.length - 1; i >= 0; i--) {
                    T.selectedRows[i].blur();
                }
                if (T.selectedRows.indexOf(T2) == -1) T2.focus();
            }
        });
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
        if (S.dragLine) {
            var move = function(box, x, y) {
                var obj = null;
                var x1 = x + box.width(), y1 = y + box.height();
                var tr = tbody.find("tr");
                for (var i = 0; i < tr.length; i++) {
                    var dom = $(tr[i]);
                    var xy = dom.position();
                    w = dom.width(); h = dom.height();
                    var f = w / 10, f2 = h / 10;
                    var pix1 = y > (xy.top + f2) && y < (xy.top + h - f2);
                    var pix2 = y1 > (xy.top + f2) && y1 < (xy.top + h - f2);
                    //var pix3 = x > (xy.left + f) && y1 > (xy.top + f2) && x < (xy.left + w - f) && y1 < (xy.top + h - f2);
                    //var pix4 = x1 > (xy.left + f) && y > (xy.top + f2) && x1 < (xy.left + w - f) && y < (xy.top + h - f2);
                    if ((pix1 || pix2)) {
                        obj = dom;
                        i = tr.length;
                    }
                }
                if (obj) exchangePos(blankBox, obj);
            };
            tr.moveBox({
                onStart: function(sender, e) {
                    //blankBox = $("<tr></tr>").insertAfter(sender);
                    blankBox = sender.clone(true);
                    blankBox.insertAfter(sender);
                    blankBox.css({ width: sender.outerWidth() + "px", height: sender.outerHeight() + "px", 'opacity': '0.2' });
                },
                onMove: function(sender, e) {
                    move(sender, e.x, e.y);
                },
                onEnd: function(sender, e) {
                    blankBox.before(sender);
                    blankBox.remove();
                    sender.css({ position: "" });
                }
            });
        }
        T2.attr = function(a, b) {
            if (a == "visible") {
                if (b != null) {
                    if (b) { tr.show(); } else { tr.hide(); }
                } else {
                    return tr.is(":visible");
                }
            }
            return (S2[a]);
        };
    };
    T.addRow = function(data) {
        if (data) {

            if (data.length == 0) return;
            if (typeof (data[0]) == "object") {
                for (var i = 0; i < data.length; i++) {
                    T.addRow(data[i]);
                }
            } else {
                var row = new T.tableRow({ value: data });
                return row;
            }
        }
    };
    T.getSelection = function() {
        var sels = [];
        var line = bodyTable.find('.rowCheck'); //.prop("checked", $(this).prop("checked"));
        for (var i = 0; i < line.length; i++) {
            if ($(line[i]).prop("checked")) {
                sels[sels.length] = T.rows[$(line[i]).attr("index")];
            }
        }
        return sels;
    };
    var foucsObj = { control: null, cell: null, td: null };
    var focusBlur = function() {
        headDiv.scrollLeft($(this).scrollLeft());
        if (foucsObj.control != null) {
            foucsObj.cell.val(foucsObj.control.val());
            if (S.onCellEndEdit) S.onCellEndEdit(T, { x: foucsObj.td.attr("_x"), y: foucsObj.td.parent().attr("_y") });
            foucsObj.control.dispose(); foucsObj.control = null; foucsObj.cell = null; foucsObj.td = null;
        }
    };
    bodyTable.on("scroll", focusBlur);

    //    tbody.on("click", function (e) {
    //        foucsObj.dom = $(e.target);
    //        var xy = foucsObj.dom.offset();
    //        var x = foucsObj.dom.attr("_x");
    //        var y = foucsObj.dom.attr("_y");
    //        if (x == null || x > (S.columns.length - 1)) return;
    //        if (S.columns[x].xtype && S.columns[x].xtype != "CheckBox") {
    //            foucsObj.control = A.addControl({
    //                xtype: S.columns[x].xtype,
    //                style: { position: "fixed", zIndex: $M.zIndex++, left: xy.left + "px", top: xy.top + "px", width: foucsObj.dom.outerWidth() + "px", height: foucsObj.dom.outerHeight() + "px" },
    //                onBlur: focusBlur
    //            });
    //            foucsObj.control.val(T.rows[y].cells[x].val());
    //            //foucsObj.control.val(foucsObj.dom.html());
    //            foucsObj.control.focus();
    //        }
    //    });
    T.val = function(data) {
        if (data != null) {
            T.addRow(data);
        }
        var data = [];
        var tr = bodyTable.find("tr");
        var rowsLength = T.rows.length;
        for (var i = 0; i < rowsLength; i++) {
            var td = $(tr[i]).find("td");
            var item = [];
            var cellLength = T.rows[i].cells.length;
            for (var i1 = 0; i1 < cellLength; i1++) {
                item[i1] = T.rows[i].cells[i1].val();
            }
            data[i] = item;
        }
        return data;
    };
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    T.css = function(style) {
        A.css(style);
        if (style && style.height) {
            bodyTable.outerHeight(A.outerHeight() - headTable.outerHeight());
        }
    };
    var selectAll = function(checked) {
        for (var i = 0; i < T.rows.length; i++) {
            if (checked) T.rows[i].focus();
            else { T.rows[i].blur(); }
        }
    };
    T.attr = function(a, b) {
        if (b != null) {
            S[a] = b;
            if (a == "columns") {
                theadTr.html("");
                columnFocus = null;
                tableWidth = 0;
                if (S.allowMultiple) {
                    tableWidth += 30;
                    var th = $("<th style='width:30px;'></th>").appendTo(theadTr);
                    mainCheck = $("<input type=checkbox >").appendTo(th);
                    mainCheck.change(function() {
                        selectAll($(this).prop("checked"));
                        //                        bodyTable.find('.rowCheck').prop("checked", $(this).prop("checked"));

                    });
                }
                T.addColumn(S.columns);
                headTable.css({ width: tableWidth + "px" });
                bodyTable2.css({ width: tableWidth + "px" });

            }
        }
        return S[a];
    };
    T.attr("columns", S.columns);
    T.clear = function() {
        T.rows.length = 0;
        T.selectedRows.length = 0;
        tbody.html("");
        if (mainCheck != null) mainCheck[0].checked = false;
    };

    var keydown = function(e) {
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) {
            if (S.onKeyDown) S.onKeyDown(T, e);
        }
    };
    $(document).on("keydown", keydown);

    A.mouseup(function(e) {
        if (e.which == 3 && S.contextMenuStrip) S.contextMenuStrip.open(e.pageX, e.pageY);
    });
    if (S.data) T.addRow(S.data);
    if (S.style) T.css(S.style);
};
$M.Control["Notify"] = function (BoxID, S) {
    var T = this;
    T.items = [];
    var A = $("<div class=\"sticky-queue\" style='display:none'></div>").appendTo(BoxID);
    A.addClass("top-right");
    var item = function (S2) {
        var T2 = this;
        T.items[T.items.length] = T2;
        if (T.items.length > 0) A.show();
        var box = $("<div class=\"sticky\"  ></div>").appendTo(A);
        box.addClass("border-top-right");
        var close = $("<span class=\"close st-close\" title=\"Close\">×</span>").appendTo(box);
        var con = $("<div class=\"sticky-note\" ></div>").appendTo(box);
        if (S2.color != null) box.addClass("st-" + $M.Control.Constant.colorCss[S2.color]);
        var text = (S2.ico != "" ? "<i class='fa " + S2.ico + "'/> " : "") + S2.text;
        con.html(text);
        T2.close = function () {
            box.animate({ height: '0px', paddingBottom: "0px", paddingTop: "0px", opacity: 0 }, function () {
                box.remove();
                T.items = T.items.del(T2);
                T2 = null;
                if (T.items.length == 0) A.hide();
            });
        };
        if (S2.autoClose != null) setTimeout(T2.close, S2.autoClose);
        close.click(function () {
            T2.close();
            if (S2.onClose) S2.onClose(T2, null);
        });
        T.container = box;
        $M.BaseClass.apply(T2, [S2]);
    };
    T.find = function (name, value) {
        for (var i = 0; i < T.items.length; i++) {
            if (T.items[i].attr(name) == value) {
                return T.items[i];
            }
        }
        return null;
    };
    T.addItem = function (data) {
        if (data) {
            if (data.length == 0) return;
            if (typeof (data[0]) == "object") {
                for (var i = 0; i < data.length; i++) {
                    T.addItem(data[i]);
                }
            } else {
                var row = new item(data);
            }
        }
    };
};

tabIndent = {
    version: '0.1.8',
    config: {
        tab: '\t'
    },
    events: {
        keydown: function (e) {
            var tab = tabIndent.config.tab;
            var tabWidth = tab.length;
            if (e.keyCode === 9) {
                e.preventDefault();
                var currentStart = this.selectionStart,
					currentEnd = this.selectionEnd;
                if (e.shiftKey === false) {
                    // Normal Tab Behaviour
                    if (!tabIndent.isMultiLine(this)) {
                        // Add tab before selection, maintain highlighted text selection
                        this.value = this.value.slice(0, currentStart) + tab + this.value.slice(currentStart);
                        this.selectionStart = currentStart + tabWidth;
                        this.selectionEnd = currentEnd + tabWidth;
                    } else {
                        // Iterating through the startIndices, if the index falls within selectionStart and selectionEnd, indent it there.
                        var startIndices = tabIndent.findStartIndices(this),
							l = startIndices.length,
							newStart = undefined,
							newEnd = undefined,
							affectedRows = 0;

                        while (l--) {
                            var lowerBound = startIndices[l];
                            if (startIndices[l + 1] && currentStart != startIndices[l + 1]) lowerBound = startIndices[l + 1];

                            if (lowerBound >= currentStart && startIndices[l] < currentEnd) {
                                this.value = this.value.slice(0, startIndices[l]) + tab + this.value.slice(startIndices[l]);

                                newStart = startIndices[l];
                                if (!newEnd) newEnd = (startIndices[l + 1] ? startIndices[l + 1] - 1 : 'end');
                                affectedRows++;
                            }
                        }

                        this.selectionStart = newStart;
                        this.selectionEnd = (newEnd !== 'end' ? newEnd + (tabWidth * affectedRows) : this.value.length);
                    }
                } else {
                    // Shift-Tab Behaviour
                    if (!tabIndent.isMultiLine(this)) {
                        if (this.value.substr(currentStart - tabWidth, tabWidth) == tab) {
                            // If there's a tab before the selectionStart, remove it
                            this.value = this.value.substr(0, currentStart - tabWidth) + this.value.substr(currentStart);
                            this.selectionStart = currentStart - tabWidth;
                            this.selectionEnd = currentEnd - tabWidth;
                        } else if (this.value.substr(currentStart - 1, 1) == "\n" && this.value.substr(currentStart, tabWidth) == tab) {
                            // However, if the selection is at the start of the line, and the first character is a tab, remove it
                            this.value = this.value.substring(0, currentStart) + this.value.substr(currentStart + tabWidth);
                            this.selectionStart = currentStart;
                            this.selectionEnd = currentEnd - tabWidth;
                        }
                    } else {
                        // Iterating through the startIndices, if the index falls within selectionStart and selectionEnd, remove an indent from that row
                        var startIndices = tabIndent.findStartIndices(this),
							l = startIndices.length,
							newStart = undefined,
							newEnd = undefined,
							affectedRows = 0;

                        while (l--) {
                            var lowerBound = startIndices[l];
                            if (startIndices[l + 1] && currentStart != startIndices[l + 1]) lowerBound = startIndices[l + 1];

                            if (lowerBound >= currentStart && startIndices[l] < currentEnd) {
                                if (this.value.substr(startIndices[l], tabWidth) == tab) {
                                    // Remove a tab
                                    this.value = this.value.slice(0, startIndices[l]) + this.value.slice(startIndices[l] + tabWidth);
                                    affectedRows++;
                                } else { }	// Do nothing

                                newStart = startIndices[l];
                                if (!newEnd) newEnd = (startIndices[l + 1] ? startIndices[l + 1] - 1 : 'end');
                            }
                        }

                        this.selectionStart = newStart;
                        this.selectionEnd = (newEnd !== 'end' ? newEnd - (affectedRows * tabWidth) : this.value.length);
                    }
                }
            } else if (e.keyCode === 27) {	// Esc
                tabIndent.events.disable(e);
            } else if (e.keyCode === 13 && e.shiftKey === false) {	// Enter
                var self = tabIndent,
					cursorPos = this.selectionStart,
					startIndices = self.findStartIndices(this),
					numStartIndices = startIndices.length,
					startIndex = 0,
					endIndex = 0,
					tabMatch = new RegExp(tab.replace('\t', '\\t').replace(/ /g, '\\s'), 'g'),
					lineText = '',
					indentText = '';
                tabs = null,
                numTabs = 0;

                for (var x = 0; x < numStartIndices; x++) {
                    if (startIndices[x + 1] && (cursorPos >= startIndices[x]) && (cursorPos < startIndices[x + 1])) {
                        startIndex = startIndices[x];
                        endIndex = startIndices[x + 1] - 1;
                        break;
                    } else {
                        startIndex = startIndices[numStartIndices - 1];
                        endIndex = this.value.length;
                    }
                }

                // Find the number of tab characters following this line start index
                lineText = this.value.slice(startIndex, endIndex);
                tabs = lineText.match(tabMatch);
                if (tabs !== null) {
                    e.preventDefault();
                    numTabs = tabs.length;
                    for (x = 0; x < numTabs; x++) {
                        indentText += tab;
                    }

                    this.value = this.value.slice(0, endIndex) + "\n" + indentText + this.value.slice(endIndex);
                    this.selectionStart = endIndex + (tabWidth * numTabs) + 1;
                    this.selectionEnd = endIndex + (tabWidth * numTabs) + 1;
                }
            }
        },
        disable: function (e) {
            var events = this;

            // Temporarily suspend the main tabIndent event
            tabIndent.remove(e.target);
        },
        focus: function () {
            var self = tabIndent,
				el = this,
				delayedRefocus = setTimeout(function () {
				    var classes = (el.getAttribute('class') || '').split(' '),
					contains = classes.indexOf('tabIndent');

				    el.addEventListener('keydown', self.events.keydown);


				    if (contains !== -1) classes.splice(contains, 1);
				    classes.push('tabIndent-rendered');
				    el.setAttribute('class', classes.join(' '));

				    el.removeEventListener('focus', self.events.keydown);
				}, 500);

            // If they were just tabbing through the input, let them continue unimpeded
            el.addEventListener('blur', function b() {
                clearTimeout(delayedRefocus);
                el.removeEventListener('blur', b);
            });
        }
    },
    render: function (el) {
        var self = this;
        el.focus(self.events.focus);
        el.blur(function b(e) {
            self.events.disable(e);
        });
    },
    remove: function (el) {
        if (el.nodeName === 'TEXTAREA') {
            var classes = (el.getAttribute('class') || '').split(' '),
				contains = classes.indexOf('tabIndent-rendered');

            if (contains !== -1) {
                el.removeEventListener('keydown', this.events.keydown);
                el.style.backgroundImage = '';

                classes.splice(contains, 1);
                classes.push('tabIndent');
                el.setAttribute('class', (classes.length > 1 ? classes.join(' ') : classes[0]));
            }
        }
    },
    removeAll: function () {
        // Find all elements with the tabIndent class
        var textareas = document.getElementsByTagName('textarea'),
			t = textareas.length,
			contains = -1,
			classes = [],
			el = undefined;

        while (t--) {
            classes = (textareas[t].getAttribute('class') || '').split(' ');
            contains = classes.indexOf('tabIndent-rendered');

            if (contains !== -1) {
                el = textareas[t];
                this.remove(el);
            }
            contains = -1;
            classes = [];
            el = undefined;
        }
    },
    isMultiLine: function (el) {
        // Extract the selection
        var snippet = el.value.slice(el.selectionStart, el.selectionEnd),
			nlRegex = new RegExp(/\n/);

        if (nlRegex.test(snippet)) return true;
        else return false;
    },
    findStartIndices: function (el) {
        var text = el.value,
			startIndices = [],
			offset = 0;

        while (text.match(/\n/) && text.match(/\n/).length > 0) {
            offset = (startIndices.length > 0 ? startIndices[startIndices.length - 1] : 0);
            var lineEnd = text.search("\n");
            startIndices.push(lineEnd + offset + 1);
            text = text.substring(lineEnd + 1);
        }
        startIndices.unshift(0);

        return startIndices;
    }
};