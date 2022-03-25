import adapter from '@sveltejs/adapter-static';
import sveltePreprocess from 'svelte-preprocess';
import materialPostcss from '@material-svelte/postcss-plugin';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	preprocess: sveltePreprocess({
		postcss: {
			plugins: [materialPostcss()]
		}
	}),
	kit: {
		// hydrate the <div id="svelte"> element in src/app.html
		adapter: adapter()
	}
};

export default config;
