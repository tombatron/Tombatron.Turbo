import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
    static values = { currentUser: String }

    connect() {
        // Watch for Turbo Stream updates to this element
        this.observer = new MutationObserver(() => this.render());
        this.observer.observe(this.element, { childList: true, subtree: true });
        this.render();
    }

    disconnect() {
        this.observer?.disconnect();
    }

    render() {
        const dataSpan = this.element.querySelector('[data-typing-users]');
        if (!dataSpan) {
            return;
        }

        const typingUsers = JSON.parse(dataSpan.dataset.typingUsers || '[]')
            .filter(user => user !== this.currentUserValue);

        let text = '';
        if (typingUsers.length === 0) {
            text = '';
        } else if (typingUsers.length === 1) {
            text = `${typingUsers[0]} is typing...`;
        } else if (typingUsers.length === 2) {
            text = `${typingUsers[0]} and ${typingUsers[1]} are typing...`;
        } else if (typingUsers.length === 3) {
            text = `${typingUsers[0]}, ${typingUsers[1]}, and ${typingUsers[2]} are typing...`;
        } else {
            const displayed = typingUsers.slice(0, 3);
            const othersCount = typingUsers.length - 3;
            const othersText = othersCount === 1 ? '1 other' : `${othersCount} others`;
            text = `${displayed.join(', ')}, and ${othersText} are typing...`;
        }

        // Temporarily disconnect observer to avoid infinite loop
        this.observer.disconnect();
        this.element.textContent = text;
        this.observer.observe(this.element, { childList: true, subtree: true });
    }
}
