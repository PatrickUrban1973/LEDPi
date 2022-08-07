# LEDPi

## Why this project
Der Grund warum ich dieses Projekt für mich gestartet habe ist einfach. Ich wollte schon immer Uhr haben, die die Sekunden in Millisekunden anzeigt. Als ich weiter darüber nachdachte war mir klar, daß nur das Anzeigen der Zeit zu langweilig war. Sie sollte auch in regelmäßigen Abständen andere Sachen anzeigen können (Bilder, Gifs oder auch visuelle Effekte).

## Hardware
- Raspberry Pi 4 (optimized) einer Pi3 geht aber auch muß nur dann im Code entsprechend optimiert werden
- Adafruit 64x64 RGB LED Matrix Panel (or other supported LED Panel on https://github.com/hzeller/rpi-rgb-led-matrix)
- Adafruit RGB Matrix Bonnet (not needed, but I wanted to use only ONE power supply)
- BTF-LIGHTING DC 5V 10A 50W 100-240V (Important: you need much ampere)
- Ikea SANNAHED Bilderrahmen

## Software requirements
- rpi-rgb-led-matrix (https://github.com/hzeller/rpi-rgb-led-matrix) is used as libary for the communication over GPIO to the LED Matrix
   Get this project git, compile it and copy/rename the needed libaries. The whole process you can also read in the project of hzeller 
- install ttf-mscorefonts-installer
- 

Todo:
- Water ripple
- 3D Knots
- Chaos Game

https://thecodingtrain.com/CodingChallenges/102-2d-water-ripple.html

