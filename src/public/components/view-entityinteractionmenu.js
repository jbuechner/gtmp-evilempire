Slim.tag('view-entityinteractionmenu', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        this.app = window.app;
        document.addEventListener('updateview', (ev) => {
            if (ev.detail.value.entityId && ev.detail.value.entityId === this.entityId) {
                switch (ev.detail.what) {
                    case 'entitytargetpos':
                        this.pos = { x: ev.detail.value.x, y: ev.detail.value.y };
                        break;
                    case 'content':
                        this.dialogue = new BranchingDialogue(ev.detail.value.dialogue);
                        this.resetIsLoading();
                        break;
                    case 'entityAction':
                        this.resetIsLoading();
                        this.markdown = '';
                        break;
                }
            }
        });
    }

    onCreated() {
        this._entityId = null;
        this._pos = null;
        this._actions = [];
        this._dialogue = null;
        this.resetIsLoading();
        this.isContentVisible = false;
    }

    onContentDomLinkClick(e) {
        e.preventDefault();
        let action = e.target.getAttribute('data-dialogue-action');
        let target = e.target.getAttribute('data-dialogue-target');
        switch (('' + action).toUpperCase()) {
            case 'JUMPTOPAGE':
                let page = this.dialogue.findPage(target, true);
                if (page && page.markdown) {
                    this.markdown = page.markdown;
                    break;
                }
                if (page.action) {
                    let invariantAction = page.action.toUpperCase();
                    if (invariantAction === 'CLOSEACTIVEENTITYINTERACTION') {
                        this.dialogue = null;
                        return;
                    }
                    if (invariantAction.startsWith('@')) {
                        this.triggerServerAction(page.action);
                        return;
                    }
                }
                break;
        }
    }

    postProcessContentDom(root) {
        let links = root.querySelectorAll('a');
        for (let i = 0; i < links.length; i++) {
            let link = links.item(i);
            link.onclick = (e) => this.onContentDomLinkClick(e);

            let href = link.getAttribute('href');
            if (typeof href === 'string' && href.startsWith('#')) {
                link.setAttribute('data-dialogue-action', 'jumpToPage');
                link.setAttribute('data-dialogue-target', href.substring(1));
            }
        }
    }

    get dialogue() {
        return this._dialogue;
    }

    set dialogue(v) {
        if (v !== this.dialogue) {
            this._dialogue = v;
            if (v) {
                this.markdown = v.markdown;
            } else {
                this.markdown = null;
                this.isContentVisible = false;
                this.resetActiveActions();
            }
        }
    }

    set markdown(v) {
        let converter = new showdown.Converter();
        let html = converter.makeHtml(v);

        this.content.innerHTML = html;

        this.postProcessContentDom(this.content);
    }

    get entityId() {
        return this._entityId;
    }

    set entityId(v) {
        this._entityId = v;
        this.setAttribute('data-entityId', v);
    }

    get title() {
        return this.titleElement.innerHTML;
    }

    set title(v) {
        this.titleElement.innerHTML = v;
    }

    get pos() {
        return this._pos;
    }

    set pos(v) {
        if (v !== this._pos) {
            if (v && v.x && v.y) {
                this.container.style.visibility = 'visible';
                this.container.style.left = v.x + 'px';
                this.container.style.top = v.y + 'px';
            } else {
                this.container.style.visibility = 'hidden';
            }
            this._pos = v;
        }
    }

    get actions() {
        return this._actions;
    }

    set actions(v) {
        if (v !== this._actions) {
            this._actions = v;

            let available = {};
            if (v) {
                if (v.forEach) {
                    v.forEach(item => available[item] = true);
                } else {
                    Object.getOwnPropertyNames(v).forEach(prop => available[prop] = v[prop]);
                }
            }
            this.available = available;
        }
    }

    resetIsLoading() {
        this.isLoading = false;
        this.loadingText = 'Please wait ...';
    }

    triggerServerAction(serverAction) {
        if (this.isLoading) {
            this.loadingText = 'Patience ...';
            return;
        }

        this.isLoading = true;
        this.markdown = '';
        if (this.app) {
            this.app.triggerEntityAction(this.entityId, serverAction);
        } else {
            console.warn('unable to dispatch entity interaction.');
        }
    }

    raiseAction(e) {
        e.preventDefault();

        if (this.isLoading) {
            this.loadingText = 'Patience ...';
            return;
        }

        let action = e.target.getAttribute('data-action');
        let requiresContent = e.target.getAttribute('data-action-requiresContent');

        let isActive = e.target.classList.contains('active');
        this.resetActiveActions();
        if (isActive) {
            this.resetIsLoading();
            this.isContentVisible = false;
            return;
        }

        this.isContentVisible = requiresContent === 'true';
        this.isLoading = this.isContentVisible;
        if (this.isContentVisible) {
            this.markdown = null;
        }

        e.target.classList.add('active');

        if (this.app) {
            this.app.entityinteraction(this.entityId, action);
        } else {
            console.warn('unable to dispatch entity interaction.');
        }
    }

    resetActiveActions() {
        let nodes = this.querySelectorAll('.hover-box-icons .fa');
        for (let i = 0; i < nodes.length; i++) {
            nodes.item(i).classList.remove('active');
        }
    }

    get template() {
        return `<div slim-id="container" style="position:absolute;visibility: hidden;">
         <div class="hover-box">
            <div class="hover-box-title" slim-id="titleElement"></div>
            <div class="hover-box-icons">
                <i slim-if="available.speak" class="fa fa-comments-o hover-box-icon" aria-hidden="true" click="raiseAction" data-action="speak" data-action-requiresContent="true"></i>
                <i slim-if="available.lock" class="fa fa-key hover-box-icon" aria-hidden="true" click="raiseAction" data-action="lock" data-action-requiresContent="false"></i>
            </div>
        </div>
        <div slim-if="isContentVisible" class="hover-box-content">
            <p slim-if="isLoading" >
                <span>
                    <i class="fa fa-spinner fa-pulse fa-fw" style="margin-top:3px"></i>
                    <span bind>[[loadingText]]</span>
                </span>
            </p>
            <div slim-id="content"></div>
        </div>
    </div>
</div>
`;
    }
});