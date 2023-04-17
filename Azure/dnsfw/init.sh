#!/bin/bash
# install software 
sudo apt-get update -y && sudo DEBIAN_FRONTEND=noninteractive apt-get install --no-install-recommends -y bind9 net-tools

# configure Bind9 for forwarding
sudo cat > named.conf.options << EndOFNamedConfOptions
options {
        directory "/var/cache/bind";
        recursion yes;
        allow-query { any; };
        forwarders {
            168.63.129.16;
        };
        forward only;
        dnssec-validation no; # needed for private dns zones
        auth-nxdomain no;    # conform to RFC1035
        listen-on { any; };
};
EndOFNamedConfOptions

sudo cp named.conf.options /etc/bind
sudo service bind9 restart
sudo sysctl --system
