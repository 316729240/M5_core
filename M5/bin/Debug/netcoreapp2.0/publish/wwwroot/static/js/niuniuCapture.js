
var niuniuCapture = null;
var savedPictureContent = '';
var extendName = '';

var WatermarkPicturePath = "";
var WatermarkTextValue = "";
var _CaptureFinished = null;
/*******************************************************************************/
//设置截图的参数  
var emPensize = 1;		//设置画笔大小
var emDrawType = 2;		//设置是腾讯风格还是360风格 0： 腾讯风格   1： 360风格
var emTrackColor = 3;		//自动识别的边框的颜色
var emEditBorderColor = 4;	//文本输入的边框颜色
var emTransparent = 5;		//工具栏的透明度
var emWindowAware = 6;		//设置是否禁用随着DPI放大
var emSetSaveName = 8;		//设置保存时的开始文字     免费版本无效
var emSetMagnifierBkColor = 9; //设置放大镜的背景色，不设置则透明
var emSetMagnifierLogoText = 10; //设置放大镜上的LOGO字符，可提示快捷键，如： 牛牛截图(CTRL + SHIFT + A)     免费版本无效
var emSetWatermarkPictureType = 20;						//设置水印的类型 
var emSetWatermarkPicturePath = 21;						//设置水印的路径 
var emSetWatermarkTextType = 22;						//设置水印文字的类型 
var emSetWatermarkTextValue = 23;                       //设置水印文字的字符串

/*******************************************************************************/

function OnCaptureFinished(x, y, width, height, content, localpath) {
    if (_CaptureFinished) _CaptureFinished(content);

}
function OnCaptureFinishedEx(type, x, y, width, height, info, content, localpath) {
    if (type == 1 || type==4) {
        if (_CaptureFinished) _CaptureFinished(content);
        //保存图片
    }
}

function pluginValid() {
    if (niuniuCapture[0].valid) {
        return true;
    }
    return false;
}

function pluginLoaded() {
    //此处可以通过niuniuCapture[0].GetLocation()获取控件的路径，可心通过GetVersion获取版本号
    var myencodedauth = "niuniu";
    //myencodedauth = $.md5("myauth");

    //此函数必需调用，传递正确的参数，且必需先于其他函数调用  
    niuniuCapture[0].InitCapture(myencodedauth);

    niuniuCapture[0].InitParam(emPensize, 2);
    niuniuCapture[0].InitParam(emDrawType, 1);
    niuniuCapture[0].InitParam(emTrackColor, rgb2value(255, 0, 0));
    niuniuCapture[0].InitParam(emEditBorderColor, rgb2value(0, 0, 255));
    niuniuCapture[0].InitParam(emTransparent, 220);


    function addEvent(obj, name, func) {
        if (obj.attachEvent) {
            obj.attachEvent("on" + name, func);
        } else {
            obj.addEventListener(name, func, false);
        }
    };
    //添加控件的事件监听 
    addEvent(niuniuCapture[0], 'CaptureFinishedEx', OnCaptureFinishedEx);
    //以下这个事件主要是用于兼容旧的浏览器控件的事件通知
    addEvent(niuniuCapture[0], 'CaptureFinished', OnCaptureFinished);
}

function LoadPlugin() {
    niuniuCapture = $('<object type="application/x-niuniuwebcapture" width="0" height="0"><param name="onload" value="pluginLoaded" /></object>').appendTo($(document.body));
}



function ShowChromeInstallDownload() {
    if (window.confirm("您需要先下载Chrome扩展安装包,是否确定现在下载安装")) location.href = "http://www.ggniu.cn/download/CaptureInstallChrome.exe";

}
function DoCaptureForChrome(name, hide, AutoCapture, x, y, width, height) {
    var obj = NewCaptureParamObject(name, hide, AutoCapture, x, y, width, height);
    try {
        var json = $.toJSON(obj);

        var CrxEventFlag = 'NiuniuCaptureEvent';
        var objFlag = $('#' + CrxEventFlag);
        if (objFlag.length < 1) {
            ShowChromeInstallDownload();
            return;
        }
        else {
            var evt = document.createEvent("CustomEvent");
            evt.initCustomEvent(CrxEventFlag, true, false, json);
            document.dispatchEvent(evt);
        }
    }
    catch (e) {
        ShowChromeInstallDownload();
    }
}

function IsNeedCrx() {
    var isChrome = IsRealChrome();
    var chromeMainVersion = GetChromeMainVersion();
    if (isChrome && chromeMainVersion > 41) {
        return true;
    }
    return false;
}

function DoCapture(name, hide, AutoCapture, x, y, width, height) {
    if (IsNeedCrx()) {
        DoCaptureForChrome(name, hide, AutoCapture, parseInt(x), parseInt(y), parseInt(width), parseInt(height));
        return;
    }

    if (pluginValid()) {

        niuniuCapture[0].Capture(name, hide, AutoCapture, x, y, width, height);
    }
    else {
        if (window.confirm("没有安装截图插件是否确定现在下载安装")) location.href = "http://www.ggniu.cn/download/CaptureInstall.exe";
    }
}

$M.captureScreen = function (type, back) {
    _CaptureFinished = back;
    if (type == 0) {//不隐藏窗口截取
        DoCapture("1.jpg", 0, 0, 0, 0, 0, 0);
    } else if (type == 1) {//隐藏窗口截取
        DoCapture("1.jpg", 1, 0, 0, 0, 0, 0);
    } else {//粘贴内存中的图片
        DoCapture("", 0, 4, 0, 0, 0, 0);
    }
}

function rgb2value(r, g, b) {
    return r | g << 8 | b << 16;
}


function NewCaptureParamObject(defaultpath, hideCurrWindow, autoCaptureFlag, x, y, width, height) {
    var obj = new Object();
    obj.IsGBK = 0;				//是否是GBK编码，这样会涉及到编码转换  
    obj.AuthKey = "niuniu";  //						
    obj.Pensize = 2;		//设置画笔大小
    obj.DrawType = 0;			//设置是腾讯风格还是360风格
    obj.TrackColor = rgb2value(255, 0, 0);		//自动识别的边框的颜色
    obj.EditBorderColor = rgb2value(0, 255, 0);	//文本输入的边框颜色
    obj.Transparent = 230;		//工具栏的透明度
    obj.SetSaveName = "牛牛截图";									//设置保存时的开始文字
    obj.SetMagnifierLogoText = "牛牛截图";						//设置放大镜上的LOGO字符   
    obj.SetWatermarkPictureType = 3;						//设置水印的类型 
    obj.SetWatermarkPicturePath = WatermarkPicturePath;						//设置水印的路径 
    obj.SetWatermarkTextType = 1;							//设置水印文字的类型 
    obj.SetWatermarkTextValue = WatermarkTextValue;						//设置水印文字


    //以下是截图时传递的参数 
    obj.DefaultPath = defaultpath;
    obj.HideCurrentWindow = hideCurrWindow;
    obj.AutoCaptureFlag = autoCaptureFlag;
    obj.x = x;
    obj.y = y;
    obj.Width = width;
    obj.Height = height;
    return obj;
}

//此函数用于绑定在Chrome42以上的版本时，扩展在截图完成后进行事件通知的处理 
function BindChromeCallback() {
    document.addEventListener('NiuniuCaptureEventCallBack', function (evt) {
        var _aoResult = evt.detail;
        if (_aoResult.Result == -2) {
            ShowChromeInstallDownload();
        }
        if (_aoResult.Result != -1) {
            OnCaptureFinishedEx(_aoResult.Type, _aoResult.x, _aoResult.y, _aoResult.Width, _aoResult.Height, _aoResult.Info, _aoResult.Content, _aoResult.LocalPath);
        }
        else {
            alert("出错：" + _aoResult.Info);
        }
    });
}

function InitNiuniuCapture() {
    if (!IsNeedCrx()) {
        LoadPlugin();
    }
    else {
        BindChromeCallback();
    }
}

function IsRealChrome() {
    try {
        var isChrome = window.navigator.userAgent.indexOf("Chrome") != -1;
        if (isChrome) {
            if (chrome && chrome.runtime) {
                return true;
            }
        }
        return false;
    }
    catch (e) {
    }
    return false;
}

function GetChromeMainVersion() {
    var gsAgent = navigator.userAgent.toLowerCase();
    var gsChromeVer = "" + (/chrome\/((\d|\.)+)/i.test(gsAgent) && RegExp["$1"]);

    if (gsChromeVer != "false")
        return parseInt(gsChromeVer);
    return 0;
    //return gsChromeVer;
}
$().ready(InitNiuniuCapture);
