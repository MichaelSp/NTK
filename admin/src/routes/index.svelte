<script context="module">
    import {browser, dev} from '$app/env';

    // we don't need any JS on this page, though we'll load
    // it in dev so that we get hot module replacement...
    export const hydrate = dev;

    // ...but if the client-side router is already loaded
    // (i.e. we came here from elsewhere in the app), use it
    export const router = browser;

    // since there's no dynamic data here, we can prerender
    // it so that it gets served as a static asset in prod
    export const prerender = true;

    function toTime(seconds) {
        var date = new Date(null);
        date.setSeconds(seconds);
        return date.toISOString().substr(11, 8);
    }
</script>

<script>
    import {List, ListItem} from '@material-svelte/list';
    import {Icon} from "@material-svelte/icon";
    import {mdiAccountCircle} from '@mdi/js';

    export let users;
</script>

<svelte:head>
    <title>Not Today Kids</title>
</svelte:head>


<List>
    {#each users as user}
        <ListItem>
            <Icon slot="visual" path={mdiAccountCircle}/>

            {user.username}
            <span slot="secondary">{toTime(user.uptime_seconds / 60 * 10)}</span>
        </ListItem>
    {/each}
</List>

<style>
</style>
