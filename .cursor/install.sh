#!/usr/bin/env bash
# Cloud Agent bootstrap for the agentPacks .NET tooling.
# Installs the SDK pinned by global.json (if missing), puts `dotnet` on PATH for
# every future shell, and restores NuGet packages in locked mode. Safe to re-run.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

DOTNET_INSTALL_DIR="$HOME/.dotnet"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
export PATH="$DOTNET_INSTALL_DIR:$PATH"

# Install the SDK band pinned by global.json only when a matching one is absent.
if ! dotnet --list-sdks 2>/dev/null | grep -qE '^10\.'; then
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
  /tmp/dotnet-install.sh --jsonfile global.json --install-dir "$DOTNET_INSTALL_DIR"
fi

# Expose the muxer on the system PATH without mutating shell profiles. The dotnet
# host resolves its root from the real binary, so a symlink here is sufficient.
sudo ln -sfn "$DOTNET_INSTALL_DIR/dotnet" /usr/local/bin/dotnet

dotnet --info | head -n 5
dotnet restore tools/AgentPacks.slnx --locked-mode
