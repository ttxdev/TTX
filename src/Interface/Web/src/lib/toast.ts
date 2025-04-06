import { toast, type ToastPosition } from 'svelte-french-toast';

const defaultOptions = {
	duration: 5000,
	position: 'top-right' as ToastPosition
};

export function successToast(message: string) {
	toast.success(message, defaultOptions);
}

export function errorToast(message: string) {
	toast.error(message, defaultOptions);
}
