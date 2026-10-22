# MealPlannerApp

Meal Planner application creates the meal plan based on the Meal Catalog and the constraints of the user
(allergens, disliked ingredients, budget, etc.). The Dijkstra's algorithm from the Planning layer searches
for the shortest path in the LayeredGraph with the IndexedBinaryHeap.

## Architecture

Clean architecture creates several layers with dependencies of these layers directed inwards
to the Domain and Planning layers.

- `MealPlanner.Web` depends on `MealPlanner.Application` and `MealPlanner.Infrastructure`
- `MealPlanner.Infrastructure` depends on `MealPlanner.Application`
- `MealPlanner.Application` depends on `MealPlanner.Domain` and `MealPlanner.Planning`

`MealPlanner.Domain` stores all the Entities of the application like Dish, MealPlan, Ingrediet, and etc.
`MealPlanner.Planning` stores the algorithms and does not know anything about layers above: infrastructure and Web

## Details of installation requirements

Need:

- .NET SDK 10
- Docker to run the application fully

SA password for MSSQL is not kept in the repo. Copy `.env.example` into `.env` and set the `MSSQL_SA_PASSWORD`.
It is needed for the container. Application does not connect using `sa` - the login will be made later.

## How to start

```bash
docker compose up --build
```

Web-application: http://localhost:5080
