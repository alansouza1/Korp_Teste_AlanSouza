#!/usr/bin/env bash

set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CURRENT_STAGE="initialization"

print_section() {
  printf '\n========== %s ==========\n' "$1"
}

print_pass() {
  printf '[PASS] %s\n' "$1"
}

print_fail() {
  printf '[FAIL] %s\n' "$1" >&2
}

on_error() {
  local exit_code=$?
  print_fail "${CURRENT_STAGE} failed with exit code ${exit_code}."
  exit "${exit_code}"
}

trap on_error ERR

detect_chrome_bin() {
  if [[ -n "${CHROME_BIN:-}" ]]; then
    if command -v "${CHROME_BIN}" >/dev/null 2>&1; then
      printf '%s\n' "${CHROME_BIN}"
      return 0
    fi

    print_fail "CHROME_BIN is set to '${CHROME_BIN}', but this executable was not found."
    return 1
  fi

  local candidates=(
    chromium-browser
    chromium
    google-chrome
    google-chrome-stable
    chrome
  )

  local candidate
  for candidate in "${candidates[@]}"; do
    if command -v "${candidate}" >/dev/null 2>&1; then
      printf '%s\n' "${candidate}"
      return 0
    fi
  done

  print_fail "Chrome/Chromium was not found. Set CHROME_BIN to a valid browser executable to run frontend tests."
  return 1
}

run_estoque_tests() {
  CURRENT_STAGE="estoque-service tests"
  print_section "[1/3] Running estoque-service tests..."

  (
    cd "${ROOT_DIR}" &&
    dotnet test estoque-service/tests/EstoqueService.IntegrationTests/EstoqueService.IntegrationTests.csproj -v minimal
  )

  print_pass "estoque-service tests passed."
}

run_faturamento_tests() {
  CURRENT_STAGE="faturamento-service tests"
  print_section "[2/3] Running faturamento-service tests..."

  (
    cd "${ROOT_DIR}" &&
    dotnet test faturamento-service/tests/FaturamentoService.IntegrationTests/FaturamentoService.IntegrationTests.csproj -v minimal
  )

  print_pass "faturamento-service tests passed."
}

run_frontend_tests() {
  CURRENT_STAGE="frontend-angular tests"
  print_section "[3/3] Running frontend-angular tests..."

  local chrome_bin
  chrome_bin="$(detect_chrome_bin)"
  printf 'Using browser executable: %s\n' "${chrome_bin}"

  (
    cd "${ROOT_DIR}" &&
    CHROME_BIN="${chrome_bin}" npm --prefix frontend-angular run test
  )

  print_pass "frontend-angular tests passed."
}

run_estoque_tests
run_faturamento_tests
run_frontend_tests

CURRENT_STAGE="all test stages"
print_section "Unified test run completed"
print_pass "All test stages completed successfully."
