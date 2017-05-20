'use strict';

API.onResourceStart.connect(function() {
    try {
        // // Object.getOwnPropertyNames(resource).forEach(propertyName => {
        // //     API.sendNotification(propertyName);
        // // });
        // //resource.browser_client.get();
        // let r = resource.sha512.r();
        // //let hash = resource.sha512_min().sha512('The quick brown fox jumps over the lazy dog');
        // API.sendNotification(typeof r);

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

	class App {
		constructor() {
            this._moduleCache = new Map();
		    this._client = new Client();
			this._lifecycle = new ClientLifecycle();
			this._browser = null;

			Browser.create().then(browser => {
                this._browser = browser;
                browser.navigate('index.html');
            });

			this.lifecycle.transit(ClientLifecycleState.Connected, this);

			let m = this.require('sha512');
			Object.getOwnPropertyNames(prop => API.sendNotification(prop));
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
	new App();
});