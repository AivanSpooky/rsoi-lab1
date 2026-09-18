#!/usr/bin/env bash
set -euo pipefail

: "${RENDER_API_KEY:?RENDER_API_KEY is not set}"
: "${RENDER_SERVICE_ID:?RENDER_SERVICE_ID is not set}"
: "${IMAGE_URL:?IMAGE_URL is not set}"

api_base="https://api.render.com/v1"

render_api() {
  curl --silent --show-error --fail-with-body \
    --header "Authorization: Bearer ${RENDER_API_KEY}" \
    --header "Accept: application/json" \
    "$@"
}

deploy_id="$(
  render_api --request POST "${api_base}/services/${RENDER_SERVICE_ID}/deploys" \
    --header "Content-Type: application/json" \
    --data "$(jq --null-input --arg image "${IMAGE_URL}" '{imageUrl: $image}')" |
    jq --raw-output '.id'
)"

echo "Triggered Render deploy ${deploy_id} for image ${IMAGE_URL}"

for attempt in $(seq 1 80); do
  status="$(
    render_api "${api_base}/services/${RENDER_SERVICE_ID}/deploys/${deploy_id}" |
      jq --raw-output '.status'
  )"
  echo "attempt ${attempt}: deploy status is ${status}"

  case "${status}" in
    live)
      echo "Deploy ${deploy_id} is live"
      exit 0
      ;;
    build_failed | update_failed | pre_deploy_failed | canceled | deactivated)
      echo "::error::Render deploy ${deploy_id} ended with status ${status}"
      exit 1
      ;;
  esac

  sleep 15
done

echo "::error::Timed out waiting for Render deploy ${deploy_id}"
exit 1
