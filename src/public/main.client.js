'use strict';

const ServerClientMessage = {
    RequestLogin: 'req:login',
    CustomizeCharacter: 'req:customizeChar',
    ConfirmCustomizeCharacter: 'req:customizeChar:ok',
    CancelCustomizeCharacter: 'req:customizeChar:cancel'
};

const UserGroups = {
    Guest: 0,
    Player: 1,
    GameMaster: 100,
    Admin: 200
};

const ClientEvents = {
    'enterFreeroam': function __enterFreeroam() {
        client.cursor = false;
        client.resetCamera();
        browser.removeView('view-login');
        browser.removeView('view-character-customization');
        browser.addView('view-status', { displayCoordinates: Client.hasRequiredUserGroup(UserGroups.GameMaster) });
    },
    '::display:login': function __display_login() {
        client.cursor = true;
        client.setCamera(200, 200, 150, 1, 1, 1);
        browser.addView('view-login');
    },
    '::display:characterCustomization': function __display_characterCustomization(data) {
        client.cursor = true;
        client.setCameraToViewPlayer();
        let freeroamCustomizationData = deserializeFromDesignatedJson(data);
        browser.addView('view-character-customization', { current: characterCustomization, customization: freeroamCustomizationData });
    },
    'res:login': function __login_response(success, data) {
        data = deserializeFromDesignatedJson(data);
        if (success) {
            client.cursor = false;
            client.resetCamera();
            browser.removeView('view-login');
            user = data.User;
            character = data.Character;
            characterCustomization = data.CharacterCustomization;
        } else {
            data = {};
        }
        data.success = success;
        browser.raiseEventInBrowser('res:login', data);
    },
    'res:customizeChar': function __customizeChar_response(success, data) {
        data = deserializeFromDesignatedJson(data);
        if (!success) {
            data = {};
        }
        data.success = success;
        browser.raiseEventInBrowser('res:customizeChar', data);
    }
};

const BrowserEvents = {
    'disconnect': function __disconnect(reason) {
        API.disconnect(reason);
    },
    'login': function __login(username, password) {
        sendToServer(ServerClientMessage.RequestLogin, argumentsToArray(arguments));
    },
    'customizeCharacter': function __customizeChar(what, value) {
        sendToServer(ServerClientMessage.CustomizeCharacter, argumentsToArray(arguments));
    },
    'confirmCharacterCustomization': function __confirmCustomizeChar() {
        sendToServer(ServerClientMessage.ConfirmCustomizeCharacter);
    },
    'cancelCharacterCustomization': function __cancelCustomizeChar() {
        sendToServer(ServerClientMessage.CancelCustomizeCharacter);
    }
};

function debugOut(ex) {
    let msg = ':: ' + ex.toString();
    if (ex.stack) {
        msg += ex.stack;
    }
    API.sendNotification(msg);
}

function deserializeFromDesignatedJson(value) {
    if (typeof value === 'string' && value.startsWith('$json')) {
        return JSON.parse(value.substring(5));
    }
    return value;
}

function serializeToDesignatedJson(value) {
    return '$json' + JSON.stringify(value);
}

function argumentsToArray(args) {
    return Array.prototype.slice.call(args, 0);
}



class Browser {
    constructor() {
        const res = API.getScreenResolution();

        this._instance = API.createCefBrowser(res.Width, res.Height);
        API.setCefBrowserPosition(this._instance, 0, 0);
    }

    navigate(url) {
        return new Promise(resolve => {
            let instance = this._instance;
            let watch = () => {
                try {
                    if (!API.isCefBrowserLoading(instance)) {
                        subscription.disconnect();
                        resolve();
                    }
                } catch (ex) {
                    module.debugOut(ex);
                    subscription.disconnect();
                }
            };
            API.loadPageCefBrowser(instance, url);
            let subscription = API.onUpdate.connect(watch);
        });
    }

    show() {
        API.setCefBrowserHeadless(this._instance, false);
    }

    hide() {
        API.setCefBrowserHeadless(this._instance, true);
    }

    raiseEventInBrowser(eventName, data) {
        this._instance.call('raiseEvent', serializeToDesignatedJson(({ eventName, data })));
    }

    addView(viewName, data) {
        this._instance.call('addView', serializeToDesignatedJson({ selector: viewName, options: data }));
    }

    removeView(viewName) {
        this._instance.call('removeView', serializeToDesignatedJson({ selector: viewName }));
    }

    static create() {
        return new Promise((resolve) => {
            let browser = new Browser();
            browser.hide();
            API.waitUntilCefBrowserInit(browser._instance);
            resolve(browser);
        }).catch(debugOut);
    }
}

class Client {
    constructor() {
        this._cursor = false;
        this._cursorToggle = false;
    }

    get cursor() {
        return this._cursor;
    }

    set cursor(v) {
        if (this._cursor !== v) {
            this._cursor = v;
            this._onCursorStateChanged();
        }
    }

    setCamera(x, y, z, rx, ry, rz) {
        let pos = x;
        let rot = y;
        if (typeof x === 'number' && typeof y === 'number') {
            pos = new Vector3(x, y, z);
            rot = new Vector3(rx, ry, rz);
        }
        let camera = API.createCamera(pos, rot);
        API.setActiveCamera(camera);
    }

    setCameraToViewPlayer() {
        let player = API.getLocalPlayer();
        let pos = API.getEntityPosition(player);
        let rot = API.getGameplayCamDir();
        let camera = API.createCamera(pos, rot);

        pos.X += (rot.X * 3);
        pos.Y += (rot.Y * 3);
        pos.Z -= 0.1;
        let offset = new Vector3(0, 0, 0);
        API.pointCameraAtEntity(camera, player, offset);
        API.setActiveCamera(camera);
    }

    get isHudVisible() {
        return API.getHudVisible();
    }

    set isHudVisible(v) {
        API.setHudVisible(v);
    }

    get coordinates() {
        let player = API.getLocalPlayer();
        let c = API.returnNative('0x3FEF770D40960D5A', 5, player, false);
        return c;
    }

    get rotation() {
        let player = API.getLocalPlayer()
        let r = API.returnNative('0xAFBD61CC738D9EB9', 5, player, 0);
        return r;
    }

    resetCamera() {
        API.setActiveCamera(null);
    }

    static hasRequiredUserGroup(userGroup) {
        return user && user.UserGroup && user.UserGroup >= userGroup;
    }

    _onCursorStateChanged() {
        API.showCursor(this._cursor || this._cursorToggle);
    }
}

let serverEventTriggerQueue = [];
function onServerEventTrigger(eventName, argsArray) {
    let newArgs = [];
    for(let arg in argsArray) {
        let v = argsArray[arg];
        if (typeof v !== 'function') {
            newArgs.push(v);
        }
    }
    serverEventTriggerQueue.push([eventName, newArgs]);
    if (!isReadyToProcessServerEventTriggers) {
        return;
    }
    processServerEventTriggers();
}

function processServerEventTriggers() {
    while (serverEventTriggerQueue.length > 0) {
        let args = serverEventTriggerQueue.shift();
        let handler = ClientEvents[args[0]];
        if (typeof handler === 'function') {
            try {
                handler.apply(null, args[1]);
            } catch(ex) {
                API.sendNotification('process server event trigger ' + args[0]);
                API.sendNotification('' + ex);
                if (ex.stack) {
                    API.sendNotification('' + ex);
                }
            }
        }
    }
}

function sendToServer(eventName, args) {
    args = serializeToDesignatedJson(args);
    API.triggerServerEvent(eventName, args);
}

function pushViewData() {
    if (pushViewDataTickCount++ > 360) {
        pushViewDataTickCount = 0;
        let coordinates = client.coordinates;
        let rotation = client.rotation;

        let value = {
            coord: { x: coordinates.X, y: coordinates.Y, z: coordinates.Z },
            rot: { x: rotation.X, y: rotation.Y, z: rotation.Z }
        };

        browser.raiseEventInBrowser('updateview', { what: 'coordinates', value });
    }
}

let pushViewDataTickCount = 0;
let user = null;
let character = null;
let characterCustomization = null;

let browser = null;
let client = null;
let isReadyToProcessServerEventTriggers = false;
let onServerEventTriggerSubscription = null;
let onResourceStartSubscription = API.onResourceStart.connect(() => {
    onResourceStartSubscription.disconnect();
    onResourceStartSubscription = null;

    onServerEventTriggerSubscription = API.onServerEventTrigger.connect(onServerEventTrigger);

    client = new Client();
    Browser.create().then(newBrowser => {
        browser = newBrowser;
        browser.navigate('index.html').then(() => {
            browser.show();
        });
    });
});

function browser_ready() {
    isReadyToProcessServerEventTriggers = true;
    processServerEventTriggers();

    API.onUpdate.connect(() => pushViewData());
}

function browser_backend(args) {
    args = deserializeFromDesignatedJson(args);
    let handler = BrowserEvents[args.eventName];
    if (typeof handler === 'function') {
        handler.apply(null, args.args);
    }
}


// 'use strict';
// const debug = false;
// class ModuleLoader {
//     constructor() {
//         this._cache = new Map();
//     }
//
//     require(moduleName) {
//         try {
//             if (!moduleName.endsWith('_client')) {
//                 moduleName += '_client';
//             }
//             if (!this._cache.has(moduleName)) {
//                 let res = resource[moduleName];
//                 if (res) {
//                     if (typeof res.define === 'function') {
//                         this._cache.set(moduleName, this._requireFromFile(res));
//                     } else {
//                         API.sendNotification(`Module ${moduleName} does not contain a define function.`);
//                     }
//                 } else {
//                     API.sendNotification(`Module ${moduleName} not part of resource object.`);
//                 }
//             }
//
//             let module = this._cache.get(moduleName);
//             if (module) {
//                 return module.exports;
//             }
//         } catch(ex) {
//             API.sendNotification('error during require for ' + moduleName);
//             API.sendNotification('' + ex);
//             if (ex.stack) {
//                 API.sendNotification('' + ex.stack);
//             }
//             throw ex;
//         }
//     }
//
//     _requireFromFile(res) {
//         let module = { exports: {}, require: (moduleName) => this.require(moduleName), debugOut: debugOut };
//         res.define(module);
//         return module;
//     }
// }
//
// const debugOut = function(ex) {
//     let msg = ':: ' + ex.toString();
//     if (ex.stack) {
//         msg += ex.stack;
//     }
//     API.sendNotification(msg);
// };
//
// const debugPrintObject = function(v) {
//     Object.getOwnPropertyNames(v).forEach(prop => {
//         let propV = v[prop];
//         API.sendNotification('' + prop + '[' + (typeof propV) + ']=' + propV);
//     });
// };
//
// const debugPrint = function(text, args) {
//     if (text) {
//         API.sendChatMessage(text);
//     }
//     if (args) {
//         for (let i = 0; i < args.length; i++) {
//             let v = args[i];
//             let t = typeof(v);
//             if (typeof v === 'object') {
//                 v = '[object]=' + JSON.stringify(v);
//             }
//             if (typeof v === 'undefined') {
//                 v = '[undefined]';
//             }
//             if (v === undefined) {
//                 v = '(undefined)';
//             }
//             if (v === null) {
//                 v = '(null)';
//             }
//             API.sendChatMessage('' + i + ':[' + t + ']' + v);
//         }
//     }
// };
//
// const toClientArgs = function(args) {
//     for (let i = 0; i < args.length; i++) {
//         let arg = args[i];
//         if (typeof arg === 'string' && arg.startsWith('$obj')) {
//             args[i] = JSON.parse(arg.substring('$obj'.length));
//         }
//     }
// };
//
// const bind = function(t, fn) {
//     return function() {
//         fn.apply(t, arguments);
//     }
// };
//
// const loader = new ModuleLoader();
// const require = function() {
//     return loader.require.apply(loader, arguments);
// };
//
// API.onResourceStart.connect(function onConnect() {
//     try {
//         //boot(resource);
//     } catch (ex) {
//         API.sendNotification(ex.toString());
//         if (ex.stack) {
//             API.sendNotification(ex.stack);
//         }
//     }
// });
//
// let boot = (function(resource) {
//     const $sha = require('sha512');
//     const $client = require('client');
//     const $lifecycle = require('lifecycle');
//     const $browser = require('browser');
//     const $controls = require('controls');
//
//     const ServiceResultState = {
//         None: 0,
//         Error: 1,
//         Success: 2
//     };
//
//     class Proxy {
// 	    constructor() {
// 	        this.knownRoots = new Map();
// 	        this._serverEventHandlers = new Map();
//         }
//
//         addServerEventHandler(ev, handler) {
// 	        this._serverEventHandlers.set(ev, handler);
//         }
//
//         onCefInvocation(target) {
// 	        if (debug) {
//                 debugPrint('onCefInvocation', arguments);
//             }
//             if (!target) {
//                 API.sendNotification('received invalid invocation target (null/undefined).');
//                 return;
//             }
//             let paths = target.split('.');
//             let current = null;
//             let previous = null;
//             for (let i = 0; i < paths.length; i++) {
//                 if (i === 0) {
//                     if (!this.knownRoots.has(paths[i])) {
//                         API.sendNotification(`received invocation with unknown root of "${paths[i]}". Path was ${target}.`);
//                         return;
//                     }
//                     current = this.knownRoots.get(paths[i]);
//                 } else {
//                     if (current) {
//                         previous = current;
//                         current = current[paths[i]];
//                     } else {
//                         API.sendNotification(`received invocation where path level ${i - 1} is null/undefined. Path was ${target}.`);
//                     }
//                 }
//             }
//
//             if (typeof current === 'function') {
//                 let args = Array.prototype.slice.call(arguments, 1);
//                 current.apply(previous, args);
//             } else {
//                 API.sendNotification(`received invocation where path select not a function. Path was ${target}.`);
//             }
//         }
//
//         onServerEventTrigger(ev, args) {
// 	        if (debug) {
// 	            debugPrint('proxy onServerEventTrigger', arguments);
//             }
//
// 	        if (!this._serverEventHandlers.has(ev)) {
// 	            API.sendNotification('no server event handler for ' + ev);
//                 return;
//             }
//
//             let handler = this._serverEventHandlers.get(ev);
// 	        if (typeof handler !== 'function') {
//                 API.sendNotification('server event handler for ' + ev + ' is not a function.');
//                 return;
//             }
//
//             toClientArgs(args);
// 	        handler.apply(null, args);
//         }
//     }
//
//     class AppProxy {
//         constructor(app) {
//             this._app = app;
//         }
//
//         disconnect(reason) {
//             API.disconnect(reason);
//         }
//
//         login(username, password) {
//             if (debug) {
//                 debugPrint('login', arguments);
//             }
//             if (password) {
//                     let a = '__' + password + '::0';
//                     for (let i = 0; i < 10; i++) {
//                         a = $sha.sha512(a);
//                     }
//                 password = '::' + a;
//             }
//             this._app.sendToServer('req:login', username, password);
//         }
//
//         customizeCharacter(what, value) {
//             if (debug) {
//                 debugPrint('customizeCharacter', arguments);
//             }
//             this._app.sendToServer('req:customizeCharacter', what, value);
//         }
//     }
//
// 	class App {
// 		constructor() {
//             this._proxy = new Proxy();
// 		    this._client = new $client.Client();
// 			this._lifecycle = new $lifecycle.ClientLifecycle();
// 			this._browser = null;
// 			this._serverEvents = new Map([
//                 [ 'login:response', this.onLoginResponse ]
//             ]);
// 			this._input = new $controls.InputController();
// 			$browser.Browser.create().then(browser => {
//                 this._browser = browser;
//                 browser.navigate('index.html').then(() => {
//                     this.lifecycle.transit($lifecycle.ClientLifecycleState.Connected, this);
//                 }).catch(debugOut);
//             }).catch(debugOut);
//
// 			this.proxy.knownRoots.set('app', new AppProxy(this));
// 			this.proxy.addServerEventHandler('update', bind(this, this.onServerUpdate));
// 			this.proxy.addServerEventHandler('res:login', bind(this, this.onLoginResponse));
// 			this.proxy.addServerEventHandler('res:customizeCharacter', bind(this, this.onCustomizeCharacterResponse));
// 			this.proxy.addServerEventHandler('startCharacterCustomization', bind(this, this.onStartCharacterCustomization));
//
// 			this.pushViewDataTickCount = 0;
// 		}
//
// 		get proxy() {
// 		    return this._proxy;
//         }
//
// 		get client() {
//             return this._client;
//         }
//
// 		get lifecycle() {
// 			return this._lifecycle;
// 		}
//
// 		get browser() {
// 		    return this._browser;
//         }
//
//         get input() {
// 		    return this._input;
//         }
//
//         sendToServer() {
//             if (debug) {
//                 debugPrint('sendToServer', arguments);
//             }
//             let eventName = arguments[0];
//             let args = Array.prototype.slice.call(arguments, 1);
//             args.forEach((arg, index, arr) => {
//                 if (typeof arg === 'object') {
//                     arr[index] = '$obj' + JSON.stringify(arg);
//                 }
//             });
// 		    try {
// 		        // bitch switch because the triggerServerEvent is not a real JavaScript function and it does not have a apply function in addition the
//                 // function can not take simple arrays and serialize on its own ...
// 		        switch(args.length) {
//                     case 0:
//                         API.triggerServerEvent(eventName);
//                         break;
//                     case 1:
//                         API.triggerServerEvent(eventName, args[0]);
//                         break;
//                     case 2:
//                         API.triggerServerEvent(eventName, args[0], args[1]);
//                         break;
//                     default:
//                         API.sendNotification('sendToServer invalid args count');
//                 }
//             }catch(ex) {
//                 debugOut(ex);
//             }
//         }
//
//         onLoginResponse(status, response) {
// 		    if (debug) {
//                 debugPrint('onLoginResponse', arguments);
//             }
//             if (status === ServiceResultState.Success) {
//                 this.client.user = { login: response.login, userGroup: response.userGroup, hasBeenCharacterThroughInitialCustomization: response.hasBeenThroughInitialCustomization };
//                 this.lifecycle.transit($lifecycle.ClientLifecycleState.LoggedIn, this);
//
//                 API.onUpdate.connect(() => app.pushViewData());
//
//                 if (!this.hasBeenCharacterThroughInitialCustomization) {
//                     this.client.character.customization.freeroamCustomizationData = response.freeroamCustomizationData;
//                     this.client.character.customization.data = response.characterCustomizationData;
//                     this.lifecycle.transit($lifecycle.ClientLifecycleState.CharacterCustomization, { app: this, data: this.client.character.customization.freeroamCustomizationData} );
//                 }
//             }
//             this.browser._instance.call('relay', JSON.stringify({ event: 'res:login', status: status, data: response }));
//         }
//
//         onCustomizeCharacterResponse(status, response) {
// 		    this.client.character.customization.data = response.characterCustomizationData;
// 		    //todo: update ui ...
// 		    //this.browser.updateView('view-character-customization', response.characterCustomizationData );
//         }
//
//         onServerUpdate(what, v) {
// 		    this.browser._instance.call('relay', JSON.stringify(({ event: 'update', what, value: v })));
//         }
//
//         onStartCharacterCustomization(data) {
// 		    if (this.lifecycle.state === $lifecycle.ClientLifecycleState.LoggedIn) {
//                 this.lifecycle.transit($lifecycle.ClientLifecycleState.CharacterCustomization, { app: this,  data });
//             } else {
//                 this.client.character.customization.freeroamCustomizationData = data;
//             }
//         }
//
//         pushViewData() {
//             if (this.pushViewDataTickCount++ > 360) {
//                 this.pushViewDataTickCount = 0;
//                 let coordinates = this.client.coordinates;
//                 let rotation = this.client.rotation;
//
//                 let value = {
//                     coord: { x: coordinates.X, y: coordinates.Y, z: coordinates.Z },
//                     rot: { x: rotation.X, y: rotation.Y, z: rotation.Z }
//                 };
//
//                 this.browser._instance.call('relay', JSON.stringify(({ event: 'update', what: 'coordinates', value: value })));
//             }
//         }
// 	}
// 	let app = new App();
//
//     app.input.addMapping($controls.KEYS.CTRL, { onDown: () => app.client.cursorToggle = true, onUp: () => app.client.cursorToggle = false });
//     app.input.addMapping($controls.KEYS.F12, () => app.client.cursor = !app.client.cursor);
//
//     resource.browser_client.cef_invoke = function app_cef_invoke() {
//         if (debug) {
//             API.sendNotification('cef_invoke');
//         }
//         toClientArgs(arguments);
//         app.proxy.onCefInvocation.apply(app.proxy, arguments);
//     };
//
//     API.onServerEventTrigger.connect(function(ev, args) {
//         if (debug) {
//             debugPrint('onServerEventTrigger', arguments);
//         }
//         try {
//             let newArgs = [];
//             for(let arg in args) {
//                 let v = args[arg];
//                 if (typeof v !== 'function') {
//                     newArgs.push(v);
//                 }
//             }
//             app.proxy.onServerEventTrigger(ev, newArgs);
//         } catch(ex) {
//             debugOut(ex);
//         }
//     });
// });