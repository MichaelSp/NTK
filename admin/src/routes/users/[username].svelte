<script lang="ts">
    import {Button} from "@material-svelte/button";
    import {goto} from "$app/navigation";
    import {Icon} from "@material-svelte/icon";
    import {mdiMinus, mdiPlus, mdiTrashCan} from '@mdi/js';
    import {toTime} from "../../lib/utils";

    export let user
    let addColor = "#334131"
    let subColor = "#7c5959"

    async function updateTime(mins: number) {
        const body = {uptime_seconds: user.uptime_seconds + mins * 60}
        const response = await fetch(`/users/${user.username}`, {
            method: "POST",
            body: JSON.stringify(body),
            headers: {
                accept: 'application/json'
            }
        });
        const answer = await response.json()
        user.uptime_seconds = answer.uptime_seconds
    }

    async function prank() {
        const body = {prank: "Ha! Gotcha!"}
        await fetch(`/users/${user.username}`, {
            method: "POST",
            body: JSON.stringify(body),
            headers: {
                accept: 'application/json'
            }
        });
    }

    async function deleteUser() {
        await fetch(`/users/${user.username}`, {
            method: "DELETE",
            headers: {
                accept: 'application/json'
            }
        });
    }
</script>

<Button on:click={() => goto('/')}>back</Button>
<h1>{user.username}</h1>
<label for="remaining">Time Remaining</label>
<input class="timebox" readonly id="remaining" value="{toTime(user.uptime_seconds)}"/>

<div class="container">
    <Button on:click={()=>updateTime(5)} backgroundColor="{addColor}">
        <Icon slot="icon" path={mdiPlus}/>
        5 min
    </Button>
    <Button on:click={()=>updateTime(10)} backgroundColor="{addColor}">
        <Icon slot="icon" path={mdiPlus}/>
        10 min
    </Button>
    <Button on:click={()=>updateTime(10)} backgroundColor="{addColor}">
        <Icon slot="icon" path={mdiPlus}/>
        20 min
    </Button>
    <Button on:click={()=>updateTime(10)} backgroundColor="{addColor}">
        <Icon slot="icon" path={mdiPlus}/>
        30 min
    </Button>
</div>
<div class="container">
    <Button on:click={()=>updateTime(-5)} backgroundColor="{subColor}">
        <Icon slot="icon" path={mdiMinus}/>
        5 min
    </Button>
    <Button on:click={()=>updateTime(-10)} backgroundColor="{subColor}">
        <Icon slot="icon" path={mdiMinus}/>
        10 min
    </Button>
    <Button on:click={()=>updateTime(-10)} backgroundColor="{subColor}">
        <Icon slot="icon" path={mdiMinus}/>
        20 min
    </Button>
    <Button on:click={()=>updateTime(-10)} backgroundColor="{subColor}">
        <Icon slot="icon" path={mdiMinus}/>
        30 min
    </Button>
</div>

<div class="container">
    <Button on:click={()=>updateTime(-user.uptime_seconds/60)}>Done for today!</Button>

    <Button on:click={()=> prank()}>Prank!</Button>
</div>

<div class="container">
    <Button on:click={() => deleteUser()}>
        <Icon path={mdiTrashCan} slot="icon"/>
        Delete
    </Button>
</div>

<style>
    .container {
        display: flex;
        justify-content: space-between;
        margin-top: 2rem;
    }

    .timebox {
        background: #c5c5c5;
        font-size: 2rem;
        border: 0;
        margin: 1rem
    }
</style>