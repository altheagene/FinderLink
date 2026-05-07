# FinderLink Database Design Documentation

## Entity-Relationship Diagram (ERD)

```
┌─────────────────────────────────────────────────────────────────┐
│                        DATABASE SCHEMA                           │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────────┐
│         Users                │
├──────────────────────────────┤
│ PK │ UserId (int)            │
│    │ Name (varchar)          │
│    │ Email (varchar) UNIQUE  │
│    │ Password (varchar)      │
│    │ Role (varchar)          │
│    │ CreatedAt (datetime2)   │
└──────────────┬───────────────┘
               │
      ┌────────┴────────┐
      │                 │
      ▼                 ▼
┌──────────────────────┐   ┌──────────────────────────────────┐
│      Items           │   │      Claims                      │
├──────────────────────┤   ├──────────────────────────────────┤
│ PK │ ItemId          │   │ PK │ ClaimId                     │
│    │ ItemName        │   │ FK │ ItemId (Items)              │
│    │ Description     │   │ FK │ UserId (Users)              │
│    │ LocationFound   │   │    │ ClaimDescription            │
│    │ Status          │   │    │ Status (pending/verified...) │
│    │ DateFound       │   │    │ DateClaimed                 │
│ FK │ FoundByUserId   ├──◄┼ FK │ VerifiedBy (Users)          │
│ FK │ CreatedBy       │   │    │ DateVerified                │
│    │ CreatedAt       │   └──────┬────────────────────────────┘
└──────────┬───────────┘          │
           │                      │
           │        ┌─────────────┘
           │        │
           │        ▼
           │   ┌──────────────────────────────┐
           │   │       Releases               │
           │   ├──────────────────────────────┤
           │   │ PK │ ReleaseId               │
           │   │ FK │ ItemId (Items)          │
           │   │ FK │ ClaimId (Claims)        │
           │   │ FK │ ReleasedTo (Users)      │
           │   │ FK │ ReleasedBy (Users)      │
           │   │    │ ReleaseDate             │
           │   │    │ Proof (varchar)         │
           │   └──────────────────────────────┘
           │
           │
           ▼
┌──────────────────────────────┐
│      AdminLogs               │
├──────────────────────────────┤
│ PK │ LogId                   │
│ FK │ AdminId (Users)         │
│    │ Action (varchar)        │
│ FK │ ItemId (Items)          │
│ FK │ ClaimId (Claims)        │
│    │ LogDate (datetime2)     │
│    │ Remarks (varchar)       │
└──────────────────────────────┘

```

## Table Descriptions

### Users Table
- **Purpose**: Stores user accounts (both regular users and admins)
- **Key Fields**:
  - `UserId`: Primary key, auto-increment
  - `Email`: Unique identifier for login
  - `Role`: 'user' or 'admin'
  - `CreatedAt`: Account creation timestamp

### Items Table
- **Purpose**: Stores found items/lost and found reports
- **Key Fields**:
  - `ItemId`: Primary key
  - `Status`: unclaimed → pending → claimed → released
  - `FoundByUserId`: FK to Users (null if unregistered finder)
  - `FoundByName/Contact`: Fallback for unregistered finders
  - `CreatedBy`: Admin who created the record
  - `LocationFound`: Where the item was found
  - `DateFound`: Date when item was found

### Claims Table
- **Purpose**: Stores claims made by users on items
- **Key Fields**:
  - `ClaimId`: Primary key
  - `ItemId`: FK to Items
  - `UserId`: FK to Users (claimant)
  - `Status`: pending → verified → rejected OR released
  - `VerifiedBy`: FK to Users (admin who verified)
  - `DateVerified`: When claim was verified

### Releases Table
- **Purpose**: Records final handover of claimed items
- **Key Fields**:
  - `ReleaseId`: Primary key
  - `ItemId`: FK to Items
  - `ClaimId`: FK to Claims (one-to-one)
  - `ReleasedTo`: FK to Users (claimant receiving item)
  - `ReleasedBy`: FK to Users (admin releasing item)
  - `Proof`: URL to proof document (pickup confirmation, photo, etc.)

### AdminLogs Table
- **Purpose**: Audit trail for admin actions
- **Key Fields**:
  - `LogId`: Primary key
  - `AdminId`: FK to Users
  - `Action`: verify_claim, release_item, add_item, update_item
  - `ItemId/ClaimId`: References to affected entities
  - `LogDate`: When action was performed

## Relationships

1. **Users → Items** (One-to-Many)
   - A user can find multiple items (FoundByUserId)
   - A user can create multiple items records (CreatedBy)

2. **Items → Claims** (One-to-Many)
   - One item can have multiple claims

3. **Users → Claims** (One-to-Many)
   - A user can make multiple claims

4. **Claims → Releases** (One-to-One)
   - Each verified claim can result in one release

5. **Users → Releases** (One-to-Many)
   - A user can receive multiple released items
   - A user can administer multiple releases

6. **Users → AdminLogs** (One-to-Many)
   - Admin can perform multiple logged actions

## Workflow / Business Logic

### Item Lifecycle
```
1. Item Found
   - Status: unclaimed
   - Reported by finder or admin

2. Claim Made
   - Status: pending
   - User claims ownership

3. Claim Verified
   - Claim Status: verified
   - Item Status: claimed
   - Admin confirms claimant is legitimate owner

4. Item Released
   - Claim Status: released
   - Item Status: released
   - Item handed to claimant with proof
```

### Transaction Points
- **Verify Claim**: Updates both Claim and Item status atomically
- **Release Item**: Creates Release record and updates Claim/Item status atomically

## Indexes
All tables have strategic indexes on:
- Foreign keys
- Status columns (frequent filtering)
- Date columns (sorting/range queries)
- Unique/lookup columns (Email)

## Views (for reporting)
- `vw_OutstandingItems`: Items awaiting claims/resolution
- `vw_UserClaimsSummary`: User claim statistics
- `vw_ItemsByFinder`: Finder contribution statistics
