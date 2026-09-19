workspace {

    model {

        // =========================================================
        // PEOPLE
        // =========================================================

        Publisher = person "Professional Publisher" "Can draft, review and publish articles." {
            tags "Publisher"
        }

        Reader = person "Reader" "Can comment, share articles and subscribe to newsletters." {
            tags "Reader"
        }

        OperationsEngineer = person "Operations Engineer" "Monitors the health and performance of Happy Headlines." {
            tags "Operations"
        }


        // =========================================================
        // SOFTWARE SYSTEM
        // =========================================================

        HappyHeadlines = softwareSystem "Happy Headlines" "A positive news platform that allows publishers to create and publish articles, and readers to interact with articles and subscribe to newsletters." {

            // -----------------------------------------------------
            // WEB APPLICATIONS
            // -----------------------------------------------------

            Webapp = container "Webapp" "Allows publishers to write, edit and publish articles." "Web application" {
                tags "WebApp"
            }

            Website = container "Website" "Allows readers to read articles, comment and subscribe." "Web application" {
                tags "WebApp"
            }


            // -----------------------------------------------------
            // MONITORING
            // -----------------------------------------------------
           
            TelemetryCollector = container "OpenTelemetry Collector" "Collects metrics, logs and traces from Happy Headlines and forwards them to the external observability platform." "OpenTelemetry Collector" {
                tags "Monitoring"
            }


            // -----------------------------------------------------
            // SERVICES
            // -----------------------------------------------------

            DraftService = container "DraftService" "Manages article drafts." "Service" {
                tags "Service"
            }

            PublisherService = container "PublisherService" "Handles the article publishing workflow." "Service" {
                tags "Service"
            }

            ProfanityService = container "ProfanityService" "Checks content for profanity." "Service" {
                tags "Service"
            }

            ArticleService = container "ArticleService" "Provides and manages published articles." "Service" {
                tags "Service"
            }

            CommentService = container "CommentService" "Manages comments on articles." "Service" {
                tags "Service"
            }

            SubscriberService = container "SubscriberService" "Manages newsletter subscriptions." "Service" {
                tags "Service"
            }

            NewsletterService = container "NewsletterService" "Sends newsletters to active subscribers." "Service" {
                tags "Service"
            }


            // -----------------------------------------------------
            // DATABASES
            // -----------------------------------------------------

            DraftDatabase = container "DraftDatabase" "Stores article drafts." "Database" {
                tags "Database"
            }

            ProfanityDatabase = container "ProfanityDatabase" "Stores prohibited words." "Database" {
                tags "Database"
            }

            ArticleDatabase = container "ArticleDatabase" "Stores published articles." "Database" {
                tags "Database"
            }

            CommentDatabase = container "CommentDatabase" "Stores article comments." "Database" {
                tags "Database"
            }

            SubscriberDatabase = container "SubscriberDatabase" "Stores subscriber information." "Database" {
                tags "Database"
            }


            // -----------------------------------------------------
            // QUEUES
            // -----------------------------------------------------

            ArticleQueue = container "ArticleQueue" "Queue containing approved articles awaiting processing." "Message Queue" {
                tags "Queue"
            }

            SubscriberQueue = container "SubscriberQueue" "Queue containing new subscribers." "Message Queue" {
                tags "Queue"
            }
        }

        ObservabilityPlatform = softwareSystem "Observability Platform" "External monitoring platform used to visualize metrics, logs and distributed traces." {
            tags "Monitoring"
        }



        // =========================================================
        // RELATIONSHIPS
        // =========================================================

        // Publisher workflow
        Publisher -> Webapp "Uses"
        Webapp -> DraftService "Saves and retrieves drafts"
        DraftService -> DraftDatabase "Stores and retrieves drafts"

        Webapp -> PublisherService "Publishes articles"
        PublisherService -> ProfanityService "Checks article for profanity"
        ProfanityService -> ProfanityDatabase "Retrieves prohibited words"

        PublisherService -> ArticleQueue "Places approved article"
        ArticleQueue -> ArticleService "Provides new published articles"
        ArticleService -> ArticleDatabase "Stores and retrieves articles"


        // Reader workflow
        Reader -> Website "Uses"
        Website -> ArticleService "Fetches articles"

        Website -> CommentService "Posts and retrieves comments"
        CommentService -> CommentDatabase "Stores and retrieves comments"
        CommentService -> ProfanityService "Checks comments for profanity"

        Website -> SubscriberService "Sends subscription request"
        SubscriberService -> SubscriberDatabase "Stores subscriber information"
        SubscriberService -> SubscriberQueue "Adds new subscriber"


        // Newsletter workflow
        ArticleService -> NewsletterService "Provides recent articles"
        SubscriberService -> NewsletterService "Provides active subscriber information"

        //Monitoring lvl 1
        OperationsEngineer -> ObservabilityPlatform "Monitors system health and investigates incidents"
        HappyHeadlines -> ObservabilityPlatform "Sends metrics, logs and traces"

        // Monitoring lvl 2
        Webapp -> TelemetryCollector "Sends telemetry"
        Website -> TelemetryCollector "Sends telemetry"

        DraftService -> TelemetryCollector "Sends metrics, logs and traces"
        PublisherService -> TelemetryCollector "Sends metrics, logs and traces"
        ProfanityService -> TelemetryCollector "Sends metrics, logs and traces"
        ArticleService -> TelemetryCollector "Sends metrics, logs and traces"
        CommentService -> TelemetryCollector "Sends metrics, logs and traces"
        SubscriberService -> TelemetryCollector "Sends metrics, logs and traces"
        NewsletterService -> TelemetryCollector "Sends metrics, logs and traces"

        TelemetryCollector -> ObservabilityPlatform "Exports metrics, logs and traces"
    }


    views {

        // =========================================================
        // LEVEL 1 - SYSTEM CONTEXT
        // =========================================================

        systemContext HappyHeadlines "SystemContext" {
            include Publisher
            include Reader
            include OperationsEngineer
            include HappyHeadlines
            include ObservabilityPlatform

            autoLayout lr
        }


        // =========================================================
        // LEVEL 2 - CONTAINER DIAGRAM
        // =========================================================

        container HappyHeadlines "Containers" {
            include *

            autoLayout lr
        }


        // =========================================================
        // STYLES
        // =========================================================

        styles {

            element "Element" {
                color #ffffff
            }

            element "Person" {
                shape Person
                color #ffffff
            }

            element "Publisher" {
                background #ffb84d
            }

            element "Reader" {
                background #12b8d0
            }

            element "Software System" {
                background #ff4d4d
                color #ffffff
            }

            element "Container" {
                background #f27f7f
                color #ffffff
            }

            element "WebApp" {
                background #ff5757
                color #ffffff
            }

            element "Service" {
                background #f27f7f
                color #ffffff
            }

            element "Database" {
                shape Cylinder
                background #ff6b6b
                color #ffffff
            }

            element "Queue" {
                background #d995e5
                color #ffffff
            }

            element "Monitoring" {
                background #4CAF50
                color #ffffff
            }

            element "Operations" {
                background #4CAF50
                color #ffffff
            }
        }
    }
}