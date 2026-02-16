import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
    static values = { roomId: String }
    static targets = ["frame", "loading"]

    connect() {
        console.log("profile side bar controller connected.");

        this.frameTarget.addEventListener('turbo:before-fetch-request', this.showLoading.bind(this));
        this.frameTarget.addEventListener('turbo:frame-load', this.hideLoading.bind(this));
    }

    show(username) {
        // Clear any profile card that might already exist.
        this.frameTarget.innerHTML = '';

        // Open the sidebar.
        this.element.classList.add("open");

        // Trigger the loading of the profile data.
        this.frameTarget.src = `/Room/${this.roomIdValue}?handler=UserProfile&username=${username}`;
    }

    showLoading() {
        this.loadingTarget.classList.remove('hidden');
    }

    hideLoading() {
        this.loadingTarget.classList.add('hidden');
    }

    hide(event) {
        event.preventDefault();
        this.element.classList.remove("open");
    }
}
