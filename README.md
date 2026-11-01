# Warehouse Logistics Simulator

A C# domain model that simulates the movement of product batches between warehouses. A route is built from individual stages: a truck can travel to a warehouse, receive cargo, and ship cargo to another destination.

## What the project models

| Component | Responsibility |
| --- | --- |
| `Sku` and `ProductBatch` | Describe products, quantities, weight, and volume. |
| `ProductStorage` | Stores products within weight and volume limits and performs atomic transfers. |
| `Truck` | Carries cargo and moves between geographic coordinates at a configured speed. |
| `Warehouse` | Receives and ships products through its staff and internal storage. |
| `Employee` and `WarehouseStaff` | Determine whether products can be carried and calculate handling time. |
| `RouteSheet` | Executes an ordered sequence of movement, receiving, and shipping stages. |

## How route execution works

1. A route sheet receives an ordered collection of stages.
2. Each stage validates its own preconditions before changing the truck or warehouse state.
3. Successful stages contribute their duration to the total route time.
4. If a stage fails, execution stops and returns the failure reason, stage index, and elapsed time.

Expected domain failures are represented by typed result objects. Storage transfers are validated before either storage is changed, so a failed operation cannot leave the system in a partially updated state.

## Design highlights

- value objects protect quantities, identifiers, coordinates, speed, duration, weight, and volume;
- route stages share a common abstraction and can be composed in different orders;
- domain errors are returned explicitly instead of being represented by generic booleans;
- mutable collections are not exposed directly;
- the domain model does not depend on a user interface or persistence layer.

## Repository structure

```text
src/WarehouseLogistics/          Domain model and route execution
tests/WarehouseLogistics.Tests/ Unit and end-to-end domain scenarios
```

## Requirements

- .NET SDK 10.0 or newer

## Build and test

```bash
dotnet restore WarehouseLogistics.slnx
dotnet build WarehouseLogistics.slnx --no-restore
dotnet test WarehouseLogistics.slnx --no-build --no-restore
```

The same commands run automatically in GitHub Actions for every push and pull request.
