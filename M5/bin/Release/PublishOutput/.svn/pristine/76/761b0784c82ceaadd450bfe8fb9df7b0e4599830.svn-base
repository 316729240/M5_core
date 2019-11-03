$M.templateManage.manage = function (S) {
    var tab = mainTab.find("templateManage");
    if (tab) { tab.focus(); return; }

    var item = null;

    var faId = 0, typeId = 0, dataTypeId = 0, moduleId = 0;
    var loadList = null;
    var listGrid = null;

    tab = mainTab.addItem({ico:(S.u_webFAid == 0 ? "fa-laptop" : "fa-mobile"),text: "模板管理", name: "templateManage", closeButton: true, onClose: function () { } });
    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: 200 }, { size: "*"}] });

    var delData = function () {
        if (listGrid.selectedRows.length == 0) return;
        var _id = listGrid.selectedRows[0].cells[0].val();
        if (_id == 0) return;
        $M.confirm("确定要删除所选模板？", function () {
            var _defaultFlag = listGrid.selectedRows[0].cells[4].val();
            var _classId = listGrid.selectedRows[0].cells[5].val();
            var _datatypeId = listGrid.selectedRows[0].cells[6].val();
            $M.comm("templateManage.delTemplate", { id: _id, classId: _classId, datatypeId: _datatypeId, typeId: typeId }, loadList);
        });
    };
    var locateTemplate = function (_moduleId, _classId, _typeId, _id) {
        var e = null;
        for (var i = 0; i < moduleMenu.items.length; i++) {

            if (moduleMenu.items[i].attr("value") == _moduleId) e = moduleMenu.items[i];
        }
        loadColumnList(e.attr("text"), e.attr("value"), e.attr("datatypeId"), function () {
            for (var i = 0; i < faGrid.rows.length; i++) {
                if (faGrid.rows[i].cells[0].val() == _classId) {
                    if (faGrid.selectedRows.length > 0) faGrid.selectedRows[0].blur();
                    faGrid.rows[i].focus();
                }
            }
            if (typeGrid.selectedRows.length > 0) typeGrid.selectedRows[0].blur(false);
            typeGrid.rows[_typeId].focus(false);
            typeId = _typeId;
            loadList(function () {
                for (var i = 0; i < listGrid.rows.length; i++) {
                    if (listGrid.rows[i].cells[0].val() == _id) {
                        if (listGrid.selectedRows.length > 0) listGrid.selectedRows[0].blur();
                        listGrid.rows[i].focus();
                    }
                }
            });
        });
    };
    var toolBar = frame.items[1].addControl({
        xtype: "ToolBar", items: [[{ ico: "fa-plus", text: "创建模板", enabled: false, name: "createbutton" }, { ico: "fa-trash-o", text: "删除模板", enabled: false, name: "delbutton",
            onClick: delData
        }], [{ ico: "fa-refresh", text: "刷新", onClick: function () { loadList(); } }]
            , [{ text: "定位模板", onClick: function () {
                $M.prompt("网址",
                    function (value) {
                        $M.comm("templateManage.locateTemplate", { url: value, u_webFAid: S.u_webFAid }, function (json) {
                            locateTemplate(json[0], json[1], json[2], json[3]);
                        });
                    },
                    { vtype: { required: true, url: true} });

            }
            }]
            ]
    });
    var createButton = toolBar.find("createbutton"), delButton = toolBar.find("delbutton");
    createButton.attr("onClick", function () {
        $M.prompt("自定义模板名称",
                    function (value) {
                        var faId = faGrid.selectedRows[0].cells[0].val();
                        var typeId = typeGrid.selectedRows[0].cells[0].val();
                        var dataTypeId = typeId == 2 ? faGrid.selectedRows[0].cells[2].val() : 0;
                        $M.templateManage.edit({ title: value, classId: faId, datatypeId: dataTypeId, defaultFlag: 0, typeId: typeId,u_webFAid:S.u_webFAid, back: function () { loadList(); } });
                    },
                    { vtype: { required: true, isRightfulString: true} });
    });
    listGrid = frame.items[1].addControl({
        xtype: "GridView",
        dock: $M.Control.Constant.dockStyle.fill,
        columns: [
                    { text: "id", name: "id", visible: false },
                    { text: "有效范围", name: "title", width: 100 },
                    { text: "名称", name: "title", width: 400, isLink: true },
                    { text: "上次修改时间", name: "updateDate", width: 155 },
                    { text: "", name: "defaultFlag", width: 150, visible: false },
                    { text: "", name: "classId", width: 150, visible: false },
                    { text: "", name: "datatypeId", width: 150, visible: false }
                ],
        onRowCommand: function (sender, e) {
            if (e.commandName == "link") {
                var _id = sender.rows[e.y].cells[0].val();
                var _defaultFlag = sender.rows[e.y].cells[4].val();
                var _classId = sender.rows[e.y].cells[5].val();
                var _datatypeId = sender.rows[e.y].cells[6].val();
                var _title = sender.rows[e.y].cells[2].val();
                $M.app.call("$M.templateManage.edit", { title: _title, id: _id, classId: _classId, datatypeId: _datatypeId, defaultFlag: _defaultFlag, typeId: typeId, u_webFAid: S.u_webFAid, back: function () { loadList(); } });
            }
        },
        onSelectionChanged: function (sender, e) {
            delButton.enabled(sender.rows[e.y].cells[0].val() != 0);
        },
        onKeyDown: function (sender, e) {
            if (e.which == 46) delData();
        }
    });
    loadList = function (back) {
        createButton.enabled(typeId > 0);
        delButton.enabled(false);
        $M.comm("templateManage.templateList", { moduleId: moduleId, faId: faId, typeId: typeId, dataTypeId: dataTypeId, u_webFAid: S.u_webFAid },
                    function (json) {
                        listGrid.clear();
                        for (var i = 0; i < json.length; i++) {
                            var row = new listGrid.tableRow({ value: json[i] });
                            if (json[i][0] == 0) {
                                row.cells[1].attr("foreColor", "#C0C0C0");
                                row.cells[2].attr("foreColor", "#C0C0C0");
                            }
                        }
                        if (back) back();
                    });
    };
    var moduleMenu = $(document.body).addControl({ xtype: "Menu" });
    var leftFrame = frame.items[0].addControl({
        xtype: "Frame", type: "y", dock: 2, items: [{
            size: "*", text: "模板方案", buttons: [{ menu: moduleMenu}]
        }, { size: 150, text: "模板类型"}]
    });
    //leftFrame.items[0].addControl({ xtype: "SelectBox"});
    var faGrid = leftFrame.items[0].addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, showHeader: 0, border: 0, condensed: 1,
        columns: [{ text: "id", name: "id", visible: false }, { text: "模板方案", name: "text", width: 200 }, { text: "", name: "dataTypeId", visible: false}],
        onSelectionChanged: function (sender, e) {
            if (sender.selectedRows.length == 0) return;
            faId = sender.selectedRows[0].cells[0].val();
            dataTypeId = sender.selectedRows[0].cells[2].val();
            loadList();
        }
    });
    var typeGrid = leftFrame.items[1].addControl({ xtype: "GridView", showHeader: 0, dock: $M.Control.Constant.dockStyle.fill, border: 0, condensed: 1, columns: [{ text: "id", visible: false }, { text: "模板类型", width: 200}], data: [[0, "<i class='fa fa-book'/> 频道模板"], [1, "<i class='fa fa-folder-o'/> 栏目模板"], [2, "<i class='fa fa-file-text-o'/> 内容模板"], [3, "<i class='fa fa-file-o'/> 自定义页面模板"]],
        onSelectionChanged: function (sender, e) {
            if (sender.selectedRows.length == 0) return;
            typeId = e.y; loadList();
        }
    });
    typeGrid.rows[0].focus();
    var loadColumnList = function (text, mId, dataTypeId, back) {
        leftFrame.items[0].attr("text", "模板方案(" + text + ")");
        moduleId = mId;
        $M.comm("columnList", { moduleId: moduleId, classId: 7 }, function (json) {
            faGrid.clear();
            faGrid.addRow([0, "<i class='fa fa-cube'/> 全部默认模板", dataTypeId]);
            if (moduleId > 0) faGrid.addRow([moduleId, "<i class='fa fa-cube'/> 模块默认模板", dataTypeId]);
            //                if (!dataTypeId) faGrid.addRow([1, "<i class='fa fa-cube'/> 模块默认模板", dataTypeId]);
            for (var i = 0; i < json.length; i++) {
                json[i].text = "<i class='fa fa-puzzle-piece'/> " + json[i].text;
                faGrid.addRow(json[i]);
            }
            if (back) { back(); }
            else {
                if (faGrid.rows.length > 0) faGrid.rows[0].focus();
            }
        });
    }
    $M.comm("moduleList", null, function (json) {
        var menuItem = [{ text: "全部", value: 0 }, { text: "-"}];
        Array.prototype.push.apply(menuItem, json);
        moduleMenu.attr("items", menuItem);
        if (json.length > 0) loadColumnList(json[0].text, json[0].value, json[0].datatypeId);
    });
    moduleMenu.attr("onItemClicked", function (sender, e) {

        loadColumnList(e.attr("text"), e.attr("value"), e.attr("datatypeId"))
    });

    tab.focus();
};
$M.templateManage.edit = function (S) {
    var item = null;
    if ($M._templateManage_edit) {
        //$M._dataManage._moduleId = moduleId;
        //$M._dataManage._classId = classId;
    } else {
        var editBox = null;
        $M._dataManage = mainTab.addItem({ ico: (S.u_webFAid == 0 ? "fa-laptop" : "fa-mobile"), text: S.title, closeButton: true, onClose: function () { $M._dataManage = null; } });
        var frame = $M._dataManage.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: "*" }, { size: 200, text: "模板标签"}] });
        var toolbar = frame.items[0].addControl({
            xtype: "ToolBar", items: [
                    [{ ico: "fa-save", name: "save", text: "保存", onClick: function () {
                        $M.comm("templateManage.saveTemplate", {
                            id: S.id,
                            title: S.title,
                            u_content: editBox.val(),
                            u_typeId: S.typeId,
                            u_defaultFlag: S.defaultFlag,
                            classId: S.classId,
                            u_datatypeId: S.datatypeId,
                            u_editboxStatus: editBox.attr("sourceMode") ? 1 : 0,
                            u_webFAid: S.u_webFAid
                        }, function (json) {
                            S.id = json;
                            $M.alert("保存成功！");
                            if (S.back) S.back();
                        });
                    }
                    }], [{ ico: "fa-history", name: "reduction", text: "还原", onClick: function () {
                        $M.app.call("$M.templateManage.backupRestore", {
                            classId: S.classId, u_type: S.typeId, u_defaultFlag: S.defaultFlag, title: S.title, u_datatypeId: S.datatypeId, u_webFAid: S.u_webFAid,
                            back: function (html) {
                                editBox.val(html);
                            }
                        });
                    }
                    }, { ico: "fa-copy", name: "backup", text: "备份", onClick: function () {
                        $M.comm("templateManage.backupTemplate", { id: S.id,html:editBox.val() }, function () { $M.alert("备份成功！"); });
                    }
                    }
                    ]
            ]
        });
        var reductionButton = toolbar.find("reduction");
        var backupButton = toolbar.find("backup");
        $M.comm("templateManage.shortKeyword.loadKeyword", null, function (json) {
            editBox.attr("shortKeyword",json);
        });
        editBox = frame.items[0].addControl({ xtype: "Editor", name: "u_content", codemirror: true, sourceMode: true,  dock: 2 });
        var tree = frame.items[1].addControl({
            xtype: "TreeView",
            //onMouseDown: function () { return false; },
            onMouseDoubleClick: function (sender, e) {
                editBox.getFocus();
                var type = sender.selectedItem.attr("type");
                if (type == 11) {
                    editBox.insertHtml("@sys." + sender.selectedItem.attr("value"));
                } else if (type == 21) {
                    editBox.insertHtml("@page." + sender.selectedItem.attr("name"));
                } else if (type == 32) {
                    $M.app.call("$M.templateManage.label", function (html) { editBox.insertHtml(html); });
                } else if (type == 33) {
                    $M.app.call("$M.templateManage.sqlLabel", function (html) { editBox.insertHtml(html); });
                } else if (type == 6) {
                    editBox.insertHtml("@view." + sender.selectedItem.parentItem.attr("text") + "." + sender.selectedItem.attr("text") + "()");
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
        tree.root.addItem([{ ico: "fa-folder-o", text: "页面参数", type: 1 }, { ico: "fa-folder-o", text: "网站变量", type: 1 }, { ico: "fa-folder-o", text: "页面变量", type: 2 }, { ico: "fa-folder-o", text: "自定义标签", type: 3 }, { ico: "fa-folder-o", text: "自定义视图", type: 4}]);
        if (S.typeId == 3) {
            tree.root.items[0].addItem({ text: "参数1", name: "parameter[0]", type: 21 });
            tree.root.items[0].addItem({ text: "参数2", name: "parameter[1]", type: 21 });
            tree.root.items[0].addItem({ text: "参数3", name: "parameter[2]", type: 21 });
        }
        $M.comm([
                ["templateManage.readTemplateLable", { dataTypeId: S.datatypeId}],
                ["templateManage.readTemplate", { id: S.id, classId: S.classId, typeId: S.typeId, datatypeId: S.datatypeId, defaultFlag: S.defaultFlag, title: S.title, u_webFAid: S.u_webFAid}]
                ], function (json) {
                    for (var data in json[0].systemVariables) {
                        tree.root.items[1].addItem({ type: 11, ico: "fa-tag", text: json[0].systemVariables[data][0], value: data });
                    }
                    if (json[0].pageVariables) {
                        for (var i = 0; i < json[0].pageVariables.length; i++) {
                            json[0].pageVariables[i].ico = "fa-tag";
                            json[0].pageVariables[i].type = 21;
                            tree.root.items[2].addItem(json[0].pageVariables[i]);
                        }
                    }
                    tree.root.items[3].addItem([{ ico: "fa-cube", text: "自定义数据标签", type: 32 }, { ico: "fa-cube", text: "Sql查询标签", type: 33}]);
                    if (json[1]) {
                        S.id = json[1].id;
                        editBox.val(json[1].u_content);
                        S.title = json[1].title;
                        S.datatypeId = json[1].u_datatypeId;
                        S.classId = json[1].classId;
                        S.typeId = json[1].u_type;
                        S.defaultFlag = json[1].u_defaultFlag;
                        S.parameterValue = json[1].u_parameterValue;
                    }
                });
        if (S.id > 0) backupButton.enabled(true);
    }
    $M._dataManage.focus();
};
$M.templateManage.label = function (back) {

    var win = new $M.dialog({
        title: "自定义标签",
        ico: "fa-cube",
        style: { width: "700px" }
    });

    var editBox = null;
    var moduleId = null;
    var toolbar = win.addControl({ xtype: "ToolBar", items: [
            [{ xtype: "SelectBox", style: { width: 100 }, name: "moduleId"}],
            [{ xtype: "DialogInput", name: "classId", inputReadOnly: true, style: { width: 120 },
                onButtonClick: function (sender, e) {
                    var back = function (json) {
                        sender.val(json.text);
                        sender.data = json;
                    };
                    $M.dialog.selectColumn(moduleId.val(), back);
                }
            }],
            [{ xtype: "SelectBox", name: "orderby", items: [{ text: "默认排序", value: 0 }, { text: "按最新显示", value: 1 }, { text: "随机显示", value: 2 }, { text: "访问量显示", value: 3}]}],
            [{ xtype: "CheckBox", items: [{ text: "推荐", value: 1 }, { text: "精华", value: 2 }], labelText: "属性：", name: "attribute" }]
            ]
    });
    var box = win.append("<div style='height:280px;'></div>");
    var frame = box.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: 150, text: "模板标签" }, { size: "*"}] });
    var field = frame.items[0].addControl({ xtype: "GridView", allowMultiple: true, dock: $M.Control.Constant.dockStyle.fill, showHeader: 0, border: 0, condensed: 1,
        columns: [{ text: "id", name: "id", visible: false }, { text: "", name: "text", width: 200 }, { text: "", name: "name", visible: false}],
        onMouseDown: function () { return false; },
        onMouseDoubleClick: function (sender, e) {
            var value = "$" + sender.selectedRows[0].cells[2].val();
            editBox.insertHtml(value);
        }
    });
    var line = win.append("<div class='form-inline' style='padding-top:10px;'></div>");
    var recordCount = line.addControl({ xtype: "Spinner", labelText: "调用条数：", labelWidth: 6, style: { width: 80 }, name: "recordCount", value: 0 });
    var pageSize = line.addControl({ xtype: "CheckBox", labelText: "是否分页：", labelWidth: 8, items: [{ text: "是", value: "1"}], name: "pageSize" });

    editBox = frame.items[1].addControl({ xtype: "Editor",  sourceMode: true, dock: 2, size: 0 });
    moduleId = toolbar.find("moduleId");
    var moduleJson = null;
    var loadField = function (datatypeId) {
        $M.comm("templateManage.readDataTypeLable", { datatypeId: datatypeId }, function (json) {
            field.clear();
            field.addRow([{ "name": "index", "text": "索引" }, { "name": "url", "text": "网址" }, { "name": "title", "text": "标题"}]);
            field.addRow(json.fields);
        });
    };
    moduleId.attr("onChange", function (sender) {
        var selectedIndex = sender.attr("selectedIndex");
        loadField(moduleJson[selectedIndex].saveDataType);
    });
    $M.comm("moduleList", null, function (json) {
        moduleId.addItem(json);
        moduleJson = json;
        loadField(moduleJson[0].saveDataType);
    });
    win.attr("onClose", function (sender, e) {
        if (sender.dialogResult == $M.dialogResult.ok) {
            var fields = "";
            for (var i = 0; i < field.selectedRows.length; i++) {
                if (fields != "") fields += ",";
                fields += field.selectedRows[i].cells[2].val();
            }
            var labelId=$M.getId();
            var html = "<!-- #Label#\n";
            html += "labelId=" + labelId+"\n";
            html += "moduleId=" + moduleId.val() + "\n";
            html += "classId=" + ((toolbar.find("classId").data == null) ? "" : toolbar.find("classId").data.id) + "\n";
            html += "orderby=" + toolbar.find("orderby").val() + "\n";
            html += "fields=" + fields + "\n";
            html += "attribute=" + toolbar.find("attribute").val() + "\n";
            html += "datatypeId=" + moduleJson[moduleId.attr("selectedIndex")].saveDataType + "\n";
            html += "recordCount=" + recordCount.val() + "\n";
            html += "pageSize=" + (pageSize.val()=="1"?recordCount.val():"") + "\n";
            html += "<htmlTemplate>";
            html += editBox.val();
            html += "</htmlTemplate>";
            html += "-->";
            if (pageSize.val() > 0) {
                html += "<!-- #PageBar#\n";
                html += "pageBarId=" + labelId + "\n";
                html += "showCount=12\n";
                html += "<HtmlTemplate>{LabelName=FirstPage Value=首页}&nbsp; {LabelName=Prev Value=上一页} &nbsp;{LabelName=PageNumber}&nbsp; {LabelName=Next Value=下一页} {LabelName=EndPage Value=尾页} 共[RecordCount]条记录 [PageNo]/[PageCount]</HtmlTemplate>\n";
                html += "-->";
            }

            back(html);
        }
    });
    //win.addControl({ xtype: "ToolBar", items: [[{ xtype: "SelectBox", style: { width: "110px" } }], [{ xtype: "SelectBox", style: { width: "110px" }}]] });
    win.show();
};

$M.templateManage.sqlLabel = function (back) {
    var win = new $M.dialog({
        title: "Sql标签",
        ico: "fa-cube",
        style: { width: "700px" }
    });

    var editBox = null;
    var form_group = win.append("<div class=\"form-group\"></div>");
    var sqlBox = form_group.addControl({ xtype: "DialogInput", ico: "fa-check", name: "sql",
        onButtonClick: function (sender, e) {
            loadField(sender.val());
        },
        onKeyDown: function (sender, e) {
            if (e.which == 13) {
                loadField(sender.val());
                return false;
            }
        }
    });
    var box = win.append("<div style='height:280px;'></div>");
    var frame = box.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: 150, text: "模板标签" }, { size: "*"}] });
    var field = frame.items[0].addControl({ xtype: "GridView", allowMultiple: true, dock: $M.Control.Constant.dockStyle.fill, showHeader: 0, border: 0, condensed: 1,
        columns: [{ text: "", name: "text", width: 200 }, { text: "", name: "name", visible: false}],
        onMouseDown: function () { return false; },
        onMouseDoubleClick: function (sender, e) {
            var value = "$" + sender.selectedRows[0].cells[1].val();
            editBox.insertHtml(value);
        }
    });
    var line = win.append("<div class='form-inline' style='padding-top:10px;'></div>");
    var recordCount = line.addControl({ xtype: "Spinner", labelText: "调用条数：", labelWidth: 6, style: { width: 80 }, name: "recordCount", value: 0 });
    var pageSize = line.addControl({ xtype: "CheckBox", labelText: "是否分页：", labelWidth: 8, items: [{ text: "是", value: "1"}], name: "pageSize" });
    editBox = frame.items[1].addControl({ xtype: "Editor",sourceMode: true, dock: 2, size: 0 });
    var moduleJson = null;
    var loadField = function (sql) {
        $M.comm("templateManage.readSqlLable", { sql: sql }, function (json) {
            field.clear();
            field.addRow([{ "name": "index", "text": "索引"}]);
            field.addRow(json);
        });
    };
    win.attr("onClose", function (sender, e) {
        if (sender.dialogResult == $M.dialogResult.ok) {
            var fields = "";
            for (var i = 0; i < field.selectedRows.length; i++) {
                if (fields != "") fields += ",";
                fields += field.selectedRows[i].cells[1].val();
            }
            var labelId = $M.getId();
            var html = "<!-- #SqlLabel#\n";
            html += "labelId=" + labelId + "\n";
            html += "sql=" + sqlBox.val() + "\n";
            html += "recordCount=" + recordCount.val() + "\n";
            html += "pageSize=" + (pageSize.val() == "1" ? recordCount.val() : "") + "\n";
            html += "<htmlTemplate>";
            html += editBox.val();
            html += "</htmlTemplate>";
            html += "-->";
            if (pageSize.val() > 0) {
                html += "<!-- #PageBar#\n";
                html += "pageBarId=" + labelId + "\n";
                html += "showCount=12\n";
                html += "<HtmlTemplate>{LabelName=FirstPage Value=首页}&nbsp; {LabelName=Prev Value=上一页} &nbsp;{LabelName=PageNumber}&nbsp; {LabelName=Next Value=下一页} {LabelName=EndPage Value=尾页} 共[RecordCount]条记录 [PageNo]/[PageCount]</HtmlTemplate>\n";
                html += "-->";
            }
            back(html);
        }
    });
    //win.addControl({ xtype: "ToolBar", items: [[{ xtype: "SelectBox", style: { width: "110px" } }], [{ xtype: "SelectBox", style: { width: "110px" }}]] });
    win.show();
};