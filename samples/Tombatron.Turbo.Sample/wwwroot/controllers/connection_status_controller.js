import { Controller } from "@hotwired/stimulus";

export default class extends Controller {
    static classes = ["connected", "disconnected", "connecting"];

    connect() {
        this._onConnected = () => this._update("connected", "Live");
        this._onDisconnected = () => this._update("disconnected", "Offline");
        this._onReconnecting = () => this._update("connecting", "Connecting...");
        this._onReconnected = () => this._update("connected", "Live");

        document.addEventListener("turbo:signalr:connected", this._onConnected);
        document.addEventListener("turbo:signalr:disconnected", this._onDisconnected);
        document.addEventListener("turbo:signalr:reconnecting", this._onReconnecting);
        document.addEventListener("turbo:signalr:reconnected", this._onReconnected);
    }

    disconnect() {
        document.removeEventListener("turbo:signalr:connected", this._onConnected);
        document.removeEventListener("turbo:signalr:disconnected", this._onDisconnected);
        document.removeEventListener("turbo:signalr:reconnecting", this._onReconnecting);
        document.removeEventListener("turbo:signalr:reconnected", this._onReconnected);
    }

    _update(status, label) {
        this.element.className = `connection-status ${status}`;
        this.element.textContent = label;
    }
}
