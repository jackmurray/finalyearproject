-- trivial protocol example
-- declare our protocol
trivial_proto = Proto("mcspkr","Multicast Speaker Control Protocol")
-- create a function to dissect it
function trivial_proto.dissector(buffer,pinfo,tree)
    pinfo.cols.protocol = "MCSPKR"
    local subtree = tree:add(trivial_proto,buffer(),"Multicast Speaker Control Protocol")
    subtree:add(buffer(0,1),"Service ID: " .. buffer(0,1):uint())
    subtree:add(buffer(1,1),"Operation ID: " .. buffer(1,1):uint())
	subtree:add(buffer(2,4),"Message length: " .. buffer(2,4):uint())
end
-- load the udp.port table
tcp_table = DissectorTable.get("tcp.port")
-- register our protocol to handle udp port 7777
tcp_table:add(10451,trivial_proto)