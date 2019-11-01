//$M.Config.sysPath="/manage/";
//$M.Config.sysPath="/manage/";
$M.config = {
    webPath:"/",
    sysAppPath:"system/",
    appPath: "/manage/app/",
    operationTimeDelay:100
};
if (typeof (ZeroClipboard) != "undefined") ZeroClipboard.setMoviePath("../static/js/ZeroClipboard.swf");
$M.tab = {};
$M.comm = function (command, back, errback) {
    var commandList = [];
    var errdList = [];
    var runComm = function (appName, data, _back, _errback, index) {
        var path = appName.split('.');
        var url = $M.config.appPath;
        if (data == null) data = {};
        if (path.length == 1) {
            data["_m"] = appName;
            //url = $M.config.sysAppPath + "ajax.ashx";
            url = $M.config.sysAppPath + appName;
        } else if (path.length == 22) {
            data["_m"] = path[1];
           // url = $M.config.sysAppPath + path[0] + ".ashx";
            url = $M.config.sysAppPath + path[0] + "/" + path[1];

        } else {
            for (var i = 0; i < path.length - 2; i++) {
                url += path[i] + "/";
            }
            //url += path[path.length - 2] + ".ashx";
            url += path[path.length - 2] + "/" + path[path.length - 1];
            data["_m"] = path[path.length - 1];
        }
        $M.ajax(url, data, function (json) {
            if (index != null) commandList[index] = json.userData;
            if (json.errNo > -1) {
                if (_back) _back(json.userData);
            } else {
                if(json.errNo==-1000){window.document.location.reload();return;}
                if (index != null) errdList[errdList.length] = { index: index, errNo: json.errNo, errMsg: json.errMsg };
                if (_errback) {
                    _errback(json);
                } else {
                    $M.alert(json.errMsg);
                }
            }
        }, function (json) {
            if (_errback) {
                _errback(json);
            } else {
                $M.alert(json.errMsg);
            }
        });
    };
    var _commandBack = function () { //命令执行完成
        if (endComm == commCount) {//全部完成
            if (back) back(commandList);
            if (errback) errback(errList);
        }
    };
    if (typeof (arguments[0]) != "string") {
        var commCount = command.length; //命令总数
        var endComm = 0; //已完成命令
        for (var i = 0; i < commCount; i++) {
            runComm(
                command[i][0],
                command[i][1],
                function (json) { endComm++; _commandBack(); },
                function (json) { endComm++; _commandBack(); },
                i
                );
        }
    } else {
        runComm(
                arguments[0],
                arguments[1],
                arguments[2],
                arguments[3],
                i
                );
    }
};
$M.app = {};
$M.app.loadIncludeFile = function (app, back) {
    var files = [];
    if (typeof (app.include) == "string") {
        files = [$M.config.appPath + app.name + "/" + app.include];
    } else {
        for (var i = 0; i < app.include.length; i++) {
            files[i] = $M.config.appPath + app.name + "/" + app.include[i];
        }
    }
    $M.loadJs(files, back);
};
$M.app.interface = function (interfaceName, dataTypeId, back) {
    var app = null;
    for (var i = 0; i < _appReg.reg.app.length; i++) {
        if (_appReg.reg.app[i].dataTypeId == dataTypeId) app = _appReg.reg.app[i];
    }
    if (app == null) { back(null); return;}
    var f = $M[app.name];
    if (f) {
        back(f[interfaceName]);
    } else {
        $M[app.name] = {};
        $M.app.loadIncludeFile(app, function () {
            back($M[app.name][interfaceName]);
        });
    };
};
$M.app.call = function (path, json) {
    var p = path.split(".");
    if (p[0] == "$M") {
        $M.app.openFunction(p[1], p[2], json);
    } else {
        alert("非法函数调用");
    }
};
$M.app.openFunction = function (appName, fname, json) {
    var app = null;
    for (var i = 0; i < _appReg.reg.app.length; i++) {
        if (_appReg.reg.app[i].name == appName) app = _appReg.reg.app[i];
    }
    var f = $M[app.name];
    if (f) {
        f[fname](json);
    } else {
        $M[app.name] = {};
        $M.app.loadIncludeFile(app, function () {
            $M[app.name][fname](json);
        });
    };
};
$M.loadInterface=function(_p,_dataTypeId,onClick){
    if(_interface.interface==null)return;
            var _Extension = null;
            var _function=_interface.interface.function;
            if (_function.length > 0) {
                _Extension = [];
                for (var i = 0; i < _function.length; i++) {
                    var parameter=_function[i].parameter.split(",");
                    var flag=true;
                    for(var i1=0;i1<parameter.length;i1++){
                        if(_p.indexOf(parameter[i1])==-1)flag=false;
                    }
                    if(_function[i].dataTypeId!=null && _function[i].dataTypeId!=_dataTypeId)flag=false;
                    if(flag){
                        var name=_function[i].name;
                        _Extension[_Extension.length] = {
                            ico:_function[i].ico,
                            text:_function[i].text,
                            fName:name,
                            onClick:function(sender,e){
                                if(onClick)onClick(e.attr("fName"));
                            }
                            };
                    }
                }
                if(_Extension.length==0)_Extension=null;
            }
            return _Extension;
        };