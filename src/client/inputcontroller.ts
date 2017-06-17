type KeyEventArgs = System.Windows.Forms.KeyEventArgs;

const enum KEY {
    TAB = 9,
    SHIFT = 16,
    CTRL = 17,
    ALT = 18,
    _0 = 48,
    _1 = 49,
    _2 = 50,
    _3 = 51,
    _4 = 52,
    _5 = 53,
    _6 = 54,
    _7 = 55,
    _8 = 56,
    _9 = 57,
    A = 65,
    B = 66,
    C = 67,
    D = 68,
    E = 69,
    F = 70,
    G = 71,
    H = 72,
    I = 73,
    J = 74,
    K = 75,
    L = 76,
    M = 77,
    N = 78,
    O = 79,
    P = 80,
    Q = 81,
    R = 82,
    S = 83,
    T = 84,
    U = 85,
    V = 86,
    W = 87,
    X = 88,
    Y = 89,
    Z = 90,
    F1 = 112,
    F2 = 113,
    F3 = 114,
    F4 = 115,
    F5 = 116,
    F6 = 117,
    F7 = 118,
    F8 = 119,
    F9 = 120,
    F10 = 121,
    F11 = 122,
    F12 = 123,
    BACKQUOTE = 220
}

const enum KeyPhase {
    Down = 1,
    Up = 2
}

interface IInputControllerAction {
    onDown?: Function;
    onUp?: Function;
}

interface IInputControllerMapping {
    keys: KEY[],
    actions: any[]
}

class InputController {
    _pressed: Map<KEY, boolean> = new Map<KEY, boolean>();
    _mappings: IInputControllerMapping[] = [];

    constructor() {
        API.onKeyUp.connect((sender: any, e: KeyEventArgs) => this.onKeyUp(sender, e));
        API.onKeyDown.connect((sender: any, e: KeyEventArgs) => this.onKeyDown(sender, e));
    }

    get mappings(): IInputControllerMapping[] {
        return this._mappings;
    }

    addMapping(key: KEY, action: Function);
    addMapping(key: KEY, action: IInputControllerAction);
    addMapping(key: KEY, action: any) {
        let mapping = { keys: [key], actions: [action] };
        this._mappings.push(mapping);
        return mapping;
    }

    areKeysPressed(keys: KEY[]): boolean {
        return keys.every(p => this._pressed.has(p) && this._pressed.get(p));
    }

    isKeyPressed(key: KEY): boolean {
        let isPressed = this._pressed.get(key);
        return isPressed === undefined ? false : isPressed;
    }

    setKeyPressed(key: KEY, pressed: boolean): void {
        this._pressed.set(key, pressed);
    }

    onKeyDown(sender: any, e: KeyEventArgs): void {
        if (!this.isKeyPressed(e.KeyValue)) {
            this.setKeyPressed(e.KeyValue, true);
            this.triggerActions(KeyPhase.Down);
        }
    }

    onKeyUp(sender: any, e: KeyEventArgs): void {
        if (this.isKeyPressed(e.KeyValue)) {
            this.triggerActions(KeyPhase.Up);
            this.setKeyPressed(e.KeyValue, false);
        }
    }

    triggerActions(phase: KeyPhase): void {
        this._mappings.forEach(mapping => {
            if (this.areKeysPressed(mapping.keys)) {
                mapping.actions.forEach(action => {
                    try {
                        if (typeof action === 'function' && phase === KeyPhase.Down) {
                            action.call(null);
                            return;
                        }

                        if (action.onDown && phase === KeyPhase.Down) {
                            action.onDown.call(null);
                            return;
                        }
                        if (action.onUp && phase === KeyPhase.Up) {
                            action.onUp.call(null);
                            return;
                        }
                    } catch(ex) {
                        debugOut(ex);
                    }
                });
            }
        });
    }
}