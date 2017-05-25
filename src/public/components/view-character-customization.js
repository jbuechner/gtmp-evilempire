Slim.tag('view-character-customization', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        this.app = window.app;
    }

    onCreated() {
        this.selectedModel = null;
        this.selectedModelIndex = null;
    }

    getModelName(models, selected) {
        if (this.selectedModel === null && this.Models && this.Models.length > 0)  {
            this.selectedModel = this.Models[0];
            this.selectedModelIndex = 0;
        }
        if (this.selectedModel) {
            return this.selectedModel.Name;
        }
    }

    selectPreviousModel() {
        if (this.selectedModelIndex !== null && --this.selectedModelIndex < 0) {
            this.selectedModelIndex = this.Models.length - 1;
        }
        this.selectedModel = this.Models[this.selectedModelIndex];
    }


    selectNextModel() {
        if (this.selectedModelIndex !== null && ++this.selectedModelIndex >= this.Models.length) {
            this.selectedModelIndex = 0;
        }
        this.selectedModel = this.Models[this.selectedModelIndex];
    }

    get template() {
        return `<div style="position:absolute;top:10%;right:0;">
<div class="box">




<div class="field has-addons">
  <p class="control">
    <a class="button" click="selectPreviousModel">
      &lt;
    </a>
  </p>
  <p class="control">
    <input class="input" type="text" placeholder="Gender" disabled="disabled" value="[[getModelName(Models, selectedModel)]]">
  </p>
  <p class="control">
    <a class="button" click="selectNextModel">
      &gt;
    </a>
  </p>
</div>






</div>
</div>`;
    }
});