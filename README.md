# 🧩 DLMS MQTT Client with Credentials

A modified version of the **Gurux DLMS MQTT client** that adds support for **username and password authentication** when connecting to an MQTT broker.

This project allows secure DLMS/COSEM data exchange via MQTT with brokers that require login credentials, while keeping backward compatibility with unauthenticated connections.

---

## License
Licensed under the **GNU General Public License v2.0 only**.  
See the [LICENSE](./LICENSE) file for full details.

---

## 🚀 Features

- 🔑 **Username & Password authentication** support  
- ⚙️ Compatible with **public or private** MQTT brokers  
- 🧱 **Backward compatible** with previous Gurux MQTT client versions  
- 📜 Licensed under **GNU GPL v2.0 only**

---

## 📦 Installation

Clone the repository and build it using .NET:

```bash
git clone https://github.com/yourusername/dlms-mqtt-client-with-credentials.git
cd dlms-mqtt-client-with-credentials
dotnet build
```

## ⚙️ Environment Variables Schema
You can configure MQTT connection settings using environment variables.
These are automatically loaded via .env or system environment configuration.
```env
APP_MqttSettings__Host=localhost
APP_MqttSettings__Port=1883
APP_MqttSettings__Username=user
APP_MqttSettings__Password=pass
```

## 🔧 Changes Made

### 1. Added Username and Password Properties
```cs
public string Username { get; set; }
public string Password { get; set; }
```

### 2. Updated MQTT Connection Options
```cs
var builder = new MqttClientOptionsBuilder()
    .WithTcpServer(serverAddress, port)
    .WithClientId(clientId);

if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
    builder.WithCredentials(Username, Password);

var options = builder.Build();
```

## 🧩 Compatibility

- ✅ Still supports unauthenticated brokers
- ✅ Tested with Mosquitto

## 🏃 Running the Client

After building the project, you can run the DLMS MQTT client from the `dlms-client` directory.  
Use the following command to pass application-specific arguments correctly:

```bash
cd dlms-mqtt-client-with-credentials/dlms-client
dotnet run -- -c 18 -a High -P Gurux -w 1 -f 128 -t Verbose -q <bridge/topic> -g 0.0.1.0.0.255:2
