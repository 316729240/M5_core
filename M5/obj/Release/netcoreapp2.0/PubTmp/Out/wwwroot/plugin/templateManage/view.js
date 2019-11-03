$M.templateManage.viewManage = function (S) {
    var tab = mainTab.find("viewManage");
    if (tab) { tab.focus(); return; }
    tab = mainTab.addItem({ text: "视图管理", closeButton: true, name:"viewManage",onClose: function () { $.dataManage = null; } });
    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: 200, text: "分类", ico: "fa-folder-open-o" }, { size: "*"}] });
    var viewList = null, viewClass = null;
    var reloadClass = function () {
        $M.comm("templateManage.readTemplateViewClass", null, function (json) { viewClass.clear(); viewClass.addRow(json); });
    };
    var addClass = function () {
        $M.prompt("分类名称", function (value) {
            $M.comm("templateManage.addViewClass", { className: value }, reloadClass);
        });
    };
    var reload = function () {
        if (viewClass.selectedRows.length == 0) return;
        var id = viewClass.selectedRows[0].cells[0].val();
        $M.comm("templateManage.readTemplateView", { classId: id }, function (json) {
            viewList.clear();
            viewList.addRow(json);
            setEditButtonStatus();
        });
    };
    var delClass = function () {
        if (viewClass.selectedRows.length == 0) return;
        var id = viewClass.selectedRows[0].cells[0].val();
        $M.confirm("您确定要删除所选分类吗？", function () {
            $M.comm("templateManage.delViewClass", { id: id }, reloadClass);
        });
    };
    var menu1 = $(document.body).addControl({ xtype: "Menu", items: [
            { text: "新增", onClick: addClass },
		    { text: "删除", onClick: delClass },
		    { text: "-" },
		    { text: "刷新", onClick: reloadClass }
        ]
    });
    viewClass = frame.items[0].addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, showHeader: 0, border: 0, condensed: 1,
        columns: [{ text: "id", name: "id", visible: false }, { text: "模板方案", name: "text", width: 200}],
        contextMenuStrip: menu1,
        onSelectionChanged: function (sender, e) {
            reload();
        },
        onKeyDown: function (sender, e) {
            if (e.which == 46) delClass();
        }
    });

    var getId = function () {
        var ids = "";
        for (var i = 0; i < viewList.selectedRows.length; i++) {
            if (i > 0) ids += ",";
            ids += viewList.selectedRows[i].cells[0].val();
        }
        return ids;
    };
    var addData = function () {
        if (viewClass.selectedRows.length == 0) return;
        var id = viewClass.selectedRows[0].cells[0].val();
        $M.prompt("视图名称", function (value) {
            $M.app.openFunction("templateManage", "viewEdit", { id: null, classId: id, viewName: value,back:reload });
        });
    };
    var editData = function () {
        if (viewList.selectedRows.length == 0) return;
        $M.app.openFunction("templateManage", "viewEdit", { id: viewList.selectedRows[0].cells[0].val() });
    };
    var delData = function () {
        if (viewList.selectedRows.length == 0) return;
        $M.confirm("您确定要删除所选数据吗？", function () {
            var ids = getId();
            if (ids != "") {
                $M.comm("templateManage.delView", { ids: ids }, reload);
            }
        });
    };
    var toolBar = frame.items[1].addControl({
        xtype: "ToolBar", items: [
        [{ xtype: "InputGroup", name: "searchBoxInput", style: { width: "300px" }, items: [{ xtype: "Button", text: "搜索" }, { name: "searchBox", xtype: "TextBox", text: ""}]}],
        [
            { text: "添加", name: "appendButton", ico: "fa-plus", onClick: addData },
            { text: "修改", name: "editButton", ico: "fa-edit", onClick: editData, enabled: false },
            { text: "删除", name: "delButton", ico: "fa-trash-o", onClick: delData, enabled: false }
       ]]
    });
    var appendButton = toolBar.find("appendButton");
    var editButton = toolBar.find("editButton");
    var delButton = toolBar.find("delButton");
    var setEditButtonStatus = function (flag) {
        editButton.enabled(flag);
        delButton.enabled(flag);
    };
    var searchBoxInput = toolBar.find("searchBoxInput");
    var searchBox = searchBoxInput.find("searchBox");
    searchBox.attr("onKeyUp", function () {
        var keyword = searchBox.val();
        for (var i = 0; i < viewList.rows.length; i++) {
            var title = viewList.rows[i].cells[1].val();
            var pinyin = viewList.rows[i].cells[2].val();
            viewList.rows[i].attr("visible", (pinyin.indexOf(keyword) > -1 || title.indexOf(keyword) > -1));
        }
    });
    viewList = frame.items[1].addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, border: 1, condensed: 1,
        allowMultiple: true,
        columns: [{ text: "id", name: "id", visible: false, width: 100 }, { text: "视图名称", name: "text", width: 300, isLink: true }, { text: "pinyin", name: "u_pinyin", visible: false, width: 100}],
        onRowCommand: function (sender, e) {
            if (e.commandName == "link") {
                var _id = sender.rows[e.y].cells[0].val();
                $M.app.openFunction("templateManage", "viewEdit", { id: _id });
            }
        },
        onKeyDown: function (sender, e) {
            if (e.which == 46) delData();
        },
        onSelectionChanged: function (sender, e) {
            setEditButtonStatus(sender.selectedRows.length > 0);
        }
    });
    $M.comm("templateManage.readTemplateViewClass", null, function (json) { viewClass.addRow(json); viewClass.rows[0].focus(); });
    tab.focus();
};
$M.templateManage.viewEdit = function (S) {
    if ($M._templateManage_edit) {
        //$M._dataManage._moduleId = moduleId;
        //$M._dataManage._classId = classId;
    } else {
        var u_html = null;
        $M._dataManage = mainTab.addItem({ text: "视图编辑", closeButton: true, onClose: function () { $M._dataManage = null; } });
        var frame = $M._dataManage.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: "*" }, { size: 200, text: "模板标签"}] });
        var toolbar = frame.items[0].addControl({
            xtype: "ToolBar", items: [
                    [{ ico: "fa-save", name: "save", text: "保存", onClick: function () {
                        $M.comm("templateManage.saveView", {
                            id: S.id,
                            title: S.viewName,
                            u_html: u_html.val(),
                            u_viewType: S.viewType,
                            classId: S.classId,
                            u_datatypeId: S.datatypeId,
                            u_editboxStatus: u_html.attr("sourceMode") ? 1 : 0
                        }, function (json) {
                            reductionButton.enabled(true);
                            backupButton.enabled(true);
                            $M.alert("保存成功！"); S.id = json; S.back();
                        });
                    }
                    }], [{
                        ico: "fa-clipboard", name: "reduction", text: "还原", enabled: false, onClick: function () {
                            $M.app.call("$M.templateManage.backupViewRestore", {
                                dataId: S.id,
                                title: S.viewName,
                                classId: S.classId,
                                back: function (html) {
                                    u_html.val(html);
                                }
                            });
                        }
                    }, {
                        ico: "fa-copy", name: "backup", text: "备份", enabled: false, onClick: function () {
                                $M.comm("templateManage.backupView", { id: S.id, html: u_html.val() }, function () { $M.alert("备份成功！"); });

                    }
                    }]]
        });
        var reductionButton = toolbar.find("reduction");
        var backupButton = toolbar.find("backup");
        u_html = frame.items[0].addControl({ xtype: "Editor", name: "u_html", codemirror:true,sourceMode: true, style: { height: 300 }, dock: 2 });
        var tree = frame.items[1].addControl({
            xtype: "TreeView",
            onMouseDown: function () { return false; },
            onMouseDoubleClick: function (sender, e) {
                var type = sender.selectedItem.attr("type");
                if (type == 0) {
                    u_html.insertHtml("@" + sender.selectedItem.attr("name"));
                }else if (type == 11) {
                    u_html.insertHtml("@sys." + sender.selectedItem.attr("value"));
                } else if (type == 21) {
                    u_html.insertHtml("@page." + sender.selectedItem.attr("name"));
                } else if (type == 6) {
                    u_html.insertHtml("@view." + sender.selectedItem.parentItem.attr("text") + "." + sender.selectedItem.attr("text") + "()");
                } else if (type == 32) {
                    $M.app.call("$M.templateManage.label", function (html) { u_html.insertHtml(html); });
                } else if (type == 33) {
                    $M.app.call("$M.templateManage.sqlLabel", function (html) { u_html.insertHtml(html); });
                }
            },
            onAfterSelect: function (sender, e) {
                if (!(e.item.attr("loadTag") == null && e.item.child.items.length == 0)) return;
                if (e.item.attr("type") == 4) {
                    $M.comm("templateManage.readTemplateViewClass", null, function (json) {
                        for (var i = 0; i < json.length; i++) {
                            json[i].ico = "fa-folder-o";
                            e.item.addItem(json[i]);
                        }
                        e.item.attr("loadTag", "1")
                    });
                } else if (e.item.attr("type") == 5) {
                    $M.comm("templateManage.readTemplateView", { classId: e.item.attr("id") }, function (json) {
                        for (var i = 0; i < json.length; i++) {
                            json[i].ico = "fa-puzzle-piece";
                            e.item.addItem(json[i]);
                        }
                        e.item.attr("loadTag", "1")
                    });
                }
            }
        });
        tree.root.addItem([{ ico: "fa-folder-o", text: "视图参数", type: 1 },{ ico: "fa-folder-o", text: "网站变量", type: 1 }, { ico: "fa-folder-o", text: "自定义标签", type: 3 }, { ico: "fa-folder-o", text: "自定义视图", type: 4}]);
        tree.root.items[0].addItem({ text: "参数1", name: "parameter1", type: 0 });
        tree.root.items[0].addItem({ text: "参数2", name: "parameter2", type: 0 });
        tree.root.items[0].addItem({ text: "参数3", name: "parameter3", type: 0 });
        $M.comm([
                ["templateManage.readTemplateLable", { dataTypeId: -1}],
                ["templateManage.readView", { id: S.id,viewName:S.viewName}]
                ], function (json) {
                    for (var data in json[0].systemVariables) {
                        tree.root.items[1].addItem({ type: 11, ico: "fa-tag", text: json[0].systemVariables[data][0], value: data });
                    }
                    tree.root.items[2].addItem([{ ico: "fa-cube", text: "自定义数据标签", type: 32 }, { ico: "fa-cube", text: "Sql查询标签", type: 33}]);
                    if (S.id || S.viewName) {
                        u_html.val(json[1].u_html);
                        S.viewName = json[1].title;
                        S.parameterValue = json[1].u_parameterValue;
                        S.viewType = json[1].u_viewType;
                        S.datatypeId = json[1].u_datatypeId;
                        S.classId = json[1].classId;
                        S.id = json[1].id;
                    }
                });
        if (S.id > 0) {
            reductionButton.enabled(true);
            backupButton.enabled(true);
        }
    }
    $M._dataManage.focus();
};
$M.templateManage.insertView = function (S) {
    var n1 = S.viewName.indexOf("("), n2 = S.viewName.indexOf(")");
    var p = S.viewName.substr(n1 + 1, n2 - n1 - 1).split(",");
    var viewName = S.viewName.substr(0, n1);
    for (var i = 0; i < p.length; i++) if (p[i].substr(0, 1) == "\"" || p[i].substr(0, 1) == "'") p[i] = p[i].substr(1, p[i].length - 2);
    var win = new $M.dialog({
        title: "列表编辑",
        style: { width: 600, height: 500 },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                for (var i = 0; i < p.length; i++) {
                    p[i] = "\"" + win.form.find("parameter" + (i + 1)).val() + "\"";
                }
                if (S.back) S.back(viewName + "(" + p + ")");
            }
        }
    });
    win.show();
    win.form.addClass('form-horizontal');
    var addGrid = function (form, json) {
        json.commit = false;
        var gridText = form.append("<textarea name='grid' style='display:none'></textarea>");
        var grid = null;
        var toolBar = form.addControl({
            xtype: "ToolBar",
            items: [
                                [{ xtype: "Button", ico: "fa-pencil", text: "添加", onClick: function () { grid.addRow({}); return false; } },
                                {
                                    xtype: "Button", ico: "fa-times", text: "删除",
                                    onClick: function () {
                                        var count = grid.selectedRows.length;
                                        for (var i = count - 1; i > -1; i--) grid.selectedRows[i].remove();
                                        return false;
                                    }
                                }]
            ]
        });
        if (typeof (json.style) == "string") { eval("json.style=" + json.style); }
        grid = form.addControl(json);
        form.attr("onBeginSubmit", function () {
            var xmlcontent = "";
            var data = grid.val();
            for (var i = 0; i < data.length; i++) {
                var xml = new $M.xml();
                var node = xml.addDom("data");
                for (var i1 = 0; i1 < json.columns.length; i1++) {
                    var f = node.addDom(json.columns[i1].name);
                    f.val(data[i][i1]);
                }
                xmlcontent += xml.getXML();
            }
            gridText.val(xmlcontent);
        });
    };
    var addForm = function (form, json) {
        if (json.item.xtype == "Tab") {
            var items = [];
            if ($.isArray(json.item.item)){
                for (var i = 0; i < json.item.item.length; i++) {
                    items[i] = { text: json.item.item[i].text };
                }
            } else {
                items = [json.item.item];
            }
            var tab = form.addControl({ xtype: "Tab", items: items });
            for (var i = 0; i < items.length; i++) {
                if ($.isArray(json.item.item)) {
                    addForm(tab.items[i], json.item.item[i]);
                } else {
                    addForm(tab.items[i], json.item.item);
                }
            }
        }else if (json.item.xtype == "GridView") {
            json.item.allowMultiple = true;
            addGrid(form, json.item);
        } else {
            if ($.isArray(json.item)) {
                for (var i = 0; i < json.item.length; i++) {
                    if (json.item[i].xtype == "GridView") {
                        addGrid(form, json.item[i]);
                    } else {
                        if (json.item[i].xtype == null) json.item[i].xtype = "TextBox";
                        form.addControl(json.item[i]);
                    }
                }
            } else {
                if (json.item.xtype == null) json.item.xtype = "TextBox";
                form.addControl(json.item);
            }
        }
    };
    $M.comm([
        ["templateManage.readView", { viewName: S.viewName }],
        ["templateManage.readViewForm", { viewName: S.viewName }]], function (json) {
            addForm(win, json[1].config);
            for (var i = 0; i <20; i++) {
                var c = win.form.find("parameter" + (i + 1));
                if (c == null) break;
                    c.val(p[i]);
                    p[i] = "";
            }
        });
};