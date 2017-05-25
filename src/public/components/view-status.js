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
                    case 'coordinates':
                        this.coordinateX = ev.detail.value.x;
                        this.coordinateY = ev.detail.value.y;
                        this.coordinateZ = ev.detail.value.z;
                        break;
                }
            }
        });
    }

    onCreated() {
        this.cash = 'n/a';
        this.displayCoordinates = false;
        this.coordinateX = this.coordinateY = this.coordinateZ = 0;
    }

    asCurrency(v) {
        if (typeof v === 'string') {
            return v;
        }
        return this.app.formatters.currency.format(v);
    }

    formatCoordinate(v) {
        if (typeof v === 'string') {
            return v;
        }
        let s = v.toLocaleString('en-US', { minimumFractionDigits: 3, minimumIntegerDigits: 5, useGrouping: false });
        if (v >= 0) {
            return '+' + s;
        }
        return s;
    }

    toBool(v) {
        if (typeof v === 'boolean') {
            return v;
        }
        return v === 'true';
    }

    get template() {
        return `<div class="bg-teal lighten-1 fg-white" style="position: absolute; bottom: 0; left: 0; right: 0; height: 32px; font-size: 16px; padding: 6px;">
    <span bind>[[asCurrency(cash)]]</span>
    <span slim-if="displayCoordinates" class="is-pulled-right monospace">
        <span>X</span><span bind>[[formatCoordinate(coordinateX)]]</span>
        <span>Y</span><span bind>[[formatCoordinate(coordinateY)]]</span>
        <span>Z</span><span bind>[[formatCoordinate(coordinateZ)]]</span>
</span></div>`;
    }
});