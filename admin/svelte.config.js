import adapter from '@sveltejs/adapter-node';
import preprocess from 'svelte-preprocess';
import materialPostcss from '@material-svelte/postcss-plugin';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	// Consult https://github.com/sveltejs/svelte-preprocess
	// for more information about preprocessors
	preprocess: preprocess({
		postcss: {
			plugins: [materialPostcss()],
		},
	}),

	kit: {
		adapter: adapter({
			// default options are shown
			out: 'build',
			precompress: true,
			env: {
				path: 'SOCKET_PATH',
				host: 'HOST',
				port: 'PORT',
				origin: 'ORIGIN',
				headers: {
					protocol: 'PROTOCOL_HEADER',
					host: 'HOST_HEADER'
				}
			}
		}),
	}
};

export default config;
