$M.Control["EditorColorBox"] = function (BoxID, S) {
    var T = this;
    var A = null;
    var color = null,backColor=null;
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
        A = $("<div class=\"M5_droppicker dropdown-menu\" ></div>").appendTo(BoxID);
        var html = "<div class=\"M5_Color btn-group\" style='float:left;'>";
        html += "<div class=\"note-palette-title\" >背景颜色</div>";
        html += "<div class=\"note-color-reset\" unselectable=\"on\" _t=0>设为透明</div>";
        for (var i = 0; i < list.length; i++) {
            html += "<div class=\"color-row\">";
            for (var i1 = 0; i1 < list[i].length; i1++) {
                html += "<button type=\"button\" class=\"color-btn\" style=\"background-color:" + list[i][i1] + ";\"  _t=0 val=\"" + list[i][i1] + "\"  />";
            }
            html += "</div>";
        }
        html += "</div>";

        html += "<div class=\"M5_Color btn-group\"  style='float:left;'>";
        html += "<div class=\"note-palette-title\" >前景颜色</div>";
        html += "<div class=\"note-color-reset\" unselectable=on _t=1>设为默认颜色</div>";
        for (var i = 0; i < list.length; i++) {
            html += "<div class=\"color-row\">";
            for (var i1 = 0; i1 < list[i].length; i1++) {
                html += "<button type=\"button\" class=\"color-btn\" style=\"background-color:" + list[i][i1] + ";\" _t=1 val=\"" + list[i][i1] + "\"  />";
            }
            html += "</div>";
        }
        html += "</div>";
        A.html(html);
        A.find("button").click(function () {
            if (S.onChanage) S.onChanage(T, { type: $(this).attr("_t"), color: $(this).attr("val") });
            T.close();
        });
        A.find(".note-color-reset").click(function () {
            if (S.onChanage) S.onChanage(T, { type: $(this).attr("_t"), color:""});
            T.close();
            return false;
        });
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
$M.Control["Editor"] = function (BoxID, S, CID) {
    var HeadHtml = "", EndHtml = "", CssStr = "";
    var Labels = new Array();
    var T = this;
    var A = null;
    if (CID != null) A = CID;
    else { A = $("<div class=\"M5_Editor\"></div>").appendTo(BoxID); }

    var content, content_body, codeBox;
    var colorButton = null;
    var colorMenu = $('body').addControl({
        xtype: "EditorColorBox", onChanage: function (sender, e) {
            if (e.type == 0) {
                colorButton.$("i").css({ color: e.color });
                execComm('ForeColor', e.color);
                colorButton.attr("_color", e.color);
            } else {
                colorButton.$("i").css({ "background-color": e.color });
                execComm('BackColor', e.color);
                colorButton.attr("_backColor", e.color);
            }
        }
    });

    var savePic = function (byteStr) {
        $M.comm("saveCaptureScreen", { byteStr: byteStr }, function (json) {
            var imgHtml = "";
            if (S.onInsertImg) {
                imgHtml=S.onInsertImg(T, json);
            } else {
                imgHtml = "<img src='" + json + "'>";
            }
            T.insertHtml(imgHtml);
        });
    }
    var isCodeMode = false;

    T.replaceValue = function (value) {
        var _replace = function (m, a, b, c, index) {
            return (Labels[index].html);
        }
        value = value.replace(/(<|&lt;)(IMG|INPUT)([^>]*)SystemPic_(\d+)([^>]*)(>|&gt;)/gi, _replace);
        value = value.replace(/©/, "&copy;");
        return (value);
    };
    T.replaceLabel = function (value) {
        var RegexEntries = [
	        /<%[\s\S]*?%>/gi,
	        /(<|&lt;)!-- #Label#[\s\S]*?--(>|&gt;)/gi,
	        /(<|&lt;)!-- #ClassLabel#[\s\S]*?--(>|&gt;)/gi,
	        /(<|&lt;)!-- #PageBar#[\s\S]*?--(>|&gt;)/gi,
	        /<!-- #SqlLabel#[\s\S]*?-->/gi,
	        /<!-- #SearchLabel#[\s\S]*?-->/gi,
	        /<object[\s\S]*?<\/object>/gi,
	        /<!-- PageSpacer -->/gi,
	        /<script[\s\S]*?<\/script>/gi,
	        /(<[^>]*|){[\s\S]*?ViewID=[\s\S]*?}/gi
        ];
        var Img = ['AspIco', 'LabelIco', 'ClassLabelIco', 'PageBarIco', 'SqlIco', 'SearchIco', 'ObjectIco', 'PageSpacerIco', 'ScriptIco', 'ViewIco'];
        var _replace = function (protectedSource) {
            var tag = true;
            if (i == 9) {
                var m = protectedSource.match(new RegExp(/^<[^>]*/gi));
                tag = (m == null);
            }
            if (tag) {
                var Index = Labels.length;
                Labels[Index] = { html: protectedSource, index: i };
                if (Img[i] == "ViewIco") {
                    var getValue = function () {
                        var r, re; // 声明变量。
                        re = new RegExp(/ViewName=([^ ]*)( |})/gi);
                        r = protectedSource.match(re); // 在字符串 s 中查找匹配。
                        if (r != null) {
                            r[0] = r[0].replace("ViewName=", "");
                            r[0] = r[0].replace("}", "");
                            return (r[0]);
                        }
                        return ("");
                    }
                    return ('<input id="SystemPic_' + Index + '" class="ViewBox" type=button value="' + getValue() + '" />');
                } else {
                    return ('<img id="SystemPic_' + Index + '" src="\/static\/img\/blank.gif" class="Label ' + Img[i] + '"\/>');
                }
            } else {
                return (protectedSource);
            }
        }
        for (var i = 0; i < RegexEntries.length; i++) {
            value = value.replace(RegexEntries[i], _replace);
        }
        return (value);
    };
    var insertTag = function (name) {
        var sel = getSelection();
        alert(sel.type);
        return;
        if (name == "") {
            execComm("RemoveFormat");
        } else {
            html = "<" + tagname + ">" + S.html + "</" + tagname + ">";
            T.setDocumentFocus();
            T.insertHtml(html);
        }
    };
    var setCodeMode = function (flag) {
        if (flag == isCodeMode) return;
        for (var i = 0; i < toolbar.controls.length - 1; i++) toolbar.controls[i].enabled(!flag);
        if (flag) {
            ifr.hide();
            codeBox.show();
            textarea.setValue(HeadHtml + T.val() + EndHtml);
            
        } else {
            ifr.show();
            codeBox.hide();

            var html = T.val();
            var r = html.match(new RegExp(/<\/body>[^~]*/gi));
            if (r != null) EndHtml = r[0];
            else { EndHtml = ""; }
            r = html.match(new RegExp(/^[^~]*<body([^>]*)>/gi));
            if (r != null) HeadHtml = r[0];
            else { HeadHtml = ""; }

            html = html.replace(/<\/body>[^*]*/gi, "").replace(/^[^*]*<body([^>]*)>/gi, "");

            html = T.replaceLabel(html);
            r = html.match(new RegExp(/<link[^>]*>/gi));
            CssStr = "";
            if (r != null) {
                for (var n = 0; n < r.length; n++) {
                    CssStr = CssStr + r[n];
                }
            }
            content_body.html(html);
        }
        S.sourceMode = flag;
        isCodeMode = flag;
    };
    var fontSizeMenu = $(document.body).addControl({
        xtype: "Menu", items: [
                    { text: "<span style='font-size:12px;'>特小(12px)</span>", value: 12 },
                    { text: "<span style='font-size:16px;'>小(16px)</span>", value: 16 },
                    { text: "<span style='font-size:18px;'>中(18px)</span>", value: 18 },
                    { text: "<span style='font-size:24px;'>大(24px)</span>", value: 24 },
                    { text: "<span style='font-size:32px;'>特大(32px)</span>", value: 32 },
                    { text: "<span style='font-size:48px;'>特大(48px)</span>", value: 48 }
        ], onItemClicked: function (sender,e) {
            T.getFocus();
            T.insertHtml("<span style='font-size:"+e.attr("value")+"px'>"+documentFocus.html+"</span>");
        }
    });
    var execComm = function (c, p) { content[0].execCommand(c, null, p); };
    var toolButtonClick = function (sender,e) {
        var name = sender.attr("name");
        switch (name) {
            case "bold":
                execComm('Bold');
                break;
            case "italic":
                execComm('Italic');
                break;
            case "underline":
                execComm('Underline');
                break;
            case "justifyLeft":
                execComm('JustifyLeft');
                break;
            case "justifyCenter":
                execComm('JustifyCenter');
                break;
            case "justifyRight":
                execComm('JustifyRight');
                break;
            case "justifyNone":
                execComm('JustifyNone');
                break;
            case "insertOrderedList":
                execComm('InsertOrderedList');
                break;
            case "insertUnorderedList":
                execComm('InsertUnorderedList');
                break;
            case "insertPic":
                $M.app.call("$M.system.insertPic", {
                    back: function (json) {
                        var html = "";
                        for (var i = 0; i < json.length; i++) {

                            if (S.onInsertImg) {
                                html += S.onInsertImg(T, json[i]);
                            } else {
                                html += "<img src='" + json[i] + "'>";
                            }
                        }
                        T.insertHtml(html);
                    }
                });
                break;
            case "captureScreen":
                $M.captureScreen(1, savePic);
                break;
            case "createLink":
                T.getFocus();
                $M.prompt("网址", function (value) {
                    T.focus();
                    execComm("CreateLink", value);
                });
                break;
            case "createLink":
                T.getFocus();
                $M.prompt("网址", function (value) {
                    T.focus();
                    execComm("CreateLink", value);
                });
                break;
            case "colorC":
                execComm('ForeColor', colorButton.attr("_color")); execComm('BackColor', colorButton.attr("_backColor"));
                break;
            case "pageSpacer":
                T.insertHtml(T.replaceLabel("<!-- PageSpacer -->&nbsp; "));
                break;
            case "code":
                setCodeMode(!isCodeMode);
                break;
        }
    };
    var toolButton = {
        font: [{ text: "普通", menu: fontSizeMenu }],
        figure:[
            { ico: "fa-bold", name: "bold", onClick: toolButtonClick },
            { ico: "fa-italic", name: "italic", onClick: toolButtonClick },
            { ico: "fa-underline", name: "underline", onClick: toolButtonClick }
        ],
        align: [
            { ico: "fa-align-left", name: "justifyLeft", onClick: toolButtonClick },
            { ico: "fa-align-center", name: "justifyCenter", onClick: toolButtonClick },
            { ico: "fa-align-right", name: "justifyRight", onClick: toolButtonClick },
            { ico: "fa-align-justify", name: "justifyNone", onClick: toolButtonClick }
        ],
        list: [
            { ico: "fa-list-ol", name: "insertOrderedList", onClick: toolButtonClick },
            { ico: "fa-list-ul", name: "insertUnorderedList", onClick: toolButtonClick }
        ],
        pic: [
            { ico: "fa-file-image-o", name: "insertPic", onClick: toolButtonClick },
            {
                ico: "fa-scissors", tip: "截屏", name: "captureScreen", onClick: toolButtonClick,
                menu: [
                { text: "不隐藏当前窗口", onClick: function () { $M.captureScreen(0, savePic); } },
                { text: "从剪切板中获取", onClick: function () { $M.captureScreen(2, savePic); } }
                ]
            }],
        other:[
            { ico: "fa-link", name: "createLink", onClick: toolButtonClick },
            { name: "colorC", ico: "fa-font", menu: colorMenu, onClick: toolButtonClick }],
        pageBar: [{ ico: "fa-bookmark-o", tip: "分页标签", name: "pageSpacer", onClick: toolButtonClick }],
        code: [{ ico: "fa-code", name: "code", onClick: toolButtonClick }]
    };

    var buttons = [];
    if (S.customItem) {
        for (var i = 0; i < S.customItem.length; i++) {
            buttons[i] = toolButton[S.customItem[i]];
        }
    }else{
        buttons=[
        toolButton.font,
        toolButton.figure,
        toolButton.align,
        toolButton.list,
        toolButton.pic,
        toolButton.other,
        toolButton.pageBar,
        toolButton.code
        ];
    }
    var toolbar = A.addControl({
        xtype: "ToolBar", size: S.size == null ? 1 : S.size, "class": "note-toolbar", items: buttons
    });

    colorButton = toolbar.find("colorC");
    var ifr = $("<iframe style='width:100%;height:100%;' frameborder='0'/>").appendTo(A);
    content = $(ifr[0].contentWindow.document);
    content.attr("designMode", "on");
    content[0].open();
    content[0].write('<html><head><link rel="stylesheet" type="text\/css" href="..\/static\/css\/editor.css" \/>'+CssStr+'<meta http-equiv="Content-Type" content="text\/html; charset=gb2312" \/></head><body ><\/body><\/html>');
    content[0].close();
    content_body = $(content[0].body);
    content.keydown(function (e) {
        if (e.which == 13) {
            execComm('formatblock', '<p>');
        }
    });
    /*
    content.click(function (e) {
        if (e.target.tagName == "IMG") {
            //alert(document.body.createRange)
            var range = content[0].createRange();
            var s = window.getSelection();
            s.collapse(document.body, 0);
            var a = ifr[0].contentWindow.find('img');
            alert(e.target.createRange)

            //alert(window.getSelection)
            //range.moveToElementText(e.target);
            //range.select();
            //e.target.focus();
        }
    });*/
    var codeBox = $("<div/>").appendTo(A);
    var prebox = $("<pre style='width:100%;height:100%;'/>").appendTo(codeBox);
    var textarea = null;
    ace.shortKeyword =S.shortKeyword;
    //if (S.codemirror) {
        textarea = ace.edit(prebox[0]);
        textarea.setOptions({
            enableBasicAutocompletion: true,
            enableSnippets: true,
            enableLiveAutocompletion: true
        });
        //textarea.setTheme("ace/theme/twilight");
        textarea.session.setMode("ace/mode/razor");
        var langTools = ace.require("ace/ext/language_tools");

    //}
    codeBox.hide();
    T.val = function (html) {
        if (html!=null) {
            if (isCodeMode) {
                if (textarea.getValue) {
                    textarea.setValue(html);
                }else{
                    textarea.val(html);
                }
            } else {
                html = T.replaceLabel(html);
                content_body.html(html);
            }
        }
        var html = "";
        if (isCodeMode) {
            if (textarea.getValue) {
                html=textarea.getValue();
            }else{
                html = textarea.val();
            }
        } else {
            html = content_body.html();
            html=T.replaceValue(html);
        }
        return html;
    };
    var documentFocus = null, documentCodeFocus = null;
    T.focus = function () {
        if (isCodeMode) {
            //if (ieTag) {
            textarea.focus();
            //	            setCaret(codeBox, DocumentCodeFocus);
            //}
        } else {
            if (documentFocus != null) {
                var sel = content[0].getSelection();
                sel.removeAllRanges();
                sel.addRange(documentFocus.range);
                //documentFocus = null;
            } else { content.focus(); }
        }
    };

    T.getFocus = function () {
        documentFocus = null;
        documentCodeFocus = null;
        var V = null;
        if (isCodeMode) {
            /*
            if ($.browser.msie) {
                DocumentCodeFocus = getPos(textarea);
            } else {
                //alert(codeBox.getSelection());
                //alert(codeBox.selection);

                //var R=codeBox.getSelection();
                //if(R.getRangeAt.length>0){
                //    DocumentCodeFocus=R.getRangeAt(0);
                //    alert(1);
                //}
            }*/
        } else {
            documentFocus = {};
            if ($.browser.msie) {
                documentFocus.sel = content[0].selection;
                documentFocus.range = documentFocus.sel.createRange();
                if (documentFocus.type == "Control") {
                    if (documentFocus.range) documentFocus.obj = documentFocus.range.item(0);
                    documentFocus.type = "Control";
                    documentFocus.html = documentFocus.obj.outerHTML;
                } else {
                    if (documentFocus.range) {
                        documentFocus.type = "Text";
                        documentFocus.text = documentFocus.range.text;
                        documentFocus.html = documentFocus.range.htmlText;
                    }
                }
            } else {
                //documentFocus.sel = window.getSelection();
                documentFocus.sel = content[0].getSelection();
                documentFocus.range = documentFocus.sel.getRangeAt(0);
                if (documentFocus.sel.getRangeAt.length > 0) {
                    var range = documentFocus.sel.getRangeAt(0);
                    if (range.startContainer.nodeType == 1) {
                        documentFocus.type = "Control";
                        documentFocus.obj = documentFocus.sel.anchorNode.childNodes[documentFocus.sel.anchorOffset];
                        if (documentFocus.obj && documentFocus.obj.tagName) documentFocus.html = getOuterHTML(documentFocus.obj);
                    } else {
                        documentFocus.type = "Html";
                        var range = documentFocus.sel.getRangeAt(0);
                        var container = content[0].createElement('div');
                        container.appendChild(range.cloneContents());
                        documentFocus.text = documentFocus.sel.toString();
                        documentFocus.html = container.innerHTML;
                    }
                } else {
                    documentFocus.type = "Html";
                    var range = documentFocus.sel.getRangeAt(0);
                    var container = content[0].createElement('div');
                    container.appendChild(range.cloneContents());
                    documentFocus.text = documentFocus.sel.toString();
                    documentFocus.html = container.innerHTML;

                }
                //                alert(window.getSelection());
                //documentFocus.sel = ifr.contentWindow.getSelection();
            }
        }
        return V;
    };
    var getOuterHTML = function (obj) {
        var canHaveChildren = function (obj) {
            return !/^(area|base|basefont|col|frame|hr|img|br|input|isindex|link|meta|param)$/.test(obj.tagName.toLowerCase());
        };
        var a = obj.attributes, str = "<" + obj.tagName, i = 0;
        if (a != null) {
            for (; i < a.length; i++)
                if (a[i].specified) str += " " + a[i].name + '="' + a[i].value + '"';
        }
        if (!canHaveChildren(obj)) return str + " /> ";
        return str + ">" + obj.innerHTML + "</" + obj.tagName + ">";
    };
    var insertText = function (html) {
        textarea.insert(html);
        return;
        if (textarea.replaceSelection) {
            textarea.replaceSelection(html);
            return;
        }
        var oValue = textarea[0].value;
        var startIndex = 0;
        var selLength = 0;
        var scrollTop = textarea.scrollTop;
        startIndex = textarea[0].selectionEnd;
        selLength = textarea[0].selectionEnd - textarea[0].selectionStart;
        var fValue = oValue.substring(0, startIndex - selLength);
        var eValue = oValue.substr(startIndex);
        textarea[0].value = fValue + html + eValue;
        textarea[0].focus();
        textarea[0].setSelectionRange(startIndex - selLength, startIndex - selLength + html.length);
        //codeBox[0].scrollTop = scrollTop;
    };
    T.insertHtml = function (html) {
        T.focus();
        if ($.browser.msie) {
        } else {
            if (isCodeMode) {
                insertText(html);
            } else {
                execComm("InsertHtml", html);
            }
        }

    };
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    T.css = function (style) {
        if (style) {
            A.css(style);
            ifr.outerHeight(A.height() - toolbar.height());
            /*if (textarea.getValue) {
                textarea.setSize(A.width(), (A.height() - toolbar.container.outerHeight()));
            }else{
                prebox.css({ "width": A.width() + "px", "height": (A.height() - toolbar.container.outerHeight()) + "px" });
            }*/
            prebox.css({ "width": A.width() + "px", "height": (A.height() - toolbar.container.outerHeight()) + "px" });
            //codeBox.setSize(A.width(), content.height());

            //if (style && style.height) {
            //}
        }
    };
    T.css(S.style);
    T.attr = function (name, value) {
        if (value != null) {
            S[name] = value;
            if (name == "shortKeyword") ace.shortKeyword = value;
        }
        return (S[name]);
    };
    if (S.sourceMode != null) setCodeMode(S.sourceMode);
 
}
