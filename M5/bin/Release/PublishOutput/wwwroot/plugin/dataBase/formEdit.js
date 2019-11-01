$M.dataBase.formEdit = function (S) {
    var tab = mainTab.find("formEdit");
    if (tab) { tab.focus(); return; }
    tab = mainTab.addItem({ text: "表单编辑器", name: "tableStructure", ico: "fa-database1", closeButton: true });
    var toolBar=tab.addControl({ xtype: "ToolBar", items: [{ text: "保存修改", name:"save" }] });
    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: "*" },{ size: 200 }] });
    tab.focus();
    var formHtml=frame.items[0].addControl({ xtype: "Editor",
        dock: $M.Control.Constant.dockStyle.fill });
    //var addControl = function (parent, json) {
    //    var div = parent.append('<div class="form-group"><label>' + json.labelText + '</label><div class="form-control input-sm"></div></div>');
    //    var control = div.find(".form-control");
    //    for (var x in json) {
    //        control.attr(x, json[x]);
    //        if (x == "value") control.html(json[x]);
    //    }
    //};
    //var addForm = function (form, json) {
    //    if ($.isArray(json.item)) {
    //        for (var i = 0; i < json.item.length; i++) {
    //            if (json.item[i].xtype == "GridView") {
    //                addGrid(form, json.item[i]);
    //            } else {
    //                if (json.item[i].xtype == null) json.item[i].xtype = "TextBox";
    //                addControl(form,json.item[i]);
    //            }
    //        }
    //    } else {
    //        if (json.item.xtype == null) json.item.xtype = "TextBox";
    //        addControl(form,json.item);
    //    }
    //};
    $M.comm("dataBase.getForm", { file: "/test/form/form1.html" }, function (json) {
        formHtml.val(json);
        //addForm(frame.items[1],json.config);
    });
};