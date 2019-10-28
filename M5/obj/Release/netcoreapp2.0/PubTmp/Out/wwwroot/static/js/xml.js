$M.xml = function (xml) {
    var T = this;
    T.documentElement = {};
    var xmlDom = null, rootDom = null, browseIE = $.browser.msie;
    var getIEXmlAX = function () {
        var i, activeXarr;
        var activeXarr = [
            "MSXML2.DOMDocument.5.0",
            "MSXML2.DOMDocument.4.0",
            "MSXML2.DOMDocument.3.0",
            "MSXML2.DOMDocument",
            "Microsoft.XmlDom"
        ];
        for (i = 0; i < activeXarr.length; i++) {
            try {
                var o = new ActiveXObject(activeXarr[i]);
                o.async = false;
                return o;
            } catch (e) { }
        }
        return false;
    };
    T.childNodes = function (element) {
        var nodes = [], node = element.childNodes;
        var C = node.length;
        for (var i = 0; i < C; i++) {
            if ((node[i].nodeType != 3)) {
                nodes[nodes.length] = {
                    node: node[i],
                    text: browseIE ? node[i].text : node[i].textContent,
                    nodeName: browseIE ? node[i].nodeName : node[i].nodeName,
                    attr: function (name, v) {
                        if (v == null) {
                            return this.node.getAttribute(name);
                        } else {
                            this.node.setAttribute(name, v);
                        }
                    },
                    childNodes: node[i].childNodes != null ? T.childNodes(node[i]) : null
                };
            }
        }
        return (nodes);
    };
    T.loadFile = function (file) {
        var o = null;
        if (browseIE) {
            o = getIEXmlAX();
            o.load(file);
        }
        else {
            var xmlhttp = new window.XMLHttpRequest();
            xmlhttp.open("GET", file, false);
            xmlhttp.send(null);
            var o = xmlhttp.responseXML;
        }
        T.documentElement = { childNodes: T.childNodes(o.documentElement) };
    };
    T.loadXML = function (source) {
        var o = null;
        if (browseIE) {
            o = getIEXmlAX();
            o.loadXML(source);

        }
        else {
            var domParser = new DOMParser();
            o = domParser.parseFromString(source, 'text/xml');
        }
        T.documentElement = { childNodes: T.childNodes(o.documentElement) };
    };
    T.addDom = function (text, dom2) {
        var obj = new Object, tagF = false;
        if (dom2 == null) tagF = true;
        if (xmlDom == null) {
            if (browseIE) {
                xmlDom = getIEXmlAX();
            }
            else {
                var domParser = new DOMParser();
                xmlDom = domParser.parseFromString('', 'text/xml');
            }
        }
        obj.addDom = function (text) { return (T.addDom(text, dom)); };
        obj.val = function (text) {
            if (browseIE) {
                dom.text = text;
            } else {
                dom.textContent = text;
            }
        };
        if (tagF) dom2 = xmlDom;
        obj.attr = function (text, text2) { dom.setAttribute(text, text2); };
        var dom = xmlDom.createElement(text);
        if (!browseIE && tagF) {
            dom2.documentElement.appendChild(dom);
        } else {
            dom2.appendChild(dom);
        }
        if (tagF) rootDom = dom;
        return (obj);
    };
    T.getXML = function () {
        return (browseIE ? rootDom.xml : (new XMLSerializer()).serializeToString(rootDom));
    };
    if (xml != null) T.loadXML(xml);
};