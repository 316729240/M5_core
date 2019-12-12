$M.questionManage.del = function (S) {
    $M.comm("questionManage.delData", { ids: S.ids, moduleId: S.moduleId, classId: S.classId, tag: S.type == 0 ? 0 : 1 }, function (json) {
        S.back();
    });
};
$M.questionManage.addAnswer = function (S) {
    var obj = $M.dialog({
        title:"编辑回答",
        style: { width: "560px" },
        command: "questionManage.editAnswer",
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                S.reload();
            }
        }
    });
    obj.show();
    if (S.answerId) obj.append("<input name=id value='" + S.answerId + "' type='hidden'>");
    var dataId=obj.append("<input name=dataId value='" + S.dataId + "' type='hidden'>");
    var content = obj.addControl({ xtype: "Editor", size: 0, name: "content", style: { height: 200 } });
    if (S.answerId) {
        $M.comm("questionManage.readAnswer", { id: S.answerId }, function (json) {
            content.val(json.content);
            dataId.val(json.dataId);
        });
    }
};
$M.questionManage.edit = function (S) {
    var tab = mainTab.addItem({ text: "发布问题", "class": "form-horizontal", closeButton: true });
    var tab2 = tab.addControl({
        xtype: "Tab", alignment: 1, dock: $M.Control.Constant.dockStyle.fill,
        items: [
        { text: "问题" },
        { text: "回答" }
        ],
        onSelectedIndexChanged: function (sender, e) {
            if (dataId && sender.items[1] == sender.selectedItem) {
                if (dataId.val() == "") {
                    $M.alert("请选保存问题！");
                    sender.items[0].focus();
                }
            }
        }
    });

    tab.focus();
    var form = tab2.items[0].addControl({
        xtype: "Form",
        command: "questionManage.edit",
        onSubmit: function (sender, e) {
            if (S.back) S.back();
            $M.confirm("保存成功，是否关闭？", tab.remove, { footer: [{ text: "是" }, { text: "否" }] });
            S.dataId = e.returnData;
            read(e.returnData);
        }
    });
    var dataId = form.append("<input name=id value='" + (S.dataId?S.dataId:"") + "' type='hidden'>");
    var u_question_answerId = form.append("<input name=u_question_answerId type='hidden'>");
    var classId = form.append("<input name=classId value='" + S.classId + "' type='hidden'>");
    form.addControl({ xtype: "TextBox", name: "title", labelText: "标题", labelWidth: 1, vtype: { required: true } });
    form.addControl({ xtype: "TextBox", name: "u_keyword", labelText: "关键词", labelWidth: 1, vtype: { required: false } });
    form.addControl({ xtype: "Editor", name: "u_content", style: { height: 300 }, labelText: "内容", labelWidth: 1 });
    var footer=form.append("<div class=\"modal-footer\"></div>");
    footer.addControl({ xtype: "Button", color: 2, text: "保存修改" });
    var read = function (dataId) {
        if (dataId) {
            $M.comm("questionManage.read", { id: dataId }, function (json) {
                form.val(json);
            });
        }
    };
    read(S.dataId);

    var toolBar = tab2.items[1].addControl({
        xtype: "ToolBar", class: "note-toolbar",
        items: [
            [
                {
                    text: "新增", ico: "fa-plus", color: 2, onClick: function () { $M.questionManage.addAnswer({ dataId: S.dataId, reload: reload }); }
                },
                { text: "修改", ico: "fa-edit", onClick: function () { edit(); } },
                { text: "删除", ico: "fa-trash-o", onClick: function () { delData(); } },
                { text: "设置为最佳答案", ico: "fa-thumbs-o-up", onClick: function () { setAnswerId(); } }
            ]

        ]
    });

    var getId = function () {
        var ids = "";
        for (var i = 0; i < grid.selectedRows.length; i++) {
            if (i > 0) ids += ",";
            ids += grid.selectedRows[i].cells[0].val();
        }
        return ids;
    };
    
    var setAnswerId = function () {
        if (grid.selectedRows.length == 0) return;
        $M.comm("questionManage.setAnswerId", { id: dataId.val(), answerId: grid.selectedRows[0].cells[0].val() }, function () {
            u_question_answerId.val(grid.selectedRows[0].cells[0].val());
            reload();
        });
    };
    var edit = function () {
        if (grid.selectedRows.length == 0) return;
        $M.questionManage.addAnswer({ answerId: grid.selectedRows[0].cells[0].val(), reload: reload });
    };
    var delData = function () {
        if (grid.selectedRows.length == 0) return;
        var ids = getId();
        if (ids == "") return;
        $M.confirm("您确定要删除所选数据吗？", function () {
            $M.comm("questionManage.delAnswer", { dataId: dataId.val(), ids: ids }, function () {
                reload();
            });
        });
    };
    var grid = tab2.items[1].addControl({
        xtype: "GridView",
        dock: $M.Control.Constant.dockStyle.fill, showHeader: 0, border: 0,
        allowMultiple: true,
        columns: [{ name: "id", width: 30 }, { name: "content", width: 600 }, { name: "createDate" }],
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 0) {
                if (e.value == u_question_answerId.val()) {
                    return "<i class='fa fa-thumbs-o-up' />";
                } else {
                    return "";
                }
            } else if (e.columnIndex == 2) {
                var value = new Date(parseInt(e.value.replace(/\D/igm, "")));
                return value.format("yyyy-MM-dd hh:mm:ss");
            }
        },
        onKeyDown: function (sender, e) {
            if (e.which == 46) delData();
        }
    });
    var reload = function () {
        $M.comm("questionManage.list", { pageNo: 1,dataId:S.dataId }, function (json) {
            grid.clear();
            grid.addRow(json);
        });
    };
    reload();
};