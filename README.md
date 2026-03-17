# Weather Monitor (ESP32 + ASP.NET + InfluxDB + Grafana)

An end-to-end IoT weather monitoring system using:

* ESP32 (DHT11 + MQ135)
* ASP.NET Web API
* InfluxDB
* Grafana (auto-configured)
* Docker

---

## Quick Start 

```bash
git clone https://github.com/yungwolsky/Weather-Monitor.git
cd Weather-Monitor/infrastructure
docker-compose up --build
```

---

## Access Services

* API → http://localhost:5273
* InfluxDB → http://localhost:8086
* Grafana → http://localhost:3000

Grafana login:

```
admin / admin
```

---

## Grafana Dashboard

* Preloaded automatically
* Uses API as data source
* Supports location filtering

---

## 📡 ESP32 Setup

Edit:

```cpp
const char* ssid = "YOUR_WIFI";
const char* password = "YOUR_PASSWORD";
const char* location = "home";
const char* apiEndpoint = "http://<YOUR_PC_IP>:5273/api/influx/write";
```

Upload using PlatformIO.

---

## Architecture

```
ESP32 → ASP.NET API → InfluxDB → Grafana
```

---

## API Endpoints

### Write data

```
POST /api/influx/write
```

### Read recent data

```
GET /api/influx/read/graphs?location=home
```

### Read averages

```
GET /api/influx/read/average?week=2026-03-10&location=home
```

---

## Notes

* ESP32 must be on same network as your PC (If not, a proper server configuration allowing it should be made)
* Replace `<YOUR_PC_IP>` in firmware
* Do NOT use `localhost` in ESP32

---

MIT
