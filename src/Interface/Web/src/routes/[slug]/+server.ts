import { error, redirect, type RequestHandler } from '@sveltejs/kit';

export const GET: RequestHandler = async ({ params }) => {
	const { slug } = params;

	if (!slug) return error(404, 'Not found');

	return redirect(302, `/channels/${slug}`);
};
