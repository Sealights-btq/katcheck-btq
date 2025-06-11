FROM golang:1.20.6-alpine@sha256:6f592e0689192b7e477313264bb190024d654ef0a08fb1732af4f4b498a2e8ad as builder

RUN apk add --no-cache ca-certificates git
RUN apk add build-base
ARG SEALIGHTS_TOKEN
ARG BRANCH
ARG BUILD_NAME
ARG SERVICE_NAME=chechoutservice
ENV AGENT_URL='https://agents.sealights.co/slgoagent/latest/slgoagent-linux-amd64.tar.gz'
ENV AGENT_URL_SLCI='https://agents.sealights.co/slcli/latest/slcli-linux-amd64.tar.gz'
#Golang uses two agents, one for scanning and one for monitoring
ENV OS_ARCH linux-amd64



WORKDIR /src

# restore dependencies
COPY go.mod go.sum ./

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

#Adding sealights to the code
RUN ./slcli config init --lang go --token ${SEALIGHTS_TOKEN}
#Making the build session id 
RUN ./slcli config create-bsid --app ${SERVICE_NAME} --branch ${BRANCH} --build ${BUILD_NAME}
#Scanning the code
RUN ./slcli scan --bsid buildSessionId.txt --path-to-scanner ./slgoagent --workspacepath ./ --scm none --packages-excluded "github.com/GoogleCloudPlatform/microservices-demo/src/checkoutservice/genproto"

RUN go build -gcflags="${SKAFFOLD_GO_GCFLAGS}" -o /checkoutservice .
RUN go mod download

#RUN go build -o main .

FROM alpine:3.18.2@sha256:82d1e9d7ed48a7523bdebc18cf6290bdb97b82302a8a9c27d4fe885949ea94d1
# Skaffold passes in debug-oriented compiler flags
ARG SKAFFOLD_GO_GCFLAGS

RUN apk add --no-cache ca-certificates

COPY --from=builder /checkoutservice /src/checkoutservice

# Definition of this variable is used by 'skaffold debug' to identify a golang binary.
# Default behavior - a failure prints a stack trace for the current goroutine
# See https://golang.org/pkg/runtime/
ENV GOTRACEBACK=single
ENV SEALIGHTS_LOG_LEVEL=info


EXPOSE 5050
CMD export SEALIGHTS_TOKEN="${SL_TOKEN}" && export SEALIGHTS_LAB_ID="${SL_LAB_ID}" && /src/checkoutservice
