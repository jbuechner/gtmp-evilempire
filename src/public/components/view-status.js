Slim.tag('view-status', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        this.app = window.app;
        let self = this;

        document.addEventListener('moneyChanged', ev => {
            if (ev.detail.currency === Currencies.Dollar) {
                this.cash =  ev.detail.amount;
            }
        });

        document.addEventListener('updateview', (ev) => {
            switch(ev.detail.what) {
                case 'coordinates':
                    let v = ev.detail.value;
                    this.coordinateX = v.coord.x;
                    this.coordinateY = v.coord.y;
                    this.coordinateZ = v.coord.z;
                    this.rotationX = v.rot.x;
                    this.rotationY = v.rot.y;
                    this.rotationZ = v.rot.z;
                    break;
            }
        });
    }

    onCreated() {
        this.currencyFormatter = new Intl.NumberFormat('en-US', { style: 'currency',  currency: 'USD',  minimumFractionDigits: 2, });
        this.cash = 'n/a';
        this.displayCoordinates = false;
        this.coordinateX = this.coordinateY = this.coordinateZ = 0;
        this.rotationX = this.rotationY = this.rotationZ = 0;

        this.cashDisplay.style.left = (window.displayInfo.minimap.margin.left * 2 + window.displayInfo.minimap.width) + 'px';
        this.cashDisplay.style.bottom = (window.displayInfo.minimap.margin.bottom) + 'px';
    }

    asCurrency(v) {
        if (typeof v === 'string') {
            return v;
        }
        return this.currencyFormatter.format(v);
    }

    formatCoordinate(v) {
        if (typeof v === 'string') {
            return v;
        }
        if (v === undefined) {
            return 'n/a';
        }
        if (v.toLocaleString) {
            let s = v.toLocaleString('en-US', {minimumFractionDigits: 3, minimumIntegerDigits: 4, useGrouping: false});
            if (v >= 0) {
                return '+' + s;
            }
            return s;
        }
        return 'n/a';
    }

    toBool(v) {
        if (typeof v === 'boolean') {
            return v;
        }
        return v === 'true';
    }

    get template() {
        return `
<div slim-id="cashDisplay" class="ui-big with-shadow" style="position: absolute; left: 12px; bottom: 0;">
<span bind>[[asCurrency(cash)]]</span>
</div>
<div class="ui-tiny with-shadow monospace" style="position: absolute; left: calc(50% - 180px); bottom: 0;" slim-if="displayCoordinates">
 <span>C</span>
 <span>X</span><span bind>[[formatCoordinate(coordinateX)]]</span>
 <span>Y</span><span bind>[[formatCoordinate(coordinateY)]]</span>
 <span>Z</span><span bind>[[formatCoordinate(coordinateZ)]]</span>
</div>
<div class="ui-tiny with-shadow monospace" style="position: absolute; left: calc(50% - 180px); bottom: 22px;" slim-if="displayCoordinates">
 <span>R</span>
 <span>X</span><span bind>[[formatCoordinate(rotationX)]]</span>
 <span>Y</span><span bind>[[formatCoordinate(rotationZ)]]</span>
 <span>Z</span><span bind>[[formatCoordinate(rotationY)]]</span>
</div>
`;
    }
});