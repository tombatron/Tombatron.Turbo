/**
 * Tests for turbo-stream-source-signalr custom element
 */

import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

// Mock SignalR before importing the module
const mockConnection = {
    connectionId: 'mock-connection-id',
    start: vi.fn().mockResolvedValue(undefined),
    stop: vi.fn().mockResolvedValue(undefined),
    invoke: vi.fn().mockResolvedValue(true),
    on: vi.fn(),
    onreconnecting: vi.fn(),
    onreconnected: vi.fn(),
    onclose: vi.fn()
};

const mockHubConnectionBuilder = {
    withUrl: vi.fn().mockReturnThis(),
    withAutomaticReconnect: vi.fn().mockReturnThis(),
    configureLogging: vi.fn().mockReturnThis(),
    build: vi.fn().mockReturnValue(mockConnection)
};

vi.mock('@microsoft/signalr', () => ({
    HubConnectionBuilder: vi.fn().mockImplementation(() => mockHubConnectionBuilder),
    LogLevel: {
        Warning: 3
    }
}));

// Import after mocking
import { TurboStreamSourceSignalR, connectionManager, getConnectionState, disconnect } from '../src/turbo-stream-source-signalr.js';

describe('TurboStreamSourceSignalR', () => {
    beforeEach(() => {
        // Reset connection manager state
        connectionManager.connection = null;
        connectionManager.streamRefs.clear();
        connectionManager.subscribedStreams.clear();
        connectionManager.hubUrl = null;
        connectionManager.isConnected = false;
        connectionManager.isConnecting = false;
        connectionManager.connectionCallbacks = [];

        // Reset mocks
        vi.clearAllMocks();
        mockConnection.start.mockResolvedValue(undefined);
        mockConnection.invoke.mockResolvedValue(true);
    });

    afterEach(async () => {
        // Clean up any custom elements
        document.body.innerHTML = '';
        await disconnect();
    });

    describe('Custom Element Registration', () => {
        it('should be defined as a custom element', () => {
            expect(customElements.get('turbo-stream-source-signalr')).toBe(TurboStreamSourceSignalR);
        });

        it('should create element via document.createElement', () => {
            const element = document.createElement('turbo-stream-source-signalr');
            expect(element).toBeInstanceOf(TurboStreamSourceSignalR);
        });

        it('should create element via HTML', () => {
            document.body.innerHTML = '<turbo-stream-source-signalr stream="test"></turbo-stream-source-signalr>';
            const element = document.querySelector('turbo-stream-source-signalr');
            expect(element).toBeInstanceOf(TurboStreamSourceSignalR);
        });
    });

    describe('Observed Attributes', () => {
        it('should observe stream and hub-url attributes', () => {
            expect(TurboStreamSourceSignalR.observedAttributes).toContain('stream');
            expect(TurboStreamSourceSignalR.observedAttributes).toContain('hub-url');
        });
    });

    describe('connectedCallback', () => {
        it('should subscribe to stream when connected to DOM', async () => {
            const element = document.createElement('turbo-stream-source-signalr');
            element.setAttribute('stream', 'test-stream');
            element.setAttribute('hub-url', '/test-hub');

            document.body.appendChild(element);

            // Wait for async operations
            await new Promise(resolve => setTimeout(resolve, 10));

            expect(mockConnection.invoke).toHaveBeenCalledWith('Subscribe', 'test-stream');
        });

        it('should use default hub-url if not specified', async () => {
            const element = document.createElement('turbo-stream-source-signalr');
            element.setAttribute('stream', 'test-stream');

            document.body.appendChild(element);
            await new Promise(resolve => setTimeout(resolve, 10));

            expect(mockHubConnectionBuilder.withUrl).toHaveBeenCalledWith('/turbo-hub');
        });

        it('should not subscribe if no stream attribute', async () => {
            const element = document.createElement('turbo-stream-source-signalr');

            document.body.appendChild(element);
            await new Promise(resolve => setTimeout(resolve, 10));

            expect(mockConnection.invoke).not.toHaveBeenCalled();
        });
    });

    describe('disconnectedCallback', () => {
        it('should unsubscribe when removed from DOM', async () => {
            const element = document.createElement('turbo-stream-source-signalr');
            element.setAttribute('stream', 'test-stream');

            document.body.appendChild(element);
            await new Promise(resolve => setTimeout(resolve, 10));

            // Clear previous calls
            mockConnection.invoke.mockClear();

            document.body.removeChild(element);
            await new Promise(resolve => setTimeout(resolve, 10));

            expect(mockConnection.invoke).toHaveBeenCalledWith('Unsubscribe', 'test-stream');
        });
    });

    describe('attributeChangedCallback', () => {
        it('should resubscribe when stream attribute changes', async () => {
            const element = document.createElement('turbo-stream-source-signalr');
            element.setAttribute('stream', 'stream-1');

            document.body.appendChild(element);
            await new Promise(resolve => setTimeout(resolve, 10));

            mockConnection.invoke.mockClear();

            element.setAttribute('stream', 'stream-2');
            await new Promise(resolve => setTimeout(resolve, 10));

            expect(mockConnection.invoke).toHaveBeenCalledWith('Unsubscribe', 'stream-1');
            expect(mockConnection.invoke).toHaveBeenCalledWith('Subscribe', 'stream-2');
        });
    });

    describe('Property Accessors', () => {
        it('should get and set stream property', () => {
            const element = document.createElement('turbo-stream-source-signalr');
            element.stream = 'my-stream';

            expect(element.getAttribute('stream')).toBe('my-stream');
            expect(element.stream).toBe('my-stream');
        });

        it('should get and set hubUrl property', () => {
            const element = document.createElement('turbo-stream-source-signalr');
            element.hubUrl = '/my-hub';

            expect(element.getAttribute('hub-url')).toBe('/my-hub');
            expect(element.hubUrl).toBe('/my-hub');
        });

        it('should return default hubUrl when not set', () => {
            const element = document.createElement('turbo-stream-source-signalr');
            expect(element.hubUrl).toBe('/turbo-hub');
        });
    });
});

describe('connectionManager', () => {
    beforeEach(() => {
        connectionManager.connection = null;
        connectionManager.streamRefs.clear();
        connectionManager.subscribedStreams.clear();
        connectionManager.hubUrl = null;
        connectionManager.isConnected = false;
        connectionManager.isConnecting = false;
        connectionManager.connectionCallbacks = [];

        vi.clearAllMocks();
        mockConnection.start.mockResolvedValue(undefined);
        mockConnection.invoke.mockResolvedValue(true);
    });

    afterEach(async () => {
        await disconnect();
    });

    describe('getConnection', () => {
        it('should create a new connection', async () => {
            const connection = await connectionManager.getConnection('/test-hub');

            expect(connection).toBe(mockConnection);
            expect(mockHubConnectionBuilder.withUrl).toHaveBeenCalledWith('/test-hub');
            expect(mockConnection.start).toHaveBeenCalled();
        });

        it('should reuse existing connection for same URL', async () => {
            await connectionManager.getConnection('/test-hub');
            mockConnection.start.mockClear();

            await connectionManager.getConnection('/test-hub');

            expect(mockConnection.start).not.toHaveBeenCalled();
        });

        it('should close old connection when URL changes', async () => {
            await connectionManager.getConnection('/hub-1');
            await connectionManager.getConnection('/hub-2');

            expect(mockConnection.stop).toHaveBeenCalled();
        });

        it('should set up TurboStream message handler', async () => {
            await connectionManager.getConnection('/test-hub');

            expect(mockConnection.on).toHaveBeenCalledWith('TurboStream', expect.any(Function));
        });

        it('should set up reconnection handlers', async () => {
            await connectionManager.getConnection('/test-hub');

            expect(mockConnection.onreconnecting).toHaveBeenCalled();
            expect(mockConnection.onreconnected).toHaveBeenCalled();
            expect(mockConnection.onclose).toHaveBeenCalled();
        });
    });

    describe('subscribe', () => {
        it('should subscribe to a stream', async () => {
            await connectionManager.subscribe('test-stream', '/test-hub');

            expect(mockConnection.invoke).toHaveBeenCalledWith('Subscribe', 'test-stream');
            expect(connectionManager.streamRefs.get('test-stream')).toBe(1);
        });

        it('should increment reference count for same stream', async () => {
            await connectionManager.subscribe('test-stream', '/test-hub');
            await connectionManager.subscribe('test-stream', '/test-hub');

            expect(connectionManager.streamRefs.get('test-stream')).toBe(2);
            // Should only invoke Subscribe once (first time)
            expect(mockConnection.invoke).toHaveBeenCalledTimes(1);
        });

        it('should throw on empty stream name', async () => {
            await expect(connectionManager.subscribe('', '/test-hub'))
                .rejects.toThrow('Stream name cannot be empty or whitespace');
        });

        it('should throw on null stream name', async () => {
            await expect(connectionManager.subscribe(null, '/test-hub'))
                .rejects.toThrow('Stream name is required and must be a string');
        });

        it('should throw on whitespace stream name', async () => {
            await expect(connectionManager.subscribe('   ', '/test-hub'))
                .rejects.toThrow('Stream name cannot be empty or whitespace');
        });

        it('should trim stream names', async () => {
            await connectionManager.subscribe('  test-stream  ', '/test-hub');

            expect(mockConnection.invoke).toHaveBeenCalledWith('Subscribe', 'test-stream');
        });

        it('should return false when subscription is denied', async () => {
            mockConnection.invoke.mockResolvedValue(false);

            const result = await connectionManager.subscribe('denied-stream', '/test-hub');

            expect(result).toBe(false);
            expect(connectionManager.streamRefs.get('denied-stream')).toBe(0);
        });
    });

    describe('unsubscribe', () => {
        it('should decrement reference count', async () => {
            await connectionManager.subscribe('test-stream', '/test-hub');
            await connectionManager.subscribe('test-stream', '/test-hub');

            await connectionManager.unsubscribe('test-stream');

            expect(connectionManager.streamRefs.get('test-stream')).toBe(1);
        });

        it('should unsubscribe from server when count reaches zero', async () => {
            await connectionManager.subscribe('test-stream', '/test-hub');
            mockConnection.invoke.mockClear();

            await connectionManager.unsubscribe('test-stream');

            expect(mockConnection.invoke).toHaveBeenCalledWith('Unsubscribe', 'test-stream');
            expect(connectionManager.streamRefs.has('test-stream')).toBe(false);
        });

        it('should not unsubscribe from server when count > 0', async () => {
            await connectionManager.subscribe('test-stream', '/test-hub');
            await connectionManager.subscribe('test-stream', '/test-hub');
            mockConnection.invoke.mockClear();

            await connectionManager.unsubscribe('test-stream');

            expect(mockConnection.invoke).not.toHaveBeenCalledWith('Unsubscribe', 'test-stream');
        });

        it('should handle null stream name gracefully', async () => {
            await expect(connectionManager.unsubscribe(null)).resolves.not.toThrow();
        });
    });

    describe('handleTurboStream', () => {
        it('should call Turbo.renderStreamMessage when available', () => {
            window.Turbo = {
                renderStreamMessage: vi.fn()
            };

            connectionManager.handleTurboStream('<turbo-stream action="append" target="list"></turbo-stream>');

            expect(window.Turbo.renderStreamMessage).toHaveBeenCalledWith(
                '<turbo-stream action="append" target="list"></turbo-stream>'
            );

            delete window.Turbo;
        });

        it('should warn and drop the message when Turbo is not available', () => {
            const warnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
            document.body.innerHTML = '<div id="list"></div>';

            connectionManager.handleTurboStream(
                '<turbo-stream action="append" target="list"><template><p>New item</p></template></turbo-stream>'
            );

            expect(warnSpy).toHaveBeenCalledWith(expect.stringContaining('Turbo.js is not loaded'));
            expect(document.getElementById('list').innerHTML).not.toContain('New item');

            warnSpy.mockRestore();
        });

        it('should handle empty html gracefully', () => {
            expect(() => connectionManager.handleTurboStream('')).not.toThrow();
            expect(() => connectionManager.handleTurboStream(null)).not.toThrow();
        });
    });

    describe('getState', () => {
        it('should return current state', async () => {
            await connectionManager.subscribe('stream-1', '/test-hub');
            await connectionManager.subscribe('stream-2', '/test-hub');

            const state = connectionManager.getState();

            expect(state.isConnected).toBe(true);
            expect(state.streamCount).toBe(2);
            expect(state.streams).toContain('stream-1');
            expect(state.streams).toContain('stream-2');
        });
    });

    describe('dispatchConnectionEvent', () => {
        it('should dispatch custom events', () => {
            const handler = vi.fn();
            document.addEventListener('turbo:signalr:test', handler);

            connectionManager.dispatchConnectionEvent('turbo:signalr:test', { data: 'test' });

            expect(handler).toHaveBeenCalled();
            expect(handler.mock.calls[0][0].detail).toEqual({ data: 'test', connectionId: null });

            document.removeEventListener('turbo:signalr:test', handler);
        });

        it('should include connectionId when connection exists', async () => {
            await connectionManager.subscribe('stream-1', '/test-hub');

            const handler = vi.fn();
            document.addEventListener('turbo:signalr:test', handler);

            connectionManager.dispatchConnectionEvent('turbo:signalr:test', { data: 'test' });

            expect(handler).toHaveBeenCalled();
            expect(handler.mock.calls[0][0].detail.connectionId).toBe('mock-connection-id');

            document.removeEventListener('turbo:signalr:test', handler);
        });
    });
});

describe('Exported Functions', () => {
    beforeEach(() => {
        connectionManager.connection = null;
        connectionManager.streamRefs.clear();
        connectionManager.subscribedStreams.clear();
        connectionManager.hubUrl = null;
        connectionManager.isConnected = false;
        connectionManager.isConnecting = false;
        vi.clearAllMocks();
    });

    describe('getConnectionState', () => {
        it('should return connection state', async () => {
            await connectionManager.subscribe('test-stream', '/test-hub');

            const state = getConnectionState();

            expect(state.isConnected).toBe(true);
            expect(state.streams).toContain('test-stream');
        });
    });

    describe('disconnect', () => {
        it('should close the connection', async () => {
            await connectionManager.getConnection('/test-hub');

            await disconnect();

            expect(mockConnection.stop).toHaveBeenCalled();
            expect(connectionManager.connection).toBeNull();
        });
    });
});

describe('turbo:before-fetch-request header injection', () => {
    beforeEach(() => {
        connectionManager.connection = null;
        connectionManager.streamRefs.clear();
        connectionManager.subscribedStreams.clear();
        connectionManager.hubUrl = null;
        connectionManager.isConnected = false;
        connectionManager.isConnecting = false;
        vi.clearAllMocks();
        mockConnection.start.mockResolvedValue(undefined);
        mockConnection.invoke.mockResolvedValue(true);
    });

    afterEach(async () => {
        await disconnect();
    });

    it('should set X-SignalR-Connection-Id header when connection exists', async () => {
        await connectionManager.getConnection('/test-hub');

        const fetchOptions = { headers: {} };
        const event = new CustomEvent('turbo:before-fetch-request', {
            detail: { fetchOptions }
        });
        document.dispatchEvent(event);

        expect(fetchOptions.headers['X-SignalR-Connection-Id']).toBe('mock-connection-id');
    });

    it('should not set header when no connection exists', () => {
        const fetchOptions = { headers: {} };
        const event = new CustomEvent('turbo:before-fetch-request', {
            detail: { fetchOptions }
        });
        document.dispatchEvent(event);

        expect(fetchOptions.headers['X-SignalR-Connection-Id']).toBeUndefined();
    });
});

describe('Reference Counting', () => {
    beforeEach(() => {
        connectionManager.connection = null;
        connectionManager.streamRefs.clear();
        connectionManager.subscribedStreams.clear();
        connectionManager.hubUrl = null;
        connectionManager.isConnected = false;
        connectionManager.isConnecting = false;
        vi.clearAllMocks();
    });

    it('should correctly count multiple elements for same stream', async () => {
        // Simulate multiple elements subscribing to same stream
        await connectionManager.subscribe('shared-stream', '/hub');
        await connectionManager.subscribe('shared-stream', '/hub');
        await connectionManager.subscribe('shared-stream', '/hub');

        expect(connectionManager.streamRefs.get('shared-stream')).toBe(3);
        // Subscribe should only be called once
        expect(mockConnection.invoke).toHaveBeenCalledTimes(1);
    });

    it('should only unsubscribe when last reference is removed', async () => {
        await connectionManager.subscribe('shared-stream', '/hub');
        await connectionManager.subscribe('shared-stream', '/hub');

        mockConnection.invoke.mockClear();

        // First unsubscribe - should not call server
        await connectionManager.unsubscribe('shared-stream');
        expect(mockConnection.invoke).not.toHaveBeenCalled();
        expect(connectionManager.streamRefs.get('shared-stream')).toBe(1);

        // Second unsubscribe - should call server
        await connectionManager.unsubscribe('shared-stream');
        expect(mockConnection.invoke).toHaveBeenCalledWith('Unsubscribe', 'shared-stream');
    });
});
