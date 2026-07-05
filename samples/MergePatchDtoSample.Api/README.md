# MergePatchDtoSample

Small controller-based ASP.NET Core API for exercising the local `MergePatchDto`
repo with curl.

The sample references this repo's source projects directly:

- `../../src/MergePatchDto/MergePatchDto.csproj` as the runtime library
- `../../src/MergePatchDto.Generators/MergePatchDto.Generators.csproj` as an analyzer

The API targets `net10.0` so it runs with this repo's existing SDK/runtime setup.
The package projects still target `netstandard2.0`.

## Run

```bash
dotnet run --project samples/MergePatchDtoSample.Api
```

The launch profile uses `http://localhost:5275`.

## Curl Samples

Reset the in-memory people:

```bash
curl -s -X POST http://localhost:5275/api/people/reset | jq
```

List people:

```bash
curl -s http://localhost:5275/api/people | jq
```

Generated `ApplyTo`, with same-name mapping, `JsonPropertyName`, `PatchTo`,
`PatchUsing`, `JsonNumberHandling`, nested replacement, array replacement, and
`PatchIgnore`:

```bash
curl -i -X PATCH http://localhost:5275/api/people/11111111-1111-1111-1111-111111111111/generated \
  -H 'content-type: application/json' \
  -d '{
    "name": "Ada Updated",
    "email": "ada.updated@example.com",
    "phone": "+46 70 999 99 99",
    "bio": null,
    "age": "29",
    "address": { "city": "Gothenburg", "country": "SE" },
    "skills": ["math", "api design"],
    "requestId": "seen-as-provided-but-not-applied"
  }'
```

Fetch the person after a patch to inspect the updated state:

```bash
curl -s http://localhost:5275/api/people/11111111-1111-1111-1111-111111111111 | jq
```

Nested objects are replacement values. This updates the whole `Address`; because
`country` is omitted from the new address object, the resulting country is
`null` rather than the old value:

```bash
curl -s -X POST http://localhost:5275/api/people/reset >/dev/null && \
curl -i -X PATCH http://localhost:5275/api/people/11111111-1111-1111-1111-111111111111/generated \
  -H 'content-type: application/json' \
  -d '{
    "address": { "city": "Gothenburg" }
  }' && \
curl -s http://localhost:5275/api/people/11111111-1111-1111-1111-111111111111 | jq
```

Targetless `[MergePatch]` with manual `Has` checks:

```bash
curl -i -X PATCH http://localhost:5275/api/people/11111111-1111-1111-1111-111111111111/manual \
  -H 'content-type: application/json' \
  -d '{
    "isActive": false,
    "adminNote": "Disabled by support request"
  }'
```

Generated `ApplyTo` where the target type is an interface:

```bash
curl -i -X PATCH http://localhost:5275/api/people/11111111-1111-1111-1111-111111111111/interface \
  -H 'content-type: application/json' \
  -d '{
    "name": "Updated through interface",
    "email": "updated@example.com",
    "phone": "+46 70 333 33 33"
  }'
```

Strict unknown-property rejection:

```bash
curl -i -X PATCH http://localhost:5275/api/people/11111111-1111-1111-1111-111111111111/strict \
  -H 'content-type: application/json' \
  -d '{ "name": "Strict name", "unknown": true }'
```
