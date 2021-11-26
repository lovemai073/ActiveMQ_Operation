# ActiveMQ_Operation

Befor execution Producer and Consumer, please config the MQ information in the following config file.
. src/Consumer/setting.json
. src/Producer/setting.json

The sample as below:
```
{
  "MQconfig": {
    "Account": "YourMQAccount",
    "Password": "MQPassword",
    "AmqpsUrl": "amqps://url.xxxxx.xxx:5762",
    "MessageCount": 100000,
    "MessageDelayTime": 100,
    "Queue": "myqueue"
  }
}
```
