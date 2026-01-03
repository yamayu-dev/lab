#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage: ./build.sh [-d | -r]
  -d  Build Debug only
  -r  Build Release only
(No option builds both)
Environment overrides:
  SCHEME=SpeechAnalyzerKit        # Xcode scheme name
  PROJECT=SpeechAnalyzerKit.xcodeproj
  FRAMEWORK_NAME=SpeechAnalyzerKit
  FRAMEWORK_OUT=framework   # Output dir for packaged xcframework zip
EOF
}

main() {
  local opts
  opts=$(getopt drh "$@" || true)
  eval set -- "$opts"

  local build_debug=false
  local build_release=false

  while true; do
    case "$1" in
      -d) build_debug=true; shift ;;
      -r) build_release=true; shift ;;
      -h) usage; exit 0 ;;
      --) shift; break ;;
      *) usage; exit 1 ;;
    esac
  done

  if ! $build_debug && ! $build_release; then
    build_debug=true
    build_release=true
  fi

  local repo_root
  repo_root=$(cd "$(dirname "$0")" && pwd)

  local build_dir="$repo_root/build"
  local project="${PROJECT:-$repo_root/SpeechAnalyzerKit.xcodeproj}"
  local scheme="${SCHEME:-SpeechAnalyzerKit}"
  local framework_name="${FRAMEWORK_NAME:-SpeechAnalyzerKit}"
  local framework_out="${FRAMEWORK_OUT:-$repo_root/framework}"

  echo "Cleaning build directory: $build_dir"
  rm -rf "$build_dir"
  mkdir -p "$build_dir"
  mkdir -p "$framework_out"

  $build_debug && build_config "Debug" "$build_dir" "$project" "$scheme" "$framework_name" "$framework_out"
  $build_release && build_config "Release" "$build_dir" "$project" "$scheme" "$framework_name" "$framework_out"

  echo "Done."
}

build_config() {
  local config="$1"
  local build_dir="$2"
  local project="$3"
  local scheme="$4"
  local framework_name="$5"
  local framework_out="$6"

  local config_dir
  config_dir=$(echo "$config" | tr 'A-Z' 'a-z')
  local out_dir="$build_dir/$config_dir"
  local archive_ios="$out_dir/${framework_name}-iOS.xcarchive"
  local archive_sim="$out_dir/${framework_name}-Sim.xcarchive"
  local xcframework_path="$out_dir/${framework_name}.xcframework"

  mkdir -p "$out_dir"

  echo "[${config}] Archiving for iOS device..."
  xcodebuild \
    -project "$project" \
    -scheme "$scheme" \
    -configuration "$config" \
    -destination 'generic/platform=iOS' \
    -archivePath "$archive_ios" \
    SKIP_INSTALL=NO \
    BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
    DEBUG_INFORMATION_FORMAT="dwarf-with-dsym" \
    DWARF_DSYM_FILE_SHOULD_ACCOMPANY_PRODUCT=YES \
    GCC_GENERATE_DEBUGGING_SYMBOLS=YES \
    SWIFT_OPTIMIZATION_LEVEL="-Onone" \
    STRIP_INSTALLED_PRODUCT=NO \
    STRIP_SWIFT_SYMBOLS=NO \
    DEAD_CODE_STRIPPING=NO \
    clean archive

  echo "[${config}] Archiving for iOS Simulator..."
  xcodebuild \
    -project "$project" \
    -scheme "$scheme" \
    -configuration "$config" \
    -destination 'generic/platform=iOS Simulator' \
    -archivePath "$archive_sim" \
    SKIP_INSTALL=NO \
    BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
    DEBUG_INFORMATION_FORMAT="dwarf-with-dsym" \
    DWARF_DSYM_FILE_SHOULD_ACCOMPANY_PRODUCT=YES \
    GCC_GENERATE_DEBUGGING_SYMBOLS=YES \
    SWIFT_OPTIMIZATION_LEVEL="-Onone" \
    STRIP_INSTALLED_PRODUCT=NO \
    STRIP_SWIFT_SYMBOLS=NO \
    DEAD_CODE_STRIPPING=NO \
    clean archive

  echo "[${config}] Creating xcframework..."
  xcodebuild -create-xcframework \
    -framework "$archive_ios/Products/Library/Frameworks/${framework_name}.framework" \
    -framework "$archive_sim/Products/Library/Frameworks/${framework_name}.framework" \
    -output "$xcframework_path"

  echo "[${config}] Packaging xcframework zip..."
  local zip_name="${framework_name}-${config}.xcframework.zip"
  (cd "$out_dir" && rm -f "$zip_name" && zip -rq "$zip_name" "${framework_name}.xcframework")
  cp "$out_dir/$zip_name" "$framework_out/"

  echo "[${config}] Output: $out_dir"
}

main "$@"
