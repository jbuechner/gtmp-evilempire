Slim.tag('view-entityinteractionmenu', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        this.app = window.app;
        let self = this;
        document.addEventListener('updateview', (ev) => {
            switch(ev.detail.what) {
                case 'entitytargetpos':
                    this.pos = ev.detail.value;
                    break;
            }
        });
    }

    onCreated() {
        this._pos = null;
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

    get template() {
        return `
<div slim-id="container" style="position:absolute;visibility: hidden;" class="hover-box">
    <div class="hover-box-title" >Target Entity</div>
    <div class="hover-box-content">
        Content
    </div>
</div>
`;
    }
});