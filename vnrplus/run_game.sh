export CX_BOTTLE=$1
gamepath=$2
gamename=$3
cxpipe=$4
user="$(whoami)"

export 'COMMAND_MODE'='unix2003'
export '__CFBundleIdentifier'='com.codeweavers.CrossOver'
export 'TMPDIR'='/var/folders/rr/9z6j7fc16q9grp79wl03sts80000gn/T/'
export 'XPC_FLAGS'='0x0'
export 'CX_BOTTLE_PATH'="/Users/${user}/Library/Application Support/CrossOver/Bottles"
export 'SSH_AUTH_SOCK'='/private/tmp/com.apple.launchd.n7X32PRXFv/Listeners'
export 'PYTHONPATH'='/Applications/CrossOver.app/Contents/SharedSupport/CrossOver/lib/python'
export 'XPC_SERVICE_NAME'='application.com.codeweavers.CrossOver.10691486.10691502'
export PATH='/Applications/CrossOver.app/Contents/SharedSupport/CrossOver/bin'":$PATH"

wine ${gamepath}
wine ${cxpipe} ${gamename}
