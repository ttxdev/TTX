import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { PUBLIC_API_BASE_URL as apiBaseUrl } from '$env/static/public';

export async function startConnection(hub: string): Promise<HubConnection> {
	const connection = new HubConnectionBuilder()
		.withUrl(`${apiBaseUrl}/hubs/${hub}`, { withCredentials: false })
		.withAutomaticReconnect()
		.build();

	await connection.start();

	return connection;
}
