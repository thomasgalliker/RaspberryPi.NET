# RaspberryPi.NET

### Notes

FileNotFOundException: /etc/dhcpcd.conf not found ---> erstellen, falls nicht existiert

country=XX darf nie  zu oberst stehen

country=XX --> XX muss immer upper-letter sein

wpa_supplicant config file:
ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev
update_config=1
country=NL

network={
ssid=" [not an ssid for sure] "
psk=" [this is not the password i wont show it] "
key_mgmt=WPA-PSK
disabled=0
}

# umschalten von AP nach station mode:
# sudo ip link set wlan0 down --> up um sicherzustellen, dass neue DHCP adresse empfangen wird
# nach Config changes in dhcpcd.conf immer sudo systemctl restart dhcpcd



### Troubleshooting
#### Check if wpa_supplicant works:
 ```
wpa_supplicant -B -i wlan0 -c /etc/wpa_supplicant/wpa_supplicant.conf
```

#### Check Log Tail of hostapd
```
journalctl -u hostapd -f
```

sudo dhclient -v eth0

### Links
- Official RaspberryPi documentation: https://www.raspberrypi.com/documentation/computers/configuration.html#setting-up-a-routed-wireless-access-point
- Raspberry Pi als WLAN-Router einrichten (WLAN-Access-Point): https://www.elektronik-kompendium.de/sites/raspberry-pi/2002171.htm

#### Hostapd
- Hostapd Know-how: https://wiki.gentoo.org/wiki/Hostapd

#### wpa_supplicant
- https://wiki.archlinux.org/title/Wpa_supplicant
- https://www.elektronik-kompendium.de/sites/raspberry-pi/1912221.htm
- https://www.elektronik-kompendium.de/sites/raspberry-pi/2002171.htm


https://chriscant.phdcc.com/2010/02/systemstring-hidden-utf8-bom.html

https://github.com/snowdayclub/rpi-wifisetup-ble/blob/5bc236a90fa6c8ccaebef845d62c526fa2b487d2/wpamanager.py

https://www.daemon-systems.org/man/wpa_supplicant.conf.5.html

