'use strict';

let app = null;

function invoke(raw) {
    if (app) {
        let args = {};
        if (typeof raw === 'string') {
            args = JSON.parse(raw);
        }
        app.proxy.onReceived(args);
    }
}

API.onResourceStart.connect(function() {
    try {
        boot();
    } catch (ex) {
        API.sendNotification(ex.toString());
        if (ex.stack) {
            API.sendNotification(ex.stack);
        }
    }
});

let boot = (function() {
	const ClientLifecycleState = {
		None: 0,
		Connected: 1
	};

	const debugOut = function(ex) {
        let msg = ':: ' + ex.toString();
	    if (ex.stack) {
            msg += ex.stack;
        }
        API.sendNotification(msg);
    };

	class ClientLifecycle {
        constructor() {
            this._transitions = new Map([
                [ClientLifecycle.getStateTransitionKey(ClientLifecycleState.None, ClientLifecycleState.Connected), ClientLifecycle.onClientConnected]
            ]);
            this._state = ClientLifecycleState.None;
        }

        get state() {
            return this._state;
        }

        transit(state, context) {
            const changed = this._state !== state;
            if (changed) {
                const old = this._state;
                this._state = state;
                this.invokeStateTransition(old, state, context);
            }
        }

        invokeStateTransition(from, to, context) {
            const key = ClientLifecycle.getStateTransitionKey(from, to);
            const transition = this._transitions.get(key);
            if (transition !== undefined && typeof transition === 'function') {
                transition.call(null, context);
            } else {
                API.sendNotification(`missing transition from ${from} to ${to}.`);
            }
        }

        static getStateTransitionKey(from, to) {
            return ((from & 0xffff) << 16) + (to & 0xfffff);
        }

        static onClientConnected(app) {
            app.client.setCamera(200, 200, 150, 1, 1, 1);
            API.showCursor(true);
        }
    }

	class Client {
		// {or v3, v3}
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

		cursor(v) {
            API.showCursor(v);
        }
	}

	class Browser {
        constructor() {
            const res = API.getScreenResolution();
            this._instance = API.createCefBrowser(res.Width, res.Height);
            API.setCefBrowserPosition(this._instance, 0, 0);
        }

        navigate(url) {
            API.loadPageCefBrowser(this._instance, url);
        }

        static create() {
            return new Promise((resolve) => {
                let browser = new Browser();
                API.waitUntilCefBrowserInit(browser._instance);
                resolve(browser);
            }).catch(debugOut);
        }
    }

    class Proxy {
	    constructor() {
	        this.knownRoots = new Map();
        }

	    onReceived(args) {
	        args = args || {};
            let target = args.target;
            args = args.args;
            if (!target) {
                API.sendNotification('received invalid invocation target (null/undefined).');
                return;
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
            this._app.sendToServer('login', args);
        }
    }

	class App {
		constructor() {
		    this._proxy = new Proxy();
            this._moduleCache = new Map();
		    this._client = new Client();
			this._lifecycle = new ClientLifecycle();
			this._browser = null;

			Browser.create().then(browser => {
                this._browser = browser;
                browser.navigate('index.html');
            });

			this.lifecycle.transit(ClientLifecycleState.Connected, this);

			this.proxy.knownRoots.set('app', new AppProxy(this));

			let m = this.require('sha512');
			Object.getOwnPropertyNames(prop => API.sendNotification(prop));

            API.onServerEventTrigger.connect(this.onServerEventTrigger);
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

        sendToServer(target, args) {
		    let raw = '';
		    if (args) {
    		    raw = JSON.stringify(args);
            }
		    try {
		        API.sendNotification(`send to server "${target}"`);
                API.triggerServerEvent(target, raw);
            }catch(ex) {
                debugOut(ex);
            }
        }

        onServerEventTrigger(target, args) {
            API.sendNotification(`from server: ${target}`);
            if (typeof args === 'string') {
		        args = JSON.parse(args);
            }
        }

        require(module) {
            if (!this._moduleCache.has(module)) {
                let res = resource[module];
                if (res) {
                    if (typeof res.define === 'function') {
                        let m = res.define();
                        this._moduleCache.set(module, m);
                    } else {
                        API.sendNotification(`Module ${module} does not contain a define function.`);
                    }
                } else {
                    API.sendNotification(`Module ${module} not part of resource object.`);
                }
            }
            return this._moduleCache[module];
        }
	}
	app = new App();
});