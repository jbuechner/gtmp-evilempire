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

const KEYS = {
    SHIFT: 16,
    CTRL: 17,
    ALT: 18,
    _0: 48,
    _1: 49,
    _2: 50,
    _3: 51,
    _4: 52,
    _5: 53,
    _6: 54,
    _7: 55,
    _8: 56,
    _9: 57,
    A: 65,
    B: 66,
    C: 67,
    D: 68,
    E: 69,
    F: 70,
    G: 71,
    H: 72,
    I: 73,
    J: 74,
    K: 75,
    L: 76,
    M: 77,
    N: 78,
    O: 79,
    P: 80,
    Q: 81,
    R: 82,
    S: 83,
    T: 84,
    U: 85,
    V: 86,
    W: 87,
    X: 88,
    Y: 89,
    Z: 90,
    F1: 112,
    F2: 113,
    F3: 114,
    F4: 115,
    F5: 116,
    F6: 117,
    F7: 118,
    F8: 119,
    F9: 120,
    F10: 121,
    F11: 122,
    F12: 123
};

const KeyPhase = {
    Down: 1,
    Up: 2
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
        if (!success && typeof data !== 'object') {
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

    get cursorToggle() {
        return this._cursorToggle;
    }

    set cursorToggle(v) {
        if (this._cursorToggle !== v) {
            this._cursorToggle = v;
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

        pos.Z += 0.5;
        pos.X -= (rot.X * 2);
        pos.Y += (rot.Y * 1.2);

        let camera = API.createCamera(pos, rot);

        let offset = new Vector3(0, 0, 0.5);
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

class InputController {
    constructor() {
        this._pressed = new Map();
        this._mappings = [];

        API.onKeyUp.connect((sender, e) => this.onKeyUp(sender, e));
        API.onKeyDown.connect((sender, e) => this.onKeyDown(sender, e));
    }

    get mappings() {
        return this._mappings;
    }

    addMapping(key, action) {
        let mapping = { keys: [key], actions: [action] };
        this._mappings.push(mapping);
        return mapping;
    }

    areKeysPressed(keys) {
        return keys.every(p => this._pressed.has(p) && this._pressed.get(p));
    }

    isKeyPressed(key) {
        let isPressed = this._pressed.get(key);
        return isPressed === undefined ? false : isPressed;
    }

    setKeyPressed(key, pressed) {
        this._pressed.set(key, pressed);
    }

    onKeyDown(sender, e) {
        if (!this.isKeyPressed(e.KeyValue)) {
            this.setKeyPressed(e.KeyValue, true);
            this.triggerActions(KeyPhase.Down);
        }
    }

    onKeyUp(sender, e) {
        if (this.isKeyPressed(e.KeyValue)) {
            this.triggerActions(KeyPhase.Up);
            this.setKeyPressed(e.KeyValue, false);
        }
    }

    triggerActions(phase) {
        this._mappings.forEach(mapping => {
            if (this.areKeysPressed(mapping.keys)) {
                mapping.actions.forEach(action => {
                    try {
                        if (action.call && phase === KeyPhase.Down) {
                            action.call(null);
                        }
                        if (action.onDown && phase === KeyPhase.Down) {
                            action.onDown.call(null);
                        }
                        if (action.onUp && phase === KeyPhase.Up) {
                            action.onUp.call(null);
                        }
                    } catch(ex) {
                        debugOut(ex);
                    }
                });
            }
        });
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
let inputs = null;
let isReadyToProcessServerEventTriggers = false;
let onServerEventTriggerSubscription = null;
let onResourceStartSubscription = API.onResourceStart.connect(() => {
    onResourceStartSubscription.disconnect();
    onResourceStartSubscription = null;

    onServerEventTriggerSubscription = API.onServerEventTrigger.connect(onServerEventTrigger);

    client = new Client();
    inputs = new InputController();
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

    inputs.addMapping(KEYS.CTRL, { onDown: () => client.cursorToggle = true, onUp: () => client.cursorToggle = false });
    inputs.addMapping(KEYS.F12, () => client.cursor = !client.cursor);
}

function browser_backend(args) {
    args = deserializeFromDesignatedJson(args);
    let handler = BrowserEvents[args.eventName];
    if (typeof handler === 'function') {
        handler.apply(null, args.args);
    }
}
