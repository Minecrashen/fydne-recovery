$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$socketDir = Join-Path $root "backend\fydne-socket"

if (!(Get-Command node -ErrorAction SilentlyContinue)) {
    throw "Node.js is required to run backend\fydne-socket"
}

Push-Location $socketDir
try {
    node server.js
}
finally {
    Pop-Location
}
