<script lang="ts">
	import { Button } from '@material-svelte/button';
	import { FakeClient } from '$lib/fake-client';
	import { UDP } from '@frontall/capacitor-udp';
	import { Hello, toTime } from './utils';
	import { onDestroy } from 'svelte';

	let session: { socketId: number; ipv4: string; ipv6: string } | null = null;
	let timer;
	let uptime = 0;
	let allowedTime = 60 * 60;
	let error = '';

	onDestroy(() => {
		stop();
	});

	async function start() {
		session = await UDP.create();
		await UDP.setBroadcast({ socketId: session.socketId, enabled: true });
		await UDP.bind({ socketId: session.socketId, address: '0.0.0.0', port: 5552 });
		timer = setInterval(tick, 1000);
		return tick();
	}

	async function stop() {
		if (session) {
			await UDP.close({ socketId: session.socketId });
			clearInterval(timer);
			session = null;
		}
	}

	$: isOnline = session != null;

	async function tick() {
		uptime += 1;
		const hello = new Hello();
		hello.AllowedTime = allowedTime.toString();
		hello.Uptime = uptime.toString();
		hello.User = 'random-kid';
		const buffer: string = JSON.stringify(hello);
		try {
			await UDP.send({
				buffer: btoa(buffer),
				address: '255.255.255.255',
				port: 5551,
				socketId: session.socketId
			});
		} catch (e) {
			error = e;
		}
	}

	async function toggleOnline() {
		isOnline ? await stop() : await start();
	}
</script>

<Button on:click={() => toggleOnline()}>{isOnline ? 'Kid goes offline' : 'Kid comes online'}</Button
>
<span>Uptime {toTime(uptime)}</span>
<p>{error}</p>
