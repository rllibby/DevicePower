# DevicePower
DevicePower for Microsoft Band 2.

A Windows 10 UWP application that interfaces with the Microsoft Band 2. The band tile will display the phone's battery percentage once added to the band device. There are also two background triggers to update the band. The first is a SystemTrigger based on power state changes (charging, discharging, idle, etc). The second is a timer based trigger to peform periodic updating. An AppService has also been added, which lets the phone respond to tile events even when the application isn't running. When the band tile is opened, a background task will process the phone battery data and update the band tile in real time.

