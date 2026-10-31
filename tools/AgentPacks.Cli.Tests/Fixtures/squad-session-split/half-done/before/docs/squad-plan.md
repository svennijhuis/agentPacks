# Squad plan: health and version endpoints

## Request
/squad build both PBIs: PBI-1 add a /health endpoint with a test; PBI-2 add a /version endpoint that reads the build tag, with a test and a README line.

## Tasks

### 1. Health endpoint
- Goal: GET /health returns 200 with body ok (src/health.ts).
- Done-check: `npm test -- health`
- Status: done

### 2. Version endpoint
- Goal: GET /version returns the build tag.
- Done-check: `npm test -- version`
- Status: started

### 3. README line
- Goal: README names /health and /version.
- Done-check: `grep -q /version README.md`
- Status: open
