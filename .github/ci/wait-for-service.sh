#!/usr/bin/env bash
set -euo pipefail

: "${BASE_URL:?BASE_URL is not set}"

attempts="${ATTEMPTS:-60}"
delay="${DELAY:-5}"
health_url="${BASE_URL%/}/manage/health"

for attempt in $(seq 1 "${attempts}"); do
  code="$(curl --silent --output /dev/null --write-out '%{http_code}' --max-time 30 "${health_url}" || true)"
  if [[ "${code}" == "200" ]]; then
    echo "${health_url} is healthy after ${attempt} attempt(s)"
    exit 0
  fi
  echo "attempt ${attempt}/${attempts}: ${health_url} returned ${code}"
  sleep "${delay}"
done

echo "::error::${health_url} did not become healthy in time"
exit 1
