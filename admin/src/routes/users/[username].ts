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

async function updateTime(data, username: string) {
    const uptime_seconds = data.uptime_seconds
    await User.update({
        uptime_seconds,
    }, {
        where: {username},
        fields: ['uptime_seconds']
    })
    return {status: 201, body: {uptime_seconds}};
}

async function prank(username: string, prank: string) {
    let message = JSON.stringify({
        Action: "PRANK",
        Message: prank
    })
    const user = await User.findByPk(username)

    UDP.sendMessage(user, message)
}

export async function post(p: { request: Request, params: { username?: string } }) {
    const data = await p.request.json()
    const username = p.params.username
    if (typeof username === "string") {

        if (data.uptime_seconds) {
            return await updateTime(data, username);
        }
        if (data.prank) {
            return await prank(username, data.prank)
        }
    } else {
        return {status: 404}
    }
}

export async function del(p: { request: Request, params: { username?: string } }) {
    const username = p.params.username
    await User.destroy({where: {username}})
}