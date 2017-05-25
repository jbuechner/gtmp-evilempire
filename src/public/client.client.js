'use strict';
function define(module) {
    class Client {
        constructor() {
            this._cursor = false;
            this._cursorToggle = false;
        }

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

        set canOpenChat(v) {
            API.SetCanOpenChat(true);
        }

        get isHudVisible() {
            return API.getHudVisible();
        }

        set isHudVisible(v) {
            API.setHudVisible(v);
        }

        get isRadarVisible() {
            return !API.callNative('0x157F93B036700462');
        }

        set isRadarVisible(v) {
            API.callNative('0xA0EBB943C300E693', v);
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

        get cursor() {
            return this._cursor;
        }

        set cursor(v) {
            if (this._cursor !== v) {
                this._cursor = v;
                this._onCursorStateChanged();
            }
        }

        resetCamera() {
            API.setActiveCamera(null);
        }

        _onCursorStateChanged() {
            API.showCursor(this._cursor || this._cursorToggle);
        }
    }

    module.exports = {
        Client
    };
}