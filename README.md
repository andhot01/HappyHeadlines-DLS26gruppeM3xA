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



# Week 37

Implemented the `CommentService` and `ProfanityService`, including their own PostgreSQL databases. Direct communication between the two services and fault isolation using a circuit breaker have also been added.

## CommentService

`CommentService` is responsible for storing and retrieving comments belonging to articles.

Each comment contains an `ArticleId`, which connects the comment to an article without storing or owning the article data itself.

The service has its own `CommentDatabase` and supports creating, retrieving and deleting comments through its REST API.

## ProfanityService

`ProfanityService` is responsible for checking comments for profanity.

It has its own `ProfanityDatabase`, which stores the words used by the profanity checker. The service provides endpoints for adding and retrieving profanity words as well as checking whether a given text contains profanity.

## Direct service communication

When a new comment is posted, `CommentService` sends the comment content directly to `ProfanityService` through HTTP.

The flow is:

Client → CommentService → ProfanityService → ProfanityDatabase

If the comment is clean, it is saved in `CommentDatabase`.

If profanity is detected, the comment is rejected and is not stored.

This means `CommentService` owns the comment operation, while `ProfanityService` is only responsible for determining whether the content contains profanity.

## Circuit breaker and fault isolation

A circuit breaker has been added to the HTTP communication from `CommentService` to `ProfanityService`.

If `ProfanityService` becomes unavailable, `CommentService` remains operational instead of failing completely. Operations that do not depend on `ProfanityService`, such as retrieving existing comments, continue to work.

Creating a new comment requires a successful profanity check. If `ProfanityService` is unavailable, the comment is therefore not saved and `CommentService` returns:

`503 Service Unavailable`

After repeated failures, the circuit breaker prevents unnecessary calls to the unavailable service until it can recover.

## Docker

Both new services and databases have been added to Docker Compose:

- `comment-service`
- `comment-db`
- `profanity-service`
- `profanity-db`

`CommentService` is exposed on port `8082` and `ProfanityService` on port `8083`.

Inside Docker, the services communicate using their Docker service names rather than `localhost`:

`comment-service → profanity-service:8080`

> **Note:** There are still no Docker health checks

## Testing

Requests for the individual services can be tested using:

- `CommentService.http`
- `ProfanityService.http`

The complete flow can be tested through `CommentService`:

`POST http://localhost:8082/api/comments`

A clean comment should return `201 Created`, while a comment containing a word stored in `ProfanityDatabase` should return `400 Bad Request`.

Fault isolation can be tested by stopping `profanity-service` and posting another comment. The POST should return `503 Service Unavailable`, while requests for existing comments should continue to return `200 OK`.