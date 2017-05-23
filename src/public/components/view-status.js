Slim.tag('view-status', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        this.app = window.app;
        let self = this;
        document.addEventListener('relay', (ev) => {
            if (ev.detail.event === 'update') {
                switch(ev.detail.what) {
                    case 'cash':
                        this.cash = ev.detail.value;
                        break;
                }
            }
        });
    }

    onCreated() {
        this.cash = 'n/a';
    }

    asCurrency(v) {
        if (typeof v === 'string') {
            return v;
        }
        return this.app.formatters.currency.format(v);
    }

    get template() {
        return `<div class="bg-teal lighten-1 fg-white" style="position: absolute; bottom: 0; left: 0; right: 0; height: 21px; font-size: 12px; padding: 3px;">
    <span bind>[[asCurrency(cash)]]</span>
</div>`;
    }
});