#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -lt 2 ] || [ "$#" -gt 4 ]; then
  printf 'Usage: %s <package-feed-directory> <package-version> [target-framework] [sdk-version]\n' "$0" >&2
  exit 2
fi

feed_dir="$1"
package_version="$2"
target_framework="${3:-net10.0}"
sdk_version="${4:-}"

if [ -z "$package_version" ]; then
  printf 'Package version must not be empty.\n' >&2
  exit 2
fi

if [ ! -d "$feed_dir" ]; then
  printf 'Package feed directory does not exist: %s\n' "$feed_dir" >&2
  exit 1
fi

feed_dir="$(cd "$feed_dir" && pwd)"
package_path="$feed_dir/MergePatchDto.$package_version.nupkg"

if [ ! -f "$package_path" ]; then
  printf 'Expected package was not found: %s\n' "$package_path" >&2
  exit 1
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
consumer_src="$script_dir/Consumer"
work_dir="$(mktemp -d)"

cleanup() {
  rm -rf "$work_dir"
}
trap cleanup EXIT

cp -R "$consumer_src/." "$work_dir/"

if [ -n "$sdk_version" ]; then
  cat > "$work_dir/global.json" <<EOF
{
  "sdk": {
    "version": "$sdk_version",
    "rollForward": "latestFeature"
  }
}
EOF
fi

cat > "$work_dir/NuGet.Config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="package-smoke" value="$feed_dir" />
  </packageSources>
</configuration>
EOF

export NUGET_PACKAGES="$work_dir/.nuget/packages"

cd "$work_dir"

project="PackageSmoke.Consumer.csproj"

dotnet restore "$project" \
  --configfile NuGet.Config \
  -p:MergePatchDtoPackageVersion="$package_version" \
  -p:PackageSmokeTargetFramework="$target_framework"

dotnet build "$project" \
  --configuration Release \
  --no-restore \
  -p:MergePatchDtoPackageVersion="$package_version" \
  -p:PackageSmokeTargetFramework="$target_framework"

dotnet "bin/Release/$target_framework/PackageSmoke.Consumer.dll"
