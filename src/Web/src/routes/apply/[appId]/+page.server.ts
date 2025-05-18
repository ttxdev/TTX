import { getApiClient } from "$lib";
import { getToken } from "$lib/auth/sessions";
import { error } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";
import type { CreatorApplicationDto, PlayerDto } from "$lib/api";

export const load: PageServerLoad = async ({ params, cookies }) => {
    const { appId } = params;

    const appIdInt = Number(appId)
    if (isNaN(appIdInt)) throw error(400, 'Invalid application ID')

    const client = getApiClient(getToken(cookies) ?? '');
    const self = await client.getSelf();

    if (!self) throw error(401, 'You must be logged in to view this application')
    const app = await client.getCreatorApplication(appIdInt)

    if (!app) throw error(404, 'Application not found')
    if (app.submitter.id !== self.id) throw error(403, 'You cannot view this application')

    return {
        app: app.toJSON() as CreatorApplicationDto,
        user: self.toJSON() as PlayerDto
    }
}