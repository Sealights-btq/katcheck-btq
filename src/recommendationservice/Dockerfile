FROM python:3.10.8-slim@sha256:49749648f4426b31b20fca55ad854caa55ff59dc604f2f76b57d814e0a47c181 as base
FROM base as builder

ARG SEALIGHTS_TOKEN
ARG BRANCH
ARG BUILD_NAME=0.4.3
ENV SEALIGHTS_PYTHON=sealights-python-agent
# Install Git in the base stage
RUN apt-get -qq update \
    && apt-get install -y --no-install-recommends \
        wget g++ git \
    && rm -rf /var/lib/apt/lists/*

# get packages
COPY requirements.txt .
RUN pip install -r requirements.txt
#Install agent (not a url)
RUN pip install ${SEALIGHTS_PYTHON}

FROM builder

ENV PYTHONUNBUFFERED=1
# get packages

COPY --from=builder /usr/local/lib/python3.10/ /usr/local/lib/python3.10/
WORKDIR /recommendationservice
COPY . .
#Get the build session id
RUN sl-python config --appname recommendationservice --branchname ${BRANCH} --buildname ${BUILD_NAME} --workspacepath ./ --token ${SEALIGHTS_TOKEN} --exclude "*grpc*"
#Scan the code
RUN sl-python scan --scm none --token ${SEALIGHTS_TOKEN}
#COPY --from=builder /recommendationservice/buildSessionId.txt .
#RUN export SL_BUILD_SESSION_ID=`cat buildSessionId.txt`
ENV SL_DEBUG=true

ENV PORT "8080"
EXPOSE 8080
#Run the python code with sl-python run command
CMD sl-python run python recommendation_server.py --labid ${SL_LAB_ID} --token ${SL_TOKEN}