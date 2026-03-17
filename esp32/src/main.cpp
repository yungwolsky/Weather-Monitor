#include <Arduino.h>
#include <../lib/dht11/dht11.h>
#include <../lib/MQ135/MQ135.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <map>
#include <string>
#include <time.h>

// Sensores pins
#define DHT11PIN 4
#define MQ135PIN 32

// To configurate
const char* ssid = "";
const char* password = "";
const char* location = "";
const char* apiEndpoint = "http://<YOUR_PC_IP>:5273/api/influx/write";

dht11 DHT11;
MQ135 airSensore(MQ135PIN, 76.63, 10.0);

float humidity, temperature, airPPM;

void initWifi()
{
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);

  Serial.print("Connecting to WiFi...");

  while(WiFi.status() != WL_CONNECTED)
  {
    Serial.print('.');
    delay(1000);
  }

  Serial.println(WiFi.localIP());
}

void getReadings()
{
  int chk1 = DHT11.read(DHT11PIN);

  humidity = DHT11.humidity;
  temperature = DHT11.temperature;
  airPPM = airSensore.getPPM();
}

String makeJSON()
{
  String JSON = "{";
  JSON += "\"location\":\"" + String(location) + "\",";
  JSON += "\"temperature\":" + String(temperature, 2) + ",";
  JSON += "\"humidity\":" + String(humidity, 2) + ",";
  JSON += "\"airPPM\":" + String(airPPM, 2);
  JSON += "}";

  return JSON;
}
void setup() {
  Serial.begin(9600);
  initWifi();
}

void loop() {
  WiFiClient client;
  HTTPClient http;

  Serial.println();
  getReadings();

  Serial.print("Humidity (%): ");
  Serial.println((float)humidity, 2);

  Serial.print("Temperature (C): ");
  Serial.println((float)temperature, 2);

  Serial.print("Air quality (PPM): ");
  Serial.println(airPPM);

  http.begin(client, apiEndpoint);
  http.addHeader("Content-Type", "application/json");

  String payload = makeJSON();

  int httpResponseCode = http.POST(payload);

  Serial.print("HTTP Response code: ");
  Serial.println(httpResponseCode);

  http.end();
  delay(30000);
}
