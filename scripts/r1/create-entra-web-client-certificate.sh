#!/usr/bin/env bash
# Generates a self-signed dev client certificate for the Letoryn web Entra app registration.
# Outputs .cer (upload to Entra) and .pfx (Base64 into AppHost user secrets). Never commit these files.

set -euo pipefail

CERT_DIR="${CERT_DIR:-${HOME}/.local/share/letoryn-m365/certs}"
CERT_NAME="${CERT_NAME:-entra-web-dev}"
DAYS_VALID="${DAYS_VALID:-825}"
PFX_PASSWORD="${PFX_PASSWORD:-}"

usage() {
  cat <<'EOF'
Usage: create-entra-web-client-certificate.sh [--password <pfx-password>]

Creates:
  <cert-dir>/entra-web-dev.cer : upload to Entra (Certificates & secrets)
  <cert-dir>/entra-web-dev.pfx : encode for Parameters:EntraWebClientCertificatePfx

Environment:
  CERT_DIR       Output directory (default: ~/.local/share/letoryn-m365/certs)
  CERT_NAME      Base file name without extension (default: entra-web-dev)
  DAYS_VALID     Certificate validity in days (default: 825)
  PFX_PASSWORD   PFX password; if unset, a random password is generated and printed once

EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -h|--help) usage; exit 0 ;;
    --password)
      shift
      PFX_PASSWORD="${1:-}"
      shift
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

if ! command -v openssl >/dev/null 2>&1; then
  echo "openssl is required." >&2
  exit 1
fi

mkdir -p "$CERT_DIR"
readonly CER_PATH="${CERT_DIR}/${CERT_NAME}.cer"
readonly PFX_PATH="${CERT_DIR}/${CERT_NAME}.pfx"
readonly KEY_PATH="${CERT_DIR}/${CERT_NAME}.key"
readonly CSR_PATH="${CERT_DIR}/${CERT_NAME}.csr"

if [[ -z "$PFX_PASSWORD" ]]; then
  PFX_PASSWORD="$(openssl rand -base64 24)"
  echo "Generated PFX password (save for Parameters:EntraWebClientCertificatePassword):"
  echo "$PFX_PASSWORD"
  echo
fi

openssl req -new -newkey rsa:2048 -nodes -keyout "$KEY_PATH" -out "$CSR_PATH" \
  -subj "/CN=Letoryn Web (Dev)/O=Letoryn/C=GB" \
  -addext "subjectAltName=DNS:localhost" 2>/dev/null \
  || openssl req -new -newkey rsa:2048 -nodes -keyout "$KEY_PATH" -out "$CSR_PATH" \
  -subj "/CN=Letoryn Web (Dev)/O=Letoryn/C=GB"

openssl x509 -req -in "$CSR_PATH" -signkey "$KEY_PATH" -out "$CER_PATH" -days "$DAYS_VALID" -sha256

openssl pkcs12 -export -out "$PFX_PATH" -inkey "$KEY_PATH" -in "$CER_PATH" -passout "pass:${PFX_PASSWORD}"

rm -f "$KEY_PATH" "$CSR_PATH"

THUMBPRINT="$(openssl x509 -in "$CER_PATH" -noout -fingerprint -sha1 | sed 's/sha1 Fingerprint=//;s/://g')"

cat <<EOF
Certificate files written:
  CER (upload to Entra): $CER_PATH
  PFX (keep private):    $PFX_PATH
  SHA1 thumbprint:       $THUMBPRINT

Next steps:
  1. Entra admin center → web app → Certificates & secrets → Upload certificate → $CER_PATH
  2. From src/Letoryn.AppHost:
     dotnet user-secrets set "Parameters:EntraWebClientCertificatePfx" "\$(base64 -w0 $PFX_PATH)"
     dotnet user-secrets set "Parameters:EntraWebClientCertificatePassword" "<pfx-password>"
  3. Remove legacy Parameters:EntraWebClientSecret if present.

See docs/operations-rebuild-runbook.md §2.4 and §3.
EOF
