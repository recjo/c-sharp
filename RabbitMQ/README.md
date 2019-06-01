We had a big project that required saving JSON from Magento and Shopiy API to RabbitMQ, and another service that read the messages from RabbitMQ and processed them.

As a developer I took the initaitive to read the RabbitMQ documentation, and create the necessary Virtual Hosts, Exchanges, Routing Keys, Queues, and users accounts on both the development and production RabbitMQ server. I also configured a Dead Letter queue for automatic retring of messages. I monitored connnections and queue message activity using the RabbitMQ Management dashboard.

<!--- I wrote [confluence documentation](/RabbitMQ/howto.html) for the other developers on how to set up personal RabbitMQ queues for local development. -->

RabbitTransport class<br />
The [RabbitTransport](/RabbitMQ/RabbitTransport.cs) class is responsible for pushing messages to the RabbitMQ server.

RabbitListener<br />
The [RabbitListener](/RabbitMQ/RabbitListener.cs) class is responsible for listening to a queue to auto-receive messages from the RabbitMQ server.

RabbitApi class<br />
The [RabbitApi](/RabbitMQ/RabbitApi.cs) class is responsible for interacting with the RabbitMQ API, to provide external applications with diagostic information about the RabbitMQ server.

Publish<br />
The [Publish](/RabbitMQ/Publish.cs) class is an example of a producer client that sends messages to the RabbitMQ server.

ListenerService<br />
[ListenerService](/RabbitMQ/ListenerService.cs) is an example of the Windows Service I set up to continually listen and process messages received from the RabbitMQ server.

