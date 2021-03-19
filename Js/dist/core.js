var __classPrivateFieldGet = (this && this.__classPrivateFieldGet) || function (receiver, privateMap) {
    if (!privateMap.has(receiver)) {
        throw new TypeError("attempted to get private field on non-instance");
    }
    return privateMap.get(receiver);
};
/**
 * NuScien core library for front-end (web client).
 * https://github.com/nuscien/trivial
 * Copyright (c) 2020 Kingcean Tuan. All rights reserved.
 */
var NuScien;
(function (NuScien) {
    /**
     * The main version.
     */
    NuScien.ver = "5.0";
    /**
     * Assert helper.
     */
    class Assert {
        /**
         * Converts to string. Or returns null for unsupported type.
         * @param obj  The source object.
         * @param level  The options.
         *  - "default" or null, to convert the object to string in normal way;
         *  - "text" to convert string, number or symbol only;
         *  - "compatible" to convert basic types and stringify object and array;
         *  - "json" to stringify in JSON format;
         *  - "query" to stringify in URI query component format;
         *  - "url" to stringify in URI query format;
         *  - "string" to pass only for string.
         */
        static toStr(obj, level) {
            if (typeof obj === "undefined")
                return null;
            if (!level)
                level = "d";
            else
                level = level.toLowerCase();
            if (level === "json" || level === "j")
                return JSON.stringify(obj) || null;
            if (typeof obj === "string")
                return obj;
            if (obj === null || level === "string" || level === "s")
                return null;
            if (typeof obj === "number")
                return isNaN(obj) ? null : obj.toString(10);
            if (typeof obj === "symbol")
                return obj.toString();
            if (level === "text" || level === "t")
                return null;
            if (typeof obj === "boolean")
                return obj.toString();
            if (level === "compatible" || level === "c")
                return JSON.stringify(obj) || null;
            if (level === "query" || level === "q") {
                if (obj instanceof Array)
                    return obj.filter(ele => typeof ele !== "undefined").map(ele => encodeURIComponent(Assert.toStr(obj[ele], "c") || "")).join(",");
                return Object.keys(obj).map(ele => `${encodeURIComponent(ele)}=${encodeURIComponent(Assert.toStr(obj[ele], "c") || "")}`).join("&");
            }
            if (level === "url" || level === "u") {
                if (obj instanceof Array)
                    return obj.filter(ele => typeof ele !== "undefined").map(ele => encodeURIComponent(Assert.toStr(obj[ele], "c") || "")).join(",");
                return Object.keys(obj).map(ele => `${encodeURIComponent(ele)}=${encodeURIComponent(Assert.toStr(obj[ele], "q") || "")}`).join("&");
            }
            return obj instanceof Array && obj.length === 1 ? Assert.toStr(obj[0]) : null;
        }
        static isNoNull(input, throwIfFailure = false) {
            var isNull = typeof input === "undefined" || input === null || (typeof input === "number" && isNaN(input));
            if (!isNull)
                return true;
            if (throwIfFailure)
                throw throwIfFailure === true ? new Error("input is null or undefined.") : throwIfFailure;
            return false;
        }
        static isString(input, throwIfFailure = false) {
            var isStr = typeof input === "string";
            if (isStr)
                return true;
            if (throwIfFailure)
                throw throwIfFailure === true ? new Error("input is not a string.") : throwIfFailure;
            return false;
        }
        static isStrOrNull(input, throwIfFailure = false) {
            var isStr = typeof input === "string" || typeof input === "undefined" || input === null;
            if (isStr)
                return true;
            if (throwIfFailure)
                throw throwIfFailure === true ? new TypeError("input is not a string.") : throwIfFailure;
            return false;
        }
        static isValidNumber(input, throwIfFailure = false) {
            var isNum = typeof input === "number" && !isNaN(input);
            if (isNum)
                return true;
            if (throwIfFailure)
                throw throwIfFailure === true ? new TypeError("input is not a valid number.") : throwIfFailure;
            return false;
        }
        static isSafeInteger(input, throwIfFailure = false) {
            if (Number.isSafeInteger(input))
                return true;
            if (throwIfFailure)
                throw throwIfFailure === true ? new TypeError("input is not a safe integer.") : throwIfFailure;
            return false;
        }
    }
    NuScien.Assert = Assert;
})(NuScien || (NuScien = {}));
/// <reference path="./core.ts" />
// For asynchronous modules loaders.
(function () {
    if (typeof define === 'function') {
        if (define.amd || typeof __webpack_require__ !== "undefined") {
            define(["exports"], function (exports) {
                return NuScien;
            });
        }
    }
    else if (typeof require === "function" && typeof exports === "object" && typeof module === "object") {
        module["exports"] = NuScien;
    }
})();
/// <reference path="./main.ts" />
var NuScien;
(function (NuScien) {
    var _internalModelServices;
    function defineObjectProperty(obj, key, value) {
        Object.defineProperty(obj, key, { value, enumerable: false });
    }
    function defineAccessorProperty(obj, key, getter, setter) {
        Object.defineProperty(obj, key, { get: getter, set: setter, enumerable: false });
    }
    function appendUrl(host, path, path2, query) {
        host = NuScien.Assert.toStr(host, "t");
        path = NuScien.Assert.toStr(path, "t");
        if (!host)
            host = path && (path.indexOf("//") === 0 || path.indexOf("://") > 0) ? path : (typeof location === "undefined" ? "http://localhost" : "/");
        let q = NuScien.Assert.toStr(query, "q");
        if (path) {
            let last = host[host.length - 1];
            let first = path[0];
            if (last === "/" && first === "/")
                host += path.substring(1);
            else if (last !== "/" && first !== "/")
                host += "/" + path;
            else
                host += path;
        }
        if (path2) {
            let last = host[host.length - 1];
            let first = path2[0];
            if (last === "/" && first === "/")
                host += path2.substring(1);
            else if (last !== "/" && first !== "/")
                host += "/" + path2;
            else
                host += path2;
        }
        if (q)
            host += (host.indexOf("?") ? "?" : "&") + (q[0] == "?" || q[0] == "&" ? q.substring(1) : q);
        return host;
    }
    function fetchImpl(path, reqInit) {
        return fetch(path, reqInit);
    }
    /**
     * The resource access client.
     */
    class Client {
        /**
         * Initializes a new instance of the Client class.
         * @param options  The options containing the client information.
         */
        constructor(options) {
            _internalModelServices.set(this, {
                reqInit: {}
            });
            if (!options)
                options = {};
            let clientId = NuScien.Assert.toStr(options.clientId, "t") || "webjsclient";
            let secretKey = NuScien.Assert.toStr(options.secretKey, "t");
            let host = NuScien.Assert.toStr(options.host, "t");
            let pathes = {};
            let onreq = this.onreqinit;
            let clientTypeStr = `jssdk; ${NuScien.ver}; fetch; ${encodeURIComponent(clientId)};`;
            let getLoginInit = (body) => {
                let s = NuScien.Assert.toStr(body, "u") || "";
                if (secretKey)
                    s = "client_secret=" + encodeURIComponent(secretKey) + "&" + s;
                if (clientId)
                    s = "client_id=" + encodeURIComponent(clientId) + "&" + s;
                let init = {
                    method: "POST",
                    body: s,
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        "X-Ns-Client-Type": clientTypeStr
                    }
                };
                if (typeof onreq.login === "function")
                    onreq.login(init);
                let resp = fetchImpl(appendUrl(host, pathes.passport || "nuscien5/passport", "login", null), init);
                return resp;
            };
            defineObjectProperty(__classPrivateFieldGet(this, _internalModelServices), "app", {
                id() {
                    return clientId;
                },
                clientType() {
                    return clientTypeStr;
                },
                login(body) {
                    return getLoginInit(body);
                },
                logout() {
                    let init = {
                        method: "POST",
                        body: "",
                        headers: {
                            "Content-Type": "application/x-www-form-urlencoded",
                            "X-Ns-Client-Type": clientTypeStr
                        }
                    };
                    if (typeof onreq.logout === "function")
                        onreq.logout(init);
                    let resp = fetchImpl(appendUrl(host, pathes.passport || "nuscien5/passport", "logout", null), init);
                    return resp;
                },
                url(path, query) {
                    return appendUrl(path.indexOf("//") === 0 || path.indexOf("://") > 0 ? null : host, path, null, query);
                },
                path(key, value, skipIfExist) {
                    if (!key)
                        return undefined;
                    if (arguments.length > 1) {
                        if (!skipIfExist || !pathes[key]) {
                            if (typeof value === "undefined")
                                delete pathes[key];
                            else
                                pathes[key] = NuScien.Assert.toStr(value);
                        }
                    }
                    return pathes[key];
                },
                pathKeys() {
                    return Object.keys(pathes);
                },
                fetch(path, reqInit) {
                    let onreq = this.onreqinit;
                    if (typeof onreq.all === "function")
                        onreq.all(reqInit);
                    let query = null;
                    let subFolder = null;
                    if (reqInit) {
                        query = reqInit.query;
                        subFolder = reqInit.path;
                        //if (query || subFolder) {
                        //    let reqInit2 = { ...reqInit };
                        //    if (reqInit.query === query && reqInit.path === subFolder) {
                        //        reqInit = reqInit2;
                        //        delete reqInit.query;
                        //        delete reqInit.path;
                        //    }
                        //}
                    }
                    let url = appendUrl(path.indexOf("//") === 0 || path.indexOf("://") > 0 ? null : host, path, subFolder, query);
                    return fetchImpl(url, reqInit);
                }
            });
        }
        get onreqinit() {
            return __classPrivateFieldGet(this, _internalModelServices).reqInit;
        }
        /**
         * Signs in.
         */
        get login() {
            let m = __classPrivateFieldGet(this, _internalModelServices).login;
            if (m)
                return m;
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            m = ((req) => appInfo.login(req));
            m.alive = () => appInfo.login(undefined);
            m.password = (username, password, options) => appInfo.login(Object.assign({ grant_type: "password", username,
                password }, options));
            m.refreshToken = (refreshToken, options) => appInfo.login(Object.assign({ grant_type: "refresh_token", refresh_token: refreshToken }, options));
            m.authCode = (code, options) => {
                let data = Object.assign({ grant_type: "authorization_code", code }, options);
                if (data.redir && !data.redirect_uri) {
                    data.redirect_uri = data.redir;
                    delete data.redir;
                }
                if (data.verifier && !data.code_verifier) {
                    data.code_verifier = data.verifier;
                    delete data.verifier;
                }
                return appInfo.login(data);
            };
            m.client = (options) => {
                return appInfo.login(Object.assign({ grant_type: "client_credentials" }, options));
            };
            m.setAuthCode = (serviceProvider, code, insert) => {
                let body = "code=" + encodeURIComponent(NuScien.Assert.toStr(code, "t"));
                if (insert)
                    body += "&insert=true";
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "authcode/" + serviceProvider,
                    body: body,
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    }
                });
            };
            m.logout = () => {
                return appInfo.logout();
            };
            return __classPrivateFieldGet(this, _internalModelServices).login = m;
        }
        /**
         * Gets a specific path.
         */
        get path() {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            let p = function (key, value, skipIfExist) {
                if (arguments.length > 1)
                    return appInfo.path(key, value, skipIfExist);
                return appInfo.path(key);
            };
            p.get = (key) => appInfo.path(key);
            p.set = function (key, value, skipIfExist) {
                if (typeof key === "object") {
                    let keys = Object.keys(key);
                    if (typeof skipIfExist === "undefined" || skipIfExist === null)
                        skipIfExist = !!value;
                    for (let k in keys) {
                        appInfo.path(k, keys[k], skipIfExist);
                    }
                    return;
                }
                appInfo.path(key, value, skipIfExist);
            };
            p.remove = function (key) {
                if (key && key instanceof Array) {
                    for (let item in key) {
                        appInfo.path(item, undefined);
                    }
                    return;
                }
                appInfo.path(key, undefined);
            };
            p.keys = () => appInfo.pathKeys();
            return p;
        }
        /**
         * User resources.
         */
        get user() {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            let m = __classPrivateFieldGet(this, _internalModelServices).user;
            if (m)
                return m;
            m = function (id) {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "users/e/" + (NuScien.Assert.toStr(id, "t") || "0")
                });
            };
            m.exist = (name) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "POST",
                    path: "users/exist",
                    body: "logname=" + encodeURIComponent(NuScien.Assert.toStr(name, "t")),
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    }
                });
            };
            m.searchByGroup = (id) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "users/group/" + (NuScien.Assert.toStr(id, "t") || "0")
                });
            };
            m.save = (entity) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "users/e",
                    body: JSON.stringify(entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.update = (id, data) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "users/e/" + (NuScien.Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.rela = (q) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "rela",
                    query: q
                });
            };
            m.saveRela = (data) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "rela",
                    body: JSON.stringify(data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.contact = (id) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "contact/" + (NuScien.Assert.toStr(id, "t") || "0")
                });
            };
            m.contacts = (q) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "contact",
                    query: q
                });
            };
            m.saveContact = (entity) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "contact",
                    body: JSON.stringify(entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.updateContact = (id, data) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "contact/" + (NuScien.Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            return __classPrivateFieldGet(this, _internalModelServices).user = m;
        }
        /**
         * User group resources.
         */
        get group() {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            let m = __classPrivateFieldGet(this, _internalModelServices).group;
            if (m)
                return m;
            m = function (id) {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "groups/e/" + (NuScien.Assert.toStr(id, "t") || "0")
                });
            };
            m.list = (q) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "groups",
                    query: q
                });
            };
            m.save = (entity) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "groups/e",
                    body: JSON.stringify(entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.update = (id, data) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "groups/e/" + (NuScien.Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.rela = (group, user) => {
                let path = "rela/g/" + group;
                if (user)
                    path += "/" + user;
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: path
                });
            };
            return __classPrivateFieldGet(this, _internalModelServices).group = m;
        }
        /**
         * Settings.
         */
        get settings() {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            let m = __classPrivateFieldGet(this, _internalModelServices).settings;
            if (m)
                return m;
            m = function (key, site) {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "GET",
                    path: site ? `global/${key}` : `site/${site}/${key}`
                });
            };
            m.site = (site, key) => {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "GET",
                    path: site ? `global/${key}` : `site/${site}/${key}`
                });
            };
            m.setGlobal = (key, value) => {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "PUT",
                    path: "global/" + key,
                    body: JSON.stringify(value),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.setSite = (site, key, value) => {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "PUT",
                    path: site ? `global/${key}` : `site/${site}/${key}`,
                    body: JSON.stringify(value),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.getPerm = (site, targetType, targetId) => {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "GET",
                    path: `perms/${site}/${targetType}/${targetId}`
                });
            };
            m.setPerm = (site, targetType, targetId, value) => {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "PUT",
                    path: `perms/${site}/${targetType}/${targetId}`,
                    body: JSON.stringify(value),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            return __classPrivateFieldGet(this, _internalModelServices).settings = m;
        }
        /**
         * CMS.
         */
        get cms() {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            let m = __classPrivateFieldGet(this, _internalModelServices).cms;
            if (m)
                return m;
            m = function (id) {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "c/" + id
                });
            };
            m.list = (id, q) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "c",
                    query: q ? Object.assign(Object.assign({}, q), { parent: id }) : { parent: id }
                });
            };
            m.save = (entity, message) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "c",
                    body: JSON.stringify(message && entity ? Object.assign(Object.assign({}, entity), { message }) : entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.update = (id, data, message) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "c/" + (NuScien.Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(message && data ? Object.assign(Object.assign({}, data), { message }) : data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.revs = (parent, q) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "c/" + parent + "/rev",
                    query: q
                });
            };
            m.rev = (id) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "cr/" + id
                });
            };
            m.template = (id) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "t/" + id
                });
            };
            m.templates = (q) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "t",
                    query: q
                });
            };
            m.saveTemplate = (entity, message) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "t",
                    body: JSON.stringify(message && entity ? Object.assign(Object.assign({}, entity), { message }) : entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.updateTemplate = (id, data, message) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "t/" + (NuScien.Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(message && data ? Object.assign(Object.assign({}, data), { message }) : data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.templateRevs = (parent, q) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "t/" + parent + "/rev",
                    query: q
                });
            };
            m.templateRev = (id) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "tr/" + id
                });
            };
            m.comment = (id) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "cc/" + id
                });
            };
            m.comments = (content, plain) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: `c/${content}/comments`,
                    query: plain ? { plain: true } : null
                });
            };
            m.childComments = (parent) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: `cc/${parent}/children`
                });
            };
            m.saveComment = (entity) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "cc",
                    body: JSON.stringify(entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.updateComment = (id, data) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "cc/" + (NuScien.Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.deleteComment = (id) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "DELETE",
                    path: "cc/" + (NuScien.Assert.toStr(id, "t") || "0")
                });
            };
            return __classPrivateFieldGet(this, _internalModelServices).cms = m;
        }
        /**
         * User activities.
         */
        get activities() {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            let m = __classPrivateFieldGet(this, _internalModelServices).activities;
            if (m)
                return m;
            m = function () {
            };
            return __classPrivateFieldGet(this, _internalModelServices).activities = m;
        }
        /**
         * Signs out.
         */
        logout() {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            return appInfo.logout();
        }
        /**
         * Gets a URL.
         * @param path  The relative path.
         * @param query  The query data.
         */
        url(path, query) {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            return appInfo.url(path, query);
        }
        /**
         * Sends request to service and gets response.
         * @param path  The relative path.
         * @param reqInit  The options.
         */
        fetch(path, reqInit) {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            if (typeof this.onreqinit.fetch === "function")
                this.onreqinit.fetch(reqInit);
            return appInfo.fetch(path, reqInit);
        }
        /**
         * Gets the resource entity provider.
         * @param path  The relative root path.
         */
        resProvider(path) {
            return new ResourceEntityProvider(__classPrivateFieldGet(this, _internalModelServices).app, path);
        }
    }
    _internalModelServices = new WeakMap();
    NuScien.Client = Client;
    /**
     * The resource entity provider.
     */
    class ResourceEntityProvider {
        /**
         * Initializes a new instance of the ResourceEntityProvider class.
         * @param appInfo  The client proxy.
         * @param path  The relative root path.
         */
        constructor(appInfo, path) {
            this.appInfo = appInfo;
            this.path = path;
        }
        /**
         * Searches.
         * @param q  The query.
         */
        search(q) {
            return this.appInfo.fetch(this.path, {
                method: "GET",
                query: q
            });
        }
        /**
         * Gets a resource entity.
         * @param id  The entity identifier.
         */
        get(id) {
            return this.appInfo.fetch(this.path, {
                method: "GET",
                path: "e/" + (NuScien.Assert.toStr(id, "t") || "0")
            });
        }
        /**
         * Creates or updates a specific resource entity.
         * @param value  The entity to save or the content to delta update.
         * @param id  The optional entity identifier. Only set this parameter when need delta update.
         */
        save(value, id) {
            NuScien.Assert.isNoNull(value, true);
            if (id) {
                id = NuScien.Assert.toStr(id, "t");
                if (id)
                    return this.appInfo.fetch(this.path, {
                        method: "PUT",
                        body: JSON.stringify(value),
                        path: "e/" + (NuScien.Assert.toStr(id, "t") || "0"),
                        headers: {
                            "Content-Type": "application/json"
                        }
                    });
            }
            return this.appInfo.fetch(this.path, {
                method: "PUT",
                body: JSON.stringify(value),
                headers: {
                    "Content-Type": "application/json"
                }
            });
        }
        /**
         * Deletes a specific resource entity.
         * @param id  The entity identifier.
         */
        delete(id) {
            return this.appInfo.fetch(this.path, {
                method: "DELETE",
                path: "e/" + (NuScien.Assert.toStr(id, "t") || "0")
            });
        }
        /**
         * Sends request to service and gets response.
         * @param path  The relative path.
         * @param reqInit  The options.
         */
        fetch(subPath, reqInit) {
            let url = this.path || subPath;
            if (url && subPath) {
                if (url[url.length - 1] === "/" && subPath[0] === "/")
                    url += subPath.substring(1);
                else if (url[url.length - 1] !== "/" && subPath[0] !== "/")
                    url += "/" + subPath;
                else
                    url += subPath;
            }
            return this.appInfo.fetch(url, reqInit);
        }
    }
    NuScien.ResourceEntityProvider = ResourceEntityProvider;
})(NuScien || (NuScien = {}));
//# sourceMappingURL=core.js.map