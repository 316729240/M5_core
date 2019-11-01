$M.batchAddColumn.main = function (S) {
    var win = new $M.dialog({
        title: "批量添加栏目",
        style: { width: "600px" },
        ico: "fa-bars",
        command: "batchAddColumn.addColumn",
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                if (e.returnData != "") $M.alert(e.returnData + "添加失败");
                if (S.reload) S.reload();
                //e.formData.id = e.returnData;
                //if (S.back) S.back(e.formData);
            }
        }
    });
    win.append("<h5>每行填写一个栏目(【栏目名称】[tab]【目录名】[tab]【关键词】)</h5>");
    win.append("<h5>子栏目相对于父栏目使用两个英空格缩进</h5>");
    win.append("<input type='hidden' name='dataTypeId' />").val(S.dataTypeId);
    win.append("<input type='hidden' name='moduleId' />").val(S.moduleId);
    win.append("<input type='hidden' name='classId' />").val(S.classId);
    win.addControl({ xtype: "TextBox", name: "list", rows: 10, multiLine: true });
    win.show();
};