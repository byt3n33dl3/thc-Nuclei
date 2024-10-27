FROM golang:1.21-alpine AS build-env
RUN apk add build-base
WORKDIR /app
COPY . /app
RUN go mod download
RUN go build thc-nuclei

FROM alpine:3.18.6

ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8
ENV PIP_NO_CACHE_DIR=off

WORKDIR /usr/src/thc-nuclei

RUN apt-get update && \
    apt-get install -y libffi-dev libxml2-dev libxslt-dev libssl-dev openssl autoconf g++ python3-dev curl git
RUN apt-get update
RUN curl https://sh.rustup.rs -sSf | bash -s -- -y
ENV PATH="/root/.cargo/bin:${PATH}"
RUN cargo --help

COPY . .
RUN pip install .

ENTRYPOINT [ "thc-nuclei" ]