'use strict';
window.debug = false;

const ClientLifecycleState = {
    None: 0,
    Connected: 1,
    LoggedIn: 2
};

class Proxy {
    invoke() {
        let args = Array.prototype.slice.call(arguments, 0);
        args.forEach((arg, index, arr) => {
            if (typeof arg === 'object') {
                arr[index] = '$obj' + JSON.stringify(arg);
            }
        });
        resourceCall.apply(null, ['cef_invoke'].concat(args));
    }

    relay(args) {
        document.dispatchEvent(new CustomEvent('relay', { detail: args }));
    }
}

class App {
    constructor() {
        this.proxy = new Proxy();
        this.formatters = {
            currency:  new Intl.NumberFormat('en-US', { style: 'currency',  currency: 'USD',  minimumFractionDigits: 2, })
        };
    }

    login(credentials) {
        this.proxy.invoke('app.login', credentials.username, credentials.password);
    }

    disconnect() {
        this.proxy.invoke('app.disconnect', 'disconnect at login screen');
    }

    customizeCharacter(what, value) {
        this.proxy.invoke('app.customizeCharacter', what, value);
    }
}

function relay(raw) {
    if (debug) {
        console.log(arguments);
    }
    if (window.app) {
        let args = {};
        if (typeof raw === 'string') {
            args = JSON.parse(raw);
        }
        window.app.proxy.relay(args);
    }
}

function addView(args) {
    if (typeof args !== 'object') {
        args = JSON.parse(args);
    }
    if (debug) {
        console.log('addView', args);
    }
    let el = document.createElement(args.selector);
    if (args.options) {
        Object.getOwnPropertyNames(args.options).forEach(propName => {
            el[propName] = args.options[propName];
        });
    }
    document.body.appendChild(el);
}

function updateView(args) {
    if (typeof args !== 'object') {
        args = JSON.parse(args);
    }
    let nodes = document.querySelectorAll(args.selector);
    for (let i = 0; i < nodes.length; i++) {
        if (args.options) {
            Object.getOwnPropertyNames(args.options).forEach(propName => {
                nodes[i][propName] = args.options.propName;
            });
        }
    }
}

function removeView(args) {
    args = JSON.parse(args);
    let elements = document.querySelectorAll(args.selector);
    for (let i = 0; i < elements.length; i++) {
         let el = elements[i];
         if (el && el.parentNode) {
             el.parentNode.removeChild(el);
         }
    }
}

window.app = new App();