import {db} from "$lib/db";
import {UDP} from "$lib/udp";

db.sync().then(() => new UDP().startUdp());