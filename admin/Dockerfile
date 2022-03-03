FROM node:17 as build
WORKDIR /app
EXPOSE 3000 6868

RUN npm -g install pnpm

COPY package.json pnpm-lock.yaml /app/
RUN pnpm install

COPY . /app
RUN pnpm run build

CMD ["node", "/app/build"]