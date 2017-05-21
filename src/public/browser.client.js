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
            API.loadPageCefBrowser(this._instance, url);
        }

        show() {
            API.setCefBrowserHeadless(this._instance, false);
        }

        hide() {
            API.setCefBrowserHeadless(this._instance, true);
        }

        static create() {
            return new Promise((resolve) => {
                let browser = new Browser();
                browser.hide();
                API.waitUntilCefBrowserInit(browser._instance);
                resolve(browser);
            }).catch(module.debugOut);
        }
    }
    module.exports = {
        Browser
    };
}