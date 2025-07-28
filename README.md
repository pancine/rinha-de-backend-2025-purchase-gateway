# Description
Service implementation for a backend challenge named `rinha de backend`.

The challenge invites developers to build a payment gateway that mediates transactions to two external often unstable Payment Processors. The core task is to maximize profit by deciding which payment processor to use. Also, I had to maintain data consistency due to penalties in case of discrepancies. 

Participants must optimize for performance, with bonuses awarded for achieving extremely low p99 response times. The solution needs to be deployed as a Docker Compose setup with strict resource constraints, making it a comprehensive test of resilient system design, cost optimization, and high-performance backend development.

For full details visit: [rinha de backend](https://github.com/zanfranceschi/rinha-de-backend-2025/tree/main)

## Implementation Details
Link to the repository: https://github.com/pancine/rinha-de-backend-2025-purchase-gateway

In summary, upon receiving a payment request, the API places the request into a queue using the native Channel functionality.

An x number of workers (PURCHASE_WORKERS in docker-compose) running in the background pull messages from this queue, process the payment, and then asynchronously send it to the database.

## Technologies Used
- Redis
- .NET
- PostgreSQL