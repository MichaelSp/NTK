import {db} from "$lib/db";
import {startUdp} from "$lib/udp";

db.sync().then(() => startUdp());