// place files you want to import through the `$lib` alias in this folder.
import { TTXClient } from './api';
import { PUBLIC_API_BASE_URL as apiBaseUrl } from '$env/static/public';

export function getApiClient(token: string): TTXClient {
	return new TTXClient(apiBaseUrl, {
		fetch(url: RequestInfo, init?: RequestInit): Promise<Response> {
			if (!init) {
				init = {};
			}
			init.headers ||= new Headers();
			// @ts-expect-error Can't set headers on RequestInit
			init.headers['Authorization'] = 'Bearer ' + token;

			return fetch(url, init);
		}
	});
}
