import { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
	appId: 'net.ntk.admin',
	appName: 'NTK Admin',
	webDir: 'build',
	bundledWebRuntime: true,
	plugins: {
		SplashScreen: {
			launchShowDuration: 0
		}
	},
	server: {
		url: 'http://10.0.2.2:3000',
		cleartext: true
	}
};

export default config;
