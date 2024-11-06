#!/bin/sh

. ./common.sh

docker-compose -f $CFG_FOLDER/configs/kafka-single-compose.yaml up -d
