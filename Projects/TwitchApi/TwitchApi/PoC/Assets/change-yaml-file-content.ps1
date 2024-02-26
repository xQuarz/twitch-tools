function ReplaceStringInYamlFile
{
    param([string]$filepath, [string]$toFind, [string]$toReplace)
    $content = Get-Content $filepath -Raw -Encoding utf8bom
    $content -Replace $toFind, $toReplace | Set-Content $filepath
}

$filepath = $args[0]
if (!$args[0])
{
    $filepath = "twitch-api.yaml"
}

# Update openapi version from 3.0.0 to 3.1.0
ReplaceStringInYamlFile $filepath '(?<=openapi:\s)3.0.0' '3.1.0'

# Replace client_id with Client-Id
ReplaceStringInYamlFile $filepath "client_id" "Client-Id"

# Add missing nullable to type from vod offset
ReplaceStringInYamlFile $filepath '(?<=vod_offset:\s+?type:\s)number' "`r`n                            - number`r`n                            - 'null'"


# Remove whitespace at end of file
ReplaceStringInYamlFile $filepath '\s+?$' ''