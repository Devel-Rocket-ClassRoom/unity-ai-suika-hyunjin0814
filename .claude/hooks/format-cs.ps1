$d = [Console]::In.ReadToEnd() | ConvertFrom-Json
$f = $d.tool_input.file_path
if ($f -match '\.cs$') {
    csharpier format $f
}
