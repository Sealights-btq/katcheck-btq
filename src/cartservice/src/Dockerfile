# Copyright 2021 Google LLC
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

# https://mcr.microsoft.com/product/dotnet/sdk
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:363f595704ae9f9be51db003e49b75f65197b0a44d39cba6f7e2d9b020458604 AS builder
WORKDIR /app
COPY cartservice.csproj .
RUN dotnet restore cartservice.csproj \
    -r linux-musl-x64
COPY . .
RUN dotnet publish cartservice.csproj \
    -p:PublishSingleFile=false \
    -r linux-musl-x64 \
    --self-contained true \
    -p:PublishTrimmed=True \
    -p:TrimMode=Full \
    -c release \
    -o /cartservice \
    --no-restore

# https://mcr.microsoft.com/product/dotnet/runtime-deps.
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine@sha256:430a298f1b8be2756c168a2a80019d9445a7552e40fdcd0e560ff6d8a0dbe284

WORKDIR /app
COPY --from=builder /cartservice .
EXPOSE 7070
ENV DOTNET_EnableDiagnostics=0 \
    ASPNETCORE_HTTP_PORTS=7070
USER 1000
ENTRYPOINT ["/app/cartservice"]
