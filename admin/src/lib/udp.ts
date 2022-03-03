import { User } from "./db";
import * as dgram from "dgram";

class Hello {
  User!: string;
  Uptime!: string;
}

export const udpServer = dgram.createSocket("udp4");

udpServer.on("error", (err) => {
  console.log(`server error:\n${err.stack}`);
  udpServer.close();
});

udpServer.on("message", async (msg: Buffer, rinfo) => {
  const hello: Hello = JSON.parse(msg.toString("utf8"));
  await User.upsert({
      username: hello.User,
      uptime_seconds: hello.Uptime,
      ip: rinfo.address
    }, { where: { username: hello.User } }
  );
  console.log(`server got: ${msg} from ${rinfo.address}:${rinfo.port}`);
});

udpServer.on("listening", () => {
  const address = udpServer.address();
  console.log(`server listening ${address.address}:${address.port}`);
});

let running = false;

export function startUdp() {
  if (!running) {
    udpServer.bind(6868);
    running = true;
  }
}

process.on('SIGINT', () => {
  console.log("Caught interrupt signal. Shutdown");
  udpServer.close()
  process.exit(0)
});