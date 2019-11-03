$M.Control["InputSelect"] = function(BoxID, S, CID) {
    var T = this;
    var A = null;
    var html = '<div class="input-group btn-group">' +
				'<div class="input-group-btn"><button type="button" class="btn btn-default dropdown-toggle"></button></div>' +
			'</div>';
    if (CID) {
        A = $(html);
        A.insertBefore(CID);
        var input_group_btn = A.find(".input-group-btn");
        $(CID).insertBefore(input_group_btn);
        CID.addClass("inputbox");
        CID.addClass("form-control");
    } else {
        A = $(html).appendTo(BoxID);
        var input_group_btn = A.find(".input_group_btn");
        input_group_btn.insertBefore('<input class="inputbox form-control"  type="text">');
    }
    A.addClass("input-group-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size]);
    var button = A.find("button");
    var inputbox = A.find(".inputbox");

    if (typeof (S.items) == "string") { eval("S.items=" + S.items); }
    var menu = $(document.body).addControl({ xtype: "Menu", items:S.items, style: { width: '100%' },
        onClose: function(sender, e) {
            A.removeClass("open");
        },
        onItemClicked: function(sender, e) {
        inputbox.val(e.attr("text"));
        inputbox.focus();
        inputbox.select();
        }
    });
    button.click(function() {
        A.addClass("open");
        menu.open(null, null, A);
    });
    T.attr = function(a, b) {
        return S[a];
    };

    T.val = function(value) {
        if (value == null) return inputbox.val();
        inputbox.val(value);
    }; 
    inputbox[0]._control = A;
};
$M.Control["PageBar"] = function (BoxID, S, CID) {
    var A = null, T = this;
    var pageCount = Math.floor(((S.recordCount - 1) / S.pageSize) + 1);
    var record1 = ((S.pageNo - 1) * S.pageSize) + 1;
    var record2 = record1 + S.pageSize - 1;
    var firstPage = function () {
        pageChanged(0);
    };
    var prePage = function () {
        S.pageNo--;
        pageChanged(1);
    };
    var nextPage = function () {
        S.pageNo++;
        pageChanged(2);
    };
    var lastPage = function () {
        S.pageNo = pageCount;
        pageChanged(3);
    };
    var pageChanged = function (type) {
        if (S.onPageChanged) S.onPageChanged(T, { pageNo: S.pageNo });
    };
    var bar = BoxID.addControl({ xtype: "ToolBar", items: [
                        [{ xtype: "Button", name: "firstButton", ico: "fa-fast-backward", onClick: firstPage }, { xtype: "Button", name: "preButton", ico: "fa-backward", onClick: prePage}],
                        [{ xtype: "Label", text: "第", style: { float: "left", padding: "4px 0px 0px 0px"}}],
                        [{ xtype: "TextBox", name: "pageNo", style: { width: "50px" }, size: 1, text: S.pageNo, onKeyDown: function (sender, e) {
                            if (e.which == 13) {
                                S.pageNo = sender.val();
                                if (isNaN(S.pageNo)) {
                                    S.pageNo = 1;
                                    sender.val(1);
                                }
                                if (S.pageNo > pageCount) S.pageNo = pageCount;
                                if (S.pageNo < 1) S.pageNo = 1;

                                pageChanged(3);
                            }
                        }
                        }],
                        [{ xtype: "Label", name: "pageCount", text: "页，共页", style: { float: "left", padding: "4px 0px 0px 0px"}}],
                        [{ xtype: "Button", name: "nextButton", ico: "fa-forward", onClick: nextPage }, { xtype: "Button", name: "lastButton", ico: "fa-fast-forward", onClick: lastPage}],
                        [{ xtype: "Button", ico: "fa-refresh", onClick: function () { pageChanged(3); } }],
                        { xtype: "Label", name: "recordCount", text: "当前到条 共条", style: { float: "right", padding: "4px 0px 0px 0px"} }
                        ]
    });
    var firstButton = bar.find("firstButton");
    var preButton = bar.find("preButton");
    var nextButton = bar.find("nextButton");
    var lastButton = bar.find("lastButton");
    var pageNoText = bar.find("pageNo");
    var pageCountLabel = bar.find("pageCount");
    var recordCountLabel = bar.find("recordCount");
    T.css = function (style) { bar.css(style); };
    T.height = function (h) { return bar.height(h); };
    T.attr = function (a, b) {
        S[a] = b;
        if (a == "pageNo") {
            pageNoText.val(b);
            firstButton.enabled(b > 1);
            preButton.enabled(b > 1);
            nextButton.enabled(b < pageCount);
            lastButton.enabled(b < pageCount);

            pageCount = Math.floor(((S.recordCount - 1) / S.pageSize) + 1);
            record1 = ((S.pageNo - 1) * S.pageSize) + 1;
            record2 = record1 + S.pageSize - 1;
            pageCountLabel.html("页，共" + pageCount + "页");
            recordCountLabel.html("当前" + record1 + "到" + record2 + "条 共" + S.recordCount + "条");


        }
        else if (a == "recordCount") {
            pageCount = Math.floor(((S.recordCount - 1) / S.pageSize) + 1);
            record1 = ((S.pageNo - 1) * S.pageSize) + 1;
            record2 = record1 + S.pageSize - 1;
            pageCountLabel.html("页，共" + pageCount + "页");
            recordCountLabel.html("当前" + record1 + "到" + record2 + "条 共" + b + "条");
        }
        return b;
    };
    T.go = function (number) {
        S.pageNo = number;
        pageChanged(3);
    };

};
$M.Control["SelectColumn"] = function (BoxID, S, CID) {
    var T=this;
    var column=BoxID.addControl({
        xtype: "DialogInput", name: S.name, inputReadOnly: true, style: { width: 120 },
        onButtonClick: function (sender, e) {
            var back = function (json) {
                sender.val(json.text);
                sender.data = json;
            };
            $M.dialog.selectColumn("", back);
        }
    });
    T.attr = function (a, b) {
        return S[a];
    };
    T.val = function (value) {
        if (value) {
            if (value == 7) {
                column.data = { text: "全部频道", id: 7 }
                column.val("全部频道");
            }else{
                $M.comm("column.columnInfo", { id: value }, function (json) {
                    column.data = { text: json.className, id: value }
                    column.val(json.className);
                });
            }
        }
        return column.data==null?0:column.data.id;
    };
};
$M.Control["PropertyGrid"] = function (BoxID, S, CID) {
    var T = this;
    S.columns = [
        { text: "name", name: "name", visible:false },
        { text: "属性", width: 150, name: "text" },
        { text: "值", width: 150, name: "value" }];
    $M.Control["GridView"].apply(this, [BoxID, S, CID]);
    S.onCellMouseClick = function (sender, e) {
        if (e.x <2) {
            T.rows[e.y].cells[2].focus();
        }
    };
    var oldAddRow = T.addRow;
    T.addRow = function (json) {
        if ($.isArray(json)) {
            for (var i = 0; i < json.length; i++) T.addRow(json[i]);
        }else{
            if (typeof (json) == "string") json = { name: json };
            if (json.xtype == null) json.xtype = "TextBox";
            var row = oldAddRow(json);
            var cell = row.cells[2];
            for(var n in json){ 
                cell.attr(n, json[n]);
            }
            row.val = function (value) {
                return cell.val(value);
            };
            return row;
        }
    };
    T.find = function (name) {
        for (var i = 0; i < T.rows.length; i++) {
            if (T.rows[i].cells[0].val() == name) {
                return T.rows[i];
            }
        }
        return null;
    };
};
$M.Control["SearchInput"] = function (BoxID, S, CID) {
    var T = this;
    var strbox = $("<div class='btn-group'></div>").appendTo(BoxID);
    var strinput=strbox.addControl({ xtype: "TextBox" });
    var numberbox=$("<div class='btn-group'></div>").appendTo(BoxID);
    var numberinput1=numberbox.addControl({ xtype: "TextBox", style: { width: "80px" } });
    numberbox.append("<span>至</span>");
    var numberinput2=numberbox.addControl({ xtype: "TextBox", style: { width: "80px" } });
    var datebox = $("<div class='btn-group'></div>").appendTo(BoxID);
    var dateinput1=datebox.addControl({ xtype: "DatePickerInput", style: { width: "120px" ,float:"left"} });
    datebox.append("<span style='float:left;'>至</span>");
    var dateinput2 = datebox.addControl({ xtype: "DatePickerInput", style: { width: "120px", float: "left" } });
    var typeIndex = 0;
    T.setShow = function (index) {
        typeIndex = index;
        strbox.hide();
        numberbox.hide();
        datebox.hide();
        if (index == 0) {
            strbox.show();
        } else if (index == 1) {
            numberbox.show();
        } else if (index == 2) {
            datebox.show();
        }
    };
    T.val = function () {
        var value = "";
        if (typeIndex == 0) {
            value = strinput.val();
        } else if (typeIndex == 1) {
            value = numberinput1.val() + "," + numberinput2.val();
        } else if (typeIndex == 2) {
            value = dateinput1.val() + "," + dateinput2.val();
        }
        return value;
    };
    T.setShow(0);

    T.container = BoxID;
    $M.BaseClass.apply(T, [S]);
};