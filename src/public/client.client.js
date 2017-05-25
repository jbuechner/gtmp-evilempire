'use strict';
function define(module) {
    const UserGroups = {
        Guest: 0,
        Player: 1,
        GameMaster: 100,
        Admin: 200
    };

    class Client {
        constructor() {
            this._cursor = false;
            this._cursorToggle = false;
            this.user = { login: null, userGroup: UserGroups.Guest };
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
            return !API.returnNative('0x157F93B036700462', 8);
        }

        get coordinates() {
            let player = API.getLocalPlayer()
            let c = API.returnNative('0x3FEF770D40960D5A', 5, player, false);
            return c;
        }

        get rotation() {
            let player = API.getLocalPlayer()
            let r = API.returnNative('0xAFBD61CC738D9EB9', 5, player, 0);
            return r;
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

        hasRequiredUserGroup(userGroup) {
            return this.user && this.user.userGroup && this.user.userGroup >= userGroup;
        }

        resetCamera() {
            API.setActiveCamera(null);
        }

        _onCursorStateChanged() {
            API.showCursor(this._cursor || this._cursorToggle);
        }
    }

    module.exports = {
        Client,
        UserGroups
    };
}