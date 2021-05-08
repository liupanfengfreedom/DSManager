server={--loginserver server self
    nettype = "kcp",
    kcp={
        port = 6999,
    }
}
remoteserver={--match server
    nettype = "kcp",
    kcp={
        port = 7000,
        serverip = "192.168.31.252",
    }
}