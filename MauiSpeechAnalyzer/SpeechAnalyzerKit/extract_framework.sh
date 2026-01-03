#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage: ./extract_framework.sh [-d | -r]
  -d  Extract Debug xcframework only
  -r  Extract Release xcframework only
(No option extracts both)

Environment overrides:
  FRAMEWORK_NAME=SpeechAnalyzerKit
  SOURCE_DIR=../../../SpeechAnalyzerKit/framework
  NATIVE_DIR=Native
EOF
}

main() {
  local opts
  opts=$(getopt drh "$@" 2>/dev/null || true)
  eval set -- "$opts"

  local extract_debug=false
  local extract_release=false

  while true; do
    case "$1" in
      -d) extract_debug=true; shift ;;
      -r) extract_release=true; shift ;;
      -h) usage; exit 0 ;;
      --) shift; break ;;
      *) break ;;
    esac
  done

  if ! $extract_debug && ! $extract_release; then
    extract_debug=true
    extract_release=true
  fi

  local script_dir
  script_dir=$(cd "$(dirname "$0")" && pwd)

  local framework_name="${FRAMEWORK_NAME:-SpeechAnalyzerKit}"
  # MauiWhisperApp/SpeechAnalyzerKit から ../../SpeechAnalyzerKit/framework へ
  local source_dir="${SOURCE_DIR:-$(cd "$script_dir/../.." && pwd)/SpeechAnalyzerKit/framework}"
  local native_dir="${NATIVE_DIR:-$script_dir/Native}"

  echo "Framework: $framework_name"
  echo "Source: $source_dir"
  echo "Native: $native_dir"

  mkdir -p "$native_dir"

  if $extract_debug; then
    extract_config "Debug" "$framework_name" "$source_dir" "$native_dir"
  fi

  if $extract_release; then
    extract_config "Release" "$framework_name" "$source_dir" "$native_dir"
  fi

  echo "Done."
}

extract_config() {
  local config="$1"
  local framework_name="$2"
  local source_dir="$3"
  local native_dir="$4"

  local zip_file="$source_dir/${framework_name}-${config}.xcframework.zip"
  local target_xcframework="$native_dir/${framework_name}-${config}.xcframework"

  if [ ! -f "$zip_file" ]; then
    echo "Error: Zip file not found: $zip_file"
    exit 1
  fi

  echo "[$config] Extracting $zip_file..."

  # Remove existing xcframework if present
  if [ -d "$target_xcframework" ]; then
    echo "[$config] Removing existing: $target_xcframework"
    rm -rf "$target_xcframework"
  fi

  # Extract zip
  unzip -q "$zip_file" -d "$native_dir"

  if [ ! -d "$target_xcframework" ]; then
    echo "Error: Expected xcframework not found after extraction: $target_xcframework"
    exit 1
  fi

  echo "[$config] Extracted to: $target_xcframework"
}

main "$@"
