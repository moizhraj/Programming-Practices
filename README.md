# Programming-Practices
This repo provides few programming best practices that helps in code readability, debugging, troubleshooting your application.

The list so far
1. **Correlation**
When you have a microservices architecture or when your application is divided into multiple components/systems where a single request hops from one to another, its important that you should be able to track each request throughout its lifecycle. To help in such a scenario, correlation Ids can be used to detect and troubleshoot errors creeping in middleware systems.