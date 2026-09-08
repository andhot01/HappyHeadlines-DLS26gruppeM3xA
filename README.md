This is a project made for DSL compulsory assignments. 

The docs contain a workspace.dsl file for version control and pictures of the diagrams for human friendly reading.


# Week 36

Implemented the ArticleService with REST CRUD functionality and added both z-axis and x-axis splitting.

## Z-axis split

The ArticleDatabase has been split into 8 PostgreSQL databases:
- Africa
- Antarctica
- Asia
- Europe
- North America
- South America
- Oceania
- Global

`ArticleDbContextFactory` selects the correct database based on the region/continent of the article. The Global database is used for articles that are globally relevant.

## X-axis split

There are now 3 identical instances of `ArticleService`.

NGINX acts as a load balancer in front of the services and distributes incoming requests between the three instances. All three instances use the same regional databases, so the ArticleService itself remains stateless.

The `/instance` endpoint can be used to demonstrate the load balancing:

GET http://localhost:8080/instance

Calling it repeatedly should return different ArticleService container IDs.

## Testing

The API requests can be tested using `ArticleService.http`.

Requests should be sent through the NGINX load balancer on port `8080`

The API supports Create, Read, Update and Delete operations.

> **Note:** There are currently no Docker health checks. When starting from scratch, start the database containers first and give them a few seconds to initialize before starting/building the ArticleService instances and NGINX. Otherwise, ArticleService may try to connect before PostgreSQL is ready.