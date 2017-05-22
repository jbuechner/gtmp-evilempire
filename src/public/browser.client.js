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

        addView(viewName) {
            this._instance.call('addView', JSON.stringify({ selector: viewName }));
        }

        removeView(viewName) {
            this._instance.call('removeView', JSON.stringify({ selector: viewName }));
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