#!/usr/bin/env bash
set -euo pipefail

: "${RENDER_API_KEY:?RENDER_API_KEY is not set}"
: "${RENDER_SERVICE_ID:?RENDER_SERVICE_ID is not set}"
: "${IMAGE_URL:?IMAGE_URL is not set}"

RENDER_SERVICE_ID="$(printf '%s' "${RENDER_SERVICE_ID}" | tr -d '[:space:]')"
IMAGE_URL="$(printf '%s' "${IMAGE_URL}" | tr -d '[:space:]')"

api_base="https://api.render.com/v1"

render_api() {
  local method="$1"
  local path="$2"
  shift 2
  local tmp
  tmp="$(mktemp)"
  local code
  code="$(
    curl --silent --show-error --output "${tmp}" --write-out '%{http_code}' \
      --header "Authorization: Bearer ${RENDER_API_KEY}" \
      --header "Accept: application/json" \
      --request "${method}" \
      "${api_base}${path}" \
      "$@" || true
  )"
  if [[ "${code}" -lt 200 || "${code}" -ge 300 ]]; then
    echo "::error::Render API ${method} ${path} returned HTTP ${code}"
    cat "${tmp}"
    echo
    rm -f "${tmp}"
    exit 1
  fi
  cat "${tmp}"
  rm -f "${tmp}"
}

image_without_tag() {
  printf '%s' "$1" | sed -E 's/:[^:/]+$//'
}

service="$(render_api GET "/services/${RENDER_SERVICE_ID}")"
current_image="$(jq --raw-output '.imagePath // empty' <<< "${service}")"
echo "Render service $(jq --raw-output '.name' <<< "${service}") currently uses image ${current_image:-<none>}"

if [[ -z "${current_image}" ]]; then
  echo "::error::Service ${RENDER_SERVICE_ID} is not image-backed. Create a Web Service from an existing image, not from a Git repo."
  exit 1
fi

if [[ "$(image_without_tag "${current_image}")" != "$(image_without_tag "${IMAGE_URL}")" ]]; then
  echo "Updating service image to ${IMAGE_URL}"
  owner_id="$(jq --raw-output '.ownerId' <<< "${service}")"
  render_api PATCH "/services/${RENDER_SERVICE_ID}" \
    --header "Content-Type: application/json" \
    --data "$(jq --null-input --arg path "${IMAGE_URL}" --arg owner "${owner_id}" \
      '{image: {imagePath: $path, ownerId: $owner}}')" >/dev/null
fi

deploy="$(
  render_api POST "/services/${RENDER_SERVICE_ID}/deploys" \
    --header "Content-Type: application/json" \
    --data "$(jq --null-input --arg image "${IMAGE_URL}" '{imageUrl: $image}')"
)"
deploy_id="$(jq --raw-output '.id' <<< "${deploy}")"
echo "Triggered Render deploy ${deploy_id} for image ${IMAGE_URL}"

for attempt in $(seq 1 80); do
  status="$(
    render_api GET "/services/${RENDER_SERVICE_ID}/deploys/${deploy_id}" |
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
