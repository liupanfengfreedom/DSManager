-- this:csharpprint("config")
import ('DSManager')
import ('System.Threading')
launchapp = LaunchApp()
launchapp:printv1('C:/Users/liu/Desktop/PlayerSimulator/Debug/DSManager.exe')
-- launchapp:wait(5000)
Thread.Sleep(1000)
-- launchapp:startprocess('C:\\Users\\liu\\Desktop\\PlayerSimulator\\Debug/DSManager.exe')


function Begin()
    print("Begin")

end
function End()
    print("End")
end
function Update(delta)
    -- print("Update",delta)
end
