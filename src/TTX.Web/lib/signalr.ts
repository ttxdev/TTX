import { HubConnectionBuilder, MessageHeaders } from "@microsoft/signalr";

export function createHub(hub: string, token?: string) {
  const headers: MessageHeaders = {};
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  return new HubConnectionBuilder()
    .withUrl(`${Deno.env.get("FRESH_PUBLIC_API_BASE_URL")!}/hubs/${hub}`, {
      headers,
      withCredentials: false,
    })
    .withAutomaticReconnect()
    .build();
}
