$M.dataReplace.main = function (S) {
    var tab = $(document.body).addControl({
        xtype: "Window",
        text: "数据替换",
        isModal: true,
        style: { width: "700px" },
        ico: "fa-cube"
    });
    var toolbar = tab.append("<div></div>");
    var input = toolbar.addControl([{ xtype: "SelectBox", name: "dataTypeId", labelText: "数据", labelWidth: 4, width: 4, onChange: function () {
        loadField(dataTypeId.val());
    }
    },
        { xtype: "SelectBox", name: "moduleId", labelText: "模块", labelWidth: 4, width: 4,
            onChange: function (sender, e) {
                classId.val("");
                classId.data = "";
            }
        },
        { xtype: "DialogInput", name: "classId", labelText: "栏目", labelWidth: 4, inputReadOnly: true, width: 4, style: { width: 120 },
            onButtonClick: function (sender, e) {
                var back = function (json) {
                    sender.val(json.text);
                    sender.data = json;
                };
                $M.dialog.selectColumn(moduleId.val(), back);
            }
        }]);
    if (S.dataTypeId) toolbar.hide();
    var dataTypeId = input[0];
    var moduleId = input[1];
    var classId = input[2];

    tab.append("<div style='clear:both;'></div>");
    var field = tab.addControl({ xtype: "GridView", allowMultiple: true, dock: $M.Control.Constant.dockStyle.fill, border: 0, condensed: 1, style: { height: "300px" },
        columns: [{ text: "name", name: "name", visible: false }, { text: "字段名", name: "text", width: 200}]
    });
    tab.append("<div style='clear:both;'></div>");
    var keyword = tab.addControl([
        { labelText: "查找内容", xtype: "TextBox", name: "content1", multiLine: true, rows: 3, width: 6, onChange: function () { replaceButton.enabled(false); } },
        { labelText: "替换内容", xtype: "TextBox", name: "content2", multiLine: true, rows: 3, width: 6 }
        ]);
    var foot = tab.append('<p class="text-right btn-toolbar"></p>');
    foot.addControl({ xtype: "Button", color: 2, text: "查找并替换",
        onClick: function () {
            findReplace();
            return false;
        }
    });
    var loadField = function (dataTypeId) {
        $M.comm("dataReplace.fieldList", { id: dataTypeId }, function (json) {
            field.clear();
            for (var i = 0; i < json.length; i++) {
                if (json[i].type == "String" || json[i].type == "Remark") field.addRow(json[i]);
            }
        });
    };
    if (S.dataTypeId == null) {
        $M.comm([["dataTypeList", null], ["moduleList", null]], function (json) {
            dataTypeId.addItem(json[0]);
            moduleId.addItem({ text: "全部", value: 0 });
            moduleId.addItem(json[1]);
            loadField(dataTypeId.val());
        });
    } else {
        loadField(S.dataTypeId);
    }
    //alert(moduleId.val());
    var findReplace = function () {

        var fieldList = "";
        for (var i = 0; i < field.selectedRows.length; i++) {
            if (fieldList != "") fieldList += ",";
            fieldList += field.selectedRows[i].cells[0].val();
        }

        if (fieldList == "" || keyword[0].val() == "") {
            $M.alert("没填写要替换的字段或关键词");
            return;
        }

        tab.comm("dataReplace.findReplace", {
            keyword: keyword[0].val(),
            dataTypeId: S.dataTypeId == null ? dataTypeId.val() : S.dataTypeId,
            moduleId: S.moduleId == null ? moduleId.val() : S.moduleId,
            classId: S.classId == null ? (((classId.data == null) ? "" : classId.data.id)) : S.classId,
            fieldList: fieldList
        },
            function (json) {

                var p = null;
                var index = 0;
                var _replace = function () {
                    if (index == json.length) {
                        p.remove();
                        p = null;
                    } else {
                        var ids = "";
                        for (var i = 0; i < 10; i++) {
                            if (index < json.length) {
                                if (ids != "") ids += ",";
                                ids += json[index];
                                p.val((index + 0.0) / json.length * 100);
                                index++;
                            }
                        }
                        $M.comm("dataReplace.replace", {
                            dataTypeId: S.dataTypeId == null ? dataTypeId.val() : S.dataTypeId,
                            ids: ids,
                            keyword1: keyword[0].val(),
                            keyword2: keyword[1].val(),
                            fieldList: fieldList
                        }, function () {
                            setTimeout(_replace, $M.config.operationTimeDelay);
                        });
                    }
                }
                if (json.length == 0) {
                    $M.alert("没有找到符合条件的数据");
                } else {
                    $M.confirm("共找到" + json.length + "条符合条件数据，您确定要执行替换操作吗？", function () {
                        p = $M.progressDialog();
                        p.show();
                        _replace();
                    });
                }
            });
    };
    tab.show();
};