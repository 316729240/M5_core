$M.userManage.editPassword = function (S) {
    var obj = new $M.dialog({
        title: "密码修改",
        style: { width: "500px" },
        "class": "form-horizontal",
        command: "userManage.editPassword",
        onBeginSubmit: function () {
        }
    });
    obj.show();
    var box = obj.append("<div class=\"form-horizontal\"></div>");
    var cList = box.addControl([
        { xtype: "TextBox", name: "className", labelText: "原密码", name: "oldPassword", labelWidth: 3, password: true, vtype: { required: true} },
        { xtype: "TextBox", name: "keyword", labelText: "新密码",name:"newPassword", labelWidth: 3,id:"oldPword", password: true, vtype: { required: true, isPassword: true} },
        { xtype: "TextBox", name: "keyword", labelText: "确认密码", labelWidth: 3, password: true, vtype: { required: true, isPassword: true, equalTo: "#oldPword"} }
        ]);
};
