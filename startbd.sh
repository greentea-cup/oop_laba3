#!/bin/bash
A=$(whoami)
sudo mkdir /run/postgresql
sudo chown -R $A:$A /run/postgresql
postgres -D ./бд
