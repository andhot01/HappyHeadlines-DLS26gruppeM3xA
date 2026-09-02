workspace {

    model {

        Publisher = person "Professional Publisher" "Can draft, review and publish articles." {
            tags "Publisher"
        }

        Reader = person "Reader" "Can comment, share articles and subscribe to newsletters." {
            tags "Reader"
        }

        HappyHeadlines = softwareSystem "Happy Headlines" "A positive news platform that allows publishers to create and publish articles, and readers to interact with articles and subscribe to newsletters."

        Publisher -> HappyHeadlines "Uses"
        Reader -> HappyHeadlines "Uses"
    }

    views {

        systemContext HappyHeadlines "SystemContext" {
            include *
            autoLayout lr
        }

        styles {

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
                background #ff3333
                color #ffffff
            }
        }
    }
}