// Stimulus application setup
const application = Stimulus.Application.start();

// Chat controller - handles messaging, typing detection, and auto-scroll
application.register("chat", class extends Stimulus.Controller {
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
});

// Typing indicator controller - filters out current user and renders display
application.register("typing-indicator", class extends Stimulus.Controller {
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
});

application.register("profile-sidebar", class extends Stimulus.Controller {
    show(username) {
        console.log("show username!!");
        this.element.classList.add("open");
    }
});

application.register("chat-message", class extends Stimulus.Controller {
    static outlets = ["profile-sidebar"];

    static values = {
        username: String
    }

    showProfile(event) {
        event.preventDefault();
        console.log("It clicked!");
        if (this.hasProfileSidebarOutlet) {
            this.profileSidebarOutlet.show(this.username);
        }
    }
});
