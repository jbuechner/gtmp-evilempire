'use strict';
const debug = false;
function define(module) {

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
            return keys.every(p => this._pressed.has(p));
        }

        isKeyPressed(key) {
            let isPressed = this._pressed.get(key);
            return isPressed === undefined ? false : isPressed;
        }

        setKeyPressed(key, pressed) {
            this._pressed.set(key, pressed);
        }

        onKeyDown(sender, e) {
            if (debug) {
                API.sendNotification('key down: ' + e.KeyValue);
            }
            if (!this.isKeyPressed(e.KeyValue)) {
                this.setKeyPressed(e.KeyValue, true);
                this.triggerActions(KeyPhase.Down);
            }
        }

        onKeyUp(sender, e) {
            if (debug) {
                API.sendNotification('key up: ' + e.KeyValue);
            }
            this.setKeyPressed(e.KeyValue, false);
            this.triggerActions(KeyPhase.Up);
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
                            module.debugOut(ex);
                        }
                    });
                }
            });
        }
    }

    module.exports = {
        InputController,
        KEYS
    };
}