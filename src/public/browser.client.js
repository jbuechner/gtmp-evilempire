'use strict';
const debug = false;
function cef_invoke() {
    API.sendNotification('stub');
}

function define(module) {
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
            if (debug) {
                API.sendNotification('show browser');
            }
            API.setCefBrowserHeadless(this._instance, false);
        }

        hide() {
            if (debug) {
                API.sendNotification('hide browser');
            }
            API.setCefBrowserHeadless(this._instance, true);
        }

        addView(viewName, options) {
            if (typeof options === 'string') {
                options = JSON.parse(options);
            }
            let args = JSON.stringify({ selector: viewName, options });
            if (debug) {
                API.sendNotification('addView=' + args);
            }
            this._instance.call('addView', args);
        }

        removeView(viewName) {
            this._instance.call('removeView', JSON.stringify({ selector: viewName }));
        }

        call() {
            if (debug) {
                API.sendNotification('browser.call for ' + arguments[0]);
                for (let i = 1; i < arguments.length; i++) {
                    API.sendNotification('' + i + '[' + typeof(arguments[i]) + ']=' + arguments[i]);
                }
            }
            let method = arguments[0].replace(/:/, '_');
            let args = [];
            for (let i = 1; i < arguments.length; i++) {
                let arg = arguments[i];
                if (typeof arg === 'object' || typeof arg === 'function') {
                    arg = '$obj' + JSON.stringify(arg);
                }
                args.push(arg);
            }
            API.sendNotification(':::');
            for (let i = 1; i < args.length; i++) {
                API.sendNotification('' + i + '[' + typeof(args[i]) + ']=' + args[i]);
            }
            // Bitch calls again
            let b = this._instance;
            switch(args.length) {
                case 1:
                    b.call(method);
                    break;
                case 2:
                    b.call(method, args[0]);
                    break;
                case 3:
                    b.call(method, args[0], args[1], args[2]);
                    break;
                case 4:
                    b.call(method, args[0], args[1], args[2], args[3]);
                    break;
                case 5:
                    b.call(method, args[0], args[1], args[2], args[3], args[4]);
                    break;
                case 6:
                    b.call(method, args[0], args[1], args[2], args[3], args[4], args[5]);
                    break;
                case 7:
                    b.call(method, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                    break;
                case 8:
                    b.call(method, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                    break;
                case 9:
                    b.call(method, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
                    break;
                case 10:
                    b.call(method, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
                    break;
                default:
                    API.sendNotification('invalid arguments length for browser.call');
                    break;
            }
        }

        static create() {
            return new Promise((resolve) => {
                let browser = new Browser();
                browser.hide();
                if (debug) {
                    API.sendNotification('browser initializing');
                }
                API.waitUntilCefBrowserInit(browser._instance);
                if (debug) {
                    API.sendNotification('browser initialized');
                }
                resolve(browser);
            }).catch(module.debugOut);
        }
    }
    module.exports = {
        Browser
    };
}