'use strict';

API.onResourceStart.connect(function() {
    API.sendNotification('notification');

    let player = API.getLocalPlayer();
    let pos = new Vector3(200, 200, 150);
    let rot = new Vector3(1, 1, 1);

    let newCamera = API.createCamera(pos, rot);
    API.setActiveCamera(newCamera);

    let browser = API.createCefBrowser(800, 600);
    API.waitUntilCefBrowserInit(browser);
    API.loadPageCefBrowser(browser, 'index.html');
});