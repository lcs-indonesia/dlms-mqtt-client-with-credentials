using DLMS.Client;
using DLMS.Client.GXMedia.Mqtt;
using DotNetEnv;

Env.Load();
var argString = "-c 18 -a High -P Gurux -w 1 -f 128 -t Verbose -q a898680b-48b2-47a5-acfb-b7a34aa7da7c/1";
var settings = new Settings();
var mqttSettings = new MqttSettings
{
    Host = Env.GetString("APP_MqttSettings__Host", "localhost"),
    Port = Env.GetInt("APP_MqttSettings__Port", 1883),
    Username = Env.GetString("APP_MqttSettings__Username"),
    Password = Env.GetString("APP_APP_MqttSettings__Password"),
};
settings.media = new GXMqtt()
{
    Port = mqttSettings.Port,
    ServerAddress = mqttSettings.Host,
    ClientId = Guid.NewGuid().ToString(),
    Username = mqttSettings.Username,
    Password = mqttSettings.Password,
};



using var client = new DLMSClient(argString.Split(" "), settings);

client.ReadObject([new("0.0.1.0.0.255", 2)]);