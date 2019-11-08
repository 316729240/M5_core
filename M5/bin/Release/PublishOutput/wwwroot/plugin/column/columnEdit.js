$M.column.columnEdit = function (S) {
    var grid = null;
    var custom = null;
    var obj = new $M.dialog({
        title: "栏目编辑",
        style: { width: "500px" },
        command: "column.columnEdit",
        onBeginSubmit: function () {
            var xml = new $M.xml();
            var domRoot = xml.addDom("variables");
            var data = grid.val();
            for (var i = 0; i < data.length; i++) {
                var node = domRoot.addDom("item")
                node.val(data[i][1]);
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
    var tab = obj.addControl({ xtype: "Tab", items: [{ text: "基本信息", "class": "form-horizontal" }, { text: "自定义变量" }, { text: "高级设置", "class": "form-horizontal" }, { text: "虚拟站点", "class": "form-horizontal" }] });

    if (S.id) tab.items[0].append("<input type=hidden name=id value='" + S.id + "'>");
    if (S.classId) tab.items[0].append("<input type=hidden name=classId value='" + S.classId + "'>");
    if (S.moduleId) tab.items[0].append("<input type=hidden name=moduleId value='" + S.moduleId + "'>");
    var dirNameBox = { xtype: "TextBox", name: "dirName", labelText: "目录名", labelWidth: 3, vtype: { required: true, isRightfulString: true} };
    if (S.id != null) {
        dirNameBox = { xtype: "InputGroup", labelText: "目录名", labelWidth: 3, items: [{ name: "dirName", xtype: "TextBox", text: "", disabled: true, vtype: { required: true, isRightfulString: true} }, { xtype: "Button", ico: "fa-edit", onClick: function () { editDirName(); return false; } }] };
    }
    var cList = tab.items[0].addControl([
        { xtype: "TextBox", name: "className", labelText: "栏目名称", labelWidth: 3, vtype: { required: true }, onChange: function (sender, e) {
            if (dirName.val() == "") {
                $M.comm("api.getDirName", { name: sender.val() }, function (json) {
                    dirName.val(json);
                });
            }
        }
        },
        dirNameBox,
        { xtype: "TextBox", name: "keyword", labelText: "关键词", labelWidth: 3 },
        { xtype: "TextBox", name: "maxIco", labelText: "栏目图片", labelWidth: 3, xtype: "UploadFileBox" },
        { xtype: "SelectBox", name: "saveDataType", labelText: "栏目类型", labelWidth: 3 },
        { labelText: "栏目模板", labelWidth: 3, items: [{ xtype: "SelectBox", name: "skinId" }, { xtype: "SelectBox", name: "_skinId" }] },
        { labelText: "内容模板", labelWidth: 3, items: [{ xtype: "SelectBox", name: "contentSkinId" }, { xtype: "SelectBox", name: "_contentSkinId" }] },
        //{ xtype: "SelectBox", name: "skinId", labelText: "栏目模板", labelWidth: 3 },
        //{ xtype: "SelectBox", name: "contentSkinId", labelText: "内容模板", labelWidth: 3 },
        { xtype: "TextBox", name: "info", labelText: "说明", labelWidth: 3 }
        ]);

    var dirName = cList[1];
    if (S.id != null) dirName = dirName.find("dirName");
    var editDirName = function () {
        $M.prompt("目录名称", function (text) {
            if (text == dirName.val()) return;
            $M.comm("column.editDirName", { id: S.id, dirName: text }, function (json) {
                dirName.val(text);
                $M.dialog.reset(S.id);
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
        columns: [{ text: "名称", xtype: "TextBox", width: 100 }, { text: "值", xtype: "TextBox", width: 280}]
    });

    tab.items[3].addControl({ xtype: "TextBox", name: "domainName", labelText: "PC域名", labelWidth: 3 });
    tab.items[3].addControl({ xtype: "TextBox", name: "_domainName", labelText: "手机域名", labelWidth: 3 });
    var div=tab.items[2].append("<div class=\"formSep\"></div>");
    var inherit = div.addControl({
        xtype: "CheckBox", name: "inherit", items: [{ text: "继承设置", value: 1 }], value: 1, onChange: function () {
            if (inherit.val() == "1") div.hide();
            else { div.show(); }
        }
    });
    

    /*
    tab.items[2].addControl({ xtype: "CheckBox", name: "saveRemoteImages", labelText: "保存图片", labelWidth: 3, items: [{ text: "", value: 1 }], value: 1 });
    tab.items[2].addControl({ labelText: "缩略图尺寸", labelWidth: 3, items: [{ xtype: "Spinner", css: { width: "60px" }, name: "thumbnailWidth", value: 0 }, { xtype: "Label", text: "*" }, { xtype: "Spinner", name: "thumbnailHeight", value: 0, css: { width: "60px" } }] });
    tab.items[2].addControl({ xtype: "CheckBox", name: "thumbnailForce", labelText: "强制剪裁", labelWidth: 3, items: [{ text: "", value: 1 }] });
    tab.items[2].addControl({ xtype: "CheckBox", name: "watermark", labelText: "加水印", labelWidth: 3, items: [{ text: "", value: 1 }], value: 1 });
    tab.items[2].addControl({ xtype: "CheckBox", name: "titleRepeat", labelText: "允许标题重复", labelWidth: 3, items: [{ text: "", value: 1 }], value: 1 });
    */
    div = tab.items[2].append("<div ></div>");
    div.addControl({ xtype: "CheckBox", name: "saveRemoteImages", labelText: "保存图片", labelWidth: 3, items: [{ text: "", value: 1 }], value: 1 });
    div.addControl({ labelText: "缩略图尺寸", labelWidth: 3, items: [{ xtype: "Spinner", style: { width: "60px" }, name: "thumbnailWidth", value: 0 }, { xtype: "Label", text: "*" }, { xtype: "Spinner", name: "thumbnailHeight", value: 0, style: { width: "60px" } }] });
    div.addControl({ xtype: "CheckBox", name: "thumbnailForce", labelText: "强制剪裁", labelWidth: 3, items: [{ text: "", value: 1 }] });
    div.addControl({ xtype: "CheckBox", name: "watermark", labelText: "加水印", labelWidth: 3, items: [{ text: "", value: 1 }], value: 1 });
    div.addControl({ xtype: "CheckBox", name: "titleRepeat", labelText: "允许标题重复", labelWidth: 3, items: [{ text: "", value: 1 }], value: 1 });
    obj.show();
    tab.items[0].resize();
    var commandList = [
            ["dataTypeList", null],
            ["templateList", { classId: S.id, type: 1, isMobile: 0 }],
            ["templateList", { classId: S.id, type: 1, isMobile: 1 }],
            ["templateList", { classId: S.id, type: 2, isMobile: 0 }],
            ["templateList", { classId: S.id, type: 2, isMobile: 1 }]
        ];
    if (S.id) commandList[commandList.length] = ["column.columnInfo", { id: S.id}];
    $M.comm(commandList, function (json) {
        cList[4].addItem(json[0]);
        cList[4].val(S.dataTypeId);
        if (json[1] != null) json[1].unshift({ text: "PC默认模板", value: 0 });
        else { json[1] = [{ text: "PC默认模板", value: 0}]; }
        if (json[2] != null) json[2].unshift({ text: "手机默认模板", value: 0 });
        else { json[2] = [{ text: "手机默认模板", value: 0 }]; }
        if (json[3] != null) json[3].unshift({ text: "PC默认模板", value: 0 });
        else { json[3] = [{ text: "PC默认模板", value: 0 }]; }
        if (json[4] != null) json[4].unshift({ text: "手机默认模板", value: 0 });
        else { json[4] = [{ text: "手机默认模板", value: 0 }]; }
        cList[5].addItem(json[1]);
        cList[6].addItem(json[2]);
        cList[7].addItem(json[3]);
        cList[8].addItem(json[4]);
        if (S.id) {
            obj.form.val(json[5]);
            if (json[5].inherit == 1) div.hide();
            else { div.show(); }
            var xml = new $M.xml(json[5].custom);
            for (var i = 0; i < xml.documentElement.childNodes.length; i++) {
                grid.addRow([xml.documentElement.childNodes[i].attr("name"), xml.documentElement.childNodes[i].text]);
            }
        }
    });
};
$M.column.columnSorting = function (S, back) {
    var obj = new $M.dialog({
        title: "栏目管理",
        style: { width: "500px",height:"480px" },
        onClose: function (sender, e) {
        }
    });
    obj.show();
    var grid = obj.addControl({ xtype: "GridView", style: { height: "300px" }, dragLine: true, columns: [{ text: "id", name: "id", visible: false }, { text: "名称", name: "text", width: 400}] });
    $M.comm("columnList", { moduleId: 1, classId: 7 }, function (json) { grid.addRow(json); });

};