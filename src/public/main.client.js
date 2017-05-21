'use strict';
const debug = false;
class ModuleLoader {
    constructor() {
        this._cache = new Map();
    }

    require(moduleName) {
        try {
            if (!moduleName.endsWith('_client')) {
                moduleName += '_client';
            }
            if (!this._cache.has(moduleName)) {
                let res = resource[moduleName];
                if (res) {
                    if (typeof res.define === 'function') {
                        this._cache.set(moduleName, this._requireFromFile(res));
                    } else {
                        API.sendNotification(`Module ${moduleName} does not contain a define function.`);
                    }
                } else {
                    API.sendNotification(`Module ${moduleName} not part of resource object.`);
                }
            }

            let module = this._cache.get(moduleName);
            if (module) {
                return module.exports;
            }
        } catch(ex) {
            API.sendNotification('error during require for ' + moduleName);
            API.sendNotification('' + ex);
            if (ex.stack) {
                API.sendNotification('' + ex.stack);
            }
            throw ex;
        }
    }

    _requireFromFile(res) {
        let module = { exports: {}, require: this.require, debugOut: debugOut };
        res.define(module);
        return module;
    }
}

const debugOut = function(ex) {
    let msg = ':: ' + ex.toString();
    if (ex.stack) {
        msg += ex.stack;
    }
    API.sendNotification(msg);
};

const loader = new ModuleLoader();
const require = function() {
    return loader.require.apply(loader, arguments);
};

API.onResourceStart.connect(function onConnect() {
    try {
        boot(resource);
    } catch (ex) {
        API.sendNotification(ex.toString());
        if (ex.stack) {
            API.sendNotification(ex.stack);
        }
    }
});

let boot = (function(resource) {
    const $sha = require('sha512');
    const $lifecycle = require('lifecycle');
    const $client = require('client');
    const $browser = require('browser');
    const $controls = require('controls');

    const ServiceResultState = {
        None: 0,
        Error: 1,
        Success: 2
    };

    class Proxy {
	    constructor() {
	        this.knownRoots = new Map();
        }

        relay(browser, target, args) {
	        if (typeof args !== 'object') {
	            args = { value: args };
            }
            browser._instance.call('relay', JSON.stringify({ target, args }) );
        }

	    onReceived(args) {
	        if (debug) {
                API.sendNotification('proxy received');
                API.sendNotification('' + arguments.length);
                API.sendNotification('' + typeof arguments[0]);
                API.sendNotification('' + arguments[0]);
            }
	        args = args || {};
            let target = args.target;
            args = args.args;

            if (!target) {
                API.sendNotification('received invalid invocation target (null/undefined).');
                return;
            }
            if (debug) {
                API.sendNotification('' + target);
                API.sendNotification('' + args);
            }

            let paths = target.split('.');
            let current = null;
            let previous = null;
            for (let i = 0; i < paths.length; i++) {
                if (i === 0) {
                    if (!this.knownRoots.has(paths[i])) {
                        API.sendNotification(`received invocation with unknown root of "${paths[i]}". Path was ${target}.`);
                        return;
                    }
                    current = this.knownRoots.get(paths[i]);
                } else {
                    if (current) {
                        previous = current;
                        current = current[paths[i]];
                    } else {
                        API.sendNotification(`received invocation where path level ${i - 1} is null/undefined. Path was ${target}.`);
                    }
                }
            }

            if (typeof current === 'function') {
                current.call(previous, args);
            } else {
                API.sendNotification(`received invocation where path select not a function. Path was ${target}.`);
            }
        }
    }

    class AppProxy {
        constructor(app) {
            this._app = app;
        }

        disconnect(args) {
            args = args || {};
            API.disconnect(args.reason);
        }

        login(args) {
            args = args || {};
            if (args) {
                if (args && args.password) {
                        let a = '__' + args.password + '::0';
                        for (let i = 0; i < 10; i++) {
                            a = $sha.sha512(a);
                        }
                        args.password = '::' + a;
                }
            }
            this._app.sendToServer('login', args);
        }
    }

	class App {
		constructor() {
            this._proxy = new Proxy();
		    this._client = new $client.Client();
			this._lifecycle = new $lifecycle.ClientLifecycle();
			this._browser = null;
			this._serverEvents = new Map([
                [ 'login:response', this.onLoginResponse ]
            ]);
			this._input = new $controls.InputController();

			const self = this;
			$browser.Browser.create().then(browser => {
                this._browser = browser;
                this.lifecycle.transit($lifecycle.ClientLifecycleState.Connected, this);
                browser.navigate('index.html');
            }).catch(debugOut);

			this.proxy.knownRoots.set('app', new AppProxy(this));

            API.onServerEventTrigger.connect(function(ev, args) {
                try {
                    if (args && args[0]) {
                        args = args[0];
                    }
                    self.onServerEventTrigger(ev, args);
                } catch(ex) {
                    debugOut(ex);
                }
            });
		}

		get proxy() {
		    return this._proxy;
        }

		get client() {
            return this._client;
        }

		get lifecycle() {
			return this._lifecycle;
		}

		get browser() {
		    return this._browser;
        }

        get input() {
		    return this._input;
        }

        sendToServer(target, args) {
		    let raw = '';
		    if (args) {
    		    raw = JSON.stringify(args);
            }
		    try {
                API.triggerServerEvent(target, raw);
            }catch(ex) {
                debugOut(ex);
            }
        }

        onServerEventTrigger(target, args) {
            if (typeof args === 'string') {
		        args = JSON.parse(args);
            }
            if (args && (args.state === ServiceResultState.Error)) {
                API.sendNotification(`error from server "${target}"`);
                if (args.data) {
                    API.sendNotification('>' + args.data);
                }
            }
            this.proxy.relay(this.browser, target, args);
            if (this._serverEvents.has(target)) {
                let handler = this._serverEvents.get(target);
                if (typeof handler === 'function') {
                    handler.call(this, args);
                } else {
                    API.sendNotification(`server event handler for ${target} is not a function.`)
                }
            } else {
                API.sendNotification(`missing server event handler for ${target}.`);
            }
        }

        onLoginResponse(args) {
            if (args.state === ServiceResultState.Success) {
                this.lifecycle.transit($lifecycle.ClientLifecycleState.LoggedIn, this);
            }
        }
	}
	let app = new App();

    app.input.addMapping($controls.KEYS.CTRL, { onDown: () => app.client.cursorToggle = true, onUp: () => app.client.cursorToggle = false });
    app.input.addMapping($controls.KEYS.F12, () => app.client.cursor = !app.client.cursor);

    resource.browser_client.cef_invoke = function app_cef_invoke() {
        let args = arguments;
        if (args && args.length === 1) {
            if (typeof args[0] === 'string') {
                args = [ JSON.parse(arguments[0]) ];
            }
        }
        app.proxy.onReceived.apply(app.proxy, args);
    }
});