<script context="module">
	import { List } from '@material-svelte/list';
	import ListItemUser from '../lib/ListItemUser.svelte';
	import Fake from '../lib/Fake.svelte';

	import { browser, dev } from '$app/env';

	// we don't need any JS on this page, though we'll load
	// it in dev so that we get hot module replacement...
	export const hydrate = dev;

	// ...but if the client-side router is already loaded
	// (i.e. we came here from elsewhere in the app), use it
	export const router = browser;
</script>

<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { Paper } from '@material-svelte/paper';
	import { Hello, sanitizeName, User } from '../lib/utils';
	import { API, Datagram } from '../lib/api';
	import { users } from '../lib/users';
	import UserView from '../lib/UserView.svelte';

	let user: User = null;
	let err = '';
	let api = new API(5551);

	function updateStore(hello: Datagram<Hello>) {
		const name = sanitizeName(hello.data.User);
		users.update((u: User[]) => {
			let currentUser: User = u.find((u: User) => u.username == name);
			if (!currentUser) {
				currentUser = new User(name);
				u.push(currentUser);
			}

			currentUser.uptime = Number.parseInt(hello.data.Uptime);
			currentUser.allowedTime = Number.parseInt(hello.data.AllowedTime);
			currentUser.ip = hello.sender;
			if (user == currentUser) {
				user = currentUser; // trigger svelte update
			}
			return u;
		});
	}

	onMount(() => {
		api.listen<Hello>(
			(msg) => {
				updateStore(msg);
			},
			(e) => {
				err = e;
			}
		);
	});
	onDestroy(() => api.stop());
</script>

<svelte:head>
	<title>Not Today Kids</title>
</svelte:head>

<Paper>
	{#if dev}<Fake />{/if}
	<hr />
	{#if user}
		<UserView {user} {api} on:back={() => (user = null)} />
	{:else}
		<List>
			{#each $users as u}
				<ListItemUser user={u} on:click={() => (user = u)} />
			{:else}
				<p>No kids online</p>
			{/each}
		</List>
	{/if}
	<p>{err}</p>
</Paper>
