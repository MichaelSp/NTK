<script lang="ts">
    import {Button} from "@material-svelte/button";
    import {goto} from "$app/navigation";
    import {Icon} from "@material-svelte/icon";
    import {mdiContentSave, mdiMinus, mdiPlus, mdiTrashCan} from '@mdi/js';
    import {toTime} from "../../lib/utils";
    import {API} from "../../lib/api";

    let api = new API()

    export let user
    let addColor = "#334131"
    let subColor = "#7c5959"

    async function updateTime(mins: number) {
        const body = {time_delta: mins * 60}
        await api.post(`/users/${user.username}`, body)
    }

    async function showMessage(str: string = "Ha! Gotcha!") {
        const body = {showMessage: str}
        await api.post(`/users/${user.username}`, body)
    }

    async function hideUi() {
        const body = {hideUi: true}
        await api.post(`/users/${user.username}`, body)
    }

    async function deleteUser() {
        await api.del(`/users/${user.username}`)
    }

    async function saveChangedTimes() {
        await api.post(`/users/${user.username}`, {saveChangedTimes: true})
    }

    /*
    async function refresh() {
        user = await api.get(`/`)
    }

    let poller
    const setupPoller = () => {
        if (poller) {
            clearInterval(poller)
        }
        poller = setInterval(refresh.bind(this), 2000)
    }

    $: setupPoller()
    */
</script>

{#if !user}
    No one is online
{:else}
    <Button on:click={() => goto('/')}>back</Button>
    <h1>{user.username}</h1>
    <div class="container">
        <p>
            <label for="uptime">Online since</label>
            <input class="timebox" readonly id="uptime" value="{toTime(user.up_time)}"/>
            <span>Remaining: {toTime((user.allowed_time || 0)- user.up_time)}</span>
        </p>
        <p>
            <label for="allowed">Allowed time</label>
            <input class="timebox" readonly id="allowed" value="{toTime(user.allowed_time || 0)}"/>
            <Button on:click={() => saveChangedTimes()}>
                <Icon slot="icon" path="{mdiContentSave}" />
            </Button>
        </p>
    </div>

    <div class="container">
        <Button on:click={()=>updateTime(1)} backgroundColor="{addColor}">
            <Icon slot="icon" path={mdiPlus}/>
            1 min
        </Button>
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
            30 min
        </Button>
    </div>
    <div class="container">
        <Button on:click={()=>updateTime(-1)} backgroundColor="{subColor}">
            <Icon slot="icon" path={mdiMinus}/>
            1 min
        </Button>
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
            30 min
        </Button>
    </div>

    <div class="container">
        <Button on:click={()=>updateTime(-user.up_time/60)}>Done for today!</Button>

        <Button on:click={()=> showMessage()}>Show Message</Button>
        <Button on:click={()=> hideUi()}>Hide UI</Button>
    </div>

    <div class="container">
        <Button on:click={() => deleteUser()}>
            <Icon path={mdiTrashCan} slot="icon"/>
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

    .timebox {
        display: inline;
        background: #e5e5e5;
        font-size: 1.5rem;
        border: 0;
        margin: 1rem
    }
</style>