import { Controller } from "@hotwired/stimulus";

export default class extends Controller {
    static targets = ["output"];

    greet() {
        this.outputTarget.textContent = "Hello from Stimulus!";
    }
}
