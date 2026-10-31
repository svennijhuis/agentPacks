# Squad plan: health and version endpoints

## Request
/squad build both PBIs: PBI-1 add a /health endpoint with a test; PBI-2 add a /version endpoint that reads the build tag, with a test and a README line.

## Tasks

### 1. Health endpoint
- Goal: GET /health returns 200 with body ok (src/health.ts).
- Done-check: `npm test -- health`
- Status: done

### 2.1. Build tag source
- Goal: the build writes its tag to build-tag.txt.
- Done-check: `npm run build && test -s build-tag.txt`
- Status: open

### 2.2. Version route
- Goal: GET /version returns the tag from build-tag.txt.
- Done-check: `npm test -- version`
- Status: open

### 3. README line
- Goal: README names /health and /version.
- Done-check: `grep -q /version README.md`
- Status: open
