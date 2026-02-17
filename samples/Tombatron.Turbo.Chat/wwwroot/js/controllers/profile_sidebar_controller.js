import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
    static values = { roomId: String }
    static targets = ["frame", "loading"]

    connect() {
        this.frameTarget.addEventListener('turbo:before-fetch-request', this.showLoading.bind(this));
        this.frameTarget.addEventListener('turbo:frame-load', this.hideLoading.bind(this));
    }

    show(userId) {
        this.frameTarget.innerHTML = '';
        this.element.classList.add("open");
        this.frameTarget.src = `/Room/${this.roomIdValue}?handler=UserProfile&userId=${userId}`;
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
