# Game.Zone
- An application to create items and purchase items and put into inventories.
- Admin can create items using Catalog Service.
- Users can add items into there inventories using Inventory Service.
- Ansyncronsous communcation from Catalog Service to Inventory Service using RabbitMQ and MassTransit.
- Users are mananged using Itentity Service.
- Resources are secured using Itentity Service.
- JWT tokens are used.

## Microservices
1. Game.Catalog.Service - web api
2. Game.Inventory.Service - web api
3. Game.Identity.Service - web api


## Others
1. Game.Catalog.Contracts - define contracts used in serveral services
2. Game.Common - common interfaces for entity, repository and setting for MongoDb, MassTransit
3. Game.Frontend - React Frontend 
4. Game.Infra - docker compose file for MongoDB and RabbitMQ
