$M.dataBase.backupData = function () {
    var myDate = new Date();
    $M.prompt("备份名称",
                    function (value) {
                        $M.app.call("$M.dataBase.backup", { name: value });
                    },
                    { vtype: { required: true, isRightfulString: true, value: myDate.format("yyyyMMddhhmmss")} });
};
$M.dataBase.main = function (S) {
    var tab = mainTab.find("dataBase");
    if (tab) { tab.focus(); return; }
    tab = mainTab.addItem({ text: "数据库管理", name: "dataBase", ico: "fa-database", closeButton: true });

    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: 200, text: "表", ico: "fa-table", buttons: [{ text: "备份", onClick: function () { $M.dataBase.backupData(); }, menu: [{ text: "还原数据库", onClick: function () { $M.app.call("$M.dataBase.backupRestore", null); } }] }] }, { size: "*" }] });
    var tableTree = frame.items[0].addControl({
        xtype: "TreeView", dock: 2,
        onMouseDoubleClick: function (sender, e) {
            var tableName = sender.selectedItem.attr("tableName");
            if (tableName) {
                var sqlstr = "select * from [" + tableName + "]";
                sqlScript.val(sqlstr);
                loadPage(sqlScript.val(), 1);
            }
        }
    });
    tableTree.root.addItem([{ text: "自定义表" }, { text: "系统表"}]);
    var sqlScript = frame.items[1].addControl({ xtype: "DialogInput", ico: "fa-exclamation", onButtonClick: function () { loadPage(sqlScript.val(), 1); } });
    var dataList = frame.items[1].addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, border: 1, condensed: 1
    });
    tab.focus();
    $M.comm("dataBase.readTableList", {}, function (json) {
        for (var i = 0; i < json.length; i++) {
            if (json[i][0] == "1") {
                tableTree.root.items[0].addItem({ text: json[i][1], tableName: json[i][2] });
            } else {
                tableTree.root.items[1].addItem({ text: json[i][1], tableName: json[i][2] });
            }
        }
    });
    var pageBar = frame.items[1].addControl({
        xtype: "PageBar",
        onPageChanged: function (sender, e) {
            loadPage(sqlScript.val(), e.pageNo);
        }
    });
    var loadPage = function (sql, pageNo) {
        $M.comm("dataBase.dataList", {
            sql: sql,
            pageNo: pageNo
        },
            function (json) {
                dataList.clear();
                dataList.attr("columns", json[0]);
                dataList.addRow(json[1].data);
                pageBar.attr("pageSize", json[1].pageSize);
                pageBar.attr("recordCount", json[1].recordCount);
                pageBar.attr("pageNo", json[1].pageNo);
                frame.resize();
            });
    };
    //loadPage("select u_content from article", 1);
};
$M.dataBase.tableStructure = function (S) {
    var tab = mainTab.find("tableStructure");
    if (tab) { tab.focus(); return; }
    tab = mainTab.addItem({ text: "表结构编辑器", name: "tableStructure", ico: "fa-database", closeButton: true });
    var toolBar=tab.addControl({ xtype: "ToolBar", items: [{ text: "保存修改", name:"save" }] });
    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: "*" }, { size: 330 }] });
    var fieldList=frame.items[0].addControl({xtype:"GridView",
        columns: [
            { text: "助记名", name: "text", xtype: "TextBox", width: 150 },
            { text: "字段名称", name: "name", xtype: "TextBox", width: 150 },
            {
                text: "类型",name:"type", xtype: "SelectBox", width: 150, items: [
                    { text: "短文本", value: "string" },
                    { text: "长文本", value: "text" },
                    { text: "整数", value: "int" },
                    { text: "小数", value: "float" },
                    { text: "日期时间", value: "dateTime" }
                ]
            },
            { name: "size", visible: false },
            { name: "default", visible: false },
            { name: "required", visible: false },
            { name: "validate", visible: false },
            { name: "validateText", visible: false },
            { name: "control", visible: false }
        ],
        onCellEndEdit: function (sender,e) {
            if (e.x == 0) {
                var value = sender.rows[e.y].cells[0].val();
                var value2 = sender.rows[e.y].cells[1].val();
                if (value != "" && value2 == "") {
                    $M.comm("api.getDirName", { name: value }, function (json) {
                        sender.rows[e.y].cells[1].val("u_" + json);
                    });
                    if (e.y == sender.rows.length - 1) fieldList.addRow(["", "", "string",255]);
                }
            } else if (e.x == 2) {
                var value = fieldList.selectedRows[0].cells[e.x].val();
                attrList.rows[0].attr("visible",value=="string");
            }
        }
    });
    var attrTab = frame.items[1].addControl({ xtype: "Tab", items: [{ text: "常规" }, { text: "控件" }] });
    var attrList = attrTab.items[0].addControl({
        xtype: "PropertyGrid",
        showHeader: true
    });
    var cList = attrTab.items[1].addControl({
        xtype: "PropertyGrid",
        showHeader: true
    });
    attrList.addRow([
        {text:"字段大小",name:"size"},
        {text:"默认值",name:"default"},
        { name: "required", text: "必填", xtype: "SelectBox", items: ["是", "否"], value: "是" },
        { name: "validate",text:"验证规则", xtype: "SelectBox", value: "no", items: [{ text: "不验证", value: "no" }, { text: "手机", value: "isMobile" }, { text: "邮箱", value: "isMail" }] },
        { name: "validateText", text: "验证文本" }
    ]);
    cList.addRow([
        {
            text: "输入控件", name: "control", xtype: "SelectBox", items: [
              { text: "文本框", value: "TextBox" },
              { text: "选择框", value: "SelectBox" },
              { text: "日期框", value: "DateBox" },
              { text: "复选框", value: "SelectBox" }
            ]
        }
    ]);

    attrList.attr("onCellEndEdit", function (sender, e) {
        if (fieldList.selectedRows.length == 0)return;
            var name = sender.rows[e.y].cells[0].val();
            fieldList.selectedRows[0].find(name).val(sender.rows[e.y].val());
    });
    cList.attr("onCellEndEdit", function (sender, e) {
        if (fieldList.selectedRows.length == 0)return;
            var name = sender.rows[e.y].cells[0].val();
            fieldList.selectedRows[0].find(name).val(sender.rows[e.y].val());
    });
    fieldList.attr("onSelectionChanged", function (sender, e) {
        if (fieldList.selectedRows.length == 0) return;
        var columns = sender.attr("columns");
        for (var i = 0; i < columns.length; i++) {
            var obj=attrList.find(columns[i].name);
            if (obj) {
                var value = sender.selectedRows[0].cells[i].val();
                if (value == null) value = "";
                obj.val(value);
            } else {
                obj = cList.find(columns[i].name);
                if (obj) {
                    var value = sender.selectedRows[0].cells[i].val();
                    if (value == null) value = "";
                    obj.val(value);
                }
            }
        }
        var value = fieldList.selectedRows[0].cells[2].val();
        attrList.rows[0].attr("visible", value == "string");
    });
    fieldList.attr("onKeyDown", function (sender, e) {
        if (e.which == 46) {
            if (fieldList.selectedRows.length > 0) {
                fieldList.selectedRows[0].attr("visible", false);
            }
        }
    });
    toolBar.find("save").attr("onClick", function () {
        var data= [];
        var columns = fieldList.attr("columns");
        for (var i = 0; i < fieldList.rows.length; i++)
        {
            if (fieldList.rows[i].cells[0].val()!="") {
                var json = {};
                for (var i1 = 0; i1 < columns.length; i1++) {
                    json[columns[i1].name] = fieldList.rows[i].cells[i1].val();
                }
                if (!fieldList.rows[i].attr("visible")) json["del"] = true;
                data[data.length] = json;
            }
        }
        $M.comm("dataBase.saveDatatype", {id:S.id,tableName:S.tableName,datatypeName:S.datatypeName, data: JSON.stringify(data) }, function () {
        });
    });
    if(S.id){
        $M.comm("dataBase.readDatatype", { id: S.id }, function (data) {
            var ts = $.parseJSON(data.tableStructure);
            fieldList.addRow(ts);
            fieldList.addRow(["", "", "string", 255]);

        });
    } else {
        fieldList.addRow(["", "", "string", 255]);
    }
    tab.focus();
};