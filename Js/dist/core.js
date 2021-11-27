// NuScien 6 JS Core SDK
var NuScien,__classPrivateFieldGet=this&&this.__classPrivateFieldGet||function(t,e){if(!e.has(t))throw new TypeError("attempted to get private field on non-instance");return e.get(t)};!function(t){t.ver="6.0";class e{static toStr(t,s){return void 0===t?null:"json"===(s=s?s.toLowerCase():"d")||"j"===s?JSON.stringify(t)||null:"string"==typeof t?t:null===t||"string"===s||"s"===s?null:"number"==typeof t?isNaN(t)?null:t.toString(10):"symbol"==typeof t?t.toString():"text"===s||"t"===s?null:"boolean"==typeof t?t.toString():"compatible"===s||"c"===s?JSON.stringify(t)||null:"query"===s||"q"===s?t instanceof Array?t.filter(t=>void 0!==t).map(s=>encodeURIComponent(e.toStr(t[s],"c")||"")).join(","):Object.keys(t).map(s=>`${encodeURIComponent(s)}=${encodeURIComponent(e.toStr(t[s],"c")||"")}`).join("&"):"url"===s||"u"===s?t instanceof Array?t.filter(t=>void 0!==t).map(s=>encodeURIComponent(e.toStr(t[s],"c")||"")).join(","):Object.keys(t).map(s=>`${encodeURIComponent(s)}=${encodeURIComponent(e.toStr(t[s],"q")||"")}`).join("&"):t instanceof Array&&1===t.length?e.toStr(t[0]):null}static isNoNull(t,e=!1){if(!(null==t||"number"==typeof t&&isNaN(t)))return!0;if(e)throw!0===e?new Error("input is null or undefined."):e;return!1}static isString(t,e=!1){if("string"==typeof t)return!0;if(e)throw!0===e?new Error("input is not a string."):e;return!1}static isStrOrNull(t,e=!1){if("string"==typeof t||null==t)return!0;if(e)throw!0===e?new TypeError("input is not a string."):e;return!1}static isValidNumber(t,e=!1){if("number"==typeof t&&!isNaN(t))return!0;if(e)throw!0===e?new TypeError("input is not a valid number."):e;return!1}static isSafeInteger(t,e=!1){if(Number.isSafeInteger(t))return!0;if(e)throw!0===e?new TypeError("input is not a safe integer."):e;return!1}}t.Assert=e}(NuScien||(NuScien={})),"function"==typeof define?(define.amd||"undefined"!=typeof __webpack_require__)&&define(["exports"],function(t){return NuScien}):"function"==typeof require&&"object"==typeof exports&&"object"==typeof module&&(module.exports=NuScien),function(t){var e;function s(e,s,n,i){e=t.Assert.toStr(e,"t"),s=t.Assert.toStr(s,"t"),e||(e=s&&(0===s.indexOf("//")||s.indexOf("://")>0)?s:"undefined"==typeof location?"http://localhost":"/");let r=t.Assert.toStr(i,"q");if(s){let t=e[e.length-1],n=s[0];e+="/"===t&&"/"===n?s.substring(1):"/"!==t&&"/"!==n?"/"+s:s}if(n){let t=e[e.length-1],s=n[0];e+="/"===t&&"/"===s?n.substring(1):"/"!==t&&"/"!==s?"/"+n:n}return r&&(e+=(e.indexOf("?")?"?":"&")+("?"==r[0]||"&"==r[0]?r.substring(1):r)),e}function n(t,e){return fetch(t,e)}e=new WeakMap,t.Client=class{constructor(i){e.set(this,{reqInit:{}}),i||(i={});let r=t.Assert.toStr(i.clientId,"t")||"webjsclient",o=t.Assert.toStr(i.secretKey,"t"),a=t.Assert.toStr(i.host,"t"),p={},c=this.onreqinit,h=`jssdk; ${t.ver}; fetch; ${encodeURIComponent(r)};`;var l,u,d;l=__classPrivateFieldGet(this,e),u="app",d={id:()=>r,clientType:()=>h,login:e=>(e=>{let i=t.Assert.toStr(e,"u")||"";o&&(i="client_secret="+encodeURIComponent(o)+"&"+i),r&&(i="client_id="+encodeURIComponent(r)+"&"+i);let l={method:"POST",body:i,headers:{"Content-Type":"application/x-www-form-urlencoded","X-Ns-Client-Type":h}};return"function"==typeof c.login&&c.login(l),n(s(a,p.passport||"nuscien5/passport","login",null),l)})(e),logout(){let t={method:"POST",body:"",headers:{"Content-Type":"application/x-www-form-urlencoded","X-Ns-Client-Type":h}};return"function"==typeof c.logout&&c.logout(t),n(s(a,p.passport||"nuscien5/passport","logout",null),t)},url:(t,e)=>s(0===t.indexOf("//")||t.indexOf("://")>0?null:a,t,null,e),path(e,s,n){if(e)return arguments.length>1&&(n&&p[e]||(void 0===s?delete p[e]:p[e]=t.Assert.toStr(s))),p[e]},pathKeys:()=>Object.keys(p),fetch(t,e){let i=this.onreqinit;"function"==typeof i.all&&i.all(e);let r=null,o=null;return e&&(r=e.query,o=e.path),n(s(0===t.indexOf("//")||t.indexOf("://")>0?null:a,t,o,r),e)}},Object.defineProperty(l,u,{value:d,enumerable:!1})}get onreqinit(){return __classPrivateFieldGet(this,e).reqInit}get login(){let s=__classPrivateFieldGet(this,e).login;if(s)return s;let n=__classPrivateFieldGet(this,e).app;return(s=(t=>n.login(t))).alive=(()=>n.login(void 0)),s.password=((t,e,s)=>n.login(Object.assign({grant_type:"password",username:t,password:e},s))),s.refreshToken=((t,e)=>n.login(Object.assign({grant_type:"refresh_token",refresh_token:t},e))),s.authCode=((t,e)=>{let s=Object.assign({grant_type:"authorization_code",code:t},e);return s.redir&&!s.redirect_uri&&(s.redirect_uri=s.redir,delete s.redir),s.verifier&&!s.code_verifier&&(s.code_verifier=s.verifier,delete s.verifier),n.login(s)}),s.client=(t=>n.login(Object.assign({grant_type:"client_credentials"},t))),s.setAuthCode=((e,s,i)=>{let r="code="+encodeURIComponent(t.Assert.toStr(s,"t"));return i&&(r+="&insert=true"),n.fetch(n.path("passport")||"nuscien5/passport",{method:"PUT",path:"authcode/"+e,body:r,headers:{"Content-Type":"application/x-www-form-urlencoded"}})}),s.logout=(()=>n.logout()),__classPrivateFieldGet(this,e).login=s}get path(){let t=__classPrivateFieldGet(this,e).app,s=function(e,s,n){return arguments.length>1?t.path(e,s,n):t.path(e)};return s.get=(e=>t.path(e)),s.set=function(e,s,n){if("object"!=typeof e)t.path(e,s,n);else{let i=Object.keys(e);null==n&&(n=!!s);for(let e in i)t.path(e,i[e],n)}},s.remove=function(e){if(e&&e instanceof Array)for(let s in e)t.path(s,void 0);else t.path(e,void 0)},s.keys=(()=>t.pathKeys()),s}get user(){let s=__classPrivateFieldGet(this,e).app,n=__classPrivateFieldGet(this,e).user;return n||((n=function(e){return s.fetch(s.path("passport")||"nuscien5/passport",{method:"GET",path:"users/e/"+(t.Assert.toStr(e,"t")||"0")})}).exist=(e=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"POST",path:"users/exist",body:"logname="+encodeURIComponent(t.Assert.toStr(e,"t")),headers:{"Content-Type":"application/x-www-form-urlencoded"}})),n.searchByGroup=(e=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"GET",path:"users/group/"+(t.Assert.toStr(e,"t")||"0")})),n.save=(t=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"PUT",path:"users/e",body:JSON.stringify(t),headers:{"Content-Type":"application/json"}})),n.update=((e,n)=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"PUT",path:"users/e/"+(t.Assert.toStr(e,"t")||"0"),body:JSON.stringify(n),headers:{"Content-Type":"application/json"}})),n.rela=(t=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"GET",path:"rela",query:t})),n.saveRela=(t=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"PUT",path:"rela",body:JSON.stringify(t),headers:{"Content-Type":"application/json"}})),n.contact=(e=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"GET",path:"contact/"+(t.Assert.toStr(e,"t")||"0")})),n.contacts=(t=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"GET",path:"contact",query:t})),n.saveContact=(t=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"PUT",path:"contact",body:JSON.stringify(t),headers:{"Content-Type":"application/json"}})),n.updateContact=((e,n)=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"PUT",path:"contact/"+(t.Assert.toStr(e,"t")||"0"),body:JSON.stringify(n),headers:{"Content-Type":"application/json"}})),__classPrivateFieldGet(this,e).user=n)}get group(){let s=__classPrivateFieldGet(this,e).app,n=__classPrivateFieldGet(this,e).group;return n||((n=function(e){return s.fetch(s.path("passport")||"nuscien5/passport",{method:"GET",path:"groups/e/"+(t.Assert.toStr(e,"t")||"0")})}).list=(t=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"GET",path:"groups",query:t})),n.save=(t=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"PUT",path:"groups/e",body:JSON.stringify(t),headers:{"Content-Type":"application/json"}})),n.update=((e,n)=>s.fetch(s.path("passport")||"nuscien5/passport",{method:"PUT",path:"groups/e/"+(t.Assert.toStr(e,"t")||"0"),body:JSON.stringify(n),headers:{"Content-Type":"application/json"}})),n.rela=((t,e)=>{let n="rela/g/"+t;return e&&(n+="/"+e),s.fetch(s.path("passport")||"nuscien5/passport",{method:"GET",path:n})}),__classPrivateFieldGet(this,e).group=n)}get settings(){let t=__classPrivateFieldGet(this,e).app,s=__classPrivateFieldGet(this,e).settings;return s||((s=function(e,s){return t.fetch(t.path("settings")||"nuscien5/settings",{method:"GET",path:s?`global/${e}`:`site/${s}/${e}`})}).site=((e,s)=>t.fetch(t.path("settings")||"nuscien5/settings",{method:"GET",path:e?`global/${s}`:`site/${e}/${s}`})),s.setGlobal=((e,s)=>t.fetch(t.path("settings")||"nuscien5/settings",{method:"PUT",path:"global/"+e,body:JSON.stringify(s),headers:{"Content-Type":"application/json"}})),s.setSite=((e,s,n)=>t.fetch(t.path("settings")||"nuscien5/settings",{method:"PUT",path:e?`global/${s}`:`site/${e}/${s}`,body:JSON.stringify(n),headers:{"Content-Type":"application/json"}})),s.getPerm=((e,s,n)=>t.fetch(t.path("settings")||"nuscien5/settings",{method:"GET",path:`perms/${e}/${s}/${n}`})),s.setPerm=((e,s,n,i)=>t.fetch(t.path("settings")||"nuscien5/settings",{method:"PUT",path:`perms/${e}/${s}/${n}`,body:JSON.stringify(i),headers:{"Content-Type":"application/json"}})),__classPrivateFieldGet(this,e).settings=s)}get cms(){let s=__classPrivateFieldGet(this,e).app,n=__classPrivateFieldGet(this,e).cms;return n||((n=function(t){return s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:"c/"+t})}).list=((t,e)=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:"c",query:e?Object.assign(Object.assign({},e),{parent:t}):{parent:t}})),n.save=((t,e)=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"PUT",path:"c",body:JSON.stringify(e&&t?Object.assign(Object.assign({},t),{message:e}):t),headers:{"Content-Type":"application/json"}})),n.update=((e,n,i)=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"PUT",path:"c/"+(t.Assert.toStr(e,"t")||"0"),body:JSON.stringify(i&&n?Object.assign(Object.assign({},n),{message:i}):n),headers:{"Content-Type":"application/json"}})),n.revs=((t,e)=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:"c/"+t+"/rev",query:e})),n.rev=(t=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:"cr/"+t})),n.template=(t=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:"t/"+t})),n.templates=(t=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:"t",query:t})),n.saveTemplate=((t,e)=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"PUT",path:"t",body:JSON.stringify(e&&t?Object.assign(Object.assign({},t),{message:e}):t),headers:{"Content-Type":"application/json"}})),n.updateTemplate=((e,n,i)=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"PUT",path:"t/"+(t.Assert.toStr(e,"t")||"0"),body:JSON.stringify(i&&n?Object.assign(Object.assign({},n),{message:i}):n),headers:{"Content-Type":"application/json"}})),n.templateRevs=((t,e)=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:"t/"+t+"/rev",query:e})),n.templateRev=(t=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:"tr/"+t})),n.comment=(t=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:"cc/"+t})),n.comments=((t,e)=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:`c/${t}/comments`,query:e?{plain:!0}:null})),n.childComments=(t=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"GET",path:`cc/${t}/children`})),n.saveComment=(t=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"PUT",path:"cc",body:JSON.stringify(t),headers:{"Content-Type":"application/json"}})),n.updateComment=((e,n)=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"PUT",path:"cc/"+(t.Assert.toStr(e,"t")||"0"),body:JSON.stringify(n),headers:{"Content-Type":"application/json"}})),n.deleteComment=(e=>s.fetch(s.path("cms")||"nuscien5/cms",{method:"DELETE",path:"cc/"+(t.Assert.toStr(e,"t")||"0")})),__classPrivateFieldGet(this,e).cms=n)}get activities(){__classPrivateFieldGet(this,e).app;let t=__classPrivateFieldGet(this,e).activities;return t||(t=function(){},__classPrivateFieldGet(this,e).activities=t)}logout(){return __classPrivateFieldGet(this,e).app.logout()}url(t,s){return __classPrivateFieldGet(this,e).app.url(t,s)}fetch(t,s){let n=__classPrivateFieldGet(this,e).app;return"function"==typeof this.onreqinit.fetch&&this.onreqinit.fetch(s),n.fetch(t,s)}resProvider(t){return new i(__classPrivateFieldGet(this,e).app,t)}};class i{constructor(t,e){this.appInfo=t,this.path=e}search(t){return this.appInfo.fetch(this.path,{method:"GET",query:t})}get(e){return this.appInfo.fetch(this.path,{method:"GET",path:"e/"+(t.Assert.toStr(e,"t")||"0")})}save(e,s){return t.Assert.isNoNull(e,!0),s&&(s=t.Assert.toStr(s,"t"))?this.appInfo.fetch(this.path,{method:"PUT",body:JSON.stringify(e),path:"e/"+(t.Assert.toStr(s,"t")||"0"),headers:{"Content-Type":"application/json"}}):this.appInfo.fetch(this.path,{method:"PUT",body:JSON.stringify(e),headers:{"Content-Type":"application/json"}})}delete(e){return this.appInfo.fetch(this.path,{method:"DELETE",path:"e/"+(t.Assert.toStr(e,"t")||"0")})}fetch(t,e){let s=this.path||t;return s&&t&&("/"===s[s.length-1]&&"/"===t[0]?s+=t.substring(1):"/"!==s[s.length-1]&&"/"!==t[0]?s+="/"+t:s+=t),this.appInfo.fetch(s,e)}}t.ResourceEntityProvider=i}(NuScien||(NuScien={}));