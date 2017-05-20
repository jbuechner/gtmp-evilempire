'use strict';
class Proxy {
    invoke(target, args) {
        resourceCall('invoke', JSON.stringify({target, args}));
    }
}

class App {
    constructor() {
        this.proxy = new Proxy();
    }

    login(credentials) {
        this.proxy.invoke('app.login', { credentials });
    }

    disconnect() {
        this.proxy.invoke('app.disconnect', { reason: 'client disconnected.' });
    }
}
window.app = new App();