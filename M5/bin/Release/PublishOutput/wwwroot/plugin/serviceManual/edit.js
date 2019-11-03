
$M.insertFujian = function (S) {
    var win = $(document.body).addControl({
        xtype: "Window",
        text: "上传文件",
        isModal: true,
        style: { width: "500px" },
        ico: S.ico,
        onClose: function () {
            cancelFlag = true;
        },
        footer: [
            { xtype: "Button", text: "取 消", size: 2, onClick: function () { win.dialogResult = $M.dialogResult.cancel; win.remove(); return false; } }
        ]
    });
    var cancelFlag = false; //终止上传
    var label = win.append("<h2></h2>");
    var p1 = win.addControl({ xtype: "ProgressBar" });

    var files = [];
    var uploadComplete = function (json) {
        if (json.errNo < 0) {
            $M.confirm(json.errMsg, function (flag) {
                fileIndex--;
                uploadFile();
            }, { onCancel: function () { end(); } });
        } else {
            for (var i1 = 0; i1 < json.userData.length; i1++) {
                files[files.length] = json.userData[i1];
            }
            uploadFile();
        }
    };
    var uploadProgress = function (evt) {
        if (evt.lengthComputable) {
            var fen = 100 / inputFile[0].files.length;
            var percentComplete = Math.round(evt.loaded / evt.total * fen + fen * fileIndex);
            p1.val(percentComplete);
        }
    };
    var end = function () {
        win.remove();
        if (S.back) S.back(files);
        inputFile.remove();
    };
    var uploadFile = function () {
        fileIndex++;
        if (cancelFlag || fileIndex == inputFile[0].files.length) { end(); return; }
        // if (inputFile[0].files[fileIndex].type == "") {uploadFile();return;}
        var fd = new FormData();
        label.html("正在上传" + inputFile[0].files[fileIndex].name);
        fd.append("fileData", inputFile[0].files[fileIndex]);
        coveredFlag = 0;
        var xhr = new XMLHttpRequest();
        xhr.upload.addEventListener("progress", uploadProgress, false);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4) {
                var json = null;
                //try {
                eval("json=" + xhr.responseText);
                uploadComplete(json);
                //                } catch (x) {
                //                    json = { errNo: -1, errMsg: xhr.responseText };
                //                    uploadComplete(json);
                //                }
            }
        };

        xhr.open("POST", $M.config.appPath + "article/upload");
        xhr.send(fd);
    }
    var inputFile = $("<input type='file' accept='aplication/*'  " + (S.isMultiple == false ? "" : "multiple='multiple'") + " style='display:none' >").appendTo($(document.body));
    inputFile.on("change", function () {
        fileIndex = -1;
        win.show();
        uploadFile();
    });
    inputFile[0].click();
};
//后台数据编辑入口函数 命名为 edit 不能更改
$M.serviceManual.edit = function (S) {
    var tab = mainTab.addItem({ text: "资料编辑", "class": "form-horizontal", closeButton: true });

    var toolBar = tab.addControl({
        xtype: "ToolBar", class: "note-toolbar",
        items: [
            [{ text: "保存", ico: "fa-save", primary: true, onClick: function () { form.submit(); } }, {
                text: "打开网址", ico: "fa-external-link", onClick: function () {
                    if (S.url) window.open(S.url);
                }
            }, { text: "新增", ico: "fa-plus", onClick: function () { newData(); } }],
            [{
                text: "镜像数据", ico: "fa-plus", onClick: function () {
                    if (dataId.val() == null || dataId.val() == "") {
                        $M.alert("当前数据没有保存");
                        return;
                    }
                    dataId.val("");
                    $M.alert("镜像成功,你可能直接修改当前数据并保存");
                }
            }],
            [
                {
                    text: "审核", menu: [
                      {
                          text: "通过", onClick: function () {
                              $M.comm("auditData", { ids: dataId.val(), flag: 0, classId: classId.val() }, function () { $M.alert("操作成功！"); if (S.back) S.back(); });
                          }
                      }, {
                          text: "拒绝", onClick: function () {
                              $M.prompt("拒绝理由", function (msg) {
                                  $M.comm("auditData", { ids: dataId.val(), flag: 1, msg: msg, classId: classId.val() }, function () { $M.alert("操作成功！"); if (S.back) S.back(); });
                              }, { required: true });

                          }
                      }]
                }]
        ]
    });
    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: "*" }, { size: 300, visible: false }] });
    //frame.attr("items", [{ size: "*" }, { size: 0}]);
    //frame.items[1].attr("size",0);
    frame.items[0].css({ "overflow-y": "auto", "overflow-x": "hidden" });
    tab.focus();
    var form = frame.items[0].addControl({
        xtype: "Form",
        command: "serviceManual.edit",
        onSubmit: function (sender, e) {
            if (S.back) S.back();
            $M.confirm("保存成功，是否关闭？", tab.remove, { footer: [{ text: "是" }, { text: "否" }] });
            read(e.returnData);
        }
    });
    var custom = form.append("<textarea  name=\"u_custom\" style=\"display:none\"></textarea>");
    var skinId = form.append("<input name=skinId value='' type='hidden'>");
    var dataId = form.append("<input name=id value='" + S.dataId + "' type='hidden'>");
    var classId = form.append("<input name=classId value='" + S.classId + "' type='hidden'>");
    var dataList = [];
    dataList[0] = form.addControl({ xtype: "TextBox", name: "title", labelText: "文章标题", labelWidth: 2, vtype: { required: true } });
    dataList[1] = form.addControl({ xtype: "TextBox", name: "u_keyword", labelText: "关 键 词", labelWidth: 2, vtype: { required: true } });
    var car_type_box = form.append("<div class=\"form-group\"><label class=\"col-sm-2 control-label\">车  型</label><div class=\"col-sm-10\"></div>");
    var car_type_div = car_type_box.find(".col-sm-10");
    var loadSelectItem = function (t, value) {
        var obj = sel[t];
        var v = null;
        if (value != null) {
            v = { t: t };
            for (var i = 0; i < value.length; i++) {
                v["a" + (i + 1)] = value[i];
            }
        }
        $M.comm("serviceManual.pinpai", v, function (json) {
            obj.clear();
            obj.addItem({ text: "请选择" });
            for (var i = t ; i < 6; i++) {
                sel[i].clear();
                sel[i].addItem({ text: "请选择", value: "" });
            }
            obj.addItem(json);
        });
    };
    var sel = [];
    var changeFlag = false;
    sel[0] = car_type_div.addControl({
        xtype: "SelectBox", onChange: function (sender, e) {
            if (changeFlag) return;
            loadSelectItem(1, [sel[0].val()]);
        }
    });
    sel[1] = car_type_div.addControl({
        xtype: "SelectBox", onChange: function (sender, e) {
            if (changeFlag) return;
            loadSelectItem(2, [sel[0].val(), sel[1].val()]);
        }
    });
    sel[2] = car_type_div.addControl({
        xtype: "SelectBox", onChange: function (sender, e) {
            if (changeFlag) return;
            loadSelectItem(3, [sel[0].val(), sel[1].val(), sel[2].val()]);
        }
    });
    sel[3] = car_type_div.addControl({
        xtype: "SelectBox", onChange: function (sender, e) {
            if (changeFlag) return;
            loadSelectItem(4, [sel[0].val(), sel[1].val(), sel[2].val(), sel[3].val()]);
        }
    });
    sel[4] = car_type_div.addControl({
        xtype: "SelectBox", onChange: function (sender, e) {
            if (changeFlag) return;
            loadSelectItem(5, [sel[0].val(), sel[1].val(), sel[2].val(), sel[3].val(), sel[4].val()]);
        }
    });
    sel[5] = car_type_div.addControl({
        xtype: "SelectBox", vtype: { required: true }, name: "u_carId"
    });
    loadSelectItem(0);
    dataList[2] = form.addControl({ xtype: "TextBox", name: "u_info", labelText: "文章摘要", labelWidth: 2, multiLine: true, style: { height: "50px" }, vtype: { required: true } });
    dataList[3] = form.addControl({
        xtype: "Editor", name: "u_content", style: { height: 300 }, labelText: "文章内容", labelWidth: 2, onInsertImg: function (sender, e) {
            return "<img src='" + e + "'>"+sender.replaceLabel("<!-- PageSpacer -->&nbsp; ");
        }
    });
    dataList[4] = form.addControl({ xtype: "TextBox", name: "u_from", labelText: "来    源", labelWidth: 2, vtype: { required: true }, placeholder: "分类-文件名-页码" });
    dataList[5] = form.addControl({
    xtype: "DialogInput", name: "u_fromWeb", labelText: "附 　 件", labelWidth: 2, onButtonClick: function (sender,e) {

        $M.insertFujian({
            isMultiple: false,
            back: function (json) {
                sender.val(json[0]);
            }
        });
    }
});
    var downlabel = form.append("<div class=form-group><label class=\"col-sm-2 control-label\">下载PDF文件</label><div class=col-sm-10 ><a href='' target=_blank >下载</a></div></div>");
    downlabel.hide();
    var read = function (dataId) {
        if (dataId) {
            $M.comm("serviceManual.read", { id: dataId }, function (json) {
                setForm(json);
            });
        }
    };
    var setForm = function (data) {
        form.val(data);
        S.skinId = data.skinId;
        S.url = data.url;

    };
    var newData = function () {
        for (var i = 0; i < dataList.length; i++) dataList[i].val("");
        dataId.val("");
    };
    if (S.dataId != null) {
        $M.comm("serviceManual.read", { id: S.dataId }, function (data) {


            setForm(data);
            $M.comm([
            ["serviceManual.pinpai", { t: 0 }],
            ["serviceManual.pinpai", { t: 1, a1: data.carInfo[0] }],
            ["serviceManual.pinpai", { t: 2, a1: data.carInfo[0], a2: data.carInfo[1] }],
            ["serviceManual.pinpai", { t: 3, a1: data.carInfo[0], a2: data.carInfo[1], a3: data.carInfo[2] }],
            ["serviceManual.pinpai", { t: 4, a1: data.carInfo[0], a2: data.carInfo[1], a3: data.carInfo[2], a4: data.carInfo[3] }],
            ["serviceManual.pinpai", { t: 5, a1: data.carInfo[0], a2: data.carInfo[1], a3: data.carInfo[2], a4: data.carInfo[3], a5: data.carInfo[4] }]
            ], function (json) {
                changeFlag = true;
                for (var i = 0; i < json.length; i++) {
                    sel[i].clear();
                    sel[i].addItem({ text: "请选择", value: "" });
                    sel[i].addItem(json[i]);
                    sel[i].val(data.carInfo[i]);
                }
                sel[5].val(data.u_carId);
                changeFlag = false;
            });
            var url = data.u_from.split('-');
            downlabel.find("a").attr("href", "/pdf/" + url[0] + "/" + url[1] + ".pdf");
            downlabel.show();
        });
    }
};
$M.serviceManual.addCar = function (S) {
    var tab = $M.dialog({
        title: "编辑车型",
        style: { width: "660px" },
        command: "serviceManual.editBind",
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                S.reload();
            }
        }
    });
    var maindiv = tab.append("<div class='form-horizontal'></div>");
    var form = maindiv.addControl({
        xtype: "Form",
        command: "serviceManual.edit",
        onSubmit: function (sender, e) {
            if (S.back) S.back();
            $M.confirm("保存成功，是否关闭？", tab.remove, { footer: [{ text: "是" }, { text: "否" }] });
            read(e.returnData);
        }
    });
    var userId = form.append("<input name=userId value='" + S.userId + "' type='hidden'>");
    var dataId = form.append("<input name=id value='" + S.dataId + "' type='hidden'>");
    var dataList = [];
    dataList[0] = form.addControl({
        xtype: "SelectBox", name: "sel11", labelText: "车型选择方式", labelWidth: 3, items: [{ text: "直接选择", value: 0 }, { text: "VIN选择方式", value: 1 }], onChange: function (sender, e) {
            if (sender.val() == 0) {
                div1.hide();
                div2.show();
            } else {
                div2.hide();
                div1.show();
            }
        }
    });
    var div1 = form.append("<div></div>");
    var div2 = form.append("<div></div>");
    dataList[1] = div1.addControl({
        xtype: "TextBox", name: "u_vin", labelText: "VIN码", labelWidth: 3, onChange: function (sender, e) {
            $M.comm("serviceManual.frontEnd.vincar", { vin: sender.val() }, function (json) {
                dataList[2].addItem(json);
            });
        }
    });
    dataList[2] = div1.addControl({ xtype: "SelectBox", name: "u_carId2", labelText: "选择车型", labelWidth: 3, items: [{ text: "请选择", value: "" }] });
    var car_type_box = div2.append("<div class=\"form-group\"><label class=\"col-sm-3 control-label\">选择车型</label><div class=\"col-sm-9\"></div>");
    var car_type_div = car_type_box.find(".col-sm-9");
    form.addControl({ xtype: "DatePickerInput", name: "u_maintainDate", labelText: "上次保养时间", labelWidth: 3 });
    form.append("<div style='clear:both;'></div>");
    var loadSelectItem = function (t, value) {
        var obj = sel[t];
        var v = null;
        if (value != null) {
            v = { t: t };
            for (var i = 0; i < value.length; i++) {
                v["a" + (i + 1)] = value[i];
            }
        }
        $M.comm("serviceManual.pinpai", v, function (json) {
            obj.clear();
            obj.addItem({ text: "请选择" });
            for (var i = t ; i < 6; i++) {
                sel[i].clear();
                sel[i].addItem({ text: "请选择", value: "" });
            }
            obj.addItem(json);
        });
    };
    var sel = [];
    var changeFlag = false;
    sel[0] = car_type_div.addControl({
        xtype: "SelectBox", onChange: function (sender, e) {
            if (changeFlag) return;
            loadSelectItem(1, [sel[0].val()]);
        }
    });
    sel[1] = car_type_div.addControl({
        xtype: "SelectBox", onChange: function (sender, e) {
            if (changeFlag) return;
            loadSelectItem(2, [sel[0].val(), sel[1].val()]);
        }
    });
    sel[2] = car_type_div.addControl({
        xtype: "SelectBox", onChange: function (sender, e) {
            if (changeFlag) return;
            loadSelectItem(3, [sel[0].val(), sel[1].val(), sel[2].val()]);
        }
    });
    sel[3] = car_type_div.addControl({
        xtype: "SelectBox", onChange: function (sender, e) {
            if (changeFlag) return;
            loadSelectItem(4, [sel[0].val(), sel[1].val(), sel[2].val(), sel[3].val()]);
        }
    });
    sel[4] = car_type_div.addControl({
        xtype: "SelectBox", onChange: function (sender, e) {
            if (changeFlag) return;
            loadSelectItem(5, [sel[0].val(), sel[1].val(), sel[2].val(), sel[3].val(), sel[4].val()]);
        }
    });
    sel[5] = car_type_div.addControl({
        xtype: "SelectBox", vtype: { required: true }, name: "u_carId"
    });
    loadSelectItem(0);
    ;
    var read = function (dataId) {
        if (dataId) {
            $M.comm("serviceManual.read", { id: dataId }, function (json) {
                setForm(json);
            });
        }
    };
    var setForm = function (data) {
        form.val(data);
        S.skinId = data.skinId;
        S.url = data.url;

    };
    var newData = function () {
        for (var i = 0; i < dataList.length; i++) dataList[i].val("");
        dataId.val("");
    };
    if (S.dataId != null) {
        $M.comm("serviceManual.readBind", { id: S.dataId }, function (data) {
            setForm(data);
            $M.comm([
            ["serviceManual.pinpai", { t: 0 }],
            ["serviceManual.pinpai", { t: 1, a1: data.carInfo[0] }],
            ["serviceManual.pinpai", { t: 2, a1: data.carInfo[0], a2: data.carInfo[1] }],
            ["serviceManual.pinpai", { t: 3, a1: data.carInfo[0], a2: data.carInfo[1], a3: data.carInfo[2] }],
            ["serviceManual.pinpai", { t: 4, a1: data.carInfo[0], a2: data.carInfo[1], a3: data.carInfo[2], a4: data.carInfo[3] }],
            ["serviceManual.pinpai", { t: 5, a1: data.carInfo[0], a2: data.carInfo[1], a3: data.carInfo[2], a4: data.carInfo[3], a5: data.carInfo[4] }]
            ], function (json) {
                changeFlag = true;
                for (var i = 0; i < json.length; i++) {
                    sel[i].clear();
                    sel[i].addItem({ text: "请选择", value: "" });
                    sel[i].addItem(json[i]);
                    sel[i].val(data.carInfo[i]);
                }
                sel[5].val(data.u_carId);
                changeFlag = false;
            });
            var url = data.u_from.split('-');
            downlabel.find("a").attr("href", "/pdf/" + url[0] + "/" + url[1] + ".pdf");
            downlabel.show();
        });
    }

    div1.hide();
    div2.show();
    tab.show();
};
$M.serviceManual.buyCar = function (S) {
    var tab = $M.dialog({
        title: "添加车型",
        style: { width: "660px" },
        command: "serviceManual.buyP",
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                if (S.reload) S.reload();
            }
        }
    });
    tab.show();

    var div1 = tab.append("<div class=row></div>");
    var selects=div1.addControl([
        { xtype: "SelectBox", width: 4, name: "a1", labelText: "品牌", labelWidth: 4, items: [{ text: "请选择", value: "" }] },
        { xtype: "SelectBox", width: 4, name: "a2", labelText: "车型", labelWidth: 4, items: [{ text: "请选择", value: "" }] },
        { xtype: "SelectBox", width: 4, name: "a3", labelText: "版本", labelWidth: 4, items: [{ text: "请选择", value: "" }] }]
    );
    var textbox = tab.append("<input type=hidden name='pid'>");
    tab.append("<input type=hidden name='userId' value='"+S.userId+"'>");
    var grid = tab.addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, border: 0, condensed: 1,
        columns: [{ text: "id", name: "id", visible: false }, { text: "	时间", name: "u_hour", width: 200 }, { text: "价格(元)", name: "u_cash", width: 200 }],
        onSelectionChanged: function (sender, e) {
            if (grid.selectedRows.length>0) textbox.val(grid.selectedRows[0].cells[0].val());
        }
    });
    var div2 = tab.append("<div class=row style='width:150px;margin-top:10px;'></div>");
    div2.addControl({ xtype: "TextBox", labelText: "份数",name:"count", labelWidth: 5 ,value:1});
    $M.comm("serviceManual.readP1", { userId:S.userId,t: 0 }, function (json) {
                selects[0].addItem(json);
    });
    selects[0].attr("onChange", function (sender, e) {
        $M.comm("serviceManual.readP1", { userId: S.userId, t: 1, a1: sender.val() }, function (json) {
            selects[1].clear();
            selects[1].addItem({ text: "请选择", value: "" });
            selects[1].addItem(json);
        });
        $M.comm("serviceManual.readP2", { t: 1, a1: sender.val() }, function (json) {
            grid.clear();
            grid.addRow(json);
        });
    });
    selects[1].attr("onChange", function (sender, e) {
        $M.comm("serviceManual.readP1", { userId: S.userId, t: 2, a1: selects[0].val(), a2: selects[1].val() }, function (json) {
            selects[2].clear();
            selects[2].addItem({ text: "请选择", value: "" });
            selects[2].addItem(json);
        });
        $M.comm("serviceManual.readP2", { t: 2, a1: selects[0].val(), a2: selects[1].val() }, function (json) {
            grid.clear();
            grid.addRow(json);
        });
    });

    selects[2].attr("onChange", function (sender, e) {
        $M.comm("serviceManual.readP2", { t: 3, a1: selects[0].val(), a2: selects[1].val(), a3: selects[2].val() }, function (json) {
            grid.clear();
            grid.addRow(json);
        });
    });
};