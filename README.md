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


# Week 38 – DraftService, Logging and Tracing

This week focused on implementing the DraftService and adding reusable
observability to the HappyHeadlines architecture.

## DraftService

A new DraftService was implemented together with a PostgreSQL DraftDatabase.

The service supports CRUD operations for drafts:

- `GET /api/draft` – Get all drafts
- `GET /api/draft/{id}` – Get a specific draft
- `POST /api/draft` – Create a draft
- `PUT /api/draft/{id}` – Update a draft
- `DELETE /api/draft/{id}` – Delete a draft

Entity Framework Core is used for communication with the DraftDatabase.

## Centralized Observability

A shared `Observability` class library was added under:

`Shared/Observability`

The purpose of this project is to keep logging and tracing configuration
centralized and reusable instead of configuring it separately in every
microservice.

Services can enable the shared configuration through extension methods such as:

builder.Logging.AddHappyHeadlinesLogging();
builder.Services.AddHappyHeadlinesTracing("DraftService");

### Logging

Logging is used to record important events that happen while the system is
running.

Structured logging was added to DraftService. Instead of logging everything,
the service focuses on events that are useful when monitoring or debugging the
system:

- `Information` is used when a draft is successfully created, updated or deleted.
- `Warning` is used when an operation references a draft that does not exist.
- `Error` is reserved for unexpected failures.

For example, when a draft is created, its ID is included as a structured value:

info: DraftService.Controllers.DraftController
      Draft 2 created



# Week 39

This week focused on distributed tracing across service boundaries and implementing the publishing and newsletter flow using RabbitMQ.


## PublisherService

`PublisherService` is responsible for publishing new articles.

When an article is published, the service does not communicate directly with the consumers. Instead, the article is published to the RabbitMQ `ArticleQueue`.


`PublisherService` has been added to Docker Compose and is exposed on port `8086`.

## RabbitMQ and fanout messaging

RabbitMQ is used for asynchronous communication between the services.

A fanout exchange is used so that the same published article can be received independently by multiple consumers.


`ArticleService` consumes published articles so they can be persisted in the appropriate regional ArticleDatabase.

`NewsletterService` also receives the published article so that it can react independently without being directly coupled to `ArticleService`.

This allows both services to react to the same event without requiring direct communication between the consumers.

## NewsletterService

`NewsletterService` was implemented to handle newsletter functionality.

For a daily newsletter, the service requests articles from `ArticleService` through the NGINX load balancer:

`Client --> NewsletterService --> NGINX --> ArticleService --> ArticleDatabase`

`NewsletterService` is exposed on port `8085`.

The daily newsletter endpoint can be tested with:

`GET http://localhost:8085/api/newsletter/daily/Europe`

The service selects the latest article returned for the requested region and includes it in the daily newsletter response.

## Distributed tracing

The shared OpenTelemetry configuration from Week 38 is used by:

- `PublisherService`
- `ArticleService`
- `NewsletterService`

All traces are exported to the central Jaeger instance.

Trace context is propagated when crossing service boundaries so that a complete operation can be followed across multiple microservices.


A request to the daily newsletter endpoint produces a single trace containing spans from both `NewsletterService` and `ArticleService`.

Distributed tracing was also implemented across RabbitMQ.

When `PublisherService` publishes an article, the trace context is propagated with the message and restored by the consumers.


A test publication produced one trace in Jaeger containing spans from all three services:

- `PublisherService`
- `ArticleService`
- `NewsletterService`

This demonstrates that the trace is not broken when crossing either HTTP or asynchronous messaging boundaries.

## Centralized tracing with Jaeger

Jaeger is used as the central location for collecting and inspecting distributed traces.

The Jaeger UI is available at:

`http://localhost:16686`

This makes it possible to follow a request across service boundaries and identify which services participated in an operation.

For example, publishing a single article can be followed from `PublisherService`, through RabbitMQ, and into both `ArticleService` and `NewsletterService` as one distributed trace.

# Week 40 – Caching and cache dashboard

## Background

After the z-axis split of the ArticleDatabase, the global database is located in North America and is not replicated. This caused slower response times for users in Europe.

The ARB decided not to use an x-axis split because of the extra cost. Instead, a caching layer was added in front of the global ArticleDatabase and CommentDatabase.

## Technology choices

* **Redis** is used for both caches. There are two separate Redis instances: `article-cache` and `comment-cache`. This makes it possible to measure the two caches separately and means that a problem with one cache does not affect the other.
* **Decorator pattern** is used with `CachedArticleRepository` and `CachedCommentRepository`, which wrap the existing repositories. This means the controllers do not need to be changed.
* **Fail open** is used. If Redis is unavailable, the application uses the database instead. Redis errors are logged but are not returned to the client.

## ArticleCache

The ArticleCache is only used for the **global** ArticleDatabase, since that is the database located in North America. Requests for the seven regional databases do not use the cache and go directly to their databases.

The ArticleCache is filled by an offline process called `ArticleCacheWarmer`.

The warmer runs as a background service and, by default, runs every 10 minutes. It gets articles from the global database from the latest 14 days and stores them in Redis.

Since there are three ArticleService instances, a Redis lock using `SET NX` makes sure that only one instance warms the cache in each round.

The cache entries have a TTL of 30 minutes. This works as a safety net if the warmer stops running.

When reading an article, the service first checks Redis. If the article is not in the cache, it falls back to the database. A cache miss does not add the article to the cache.

When an article is created, it is added to the cache. When an article is updated or deleted, the cached version is invalidated.

One thing to note is that `GET /api/articles/Global` only returns articles from the latest 14 days when the result comes from the cache.

## CommentCache

The CommentCache uses a cache miss approach.

When the comments for an article are requested for the first time, they are loaded from the database and then stored in Redis. Later requests can then use the cached result.

The cache can contain comments for a maximum of 30 articles. A Redis sorted set called `comments:lru` keeps track of when each article was last used.

A Lua script stores the comments and removes the least recently used articles when there are more than 30 articles in the cache. This is done atomically.

When a comment is created or deleted, the cache entry for that article is invalidated.

## Dashboard

Prometheus scrapes `/metrics` from the ArticleService instances and the CommentService.

Both caches expose `cache_hits_total` and `cache_misses_total` with the label `cache="article"` or `cache="comment"`.

Grafana is available at `http://localhost:3000` using `admin/admin`.

The dashboard is called **HappyHeadlines - Cache hit ratio** and shows:

* Total cache hit ratio
* Hit ratio over time
* Hits and misses per second

The dashboard is provisioned from `monitoring/grafana/dashboards`.

## Testing

`./loadtest.sh` generates traffic for both caches. The ArticleCache is tested with requests to `/api/articles/Global`.

The observed results were:

* **ArticleCache:** around 90.9% hit ratio, which is about 10 hits for every miss in the script.
* **CommentCache:** around 69–75% hit ratio. The script uses 40 different articles, while the cache only holds 30, so the LRU cache keeps evicting articles.

Requests for regions other than Global do not use the ArticleCache and are not counted as hits or misses.
