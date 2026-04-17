#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

log_step() {
  printf '\n==> %s\n' "$1"
}

find_chrome_bin() {
  if [[ -n "${CHROME_BIN:-}" ]] && command -v "${CHROME_BIN}" >/dev/null 2>&1; then
    printf '%s\n' "${CHROME_BIN}"
    return 0
  fi

  local candidates=(
    google-chrome
    google-chrome-stable
    chromium
    chromium-browser
    chrome
  )

  local candidate
  for candidate in "${candidates[@]}"; do
    if command -v "${candidate}" >/dev/null 2>&1; then
      printf '%s\n' "${candidate}"
      return 0
    fi
  done

  return 1
}

run_backend_tests() {
  log_step "Running estoque-service tests"
  (
    cd "${ROOT_DIR}" &&
    dotnet test estoque-service/tests/EstoqueService.IntegrationTests/EstoqueService.IntegrationTests.csproj -v minimal
  )

  log_step "Running faturamento-service tests"
  (
    cd "${ROOT_DIR}" &&
    dotnet test faturamento-service/tests/FaturamentoService.IntegrationTests/FaturamentoService.IntegrationTests.csproj -v minimal
  )
}

run_frontend_tests() {
  log_step "Running frontend-angular tests"

  if chrome_bin="$(find_chrome_bin)"; then
    (
      cd "${ROOT_DIR}/frontend-angular" &&
      CHROME_BIN="${chrome_bin}" npm run test
    )
    return 0
  fi

  printf 'Skipping frontend-angular tests: Chrome/Chromium was not found. Set CHROME_BIN to enable Karma tests.\n'
}

run_backend_tests
run_frontend_tests

log_step "All available test steps completed successfully"
