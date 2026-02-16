import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
    static values = { roomId: String }
    static targets = ["messagesContainer", "messages", "input"]

    connect() {
        this.isTyping = false;
        this.typingTimeout = null;

        // Set up auto-scroll observer
        this.scrollObserver = new MutationObserver(() => this.scrollToBottom());
        this.scrollObserver.observe(this.messagesTarget, { childList: true });

        // Initial scroll to bottom
        this.scrollToBottom();
    }

    disconnect() {
        this.scrollObserver?.disconnect();
        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
        }
    }

    scrollToBottom() {
        this.messagesContainerTarget.scrollTop = this.messagesContainerTarget.scrollHeight;
    }

    async typing() {
        if (!this.isTyping) {
            this.isTyping = true;
            await fetch(`/Room/${this.roomIdValue}?handler=StartTyping`, {
                method: 'POST',
                credentials: 'same-origin'
            });
        }

        // Clear existing timeout
        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
        }

        // Stop typing after 2 seconds of inactivity
        this.typingTimeout = setTimeout(() => this.stopTyping(), 2000);
    }

    async stopTyping() {
        if (this.isTyping) {
            this.isTyping = false;
            await fetch(`/Room/${this.roomIdValue}?handler=StopTyping`, {
                method: 'POST',
                credentials: 'same-origin'
            });
        }
    }

    submitted() {
        this.inputTarget.value = '';
        this.inputTarget.focus();

        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
        }
        this.stopTyping();
    }
}
