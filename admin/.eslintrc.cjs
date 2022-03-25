module.exports = {
	root: true,
	extends: ['eslint:recommended', 'prettier'],
	plugins: ['svelte3'],
	overrides: [{ files: ['*.svelte'], processor: 'svelte3/svelte3' }],
	eslintIgnore: ['src/lib/supabase.js'],
	parserOptions: {
		sourceType: 'module',
		ecmaVersion: 2022
	},
	env: {
		browser: true,
		es2017: true,
		node: true
	}
};
