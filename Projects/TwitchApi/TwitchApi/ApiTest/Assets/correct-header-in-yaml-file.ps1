function correct_header
{
    param([string]$filepath)
    $toFind = "client_id"
    $replacement = "Client-Id"
    (Get-Content $filepath).Replace($toFind, $replacement) | Set-Content "%CD%\Assets\twitch-api.yaml"
}

correct_header $args[0]