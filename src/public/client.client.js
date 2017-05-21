'use strict';
function define(module) {
    class Client {
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

        resetCamera() {
            API.setActiveCamera(null);
        }

        cursor(v) {
            API.showCursor(v);
        }
    }

    module.exports = {
        Client
    };
}