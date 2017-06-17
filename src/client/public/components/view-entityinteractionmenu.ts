///<reference path="../../definitions.d.ts"/>

'use strict';
const KnownClientSideActions = new Map();
KnownClientSideActions.set('CLOSEACTIVEENTITYINTERACTION', function __closeActiveEntityInteraction() {
    this.keyOfActiveDialogueContent = null;
    this.markdown = '';
    this.isContentVisible = false;
    this.resetActiveActions();
});

Slim.tag('view-entityinteractionmenu', class extends Slim {
    contentElement: HTMLElement;
    containerElement: HTMLElement;
    titleElement: HTMLElement;
    availableActions: any;
    isLoading: boolean;
    loadingText: string;

    _info: IUiTrackingInfo;
    keyOfActiveDialogueContent: string;
    isContentVisible: boolean;

    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        document.addEventListener('updateview', (ev: any) => {
            if (ev.detail.value.entityId && ev.detail.value.entityId === this.entityId) {
                switch (ev.detail.what) {
                    case 'entityinfo':
                        this.info = ev.detail.value;
                        break;
                    case 'content':
                        this.resetIsLoading();

                        let content = ev.detail.value.content;
                        if (!content || typeof content === 'undefined' || content === null || typeof content.markdown === 'undefined' || content.markdown === null) {
                            this.runClientSideAction([ 'closeActiveEntityInteraction' ]);
                        } else {
                            this.keyOfActiveDialogueContent = content.key;
                            this.markdown = content.markdown;
                        }
                        break;
                }
            }
        });
    }

    onCreated() {
        this._info = null;
        this.keyOfActiveDialogueContent = null;
        this.isContentVisible = false;
        this.availableActions = {};

        this.resetIsLoading();
    }

    onContentDomLinkClick(e) {
        e.preventDefault();
        let action = e.target.getAttribute('data-dialogue-action');
        let target = e.target.getAttribute('data-dialogue-target');
        switch (('' + action).toUpperCase()) {
            case 'TRIGGERSERVERSIDEACTION':
                this.triggerServerAction(target);
                this.markdown = '';
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
                link.setAttribute('data-dialogue-action', 'triggerServerSideAction');
                link.setAttribute('data-dialogue-target', href.substring(1));
            }
        }
    }

    set markdown(v) {
        let converter = new showdown.Converter();
        this.contentElement.innerHTML = converter.makeHtml(v);

        this.postProcessContentDom(this.contentElement);
    }

    get info(): IUiTrackingInfo {
        return this._info;
    }

    set info(v: IUiTrackingInfo) {
        if (this._info !== v) {
            this._info = v;
            this.entityId = v.id;
            this.title = v.displayName;
            this.pos = v.position2d;
            this.actions = v.actions;
        }
    }

    get entityId(): number {
        if (this.info) {
            return this.info.id;
        }
        return null;
    }

    set entityId(v: number) {
        if (this.info === null) {
            this.info = <IUiTrackingInfo>{ id: v };
        }
        this.info.id = v;
        this.setAttribute('data-entityId', v.toFixed(0));
    }

    set title(v: string) {
        this.titleElement.innerHTML = v;
    }

    set pos(v: IVector) {
        if (v && v.x && v.y) {
            this.containerElement.style.visibility = 'visible';
            this.containerElement.style.left = v.x + 'px';
            this.containerElement.style.top = v.y + 'px';
        } else {
            this.containerElement.style.visibility = 'hidden';
        }
    }

    set actions(v: string[]) {
        let available = {};
        if (v) {
            if (v.forEach) {
                v.forEach(item => available[item] = true);
            } else {
                Object.getOwnPropertyNames(v).forEach(prop => available[prop] = v[prop]);
            }
        }
        this.availableActions = available;
    }

    resetIsLoading() {
        this.isLoading = false;
        this.loadingText = 'Please wait ...';
    }

    runClientSideAction(clientSideActions) {
        clientSideActions.forEach(action => {
            if (typeof action === 'string') {
                action = action.toUpperCase();
            }
            let fn = KnownClientSideActions.get(action);
            if (typeof fn === 'function') {
                fn.apply(this);
            }
        });
    }

    triggerServerAction(serverAction) {
        if (this.isLoading) {
            this.loadingText = 'Patience ...';
            return;
        }

        this.isLoading = true;
        this.markdown = '';
        App.triggerEntityAction(this.entityId, serverAction);
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

        App.entityinteraction(this.entityId, action);
    }

    resetActiveActions() {
        let nodes = this.querySelectorAll('.hover-box-icons .fa');
        for (let i = 0; i < nodes.length; i++) {
            nodes.item(i).classList.remove('active');
        }
    }

    get template() {
        return `<div slim-id="containerElement" style="position:absolute;visibility: hidden;">
         <div class="hover-box">
            <div class="hover-box-title" slim-id="titleElement"></div>
            <div class="hover-box-icons">
                <i slim-if="availableActions.speak" class="fa fa-comments-o hover-box-icon" aria-hidden="true" click="raiseAction" data-action="speak" data-action-requiresContent="true"></i>
                <i slim-if="availableActions.lock" class="fa fa-lock hover-box-icon" aria-hidden="true" click="raiseAction" data-action="lock" data-action-requiresContent="false"></i>
                <i slim-if="availableActions.engine" class="fa fa-key hover-box-icon" aria-hidden="true" click="raiseAction" data-action="engine" data-action-requiresContent="false"></i>
            </div>
        </div>
        <div slim-if="isContentVisible" class="hover-box-content">
            <p slim-if="isLoading" >
                <span>
                    <i class="fa fa-spinner fa-pulse fa-fw" style="margin-top:3px"></i>
                    <span bind>[[loadingText]]</span>
                </span>
            </p>
            <div slim-id="contentElement"></div>
        </div>
    </div>
</div>
`;
    }
});