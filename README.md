# Supervisorio-Reabilitacao

Na máquina virtual, rode esse comando para poder iniciar o container do rplidar:
```bash
docker run -it --rm --network host --privileged -v /dev/bus/usb:/dev/bus/usb brenomcd/rplidar-bridge:v1.0.0
```
