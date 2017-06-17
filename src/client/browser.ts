class Browser {
    _instance: GrandTheftMultiplayer.Client.GUI.Browser;

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
                    debugOut(ex);
                    subscription.disconnect();
                }
            };
            API.sendNotification('loadPage ' + url);
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

    addView(viewName: string, data?: any, allowOnlyOne?: boolean) {
        allowOnlyOne = allowOnlyOne || false;
        this._instance.call('addView', serializeToDesignatedJson({ selector: viewName, options: data, allowOnlyOne }));
    }

    removeView(viewName) {
        this._instance.call('removeView', serializeToDesignatedJson({ selector: viewName }));
    }

    static create(): Promise<Browser> {
        return new Promise((resolve) => {
            let browser = new Browser();
            browser.hide();
            API.waitUntilCefBrowserInit(browser._instance);
            resolve(browser);
        }).catch(debugOut);
    }
}