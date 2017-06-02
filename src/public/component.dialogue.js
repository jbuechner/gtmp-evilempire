'use strict';
class BranchingDialoguePage {
    constructor(options) {
        this.pages = new Map();
        this.markdown = options.__markdown;
        this.action = options.__action;
        this.addPagesFrom(options);
    }

    addPagesFrom(root) {
        let pageNames = Object.getOwnPropertyNames(root).filter(p => p && (typeof p === 'string') && !p.startsWith('__'));
        pageNames.forEach(pageName => {
            this.pages.set(pageName, new BranchingDialoguePage(root[pageName]));
        });
    }

    findPage(key, recursive) {
        if (this.pages.has(key)) {
            return this.pages.get(key);
        }
        if (recursive) {
            for (let [pageKey, value] of this.pages) {
                if (pageKey === key) {
                    return value;
                }
            }
        }
        return undefined;
    }
}

class BranchingDialogue extends BranchingDialoguePage {
    constructor(options) {
        let startPageOptions = options[options.__start];
        super(startPageOptions);
        super.addPagesFrom(options);
    }
}