interface sendOptions {
    method: string
    path: string
    data?: string
}

export class API {
    async send<T>(sendOpts: sendOptions): Promise<T | string> {
        const opts: RequestInit = {method: sendOpts.method, headers: {}};

        if (sendOpts.data) {
            opts.headers['Content-Type'] = 'application/json';
            opts.body = JSON.stringify(sendOpts.data);
        }

        return await fetch(sendOpts.path, opts)
            .then((r) => r.text())
            .then((json) => {
                try {
                    return JSON.parse(json);
                } catch (err) {
                    return json;
                }
            });
    }

    async get(path) {
        return this.send({method: 'GET', path});
    }

    async del(path) {
        return this.send({method: 'DELETE', path});
    }

    async post<T>(path, data): Promise<T | string> {
        return this.send<T>({method: 'POST', path, data});
    }

    async put(path, data) {
        return this.send({method: 'PUT', path, data});
    }
}