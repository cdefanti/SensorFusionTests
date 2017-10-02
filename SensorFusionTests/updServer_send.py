import triad_openvr
import time
import sys
import socket
import json

v = triad_openvr.triad_openvr()
v.print_discovered_objects()

fps = 1
interval = 1/float(fps)

valid_devices = ["tracker_1",
                 "tracker_2",
                 "tracker_3",
                 "tracker_4",
                 "tracker_5",
                 "tracker_6",
                 "tracker_7",
                 "tracker_8",
                 "tracker_9",
                 "tracker_10",
                 "tracker_11",
                 "tracker_12",
                 "tracking_reference_1",
                 "tracking_reference_2",
                 "controller_1"]

UDP_IP = "192.168.1.101"
UDP_IP2 = "127.0.0.1"
UDP_PORT = 10000
 
print ("UDP target IP:", UDP_IP)
print ("UDP target port:", UDP_PORT)
 
sock = socket.socket(socket.AF_INET, # Internet
                      socket.SOCK_DGRAM) # UDP
data = {}

serial_to_id = {
        "LHB-515DFE25" : "LIGHTHOUSE1",
        "LHB-B2857478" : "LIGHTHOUSE2",
        "LHR-09DD5808" : "Tracker25"
    }

if interval:
    while(True):
        start = time.time()
        txt = ""
        for device_key in v.devices.keys():
            # vvv fix this vvv
            if device_key not in valid_devices or serial_to_id[v.devices[device_key].get_serial()] is not "Tracker25":
                continue
            new_data = {}
            data['valid'] = False
            data_ = v.devices[device_key].get_pose_quaternion()
            data['x'] = data_[0]
            data['y'] = data_[1]
            data['z'] = data_[2]
            data['qw'] = data_[3]
            data['qx'] = data_[4]
            data['qy'] = data_[5]
            data['qz'] = data_[6]

            if abs(data['x']) + abs(data['y']) + abs(data['z']) > 0:
                data['valid'] = True

        jsondata = json.dumps(data)
        
        sock.sendto(jsondata.encode('utf-8'), (UDP_IP, UDP_PORT))
        sock.sendto(jsondata.encode('utf-8'), (UDP_IP2, UDP_PORT))
        print (jsondata)
        
        sleep_time = interval-(time.time()-start)
        if sleep_time>0:
            time.sleep(sleep_time)
