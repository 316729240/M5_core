$M.system.configEdit = function (S) {
    var tab = mainTab.find("configEdit");
    if (tab) { tab.focus(); return; }
    var T = mainTab.addItem({ text: "网站配制", closeButton: true, name: "configEdit", ico: "fa-cog" });
    var gridList = [];
    var tab = T.addControl({
        xtype: "Tab", alignment: 1, dock: $M.Control.Constant.dockStyle.fill,
        items: [
        { text: "基本设置", file: "base.config" },
        { text: "系统变量", file: "systemVariables.config" },
        { text: "邮箱设置", file: "mail.config" },
        { text: "水印设置", file: "watermark.config" },
        { text: "关键词替换", file: "keyword.config" },
        { text: "关键词过滤", file: "keywordFiltering.config" },
        { text: "内链设置", file: "link.config" },
        { text: "缓存设置", file: "cache.config" },
        { text: "映射规则", file: "userRewrite.config" },
        { text: "IP过滤", file: "ip.config"}],
        onSelectedIndexChanged: function (sender, e) {
            if (!sender.selectedItem.form) {
                $M.comm("getConfig", { file: sender.selectedItem.attr("file") }, function (json) {
                    sender.selectedItem.form = sender.selectedItem.addControl({
                        command: "saveConfig",
                        xtype: "Form",
                        "class": "col-sm-11",
                        onSubmit: function () {
                            $M.alert("保存成功！");
                        }
                    });
                    var form = sender.selectedItem.form;

                    form.attr("onBeginSubmit", function () {
                        for (var i = 0; i < gridList.length; i++) {
                            var value = getGridXml(gridList[i][0], gridList[i][2]);
                            gridList[i][1].val(value);
                        }
                    });

                    form.append("<input name='_configFile' value='" + sender.selectedItem.attr("file") + "' type=hidden>");
                    addForm(form, json.config);

                    //if (json.config.item.xtype == "GridView") {
                    //    json.config.item.allowMultiple = true;
                    //    addGrid(form, json.config.item);
                    //} else {
                    //    if ($.isArray(json.config.item)) {
                    //        for (var i = 0; i < json.config.item.length; i++) {
                    //            if (json.config.item[i].xtype == "GridView") {
                    //                addGrid(form, json.config.item[i]);
                    //            } else {
                    //                form.addControl(json.config.item[i]);
                    //            }
                    //        }
                    //    } else {
                    //        form.addControl(json.config.item);
                    //    }
                    //}
                });
            }
        }
    });
    var getGridXml = function (grid,json) {
            var xmlcontent = "";
            var data = grid.val();
            for (var i = 0; i < data.length; i++) {
                var xml = new $M.xml();
                var node = xml.addDom("data");
                for (var i1 = 0; i1 < json.columns.length; i1++) {
                    var f = node.addDom(json.columns[i1].name);
                    f.val(data[i][i1]);
                }
                xmlcontent += xml.getXML();
            }
            return xmlcontent;
    };
    var addForm = function (form, json) {
        if (json.item.xtype == "Tab") {
            var items = [];
            if ($.isArray(json.item.item)) {
                for (var i = 0; i < json.item.item.length; i++) {
                    items[i] = { text: json.item.item[i].text };
                }
            } else {
                items = [json.item.item];
            }
            var tab = form.addControl({ xtype: "Tab", items: items });
            for (var i = 0; i < items.length; i++) {
                if ($.isArray(json.item.item)) {
                    addForm(tab.items[i], json.item.item[i]);
                } else {
                    addForm(tab.items[i], json.item.item);
                }
            }
        } else if (json.item.xtype == "GridView") {
            json.item.allowMultiple = true;
            addGrid(form, json.item);
        } else {
            if ($.isArray(json.item)) {
                for (var i = 0; i < json.item.length; i++) {
                    if (json.item[i].xtype == "GridView") {
                        addGrid(form, json.item[i]);
                    } else {
                        if (json.item[i].xtype == null) json.item[i].xtype = "TextBox";
                        form.addControl(json.item[i]);
                    }
                }
            } else {
                if (json.item.xtype == null) json.item.xtype = "TextBox";
                form.addControl(json.item);
            }
        }
    };
    var addGrid = function (form, json) {
        json.commit = false;
        var gridText = form.append("<textarea name='"+json.name+"' style='display:none'></textarea>");
        var grid = null;
        var toolBar = form.addControl({
            xtype: "ToolBar",
            items: [
                                [{ xtype: "Button", ico: "fa-pencil", text: "添加", onClick: function () { grid.addRow({}); return false; } },
                                { xtype: "Button", ico: "fa-times", text: "删除",
                                    onClick: function () {
                                        var count = grid.selectedRows.length;
                                        for (var i = count - 1; i > -1; i--) grid.selectedRows[i].remove();
                                        return false;
                                    }
                                }]
                            ]
        });
        if (typeof (json.style) == "string") { eval("json.style=" + json.style); }
        grid = form.addControl(json);
        gridList[gridList.length] =[ grid,gridText,json];
        //form.attr("onBeginSubmit", function () {
        //    var xmlcontent = "";
        //    var data = grid.val();
        //    for (var i = 0; i < data.length; i++) {
        //        var xml = new $M.xml();
        //        var node = xml.addDom("data");
        //        for (var i1 = 0; i1 < json.columns.length; i1++) {
        //            var f = node.addDom(json.columns[i1].name);
        //            f.val(data[i][i1]);
        //        }
        //        xmlcontent += xml.getXML();
        //    }
        //    gridText.val(xmlcontent);
        //});
    };
    
    var foot = T.append("<div class=\"form-actions text-right\"></div>");
    var submitButton = foot.addControl({ xtype: "Button", name: "enter", text: "保存设置", color: 2, size: 2 });
    T.focus();

    tab.attr("marginHeight", foot.outerHeight());
    T.resize();
    submitButton.attr("onClick", function () { tab.selectedItem.form.submit(); });
    $M.comm("appConfigList", {}, function (json) {
        tab.addItem(json)
    });
};