import NativeReturnType = GrandTheftMultiplayer.Client.Javascript.NativeReturnType;

class Client {
    isInFreeroam: boolean = false;

    _isInVehicle: boolean = false;
    _cursor: boolean = false;
    _cursorToggle: boolean = false;
    _displayInventory: boolean = false;

    constructor() {
    }

    get cursor(): boolean {
        return this._cursor;
    }

    set cursor(v: boolean) {
        if (this._cursor !== v) {
            this._cursor = v;
            this._onCursorStateChanged();
        }
    }

    get isInVehicle(): boolean {
        return this._isInVehicle;
    }

    set isInVehicle(v: boolean) {
        if (this._isInVehicle !== v) {
            this._isInVehicle = v;
        }
    }

    get cursorToggle(): boolean {
        return this._cursorToggle;
    }

    set cursorToggle(v: boolean) {
        if (this._cursorToggle !== v) {
            this._cursorToggle = v;
            this._onCursorStateChanged();
        }
    }

    get canDisplayUiTrackedElements(): boolean {
        return !this.isInVehicle || this.cursor || this.cursorToggle;
    }

    get displayInventory(): boolean {
        return this._displayInventory;
    }

    set displayInventory(v : boolean) {
        if (v !== this._displayInventory) {
            if (v) {
                if (this.isInFreeroam) {
                    this._displayInventory = v;
                    browser.addView('view-inventory', {}, true);
                }
            } else {
                this._displayInventory = v;
                browser.removeView('view-inventory');
            }
        }
    }

    static setCamera(x: number, y: number, z: number, rx: number, ry: number, rz: number): void {
        let pos = new Vector3(x, y, z);
        let rot = new Vector3(rx, ry, rz);
        let camera = API.createCamera(pos, rot);
        API.setActiveCamera(camera);
    }

    static setCameraToViewPlayer(): void {
        let player = API.getLocalPlayer();
        let camPos = API.getOffsetInWorldCoords(player, new Vector3(0.35, 1, 0.5));
        let camera = API.createCamera(camPos, new Vector3(0, 0, 0));
        API.pointCameraAtEntity(camera, player, new Vector3(0, 0, 0.5));
        API.setActiveCamera(camera);
    }

    static get isHudVisible(): boolean {
        return API.getHudVisible();
    }

    static set isHudVisible(v: boolean) {
        API.setHudVisible(v);
    }

    static get coordinates(): Vector3 {
        let player = API.getLocalPlayer();
        let c = API.returnNative<Vector3>('0x3FEF770D40960D5A', NativeReturnType.Vector3, player, false);
        return c;
    }

    static get rotation(): Vector3 {
        let player = API.getLocalPlayer();
        let r = API.returnNative<Vector3>('0xAFBD61CC738D9EB9', NativeReturnType.Vector3, player, 0);
        return r;
    }

    static get aimCoordinates(): Vector3 {
        let player = API.getLocalPlayer();
        let c = API.getPlayerAimCoords(player);
        return c;
    }

    static resetCamera(): void {
        API.setActiveCamera(null);
    }

    static hasRequiredUserGroup(userGroup) {
        return user && user.UserGroup && user.UserGroup >= userGroup;
    }

    _onCursorStateChanged() {
        API.showCursor(this._cursor || this._cursorToggle);
    }
}