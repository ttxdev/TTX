import { getApiClient } from '$lib';
import { getToken } from '$lib/auth/sessions';
import type { PageServerLoad } from './$types';
import { redirect, fail } from '@sveltejs/kit';
import type { PlayerDto } from '$lib/api';
import { CreateCreatorApplicationCommand } from '$lib/api';

export const load: PageServerLoad= async ({ cookies }) => {
    const client = getApiClient(getToken(cookies) ?? '');
    const self = await client.getSelf();

    if (!self) return
    // If the user has already applied, redirect to the application page
    // const res = await client.getCreatorApplications(1, 1, self.name)

    // const app = res.data[0]
    // if (app) {
    //     return redirect(307, `/apply/${app.id}`)
    // }

    return {
        user: self.toJSON() as PlayerDto,
    }
}

export const actions = {
    default: async ({ request, cookies }) => {
        const formData = await request.formData();
        const ticker = formData.get('ticker')?.toString().toUpperCase();

        if (!ticker) {
            return fail(400, {
                error: 'Ticker symbol is required'
            });
        }

        if (!/^[A-Z]{1,5}$/.test(ticker)) {
            return fail(400, {
                error: 'Ticker must be 1-5 uppercase letters'
            });
        }

        try {
            const client = getApiClient(getToken(cookies) ?? '');
            const self = await client.getSelf();

            if (!self) {
                return fail(401, {
                    error: 'You must be logged in to submit an application'
                });
            }

            const res = await client.createCreatorApplication(
                new CreateCreatorApplicationCommand({
                    ticker,
                    submitter_id: self.id,
                    username: self.name
                })
            );

            throw redirect(303, `/apply/${res.id}`);
        } catch (e) {
            if (e instanceof Response) throw e;

            return fail(500, {
                error: e instanceof Error ? e.message : 'Failed to submit application. Please try again.'
            });
        }
    }
};