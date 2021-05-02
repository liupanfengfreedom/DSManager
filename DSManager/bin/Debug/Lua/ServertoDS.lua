server={--Load-balancing server self to dsmanager
    nettype = "kcp",
    kcp={
        port = 7001,
    }
}
serverv1={--Load-balancing server self to match server
    nettype = "kcp",
    kcp={
        port = 7002,
    }
}