export function toTime(seconds: number) {
	let date = new Date(null);
	date.setSeconds(seconds);
	return date.toISOString().substring(11, 19);
}

export function sanitizeName(username: string) {
	return (username || '').replaceAll(/.*[\\]/g, '');
}

export class Hello {
	User!: string;
	Uptime!: string;
	AllowedTime!: string;
}

export class ReceiveUdpEvent {
	socketId: number;
	buffer: string;
	remoteAddress: string;
}
export class User {
	uptime: number;
	allowedTime: number;
	ip: string;

	constructor(public readonly username: string) {}
}
