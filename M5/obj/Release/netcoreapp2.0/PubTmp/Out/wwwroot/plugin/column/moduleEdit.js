$M.column.moduleEdit = function (S) {
    var grid = null;
    var custom = null;
    var obj = new $M.dialog({
        title: "模块编辑",
        style: { width: "500px" },
        command: "column.moduleEdit",
        onBeginSubmit: function () {
            var xml = new $M.xml();
            var domRoot = xml.addDom("variables");
            var data = grid.val();
            for (var i = 0; i < data.length; i++) {
                var node = domRoot.addDom("item")
                node.attr("name", data[i][0]);
            }
            custom.val(xml.getXML());
        },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                e.formData.id = e.returnData;
                if (S.back) S.back(e.formData);
            } else {
                if (S.back) S.back(null);
            }
        }
    });
    var tab = obj.addControl({ xtype: "Tab", items: [{ text: "基本信息", "class": "form-horizontal" }, { text: "自定义变量" }, { text: "高级设置", "class": "form-horizontal" }, { text: "虚拟站点", "class": "form-horizontal"}] });

    if (S.id) tab.items[0].append("<input type=hidden name=id value='" + S.id + "'>");
    if (S.classId) tab.items[0].append("<input type=hidden name=classId value='" + S.classId + "'>");
    if (S.moduleId) tab.items[0].append("<input type=hidden name=moduleId value='" + S.moduleId + "'>");
    var dirNameBox = { xtype: "TextBox", name: "dirName", labelText: "目录名", labelWidth: 3, vtype: { required: true, isRightfulString: true} };
    if (S.id != null) {
        dirNameBox = { xtype: "InputGroup", labelText: "目录名", labelWidth: 3, items: [{ name: "dirName", xtype: "TextBox", text: "", disabled: true, vtype: { required: true, isRightfulString: true} }, { xtype: "Button", ico: "fa-edit", onClick: function () { editDirName(); return false; } }] };
    }
    var cList = tab.items[0].addControl([
        { xtype: "TextBox", name: "moduleName", labelText: "模块名称", labelWidth: 3, vtype: { required: true }, onChange: function (sender, e) {
            if (dirName.val() == "") {
                $M.comm("api.getDirName", { name: sender.val() }, function (json) {
                    dirName.val(json);
                });
            }
        } 
        },
        dirNameBox,
        { xtype: "RadioBox", name: "type", labelText: "类型", labelWidth: 3, items: [{ text: "虚拟目录", value: 0 }, { text: "真实目录", value: 1}], value: 0 },
        { xtype: "SelectBox", name: "saveDataType", labelText: "模块类型", labelWidth: 3 },
        { xtype: "TextBox", name: "keyword", labelText: "关键词", labelWidth: 3 },
        { xtype: "TextBox", name: "info", labelText: "说明", labelWidth: 3 }
        ]);
    var dirName = cList[1];
    var type = cList[2];
    if (S.id != null) dirName = dirName.find("dirName");
    var editDirName = function () {
        $M.prompt("目录名称", function (text) {
            if (text == dirName.val()) return;
            $M.comm("column.editModuleDirName", { id: S.id, dirName: text }, function (json) {
                dirName.val(text);
                if (S.type) $M.dialog.reset(S.id);
            });
        }, { value: dirName.val(), vtype: { required: true, isRightfulString: true} });
    };
    var addData = function () {
        $M.prompt("名称", function (text) { grid.addRow([text, ""]); });
        return false;
    };
    var delData = function () {
        var count = grid.selectedRows.length;
        for (var i = count - 1; i > -1; i--) grid.selectedRows[i].remove();
        return false;
    };
    var toolBar = tab.items[1].addControl({
        xtype: "ToolBar",
        items: [
	        [{ xtype: "Button", ico: "fa-pencil", text: "添加", onClick: addData }, { xtype: "Button", ico: "fa-times", text: "删除", onClick: delData}]
            ]
    });
    custom = tab.items[1].append("<textarea  name=\"custom\" style=\"display:none\"></textarea>");
    grid = tab.items[1].addControl({
        xtype: "GridView",
        allowMultiple: true,
        style: { height: "260px" },
        columns: [{ text: "名称", xtype: "TextBox", width: 200 }]
    });

    tab.items[3].addControl({ xtype: "TextBox", name: "domainName", labelText: "PC域名", labelWidth: 3 });
    tab.items[3].addControl({ xtype: "TextBox", name: "_domainName", labelText: "手机域名", labelWidth: 3 });
    //tab.items[2].append("<div class=\"formSep\"></div>");
    //tab.items[2].addControl({ xtype: "CheckBox", name: "inherit", items: [{ text: "继承设置", value: 1}],value:1 });
    //tab.items[2].append("<div class=\"formSep\"></div>");
    tab.items[2].addControl({ xtype: "CheckBox", name: "saveRemoteImages", labelText: "保存图片", labelWidth: 3, items: [{ text: "", value: 1}], value: 1 });
    tab.items[2].addControl({ labelText: "缩略图尺寸", labelWidth: 3, items: [{ xtype: "Spinner", css: { width: "60px" }, name: "thumbnailWidth", value: 0 }, { xtype: "Label", text: "*" }, { xtype: "Spinner", name: "thumbnailHeight", value: 0, css: { width: "60px"}}] });
    tab.items[2].addControl({ xtype: "CheckBox", name: "thumbnailForce", labelText: "强制剪裁", labelWidth: 3, items: [{ text: "", value: 1}] });
    tab.items[2].addControl({ xtype: "CheckBox", name: "watermark", labelText: "加水印", labelWidth: 3, items: [{ text: "", value: 1 }], value: 1 });
    tab.items[2].addControl({ xtype: "CheckBox", name: "titleRepeat", labelText: "允许标题重复", labelWidth: 3, items: [{ text: "", value: 1 }], value: 1 });
    obj.show();
    var commandList = [
            ["dataTypeList", null]
        ];
    if (S.id) commandList[commandList.length] = ["column.moduleInfo", { id: S.id}];
    $M.comm(commandList, function (json) {
        cList[3].addItem(json[0]);
        cList[3].val(S.dataTypeId);
        if (S.id) {
            obj.form.val(json[1]);
            S.type = json[1].type;
            type.val(S.type ? 1 : 0);
            var xml = new $M.xml(json[1].custom);
            for (var i = 0; i < xml.documentElement.childNodes.length; i++) {
                grid.addRow([xml.documentElement.childNodes[i].attr("name")]);
            }
            //temp.val(0);
        }
    });
};
