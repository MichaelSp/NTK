<script context="module">
    import {List} from '@material-svelte/list';
    import {goto} from '$app/navigation'
    import ListItemUser from "../lib/ListItemUser.svelte";

    import {browser, dev} from '$app/env';

    // we don't need any JS on this page, though we'll load
    // it in dev so that we get hot module replacement...
    export const hydrate = dev;

    // ...but if the client-side router is already loaded
    // (i.e. we came here from elsewhere in the app), use it
    export const router = browser;
</script>

<script>
    // receive JSON from server (push)
    import {Paper} from "@material-svelte/paper";

    export let users;
</script>

<svelte:head>
    <title>Not Today Kids</title>
</svelte:head>

<Paper>
    <List>
        {#each users as user}
           <ListItemUser user="{user}" on:click={() => goto(`/users/${user.username}`)} />
        {:else}
            <p>loading</p>
        {/each}
    </List>
</Paper>

<style>

</style>
