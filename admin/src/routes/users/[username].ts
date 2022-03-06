import {User} from "../../lib/db";
import {UDP} from "../../lib/udp";

export async function get({params}): Promise<{ body?: any, status?: number }> {
    const username = params.username
    const user = await User.findByPk(username)
    if (user) {
        return {
            body: {user}
        };
    }

    return {
        status: 404
    };
}

async function updateTime(time_delta: string | number, username: string) {
    const delta = typeof time_delta === "string" ? parseInt(time_delta) : time_delta;
    const user = await User.findByPk(username)

    await User.update({
        up_time: user.up_time + delta,
    }, {
        where: {username},
        fields: ['up_time']
    })

    UDP.sendMessage(user, {Action: "ADD_TIME", Message: delta})

    return {status: 201, body: {up_time: user.up_time + delta}};
}

async function showMessage(username: string, message: string, action: string = "SHOW_UI") {
    const user = await User.findByPk(username)
    UDP.sendMessage(user, {
        Action: action,
        Message: message
    })
    return {status: 200}
}

export async function post(p: { request: Request, params: { username?: string } }) {
    const data = await p.request.json()
    const username = p.params.username
    if (typeof username === "string") {
        if (data.time_delta) {
            return await updateTime(data.time_delta, username);
        }
        if (data.showMessage) {
            return await showMessage(username, data.showMessage)
        }
        if (data.hideUi){
            UDP.sendMessage(await User.findByPk(username), {Action: "HIDE_UI"})
            return {status: 200}
        }
        if (data.showImage) {
            return await showMessage(username, data.showImage, "SHOW_IMAGE")
        }
    } else {
        return {status: 404}
    }
}

export async function del(p: { request: Request, params: { username?: string } }) {
    const username = p.params.username
    await User.destroy({where: {username}})
}