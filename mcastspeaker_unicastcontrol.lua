
trivial_proto = Proto("mcspkr","Multicast Speaker Control Protocol")

function trivial_proto.dissector(buffer,pinfo,tree)
    pinfo.cols.protocol = "MCSPKR"
    local subtree = tree:add(trivial_proto,buffer(),"Multicast Speaker Control Protocol")
	subtree:add(buffer(0,4),"Message length: " .. buffer(0,4):uint())
end

tcp_table = DissectorTable.get("tcp.port")

tcp_table:add(10451,trivial_proto)