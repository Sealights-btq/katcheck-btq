FROM --platform=$BUILDPLATFORM node:20.18.1-alpine@sha256:24fb6aa7020d9a20b00d6da6d1714187c45ed00d1eb4adb01395843c338b9372 AS builder

ARG SEALIGHTS_TOKEN
ARG BUILD_NAME
ENV SEALIGHTS_NODE_AGENT=slnodejs
ARG BRANCH

# Some packages (e.g. @google-cloud/profiler) require additional
# deps for post-install scripts
RUN apk add --update --no-cache \
    python3 \
    make \
    g++

WORKDIR /usr/src/app

COPY package*.json ./
RUN npm install --only=production

#install agent (Not a url)
RUN npm i ${SEALIGHTS_NODE_AGENT}

FROM alpine:3.20.3@sha256:1e42bbe2508154c9126d48c2b8a75420c3544343bf86fd041fb7527e017a4b4a

RUN apk add --no-cache nodejs

WORKDIR /usr/src/app

COPY --from=builder /usr/src/app/node_modules ./node_modules

COPY . .
#run config to get build session id
RUN  ./node_modules/.bin/slnodejs config --token ${SEALIGHTS_TOKEN} --appname "currencyservice" --branch ${BRANCH} --build ${BUILD_NAME}
#sl-scan, scanning the code
RUN ./node_modules/.bin/slnodejs scan --token ${SEALIGHTS_TOKEN} --buildsessionidfile buildSessionId --workspacepath "." --scm none --es6Modules
ENV NODE_DEBUG=sl

EXPOSE 7000
#Running the code with sealights
CMD  ./node_modules/.bin/slnodejs run --token "${SL_TOKEN}" --buildsessionidfile buildSessionId --labid "${SL_LAB_ID}" --workspacepath . server.js
