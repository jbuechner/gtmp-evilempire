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
                }
            }
        });
    }

    onCreated() {
        this._entityId = null;
        this._pos = null;
        this._actions = [];
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

    raiseAction(e) {
        e.preventDefault();
        let action = e.target.getAttribute('data-action');
        if (this.app) {
            this.app.entityinteraction(this.entityId, action);
        }
    }

    get template() {
        return `
<div slim-id="container" style="position:absolute;visibility: hidden;" class="hover-box">
    <div class="hover-box-title" slim-id="titleElement"></div>
    <div class="hover-box-content">
        <i slim-if="available.speak" class="fa fa-comments-o hover-box-icon" aria-hidden="true" click="raiseAction" data-action="speak"></i>
        <i slim-if="available.lock" class="fa fa-key" aria-hidden="true" click="raiseAction" data-action="lock"></i>
    </div>
</div>
`;
    }
});