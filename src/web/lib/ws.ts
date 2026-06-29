type Envelope = { type?: unknown } & Record<string, unknown>;
export type HubHandler = (payload: Envelope) => void;

const MAX_RECONNECT_DELAY = 30_000;

export class Hub {
  readonly #url: string;
  #socket: WebSocket | null = null;
  readonly #handlers = new Map<string, Set<HubHandler>>();
  #creatorId: number | null = null;
  #playerId: number | null = null;
  #closedByUser = false;
  #reconnectDelay = 1_000;
  #reconnectTimer: number | null = null;
  #stableTimer: number | null = null;

  constructor(url: string) {
    this.#url = url;
  }

  on<T = Envelope>(type: string, handler: (payload: T) => void): this {
    let handlers = this.#handlers.get(type);
    if (!handlers) {
      handlers = new Set();
      this.#handlers.set(type, handlers);
    }
    handlers.add(handler as unknown as HubHandler);
    return this;
  }

  off<T = Envelope>(type: string, handler: (payload: T) => void): this {
    this.#handlers.get(type)?.delete(handler as unknown as HubHandler);
    return this;
  }

  start(): Promise<void> {
    this.#closedByUser = false;
    return this.#connect();
  }

  invoke(method: "SetCreator" | "SetPlayer", id: number): Promise<void> {
    if (method === "SetCreator") {
      this.#creatorId = id;
    } else if (method === "SetPlayer") {
      this.#playerId = id;
    }
    return Promise.resolve();
  }

  stop(): Promise<void> {
    this.#closedByUser = true;
    this.#clearTimers();
    this.#socket?.close();
    this.#socket = null;
    return Promise.resolve();
  }

  #clearTimers(): void {
    if (this.#reconnectTimer !== null) {
      clearTimeout(this.#reconnectTimer);
      this.#reconnectTimer = null;
    }
    if (this.#stableTimer !== null) {
      clearTimeout(this.#stableTimer);
      this.#stableTimer = null;
    }
  }

  #connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (this.#closedByUser) {
        resolve();
        return;
      }

      const socket = new WebSocket(this.#url);
      this.#socket = socket;

      socket.onopen = () => {
        resolve();
        this.#stableTimer = setTimeout(() => {
          this.#reconnectDelay = 1_000;
        }, 5_000);
      };
      socket.onmessage = (event) => this.#dispatch(event.data);
      socket.onerror = (event) => reject(event);
      socket.onclose = () => {
        if (this.#stableTimer !== null) {
          clearTimeout(this.#stableTimer);
          this.#stableTimer = null;
        }
        if (this.#closedByUser) {
          return;
        }
        const delay = this.#reconnectDelay;
        this.#reconnectDelay = Math.min(delay * 2, MAX_RECONNECT_DELAY);
        this.#reconnectTimer = setTimeout(() => {
          this.#reconnectTimer = null;
          this.#connect().catch(() => {});
        }, delay);
      };
    });
  }

  #dispatch(data: unknown): void {
    if (typeof data !== "string") {
      return;
    }

    let envelope: Envelope;
    try {
      envelope = JSON.parse(data) as Envelope;
    } catch {
      return;
    }

    const type = envelope.type;
    if (typeof type !== "string" || !this.#matchesFilter(type, envelope)) {
      return;
    }

    const handlers = this.#handlers.get(type);
    if (!handlers) {
      return;
    }
    for (const handler of handlers) {
      handler(envelope);
    }
  }

  #matchesFilter(type: string, envelope: Envelope): boolean {
    if (type === "UpdateCreatorValueEvent" && this.#creatorId !== null) {
      const vote = envelope.vote as { creator_id?: number } | undefined;
      return vote?.creator_id === this.#creatorId;
    }
    if (type === "UpdatePlayerPortfolioEvent" && this.#playerId !== null) {
      const player = envelope.player as { id?: number } | undefined;
      return player?.id === this.#playerId;
    }
    return true;
  }
}

export function createHub(hub: string, token?: string): Hub {
  const base = Deno.env.get("FRESH_PUBLIC_API_BASE_URL")!;
  const wsBase = base.replace(/^http/, "ws"); // http -> ws, https -> wss
  let url = `${wsBase}/hubs/${hub}`;
  if (token) {
    url += `?access_token=${encodeURIComponent(token)}`;
  }
  return new Hub(url);
}
