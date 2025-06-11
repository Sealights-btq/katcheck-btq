# Copyright 2020 Google LLC
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

FROM golang:1.20.6-alpine@sha256:6f592e0689192b7e477313264bb190024d654ef0a08fb1732af4f4b498a2e8ad as builder
RUN apk add --no-cache ca-certificates git
RUN apk add build-base
ARG SEALIGHTS_TOKEN
ARG BUILD_NAME
ARG BRANCH
ARG SERVICE_NAME=frontendservice
ENV OS_ARCH linux-amd64
#Golang uses two agents, one for scanning and one for monitoring
ENV AGENT_URL='https://agents.sealights.co/slgoagent/latest/slgoagent-linux-amd64.tar.gz'
ENV AGENT_URL_SLCI='https://agents.sealights.co/slcli/latest/slcli-linux-amd64.tar.gz'

WORKDIR /src

# restore dependencies
COPY go.mod go.sum ./
RUN go mod download
COPY . .

#Downloading the agents 
RUN wget -U "slgoagent" -q -O slgoagent.tar.gz \
   ${AGENT_URL} \
    && tar -xvf slgoagent.tar.gz \
    && rm slgoagent.tar.gz
RUN wget -U "slcli" -q -O slcli.tar.gz \
    ${AGENT_URL_SLCI} \
    && tar -xvf slcli.tar.gz \
    && rm slcli.tar.gz
RUN echo ${SEALIGHTS_TOKEN} >> ./token.txt
#Adding sealights to the code
RUN ./slcli config init --lang go --token ./token.txt
#Getting the build session id 
RUN ./slcli config create-bsid --app ${SERVICE_NAME} --branch ${BRANCH} --build ${BUILD_NAME}
#Scanning the code with sealights
RUN ./slcli scan --bsid buildSessionId.txt --path-to-scanner ./slgoagent --workspacepath ./ --scm none --packages-excluded "github.com/GoogleCloudPlatform/microservices-demo/src/frontend/genproto"


# Skaffold passes in debug-oriented compiler flags
ARG SKAFFOLD_GO_GCFLAGS
RUN go build -gcflags="${SKAFFOLD_GO_GCFLAGS}" -o /go/bin/frontend .

FROM alpine:3.18.2@sha256:82d1e9d7ed48a7523bdebc18cf6290bdb97b82302a8a9c27d4fe885949ea94d1 as release
RUN apk add --no-cache ca-certificates \
    busybox-extras net-tools bind-tools
WORKDIR /src
COPY --from=builder /go/bin/frontend /src/server
COPY ./templates ./templates
COPY ./static ./static

# Definition of this variable is used by 'skaffold debug' to identify a golang binary.
# Default behavior - a failure prints a stack trace for the current goroutine.
# See https://golang.org/pkg/runtime/
ENV GOTRACEBACK=single
ENV SEALIGHTS_LOG_LEVEL=info

EXPOSE 8080
#Running the code with sealights in it
CMD export SEALIGHTS_TOKEN="${SL_TOKEN}" && export SEALIGHTS_LAB_ID="${SL_LAB_ID}" && /src/server
