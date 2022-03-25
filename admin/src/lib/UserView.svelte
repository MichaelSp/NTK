<script lang="ts">
	import { Button } from '@material-svelte/button';
	import { Icon } from '@material-svelte/icon';
	import { mdiContentSave, mdiMinus, mdiPlus, mdiTrashCan } from '@mdi/js';
	import { toTime, User } from './utils';
	import { API } from './api';

	import { createEventDispatcher } from 'svelte';

	export let user: User;
	export let api: API;
	const dispatch = createEventDispatcher();

	let addColor = '#334131';
	let subColor = '#7c5959';

	async function updateTime(mins: number) {
		return api.sendTo(user, { Action: 'ADDTIME', Message: (mins * 60).toString() });
	}

	async function showMessage(str: string = 'Ha! Gotcha!') {
		return api.sendTo(user, { Action: 'SHOWUI', Message: str });
	}

	async function hideUi() {
		return api.sendTo(user, { Action: 'HIDEUI' });
	}
	async function saveChangedTimes() {
		return api.sendTo(user, { Action: 'SAVE_CHANGED_TIMES' });
	}
</script>

<Button on:click={() => dispatch('back')}>back</Button>
{#if !user}
	No one is online
{:else}
	<h1>{user.username}</h1>
	<div class="container">
		<div class="item">
			<label for="uptime">Online since</label>
			<input class="timebox" readonly id="uptime" value={toTime(user.uptime)} />
			<span>Remaining: {toTime((user.allowedTime || 0) - user.uptime)}</span>
		</div>
		<div class="item">
			<label for="allowed">Allowed time</label>
			<input class="timebox" readonly id="allowed" value={toTime(user.allowedTime || 0)} />
			<Button on:click={() => saveChangedTimes()}>
				<Icon slot="icon" path={mdiContentSave} />
			</Button>
		</div>
	</div>
	<div class="container">
		<Button on:click={() => updateTime(1)} backgroundColor={addColor}>
			<Icon slot="icon" path={mdiPlus} />
			1 min
		</Button>
		<Button on:click={() => updateTime(5)} backgroundColor={addColor}>
			<Icon slot="icon" path={mdiPlus} />
			5 min
		</Button>
		<Button on:click={() => updateTime(10)} backgroundColor={addColor}>
			<Icon slot="icon" path={mdiPlus} />
			10 min
		</Button>
		<Button on:click={() => updateTime(10)} backgroundColor={addColor}>
			<Icon slot="icon" path={mdiPlus} />
			30 min
		</Button>
	</div>
	<div class="container">
		<Button on:click={() => updateTime(-1)} backgroundColor={subColor}>
			<Icon slot="icon" path={mdiMinus} />
			1 min
		</Button>
		<Button on:click={() => updateTime(-5)} backgroundColor={subColor}>
			<Icon slot="icon" path={mdiMinus} />
			5 min
		</Button>
		<Button on:click={() => updateTime(-10)} backgroundColor={subColor}>
			<Icon slot="icon" path={mdiMinus} />
			10 min
		</Button>
		<Button on:click={() => updateTime(-10)} backgroundColor={subColor}>
			<Icon slot="icon" path={mdiMinus} />
			30 min
		</Button>
	</div>

	<div class="container">
		<Button on:click={() => updateTime(-user.uptime / 60)}>Done for today!</Button>

		<Button on:click={() => showMessage()}>Show Message</Button>
		<Button on:click={() => hideUi()}>Hide UI</Button>
	</div>

	<div class="container">
		<Button>
			<Icon path={mdiTrashCan} slot="icon" />
			Delete
		</Button>
	</div>
{/if}

<style>
	.container {
		display: flex;
		justify-content: space-between;
		margin-top: 2rem;
	}

	.item {
		width: 50%;
	}

	.timebox {
		display: inline;
		background: #e5e5e5;
		font-size: 1.5rem;
		border: 0;
		margin: 1rem;
	}
</style>
