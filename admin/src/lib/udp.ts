import {User} from "$lib/db";
import * as dgram from "dgram";

class Hello {
    User!: string;
    Uptime!: string;
}

const udpSocket = dgram.createSocket("udp4");

export class UDP {
    private running = false;

    constructor() {
        udpSocket.on("error", this.error.bind(this))
        udpSocket.on("message", this.receiveMessage.bind(this))
        udpSocket.on("listening", this.listening.bind(this))
    }

    static sendMessage(user: User, message: string) {
        udpSocket.send(message, 0,
            message.length, 6868,
            user.ip, (err) => {
                if (err) {
                    console.warn("Unable to message. Error:" + err.message, err);
                }
            });
    }

    static sanitizeName(username: string) {
        return (username || "").replaceAll(/.*[\\]/g, "");
    }

    error(err) {
        console.log(`server error:\n${err.stack}`);
        udpSocket.close();
    }

    async receiveMessage(msg: Buffer, rinfo) {
        const hello: Hello = JSON.parse(msg.toString("utf8"));
        const username = UDP.sanitizeName(hello.User);
        if (username === "") {
            return console.warn("Empty user-name")
        } else {
            await User.upsert({
                    username: username,
                    uptime_seconds: hello.Uptime,
                    ip: rinfo.address
                }
            );
            console.log(`server got: ${msg} from ${rinfo.address}:${rinfo.port}`);
        }
    }

    listening() {
        const address = udpSocket.address();
        console.log(`server listening ${address.address}:${address.port}`);
    }

    startUdp() {
        if (!this.running) {
            udpSocket.bind(6868);
            this.running = true;

            process.on('SIGINT', () => {
                console.log("Caught interrupt signal. Shutdown");
                udpSocket.close()
                process.exit(0)
            });
        }
    }
}