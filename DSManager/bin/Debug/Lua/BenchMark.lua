-- this:csharpprint("config")
import ('DSManager')
import ('System.Threading')

this:startprocess('C:/Users/liu/Desktop/player/owner/PlayerSimulator/Debug/DSManager.exe','')
this:startprocess('C:/Users/liu/Desktop/player/owner/PlayerSimulator1/Debug/DSManager.exe','')
this:startprocess('C:/Users/liu/Desktop/player/owner/PlayerSimulator2/Debug/DSManager.exe','')
this:startprocess('C:/Users/liu/Desktop/player/owner/PlayerSimulator3/Debug/DSManager.exe','')
this:startprocess('C:/Users/liu/Desktop/player/owner/PlayerSimulator4/Debug/DSManager.exe','')

for i=0,20 do
    this:startprocess('C:/Users/liu/Desktop/player/guest/PlayerSimulator/Debug/DSManager.exe','')
    this:startprocess('C:/Users/liu/Desktop/player/guest/PlayerSimulator1/Debug/DSManager.exe','')
    this:startprocess('C:/Users/liu/Desktop/player/guest/PlayerSimulator2/Debug/DSManager.exe','')
    this:startprocess('C:/Users/liu/Desktop/player/guest/PlayerSimulator3/Debug/DSManager.exe','')
    this:startprocess('C:/Users/liu/Desktop/player/guest/PlayerSimulator4/Debug/DSManager.exe','')
end


print("Sleep start")
Thread.Sleep(100)
print("Sleep end")


-- launchapp:startprocess('C:\\Users\\liu\\Desktop\\PlayerSimulator\\Debug/DSManager.exe')


function Begin()
    print("Begin")
---------------------------------------------------
    launchapp = LaunchApp()
    launchapp1 = LaunchApp()
    launchapp:funct(function(str)
        print('...'..str)
        return true
    end,"parastr")


end


function End()
    print("End")
end
function Update(delta)
    -- print("Update",delta)
end
