import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
    toggle() {
        const form = document.getElementById("create-room-form");
        if (!form) return;

        const isHidden = form.style.display === "none";
        form.style.display = isHidden ? "block" : "none";

        if (isHidden) {
            const input = form.querySelector("input[name='name']");
            if (input) input.focus();
        }
    }
}
