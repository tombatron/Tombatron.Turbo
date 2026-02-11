(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ? factory(exports, require('@microsoft/signalr')) :
    typeof define === 'function' && define.amd ? define(['exports', '@microsoft/signalr'], factory) :
    (global = typeof globalThis !== 'undefined' ? globalThis : global || self, factory(global.TurboSignalR = {}, global.signalR));
})(this, (function (exports, signalR) { 'use strict';

    function _interopNamespaceDefault(e) {
        var n = Object.create(null);
        if (e) {
            Object.keys(e).forEach(function (k) {
                if (k !== 'default') {
                    var d = Object.getOwnPropertyDescriptor(e, k);
                    Object.defineProperty(n, k, d.get ? d : {
                        enumerable: true,
                        get: function () { return e[k]; }
                    });
                }
            });
        }
        n.default = e;
        return Object.freeze(n);
    }

    var signalR__namespace = /*#__PURE__*/_interopNamespaceDefault(signalR);

    /**
     * Turbo Stream Source SignalR - Custom element for receiving Turbo Streams via SignalR
     *
     * This custom element connects to an ASP.NET Core SignalR hub and subscribes to
     * named streams. When Turbo Stream messages are received, they are rendered
     * using the Hotwire Turbo library.
     *
     * Usage:
     *   <turbo-stream-source-signalr stream="my-stream" hub-url="/turbo-hub"></turbo-stream-source-signalr>
     */


    // Singleton connection manager
    const connectionManager = {
        /** @type {signalR.HubConnection|null} */
        connection: null,

        /** @type {Map<string, number>} Stream name to reference count */
        streamRefs: new Map(),

        /** @type {Set<string>} Streams currently subscribed on the server */
        subscribedStreams: new Set(),

        /** @type {string|null} Current hub URL */
        hubUrl: null,

        /** @type {boolean} Whether we're currently connected */
        isConnected: false,

        /** @type {boolean} Whether we're currently connecting */
        isConnecting: false,

        /** @type {Array<Function>} Callbacks waiting for connection */
        connectionCallbacks: [],

        /**
         * Gets or creates a connection to the specified hub URL.
         * @param {string} hubUrl - The SignalR hub URL
         * @returns {Promise<signalR.HubConnection>}
         */
        async getConnection(hubUrl) {
            // If we have a connection to a different URL, close it
            if (this.connection && this.hubUrl !== hubUrl) {
                await this.closeConnection();
            }

            // If already connected, return the connection
            if (this.connection && this.isConnected) {
                return this.connection;
            }

            // If currently connecting, wait for it
            if (this.isConnecting) {
                return new Promise((resolve) => {
                    this.connectionCallbacks.push(() => resolve(this.connection));
                });
            }

            // Create new connection
            this.isConnecting = true;
            this.hubUrl = hubUrl;

            this.connection = new signalR__namespace.HubConnectionBuilder()
                .withUrl(hubUrl)
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: (retryContext) => {
                        // Exponential backoff: 0s, 2s, 4s, 8s, 16s, max 30s
                        const delay = Math.min(Math.pow(2, retryContext.previousRetryCount) * 1000, 30000);
                        return delay;
                    }
                })
                .configureLogging(signalR__namespace.LogLevel.Warning)
                .build();

            // Set up message handler
            this.connection.on('TurboStream', (html) => {
                this.handleTurboStream(html);
            });

            // Set up reconnection handlers
            this.connection.onreconnecting(() => {
                this.isConnected = false;
                this.dispatchConnectionEvent('turbo:signalr:reconnecting');
            });

            this.connection.onreconnected(async () => {
                this.isConnected = true;
                this.dispatchConnectionEvent('turbo:signalr:reconnected');

                // Resubscribe to all active streams
                await this.resubscribeAll();
            });

            this.connection.onclose(() => {
                this.isConnected = false;
                this.subscribedStreams.clear();
                this.dispatchConnectionEvent('turbo:signalr:disconnected');
            });

            try {
                await this.connection.start();
                this.isConnected = true;
                this.isConnecting = false;
                this.dispatchConnectionEvent('turbo:signalr:connected');

                // Notify any waiting callbacks
                this.connectionCallbacks.forEach((cb) => cb());
                this.connectionCallbacks = [];

                return this.connection;
            } catch (error) {
                this.isConnecting = false;
                this.dispatchConnectionEvent('turbo:signalr:error', { error });
                throw error;
            }
        },

        /**
         * Closes the current connection.
         */
        async closeConnection() {
            if (this.connection) {
                await this.connection.stop();
                this.connection = null;
                this.hubUrl = null;
                this.isConnected = false;
                this.subscribedStreams.clear();
            }
        },

        /**
         * Subscribes to a stream, incrementing the reference count.
         * @param {string} streamName - The stream name to subscribe to
         * @param {string} hubUrl - The SignalR hub URL
         * @returns {Promise<boolean>} True if subscription was successful
         */
        async subscribe(streamName, hubUrl) {
            if (!streamName || typeof streamName !== 'string') {
                throw new Error('Stream name is required and must be a string');
            }

            const trimmedName = streamName.trim();
            if (trimmedName.length === 0) {
                throw new Error('Stream name cannot be empty or whitespace');
            }

            // Increment reference count
            const currentCount = this.streamRefs.get(trimmedName) || 0;
            this.streamRefs.set(trimmedName, currentCount + 1);

            // If this is the first reference, subscribe on the server
            if (currentCount === 0) {
                try {
                    const connection = await this.getConnection(hubUrl);
                    const success = await connection.invoke('Subscribe', trimmedName);

                    if (success) {
                        this.subscribedStreams.add(trimmedName);
                        return true;
                    } else {
                        // Subscription was denied (authorization failed)
                        this.streamRefs.set(trimmedName, currentCount);
                        return false;
                    }
                } catch (error) {
                    // Rollback reference count on error
                    this.streamRefs.set(trimmedName, currentCount);
                    throw error;
                }
            }

            return true;
        },

        /**
         * Unsubscribes from a stream, decrementing the reference count.
         * @param {string} streamName - The stream name to unsubscribe from
         * @returns {Promise<void>}
         */
        async unsubscribe(streamName) {
            if (!streamName) {
                return;
            }

            const trimmedName = streamName.trim();
            const currentCount = this.streamRefs.get(trimmedName) || 0;

            if (currentCount <= 0) {
                return;
            }

            const newCount = currentCount - 1;

            if (newCount === 0) {
                // Last reference, unsubscribe from server
                this.streamRefs.delete(trimmedName);
                this.subscribedStreams.delete(trimmedName);

                if (this.connection && this.isConnected) {
                    try {
                        await this.connection.invoke('Unsubscribe', trimmedName);
                    } catch (error) {
                        // Ignore errors during unsubscribe (connection might be closing)
                        // eslint-disable-next-line no-console
                        console.warn('Error unsubscribing from stream:', error);
                    }
                }
            } else {
                this.streamRefs.set(trimmedName, newCount);
            }
        },

        /**
         * Resubscribes to all active streams after reconnection.
         */
        async resubscribeAll() {
            if (!this.connection || !this.isConnected) {
                return;
            }

            const streamsToResubscribe = Array.from(this.streamRefs.keys());

            for (const streamName of streamsToResubscribe) {
                try {
                    const success = await this.connection.invoke('Subscribe', streamName);
                    if (success) {
                        this.subscribedStreams.add(streamName);
                    }
                } catch (error) {
                    // eslint-disable-next-line no-console
                    console.warn('Error resubscribing to stream:', streamName, error);
                }
            }
        },

        /**
         * Handles incoming Turbo Stream messages.
         * @param {string} html - The Turbo Stream HTML
         */
        handleTurboStream(html) {
            if (!html) {
                return;
            }

            // Check if Turbo.js is available
            if (typeof window !== 'undefined' && window.Turbo && typeof window.Turbo.renderStreamMessage === 'function') {
                window.Turbo.renderStreamMessage(html);
            } else {
                // Fallback: manually insert the stream actions
                this.manualRenderStream(html);
            }
        },

        /**
         * Manually renders Turbo Stream HTML when Turbo.js is not available.
         * @param {string} html - The Turbo Stream HTML
         */
        manualRenderStream(html) {
            const template = document.createElement('template');
            template.innerHTML = html;

            const streams = template.content.querySelectorAll('turbo-stream');

            for (const stream of streams) {
                const action = stream.getAttribute('action');
                const target = stream.getAttribute('target');
                const templateContent = stream.querySelector('template');

                if (!target) {
                    continue;
                }

                const targetElement = document.getElementById(target);

                if (!targetElement && action !== 'remove') {
                    continue;
                }

                const content = templateContent ? templateContent.content.cloneNode(true) : null;

                switch (action) {
                    case 'append':
                        if (content) {
                            targetElement.appendChild(content);
                        }
                        break;
                    case 'prepend':
                        if (content) {
                            targetElement.prepend(content);
                        }
                        break;
                    case 'replace':
                        if (content && targetElement.parentNode) {
                            targetElement.parentNode.replaceChild(content, targetElement);
                        }
                        break;
                    case 'update':
                        if (targetElement) {
                            targetElement.innerHTML = '';
                            if (content) {
                                targetElement.appendChild(content);
                            }
                        }
                        break;
                    case 'remove':
                        if (targetElement) {
                            targetElement.remove();
                        }
                        break;
                    case 'before':
                        if (content && targetElement.parentNode) {
                            targetElement.parentNode.insertBefore(content, targetElement);
                        }
                        break;
                    case 'after':
                        if (content && targetElement.parentNode) {
                            targetElement.parentNode.insertBefore(content, targetElement.nextSibling);
                        }
                        break;
                }
            }
        },

        /**
         * Dispatches a custom event on the document.
         * @param {string} eventName - The event name
         * @param {object} detail - Optional event detail
         */
        dispatchConnectionEvent(eventName, detail = {}) {
            if (typeof document !== 'undefined') {
                document.dispatchEvent(new CustomEvent(eventName, { detail }));
            }
        },

        /**
         * Gets the current connection state.
         * @returns {{ isConnected: boolean, streamCount: number, streams: string[] }}
         */
        getState() {
            return {
                isConnected: this.isConnected,
                streamCount: this.streamRefs.size,
                streams: Array.from(this.streamRefs.keys())
            };
        }
    };

    /**
     * Custom element for Turbo Stream sources via SignalR.
     *
     * @example
     * <turbo-stream-source-signalr stream="notifications" hub-url="/turbo-hub">
     * </turbo-stream-source-signalr>
     */
    class TurboStreamSourceSignalR extends HTMLElement {
        static get observedAttributes() {
            return ['stream', 'hub-url'];
        }

        constructor() {
            super();
            this._streamName = null;
            this._hubUrl = null;
            this._subscribed = false;
        }

        /**
         * Called when the element is added to the DOM.
         */
        async connectedCallback() {
            this._streamName = this.getAttribute('stream');
            this._hubUrl = this.getAttribute('hub-url') || '/turbo-hub';

            if (this._streamName) {
                await this._subscribe();
            }
        }

        /**
         * Called when the element is removed from the DOM.
         */
        async disconnectedCallback() {
            if (this._subscribed && this._streamName) {
                await connectionManager.unsubscribe(this._streamName);
                this._subscribed = false;
            }
        }

        /**
         * Called when an observed attribute changes.
         * @param {string} name - Attribute name
         * @param {string|null} oldValue - Previous value
         * @param {string|null} newValue - New value
         */
        async attributeChangedCallback(name, oldValue, newValue) {
            if (oldValue === newValue) {
                return;
            }

            if (name === 'stream') {
                // Unsubscribe from old stream
                if (this._subscribed && this._streamName) {
                    await connectionManager.unsubscribe(this._streamName);
                    this._subscribed = false;
                }

                // Subscribe to new stream
                this._streamName = newValue;
                if (this._streamName && this.isConnected) {
                    await this._subscribe();
                }
            } else if (name === 'hub-url') {
                this._hubUrl = newValue || '/turbo-hub';

                // Reconnect with new URL if already subscribed
                if (this._subscribed && this._streamName) {
                    await connectionManager.unsubscribe(this._streamName);
                    this._subscribed = false;
                    await this._subscribe();
                }
            }
        }

        /**
         * Subscribes to the current stream.
         */
        async _subscribe() {
            if (!this._streamName) {
                return;
            }

            // Ensure hubUrl is set (may not be if attributeChangedCallback fires for 'stream' before 'hub-url')
            if (!this._hubUrl) {
                this._hubUrl = this.getAttribute('hub-url') || '/turbo-hub';
            }

            try {
                const success = await connectionManager.subscribe(this._streamName, this._hubUrl);
                this._subscribed = success;

                if (!success) {
                    this.dispatchEvent(new CustomEvent('turbo:stream:unauthorized', {
                        bubbles: true,
                        detail: { stream: this._streamName }
                    }));
                }
            } catch (error) {
                this.dispatchEvent(new CustomEvent('turbo:stream:error', {
                    bubbles: true,
                    detail: { stream: this._streamName, error }
                }));
            }
        }

        /**
         * Gets the current stream name.
         * @returns {string|null}
         */
        get stream() {
            return this._streamName;
        }

        /**
         * Sets the stream name.
         * @param {string} value
         */
        set stream(value) {
            this.setAttribute('stream', value);
        }

        /**
         * Gets the hub URL.
         * @returns {string}
         */
        get hubUrl() {
            return this._hubUrl || '/turbo-hub';
        }

        /**
         * Sets the hub URL.
         * @param {string} value
         */
        set hubUrl(value) {
            this.setAttribute('hub-url', value);
        }
    }

    // Register the custom element
    if (typeof customElements !== 'undefined') {
        customElements.define('turbo-stream-source-signalr', TurboStreamSourceSignalR);
    }

    // Export a helper to get connection state
    function getConnectionState() {
        return connectionManager.getState();
    }

    // Export a helper to manually close the connection
    async function disconnect() {
        await connectionManager.closeConnection();
    }

    exports.TurboStreamSourceSignalR = TurboStreamSourceSignalR;
    exports.connectionManager = connectionManager;
    exports.disconnect = disconnect;
    exports.getConnectionState = getConnectionState;

}));
//# sourceMappingURL=turbo-signalr.js.map
