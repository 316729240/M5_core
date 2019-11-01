$M.userManage.edit = function (S) {
    var win = $(document.body).addControl({
        xtype: "Window",
        text: "帐户信息",
        ico: "fa-user",
        isModal: true,
        onSubmit: function (sender, e) {
            $M.comm("userManage.edit", {
                id: S.id,
                uname: uname.val(),
                pword: pword == null ? "" : pword.val(),
                mobile: mobile.val(),
                email: email.val(),
                phone: phone.val(),
                role: role.val(),
                filteringIP: filteringIP.val()
            }, function (json) {
                $M.alert("保存成功", win.remove);
            });
            return false;

        },
        onClose: function () {
            //if (S.onClose) S.onClose(win, rdata);
        },
        footer: [
            { xtype: "Button", name: "enter", text: "保 存", type: "submit", color: 2, size: 2 },
            { xtype: "Button", text: "取 消", size: 2, onClick: function () { win.dialogResult = $M.dialogResult.cancel; win.remove(); return false; } }
            ]
    });
    var tab = win.addControl({
        xtype: "Tab", dock: 2,
        style: { height: "360px", width: "730px" },
        items: [
        { text: "基本信息" },
        { text: "访问过滤" }
        ]
    });
    var frame = tab.items[0].addControl({ xtype: "Frame", type: "x", dock: 2,
        items: [
                                    { size: 450, "class": "form-horizontal", style: { overflow: "auto"} },
                                    { text: "角色", size: "*", style: { padding: "5px"}}]
    });
    if (S.id == null) {
        frame.items[0].addControl([
        { xtype: "TextBox", name: "uname", labelText: "用户名", labelWidth: 4, width: 11, vtype: { minlength: 4, required: true, maxlength: 20} },
        { xtype: "TextBox", name: "pword", id: "pword", labelText: "密码", password: true, labelWidth: 4, width: 11, vtype: { required: true, isPassword: true} },
        { xtype: "TextBox", name: "pword2", labelText: "确认密码", password: true, labelWidth: 4, width: 11, vtype: { required: true, isPassword: true, equalTo: "#pword"} },
        { xtype: "TextBox", name: "mobile", labelText: "手机", labelWidth: 4, width: 11, vtype: { isMobile: true} },
        { xtype: "TextBox", name: "email", labelText: "邮箱", labelWidth: 4, width: 11, vtype: { email: true} },
        { xtype: "TextBox", name: "phone", labelText: "电话", labelWidth: 4, width: 11, vtype: { isPhone: true} }
        ]);
    } else {
        frame.items[0].addControl([
        { xtype: "Label", name: "uname", labelText: "用户名", labelWidth: 4, width: 11 },
        { xtype: "Button", name: "reset", labelText: "重置密码", text: "生成随机密码", color: 3, labelWidth: 4, width: 11,
            onClick: function () {
                $M.confirm("您确定要重置用户密码吗？", function () {
                    $M.comm("userManage.resetPassword", { id: S.id }, function (json) {
                        $M.alert("密码更改为：" + json);
                    });
                });
                return false;
            }
        },
        { xtype: "TextBox", name: "mobile", labelText: "手机", labelWidth: 4, width: 11, vtype: { isMobile: true} },
        { xtype: "TextBox", name: "email", labelText: "邮箱", labelWidth: 4, width: 11, vtype: { email: true} },
        { xtype: "TextBox", name: "phone", labelText: "电话", labelWidth: 4, width: 11, vtype: { isPhone: true } }
        ]);
    }

    var filteringIP = tab.items[1].addControl({ xtype: "TextBox", multiLine: true, rows: 8, name: "filteringIP" });
    tab.items[1].append("<span  class=\"help-block\">输入允许登陆的ip,每行只能输入一个</span>");
    var uname = frame.items[0].find("uname");
    var pword = frame.items[0].find("pword");
    var mobile = frame.items[0].find("mobile");
    var email = frame.items[0].find("email");
    var phone = frame.items[0].find("phone");
    var role = frame.items[1].addControl({ xtype: "CheckBox", name: "role", showType: true });
    win.append("<div style='clear:both'></div>");
    win.show();
    var readRole = function (json) {
        var items = [];
        for (var i = 0; i < json.length; i++) {
            items[items.length] = { text: json[i].text, value: json[i].id };
        }
        role.attr("items", items);
    };
    $M.comm("userManage.readRole", null, readRole);
    if (S.id != null) {
        $M.comm("userManage.getUser", { id: S.id }, function (json) {
            uname.val(json.username);
            mobile.val(json.mobile);
            email.val(json.email);
            phone.val(json.phone);
            role.val(json.role);
            filteringIP.val(json.filteringIP);
        });
    }
    tab.items[0].focus();
};
