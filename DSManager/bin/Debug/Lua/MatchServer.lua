server={--match server self
    nettype = "kcp",
    kcp={
        port = 7000,
    }
}
remoteserver={--loadbalance server
    nettype = "kcp",
    kcp={
        port = 7001,
        serverip = "192.168.31.252",
    }
}