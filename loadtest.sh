#!/bin/bash
ARTICLE=localhost:8084
COMMENT=localhost:8082

# 40 faste artikel-id'er: flere end de 30 som CommentCache kan rumme,
# så LRU smider nogle ud og giver misses
IDS=()
for i in $(seq 1 40); do IDS+=($(uuidgen | tr A-Z a-z)); done

for round in $(seq 1 300); do
  # ArticleCache: mest liste-opslag (hits) og af og til ukendt id (miss)
  curl -s $ARTICLE/api/articles/Europe > /dev/null
  curl -s $ARTICLE/api/articles/Europe > /dev/null
  if (( round % 5 == 0 )); then
    curl -s $ARTICLE/api/articles/Europe/$(uuidgen | tr A-Z a-z) > /dev/null
  fi

  # CommentCache: tilfældig artikel ud af 40
  curl -s $COMMENT/api/comments/article/${IDS[$((RANDOM % 40))]} > /dev/null
  sleep 0.1
done
echo "Done"
