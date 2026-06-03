
$path = 'f:\Projeler\OpenSourceHub\src\OpenSourceHub.Localization\LocalizationManager.cs'
$lines = [System.IO.File]::ReadAllLines($path, [System.Text.Encoding]::UTF8)

# Remove trailing empty lines and extra closing braces
$end = $lines.Length - 1
while ($end -ge 0 -and ($lines[$end].Trim() -eq '' -or $lines[$end].Trim() -eq '}')) {
    $end--
}

# Now $lines[0..$end] is the content without trailing braces
# Add back EXACTLY two closing braces: one for the last method + one for the class
$result = $lines[0..$end]
$result += '}'   # closes LocalizationManager class
Write-Host "Writing $($result.Length) lines"
[System.IO.File]::WriteAllLines($path, $result, [System.Text.Encoding]::UTF8)
Write-Host "Done"
