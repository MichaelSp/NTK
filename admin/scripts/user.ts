#!/usr/bin/env node

import {db, User} from "../src/lib/db";
import prompts from "prompts";

const myArgs = process.argv.slice(2);

async function getPassword(username) {
    const response = await prompts({
        type: 'password',
        name: 'password',
        message: `Please enter password for '${username}': `
    });
    return response.password
}

async function addUser(username, password) {
    await db.sync();
    await User.create({username, password});
}

async function updateUser(username, password) {
    await db.sync();
    await User.update({password}, {where: {username}});
}

async function removeUser(username) {
    await db.sync();
    await User.destroy({where: {username}});
}


async function main() {
    const username = myArgs[1]
    if (username === undefined || username === "") {
        console.warn("username is required as argument: 'user add <name>'")
        return
    }

    switch (myArgs[0]) {
        case 'add':
            await addUser(username, await getPassword(username))
            break;
        case 'remove':
            await removeUser(username)
            break;
        case 'update':
            await updateUser(username, await getPassword(username))
            break;
        default:
            console.log('Sorry, that is not something I know how to do. You can try [add|remove|update] <username>')
    }
}

main()