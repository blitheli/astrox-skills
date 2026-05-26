# Earth to 2015 XF261 Lambert transfer plan

## User request

- Use the `celestial-transfer` skill.
- Compute Lambert transfer cases from Earth to asteroid `2015 XF261`.
- Earth departure window: `2028-06-01T00:00:00Z/2028-09-01T00:00:00Z`.
- Arrival at asteroid fixed to `2029-04-10T00:00:00Z`.
- Plot Earth departure delta-V magnitude.
- Put all files under `test/earth2xf262/`.
- Show the plotted delta-V range from `0` to `1000 m/s`.

## Skill interpretation

- The selected skill maps this task to `POST /celestial/transfer`.
- Request fields:
  - `DepartureCbName`: `Earth`
  - `ArrivalCbName`: `2015 XF261`
  - `DepartureInterval`: `2028-06-01T00:00:00Z/2028-09-01T00:00:00Z`
  - `ArrivalInterval`: `2029-04-10T00:00:00Z/2029-04-10T00:00:00Z`
  - `DepartureStepDay`: `1`
  - `ArrivalStepDay`: `1`
  - `SunFrameName`: `MeanEclpJ2000`
- `ArrivalElements` are provided from the existing `celestial-transfer` fixture so the API can use fixed asteroid elements directly instead of relying on an MPC lookup.

## Implementation steps

1. Create `test/earth2xf262/transfer-request.json` with the requested date window and asteroid elements.
2. Call `http://astrox.cn:8765/celestial/transfer` with the request file.
3. Save the raw response to `test/earth2xf262/transfer-response.json`.
4. Parse `TransferResults` and export `DV1_Mag` to `test/earth2xf262/earth2xf262-dv1.csv`.
5. Draw `test/earth2xf262/earth2xf262-dv1.svg` using Python standard library only.
6. Fix the plot y-axis to `0-1000 m/s`; values above `1000 m/s` are clipped at the top edge for display.

## Validation criteria

- HTTP request succeeds.
- Response JSON has `IsSuccess == true`.
- Response has 93 transfer cases, one for each daily departure from `2028-06-01` through `2028-09-01`, inclusive.
- Every result has arrival time `2029-04-10T00:00:00.000Z`.
- The generated CSV has 94 lines including the header.
- The generated SVG contains y-axis labels from `0` to `1000 m/s`.

## Validation result

- API call succeeded with `IsSuccess == true`.
- The response message was `Success. 共 93 个转移算例.`.
- The generated result range is `2028-06-01T00:00:00.000Z` through `2028-09-01T00:00:00.000Z`.
- All cases use the fixed arrival time `2029-04-10T00:00:00.000Z`.
- Minimum Earth departure delta-V is `175.140872 m/s` at `2028-08-23T00:00:00.000Z`.
- Maximum Earth departure delta-V in this window is `464.222206 m/s` at `2028-06-01T00:00:00.000Z`.
- Because all values are within `0-1000 m/s`, no points are clipped by the requested plot display range.
