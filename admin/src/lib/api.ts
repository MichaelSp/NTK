import { UDP } from '@frontall/capacitor-udp';
import type { ReceiveUdpEvent, User } from './utils';

export class Action {
	Action:
		| 'ADDTIME'
		| 'SHOWUI'
		| 'SHOW_IMAGE' // with Message
		| 'BLOCKTODAY'
		| 'RESETTIME'
		| 'HIDEUI'
		| 'SAVE_CHANGED_TIMES'; // without Message
	Message?: string;
}

interface SocketInfo {
	socketId: number;
	ipv4: string;
	ipv6: string;
}

export class Datagram<T> {
	constructor(public readonly data: T, public readonly sender: string) {}
}

export class API {
	private socket: SocketInfo;

	constructor(private readonly port) {}

	async sendTo<T extends Action>(user: User, message: T | string): Promise<any> {
		const buffer = typeof message === 'string' ? message : JSON.stringify(message);
		return UDP.send({
			socketId: this.socketId,
			address: user.ip,
			port: this.port,
			buffer: btoa(buffer)
		});
	}

	get socketId() {
		return this.socket.socketId;
	}

	async listen<T>(dataCallback: (data: Datagram<T>) => void, errorCallback: (err: string) => void) {
		try {
			this.socket = await UDP.create();
			await UDP.setBroadcast({ socketId: this.socketId, enabled: false });
			await UDP.bind({ socketId: this.socketId, address: '0.0.0.0', port: this.port });
			await UDP.addListener('receive', (params: ReceiveUdpEvent) => {
				const msg = JSON.parse(atob(params.buffer));
				dataCallback(new Datagram<T>(msg, params.remoteAddress));
			});
			await UDP.addListener('receiveError', (e) => {
				errorCallback(e);
			});
		} catch (e) {
			errorCallback(e);
		}
	}

	async stop() {
		if (this.socket) {
			await UDP.close({ socketId: this.socketId });
			this.socket = null;
		}
	}
}
