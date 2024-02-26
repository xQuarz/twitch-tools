function correct_header
{
    param([string]$filepath)
    $toFind = "client_id"
    $replacement = "Client-Id"
    (Get-Content $filepath).Replace($toFind, $replacement) | Set-Content $filepath
}


function add_nullable_to_get_clips_response_vod_offset
{
    param([string]$filepath)
    $regex = '(?<=vod_offset:\s+?type:\s)number'
    $replacement = "`r`n                            - number`r`n                            - 'null'"
    $content = Get-Content $filepath -Raw -Encoding utf8bom
    $changedType = $content -Replace $regex, $replacement | Set-Content $filepath
    $changedType -Replace "(?s)\r\n*$" 
}

$filepath = $args[0]
if (!$args[0])
{
    $filepath = "twitch-api.yaml"
}

correct_header $filepath
add_nullable_to_get_clips_response_vod_offset $filepath
