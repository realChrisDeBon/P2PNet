
# redundantly remove
docker network rm lan_peer_network
# create NAT network
docker network create -d "nat" lan_peer_network

# build LAN network test 
docker-compose build --no-cache
# remove unneeded network
docker network rm p2pnet_lan_peer_network

# run LAN network test
docker-compose up