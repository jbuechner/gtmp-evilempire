'use strict';

API.onResourceStart.connect(() => boot());

let boot = (function() {
let player = API.getLocalPlayer();
	let pos = new Vector3(200, 200, 150);
	let rot = new Vector3(1, 1, 1);

	let newCamera = API.createCamera(pos, rot);
	API.setActiveCamera(newCamera);

	let res = API.getScreenResolution();

	let browser = API.createCefBrowser(res.Width, res.Height);
	API.setCefBrowserPosition(browser, 0, 0);
	API.waitUntilCefBrowserInit(browser);
	API.loadPageCefBrowser(browser, 'index.html');
	API.showCursor(true);
});