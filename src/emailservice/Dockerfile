FROM python:3.10.8-slim@sha256:49749648f4426b31b20fca55ad854caa55ff59dc604f2f76b57d814e0a47c181 as base
FROM base as builder

ARG SEALIGHTS_TOKEN
ARG BRANCH
ARG BUILD_NAME
ENV SEALIGHTS_PYTHON=sealights-python-agent
#Don't forget to add git
RUN apt-get -qq update \
    && apt-get install -y --no-install-recommends \
        wget g++ git \
    && rm -rf /var/lib/apt/lists/*

# get packages
COPY requirements.txt .
RUN pip install -r requirements.txt
#Installing sealights (Not a url)
RUN pip install ${SEALIGHTS_PYTHON}

FROM builder
# Enable unbuffered logging
ENV PYTHONUNBUFFERED=1
ENV ENABLE_PROFILER=1

COPY --from=builder /usr/local/lib/python3.10/ /usr/local/lib/python3.10/
WORKDIR /email_server
#Important to copy code here before running the scan, otherwise it won't scan the code
COPY . .
#run config to get build session id
RUN sl-python config --appname emailservice --branchname ${BRANCH} --buildname ${BUILD_NAME} --workspacepath ./ --token ${SEALIGHTS_TOKEN} --exclude "*grpc*"
#scan the code
RUN sl-python scan --scm none --token ${SEALIGHTS_TOKEN}
ENV SL_DEBUG=true

# Add the application
ENV PORT "8080"
EXPOSE 8080 
#use sl-python to run the code
CMD sl-python run python email_server.py --labid ${SL_LAB_ID} --token ${SL_TOKEN}
