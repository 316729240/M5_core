$M.account.edit = function (S) {
    var tab = mainTab.addItem({ text: "会员信息", "class": "form-horizontal", closeButton: true });
    /*
    var toolBar = tab.addControl({
        xtype: "ToolBar", class: "note-toolbar",
        items: [
            [{ text: "保存", ico: "fa-save", primary: true, onClick: function () {  } }]
        ]
    });*/
    var tab2 = tab.addControl({
        xtype: "Tab", alignment: 1, dock: $M.Control.Constant.dockStyle.fill,
        items: [
        { text: "会员信息" },
        { text: "绑定车型" },
        { text: "购买车型" }
        ],
        onSelectedIndexChanged: function (sender, e) {
            
        }
    });
    var form = tab2.items[0].addControl({
        xtype: "Form"
    });
    var list = null;
    if (S.classId == "9896847028") {
        list = form.addControl([
            [{ xtype: "TextBox", width: 3, name: "uname", labelText: "用户名", labelWidth: 4, vtype: { minlength: 4, required: true, maxlength: 20 } },
            { xtype: "TextBox", width: 3, name: "name", id: "name", labelText: "姓名" },
            { xtype: "TextBox", width: 2, name: "sex", labelText: "性别", xtype: "SelectBox", items: [{ text: "男", value: 0 }, { text: "女", value: 1 }] },
            { xtype: "SelectBox", width: 2, name: "marriage", labelText: "婚姻状况", items: [{ text: '未婚', value: 0 }, { text: '已婚', value: 1 }] }
            ,
            { xtype: "SelectBox", width: 2, name: "education", labelText: "教育程度", items: [{ text: '小学' }, { text: '初中' }, { text: '高中' }, { text: '大学' }, { text: '其它' }] }

            ],
            [{ xtype: "TextBox", width: 4, name: "email", id: "email", labelText: "邮箱", vtype: { email: true } },
            { xtype: "TextBox", width: 4, name: "phone", id: "phone", labelText: "电话", vtype: { isTel: true } },
            { xtype: "TextBox", width: 4, name: "mobile", id: "mobile", labelText: "手机", vtype: { isMobile: true } }],
            [{ xtype: "TextBox", width: 6, name: "industry", id: "industry", labelText: "行业" },
            { xtype: "TextBox", width: 6, name: "companyName", id: "companyName", labelText: "公司" }],
            [{ xtype: "TextBox", width: 6, name: "department", id: "department", labelText: "部门" },
            { xtype: "TextBox", width: 6, name: "position", id: "position", labelText: "职位" }],
            [{ xtype: "TextBox", width: 12, name: "url", id: "url", labelText: "公司网址" }],
            [{ xtype: "DatePickerInput", width: 6, name: "birthday", id: "birthday", labelText: "出生日期" }, { name: "city", labelText: "城市", width: 3 }, { name: "area", labelText: "地区", width: 3 }],
            [{ name: "occupation", labelText: "职称", width: 6 }, { name: "character", labelText: "性格", width: 6 }],
            [{ name: "habit", labelText: "习惯", width: 6 }, { name: "like_domain", labelText: "关心领域", width: 6 }],
            [{ name: "like_brand", labelText: "关心品牌", width: 6 }, { name: "vin", labelText: "vin码", width: 6 }]
        ]);
    } else {
        list = form.addControl([
            [{ xtype: "TextBox", width: 3, name: "uname", labelText: "用户名", labelWidth: 4, vtype: { minlength: 4, required: true, maxlength: 20 } }, { name: "city", labelText: "城市", width: 2 }, { name: "area", labelText: "地区", width: 2 }],
            [{ xtype: "TextBox", width: 6, name: "industry", id: "industry", labelText: "行业" },
            { xtype: "TextBox", width: 6, name: "companyName", id: "companyName", labelText: "公司" }],
            [{ xtype: "TextBox", width: 6, name: "department", id: "department", labelText: "部门" },
            { xtype: "TextBox", width: 6, name: "position", id: "position", labelText: "职位" }],
            [{ xtype: "TextBox", width: 12, name: "url", id: "url", labelText: "公司网址" }],
            [{ xtype: "TextBox", width: 4, name: "email", id: "email", labelText: "邮箱", vtype: { email: true } },
            { xtype: "TextBox", width: 4, name: "phone", id: "phone", labelText: "电话", vtype: { isTel: true } },
            { xtype: "TextBox", width: 4, name: "mobile", id: "mobile", labelText: "手机", vtype: { isMobile: true } }],
            [{ name: "address", labelText: "详细地址" }],
            [{ name: "occupation", labelText: "服务内容" }]
        ]);
        var zizhi = form.append("<div class=\"row\"><div class=\"col-md-12\"><label>企业资质</label><br><a href=''><img src='' width=100></a><br><a href=''>点击放大</a></div></div>");
        var shenheButton = zizhi.append("<div><button val=1>审核通过</button> <button val=-1>拒绝通过</button></div>");
        shenheButton.find("button").click(function () {
            $M.comm("account.shenhezizhi", { id: S.dataId, type: $(this).attr("val") }, function () {
                S.back();
                alert("操作成功");
            });
            return false;
        });
    }
    var read = function (dataId) {
        if (dataId) {
            $M.comm("account.read", { id: dataId }, function (json) {
                json.sex = json.sex ? "1" : "0";
                form.val(json);
                zizhi.find("a").hide();
                if (json.pic != null) {
                    zizhi.find("a").show();
                    zizhi.find("img").attr("src", json.pic);
                    zizhi.find("a").attr("target", "_blank");
                    zizhi.find("a").attr("href", json.pic);
                }
            });
        }
    };
    tab.focus();
    read(S.dataId);
    var delData = function () {
        if (grid.selectedRows.length == 0) return;
        var ids = getId(grid);
        if (ids == "") return;
        $M.confirm("您确定要删除所选数据吗？", function () {
            $M.comm("serviceManual.delCar", {ids: ids }, function () {
                reload();
            });
        });
    };
    var delData2 = function () {
        if (grid2.selectedRows.length == 0) return;
        var ids = getId(grid2);
        if (ids == "") return;
        $M.confirm("您确定要删除所选数据吗？", function () {
            $M.comm("serviceManual.delBuyCar", { ids: ids }, function () {
                reload2();
            });
        });
    };
    var toolBar = tab2.items[1].addControl({
        xtype: "ToolBar", class: "note-toolbar",
        items: [
            [
                {
                    text: "新增", ico: "fa-plus", color: 2, onClick: function () {
                        $M.app.call("$M.serviceManual.addCar", { userId:S.dataId,reload:reload });
                    }
                },
                { text: "删除", ico: "fa-trash-o", onClick: function () { delData(); } },
            ]

        ]
    });

    var getId = function (obj) {
        var ids = "";
        for (var i = 0; i < obj.selectedRows.length; i++) {
            if (i > 0) ids += ",";
            ids += obj.selectedRows[i].cells[0].val();
        }
        return ids;
    };
    var grid = tab2.items[1].addControl({
        xtype: "GridView",
        dock: $M.Control.Constant.dockStyle.fill, 
        allowMultiple: true,
        columns: [{ name: "id", width: 120, text: "id" }, { name: "title", width: 300, text: "车型名称" }, { name: "u_vin", text: "vin", width: 160 }, { name: "u_maintainDate", text: "上次保养时间" }],
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 3) {
                var value = new Date(parseInt(e.value.replace(/\D/igm, "")));
                return value.format("yyyy-MM-dd hh:mm:ss");
            }
        },
        onKeyDown: function (sender, e) {
            if (e.which == 46) delData();
        }
    });


    var toolBar2 = tab2.items[2].addControl({
        xtype: "ToolBar", class: "note-toolbar",
        items: [
            [
                {
                    text: "新增", ico: "fa-plus", color: 2, onClick: function () {
                        $M.app.call("$M.serviceManual.buyCar", { userId: S.dataId, reload: reload2 });
                    }
                },
                { text: "删除", ico: "fa-trash-o", onClick: function () { delData2(); } },
            ]

        ]
    });

    var grid2 = tab2.items[2].addControl({
        xtype: "GridView",
        dock: $M.Control.Constant.dockStyle.fill,
        allowMultiple: true,
        columns: [
            { name: "id", width: 120, text: "id", visible:false},
            { name: "u_brand", width: 100, text: "品牌" },
            { name: "u_sub_brand", width: 100, text: "车型" },
            { name: "u_version", width: 150, text: "版本" },
            { name: "createdate", text: "购买时间", width: 150 },
            { name: "u_hour", text: "购买内容", width: 100 },
            { name: "beizhu", text: "状态" }
        ],
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 4) {
                var value = new Date(parseInt(e.value.replace(/\D/igm, "")));
                return value.format("yyyy-MM-dd hh:mm:ss");
            }
        },
        onKeyDown: function (sender, e) {
            //if (e.which == 46) delData();
        }
    });

    var reload = function () {
        $M.comm("serviceManual.carlist", { pageNo: 1, dataId: S.dataId }, function (json) {
            grid.clear();
            grid.addRow(json);
        });
    };
    var reload2 = function () {
        $M.comm("serviceManual.buyList", { pageNo: 1, userId: S.dataId }, function (json) {
            grid2.clear();
            grid2.addRow(json);
        });
    };
    reload();
    reload2();

};