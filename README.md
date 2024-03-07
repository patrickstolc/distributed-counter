# Distributed Counter

## Overview 
This is an example of a distributed counter, used for e.g. likes on a social media platform. Please note that this 
is a simplified example and does not implement all bells and whistles of a real-world distributed counter.

## Technologies
* .NET 7.0
* RabbitMQ
* RedisSearch

## Building and running the application

### Docker
To build and run the application, you can use the provided `docker-compose.yml` file. 

```shell
docker-compose up --build
```

## Service Hosts

### Counter API
```
http://localhost:8080
```

### Counter Service
```
http://localhost:5050
```

### Notification Service
```
http://localhost:9090
```