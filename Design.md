# Paint Inventory & Coating Report — Design Doc

> Living document. Update as decisions resolve. Last consolidated from planning turns 1–4.

## 1. Overview & scope

An internal web app for a coating/paint operation that does two related jobs:

1. **Inventory** — track paint stock (2‑component products) in real time via barcode scanning, across **multiple vendor locations** (own company + subcontractors). Stock In / Stock Out / Transfer / Adjust, with per‑location on‑hand and low‑stock flagging.
2. **Coating reports** — record and print the **PAINT REPORT** (Emerson standard form): per IPO item, the abrasive‑blasting readings, each coat's paint/batch/thickness/environment, total DFT, and the QC tests. Output as **PDF**.

The two are linked but decoupled: a coat *may* consume tracked stock, or may be recorded as inspection‑only when a subcontractor supplies the paint.

## 2. Tech stack (unchanged)

- **Backend:** ASP.NET Core Web API, **.NET 10**, `PaintInventory.Server`. `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, C# 12.
- **Frontend:** Vue 3 + Vite (Composition API), Vuetify, Pinia, Chart.js, `paintinventory.client`.
- **Data:** EF Core + SQL Server. **All tables prefixed `PaintInventory_`.**
- **Host:** IIS, single site (SPA served as static files by the API, `/api` same‑origin), restricted internal LAN, small user base.
- **Dev:** Visual Studio Community (this project).

**Non‑negotiables:** connection strings from machine env vars (`ConnectionStrings__DefaultConnection`), `appsettings.json` keys empty; all DB/IO `async`; parameterized SQL only; project columns (no `SELECT *`); respect DI scoping.

**Data‑access decision:** EF Core LINQ + tracked writes throughout — **no raw SQL**. There is no high‑frequency ingestion loop here (writes are user‑driven scans), so the raw‑SQL exception in the blueprint does not apply.

## 3. Decisions log

| # | Question | Decision |
|---|---|---|
| 1 | Number of coats — total or sequence? | **Both.** Item carries a total; each coat carries its position. |
| 2 | Coat ↔ stock coupling via per‑coat "deduct from stock" toggle? | **Yes.** |
| 3 | Track stock at subcontractor locations? | **Own locations only.** Subcontractor paint is usage/inspection‑only (no stock impact). |
| 4 | Report output format? | **PDF** (matching the Emerson standard form). |
| — | Part A / Part B modelling | **Separate master rows** per component, linked by partner + mix ratio. |
| — | Barcode | GTIN **lookup‑first**; Jotun tins carry only EAN‑13 (no 2D code), so batch/dates are manual at Stock In. |

## 4. Domain model — the grains

| Grain | Entity | Notes |
|---|---|---|
| Product master | `PaintProduct` | One row per GTIN/component. Comp A and Comp B are separate rows linked via `PartnerProductId`. |
| Location / party | `Vendor` | One table, role flags (own company, stores stock, blasts, paints). Serves stock‑location, blast‑vendor, painting‑vendor roles. |
| On‑hand (per location) | `StockBalance` | `(PaintProductId, VendorId)` unique. Concurrency‑guarded (`RowVersion`). |
| Stock ledger | `StockTransaction` | In / Out / Adjustment / Transfer. On‑hand is the sum of the ledger. |
| Report document | `PaintReport` | IPO‑level header (customer, project, prepared‑by). |
| Report item | `PaintReportItem` | One per item (BODY, ACTUATOR): serial, spec, totals, QC tests. |
| Surface prep | `SurfacePrep` | One per item: blasting grade, roughness, environment. |
| Coat | `CoatLine` | One per coat (Primer…4th): paint(s), batches, thickness, environment. |

`LocationHistory` (v1) is **superseded** by `StockBalance` + `StockTransaction` (Transfer). `PaintItem` / `ScanRecord` (v1) are **replaced** by `PaintProduct` / `StockTransaction`.

## 5. Tables (schema reference)

All names carry the `PaintInventory_` prefix. Decimal precision: stock qty/volume `(18,2)`; thickness/roughness `(9,2)`; humidity/temps `(6,2)`.

**`PaintInventory_Products`** — Gtin (unique), ItemCode, ProductName, Description, Component (Single/PartA/PartB), PackVolume, Unit, DefaultShade, RalCode, Manufacturer, MixRatio, PartnerProductId (self‑FK), UnNumber, HazardFlags, TracksExpiry, IsActive, CreatedAt, UpdatedAt.

**`PaintInventory_Vendors`** — Name (unique), IsOwnCompany, StoresStock, DoesBlasting, DoesPainting, IsActive, CreatedAt.

**`PaintInventory_StockBalances`** — PaintProductId, VendorId, OnHandQty, ReorderLevel, UpdatedAt, RowVersion. Unique `(PaintProductId, VendorId)`.

**`PaintInventory_StockTransactions`** — PaintProductId, VendorId, CounterpartyVendorId (transfers), Direction, Quantity, Batch, Shade, PackVolume, ManufacturingDate, BestBefore, Source, CoatLineId (link to consuming coat), Operator, Notes, DeviceId, Timestamp.

**`PaintInventory_Reports`** — Ipo, Customer, Project, PreparedBy, PreparedDate, CreatedAt, UpdatedAt.

**`PaintInventory_ReportItems`** — PaintReportId, ItemNo, SerialNumber, PaintingSpec, ComponentDescription ("Valve Assembly or Leveltrol"), ComponentLabel (BODY/ACTUATOR), AbrasiveBlasting, RequiredTotalDftUm, MeasuredTotalDftUm, AdhesionTestPerformed, AdhesionTestType (None/TestPlate/ProductionPart), MekTestNotes, OtherRemarks, BlastVendorId, PaintingVendorId.

**`PaintInventory_SurfacePreps`** — PaintReportItemId (unique, 1:1), GradeOfCleanliness (SSPC‑SP16…), RequiredRoughness (string range "30‑50"), MeasuredRoughness, HumidityPct, AirTempC, SubstrateTempC (steel temp), DewPointC, Operator, Date.

**`PaintInventory_CoatLines`** — PaintReportItemId, CoatType (Primer/2nd/3rd/4th), Sequence, PartAProductId, PartBProductId, PaintIdText (free‑text fallback), PartABatch, PartBBatch, Shade, RequiredThicknessUm, MeasuredThicknessUm, HumidityPct, AirTempC, SubstrateTempC, DewPointC, Operator, Date, DeductFromStock, StockLocationVendorId.

**`PaintInventory_AuditLogs`** — Entity, EntityId, Action, ChangedBy, ChangedAt, Details.

### Integrity rules
- On‑hand recomputed inside one `SaveChanges` per transaction; Stock Out guarded against negative on‑hand.
- `RowVersion` on `StockBalance` for optimistic concurrency (multiple handhelds on the same product).
- All FKs to `Vendor` use **Restrict** delete (avoids SQL Server multiple‑cascade‑path errors); Report→Item→(SurfacePrep, CoatLine) cascade; StockTransaction→CoatLine is SetNull.
- Products are soft‑deleted (`IsActive`), never hard‑deleted.

## 6. Report structure → schema map (Emerson PAINT REPORT)

Header (IPO/Item) → `PaintReport` + `PaintReportItem`. The **Abrasive Blasting** row → `SurfacePrep`. The coat grid → `CoatLine[]`. Totals + tests → `PaintReportItem`.

| Report field | Lands in |
|---|---|
| IPO, Customer, Project, Prepared By/Date | `PaintReport` |
| Item, Serial No, Painting Spec, component text/label | `PaintReportItem` |
| Grade of cleanliness, Rq'd/Measured roughness, blasting Humidity/Air/Steel/Dew, Operator, Date | `SurfacePrep` |
| Coat row: Paint ID, Batch A / Batch B, Shade, Rq'd thk, Measured thk, Humidity, Air, Substrate, Dew, Operator, Date | `CoatLine` |
| Required / Measured total DFT | `PaintReportItem` |
| Adhesion (Yes/No + Test plate/Prod part), MEK, Other remarks | `PaintReportItem` |

The 12 reporting fields map onto `CoatLine` (fields 2–12) with number‑of‑coats on `PaintReportItem` (field 1, total) and `CoatLine.Sequence` (field 1, position).

## 7. Features

**Core (v2)**
- Barcode/QR scan (Keyence BT‑A500GM as keyboard‑wedge) → GTIN lookup or register‑new‑product.
- Stock In (receipt): source, received qty, batch, mfg/best‑before dates, per location.
- Stock Out / Transfer / Adjustment.
- Stock Level: per‑location on‑hand, low‑stock highlight, search/filter.
- Product master + Vendor/location management.
- Coating report entry: IPO → items → surface prep + coats.
- **PDF report** matching the Emerson form (per item).
- Transaction history / audit; dashboard (KPIs, usage chart); Excel export.

**Should‑have**
- Auto dew‑point from humidity + air temp (Magnus), overridable.
- Substrate‑vs‑dew‑point margin warning (paint only when substrate ≥ dew point + ~3 °C).
- Measured‑vs‑required thickness compliance flag.
- Best‑before / shelf‑life alerts (captured at Stock In).

**Nice‑to‑have**
- Supplier GS1‑128 / DataMatrix parsing (batch/expiry via AIs) where suppliers print 2D codes.
- Label/QR printing; email/Teams reorder alerts; RAL swatch rendering.

## 8. Screens & components (views = skeletons, components = UI)

**Views:** StockInView, StockOutView, StockLevelView, TransferView, DashboardView, ProductMasterView, VendorView, ReportListView, ReportEditView, ReportPdfView, HistoryView, NotFoundView (catch‑all).

**Components:** BarcodeScanField, StockInForm, StockOutForm, StockTransferForm, StockLevelTable, StockLevelPreview, ProductForm/Dialog, VendorTable, ReportHeaderForm, SurfacePrepForm, CoatLineTable/CoatLineRow, TotalsFooter, QcTestsForm, TransactionHistoryTable, LowStockList, StatCard, UsageChart, RalSwatch.

**Stores:** `product`, `vendor`, `stock`, `report`, `refdata`. API on relative `/api` via the existing `http` wrapper + Vite proxy.

## 9. API surface (planned)

- `GET /api/products`, `GET /api/products/{gtin}` (scan lookup), `POST/PUT /api/products`
- `GET /api/vendors`, `POST/PUT /api/vendors`
- `GET /api/inventory` (levels, filter by vendor), `GET /api/inventory/low-stock`, `GET /api/inventory/{id}/history`, `GET /api/inventory/dashboard`, `GET /api/inventory/export`
- `POST /api/stock/in`, `POST /api/stock/out`, `POST /api/stock/transfer`, `POST /api/stock/adjust`
- `GET /api/reports`, `GET /api/reports/{id}`, `POST/PUT /api/reports`, `GET /api/reports/{id}/items/{itemId}/pdf`

## 10. PDF report

The Emerson form is a fixed‑grid document (logo, header block, blasting row, coat grid, totals, test checkboxes, signature). **Decision pending — pick at the reporting slice:**
- **QuestPDF** — best fluent‑layout DX for this grid. *License caveat:* Community license is free only for orgs under ~US$1M revenue; a large parent company likely needs the paid tier. Confirm licensing before adopting.
- **MigraDoc/PdfSharp** — MIT, free for any company; solid table support; slightly more verbose. **Recommended fallback** if a QuestPDF license isn't available.

## 11. Build order (slices)

1. **Data layer + migration** ← *this turn.* Entities, DbContext (`PaintInventory_` tables), fresh migration.
2. Backend: `StockService` (on‑hand recompute + negative guard + concurrency), controllers (Products, Vendors, Stock, Inventory), DTOs, server validation, global error handler, env‑var config.
3. Frontend shell: theme, router (+404), `http`, stores; then `BarcodeScanField`, Stock In/Out/Level.
4. Coating reports: report/coat entry screens + validation; dew‑point + margin helpers.
5. PDF generation (per‑item Emerson form); extend Excel export.
6. Auth (Windows/Negotiate — confirm Keyence browser passes NTLM), publish to IIS, readiness gate.

## 12. Deployment

Single IIS site: ASP.NET Core serves the built Vue `dist/` as static files with SPA fallback; API under `/api`, same‑origin (no CORS). One app pool. Connection string from machine env var. Force HTTPS + HSTS.

## 13. Open items

- Confirm auth mechanism (Windows/Negotiate vs cookie) and whether the Keyence enterprise browser passes NTLM.
- PDF library licensing decision (§10).
- Whether "number of coats" should hard‑cap at 4 or allow stripe/extra coats (currently modelled Primer→4th; extensible).
- Grade‑of‑cleanliness and roughness: free‑text now, or a managed reference list?