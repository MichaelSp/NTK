<script lang="ts">
	import { ListItem } from '@material-svelte/list';
	import { Icon } from '@material-svelte/icon';
	import { mdiAccountCircle } from '@mdi/js';
	import { createEventDispatcher } from 'svelte';
	import { toTime } from '$lib/utils';

	export let user;

	const dispatch = createEventDispatcher();
	const click = () => dispatch('click');
</script>

<ListItem on:click={click}>
	<Icon slot="visual" path={mdiAccountCircle} />

	<span slot="primary">{user.username}</span>
	<span slot="secondary">{toTime(user.uptime)}</span>
	<span slot="tertiary" class="small">{user.ip}</span>
</ListItem>

<style>
	.small {
		font-size: 10px;
	}
</style>
