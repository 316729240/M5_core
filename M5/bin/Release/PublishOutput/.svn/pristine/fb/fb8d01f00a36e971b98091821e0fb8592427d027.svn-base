$M.mainBox = null;
$M.lockList = [];
$M.dialogResult = { "cancel": 0, "ok": 1 };
$M.lock = function (obj, flag, C, mark) {
    if (flag) {
        //var dom = obj.before("<div class='modal-backdrop fade in' style=' z-index:" + ($M.zIndex++) + "'>11</div>");
        //alert(dom)
        var dom = null;
        if (mark) dom = $("<div class='modal-backdrop fade in' style=' z-index:" + ($M.zIndex++) + "'></div>").appendTo($(document.body));
        $M.lockList[$M.lockList.length] = [obj, dom, $M.focusElement, C];
        //$M.mainBox.addClass("GaussianBlur");
    } else {
        var obj = $M.lockList[$M.lockList.length - 1];
        $M.focusElement = obj[2];
        if (obj[1]) obj[1].remove();
        $M.lockList = $M.lockList.del($M.lockList.length - 1);
        //$M.mainBox.removeClass("GaussianBlur");
    }
};
$(document).on("focusin.M4.dialog", function (e) {
    if ($M.lockList.length > 0) {
        var A = $M.lockList[$M.lockList.length - 1];
        if (A[0] != e.target && !A[0].has(e.target).length) {
            A[0].focus();
            if (A[3].loseFocus) A[3].loseFocus();
        }
    }
});
$M.msg = function(message, call) {
    var win = null;
    win = $(document.body).addControl({
        xtype: "Window",
        isModal: false,
        onClose: function() { if (call) call(); }
    });
    win.append("<div class=\"alert\">" + message + "</div>");
    win.show();
    setTimeout(win.remove, 2000);
};
$M.alert = function (message, call, S) {
    var win = null;
    if (S == null) S = [];
    S.type = (S.type == null) ? 5 : S.type;
    var ico = ["", "", "fa-check-circle", "fa-info-circle", "fa-exclamation-triangle", "fa-exclamation-circle"];
    var color = $M.Control.Constant.colorCss[S.type];
    win = $(document.body).addControl({
        xtype: "Window",
        text: S.title ? S.title : "系统信息",
        isModal: true,
        footer: [{ xtype: "Button", text: "确 定", color: 2, size: 2, onClick: function () { win.remove(); return false;} }],
        onClose: function () { if (call) call(); }
    });
    win.append("<div class=\"alert text-" + color + "\"><i class=\"fa " + ico[S.type] + " alert-icon\"></i>" + message + "</div>");
    win.show();
};
$M.progressDialog = function (S) {
    var T = new Object();
    var win = $(document.body).addControl({
        xtype: "Window",
        style: { width: 500 },
        isModal: true,
        onClose: function () {
            //if (S.onClose) S.onClose(win, rdata);
        }
    });
    var label = win.append("<h2></h2>");
    var p1 = win.addControl({ xtype: "ProgressBar" });
    T.show = function () { win.show(); };
    T.remove = function () { win.remove(); };
    T.val = function (value) {
        label.html("<h2>" + (S&&S.text ? S.text : "") + parseInt(value) + "%</h2>");
        p1.val(value);
    };
    return T;
};
$M.dialog = function (S) {
    var T = new Object();
    var win = null, form = null, flag = false;
    if (S == null) S = [];
    var rdata = null;
    var footer = [
            { xtype: "Button", name: "enter", text: "确 定", type: "submit", primary:true, size: 2 },
            { xtype: "Button", text: "取 消", size: 2, onClick: function () { win.dialogResult = $M.dialogResult.cancel; win.remove(); return false; } }
            ];
    if (S.footer) {
        for (var i = 0; i < S.footer.length; i++) {
            for (var o in S.footer[i]) {
                var name = S.footer[i][o];
                footer[i][o] = S.footer[i][o];
            }
        }
    }
    win = $(document.body).addControl({
        xtype: "Window",
        text: S.title ? S.title : "系统信息",
        isModal: true,
        style: S.style,
        url: S.url,
        ico: S.ico,
        command: S.command,
        onBeginSubmit: function () {
            win.wait(true);
            win.footer[0].enabled(false);
            if (S.onBeginSubmit) S.onBeginSubmit();
        },
        onSubmit: function (sender, e) {
            win.wait(false);
            win.footer[0].enabled(true);
            rdata = e;
            win.dialogResult = $M.dialogResult.ok;
            win.remove();
        },
        onSubmitErr: function (json) {
            win.wait(false);
            win.footer[0].enabled(true);
            $M.alert(json.errMsg);
        },
        onClose: function () {
            if (S.onClose) return S.onClose(win, rdata);
        },
        footer: footer
    });
    T.addControl = function (S2) { return win.addControl(S2); };
    T.append = function (S2) { return win.append(S2); };
    T.show = function () { win.show(); win.footer[0].focus(); };
    T.form = win.form;
    T.enter = function () { win.dialogResult = $M.dialogResult.ok; win.remove(); };
    T.cancel = function () { win.dialogResult = $M.dialogResult.cancel; win.remove(); };
    T.attr = function (name, value) { S[name] = value; };
    T.addClass = function (a) {
        win.addClass(a);
    };
    return T;
};

$M.confirm = function (message, call, S) {
    var flag = false;
    var win = null;
    if (S == null) S = [];
    S.type = (S.type == null) ? 0 : S.type;
    var ico = ["fa-question-circle", "", "fa-check-circle", "fa-info-circle", "fa-exclamation-triangle", "fa-exclamation-circle"];
    var color = $M.Control.Constant.colorCss[5];
    var obj = $M.dialog({
        title: S.title ? S.title : "系统信息",
        style: { width: "300px" },
        footer: S.footer,
        onClose: function (sender, e) {
            if (sender.dialogResult == 1 && call) call(flag);

            if (sender.dialogResult == 0 && S.onCancel) S.onCancel();

        }
    });
    obj.append("<div class=\"alert text-" + color + "\" ><i class=\"fa " + ico[S.type] + " alert-icon\"></i>" + message + "</div>");
    obj.show();
};

$M.prompt = function (message, call, S) {
    var win = null, input = null, form = null;
    if (S == null) S = [];
    if (!S.vtype) S.vtype = { required: true }
    var obj = $M.dialog({
        title: S.title ? S.title : "系统信息",
        style: { width: "500px" },
        onClose: function (sender, e) { if (sender.dialogResult == $M.dialogResult.ok && call) call(text.val()); }
    });
    var text = obj.addControl({ xtype: "TextBox", name: "text",value:S.value, labelText: message, vtype: S.vtype });
    obj.show();
    text.focus();
};
$M.dialog.login = function (call) {
    var obj = new $M.dialog({
        title: "登陆",
        command: "api.login",
        style: { width: "500px" },
        onClose: function (sender, e) {
            if (call) call(sender.dialogResult == $M.dialogResult.ok, e);
        }
    });
    obj.addControl([
            { xtype: "TextBox", name: "uname", labelText: "用户名", vtype: { required: true} },
            { xtype: "TextBox", name: "pword", password: true, labelText: "密码", vtype: { required: true} }
            ]);
    obj.show();
};
$M.dialog.deleteModule = function (moduleId, type, back) {
    var p = null;
    var index = 0;
    var ids = null;
    $M.comm("moduleDel", { moduleId: moduleId, classId: type ? moduleId : 7 }, function (json) {
        ids = json;
        var moduleDel = function () {
            $M.comm("moduleDel", { moduleId: moduleId, classId: ids[index][0], type: 1,saveDataType:ids[index][1] }, function (json) {
                index++;
                p.val((index + 0.0) / ids.length * 100);
                if (index == ids.length) {
                    p.remove();
                    back();
                } else {
                    setTimeout(moduleDel, $M.config.operationTimeDelay);
                }
            }, function (json) {
                $M.confirm("发生错误：" + json.errMsg, function () {
                    moduleDel();
                }, {
                    onCancel: function () {
                        p.remove();
                    },
                    footer: [{ text: "重 试"}]
                });
            });
        };
        if (ids.length > 0) {
            p= $M.progressDialog();
            p.show();
            moduleDel();
        }
        else {
            back();
        }
    }, function (json) { $M.alert(json.errMsg, function () { p.remove(); }); });
};
$M.dialog.reset = function (columnId, back) {
    var p = $M.progressDialog();
    p.show();
    var index = 0;
    var ids = null;
    $M.comm("resetColumn", { id: columnId }, function (json) {
        ids = json.split(',');
        var resetContent = function () {
            $M.comm("resetContent", { id: ids[index] }, function (json) {
                index++;
                p.val((index + 0.0) / ids.length * 100);
                if (index == ids.length) {
                    p.remove();
                    back();
                } else {
                    setTimeout(resetContent, $M.config.operationTimeDelay);
                }
            }, function (json) {
                $M.confirm("发生错误：" + json.errMsg, function () {
                    resetContent();
                }, {
                    onCancel: function () {
                        p.remove();
                    },
                    footer: [{ text: "重 试"}]
                });
            });
        };
        resetContent();
    }, function (json) { $M.alert(json.errMsg, function () { p.remove(); }); });
};
$M.dialog.selectColumn = function (moduleId, call) {
    var module, tree;
    var rootId = 7;
    var obj = new $M.dialog({
        title: "选择栏目",
        style: { width: "300px" },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok && call) call({
                moduleId: module.val(),
                id: tree.selectedItem ? tree.selectedItem.attr("id") : rootId,
                text: tree.selectedItem ? tree.selectedItem.attr("text") : module.text()
            });
        }
    });
    var loadColumnList = function (sender) {
        var item = module.selectedItem;
        rootId = item.attr("type") ? module.val() : 7;
        $M.comm("columnList",
            { moduleId: module.val(), classId: rootId },
            function (json) {
                tree.root.clear();
                tree.root.addItem(json);
            });
    };
    module = obj.addControl({
        xtype: "SelectBox",
        onChange: loadColumnList
    });
    tree = obj.addControl({ xtype: "TreeView", style: { height: "200px" },
        onAfterSelect: function (sender, e) {
            if (e.item.attr("loadTag") == null && e.item.child.items.length == 0) {
                $M.comm("columnList", { moduleId: module.val(), classId: e.item.attr("id") }, function (json) {
                    e.item.addItem(json);
                    e.item.attr("loadTag", "1")
                });
            }
        },
        onMouseDoubleClick: function (sender, e) {
            if (sender.selectedItem) obj.enter();
        }
    });
    obj.show();
    $M.comm("moduleList", null, function (json) {
        module.addItem(json);
        if (moduleId != null) {
            module.val(moduleId);
        }
        loadColumnList();
    });
};
//选择用户
//type 0 全部 1 用户组 2用户
$M.dialog.selectUsers = function (S, call) {
    var type = S.type;
    var filter = S.filter;
    var obj = new $M.dialog({
        title: "选择用户",
        style: { width: "300px", height: "400px" },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                var value = [];
                for (var i = 0; i < grid.selectedRows.length; i++) {
                    value[value.length] = [grid.selectedRows[i].cells[0].val(), grid.selectedRows[i].cells[1].val(), grid.selectedRows[i].cells[2].val()];
                }
                if (call) call(value);
            }
        }
    });
    obj.show();
    var grid = obj.addControl({
        xtype: "GridView",
        allowMultiple: true,
        style: { height: "230px" },
        columns: [{ text: "id", width: 20, name: "id", visible: false }, { name: "type", width: 20, visible: false }, { name: "name", text: "名称", width: 150}],
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 2) {
                var type = sender.rows[e.rowIndex].cells[1].val();
                return "<i class='fa " + (type == 2 ? "fa-user" : "fa-users") + "' /> " + e.value;
            }
        }
    });

    $M.comm("getUserList", { type: type }, function (json) {
        for (var i = 0; i < json.length; i++) {
            if (filter==null || filter.indexOf(json[i].id) == -1) {
                grid.addRow(json[i]);
            }
        }
    });

};
$M.dialog.selectTemplate = function (S) {
    var win = new $M.dialog({
        title: "选择模板",
        ico: "fa-history",
        style: { width: "400px" },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                if (grid.selectedRows.length == 0) return;
                if (S.back) S.back(grid.selectedRows[0].cells[0].val());
            }
        }
    });
    var grid = win.addControl({ xtype: "GridView", style: { height: "300px" },
        columns: [{ text: "value", name: "value", visible: false }, { text: "名称", name: "text", width: 260}],
        onCellMouseDoubleClick: function (sender, e) {
            win.enter();
        }
    });
    $M.comm("templateList", { classId: S.classId, type: 2 }, function (json) {
        grid.addRow(json);
        if (S.skinId != null) {
            for (var i = 0; i < grid.rows.length; i++) {
                if (S.skinId == grid.rows[i].cells[0].val()) grid.rows[i].focus();
            }
        }
    });
    win.show();
};