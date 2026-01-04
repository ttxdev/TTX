import { useSignal, useSignalEffect } from "@preact/signals";
import { HubConnectionBuilder } from "@microsoft/signalr";

export function createHub(hub: string) {
  return new HubConnectionBuilder()
    .withUrl(`${Deno.env.get("FRESH_PUBLIC_API_BASE_URL")!}/hubs/${hub}`)
    .withAutomaticReconnect()
    .build();
}
